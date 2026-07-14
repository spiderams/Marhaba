namespace Taxi.Domain.Rides;

/// <summary>
/// Motif d'annulation d'une course, capturé au moment de l'annulation pour l'arbitrage des litiges.
/// <see cref="Other"/> s'accompagne généralement d'une précision en texte libre.
/// </summary>
public enum CancellationReason
{
    /// <summary>Changement d'avis : la personne n'a plus besoin de la course.</summary>
    ChangedMind,

    /// <summary>Attente trop longue avant la prise en charge.</summary>
    TooLongWait,

    /// <summary>Informations de prise en charge erronées (adresse ou point de rendez-vous incorrect).</summary>
    WrongPickupInfo,

    /// <summary>Client absent / injoignable au point de rendez-vous (no-show client).</summary>
    ClientNoShow,

    /// <summary>Chauffeur qui ne se présente pas (no-show chauffeur).</summary>
    DriverNoShow,

    /// <summary>Autre motif : à préciser dans la note en texte libre.</summary>
    Other
}

/// <summary>
/// Partie ayant initié l'annulation d'une course.
/// </summary>
public enum CancelledBy { Client, Driver }
