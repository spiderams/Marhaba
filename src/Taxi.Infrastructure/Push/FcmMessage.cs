using System.Text.Json.Serialization;

namespace Taxi.Infrastructure.Push;

/// <summary>
/// Enveloppe racine d'une requête FCM HTTP v1 : <c>{ "message": { ... } }</c>.
/// </summary>
internal sealed record FcmRequest(
    [property: JsonPropertyName("message")] FcmMessage Message);

/// <summary>
/// Message FCM v1 ciblant un appareil : jeton de destination, notification affichable
/// (titre/corps) et données applicatives libres (clés/valeurs, ici l'identifiant de course).
/// </summary>
internal sealed record FcmMessage(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("notification")] FcmNotification Notification,
    [property: JsonPropertyName("data")] IReadOnlyDictionary<string, string> Data);

/// <summary>
/// Partie affichable d'une notification FCM (bannière système).
/// </summary>
internal sealed record FcmNotification(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("body")] string Body);
