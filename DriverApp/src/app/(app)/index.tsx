import { DashboardScreen } from '@/features/dashboard/DashboardScreen';

/**
 * Route / — tableau de bord (zone authentifiée).
 *
 * Coquille de route : l'écran réel vit dans `features/dashboard`. La garde
 * d'authentification se trouve dans le _layout racine.
 */
export default function DashboardRoute() {
  return <DashboardScreen />;
}
