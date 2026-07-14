using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;

namespace Taxi.Infrastructure.Push;

/// <summary>
/// Fournit le jeton d'accès OAuth2 pour l'API FCM à partir d'un compte de service Google.
/// Délègue au SDK <c>Google.Apis.Auth</c> la signature du JWT, l'échange contre un access token
/// et son rafraîchissement automatique (le SDK met le jeton en cache jusqu'à son expiration).
/// </summary>
internal sealed class GoogleFcmTokenProvider : IFcmTokenProvider
{
    // Portée OAuth2 requise pour envoyer des messages via FCM HTTP v1.
    private const string MessagingScope = "https://www.googleapis.com/auth/firebase.messaging";

    private readonly ITokenAccess _credential;

    public GoogleFcmTokenProvider(IOptions<FcmSettings> settings)
    {
        var path = settings.Value.CredentialsPath;
        if (string.IsNullOrWhiteSpace(path))
            throw new InvalidOperationException("Fcm:CredentialsPath n'est pas configuré.");

        _credential = GoogleCredential
            .FromFile(path)
            .CreateScoped(MessagingScope);
    }

    public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
        => _credential.GetAccessTokenForRequestAsync(cancellationToken: cancellationToken);
}
