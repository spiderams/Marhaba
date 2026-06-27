import '@/global.css';
import { useEffect, useState } from 'react';
import { View } from 'react-native';
import { Stack, useRouter, useRootNavigationState } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import { getAccessToken } from '@/lib/auth';

/**
 * Disposition racine + garde d'authentification.
 *
 * Au démarrage de l'app, on regarde s'il existe un jeton stocké :
 * - jeton présent  → on laisse l'utilisateur sur le tableau de bord ("/").
 * - aucun jeton    → on le redirige vers l'écran de connexion ("/login").
 *
 * POINT CLÉ (sinon écran blanc) : le <Stack> doit être rendu À CHAQUE rendu.
 * Expo Router ne monte les routes que si le navigateur racine existe ; si on
 * renvoyait un <View> seul pendant la vérification, aucune route ne se monterait
 * → page blanche. On superpose donc l'écran de chargement PAR-DESSUS le Stack.
 *
 * On attend aussi que la navigation racine soit prête (`useRootNavigationState`)
 * avant d'appeler `replace`, pour ne pas "naviguer avant le montage".
 */
export default function RootLayout() {
  const router = useRouter();
  const navState = useRootNavigationState();
  const [checking, setChecking] = useState(true);

  useEffect(() => {
    // Tant que la navigation racine n'est pas montée, on ne redirige pas.
    if (!navState?.key) return;

    async function checkAuth() {
      const token = await getAccessToken();
      if (!token) {
        router.replace('/login');
      }
      setChecking(false);
    }
    checkAuth();
  }, [navState?.key, router]);

  return (
    <>
      <StatusBar style="dark" />
      {/* Le navigateur est TOUJOURS monté. */}
      <Stack screenOptions={{ headerShown: false }} />

      {/* Overlay neutre pendant la vérification du jeton (évite le flash du dashboard). */}
      {checking && <View className="absolute inset-0 bg-surface" />}
    </>
  );
}
