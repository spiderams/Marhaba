using Taxi.Application.Rides;
using Taxi.Application.Abstractions;
using Taxi.Application.Drivers;
using Taxi.Domain.Drivers;
using Taxi.Domain.Rides;
using Taxi.SharedKernel;
using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Rides.DriverHistory;

/// <summary>
/// Gère l'historique chauffeur : résout le profil chauffeur de l'utilisateur authentifié,
/// puis retourne uniquement ses courses terminées qui portent un tarif final réellement encaissé.
/// </summary>
internal sealed class GetDriverRideHistoryQueryHandler(
    IRepository<Driver> drivers,
    IRepository<Ride> rides)
    : IQueryHandler<GetDriverRideHistoryQuery, IReadOnlyList<RideDto>>
{
    public async Task<Result<IReadOnlyList<RideDto>>> Handle(
        GetDriverRideHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var driver = await drivers.FirstOrDefaultAsync(new DriverByUserIdSpec(query.DriverUserId), cancellationToken);
        if (driver is null)
            return Array.Empty<RideDto>();

        var history = await rides.ListAsync(new CompletedRideHistoryByDriverSpec(driver.Id), cancellationToken);
        return history.Select(RideDto.From).ToList();
    }
}
