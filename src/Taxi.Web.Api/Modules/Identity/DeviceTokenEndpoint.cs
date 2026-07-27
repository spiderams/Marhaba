using System.Security.Claims;
using Taxi.Application.Identity.Auth.DeviceToken;
using Taxi.SharedKernel.Messaging;
using Taxi.Web.Api.Endpoints;

namespace Taxi.Web.Api.Modules.Identity;

/// <summary>
/// Requête d'enregistrement du jeton d'appareil FCM du dernier appareil utilisé.
/// </summary>
public sealed record UpdateDeviceTokenRequest(string DeviceToken);

/// <summary>
/// Endpoint REST Identity permettant à l'application d'enregistrer son jeton FCM après authentification.
/// </summary>
public sealed class DeviceTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/auth/device-token", async (
            UpdateDeviceTokenRequest request,
            ClaimsPrincipal principal,
            ICommandHandler<UpdateDeviceTokenCommand, bool> handler,
            CancellationToken ct) =>
        {
            var userId = principal.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var result = await handler.Handle(new UpdateDeviceTokenCommand(userId, request.DeviceToken), ct);
            return result.ToHttpResult();
        })
        .RequireAuthorization()
        .WithName("UpdateDeviceToken")
        .WithTags(Tags.Identity)
        .WithSummary("Enregistrer le jeton FCM")
        .WithDescription("Associe le jeton d'appareil FCM au compte authentifié pour recevoir les offres lorsque l'application est fermée.");
    }
}