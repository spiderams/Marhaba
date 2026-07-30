/**
 * Couleurs du design TaxiDjibouti, pour le code TypeScript.
 *
 * Les VALEURS vivent dans `palette.js` (source unique partagée avec
 * tailwind.config.js). Ici on ne fait que les réexporter en typé `as const`,
 * pour que l'autocomplétion fonctionne (`colors.primary`, `colors.surface`, ...)
 * et que TypeScript vérifie qu'on n'utilise pas une couleur qui n'existe pas.
 *
 * Usage : pour les props de couleur (icônes MaterialIcons, StatusBar...) et les
 * rares styles inline. Pour le style des vues, préférer les classes NativeWind
 * (bg-primary, text-on-surface...) générées depuis la même palette.
 */
import palette from './palette';

export const colors = palette as {
  readonly primary: string;
  readonly primaryContainer: string;
  readonly onPrimaryContainer: string;
  readonly secondaryContainer: string;
  readonly onSecondaryContainer: string;
  readonly surface: string;
  readonly surfaceContainer: string;
  readonly surfaceContainerHigh: string;
  readonly white: string;
  readonly onSurface: string;
  readonly onSurfaceVariant: string;
  readonly outlineVariant: string;
  readonly statusSuccess: string;
  readonly statusError: string;
};
