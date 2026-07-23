using FluentValidation;

namespace Taxi.Application.Identity.Auth.Profile;

internal sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.FullName).NotEmpty().MaximumLength(120);
    }
}