# TaxiDjibouti — App Chauffeur

Application mobile **chauffeur** de TaxiDjibouti (service de réservation de taxi/VTC à Djibouti).
Construite avec **Expo (React Native)** + TypeScript. Se connecte au backend .NET
(`TaxiDjiboutiV2`) via API REST et SignalR temps réel (`RideHub`).

## Stack

| Brique | Rôle |
|--------|------|
| Expo (SDK 56) + Expo Router | Framework + navigation par fichiers |
| TypeScript (strict) | Typage |
| NativeWind | Styles (Tailwind pour React Native) |
| react-native-maps | Carte + GPS |
| @microsoft/signalr | Temps réel (RideHub) |
| expo-location | Position du chauffeur |
| expo-notifications | Push (FCM) |
| expo-secure-store | Stockage sécurisé des jetons JWT |

## Démarrer

```bash
npm install
npx expo start         # puis : 'a' (Android), ou scanner le QR avec Expo Go
```

> ⚠️ Node ≥ 20.19.4 requis pour les builds (actuellement testé en dev sous 20.18).
> ⚠️ `API_BASE_URL` dans `src/lib/config.ts` à adapter selon la cible
> (émulateur Android : `10.0.2.2` · téléphone physique : IP LAN du PC).
> ⚠️ La carte (`react-native-maps`) et le GPS ne fonctionnent **pas** sur le web :
> utiliser Expo Go (téléphone) ou un émulateur Android.

## Structure

```
src/
├── theme/colors.ts            # palette du design (DESIGN.md)
├── lib/                       # couche métier partagée
│   ├── types.ts               #   types miroir du backend (RideStatus, événements SignalR…)
│   ├── config.ts              #   URLs API + hub
│   ├── auth.ts                #   stockage JWT (SecureStore)
│   ├── api.ts                 #   client REST
│   └── realtime.ts            #   client SignalR RideHub
├── components/dashboard/      # composants du tableau de bord
│   ├── OnlineBadge.tsx        #   badge EN LIGNE / HORS LIGNE
│   ├── TopBar.tsx             #   barre supérieure
│   ├── MapPlaceholder.tsx     #   fond de carte (provisoire)
│   ├── EarningsCard.tsx       #   carte « gains du jour »
│   └── BottomNav.tsx          #   navigation du bas
└── app/
    ├── _layout.tsx            # disposition racine
    └── index.tsx              # tableau de bord (assemble les composants)
```

Voir `CLAUDE.md` pour les conventions du projet.
