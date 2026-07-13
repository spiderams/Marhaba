using Ardalis.Specification;
using Taxi.Domain.Pricing;

namespace Taxi.Application.Pricing.Admin;

/// <summary>
/// Spécification : sélectionne tous les tarifs zonaux, triés par zone de départ puis d'arrivée.
/// </summary>
internal sealed class AllZonePricesSpec : Specification<ZonePrice>
{
    public AllZonePricesSpec() => Query.OrderBy(z => z.FromZone).ThenBy(z => z.ToZone);
}
