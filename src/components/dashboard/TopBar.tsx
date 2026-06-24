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
type Props = {
  online: boolean;
  onToggleOnline: () => void;
  onOpenMenu: () => void;
};

export function TopBar({ online, onToggleOnline, onOpenMenu }: Props) {
  return (
    <View
      style={{
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-between',
        paddingHorizontal: 16,
        paddingVertical: 8,
        backgroundColor: colors.surface,
        borderBottomWidth: 1,
        borderBottomColor: colors.outlineVariant,
      }}
    >
      {/* Partie gauche : menu + nom de la marque. */}
      <View style={{ flexDirection: 'row', alignItems: 'center', gap: 12 }}>
        <Pressable
          onPress={onOpenMenu}
          style={{ width: 44, height: 44, alignItems: 'center', justifyContent: 'center' }}
        >
          <MaterialIcons name="menu" size={26} color={colors.primary} />
        </Pressable>
        <Text style={{ fontSize: 22, fontWeight: '700', color: colors.primary }}>
          DjiboutiRide
        </Text>
      </View>

      {/* Partie droite : badge en ligne + avatar. */}
      <View style={{ flexDirection: 'row', alignItems: 'center', gap: 8 }}>
        <OnlineBadge online={online} onToggle={onToggleOnline} />
        <View
          style={{
            width: 40,
            height: 40,
            borderRadius: 9999,
            borderWidth: 2,
            borderColor: colors.primaryContainer,
            backgroundColor: colors.surfaceContainerHigh,
            alignItems: 'center',
            justifyContent: 'center',
          }}
        >
          <MaterialIcons name="person" size={24} color={colors.primary} />
        </View>
      </View>
    </View>
  );
}
