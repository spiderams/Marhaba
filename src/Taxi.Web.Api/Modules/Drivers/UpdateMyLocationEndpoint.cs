using System.Security.Claims;
using Taxi.Application.Drivers;
using Taxi.Application.Drivers.UpdateLocation;
using Taxi.Domain.Identity;
using Taxi.SharedKernel.Messaging;
using Taxi.Web.Api.Endpoints;

namespace Taxi.Web.Api.Modules.Drivers;

public sealed record UpdateMyLocationRequest(double Latitude, double Longitude);

/// <summary>
/// Endpoint de battement de position du chauffeur en ligne hors course : rafraîchit sa dernière position connue
/// pour rester éligible au dispatch de proximité. À appeler périodiquement par l'application chauffeur tant qu'il est disponible.
/// </summary>
public sealed class UpdateMyLocationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/drivers/location", async (
            UpdateMyLocationRequest body,
            ClaimsPrincipal principal,
            ICommandHandler<UpdateMyLocationCommand, DriverDto> handler,
            CancellationToken ct) =>
        {
            var userId = principal.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var result = await handler.Handle(
                new UpdateMyLocationCommand(userId, body.Latitude, body.Longitude), ct);
            return result.ToHttpResult();
        })
        .RequireAuthorization(policy => policy.RequireRole(RoleNames.Driver))
        .WithName("UpdateMyDriverLocation")
        .WithTags(Tags.Drivers)
        .WithSummary("Mettre à jour ma position (hors course)")
        .WithDescription("Rafraîchit la dernière position connue du chauffeur en ligne sans course active, pour le dispatch de proximité.");
    }
}
