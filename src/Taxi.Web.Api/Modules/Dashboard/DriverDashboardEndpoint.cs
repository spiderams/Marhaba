using System.Security.Claims;
using Taxi.Application.Dashboard;
using Taxi.SharedKernel.Messaging;
using Taxi.Web.Api.Endpoints;
using Taxi.Domain.Identity;

namespace Taxi.Web.Api.Modules.Dashboard;

/// <summary>
/// Endpoint REST du tableau de bord chauffeur.
/// </summary>
public sealed class DriverDashboardEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/drivers/me/dashboard", async (
            ClaimsPrincipal principal,
            IQueryHandler<GetDriverDashboardQuery, DriverDashboardDto> handler,
            CancellationToken ct) =>
        {
            var userId = principal.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var result = await handler.Handle(new GetDriverDashboardQuery(userId), ct);
            return result.ToHttpResult();
        })
        .RequireAuthorization(policy => policy.RequireRole(RoleNames.Driver)).WithTags(Tags.Drivers)
        .WithSummary("Tableau de bord chauffeur")
        .WithDescription("Retourne les gains réels du chauffeur, calculés depuis FinalPrice des courses terminées.");
    }
}