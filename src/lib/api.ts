import { API_BASE_URL } from './config';
import { getAccessToken } from './auth';
import type { AuthTokens, DriverDto, RideDto } from './types';

/**
 * Client HTTP minimal pour l'API TaxiDjibouti.
 *
 * Toutes les routes protégées passent le JWT en en-tête Authorization.
 * Le backend renvoie des erreurs typées (Result/Error) mappées en codes HTTP
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
  const headers: Record<string, string> = { 'Content-Type': 'application/json' };

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
  /** POST /api/auth/login — connexion par téléphone + mot de passe. */
  login: (phoneNumber: string, password: string) =>
    request<AuthTokens>('POST', '/api/auth/login', { phoneNumber, password }, false),

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
