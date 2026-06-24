import { useState } from 'react';
import { Pressable, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { router } from 'expo-router';
import { MaterialIcons } from '@expo/vector-icons';

import { TopBar } from '@/components/dashboard/TopBar';
import { MapPlaceholder } from '@/components/dashboard/MapPlaceholder';
import { EarningsCard } from '@/components/dashboard/EarningsCard';
import { BottomNav } from '@/components/dashboard/BottomNav';
import { clearTokens } from '@/lib/auth';
import { colors } from '@/theme/colors';

/**
 * TABLEAU DE BORD DU CHAUFFEUR (écran d'accueil).
 *
 * Cet écran ne fait qu'ASSEMBLER les composants du dossier components/dashboard.
 * On voit d'un coup d'œil sa structure : une barre en haut, une carte en fond,
 * et des éléments flottants par-dessus (gains, boutons, navigation).
 *
 * Il gère le minimum d'état : si le chauffeur est en ligne ou non, et l'onglet
 * actif de la barre du bas. (Plus tard, "en ligne" déclenchera l'appel API
 * set-availability et la connexion SignalR.)
 */
export default function DashboardScreen() {
  const [online, setOnline] = useState(true);
  const [activeTab, setActiveTab] = useState('home');

  /** Déconnexion : on efface les jetons et on retourne à l'écran de connexion. */
  async function handleLogout() {
    await clearTokens();
    router.replace('/login');
  }

  return (
    <SafeAreaView style={{ flex: 1, backgroundColor: colors.surface }} edges={['top', 'bottom']}>
      {/* 1. Barre supérieure (le bouton menu déconnecte, en attendant le vrai menu latéral) */}
      <TopBar
        online={online}
        onToggleOnline={() => setOnline((v) => !v)}
        onOpenMenu={handleLogout}
      />

      {/* 2. Zone centrale : la carte en fond + éléments flottants par-dessus. */}
      <View style={{ flex: 1 }}>
        <MapPlaceholder />

        {/* Boutons d'action flottants (ma position + éclair). */}
        <View style={{ position: 'absolute', right: 16, bottom: 140, gap: 16 }}>
          <Pressable style={floatingButton(colors.white)}>
            <MaterialIcons name="my-location" size={24} color={colors.primary} />
          </Pressable>
          <Pressable style={floatingButton(colors.secondaryContainer)}>
            <MaterialIcons name="bolt" size={24} color={colors.onSecondaryContainer} />
          </Pressable>
        </View>

        {/* Carte flottante des gains, ancrée en bas. */}
        <View style={{ position: 'absolute', left: 16, right: 16, bottom: 16 }}>
          <EarningsCard
            amount={12450}
            trendPercent={12}
            onPressDetails={() => {
              // Écran de détails des gains à venir.
            }}
          />
        </View>
      </View>

      {/* 3. Navigation du bas. */}
      <BottomNav activeKey={activeTab} onSelect={setActiveTab} />
    </SafeAreaView>
  );
}

/** Style commun des deux boutons ronds flottants (évite la répétition). */
function floatingButton(backgroundColor: string) {
  return {
    width: 56,
    height: 56,
    borderRadius: 9999,
    backgroundColor,
    alignItems: 'center' as const,
    justifyContent: 'center' as const,
    shadowColor: '#000',
    shadowOpacity: 0.15,
    shadowRadius: 8,
    shadowOffset: { width: 0, height: 2 },
    elevation: 4,
  };
}
