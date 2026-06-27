using FluentValidation;

namespace Taxi.Application.Drivers.UpdateLocation;

/// <summary>
/// Règles de validation de <see cref="UpdateMyLocationCommand"/> :
/// les coordonnées GPS doivent être dans les plages WGS-84 valides.
/// </summary>
internal sealed class UpdateMyLocationCommandValidator : AbstractValidator<UpdateMyLocationCommand>
{
    public UpdateMyLocationCommandValidator()
    {
        RuleFor(c => c.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("La latitude doit être comprise entre -90 et 90.");

        RuleFor(c => c.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("La longitude doit être comprise entre -180 et 180.");
    }
}
