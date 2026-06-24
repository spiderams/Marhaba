/**
 * Types métier miroir du backend TaxiDjibouti (Taxi.Domain / DTOs).
 * À garder synchronisés avec les agrégats et DTOs côté API.
 */

/** Statuts d'une course — miroir de RideStatus (Taxi.Domain/Rides/RideStatus.cs). */
export type RideStatus =
  | 'Pending'
  | 'Offered'
  | 'Accepted'
  | 'DriverArrived'
  | 'InProgress'
  | 'Completed'
  | 'Cancelled';

/** Rôles applicatifs — miroir de RoleNames. */
export type Role = 'Client' | 'Driver' | 'Admin';

/** DTO d'une course renvoyé par l'API (miroir de RideDto). */
export interface RideDto {
  id: number;
  clientId: string;
  driverId: number | null;
  pickupAddress: string;
  destinationAddress: string;
  pickupZone: string;
  destinationZone: string;
  pickupLatitude: number | null;
  pickupLongitude: number | null;
  destinationLatitude: number | null;
  destinationLongitude: number | null;
  estimatedPrice: number;
  status: RideStatus;
}

/** Profil chauffeur (miroir de DriverDto). */
export interface DriverDto {
  id: number;
  userId: string;
  licenseNumber: string;
  vehiclePlate: string;
  vehicleType: string;
  isAvailable: boolean;
  averageRating: number;
}

/** Utilisateur connecté (renvoyé dans la réponse de login). */
export interface AuthUser {
  id: string;
  fullName: string;
  phoneNumber: string;
  roles: Role[];
}

/** Réponse d'authentification (miroir exact de la réponse de POST /api/auth/login). */
export interface AuthResponse {
  accessToken: string;
  expiresAt: string; // ISO 8601
  tokenType: string; // "Bearer"
  refreshToken: string;
  refreshTokenExpiresAt: string; // ISO 8601
  user: AuthUser;
}

// --- Événements SignalR (RideHub) — payloads reçus par l'app chauffeur ---

/** `rideOffered` : une course est offerte à ce chauffeur (vague de dispatch). */
export interface RideOfferedEvent {
  rideId: number;
  expiresAt: string; // ISO 8601
}

/** `rideOfferRevoked` : l'offre n'est plus valide (prise, expirée, annulée). */
export interface RideOfferRevokedEvent {
  rideId: number;
  reason: 'taken' | 'expired' | 'cancelled';
}

/** `rideStatusChanged` : transition d'état d'une course. */
export interface RideStatusChangedEvent {
  rideId: number;
  status: RideStatus;
  driverId: number | null;
}
