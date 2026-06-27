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

type Props = Readonly<{
  activeKey: string;
  onSelect: (key: string) => void;
}>;

export function BottomNav({ activeKey, onSelect }: Props) {
  return (
    <View className="flex-row items-center justify-around border-t border-outline-variant bg-surface py-2">
      {TABS.map((tab) => {
        const isActive = tab.key === activeKey;
        return (
          <Pressable
            key={tab.key}
            onPress={() => onSelect(tab.key)}
            // L'onglet actif a un fond bleu clair (classe conditionnelle).
            className={`h-16 min-w-[72px] items-center justify-center rounded-xl ${
              isActive ? 'bg-primary-container' : 'bg-transparent'
            }`}
          >
            <MaterialIcons
              name={tab.icon}
              size={24}
              color={isActive ? colors.onPrimaryContainer : colors.onSurfaceVariant}
            />
            <Text
              className={`mt-1 text-xs font-bold ${
                isActive ? 'text-on-primary-container' : 'text-on-surface-variant'
              }`}
            >
              {tab.label}
            </Text>
          </Pressable>
        );
      })}
    </View>
  );
}
