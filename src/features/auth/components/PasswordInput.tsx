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
type Props = Readonly<{
  value: string;
  onChangeText: (text: string) => void;
}>;

export function PasswordInput({ value, onChangeText }: Props) {
  const [hidden, setHidden] = useState(true);

  return (
    <View className="gap-2">
      <Text className="ml-1 text-sm font-bold text-on-surface-variant">
        Mot de passe
      </Text>

      <View className="h-14 flex-row items-center rounded-lg border border-outline-variant bg-white px-4">
        {/* secureTextEntry masque le texte (points) quand hidden est vrai. */}
        <TextInput
          value={value}
          onChangeText={onChangeText}
          placeholder="••••••••"
          placeholderTextColor={colors.outlineVariant}
          secureTextEntry={hidden}
          className="h-full flex-1 text-lg text-on-surface"
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
