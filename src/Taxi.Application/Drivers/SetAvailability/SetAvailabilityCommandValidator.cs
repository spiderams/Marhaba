using FluentValidation;

namespace Taxi.Application.Drivers.SetAvailability;

/// <summary>
/// Règles de validation de <see cref="SetAvailabilityCommand"/> : lorsqu'un chauffeur se met disponible,
/// ses coordonnées GPS deviennent obligatoires et doivent être dans les plages WGS-84 valides.
/// </summary>
internal sealed class SetAvailabilityCommandValidator : AbstractValidator<SetAvailabilityCommand>
{
    public SetAvailabilityCommandValidator()
    {
        When(c => c.IsAvailable, () =>
        {
            RuleFor(c => c.Latitude)
                .NotNull().WithMessage("La latitude est obligatoire pour se mettre disponible.")
                .InclusiveBetween(-90, 90).WithMessage("La latitude doit être comprise entre -90 et 90.");

            RuleFor(c => c.Longitude)
                .NotNull().WithMessage("La longitude est obligatoire pour se mettre disponible.")
                .InclusiveBetween(-180, 180).WithMessage("La longitude doit être comprise entre -180 et 180.");
        });
    }
}
