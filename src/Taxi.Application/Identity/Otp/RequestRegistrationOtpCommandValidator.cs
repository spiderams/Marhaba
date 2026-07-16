using FluentValidation;

namespace Taxi.Application.Identity.Otp;

internal sealed class RequestRegistrationOtpCommandValidator : AbstractValidator<RequestRegistrationOtpCommand>
{
    public RequestRegistrationOtpCommandValidator()
    {
        RuleFor(c => c.PhoneNumber).NotEmpty();
    }
}
