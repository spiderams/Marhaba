using Taxi.Application.Rides;
using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Rides.Transitions;

/// <summary>
/// Commande permettant au chauffeur de marquer une course comme terminée, en figeant le montant
/// réellement dû, et de se remettre disponible.
/// </summary>
public sealed record CompleteRideCommand(int RideId, string DriverUserId, decimal FinalPrice) : ICommand<RideDto>;
