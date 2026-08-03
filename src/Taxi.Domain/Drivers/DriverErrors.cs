using Taxi.SharedKernel;

namespace Taxi.Domain.Drivers;

/// <summary>
/// Catalogue des erreurs métier liées aux chauffeurs (Driver) : regroupe les codes d'erreur
/// retournés lorsque les règles de gestion empêchent une transition de statut d'approbation.
/// </summary>
public static class DriverErrors
{
    public static readonly Error RejectionReasonRequired =
        Error.Validation(
            "Driver.RejectionReasonRequired",
            "Le motif du rejet est obligatoire.");

    public static readonly Error RejectionReasonTooLong =
        Error.Validation(
            "Driver.RejectionReasonTooLong",
            "Le motif du rejet ne peut pas dépasser 500 caractères.");

    public static readonly Error InvalidStatusTransition = Error.Conflict(
        "Driver.InvalidStatusTransition",
        "Transition de statut d'approbation invalide.");

    public static readonly Error MissingRequiredDocuments = Error.Validation(
       "Driver.MissingRequiredDocuments",
       "Le permis de conduire, la carte grise et la pièce d'identité sont obligatoires avant l'approbation.");
}
