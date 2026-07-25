import { useState } from 'react';
import { ScrollView, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { router } from 'expo-router';
import { MaterialIcons } from '@expo/vector-icons';

import { PhoneInput } from './components/PhoneInput';
import { PasswordInput } from './components/PasswordInput';
import { PrimaryButton } from './components/PrimaryButton';
import { api, ApiError } from '@/lib/api';
import { saveTokens } from '@/lib/auth';
import { colors } from '@/theme/colors';

/**
 * ÉCRAN DE CONNEXION.
 *
 * Le chauffeur saisit son numéro + mot de passe, on appelle l'API backend
 * (POST /api/auth/login), on stocke les jetons reçus, puis on l'envoie vers
 * le tableau de bord.
 *
 * États gérés :
 * - phone / password : ce que l'utilisateur tape.
 * - loading : true pendant l'appel réseau (désactive le bouton).
 * - error : message d'erreur à afficher si la connexion échoue.
 */
export function LoginScreen() {
  const [phone, setPhone] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Le bouton n'est actif que si les deux champs sont remplis.
  const canSubmit = phone.trim().length > 0 && password.length > 0;

  async function handleLogin() {
    setError(null);
    setLoading(true);
    try {
      // Le backend attend le numéro brut (sans préfixe). On retire juste les espaces.
      const cleanPhone = phone.replace(/\s/g, '');
      const auth = await api.login(cleanPhone, password);
      await saveTokens(auth);
      // Connexion réussie → on remplace l'écran de login par le tableau de bord.
      router.replace('/');
    } catch (e) {
      // On traduit l'erreur backend en message clair pour le chauffeur.
      if (e instanceof ApiError && e.status === 401) {
        setError('Numéro ou mot de passe incorrect.');
      } else {
        setError('Connexion impossible. Vérifiez votre réseau et réessayez.');
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <SafeAreaView className="flex-1 bg-surface">
      <ScrollView
        contentContainerStyle={{ flexGrow: 1, paddingHorizontal: 16, paddingBottom: 24 }}
        keyboardShouldPersistTaps="handled"
      >
        {/* Marque */}
        <View className="mt-4 flex-row items-center gap-2">
          <MaterialIcons name="local-taxi" size={28} color={colors.primary} />
          <Text className="text-[22px] font-bold text-primary">DjiboutiRide</Text>
        </View>

        {/* Accroche */}
        <View className="mb-8 mt-10">
          <Text className="mb-2 text-[26px] font-bold text-primary">Bienvenue</Text>
          <Text className="text-base leading-6 text-on-surface-variant">
            Connectez-vous pour commencer votre trajet avec le service le plus fiable de Djibouti.
          </Text>
        </View>

        {/* Formulaire */}
        <View className="gap-5">
          <PhoneInput value={phone} onChangeText={setPhone} />
          <PasswordInput value={password} onChangeText={setPassword} />

          {/* Message d'erreur (affiché seulement s'il y en a un).
              On teste `!== null` explicitement pour ne jamais rendre une valeur
              falsy par accident (bonne pratique JSX). */}
          {error !== null && <Text className="ml-1 text-sm text-status-error">{error}</Text>}

          <PrimaryButton
            label="Se connecter"
            onPress={handleLogin}
            loading={loading}
            disabled={!canSubmit}
          />
        </View>

        {/* Mentions légales, poussées en bas. */}
        <View className="mt-auto pt-8">
          <Text className="text-center text-xs leading-[18px] text-on-surface-variant">
            En continuant, vous acceptez nos Conditions d'utilisation et notre Politique de
            confidentialité.
          </Text>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}
