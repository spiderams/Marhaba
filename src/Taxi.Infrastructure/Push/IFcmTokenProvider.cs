namespace Taxi.Infrastructure.Push;

/// <summary>
/// Fournit le jeton d'accès OAuth2 (Bearer) requis pour authentifier les appels à l'API FCM HTTP v1.
/// Isolé derrière une abstraction car son obtention dépend d'un compte de service Google (à câbler
/// séparément), ce qui permet de développer et tester <see cref="FcmPushNotifier"/> sans ces credentials.
/// </summary>
internal interface IFcmTokenProvider
{
    /// <summary>Retourne un jeton d'accès valide pour l'API FCM (rafraîchi si nécessaire).</summary>
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Implémentation par défaut, non encore raccordée à un compte de service Google : elle signale
/// explicitement que le fournisseur de jeton FCM n'est pas configuré. À remplacer par l'obtention
/// réelle du jeton OAuth2 (JWT signé du compte de service échangé contre un access token) lorsque
/// les credentials seront disponibles.
/// </summary>
internal sealed class NotConfiguredFcmTokenProvider : IFcmTokenProvider
{
    public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
        => throw new InvalidOperationException(
            "Le fournisseur de jeton FCM n'est pas configuré (compte de service Google manquant).");
}
