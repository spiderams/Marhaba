/** @type {import('tailwindcss').Config} */

// Couleurs partagées avec le code TS (src/theme/colors.ts) — source unique.
const palette = require('./src/theme/palette');

module.exports = {
  // Fichiers scannés par Tailwind pour générer les classes utilisées.
  content: ['./src/**/*.{js,jsx,ts,tsx}'],
  presets: [require('nativewind/preset')],
  theme: {
    extend: {
      // On expose la palette TaxiDjibouti sous des noms de classes sémantiques :
      //   bg-primary, text-on-surface, border-outline-variant, etc.
      // Les noms suivent la convention Material (rôle, pas couleur brute), donc
      // si la teinte change, le NOM de la classe reste juste.
      colors: {
        primary: palette.primary,
        'primary-container': palette.primaryContainer,
        'on-primary-container': palette.onPrimaryContainer,
        'secondary-container': palette.secondaryContainer,
        'on-secondary-container': palette.onSecondaryContainer,
        surface: palette.surface,
        'surface-container': palette.surfaceContainer,
        'surface-container-high': palette.surfaceContainerHigh,
        white: palette.white,
        'on-surface': palette.onSurface,
        'on-surface-variant': palette.onSurfaceVariant,
        'outline-variant': palette.outlineVariant,
        'status-success': palette.statusSuccess,
        'status-error': palette.statusError,
      },
    },
  },
  plugins: [],
};
