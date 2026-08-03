using FluentValidation;

namespace Taxi.Application.Rides.Request;

/// <summary>
/// Règles de validation de <see cref="RequestRideCommand"/> : les adresses, les
/// zones tarifaires et la position de prise en charge sont obligatoires.
/// </summary>
internal sealed class RequestRideCommandValidator : AbstractValidator<RequestRideCommand>
{
    public RequestRideCommandValidator()
    {
        RuleFor(c => c.PickupAddress).NotEmpty();
        RuleFor(c => c.DestinationAddress).NotEmpty();
        RuleFor(c => c.PickupZone).NotEmpty();
        RuleFor(c => c.DestinationZone).NotEmpty();
        RuleFor(c => c.PickupLatitude).NotNull().InclusiveBetween(-90, 90);
        RuleFor(c => c.PickupLongitude).NotNull().InclusiveBetween(-180, 180);
    }
}
