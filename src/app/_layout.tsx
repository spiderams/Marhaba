import '@/global.css';
import { useEffect, useState } from 'react';
import { View } from 'react-native';
import { Stack, router } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import { getAccessToken } from '@/lib/auth';
import { colors } from '@/theme/colors';

/**
 * Disposition racine + garde d'authentification.
 *
 * Au démarrage de l'app, on regarde s'il existe un jeton stocké :
 * - jeton présent  → on laisse l'utilisateur sur le tableau de bord ("/").
 * - aucun jeton    → on le redirige vers l'écran de connexion ("/login").
 *
 * Tant qu'on n'a pas fini de vérifier (lecture du stockage sécurisé), on affiche
 * un écran vide pour éviter un "flash" du dashboard avant la redirection.
 */
export default function RootLayout() {
  const [checking, setChecking] = useState(true);

  useEffect(() => {
    // Vérifie la présence d'un jeton une seule fois, au montage.
    async function checkAuth() {
      const token = await getAccessToken();
      if (!token) {
        router.replace('/login');
      }
      setChecking(false);
    }
    checkAuth();
  }, []);

  // Pendant la vérification : écran neutre (pas de contenu visible).
  if (checking) {
    return <View style={{ flex: 1, backgroundColor: colors.surface }} />;
  }

  return (
    <>
      <StatusBar style="dark" />
      <Stack screenOptions={{ headerShown: false }} />
    </>
  );
}
