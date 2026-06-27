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
type Props = Readonly<{
  label: string;
  onPress: () => void;
  loading?: boolean;
  disabled?: boolean;
}>;

export function PrimaryButton({ label, onPress, loading = false, disabled = false }: Props) {
  const isDisabled = disabled || loading;

  return (
    <Pressable
      onPress={onPress}
      disabled={isDisabled}
      // L'opacité est conditionnelle : NativeWind accepte une classe dynamique
      // (grisé quand le bouton est inactif).
      className={`h-14 flex-row items-center justify-center gap-2 rounded-xl bg-secondary-container ${
        isDisabled ? 'opacity-50' : 'opacity-100'
      }`}
    >
      {loading ? (
        <ActivityIndicator color={colors.onSecondaryContainer} />
      ) : (
        <>
          <Text className="text-base font-bold text-on-secondary-container">
            {label}
          </Text>
          <MaterialIcons name="arrow-forward" size={20} color={colors.onSecondaryContainer} />
        </>
      )}
    </Pressable>
  );
}
