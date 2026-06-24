import { ActivityIndicator, Pressable, Text } from 'react-native';
import { MaterialIcons } from '@expo/vector-icons';
import { colors } from '@/theme/colors';

/**
 * Bouton d'action principal (jaune Taxi), réutilisable.
 *
 * - `label` : le texte affiché.
 * - `onPress` : action au clic.
 * - `loading` : si vrai, affiche un indicateur de chargement et désactive le bouton
 *   (utile pendant un appel réseau, ex. la connexion).
 * - `disabled` : désactive le bouton (grisé).
 */
type Props = {
  label: string;
  onPress: () => void;
  loading?: boolean;
  disabled?: boolean;
};

export function PrimaryButton({ label, onPress, loading = false, disabled = false }: Props) {
  const isDisabled = disabled || loading;

  return (
    <Pressable
      onPress={onPress}
      disabled={isDisabled}
      style={{
        height: 56,
        borderRadius: 12,
        backgroundColor: colors.secondaryContainer,
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'center',
        gap: 8,
        opacity: isDisabled ? 0.5 : 1, // grisé quand inactif
      }}
    >
      {loading ? (
        <ActivityIndicator color={colors.onSecondaryContainer} />
      ) : (
        <>
          <Text style={{ color: colors.onSecondaryContainer, fontWeight: '700', fontSize: 16 }}>
            {label}
          </Text>
          <MaterialIcons name="arrow-forward" size={20} color={colors.onSecondaryContainer} />
        </>
      )}
    </Pressable>
  );
}
