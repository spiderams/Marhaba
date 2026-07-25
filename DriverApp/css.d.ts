// Déclarations de types pour les imports CSS (NativeWind / global.css).
// Permet à TypeScript d'accepter `import '@/global.css'` et les *.module.css.
declare module '*.css';
declare module '*.module.css' {
  const classes: { readonly [key: string]: string };
  export default classes;
}
