import * as SecureStore from 'expo-secure-store';
import type { AuthTokens } from './types';

/**
 * Stockage sécurisé des jetons JWT (access + refresh) via expo-secure-store
 * (Keychain iOS / Keystore Android). Ne jamais stocker un token en clair
 * dans AsyncStorage.
 */

const ACCESS_KEY = 'td.accessToken';
const REFRESH_KEY = 'td.refreshToken';

/** Persiste les jetons après login/refresh. */
export async function saveTokens(tokens: AuthTokens): Promise<void> {
  await SecureStore.setItemAsync(ACCESS_KEY, tokens.accessToken);
  await SecureStore.setItemAsync(REFRESH_KEY, tokens.refreshToken);
}

/** Récupère le jeton d'accès courant, ou null si non connecté. */
export async function getAccessToken(): Promise<string | null> {
  return SecureStore.getItemAsync(ACCESS_KEY);
}

/** Récupère le refresh token courant, ou null. */
export async function getRefreshToken(): Promise<string | null> {
  return SecureStore.getItemAsync(REFRESH_KEY);
}

/** Efface les jetons (déconnexion). */
export async function clearTokens(): Promise<void> {
  await SecureStore.deleteItemAsync(ACCESS_KEY);
  await SecureStore.deleteItemAsync(REFRESH_KEY);
}
