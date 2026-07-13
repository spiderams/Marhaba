using Microsoft.EntityFrameworkCore;
using Taxi.Application.Abstractions;
using Taxi.Application.Rides;
using Taxi.Domain.Drivers;
using Taxi.Domain.Rides;
using Taxi.SharedKernel;
using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Administration.Stats;

/// <summary>
/// Gère <see cref="GetAdminStatsQuery"/> : agrège les compteurs d'utilisateurs, de chauffeurs, de courses
/// et de signalements, ainsi que le volume de courses terminées et le chiffre d'affaires réel
/// (somme des tarifs finaux figés), pour retourner un snapshot <see cref="AdminStatsDto"/>.
/// </summary>
internal sealed class GetAdminStatsQueryHandler(
    IUserDirectory users,
    IRepository<Driver> drivers,
    IRepository<Ride> rides,
    IRepository<Report> reports)
    : IQueryHandler<GetAdminStatsQuery, AdminStatsDto>
{
    public async Task<Result<AdminStatsDto>> Handle(GetAdminStatsQuery query, CancellationToken cancellationToken)
    {
        var userCount = await users.CountAsync(cancellationToken);
        var driverCount = await drivers.CountAsync(cancellationToken);
        var rideCount = await rides.CountAsync(cancellationToken);
        var reportCount = await reports.CountAsync(cancellationToken);

        // Chiffre d'affaires réel : agrégation exécutée en base sur les seules courses terminées,
        // sans matérialiser les entités (IQueryable non suivi).
        var completedRides = rides.Query(new CompletedRidesSpec());
        var completedCount = await completedRides.CountAsync(cancellationToken);
        var totalRevenue = await completedRides.SumAsync(r => r.FinalPrice ?? 0m, cancellationToken);

        return new AdminStatsDto(
            userCount, driverCount, rideCount, reportCount, completedCount, totalRevenue);
    }
}
