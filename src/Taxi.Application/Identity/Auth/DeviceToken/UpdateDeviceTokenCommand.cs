using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Identity.Auth.DeviceToken;

/// <summary>
/// Commande d'enregistrement du jeton d'appareil FCM de l'utilisateur authentifié.
/// </summary>
public sealed record UpdateDeviceTokenCommand(string UserId, string DeviceToken) : ICommand<bool>;
