using Refit;

namespace Taxi.Infrastructure.Push;

/// <summary>
/// Contrat Refit de l'API FCM HTTP v1 : décrit de façon déclarative l'appel d'envoi de message.
/// Le transport (sérialisation, HttpClient) est généré par Refit ; l'adresse de base et l'authentification
/// sont fournies à la configuration du client.
/// </summary>
internal interface IFcmApi
{
    /// <summary>
    /// Envoie un message à l'API FCM pour le projet <paramref name="projectId"/>.
    /// Le jeton OAuth2 est passé en en-tête Authorization (valeur complète "Bearer &lt;token&gt;").
    /// </summary>
    [Post("/v1/projects/{projectId}/messages:send")]
    Task SendAsync(
        string projectId,
        [Body] FcmRequest request,
        [Header("Authorization")] string authorization,
        CancellationToken cancellationToken);
}
