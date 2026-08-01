using System.Security.Claims;
using Taxi.Application.Rides;
using Taxi.Application.Rides.MyRides;
using Taxi.Domain.Identity;
using Taxi.SharedKernel.Messaging;
using Taxi.Web.Api.Endpoints;

namespace Taxi.Web.Api.Modules.Rides;

/// <summary>
/// Endpoint retournant les courses de l'utilisateur connecté.
///
/// Le paramètre asDriver permet de sélectionner explicitement :
///
/// - false : les courses commandées comme Client ;
/// - true : les courses affectées comme Chauffeur.
///
/// Cette distinction est nécessaire pour les comptes multi-rôles.
/// </summary>
public sealed class MyRidesEndpoint : IEndpoint
{
    public void MapEndpoint(
        IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/rides/my-rides",
            async (
                ClaimsPrincipal principal,
                bool? asDriver,
                IQueryHandler<
                    GetMyRidesQuery,
                    IReadOnlyList<RideDto>
                > handler,
                CancellationToken ct) =>
            {
                var userId =
                    principal.GetUserId();

                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                /*
                 * Le contexte explicitement demandé par
                 * l'application mobile est prioritaire.
                 *
                 * Sans paramètre :
                 * - Driver sans rôle Client => Chauffeur ;
                 * - sinon => Client.
                 */
                var requestedAsDriver =
                    asDriver ??
                    (
                        principal.IsInRole(
                            RoleNames.Driver) &&
                        !principal.IsInRole(
                            RoleNames.Client)
                    );

                /*
                 * Un utilisateur sans rôle Driver ne peut
                 * pas demander l'historique Chauffeur.
                 */
                if (
                    requestedAsDriver &&
                    !principal.IsInRole(
                        RoleNames.Driver))
                {
                    return Results.Forbid();
                }

                var result =
                    await handler.Handle(
                        new GetMyRidesQuery(
                            userId,
                            requestedAsDriver),
                        ct);

                return result.ToHttpResult();
            })
            .RequireAuthorization()
            .WithName("MyRides")
            .WithTags(Tags.Rides)
            .WithSummary("Mes courses")
            .WithDescription(
                "Client : ses courses ; Chauffeur : " +
                "les courses qui lui sont assignées. " +
                "Le paramètre asDriver sélectionne " +
                "explicitement le contexte d’un compte " +
                "multi-rôle.");
    }
}