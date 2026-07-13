using Taxi.Application.Rides;
using Taxi.Domain.Rides;
using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Rides.Cancel;

/// <summary>
/// Commande d'annulation d'une course, utilisable aussi bien par le client que par le chauffeur assigné.
/// Capture le motif d'annulation et une précision facultative pour l'arbitrage des litiges.
/// </summary>
public sealed record CancelRideCommand(
    int RideId, string UserId, bool IsDriver, CancellationReason Reason, string? Note = null) : ICommand<RideDto>;
