using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Taxi.Application.Realtime;

namespace Taxi.Infrastructure.Push;

/// <summary>
/// Implémentation FCM (HTTP v1, via Refit) de <see cref="IPushNotifier"/> : réveille un chauffeur
/// app fermée en lui envoyant une notification d'offre de course. L'envoi est best-effort — tout échec
/// (jeton absent, FCM indisponible, jeton d'appareil invalide) est journalisé mais ne remonte jamais,
/// afin de ne pas interrompre le flux de dispatch.
/// </summary>
internal sealed partial class FcmPushNotifier(
    IFcmApi api,
    IFcmTokenProvider tokenProvider,
    IOptions<FcmSettings> settings,
    ILogger<FcmPushNotifier> logger) : IPushNotifier
{
    private readonly FcmSettings _settings = settings.Value;

    public async Task SendOfferAsync(string deviceToken, int rideId, DateTime expiresAt, CancellationToken cancellationToken)
    {
        try
        {
            var accessToken = await tokenProvider.GetAccessTokenAsync(cancellationToken);

            var request = new FcmRequest(new FcmMessage(
                Token: deviceToken,
                Notification: new FcmNotification(
                    Title: "Nouvelle course",
                    Body: "Une course vous est proposée. Ouvrez l'application pour accepter."),
                Data: new Dictionary<string, string>
                {
                    ["rideId"] = rideId.ToString(),
                    ["expiresAt"] = expiresAt.ToString("o"),
                }));

            await api.SendAsync(_settings.ProjectId, request, $"Bearer {accessToken}", cancellationToken);
        }
        catch (Exception ex)
        {
            LogPushFailed(logger, ex, rideId);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Échec de l'envoi push FCM pour la course {RideId} (best-effort)")]
    private static partial void LogPushFailed(ILogger logger, Exception ex, int rideId);
}
