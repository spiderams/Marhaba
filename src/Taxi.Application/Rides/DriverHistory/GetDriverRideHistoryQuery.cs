using Taxi.Application.Rides;
using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Rides.DriverHistory;

/// <summary>
/// Requête d'historique chauffeur : retourne les courses passées du chauffeur authentifié,
/// filtrées sur les courses terminées avec un tarif final réel.
/// </summary>
public sealed record GetDriverRideHistoryQuery(string DriverUserId) : IQuery<IReadOnlyList<RideDto>>;