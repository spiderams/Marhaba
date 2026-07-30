import { Pressable, Text, View } from 'react-native';
import { colors } from '@/theme/colors';

/**
 * Badge de disponibilité du chauffeur : « EN LIGNE » (vert) ou « HORS LIGNE » (rouge).
 *
 * C'est un composant "contrôlé" : il ne gère PAS lui-même son état.
 * Le parent lui dit s'il est en ligne (prop `online`) et lui fournit la fonction
 * à appeler quand on tape dessus (prop `onToggle`). C'est le parent qui décide
 * quoi faire (plus tard : appeler l'API set-availability du backend).
 */
type Props = Readonly<{
  online: boolean;
  onToggle: () => void;
}>;

// NOTE STYLE : ce composant garde `style={{}}` volontairement. Les couleurs sont
// calculées à l'exécution (vert/rouge selon l'état + opacité hex `${color}1A`),
// ce que les classes NativeWind statiques ne savent pas exprimer. Règle du projet :
// NativeWind (className) pour le style statique, `style` pour le dynamique calculé.
export function OnlineBadge({ online, onToggle }: Props) {
  // Selon l'état, on choisit la couleur et le texte.
  const color = online ? colors.statusSuccess : colors.statusError;
  const label = online ? 'EN LIGNE' : 'HORS LIGNE';

  return (
    <Pressable
      onPress={onToggle}
      // La "pilule" : fond teinté, bordure de la même couleur, coins arrondis.
      style={{
        flexDirection: 'row',
        alignItems: 'center',
        backgroundColor: `${color}1A`, // 1A = ~10% d'opacité en hexadécimal
        borderColor: `${color}33`, // 33 = ~20% d'opacité
        borderWidth: 1,
        paddingHorizontal: 16,
        paddingVertical: 6,
        borderRadius: 9999, // totalement arrondi (pilule)
      }}
    >
      {/* Le petit point coloré à gauche du texte. */}
      <View
        style={{
          width: 10,
          height: 10,
          borderRadius: 9999,
          backgroundColor: color,
          marginRight: 8,
        }}
      />
      <Text style={{ color, fontWeight: '700', fontSize: 14, letterSpacing: 0.5 }}>
        {label}
      </Text>
    </Pressable>
  );
}
