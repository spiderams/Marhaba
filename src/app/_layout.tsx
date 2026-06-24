import '@/global.css';
import { Stack } from 'expo-router';
import { StatusBar } from 'expo-status-bar';

/**
 * Disposition racine de l'application.
 *
 * Pour l'instant on n'a qu'un seul écran (le tableau de bord), donc une simple
 * pile (Stack) suffit. On masque l'en-tête par défaut car le tableau de bord
 * a sa propre barre supérieure (TopBar).
 */
export default function RootLayout() {
  return (
    <>
      <StatusBar style="dark" />
      <Stack screenOptions={{ headerShown: false }} />
    </>
  );
}
