using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Taxi.Application.Abstractions;
using Taxi.Application.Identity.Otp;
using Taxi.Domain.Identity;
using Taxi.SharedKernel;
using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Identity.Auth.ForgotPassword;

internal sealed class RequestPasswordResetCommandHandler(
    UserManager<ApplicationUser> userManager,
    IRepository<PhoneOtpChallenge> repository,
    ISmsSender smsSender)
    : ICommandHandler<RequestPasswordResetCommand, bool>
{
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(5);

    public async Task<Result<bool>> Handle(RequestPasswordResetCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByNameAsync(command.PhoneNumber);
        if (user is null)
            return true;

        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var challenge = PhoneOtpChallenge.CreatePasswordReset(command.PhoneNumber, code, DateTime.UtcNow.Add(OtpLifetime));

        await repository.AddAsync(challenge, cancellationToken);
        await smsSender.SendAsync(
            command.PhoneNumber,
            $"TaxiDjibouti: votre code de réinitialisation est {code}. Il expire dans 5 minutes.",
            cancellationToken);

        return true;
    }
}
