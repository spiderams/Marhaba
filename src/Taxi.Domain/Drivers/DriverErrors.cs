using Taxi.SharedKernel;

namespace Taxi.Domain.Drivers;

/// <summary>
/// Catalogue des erreurs métier liées aux chauffeurs (Driver) : regroupe les codes d'erreur
/// retournés lorsque les règles de gestion empêchent une transition de statut d'approbation.
/// </summary>
public static class DriverErrors
{
    public static readonly Error InvalidStatusTransition = Error.Conflict(
        "Driver.InvalidStatusTransition",
        "Transition de statut d'approbation invalide.");
}
