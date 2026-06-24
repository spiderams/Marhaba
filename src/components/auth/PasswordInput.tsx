import { useState } from 'react';
import { Pressable, Text, TextInput, View } from 'react-native';
import { MaterialIcons } from '@expo/vector-icons';
import { colors } from '@/theme/colors';

/**
 * Champ de saisie du mot de passe, avec un bouton "œil" pour afficher/masquer.
 *
 * Composant contrôlé pour la valeur (value / onChangeText vivent chez le parent).
 * En revanche, le fait de montrer ou masquer le texte est un détail purement
 * visuel local → on le gère ICI avec un useState interne (`hidden`).
 */
type Props = {
  value: string;
  onChangeText: (text: string) => void;
};

export function PasswordInput({ value, onChangeText }: Props) {
  const [hidden, setHidden] = useState(true);

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
        Mot de passe
      </Text>

      <View
        style={{
          flexDirection: 'row',
          alignItems: 'center',
          height: 56,
          borderWidth: 1,
          borderColor: colors.outlineVariant,
          borderRadius: 8,
          backgroundColor: colors.white,
          paddingHorizontal: 16,
        }}
      >
        {/* secureTextEntry masque le texte (points) quand hidden est vrai. */}
        <TextInput
          value={value}
          onChangeText={onChangeText}
          placeholder="••••••••"
          placeholderTextColor={colors.outlineVariant}
          secureTextEntry={hidden}
          style={{ flex: 1, fontSize: 18, color: colors.onSurface, height: '100%' }}
        />

        {/* Bouton œil : bascule l'affichage. */}
        <Pressable onPress={() => setHidden((v) => !v)} hitSlop={8}>
          <MaterialIcons
            name={hidden ? 'visibility' : 'visibility-off'}
            size={22}
            color={colors.onSurfaceVariant}
          />
        </Pressable>
      </View>
    </View>
  );
}
