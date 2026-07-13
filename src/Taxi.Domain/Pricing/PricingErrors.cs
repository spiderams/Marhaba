using Taxi.SharedKernel;

namespace Taxi.Domain.Pricing;

/// <summary>
/// Catalogue des erreurs métier liées à la tarification par zones (ZonePrice) :
/// regroupe les codes retournés lors de la gestion administrative des tarifs.
/// </summary>
public static class PricingErrors
{
    public static readonly Error NotFound = Error.NotFound("ZonePrice.NotFound", "Tarif de zone introuvable.");
    public static readonly Error DuplicatePair = Error.Conflict(
        "ZonePrice.DuplicatePair", "Un tarif existe déjà pour cette paire de zones.");
}
