using Microsoft.AspNetCore.Identity;
using Taxi.Domain.Identity;
using Taxi.SharedKernel;
using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Identity.Auth.Profile;

internal sealed class UpdateProfileCommandHandler(UserManager<ApplicationUser> userManager)
    : ICommandHandler<UpdateProfileCommand, UserInfo>
{
    public async Task<Result<UserInfo>> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId);
        if (user is null)
            return Result.Failure<UserInfo>(Error.NotFound("Auth.UserNotFound", "Utilisateur introuvable."));

        user.FullName = command.FullName.Trim();
        var updated = await userManager.UpdateAsync(user);
        if (!updated.Succeeded)
        {
            var first = updated.Errors.FirstOrDefault();
            return Result.Failure<UserInfo>(Error.Validation("Auth.ProfileUpdateFailed", first?.Description ?? "Mise à jour du profil impossible."));
        }

        var roles = await userManager.GetRolesAsync(user);
        return new UserInfo(user.Id, user.FullName, user.PhoneNumber!, roles.ToList());
    }
}