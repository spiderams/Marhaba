import { API_BASE_URL } from './config';
import { getAccessToken } from './auth';
import type { AuthResponse, DriverDto, RideDto } from './types';

/**
 * Client HTTP de l'API TaxiDjibouti.
 *
 * Inspiré du pattern du projet EchoIA (axiosManager) : une couche centrale qui
 * ajoute AUTOMATIQUEMENT l'en-tête `Authorization: Bearer <token>` à chaque
 * requête authentifiée — on n'a donc pas à le répéter à chaque appel.
 * Ici on utilise `fetch` (intégré, aucune dépendance) au lieu d'axios.
 *
 * Le backend renvoie des erreurs typées mappées en codes HTTP
 * (400/401/403/404/409/500) ; on les remonte sous forme d'ApiError.
 */

/** Erreur d'appel API portant le code HTTP et le message backend. */
export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

async function request<T>(
  method: string,
  path: string,
  body?: unknown,
  auth = true,
): Promise<T> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    // Désactive la page d'avertissement HTML que ngrok (plan gratuit) intercale
    // sinon devant l'API → garantit qu'on reçoit bien le JSON. Sans effet hors ngrok.
    'ngrok-skip-browser-warning': 'true',
  };

  // Interceptor "maison" : on attache le jeton si la requête est authentifiée.
  if (auth) {
    const token = await getAccessToken();
    if (token) headers.Authorization = `Bearer ${token}`;
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  if (!response.ok) {
    const text = await response.text().catch(() => '');
    throw new ApiError(response.status, text || response.statusText);
  }

  // 204 No Content → pas de corps à parser.
  if (response.status === 204) return undefined as T;
  return (await response.json()) as T;
}

/** API regroupée par domaine — reflète les endpoints réels du backend. */
export const api = {
  /**
   * POST /api/auth/login — connexion par téléphone + mot de passe.
   * Le numéro est envoyé tel quel (sans préfixe), comme attendu par le backend.
   */
  login: (phoneNumber: string, password: string) =>
    request<AuthResponse>('POST', '/api/auth/login', { phoneNumber, password }, false),

  /** GET /api/drivers/me — profil chauffeur courant. */
  getMyDriver: () => request<DriverDto>('GET', '/api/drivers/me'),

  /** POST /api/drivers/set-availability — bascule de disponibilité. */
  setAvailability: (isAvailable: boolean) =>
    request<DriverDto>('POST', '/api/drivers/set-availability', { isAvailable }),

  /** POST /api/rides/{id}/accept-offer — accepter l'offre de vague (premier-arrivé-gagne). */
  acceptOffer: (rideId: number) =>
    request<RideDto>('POST', `/api/rides/${rideId}/accept-offer`),

  /** POST /api/rides/{id}/decline-offer — refuser l'offre. */
  declineOffer: (rideId: number) =>
    request<RideDto>('POST', `/api/rides/${rideId}/decline-offer`),

  /** POST /api/rides/{id}/arrived — chauffeur arrivé au point de prise en charge. */
  markArrived: (rideId: number) => request<RideDto>('POST', `/api/rides/${rideId}/arrived`),

  /** POST /api/rides/{id}/start — démarrer la course. */
  startRide: (rideId: number) => request<RideDto>('POST', `/api/rides/${rideId}/start`),

  /** POST /api/rides/{id}/complete — terminer la course. */
  completeRide: (rideId: number) => request<RideDto>('POST', `/api/rides/${rideId}/complete`),
};
