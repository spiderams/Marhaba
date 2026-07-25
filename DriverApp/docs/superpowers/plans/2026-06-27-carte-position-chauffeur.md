# Carte + position du chauffeur — Plan d'implémentation

> **Pour les workers agentiques :** SOUS-SKILL REQUISE : utiliser
> superpowers:subagent-driven-development (recommandé) ou
> superpowers:executing-plans pour implémenter tâche par tâche. Les étapes
> utilisent la syntaxe checkbox (`- [ ]`).

**Goal:** Remplacer `MapPlaceholder` par une carte réelle centrée sur la position
GPS du chauffeur, avec un marqueur qui suit ses déplacements et un bouton recentrer.

**Architecture:** Séparation logique / affichage. Un hook `useDriverLocation`
gère permission + suivi GPS (`expo-location`). Un composant `DriverMap` affiche
la carte (`react-native-maps`) et reçoit la position du hook. Intégration dans
`DashboardScreen`, suppression de `MapPlaceholder`.

**Tech Stack:** Expo SDK 54, React Native 0.81, TypeScript, react-native-maps
1.20.1, expo-location 19, NativeWind.

## Global Constraints

- TypeScript strict. Alias `@/*` → `src/*`. Props des composants en `Readonly<{…}>`.
- Style : `className` NativeWind par défaut ; `style={{}}` réservé aux ombres
  (`boxShadow`) et couleurs calculées au runtime. Couleurs via `@/theme/colors`
  (jamais d'hex en dur ailleurs que `palette.js`).
- Commentaires en français.
- Carte : provider **par défaut** (PAS `PROVIDER_GOOGLE`) pour rester testable
  sur Expo Go. Google Maps différé au build natif.
- **Pas de tests automatisés** (aucun framework installé, modules natifs non
  testables sans mock lourd). Vérification de chaque tâche = `npx tsc --noEmit`
  (0 erreur) + bundle/observation réelle sur le téléphone via Expo Go.

---

### Task 1 : Permission GPS dans app.json

**Files:**
- Modify: `app.json` (bloc `plugins`)

**Interfaces:**
- Consumes: rien.
- Produces: le plugin `expo-location` configuré (permission Android/iOS).

- [ ] **Step 1 : Ajouter le plugin expo-location**

Dans `app.json`, dans le tableau `expo.plugins`, ajouter cette entrée à la suite
des plugins existants (après `"expo-secure-store"`) :

```json
[
  "expo-location",
  {
    "locationAlwaysAndWhenInUsePermission": "TaxiDjibouti utilise votre position pour le suivi des courses."
  }
]
```

- [ ] **Step 2 : Vérifier que le JSON est valide**

Run: `node -e "JSON.parse(require('fs').readFileSync('app.json','utf8')); console.log('JSON OK')"`
Expected: `JSON OK`

- [ ] **Step 3 : Commit**

```bash
git add app.json
git commit -m "feat(carte): config permission de localisation (expo-location)"
```

---

### Task 2 : Hook useDriverLocation (logique GPS)

**Files:**
- Create: `src/features/dashboard/hooks/useDriverLocation.ts`

**Interfaces:**
- Consumes: `expo-location` (`requestForegroundPermissionsAsync`,
  `watchPositionAsync`, `Accuracy.High`, type `LocationSubscription`).
- Produces:
  - type `DriverCoords = { latitude: number; longitude: number }`
  - type `LocationStatus = 'loading' | 'granted' | 'denied'`
  - hook `useDriverLocation(): { location: DriverCoords | null; status: LocationStatus; error: string | null }`

- [ ] **Step 1 : Écrire le hook complet**

Créer `src/features/dashboard/hooks/useDriverLocation.ts` :

```ts
import { useEffect, useRef, useState } from 'react';
import * as Location from 'expo-location';

/** Coordonnées simples du chauffeur. */
export type DriverCoords = { latitude: number; longitude: number };

/** État de la localisation, pour piloter l'affichage de la carte. */
export type LocationStatus = 'loading' | 'granted' | 'denied';

/**
 * Logique GPS du chauffeur : demande la permission, puis suit la position en
 * continu. Ce hook ne sait RIEN de l'affichage — il expose juste l'état.
 *
 * Nettoyage : l'abonnement GPS (`watchPositionAsync`) est coupé au démontage,
 * sinon le GPS continue de tourner (batterie + fuite mémoire).
 */
export function useDriverLocation(): {
  location: DriverCoords | null;
  status: LocationStatus;
  error: string | null;
} {
  const [location, setLocation] = useState<DriverCoords | null>(null);
  const [status, setStatus] = useState<LocationStatus>('loading');
  const [error, setError] = useState<string | null>(null);

  // On garde l'abonnement dans une ref pour pouvoir le couper au démontage.
  const subscriptionRef = useRef<Location.LocationSubscription | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function start() {
      try {
        const { status: permission } =
          await Location.requestForegroundPermissionsAsync();

        if (permission !== 'granted') {
          if (!cancelled) setStatus('denied');
          return;
        }

        // Permission OK → on s'abonne aux mises à jour de position.
        const subscription = await Location.watchPositionAsync(
          {
            accuracy: Location.Accuracy.High,
            distanceInterval: 10, // mise à jour tous les ~10 m
            timeInterval: 5000, //  ou au plus tous les ~5 s
          },
          (pos) => {
            if (cancelled) return;
            setLocation({
              latitude: pos.coords.latitude,
              longitude: pos.coords.longitude,
            });
            setStatus('granted');
          },
        );
        subscriptionRef.current = subscription;
      } catch (e) {
        if (!cancelled) {
          setError('Impossible de lire la position GPS.');
          setStatus('denied');
        }
      }
    }

    start();

    // Nettoyage au démontage : on coupe l'abonnement GPS.
    return () => {
      cancelled = true;
      subscriptionRef.current?.remove();
      subscriptionRef.current = null;
    };
  }, []);

  return { location, status, error };
}
```

- [ ] **Step 2 : Vérifier la compilation**

Run: `npx tsc --noEmit`
Expected: exit 0, aucune erreur.

- [ ] **Step 3 : Commit**

```bash
git add src/features/dashboard/hooks/useDriverLocation.ts
git commit -m "feat(carte): hook useDriverLocation (permission + suivi GPS)"
```

---

### Task 3 : Composant DriverMap (affichage)

**Files:**
- Create: `src/features/dashboard/components/DriverMap.tsx`

**Interfaces:**
- Consumes: `useDriverLocation` (Task 2), `react-native-maps` (`MapView`,
  `Marker`, type `Region`), `@/theme/colors`.
- Produces:
  - composant `DriverMap` avec une prop ref impérative `recenter()` exposée via
    `forwardRef` + `useImperativeHandle`.
  - type exporté `DriverMapHandle = { recenter: () => void }`

- [ ] **Step 1 : Écrire le composant complet**

Créer `src/features/dashboard/components/DriverMap.tsx` :

```tsx
import { forwardRef, useEffect, useImperativeHandle, useRef } from 'react';
import { Linking, Pressable, Text, View } from 'react-native';
import MapView, { Marker, type Region } from 'react-native-maps';
import { MaterialIcons } from '@expo/vector-icons';
import { useDriverLocation } from '../hooks/useDriverLocation';
import { colors } from '@/theme/colors';

/** Niveau de zoom initial (deltas Region). Plus petit = plus zoomé. */
const ZOOM_DELTA = { latitudeDelta: 0.01, longitudeDelta: 0.01 } as const;

/** Méthodes impératives exposées au parent (le dashboard). */
export type DriverMapHandle = { recenter: () => void };

/**
 * Carte du chauffeur. Affiche sa position (marqueur taxi) et suit ses
 * déplacements. Ne gère PAS le GPS lui-même : il vient du hook useDriverLocation.
 *
 * Le parent peut appeler `recenter()` (via ref) pour recentrer sur le chauffeur,
 * ce qui branche le bouton flottant déjà présent dans le dashboard.
 */
export const DriverMap = forwardRef<DriverMapHandle>(function DriverMap(_props, ref) {
  const { location, status } = useDriverLocation();
  const mapRef = useRef<MapView | null>(null);
  // Mémorise si on a déjà fait le centrage initial (une seule fois).
  const didCenterRef = useRef(false);

  // Recentre la carte sur la position courante du chauffeur.
  function recenter() {
    if (!location) return;
    const region: Region = { ...location, ...ZOOM_DELTA };
    mapRef.current?.animateToRegion(region, 500);
  }

  // On expose `recenter` au parent via la ref.
  useImperativeHandle(ref, () => ({ recenter }));

  // Centrage initial : au premier fix GPS seulement.
  useEffect(() => {
    if (location && !didCenterRef.current) {
      didCenterRef.current = true;
      const region: Region = { ...location, ...ZOOM_DELTA };
      mapRef.current?.animateToRegion(region, 0);
    }
  }, [location]);

  // Permission refusée : message + accès aux réglages.
  if (status === 'denied') {
    return (
      <View className="flex-1 items-center justify-center bg-surface-container px-8">
        <MaterialIcons name="location-off" size={40} color={colors.onSurfaceVariant} />
        <Text className="mt-4 text-center text-base text-on-surface-variant">
          La localisation est désactivée. Activez-la pour voir votre position.
        </Text>
        <Pressable
          onPress={() => Linking.openSettings()}
          className="mt-4 rounded-xl bg-secondary-container px-4 py-2"
        >
          <Text className="font-bold text-on-secondary-container">Ouvrir les réglages</Text>
        </Pressable>
      </View>
    );
  }

  // En attente du premier fix GPS : fond neutre.
  if (status === 'loading' || !location) {
    return <View className="flex-1 bg-surface-container" />;
  }

  // Position disponible : la carte.
  return (
    <MapView
      ref={mapRef}
      style={{ flex: 1 }}
      initialRegion={{ ...location, ...ZOOM_DELTA }}
      showsUserLocation={false}
    >
      <Marker coordinate={location} title="Vous">
        <View className="rounded-xl bg-primary p-2">
          <MaterialIcons name="local-taxi" size={24} color={colors.white} />
        </View>
      </Marker>
    </MapView>
  );
});
```

Note : `MapView` exige `style` (pas `className`) pour `flex: 1` — c'est un cas
natif accepté par la convention du projet.

- [ ] **Step 2 : Vérifier la compilation**

Run: `npx tsc --noEmit`
Expected: exit 0, aucune erreur.

- [ ] **Step 3 : Commit**

```bash
git add src/features/dashboard/components/DriverMap.tsx
git commit -m "feat(carte): composant DriverMap (carte + marqueur + recentrage)"
```

---

### Task 4 : Intégration dashboard + suppression de MapPlaceholder

**Files:**
- Modify: `src/features/dashboard/DashboardScreen.tsx`
- Delete: `src/features/dashboard/components/MapPlaceholder.tsx`

**Interfaces:**
- Consumes: `DriverMap`, `DriverMapHandle` (Task 3).
- Produces: rien (écran final).

- [ ] **Step 1 : Remplacer MapPlaceholder par DriverMap dans le dashboard**

Dans `src/features/dashboard/DashboardScreen.tsx` :

Remplacer l'import :
```tsx
import { MapPlaceholder } from './components/MapPlaceholder';
```
par :
```tsx
import { useRef } from 'react';
import { DriverMap, type DriverMapHandle } from './components/DriverMap';
```
(fusionner avec le `useState` déjà importé depuis `react` : `import { useRef, useState } from 'react';`)

Dans le composant `DashboardScreen`, après les `useState` existants, ajouter la ref :
```tsx
const mapRef = useRef<DriverMapHandle>(null);
```

Remplacer `<MapPlaceholder />` par :
```tsx
<DriverMap ref={mapRef} />
```

Brancher le bouton flottant « ma position » (icône `my-location`) sur le recentrage.
Remplacer le `Pressable` de ce bouton :
```tsx
<Pressable style={floatingShadow} className="h-14 w-14 items-center justify-center rounded-full bg-white">
  <MaterialIcons name="my-location" size={24} color={colors.primary} />
</Pressable>
```
par (ajout du `onPress`) :
```tsx
<Pressable
  onPress={() => mapRef.current?.recenter()}
  style={floatingShadow}
  className="h-14 w-14 items-center justify-center rounded-full bg-white"
>
  <MaterialIcons name="my-location" size={24} color={colors.primary} />
</Pressable>
```

- [ ] **Step 2 : Supprimer MapPlaceholder**

Run: `git rm src/features/dashboard/components/MapPlaceholder.tsx`
Expected: fichier supprimé.

- [ ] **Step 3 : Vérifier qu'aucun import résiduel ne pointe vers MapPlaceholder**

Run: `grep -rn "MapPlaceholder" src/ || echo "aucune référence — OK"`
Expected: `aucune référence — OK`

- [ ] **Step 4 : Vérifier la compilation**

Run: `npx tsc --noEmit`
Expected: exit 0, aucune erreur.

- [ ] **Step 5 : Commit**

```bash
git add -A
git commit -m "feat(carte): intégration DriverMap au dashboard, suppression du placeholder"
```

---

### Task 5 : Vérification réelle sur le téléphone

**Files:** aucun (vérification manuelle).

- [ ] **Step 1 : Lancer Expo avec cache vidé**

Run: `npx expo start --clear`
Expected: Metro démarre, QR code affiché.

- [ ] **Step 2 : Recharger l'app sur le téléphone (Expo Go) et observer**

Checklist de validation :
- [ ] Une demande de permission de localisation apparaît au lancement.
- [ ] Après acceptation, la carte s'affiche centrée sur ma position.
- [ ] Le marqueur taxi est sur ma position.
- [ ] Je peux déplacer la carte librement.
- [ ] Le bouton flottant « ma position » (⊙) recentre la carte sur moi.
- [ ] Si je refuse la permission, le message « localisation désactivée » + bouton
      réglages s'affiche.

Note attendue : sur Android dans Expo Go sans clé Google, les tuiles de carte
peuvent être grises — c'est normal à ce stade. La position et le marqueur, eux,
doivent fonctionner.

- [ ] **Step 3 : Aucun commit** (étape d'observation).
