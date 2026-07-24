using Microsoft.AspNetCore.Identity;
using Taxi.Domain.Identity;
using Taxi.SharedKernel;
using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Identity.Auth.DeviceToken;

/// <summary>
/// Gère <see cref="UpdateDeviceTokenCommand"/> : enregistre le dernier jeton FCM de l'appareil
/// afin de pouvoir réveiller l'application chauffeur lorsqu'une offre est dispatchée.
/// </summary>
internal sealed class UpdateDeviceTokenCommandHandler(UserManager<ApplicationUser> userManager)
    : ICommandHandler<UpdateDeviceTokenCommand, bool>
{
    public async Task<Result<bool>> Handle(UpdateDeviceTokenCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId);
        if (user is null)
            return Result.Failure<bool>(Error.NotFound("Auth.UserNotFound", "Utilisateur introuvable."));

        user.DeviceToken = command.DeviceToken.Trim();
        var updated = await userManager.UpdateAsync(user);
        if (!updated.Succeeded)
        {
            var first = updated.Errors.FirstOrDefault();
            return Result.Failure<bool>(Error.Validation(
                "Auth.DeviceTokenUpdateFailed",
                first?.Description ?? "Enregistrement du jeton d'appareil impossible."));
        }

        return true;
    }
}