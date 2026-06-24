import { Text, TextInput, View } from 'react-native';
import { colors } from '@/theme/colors';

/**
 * Champ de saisie du numéro de téléphone, avec préfixe pays +253 (Djibouti).
 *
 * Composant contrôlé : la valeur saisie vit chez le parent (prop `value`),
 * et chaque frappe remonte via `onChangeText`. Le parent décide quoi en faire.
 */
type Props = {
  value: string;
  onChangeText: (text: string) => void;
};

export function PhoneInput({ value, onChangeText }: Props) {
  return (
    <View style={{ gap: 8 }}>
      <Text
        style={{
          fontWeight: '700',
          fontSize: 14,
          color: colors.onSurfaceVariant,
          marginLeft: 4,
        }}
      >
        Numéro de téléphone
      </Text>

      {/* Conteneur : drapeau à gauche + champ de saisie à droite. */}
      <View
        style={{
          flexDirection: 'row',
          alignItems: 'center',
          height: 56,
          borderWidth: 1,
          borderColor: colors.outlineVariant,
          borderRadius: 8,
          backgroundColor: colors.white,
          overflow: 'hidden',
        }}
      >
        {/* Bloc drapeau (indicatif géré côté backend, on n'ajoute rien au numéro). */}
        <View
          style={{
            alignItems: 'center',
            justifyContent: 'center',
            paddingHorizontal: 16,
            height: '100%',
            backgroundColor: colors.surfaceContainer,
            borderRightWidth: 1,
            borderRightColor: colors.outlineVariant,
          }}
        >
          <Text style={{ fontSize: 18 }}>🇩🇯</Text>
        </View>

        {/* Champ numéro. keyboardType="phone-pad" affiche le clavier numérique. */}
        <TextInput
          value={value}
          onChangeText={onChangeText}
          placeholder="Votre numéro"
          placeholderTextColor={colors.outlineVariant}
          keyboardType="phone-pad"
          style={{
            flex: 1,
            paddingHorizontal: 16,
            fontSize: 18,
            color: colors.onSurface,
            height: '100%',
          }}
        />
      </View>
    </View>
  );
}
