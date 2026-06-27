import { Text, TextInput, View } from 'react-native';
import { colors } from '@/theme/colors';

/**
 * Champ de saisie du numéro de téléphone, avec préfixe pays 🇩🇯 (Djibouti).
 *
 * Composant contrôlé : la valeur saisie vit chez le parent (prop `value`),
 * et chaque frappe remonte via `onChangeText`. Le parent décide quoi en faire.
 *
 * Style via NativeWind (`className`). On garde `colors` seulement pour les props
 * qui n'acceptent pas de classe (placeholderTextColor).
 */
type Props = Readonly<{
  value: string;
  onChangeText: (text: string) => void;
}>;

export function PhoneInput({ value, onChangeText }: Props) {
  return (
    <View className="gap-2">
      <Text className="ml-1 text-sm font-bold text-on-surface-variant">
        Numéro de téléphone
      </Text>

      {/* Conteneur : drapeau à gauche + champ de saisie à droite. */}
      <View className="h-14 flex-row items-center overflow-hidden rounded-lg border border-outline-variant bg-white">
        {/* Bloc drapeau (indicatif géré côté backend, on n'ajoute rien au numéro). */}
        <View className="h-full items-center justify-center border-r border-outline-variant bg-surface-container px-4">
          <Text className="text-lg">🇩🇯</Text>
        </View>

        {/* Champ numéro. keyboardType="phone-pad" affiche le clavier numérique. */}
        <TextInput
          value={value}
          onChangeText={onChangeText}
          placeholder="Votre numéro"
          placeholderTextColor={colors.outlineVariant}
          keyboardType="phone-pad"
          className="h-full flex-1 px-4 text-lg text-on-surface"
        />
      </View>
    </View>
  );
}
