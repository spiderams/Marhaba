/**
 * SOURCE UNIQUE des couleurs TaxiDjibouti (extraites de DESIGN.md).
 *
 * Pourquoi un fichier .js neutre (et pas directement .ts) ?
 *   - `tailwind.config.js` (CommonJS) doit pouvoir lire ces couleurs pour
 *     générer les classes NativeWind (bg-primary, text-on-surface, ...).
 *   - Le code TypeScript (`colors.ts`) les réexporte pour le style inline et
 *     les props de couleur (icônes, etc.).
 * Un seul fichier alimente donc les DEUX mondes → aucune divergence possible.
 *
 * Règle d'or : on ne met JAMAIS un code hexadécimal ailleurs que dans ce fichier.
 */
const palette = {
  // Bleu Djibouti profond — navigation, boutons principaux, marque.
  primary: '#001e40',
  primaryContainer: '#003366',
  onPrimaryContainer: '#799dd6',

  // Jaune Taxi — actions importantes (Accepter une course, Passer en ligne).
  secondaryContainer: '#fecb00',
  onSecondaryContainer: '#6e5700',

  // Surfaces (fonds).
  surface: '#f9f9fe',
  surfaceContainer: '#eeedf2',
  surfaceContainerHigh: '#e8e8ed',
  white: '#ffffff',

  // Textes.
  onSurface: '#1a1c1f',
  onSurfaceVariant: '#43474f',

  // Bordures.
  outlineVariant: '#c3c6d1',

  // États (statuts).
  statusSuccess: '#00875A',
  statusError: '#D32F2F',
};

module.exports = palette;
