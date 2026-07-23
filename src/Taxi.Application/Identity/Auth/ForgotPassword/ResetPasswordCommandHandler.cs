using Microsoft.AspNetCore.Identity;
using Taxi.Application.Abstractions;
using Taxi.Application.Identity.Otp;
using Taxi.Domain.Identity;
using Taxi.SharedKernel;
using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Identity.Auth.ForgotPassword;

internal sealed class ResetPasswordCommandHandler(
    UserManager<ApplicationUser> userManager,
    IRepository<PhoneOtpChallenge> otpRepository)
    : ICommandHandler<ResetPasswordCommand, bool>
{
    public async Task<Result<bool>> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByNameAsync(command.PhoneNumber);
        if (user is null)
            return Result.Failure<bool>(Error.NotFound("Auth.UserNotFound", "Utilisateur introuvable."));

        var challenge = await otpRepository.FirstOrDefaultAsync(
            new LatestPhoneOtpChallengeSpec(command.PhoneNumber, PhoneOtpChallenge.PasswordResetPurpose), cancellationToken);
        var isVerified = challenge?.Verify(command.OtpCode, DateTime.UtcNow, maxAttempts: 5) is true;
        if (challenge is not null)
            await otpRepository.UpdateAsync(challenge, cancellationToken);

        if (!isVerified)
            return Result.Failure<bool>(Error.Validation("Auth.InvalidResetCode", "Code OTP invalide ou expiré."));

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var reset = await userManager.ResetPasswordAsync(user, resetToken, command.NewPassword);
        if (!reset.Succeeded)
        {
            var first = reset.Errors.FirstOrDefault();
            return Result.Failure<bool>(Error.Validation("Auth.PasswordResetFailed", first?.Description ?? "Réinitialisation impossible."));
        }

        return true;
    }
}
