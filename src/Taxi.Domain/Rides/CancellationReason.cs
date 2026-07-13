namespace Taxi.Domain.Rides;

/// <summary>
/// Motif d'annulation d'une course, capturé au moment de l'annulation pour l'arbitrage des litiges.
/// <see cref="Other"/> s'accompagne généralement d'une précision en texte libre.
/// </summary>
public enum CancellationReason
{
    ChangedMind,
    TooLongWait,
    WrongPickupInfo,
    ClientNoShow,
    DriverNoShow,
    Other
}

/// <summary>
/// Partie ayant initié l'annulation d'une course.
/// </summary>
public enum CancelledBy { Client, Driver }
