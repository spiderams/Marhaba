using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Drivers.SetAvailability;

/// <summary>
/// Commande permettant à un chauffeur de basculer son statut de disponibilité (disponible / hors-ligne).
/// Lorsqu'il se met disponible, ses coordonnées GPS courantes sont obligatoires afin d'alimenter
/// immédiatement le dispatch de proximité ; elles sont ignorées lorsqu'il se met hors-ligne.
/// </summary>
public sealed record SetAvailabilityCommand(
    string UserId,
    bool IsAvailable,
    double? Latitude = null,
    double? Longitude = null) : ICommand<DriverDto>;
