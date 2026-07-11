using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Taxi.Application.Realtime;
using Taxi.Domain.Identity;

namespace Taxi.Infrastructure.Identity;

/// <summary>
/// Lecture du jeton d'appareil (FCM) d'un utilisateur via UserManager, pour l'envoi de notifications push.
/// </summary>
internal sealed class DeviceTokenReader(UserManager<ApplicationUser> userManager) : IDeviceTokenReader
{
    public Task<string?> GetDeviceTokenAsync(string userId, CancellationToken cancellationToken)
        => userManager.Users
            .Where(u => u.Id == userId)
            .Select(u => u.DeviceToken)
            .FirstOrDefaultAsync(cancellationToken);
}
