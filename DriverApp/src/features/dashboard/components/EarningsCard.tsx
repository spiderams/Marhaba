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
type Props = Readonly<{
  amount: number; // ex : 12450
  trendPercent: number; // ex : 12 pour +12%
  onPressDetails: () => void;
}>;

// L'ombre portée reste en `style` : pas d'équivalent fiable en classes NativeWind.
// `boxShadow` est l'API moderne RN (0.81+) qui remplace les anciennes props
// shadow/elevation. Format CSS : "offsetX offsetY blur couleur" (couleur avec alpha).
const cardShadow = {
  boxShadow: '0px 4px 12px rgba(0, 0, 0, 0.12)',
} as const;

export function EarningsCard({ amount, trendPercent, onPressDetails }: Props) {
  // Formate 12450 → "12,450" (séparateur de milliers).
  const formatted = amount.toLocaleString('en-US');

  return (
    <View
      style={cardShadow}
      className="flex-row items-center justify-between rounded-2xl border border-outline-variant bg-white p-5"
    >
      {/* Gauche : libellé + montant. */}
      <View className="gap-1">
        <Text className="text-xs font-bold tracking-wide text-on-surface-variant">
          GAINS DU JOUR
        </Text>
        <View className="flex-row items-end gap-1.5">
          <Text className="text-[26px] font-bold text-primary">{formatted}</Text>
          <Text className="mb-[3px] text-sm text-on-surface-variant">DJF</Text>
        </View>
      </View>

      {/* Droite : tendance + bouton DÉTAILS. */}
      <View className="items-end gap-2">
        <View className="flex-row items-center">
          <MaterialIcons name="trending-up" size={16} color={colors.statusSuccess} />
          <Text className="ml-1 font-bold text-status-success">+{trendPercent}%</Text>
        </View>
        <Pressable
          onPress={onPressDetails}
          className="rounded-xl bg-secondary-container px-4 py-2"
        >
          <Text className="text-[13px] font-bold text-on-secondary-container">DÉTAILS</Text>
        </Pressable>
      </View>
    </View>
  );
}
