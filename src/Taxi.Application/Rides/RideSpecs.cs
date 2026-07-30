using Ardalis.Specification;
using Taxi.Domain.Rides;

namespace Taxi.Application.Rides;

/// <summary>
/// Spécification : sélectionne une course par son identifiant unique.
/// </summary>
internal sealed class RideByIdSpec : Specification<Ride>
{
    public RideByIdSpec(int rideId) => Query.Where(r => r.Id == rideId);
}

/// <summary>
/// Spécification : sélectionne toutes les courses d'un client, triées de la plus récente à la plus ancienne.
/// </summary>
internal sealed class RidesByClientSpec : Specification<Ride>
{
    public RidesByClientSpec(string clientId)
        => Query.Where(r => r.ClientId == clientId).OrderByDescending(r => r.CreatedAt);
}

/// <summary>
/// Spécification : sélectionne toutes les courses assignées à un chauffeur, triées de la plus récente à la plus ancienne.
/// </summary>
internal sealed class RidesByDriverSpec : Specification<Ride>
{
    public RidesByDriverSpec(int driverId)
        => Query.Where(r => r.DriverId == driverId).OrderByDescending(r => r.CreatedAt);
}

/// <summary>
/// Spécification : sélectionne l'historique chauffeur, limité aux courses terminées
/// qui portent un tarif final réel, triées de la plus récente à la plus ancienne.
/// </summary>
internal sealed class CompletedRideHistoryByDriverSpec : Specification<Ride>
{
    public CompletedRideHistoryByDriverSpec(int driverId)
        => Query
            .Where(r => r.DriverId == driverId && r.Status == RideStatus.Completed && r.FinalPrice != null)
            .OrderByDescending(r => r.CompletedAt);
}

/// <summary>
/// Spécification : sélectionne toutes les courses en attente de chauffeur (statut <c>Pending</c>), triées de la plus récente à la plus ancienne.
/// </summary>
internal sealed class PendingRidesSpec : Specification<Ride>
{
    public PendingRidesSpec()
        => Query.Where(r => r.Status == RideStatus.Pending).OrderByDescending(r => r.CreatedAt);
}

/// <summary>
/// Spécification : sélectionne les courses en offre dont le délai d'acceptation est expiré à l'instant <c>now</c>.
/// </summary>
public sealed class ExpiredOffersSpec : Specification<Ride>
{
    public ExpiredOffersSpec(DateTime now)
        => Query.Where(r => r.Status == RideStatus.Offered && r.OfferExpiresAt != null && r.OfferExpiresAt <= now);
}
/// <summary>
/// Spécification : recherche une course non terminale pour empêcher un client
/// de créer plusieurs réservations simultanées.
/// </summary>
internal sealed class ActiveRideByClientSpec : Specification<Ride>
{
    public ActiveRideByClientSpec(string clientId)
        => Query.Where(r =>
            r.ClientId == clientId &&
            r.Status != RideStatus.Completed &&
            r.Status != RideStatus.Cancelled &&
            r.Status != RideStatus.NoDriverFound);
}
/// <summary>
/// Spécification : sélectionne les courses terminées (statut <c>Completed</c>), qui portent un tarif final figé.
/// Base des indicateurs de chiffre d'affaires.
/// </summary>
public sealed class CompletedRidesSpec : Specification<Ride>
{
    public CompletedRidesSpec() => Query.Where(r => r.Status == RideStatus.Completed);
}
