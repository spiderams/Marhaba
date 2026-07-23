using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Dashboard;

/// <summary>
/// Requête du tableau de bord chauffeur pour l'utilisateur authentifié.
/// </summary>
public sealed record GetDriverDashboardQuery(string UserId) : IQuery<DriverDashboardDto>;