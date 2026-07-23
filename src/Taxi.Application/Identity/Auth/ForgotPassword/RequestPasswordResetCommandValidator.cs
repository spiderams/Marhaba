using FluentValidation;

namespace Taxi.Application.Identity.Auth.ForgotPassword;

internal sealed class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetCommandValidator()
    {
        RuleFor(c => c.PhoneNumber).NotEmpty();
    }
}
