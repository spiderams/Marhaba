import { LoginScreen } from '@/features/auth/LoginScreen';

/**
 * Route /login — zone publique (non authentifiée).
 *
 * Coquille de route : l'écran réel vit dans `features/auth`. Expo Router exige
 * un export par défaut qui soit un composant ; on enveloppe donc l'écran de la
 * feature dans ce composant de route.
 */
export default function LoginRoute() {
  return <LoginScreen />;
}
