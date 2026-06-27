import { Text, View } from 'react-native';
import { MaterialIcons } from '@expo/vector-icons';
import { colors } from '@/theme/colors';

/**
 * Faux-fond de carte (placeholder).
 *
 * Pour démarrer simplement, on n'affiche PAS encore une vraie carte Google Maps
 * (qui demande une clé API et ne marche pas sur le web). On montre une zone grise
 * avec le marqueur de position du chauffeur au centre. On remplacera ce composant
 * par react-native-maps plus tard, sans toucher au reste de l'écran.
 */
export function MapPlaceholder() {
  return (
    <View className="flex-1 items-center justify-center bg-surface-container">
      {/* Marqueur de position du chauffeur (le taxi noir de la maquette). */}
      <View className="rounded-xl bg-primary p-2.5">
        <MaterialIcons name="local-taxi" size={32} color={colors.white} />
      </View>
      <View className="mt-1 h-3.5 w-3.5 rounded-full border-2 border-white bg-primary" />

      <Text className="mt-4 text-[13px] text-on-surface-variant">Carte (à venir)</Text>
    </View>
  );
}
