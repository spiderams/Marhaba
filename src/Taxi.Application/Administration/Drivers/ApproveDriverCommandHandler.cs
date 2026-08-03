using Taxi.Application.Abstractions;
using Taxi.Application.Drivers;
using Taxi.Domain.Drivers;
using Taxi.SharedKernel;
using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Administration.Drivers;

internal sealed class ApproveDriverCommandHandler(IRepository<Driver> drivers)
    : ICommandHandler<ApproveDriverCommand, DriverDto>
{
    public async Task<Result<DriverDto>> Handle(ApproveDriverCommand command, CancellationToken cancellationToken)
    {
        var driver = await drivers.FirstOrDefaultAsync(new DriverByIdSpec(command.DriverId), cancellationToken);
        if (driver is null)
            return Result.Failure<DriverDto>(Error.NotFound("Driver.NotFound", "Profil chauffeur introuvable."));

        if (driver.Status == DriverStatus.PendingApproval &&
           Enum.GetValues<DriverDocumentType>().Any(type => driver.GetDocumentKey(type) is null))
            return Result.Failure<DriverDto>(DriverErrors.MissingRequiredDocuments);
        var result = driver.Approve();
        if (result.IsFailure)
            return Result.Failure<DriverDto>(result.Error);

        await drivers.UpdateAsync(driver, cancellationToken);
        return DriverDto.From(driver);
    }
}