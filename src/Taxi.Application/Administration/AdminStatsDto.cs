namespace Taxi.Application.Administration;

/// <summary>
/// Projection des indicateurs clés de l'administration : nombre total d'utilisateurs, de chauffeurs,
/// de courses et de signalements, ainsi que le volume de courses terminées et le chiffre d'affaires
/// réel (somme des tarifs finaux figés à la complétion).
/// </summary>
public sealed record AdminStatsDto(
    int Users, int Drivers, int Rides, int Reports,
    int CompletedRides, decimal TotalRevenue);
