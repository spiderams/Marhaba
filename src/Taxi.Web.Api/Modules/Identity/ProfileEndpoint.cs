using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Taxi.Application.Identity.Auth;
using Taxi.Application.Identity.Auth.Profile;
using Taxi.SharedKernel.Messaging;
using Taxi.Web.Api.Endpoints;

namespace Taxi.Web.Api.Modules.Identity;

/// <summary>
/// Requête de mise à jour des champs éditables de l'écran profil.
/// </summary>
public sealed record UpdateProfileRequest(string FullName);

/// <summary>
/// Endpoint REST du module Identity (mise à jour de l'écran profil).
/// </summary>
public sealed class ProfileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/auth/me", async (
            UpdateProfileRequest request,
            ClaimsPrincipal principal,
            ICommandHandler<UpdateProfileCommand, UserInfo> handler,
            CancellationToken ct) =>
        {
            var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                         ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var result = await handler.Handle(new UpdateProfileCommand(userId, request.FullName), ct);
            return result.ToHttpResult();
        })
        .RequireAuthorization()
        .WithName("UpdateMe")
        .WithTags(Tags.Identity)
        .WithSummary("Mettre à jour le profil courant")
        .WithDescription("Met à jour les champs éditables de l'écran profil de l'utilisateur authentifié.");
    }
}