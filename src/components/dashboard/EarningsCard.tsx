import { Pressable, Text, View } from 'react-native';
import { MaterialIcons } from '@expo/vector-icons';
import { colors } from '@/theme/colors';

/**
 * Carte flottante « Gains du jour ».
 *
 * Affiche le montant gagné aujourd'hui (en francs djiboutiens, DJF), une tendance,
 * et un bouton DÉTAILS. Le montant est passé en prop : pour l'instant l'écran
 * fournit une valeur de démonstration ; plus tard elle viendra de l'API.
 */
type Props = {
  amount: number; // ex : 12450
  trendPercent: number; // ex : 12 pour +12%
  onPressDetails: () => void;
};

export function EarningsCard({ amount, trendPercent, onPressDetails }: Props) {
  // Formate 12450 → "12,450" (séparateur de milliers).
  const formatted = amount.toLocaleString('en-US');

  return (
    <View
      style={{
        backgroundColor: colors.white,
        borderRadius: 16,
        padding: 20,
        borderWidth: 1,
        borderColor: colors.outlineVariant,
        flexDirection: 'row',
        justifyContent: 'space-between',
        alignItems: 'center',
        // Petite ombre pour "flotter" au-dessus de la carte.
        shadowColor: '#000',
        shadowOpacity: 0.12,
        shadowRadius: 12,
        shadowOffset: { width: 0, height: 4 },
        elevation: 4, // ombre côté Android
      }}
    >
      {/* Gauche : libellé + montant. */}
      <View style={{ gap: 4 }}>
        <Text
          style={{
            color: colors.onSurfaceVariant,
            fontSize: 12,
            fontWeight: '700',
            letterSpacing: 0.5,
          }}
        >
          GAINS DU JOUR
        </Text>
        <View style={{ flexDirection: 'row', alignItems: 'flex-end', gap: 6 }}>
          <Text style={{ fontSize: 26, fontWeight: '700', color: colors.primary }}>
            {formatted}
          </Text>
          <Text style={{ fontSize: 14, color: colors.onSurfaceVariant, marginBottom: 3 }}>
            DJF
          </Text>
        </View>
      </View>

      {/* Droite : tendance + bouton DÉTAILS. */}
      <View style={{ alignItems: 'flex-end', gap: 8 }}>
        <View style={{ flexDirection: 'row', alignItems: 'center' }}>
          <MaterialIcons name="trending-up" size={16} color={colors.statusSuccess} />
          <Text style={{ color: colors.statusSuccess, fontWeight: '700', marginLeft: 4 }}>
            +{trendPercent}%
          </Text>
        </View>
        <Pressable
          onPress={onPressDetails}
          style={{
            backgroundColor: colors.secondaryContainer,
            paddingHorizontal: 16,
            paddingVertical: 8,
            borderRadius: 12,
          }}
        >
          <Text style={{ color: colors.onSecondaryContainer, fontWeight: '700', fontSize: 13 }}>
            DÉTAILS
          </Text>
        </Pressable>
      </View>
    </View>
  );
}
