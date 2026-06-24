/** @type {import('tailwindcss').Config} */
module.exports = {
  // Fichiers scannés par Tailwind pour générer les classes utilisées.
  content: ['./src/**/*.{js,jsx,ts,tsx}'],
  presets: [require('nativewind/preset')],
  theme: {
    extend: {
      colors: {
        // Palette TaxiDjibouti (à affiner lors du design des écrans).
        brand: {
          DEFAULT: '#0B6E4F', // vert principal
          dark: '#08503A',
          light: '#13A06F',
        },
        accent: '#F4A300', // ambre (compte à rebours, alertes)
      },
    },
  },
  plugins: [],
};
