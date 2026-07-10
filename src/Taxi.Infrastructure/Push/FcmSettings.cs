namespace Taxi.Infrastructure.Push;

/// <summary>
/// Paramètres de l'intégration Firebase Cloud Messaging (FCM HTTP v1) :
/// identifiant du projet Firebase et adresse de base de l'API d'envoi.
/// </summary>
internal sealed class FcmSettings
{
    public const string SectionName = "Fcm";

    /// <summary>Identifiant du projet Firebase (utilisé dans l'URL <c>/v1/projects/{ProjectId}/messages:send</c>).</summary>
    public string ProjectId { get; init; } = string.Empty;

    /// <summary>Adresse de base de l'API FCM. Surchargée en test pour cibler un serveur factice.</summary>
    public string BaseUrl { get; init; } = "https://fcm.googleapis.com";
}
