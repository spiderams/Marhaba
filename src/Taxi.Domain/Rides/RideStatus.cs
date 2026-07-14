namespace Taxi.Domain.Rides;

/// <summary>
/// Cycle de vie d'une course : chaque valeur représente un état stable par lequel la course transite
/// depuis la demande initiale du client jusqu'à sa clôture. Les états terminaux sont
/// <see cref="Completed"/>, <see cref="Cancelled"/> et <see cref="NoDriverFound"/> (aucun chauffeur
/// disponible après épuisement des vagues de dispatch).
/// </summary>
public enum RideStatus { Pending, Offered, Accepted, DriverArrived, InProgress, Completed, Cancelled, NoDriverFound }
