# TaxiDjibouti — App Chauffeur (instructions projet)

App mobile **chauffeur** de TaxiDjibouti. React Native via **Expo (SDK 56)**, TypeScript strict.
Se connecte au backend .NET (`TaxiBackEnd`) : REST + SignalR `RideHub` temps réel.

## Stack
- **Expo Router** (routing par fichiers, routes dans `src/app/`)
- **NativeWind** (Tailwind pour RN — utiliser `className`)
- **react-native-maps** (carte + GPS)
- **@microsoft/signalr** (temps réel `RideHub`)
- **expo-location** (position chauffeur → `SendDriverLocation`)
- **expo-notifications** (push FCM — feature P0.2 du backend)
- **expo-secure-store** (stockage JWT sécurisé)

## Conventions
- TypeScript strict, alias `@/*` → `src/*`
- Styles via `className` NativeWind (pas de StyleSheet sauf cas natif)
- Couche `src/lib/` = logique partagée (api, auth, realtime, types, config)
- Les types dans `src/lib/types.ts` sont le **miroir du backend** — garder synchronisés
- Commentaires en français

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
