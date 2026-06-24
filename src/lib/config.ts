/**
 * Configuration d'environnement de l'app chauffeur.
 *
 * Comme dans le projet EchoIA (où l'URL du backend vient d'une variable
 * VITE_ECHOIA_BACKEND_URL), on évite de coder l'URL en dur. Avec Expo, les
 * variables d'environnement exposées au code client commencent par EXPO_PUBLIC_.
 * On les définit dans un fichier .env à la racine du projet.
 *
 * Valeur de repli (si la variable n'est pas définie) : l'URL HTTPS locale du
 * backend Aspire.
 *
 * ⚠️ Selon la cible de test, l'URL « localhost » ne pointe pas vers le même
 * endroit :
 *   - Web (navigateur sur le PC)      → https://localhost:7129
 *   - Émulateur Android               → https://10.0.2.2:7129
 *   - Téléphone physique (même Wi-Fi) → https://<IP-LAN-du-PC>:7129
 */
export const API_BASE_URL =
  process.env.EXPO_PUBLIC_API_BASE_URL ?? 'https://localhost:7129';

/** URL du hub SignalR temps réel. */
export const RIDE_HUB_URL = `${API_BASE_URL}/hubs/ride`;
