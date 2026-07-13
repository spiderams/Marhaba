using FluentValidation;

namespace Taxi.Application.Rides.Transitions;

/// <summary>
/// Règles de validation de <see cref="CompleteRideCommand"/> : le montant final figé doit être strictement positif.
/// </summary>
internal sealed class CompleteRideCommandValidator : AbstractValidator<CompleteRideCommand>
{
    public CompleteRideCommandValidator()
    {
        RuleFor(c => c.FinalPrice)
            .GreaterThan(0)
            .WithMessage("Le montant final doit être supérieur à zéro.");
    }
}
