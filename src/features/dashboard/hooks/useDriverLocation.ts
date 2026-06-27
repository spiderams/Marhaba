import { useEffect, useRef, useState } from 'react';
import * as Location from 'expo-location';

/** Coordonnées simples du chauffeur. */
export type DriverCoords = { latitude: number; longitude: number };

/** État de la localisation, pour piloter l'affichage de la carte. */
export type LocationStatus = 'loading' | 'granted' | 'denied';

/**
 * Logique GPS du chauffeur : demande la permission, puis suit la position en
 * continu. Ce hook ne sait RIEN de l'affichage — il expose juste l'état.
 *
 * Nettoyage : l'abonnement GPS (`watchPositionAsync`) est coupé au démontage,
 * sinon le GPS continue de tourner (batterie + fuite mémoire).
 */
export function useDriverLocation(): {
  location: DriverCoords | null;
  status: LocationStatus;
  error: string | null;
} {
  const [location, setLocation] = useState<DriverCoords | null>(null);
  const [status, setStatus] = useState<LocationStatus>('loading');
  const [error, setError] = useState<string | null>(null);

  // On garde l'abonnement dans une ref pour pouvoir le couper au démontage.
  const subscriptionRef = useRef<Location.LocationSubscription | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function start() {
      try {
        const { status: permission } =
          await Location.requestForegroundPermissionsAsync();

        if (permission !== 'granted') {
          if (!cancelled) setStatus('denied');
          return;
        }

        // Permission OK → on s'abonne aux mises à jour de position.
        const subscription = await Location.watchPositionAsync(
          {
            accuracy: Location.Accuracy.High,
            distanceInterval: 10, // mise à jour tous les ~10 m
            timeInterval: 5000, //  ou au plus tous les ~5 s
          },
          (pos) => {
            if (cancelled) return;
            setLocation({
              latitude: pos.coords.latitude,
              longitude: pos.coords.longitude,
            });
            setStatus('granted');
          },
        );
        subscriptionRef.current = subscription;
      } catch (e) {
        if (!cancelled) {
          setError('Impossible de lire la position GPS.');
          setStatus('denied');
        }
      }
    }

    start();

    // Nettoyage au démontage : on coupe l'abonnement GPS.
    return () => {
      cancelled = true;
      subscriptionRef.current?.remove();
      subscriptionRef.current = null;
    };
  }, []);

  return { location, status, error };
}
