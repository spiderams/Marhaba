using Taxi.Application.Abstractions;
using Taxi.Domain.Drivers;
using Taxi.SharedKernel;
using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Drivers.UpdateLocation;

/// <summary>
/// Gère <see cref="UpdateMyLocationCommand"/> : met à jour la dernière position connue du chauffeur courant,
/// sans dépendance à une course. Retourne le profil actualisé.
/// </summary>
internal sealed class UpdateMyLocationCommandHandler(IRepository<Driver> drivers)
    : ICommandHandler<UpdateMyLocationCommand, DriverDto>
{
    public async Task<Result<DriverDto>> Handle(UpdateMyLocationCommand command, CancellationToken cancellationToken)
    {
        var driver = await drivers.FirstOrDefaultAsync(new DriverByUserIdSpec(command.UserId), cancellationToken);
        if (driver is null)
            return Result.Failure<DriverDto>(Error.NotFound("Driver.NotFound", "Profil chauffeur introuvable."));

        driver.UpdateLocation(command.Latitude, command.Longitude);
        await drivers.UpdateAsync(driver, cancellationToken);
        return DriverDto.From(driver);
    }
}
