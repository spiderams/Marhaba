using System.Security.Claims;
using Taxi.Application.Rides;
using Taxi.Application.Rides.DriverHistory;
using Taxi.Domain.Identity;
using Taxi.SharedKernel.Messaging;
using Taxi.Web.Api.Endpoints;

namespace Taxi.Web.Api.Modules.Rides;

/// <summary>
/// Endpoint REST de consultation de l'historique du chauffeur authentifié.
/// </summary>
public sealed class DriverRideHistoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/drivers/me/rides/history", async (
            ClaimsPrincipal principal,
            IQueryHandler<GetDriverRideHistoryQuery, IReadOnlyList<RideDto>> handler,
            CancellationToken ct) =>
        {
            var userId = principal.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var result = await handler.Handle(new GetDriverRideHistoryQuery(userId), ct);
            return result.ToHttpResult();
        })
        .RequireAuthorization(policy => policy.RequireRole(RoleNames.Driver))
        .WithName("GetDriverRideHistory")
        .WithTags(Tags.Rides)
        .WithSummary("Historique des courses du chauffeur")
        .WithDescription("Retourne les courses terminées du chauffeur, avec leur tarif final, de la plus récente à la plus ancienne.");
    }
}
