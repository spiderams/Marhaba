import { Pressable, Text, View } from 'react-native';
import { MaterialIcons } from '@expo/vector-icons';
import { colors } from '@/theme/colors';
import { OnlineBadge } from './OnlineBadge';

/**
 * Barre supérieure du tableau de bord : bouton menu, nom de la marque,
 * badge de disponibilité, et avatar du chauffeur.
 *
 * On lui passe l'état "en ligne" et les actions, car la TopBar ne décide rien
 * elle-même — elle affiche et délègue au parent (l'écran).
 */
type Props = Readonly<{
  online: boolean;
  onToggleOnline: () => void;
  onOpenMenu: () => void;
}>;

export function TopBar({ online, onToggleOnline, onOpenMenu }: Props) {
  return (
    <View className="flex-row items-center justify-between border-b border-outline-variant bg-surface px-4 py-2">
      {/* Partie gauche : menu + nom de la marque. */}
      <View className="flex-row items-center gap-3">
        <Pressable onPress={onOpenMenu} className="h-11 w-11 items-center justify-center">
          <MaterialIcons name="menu" size={26} color={colors.primary} />
        </Pressable>
        <Text className="text-[22px] font-bold text-primary">DjiboutiRide</Text>
      </View>

      {/* Partie droite : badge en ligne + avatar. */}
      <View className="flex-row items-center gap-2">
        <OnlineBadge online={online} onToggle={onToggleOnline} />
        <View className="h-10 w-10 items-center justify-center rounded-full border-2 border-primary-container bg-surface-container-high">
          <MaterialIcons name="person" size={24} color={colors.primary} />
        </View>
      </View>
    </View>
  );
}
