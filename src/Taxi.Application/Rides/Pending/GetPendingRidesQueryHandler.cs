using Taxi.Application.Abstractions;
using Taxi.Domain.Rides;
using Taxi.SharedKernel;
using Taxi.SharedKernel.Messaging;
using Taxi.Application.Drivers;
using Taxi.Domain.Drivers;
namespace Taxi.Application.Rides.Pending;

/// <summary>
/// Gère <see cref="GetPendingRidesQuery"/> : retourne les demandes en attente et
/// les offres de la vague courante qui sont réellement destinées au chauffeur.
/// </summary>
internal sealed class GetPendingRidesQueryHandler(
    IRepository<Ride> rides,
    IRepository<Driver> drivers)
    : IQueryHandler<GetPendingRidesQuery, IReadOnlyList<RideDto>>
{
    public async Task<Result<IReadOnlyList<RideDto>>> Handle(GetPendingRidesQuery query, CancellationToken cancellationToken)
    {
        var driver = await drivers.FirstOrDefaultAsync(
         new DriverByUserIdSpec(query.DriverUserId), cancellationToken);
        if (driver is null)
            return Array.Empty<RideDto>();

        var list = await rides.ListAsync(new PendingOrOfferedRidesSpec(), cancellationToken);
        return list
            .Where(ride => ride.Status == RideStatus.Pending
                || ride.OfferedDriverIds.Contains(driver.Id))
            .Select(RideDto.From)
            .ToList();
    }
}

