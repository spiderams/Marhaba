/**
 * Configuration d'environnement de l'app chauffeur.
 *
 * En dev local, l'API tourne sous Aspire (cf. backend). Adapter l'URL selon
 * la plateforme : un émulateur Android n'atteint PAS localhost de la machine
 * hôte directement (utiliser 10.0.2.2), et un téléphone physique doit viser
 * l'IP LAN de la machine de dev.
 */

/** URL de base de l'API REST (sans slash final). */
export const API_BASE_URL = 'http://10.0.2.2:5000'; // émulateur Android → hôte

/** URL du hub SignalR temps réel. */
export const RIDE_HUB_URL = `${API_BASE_URL}/hubs/ride`;
