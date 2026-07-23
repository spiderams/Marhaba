using FluentValidation;

namespace Taxi.Application.Identity.Auth.ForgotPassword;

internal sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(c => c.PhoneNumber).NotEmpty();
        RuleFor(c => c.OtpCode).NotEmpty().Length(6);
        RuleFor(c => c.NewPassword).NotEmpty().MinimumLength(6);
    }
}