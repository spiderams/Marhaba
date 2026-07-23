# TaxiDjibouti — App Chauffeur (instructions projet)

App mobile **chauffeur** de TaxiDjibouti. React Native via **Expo (SDK 54)**, TypeScript strict.
Se connecte au backend .NET (`TaxiBackEnd`) : REST + SignalR `RideHub` temps réel.

## Stack
- **Expo Router** (routing par fichiers ; routes dans `src/app/`, groupes `(auth)` / `(app)`)
- **NativeWind** (Tailwind pour RN — utiliser `className`)
- **react-native-maps** (carte + GPS)
- **@microsoft/signalr** (temps réel `RideHub`)
- **expo-location** (position chauffeur → `SendDriverLocation`)
- **expo-notifications** (push FCM — feature P0.2 du backend)
- **expo-secure-store** (stockage JWT sécurisé)

## Architecture des dossiers
L'arbo est organisée pour qu'elle « parle » d'elle-même :
```
src/
  app/            ROUTES uniquement (Expo Router). Fines coquilles qui réexportent
                  l'écran de la feature. NE PAS y mettre de logique métier.
    (auth)/       zone publique (non connecté) : login…   → groupe invisible dans l'URL
    (app)/        zone authentifiée : dashboard…          → groupe invisible dans l'URL
  features/       LE MÉTIER, un dossier par domaine. Chaque feature est autonome :
    auth/         XxxScreen.tsx + components/             (écran + ses composants)
    dashboard/
  lib/            SOCLE partagé : api, auth, realtime, types, config
  theme/          palette.js (source unique des couleurs) + colors.ts (réexport typé)
```
- **Ajouter une fonctionnalité** = nouveau dossier `features/xxx/` + une route fine dans `app/`.
- Un fichier de `app/` ne fait que `import { XxxScreen }` puis le rendre. Rien d'autre.

## Conventions
- TypeScript strict, alias `@/*` → `src/*`. Props des composants en `Readonly<{…}>`.
- **Style** : `className` NativeWind par défaut. `style={{}}` réservé aux cas que NativeWind
  ne couvre pas : ombres (`shadow*`/`elevation`) et couleurs **calculées au runtime**
  (ex. opacité hex dynamique dans `OnlineBadge`). Toujours commenter pourquoi.
- **Couleurs** : source unique dans `src/theme/palette.js`. Consommée par `tailwind.config.js`
  (→ classes `bg-primary`, `text-on-surface`…) ET `src/theme/colors.ts` (→ props d'icônes).
  Ne jamais écrire un code hexadécimal ailleurs que dans `palette.js`.
- Couche `src/lib/` = logique partagée (api, auth, realtime, types, config)
- Les types dans `src/lib/types.ts` sont le **miroir du backend** — garder synchronisés
- Commentaires en français
- `typedRoutes` (Expo, expérimental) est **désactivé** : son générateur scanne `features/`
  comme des routes. À réactiver quand Expo corrigera ce comportement.

## Backend
- Auth : JWT maison, login par **numéro de téléphone**
- SignalR : JWT en **query string** (`?access_token=`), groupe chauffeur `JoinMyDriverGroup`
- Événements clés : `rideOffered`, `rideOfferRevoked`, `rideStatusChanged`
- Dispatch en **vagues** : offre TTL ~15s, premier-arrivé-gagne (409 si déjà pris)

## Lancer
```bash
npm install
npx expo start          # puis 'a' pour Android, ou Expo Go sur téléphone
```
> ⚠️ Node ≥ 20.19.4 requis pour les builds (actuellement 20.18 → OK en dev, à upgrader avant prod).
> ⚠️ `API_BASE_URL` dans `src/lib/config.ts` à adapter selon la cible (émulateur 10.0.2.2 / IP LAN).
