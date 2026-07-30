import { Platform } from 'react-native';
import * as SecureStore from 'expo-secure-store';
import type { AuthResponse } from './types';

/**
 * Stockage des jetons JWT (access + refresh).
 *
 * Selon la plateforme :
 *  - mobile (iOS / Android) → expo-secure-store (Keychain / Keystore, sécurisé).
 *  - web (navigateur)       → localStorage. En effet, SecureStore n'existe PAS
 *    sur le web (pas de Keychain dans un navigateur) ; sans ce repli, l'app
 *    plante au démarrage avec "getValueWithKeyAsync is not a function".
 *
 * On regroupe le choix de l'implémentation dans un petit objet `storage` pour
 * que le reste du fichier reste simple.
 */

const ACCESS_KEY = 'td.accessToken';
const REFRESH_KEY = 'td.refreshToken';

const isWeb = Platform.OS === 'web';

const storage = {
  async getItem(key: string): Promise<string | null> {
    if (isWeb) {
      return globalThis.localStorage?.getItem(key) ?? null;
    }
    return SecureStore.getItemAsync(key);
  },
  async setItem(key: string, value: string): Promise<void> {
    if (isWeb) {
      globalThis.localStorage?.setItem(key, value);
      return;
    }
    await SecureStore.setItemAsync(key, value);
  },
  async removeItem(key: string): Promise<void> {
    if (isWeb) {
      globalThis.localStorage?.removeItem(key);
      return;
    }
    await SecureStore.deleteItemAsync(key);
  },
};

/** Persiste les jetons après login/refresh. */
export async function saveTokens(auth: AuthResponse): Promise<void> {
  await storage.setItem(ACCESS_KEY, auth.accessToken);
  await storage.setItem(REFRESH_KEY, auth.refreshToken);
}

/** Récupère le jeton d'accès courant, ou null si non connecté. */
export async function getAccessToken(): Promise<string | null> {
  return storage.getItem(ACCESS_KEY);
}

/** Récupère le refresh token courant, ou null. */
export async function getRefreshToken(): Promise<string | null> {
  return storage.getItem(REFRESH_KEY);
}

/** Efface les jetons (déconnexion). */
export async function clearTokens(): Promise<void> {
  await storage.removeItem(ACCESS_KEY);
  await storage.removeItem(REFRESH_KEY);
}
