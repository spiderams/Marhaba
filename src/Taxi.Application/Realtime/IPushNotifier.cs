namespace Taxi.Application.Realtime;

/// <summary>
/// Notifications push (FCM) émises par la couche Application sans en connaître l'implémentation :
/// permettent de réveiller un chauffeur dont l'application est fermée ou en arrière-plan afin qu'il
/// ne rate pas une offre de course. Complément hors-app de <see cref="IRealtimeNotifier"/> (SignalR,
/// qui exige une connexion active). Implémentée en Infrastructure (client FCM).
/// </summary>
public interface IPushNotifier
{
    /// <summary>
    /// Envoie une notification push d'offre de course à l'appareil identifié par <paramref name="deviceToken"/>.
    /// Le ciblage se fait par jeton d'appareil (et non par identifiant utilisateur) car c'est l'unité d'adressage
    /// de FCM ; la résolution de l'utilisateur vers son jeton relève de l'appelant.
    /// </summary>
    Task SendOfferAsync(string deviceToken, int rideId, DateTime expiresAt, CancellationToken cancellationToken);
}
