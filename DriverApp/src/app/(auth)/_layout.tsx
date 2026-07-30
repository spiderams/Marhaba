import { Stack } from 'expo-router';

/**
 * Pile de navigation de la ZONE PUBLIQUE (non authentifiée) : connexion, et plus
 * tard mot de passe oublié, inscription… Le groupe `(auth)` n'apparaît pas dans l'URL.
 */
export default function AuthLayout() {
  return <Stack screenOptions={{ headerShown: false }} />;
}
