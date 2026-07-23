# Carte + position du chauffeur — Design

> **Statut : implémenté et validé sur téléphone (2026-06-27).** Vérification réelle
> OK (Task 5) : permission demandée, carte centrée sur la position, marqueur,
> recentrage via le bouton flottant. Revue finale : prêt à merger.

> Première étape de la carte du tableau de bord chauffeur. On construit la
> **fondation** : afficher une vraie carte centrée sur la position GPS du
> chauffeur, avec un marqueur qui suit ses déplacements. Les étapes ultérieures
> (envoi de la position au backend via SignalR, tracé d'une course) viendront
> ensuite et s'appuieront sur cette base.

## Objectif

Remplacer `MapPlaceholder` (zone grise) par une carte réelle qui :
- demande la permission de localisation au chauffeur ;
- affiche sa position avec un marqueur qui suit ses déplacements en continu ;
- se centre sur lui au démarrage, tout en le laissant déplacer la carte ;
- offre un bouton « recentrer sur moi ».

## Décisions techniques (et leur pourquoi)

- **Fournisseur de carte** : provider **par défaut** (pas `PROVIDER_GOOGLE`) pour
  rester **testable dans Expo Go** (SDK 54), où Google Maps ne fonctionne pas
  (nécessite un development build). Google Maps sera activé plus tard, au premier
  build natif (changement d'une ligne + clé API dans `app.json`).
  - Risque connu : sur Android dans Expo Go, les tuiles peuvent rester grises
    sans clé Google. La position et les marqueurs s'afficheront quand même, ce
    qui suffit à valider la logique GPS (cœur de cette étape).
- **Suivi GPS** : suivi **continu** + recentrage **manuel** (standard apps taxi).
  La carte est libre ; un bouton recentre sur le chauffeur.

## Architecture

On respecte l'arbo `features/`. La carte appartient au **dashboard** :

```
src/features/dashboard/
  components/
    DriverMap.tsx        ← NOUVEAU : la carte (remplace MapPlaceholder)
  hooks/
    useDriverLocation.ts ← NOUVEAU : logique GPS (permission + suivi)
```

Séparation logique / affichage :
- `useDriverLocation` = **la logique** : permission, lecture GPS, suivi. Ignore l'UI.
- `DriverMap` = **l'affichage** : reçoit la position, dessine carte + marqueur +
  bouton recentrer. Ignore le fonctionnement du GPS.

`MapPlaceholder.tsx` est supprimé.

## Composant 1 — `useDriverLocation` (hook)

**Signature :** `useDriverLocation() → { location, status, error }`

**Déroulé :**
1. Au montage, demande la permission de localisation (`expo-location`).
2. Refusée → `status: 'denied'`.
3. Accordée → démarre `watchPositionAsync` (abonnement aux mises à jour GPS).
4. À chaque mise à jour, stocke `location: { latitude, longitude }`.
5. Au démontage, **coupe l'abonnement** (sinon : batterie + fuite mémoire).

**États (`status`) :**

| `status`    | Signification              | UI                                   |
|-------------|----------------------------|--------------------------------------|
| `'loading'` | permission/position en cours | overlay neutre (fond surface)      |
| `'granted'` | position disponible        | la carte                             |
| `'denied'`  | permission refusée         | message « activez la localisation »  |

**Réglages de suivi (valeurs de départ, ajustables) :**
- `accuracy`: `Location.Accuracy.High`
- mise à jour tous les ~10 m (`distanceInterval: 10`) ou ~5 s (`timeInterval: 5000`).

## Composant 2 — `DriverMap` (affichage)

`<MapView>` plein écran (react-native-maps) avec :
- `<Marker>` sur la position du chauffeur (icône taxi, couleur `primary`).
- Centrage initial sur le premier fix GPS (`initialRegion`).
- Carte libre (le chauffeur peut la déplacer).
- Bouton **recentrer** : via une `ref` sur le `MapView`, `animateToRegion(maPosition)`.

**Rendu selon `status` :**
- `'loading'` → fond neutre (style `surface`).
- `'denied'` → message + bouton « Ouvrir les réglages » (`Linking.openSettings`).
- `'granted'` → la carte.

## Intégration dans le dashboard

Dans `DashboardScreen.tsx` :
- `<MapPlaceholder />` → `<DriverMap />`.
- Le reste (TopBar, EarningsCard, BottomNav, boutons flottants par-dessus) **inchangé**.
- Le bouton flottant ⊙ « ma position » déjà présent (aujourd'hui inerte) est **relié**
  au recentrage de la carte.

## Configuration `app.json`

- Plugin `expo-location` avec le message de permission :
  « TaxiDjibouti utilise votre position pour le suivi des courses. »
- **Pas** de clé Google Maps à cette étape (différée au build natif).

## Hors périmètre (étapes suivantes)

- Envoi de la position au backend via SignalR (`SendDriverLocation`).
- Affichage du trajet d'une course (pickup / destination / tracé).
- Activation de Google Maps (`PROVIDER_GOOGLE` + clé) au premier development build.
