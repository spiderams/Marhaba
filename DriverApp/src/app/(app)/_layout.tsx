import { Stack } from 'expo-router';

/**
 * Pile de navigation de la ZONE AUTHENTIFIÉE.
 *
 * Tous les écrans accessibles une fois connecté (tableau de bord, et plus tard
 * Gains / Historique / course en cours…) vivront sous ce groupe `(app)`.
 * Les parenthèses signifient que « (app) » n'apparaît PAS dans l'URL.
 */
export default function AppLayout() {
  return <Stack screenOptions={{ headerShown: false }} />;
}
