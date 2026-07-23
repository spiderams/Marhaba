namespace Taxi.Application.Dashboard;

/// <summary>
/// Indicateurs du tableau de bord chauffeur : les gains sont calculés uniquement sur les courses terminées,
/// à partir du tarif final réellement encaissé (<c>Ride.FinalPrice</c>), jamais depuis le prix estimé.
/// </summary>
public sealed record DriverDashboardDto(
    int DriverId,
    int CompletedRides,
    decimal TotalEarnings);
