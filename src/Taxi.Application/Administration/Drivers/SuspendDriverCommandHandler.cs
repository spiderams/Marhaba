using Taxi.Application.Abstractions;
using Taxi.Application.Drivers;
using Taxi.Domain.Drivers;
using Taxi.SharedKernel;
using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Administration.Drivers;

internal sealed class SuspendDriverCommandHandler(IRepository<Driver> drivers)
    : ICommandHandler<SuspendDriverCommand, DriverDto>
{
    public async Task<Result<DriverDto>> Handle(SuspendDriverCommand command, CancellationToken cancellationToken)
    {
        var driver = await drivers.FirstOrDefaultAsync(new DriverByIdSpec(command.DriverId), cancellationToken);
        if (driver is null)
            return Result.Failure<DriverDto>(Error.NotFound("Driver.NotFound", "Profil chauffeur introuvable."));

        var result = driver.Suspend();
        if (result.IsFailure)
            return Result.Failure<DriverDto>(result.Error);

        await drivers.UpdateAsync(driver, cancellationToken);
        return DriverDto.From(driver);
    }
}
