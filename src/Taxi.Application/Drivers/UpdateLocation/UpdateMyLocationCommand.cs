using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Drivers.UpdateLocation;

/// <summary>
/// Battement de position (« heartbeat ») envoyé périodiquement par le chauffeur en ligne mais sans course active :
/// il rafraîchit sa dernière position connue afin de rester éligible au dispatch de proximité.
/// Contrairement à la mise à jour pendant une course, il n'est lié à aucune course et ne diffuse rien au client.
/// </summary>
public sealed record UpdateMyLocationCommand(
    string UserId,
    double Latitude,
    double Longitude) : ICommand<DriverDto>;
