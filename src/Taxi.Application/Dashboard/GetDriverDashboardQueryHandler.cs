using Ardalis.Specification;
using Taxi.Application.Abstractions;
using Taxi.Application.Drivers;
using Taxi.Domain.Drivers;
using Taxi.Domain.Rides;
using Taxi.SharedKernel;
using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Dashboard;

/// <summary>
/// Gère le tableau de bord chauffeur : retrouve le profil chauffeur puis calcule les gains réels
/// à partir des courses terminées et de leur <see cref="Ride.FinalPrice"/>.
/// </summary>
internal sealed class GetDriverDashboardQueryHandler(
    IRepository<Driver> drivers,
    IRepository<Ride> rides)
    : IQueryHandler<GetDriverDashboardQuery, DriverDashboardDto>
{
    public async Task<Result<DriverDashboardDto>> Handle(GetDriverDashboardQuery query, CancellationToken cancellationToken)
    {
        var driver = await drivers.FirstOrDefaultAsync(new DriverByUserIdSpec(query.UserId), cancellationToken);
        if (driver is null)
            return Result.Failure<DriverDashboardDto>(Error.NotFound("Driver.NotFound", "Profil chauffeur introuvable."));

        var completedRides = await rides.ListAsync(new CompletedRidesByDriverSpec(driver.Id), cancellationToken);
        var totalEarnings = completedRides.Sum(ride => ride.FinalPrice ?? 0m);

        return new DriverDashboardDto(driver.Id, completedRides.Count, totalEarnings);
    }
}

/// <summary>
/// Spécification : sélectionne les courses terminées d'un chauffeur, avec un tarif final figé.
/// </summary>
internal sealed class CompletedRidesByDriverSpec : Specification<Ride>
{
    public CompletedRidesByDriverSpec(int driverId)
        => Query.Where(ride => ride.DriverId == driverId && ride.Status == RideStatus.Completed && ride.FinalPrice != null);
}