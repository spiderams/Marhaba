import { useRef, useState } from 'react';
import { Pressable, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { router } from 'expo-router';
import { MaterialIcons } from '@expo/vector-icons';

import { TopBar } from './components/TopBar';
import { DriverMap, type DriverMapHandle } from './components/DriverMap';
import { EarningsCard } from './components/EarningsCard';
import { BottomNav } from './components/BottomNav';
import { clearTokens } from '@/lib/auth';
import { colors } from '@/theme/colors';

/**
 * TABLEAU DE BORD DU CHAUFFEUR (écran d'accueil).
 *
 * Cet écran ne fait qu'ASSEMBLER les composants de ./components.
 * On voit d'un coup d'œil sa structure : une barre en haut, une carte en fond,
 * et des éléments flottants par-dessus (gains, boutons, navigation).
 *
 * Il gère le minimum d'état : si le chauffeur est en ligne ou non, et l'onglet
 * actif de la barre du bas. (Plus tard, "en ligne" déclenchera l'appel API
 * set-availability et la connexion SignalR.)
 */
export function DashboardScreen() {
  const [online, setOnline] = useState(true);
  const [activeTab, setActiveTab] = useState('home');
  // Référence vers la carte pour déclencher le recentrage depuis le bouton flottant.
  const mapRef = useRef<DriverMapHandle>(null);

  /** Déconnexion : on efface les jetons et on retourne à l'écran de connexion. */
  async function handleLogout() {
    await clearTokens();
    router.replace('/login');
  }

  return (
    <SafeAreaView className="flex-1 bg-surface" edges={['top', 'bottom']}>
      {/* 1. Barre supérieure (le bouton menu déconnecte, en attendant le vrai menu latéral) */}
      <TopBar
        online={online}
        onToggleOnline={() => setOnline((v) => !v)}
        onOpenMenu={handleLogout}
      />

      {/* 2. Zone centrale : la carte en fond + éléments flottants par-dessus. */}
      <View className="flex-1">
        <DriverMap ref={mapRef} />

        {/* Boutons d'action flottants (ma position + éclair). */}
        <View className="absolute bottom-[140px] right-4 gap-4">
          <Pressable
            onPress={() => mapRef.current?.recenter()}
            style={floatingShadow}
            className="h-14 w-14 items-center justify-center rounded-full bg-white"
          >
            <MaterialIcons name="my-location" size={24} color={colors.primary} />
          </Pressable>
          <Pressable style={floatingShadow} className="h-14 w-14 items-center justify-center rounded-full bg-secondary-container">
            <MaterialIcons name="bolt" size={24} color={colors.onSecondaryContainer} />
          </Pressable>
        </View>

        {/* Carte flottante des gains, ancrée en bas. */}
        <View className="absolute inset-x-4 bottom-4">
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

/**
 * Ombre portée commune des boutons flottants (pas d'équivalent NativeWind).
 * `boxShadow` = API moderne RN 0.81+ remplaçant les anciennes props shadow/elevation.
 */
const floatingShadow = {
  boxShadow: '0px 2px 8px rgba(0, 0, 0, 0.15)',
} as const;
