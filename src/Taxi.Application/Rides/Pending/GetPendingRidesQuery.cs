using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Rides.Pending;

/// <summary>
/// Requête qui retourne les courses encore disponibles ainsi que les offres
/// actuellement adressées au chauffeur connecté.
/// </summary>
public sealed record GetPendingRidesQuery(string DriverUserId) : IQuery<IReadOnlyList<RideDto>>;
