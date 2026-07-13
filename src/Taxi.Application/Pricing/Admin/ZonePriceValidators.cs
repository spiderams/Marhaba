using FluentValidation;

namespace Taxi.Application.Pricing.Admin;

/// <summary>
/// Règles de validation de <see cref="CreateZonePriceCommand"/> : zones non vides et prix strictement positif.
/// </summary>
internal sealed class CreateZonePriceCommandValidator : AbstractValidator<CreateZonePriceCommand>
{
    public CreateZonePriceCommandValidator()
    {
        RuleFor(c => c.FromZone).NotEmpty().WithMessage("La zone de départ est obligatoire.");
        RuleFor(c => c.ToZone).NotEmpty().WithMessage("La zone d'arrivée est obligatoire.");
        RuleFor(c => c.Price).GreaterThan(0).WithMessage("Le tarif doit être supérieur à zéro.");
    }
}

/// <summary>
/// Règles de validation de <see cref="UpdateZonePriceCommand"/> : prix strictement positif.
/// </summary>
internal sealed class UpdateZonePriceCommandValidator : AbstractValidator<UpdateZonePriceCommand>
{
    public UpdateZonePriceCommandValidator()
    {
        RuleFor(c => c.Price).GreaterThan(0).WithMessage("Le tarif doit être supérieur à zéro.");
    }
}
