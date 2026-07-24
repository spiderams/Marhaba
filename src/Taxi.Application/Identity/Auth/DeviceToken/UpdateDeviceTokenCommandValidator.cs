using FluentValidation;

namespace Taxi.Application.Identity.Auth.DeviceToken;

/// <summary>
/// Validation du jeton FCM : l'utilisateur authentifié et le jeton d'appareil sont obligatoires.
/// </summary>
internal sealed class UpdateDeviceTokenCommandValidator : AbstractValidator<UpdateDeviceTokenCommand>
{
    public UpdateDeviceTokenCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.DeviceToken)
            .NotEmpty()
            .MaximumLength(4096);
    }
}