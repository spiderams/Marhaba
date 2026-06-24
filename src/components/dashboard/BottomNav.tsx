import { Pressable, Text, View } from 'react-native';
import { MaterialIcons } from '@expo/vector-icons';
import { colors } from '@/theme/colors';

/**
 * Barre de navigation du bas : Accueil / Gains / Historique / Aide.
 *
 * Version visuelle : seul l'onglet "Accueil" est actif (mis en évidence).
 * Les autres onglets sont affichés mais ne naviguent pas encore — on créera
 * leurs écrans plus tard. On garde la liste des onglets dans un tableau pour
 * éviter de répéter le même bloc 4 fois (principe DRY).
 */

// Le type d'icône accepté par MaterialIcons (pour que TypeScript nous aide).
type IconName = keyof typeof MaterialIcons.glyphMap;

type Tab = { key: string; label: string; icon: IconName };

const TABS: Tab[] = [
  { key: 'home', label: 'Accueil', icon: 'directions-car' },
  { key: 'earnings', label: 'Gains', icon: 'payments' },
  { key: 'history', label: 'Historique', icon: 'history' },
  { key: 'help', label: 'Aide', icon: 'support-agent' },
];

type Props = {
  activeKey: string;
  onSelect: (key: string) => void;
};

export function BottomNav({ activeKey, onSelect }: Props) {
  return (
    <View
      style={{
        flexDirection: 'row',
        justifyContent: 'space-around',
        alignItems: 'center',
        backgroundColor: colors.surface,
        borderTopWidth: 1,
        borderTopColor: colors.outlineVariant,
        paddingVertical: 8,
      }}
    >
      {TABS.map((tab) => {
        const isActive = tab.key === activeKey;
        return (
          <Pressable
            key={tab.key}
            onPress={() => onSelect(tab.key)}
            style={{
              alignItems: 'center',
              justifyContent: 'center',
              minWidth: 72,
              height: 64,
              borderRadius: 12,
              // L'onglet actif a un fond bleu clair.
              backgroundColor: isActive ? colors.primaryContainer : 'transparent',
            }}
          >
            <MaterialIcons
              name={tab.icon}
              size={24}
              color={isActive ? colors.onPrimaryContainer : colors.onSurfaceVariant}
            />
            <Text
              style={{
                fontSize: 12,
                marginTop: 4,
                fontWeight: '700',
                color: isActive ? colors.onPrimaryContainer : colors.onSurfaceVariant,
              }}
            >
              {tab.label}
            </Text>
          </Pressable>
        );
      })}
    </View>
  );
}
