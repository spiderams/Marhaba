using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Rides.Request;

/// <summary>
/// Commande de création d'une course : porte les adresses, les zones tarifaires,
/// la position GPS obligatoire de prise en charge et la destination éventuellement géolocalisée
/// </summary>
public sealed record RequestRideCommand(
    string ClientId,
    string PickupAddress, string DestinationAddress,
    string PickupZone, string DestinationZone,
    double? PickupLatitude, double? PickupLongitude,
    double? DestinationLatitude, double? DestinationLongitude)
    : ICommand<RideDto>;
