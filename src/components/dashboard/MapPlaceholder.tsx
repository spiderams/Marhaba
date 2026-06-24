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
    <View
      style={{
        flex: 1,
        backgroundColor: colors.surfaceContainer,
        alignItems: 'center',
        justifyContent: 'center',
      }}
    >
      {/* Marqueur de position du chauffeur (le taxi noir de la maquette). */}
      <View
        style={{
          backgroundColor: colors.primary,
          padding: 10,
          borderRadius: 12,
        }}
      >
        <MaterialIcons name="local-taxi" size={32} color={colors.white} />
      </View>
      <View
        style={{
          width: 14,
          height: 14,
          borderRadius: 9999,
          backgroundColor: colors.primary,
          borderWidth: 2,
          borderColor: colors.white,
          marginTop: 4,
        }}
      />

      <Text style={{ marginTop: 16, color: colors.onSurfaceVariant, fontSize: 13 }}>
        Carte (à venir)
      </Text>
    </View>
  );
}
