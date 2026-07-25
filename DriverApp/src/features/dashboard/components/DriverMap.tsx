import { forwardRef, useEffect, useImperativeHandle, useRef } from 'react';
import { Linking, Pressable, Text, View } from 'react-native';
import MapView, { Marker, type Region } from 'react-native-maps';
import { MaterialIcons } from '@expo/vector-icons';
import { useDriverLocation } from '../hooks/useDriverLocation';
import { colors } from '@/theme/colors';

/** Niveau de zoom initial (deltas Region). Plus petit = plus zoomé. */
const ZOOM_DELTA = { latitudeDelta: 0.01, longitudeDelta: 0.01 } as const;

/** Méthodes impératives exposées au parent (le dashboard). */
export type DriverMapHandle = { recenter: () => void };

/**
 * Carte du chauffeur. Affiche sa position (marqueur taxi) et suit ses
 * déplacements. Ne gère PAS le GPS lui-même : il vient du hook useDriverLocation.
 *
 * Le parent peut appeler `recenter()` (via ref) pour recentrer sur le chauffeur,
 * ce qui branche le bouton flottant déjà présent dans le dashboard.
 */
export const DriverMap = forwardRef<DriverMapHandle>(function DriverMap(_props, ref) {
  const { location, status } = useDriverLocation();
  const mapRef = useRef<MapView | null>(null);
  // Mémorise si on a déjà fait le centrage initial (une seule fois).
  const didCenterRef = useRef(false);

  // Recentre la carte sur la position courante du chauffeur.
  function recenter() {
    if (!location) return;
    const region: Region = { ...location, ...ZOOM_DELTA };
    mapRef.current?.animateToRegion(region, 500);
  }

  // On expose `recenter` au parent via la ref.
  useImperativeHandle(ref, () => ({ recenter }));

  // Centrage initial : au premier fix GPS seulement.
  useEffect(() => {
    if (location && !didCenterRef.current) {
      didCenterRef.current = true;
      const region: Region = { ...location, ...ZOOM_DELTA };
      mapRef.current?.animateToRegion(region, 0);
    }
  }, [location]);

  // Permission refusée : message + accès aux réglages.
  if (status === 'denied') {
    return (
      <View className="flex-1 items-center justify-center bg-surface-container px-8">
        <MaterialIcons name="location-off" size={40} color={colors.onSurfaceVariant} />
        <Text className="mt-4 text-center text-base text-on-surface-variant">
          La localisation est désactivée. Activez-la pour voir votre position.
        </Text>
        <Pressable
          onPress={() => Linking.openSettings()}
          className="mt-4 rounded-xl bg-secondary-container px-4 py-2"
        >
          <Text className="font-bold text-on-secondary-container">Ouvrir les réglages</Text>
        </Pressable>
      </View>
    );
  }

  // En attente du premier fix GPS : fond neutre.
  if (status === 'loading' || !location) {
    return <View className="flex-1 bg-surface-container" />;
  }

  // Position disponible : la carte.
  return (
    <MapView
      ref={mapRef}
      style={{ flex: 1 }}
      initialRegion={{ ...location, ...ZOOM_DELTA }}
      showsUserLocation={false}
    >
      <Marker coordinate={location} title="Vous">
        <View className="rounded-xl bg-primary p-2">
          <MaterialIcons name="local-taxi" size={24} color={colors.white} />
        </View>
      </Marker>
    </MapView>
  );
});
