/**
 * Couleurs du design TaxiDjibouti (extraites de DESIGN.md).
 *
 * On les centralise ici pour les réutiliser partout, plutôt que d'écrire
 * des codes hexadécimaux un peu partout dans le code. Si une couleur change,
 * on ne la modifie qu'à UN seul endroit.
 */
export const colors = {
  /** Bleu Djibouti profond — navigation, boutons principaux, marque. */
  primary: '#001e40',
  primaryContainer: '#003366',
  onPrimaryContainer: '#799dd6',

  /** Jaune Taxi — actions importantes (Accepter une course, Passer en ligne). */
  secondaryContainer: '#fecb00',
  onSecondaryContainer: '#6e5700',

  /** Surfaces (fonds). */
  surface: '#f9f9fe',
  surfaceContainer: '#eeedf2',
  surfaceContainerHigh: '#e8e8ed',
  white: '#ffffff',

  /** Textes. */
  onSurface: '#1a1c1f',
  onSurfaceVariant: '#43474f',

  /** Bordures. */
  outlineVariant: '#c3c6d1',

  /** États (statuts). */
  statusSuccess: '#00875A',
  statusError: '#D32F2F',
} as const;
