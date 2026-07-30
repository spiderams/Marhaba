import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import { RIDE_HUB_URL } from './config';
import { getAccessToken } from './auth';
import type {
  RideOfferedEvent,
  RideOfferRevokedEvent,
  RideStatusChangedEvent,
} from './types';

/**
 * Client temps réel SignalR vers le RideHub du backend.
 *
 * Authentification : le JWT est passé en query string (?access_token=...),
 * conformément à la configuration du hub côté serveur.
 *
 * Côté chauffeur, on rejoint le groupe personnel (JoinMyDriverGroup) pour
 * recevoir les offres ciblées (rideOffered) et leurs révocations
 * (rideOfferRevoked) issues du dispatch en vagues.
 */

/** Callbacks d'événements que l'UI peut brancher. */
export interface RideHubHandlers {
  onRideOffered?: (e: RideOfferedEvent) => void;
  onRideOfferRevoked?: (e: RideOfferRevokedEvent) => void;
  onRideStatusChanged?: (e: RideStatusChangedEvent) => void;
}

let connection: HubConnection | null = null;

/**
 * Établit (ou réutilise) la connexion au RideHub, branche les handlers et
 * rejoint le groupe personnel du chauffeur. Retourne la connexion active.
 */
export async function connectRideHub(handlers: RideHubHandlers): Promise<HubConnection> {
  if (connection && connection.state === HubConnectionState.Connected) {
    return connection;
  }

  connection = new HubConnectionBuilder()
    .withUrl(RIDE_HUB_URL, {
      accessTokenFactory: async () => (await getAccessToken()) ?? '',
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();

  if (handlers.onRideOffered)
    connection.on('rideOffered', handlers.onRideOffered);
  if (handlers.onRideOfferRevoked)
    connection.on('rideOfferRevoked', handlers.onRideOfferRevoked);
  if (handlers.onRideStatusChanged)
    connection.on('rideStatusChanged', handlers.onRideStatusChanged);

  await connection.start();
  await connection.invoke('JoinMyDriverGroup');
  return connection;
}

/** Envoie la position GPS du chauffeur pendant une course (SendDriverLocation). */
export async function sendDriverLocation(
  rideId: number,
  latitude: number,
  longitude: number,
  heading?: number,
  speed?: number,
): Promise<void> {
  if (!connection || connection.state !== HubConnectionState.Connected) return;
  await connection.invoke('SendDriverLocation', {
    rideId,
    latitude,
    longitude,
    heading,
    speed,
  });
}

/** Ferme la connexion (déconnexion / fermeture d'app). */
export async function disconnectRideHub(): Promise<void> {
  if (connection) {
    await connection.stop();
    connection = null;
  }
}
