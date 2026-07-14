namespace Taxi.Application.Realtime;

/// <summary>
/// Fournit le jeton d'appareil (FCM) d'un utilisateur à partir de son identifiant, afin de lui envoyer
/// une notification push. Abstrait l'accès au référentiel d'identité (ASP.NET Core Identity) sans exposer
/// EF Core à la couche Application. Implémentée en Infrastructure.
/// </summary>
public interface IDeviceTokenReader
{
    /// <summary>
    /// Retourne le jeton d'appareil de l'utilisateur <paramref name="userId"/>, ou <c>null</c>
    /// si l'utilisateur n'a enregistré aucun appareil.
    /// </summary>
    Task<string?> GetDeviceTokenAsync(string userId, CancellationToken cancellationToken);
}
