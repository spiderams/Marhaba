namespace Taxi.Domain.Rides;

/// <summary>
/// Mode de paiement figé à la complétion d'une course. Au lancement, seul <see cref="Cash"/> est utilisé
/// (le marché djiboutien est majoritairement en espèces) ; <see cref="DMoney"/> est prévu pour l'intégration
/// future du paiement électronique (PSP), sans nécessiter de changement de modèle.
/// </summary>
public enum PaymentMethod { Cash, DMoney }
