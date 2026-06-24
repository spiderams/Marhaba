import { useState } from 'react';
import { ScrollView, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { router } from 'expo-router';
import { MaterialIcons } from '@expo/vector-icons';

import { PhoneInput } from '@/components/auth/PhoneInput';
import { PasswordInput } from '@/components/auth/PasswordInput';
import { PrimaryButton } from '@/components/auth/PrimaryButton';
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
export default function LoginScreen() {
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
    <SafeAreaView style={{ flex: 1, backgroundColor: colors.surface }}>
      <ScrollView
        contentContainerStyle={{ flexGrow: 1, paddingHorizontal: 16, paddingBottom: 24 }}
        keyboardShouldPersistTaps="handled"
      >
        {/* Marque */}
        <View style={{ flexDirection: 'row', alignItems: 'center', gap: 8, marginTop: 16 }}>
          <MaterialIcons name="local-taxi" size={28} color={colors.primary} />
          <Text style={{ fontSize: 22, fontWeight: '700', color: colors.primary }}>
            DjiboutiRide
          </Text>
        </View>

        {/* Accroche */}
        <View style={{ marginTop: 40, marginBottom: 32 }}>
          <Text style={{ fontSize: 26, fontWeight: '700', color: colors.primary, marginBottom: 8 }}>
            Bienvenue
          </Text>
          <Text style={{ fontSize: 16, color: colors.onSurfaceVariant, lineHeight: 24 }}>
            Connectez-vous pour commencer votre trajet avec le service le plus fiable de Djibouti.
          </Text>
        </View>

        {/* Formulaire */}
        <View style={{ gap: 20 }}>
          <PhoneInput value={phone} onChangeText={setPhone} />
          <PasswordInput value={password} onChangeText={setPassword} />

          {/* Message d'erreur (affiché seulement s'il y en a un). */}
          {error && (
            <Text style={{ color: colors.statusError, fontSize: 14, marginLeft: 4 }}>
              {error}
            </Text>
          )}

          <PrimaryButton
            label="Se connecter"
            onPress={handleLogin}
            loading={loading}
            disabled={!canSubmit}
          />
        </View>

        {/* Mentions légales, poussées en bas. */}
        <View style={{ marginTop: 'auto', paddingTop: 32 }}>
          <Text
            style={{
              fontSize: 12,
              color: colors.onSurfaceVariant,
              textAlign: 'center',
              lineHeight: 18,
            }}
          >
            En continuant, vous acceptez nos Conditions d'utilisation et notre Politique de
            confidentialité.
          </Text>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}
