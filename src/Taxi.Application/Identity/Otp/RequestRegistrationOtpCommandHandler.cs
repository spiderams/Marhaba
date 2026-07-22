using System.Security.Cryptography;
using Taxi.Application.Abstractions;
using Taxi.Domain.Identity;
using Taxi.SharedKernel;
using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Identity.Otp;

internal sealed class RequestRegistrationOtpCommandHandler(
    IRepository<PhoneOtpChallenge> repository,
    ISmsSender smsSender)
    : ICommandHandler<RequestRegistrationOtpCommand, bool>
{
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(5);

    public async Task<Result<bool>> Handle(RequestRegistrationOtpCommand command, CancellationToken cancellationToken)
    {
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var challenge = PhoneOtpChallenge.CreateRegistration(command.PhoneNumber, code, DateTime.UtcNow.Add(OtpLifetime));

        await repository.AddAsync(challenge, cancellationToken);
        await smsSender.SendAsync(
            command.PhoneNumber,
            $"TaxiDjibouti: votre code de vérification est {code}. Il expire dans 5 minutes.",
            cancellationToken);

        return true;
    }
}
