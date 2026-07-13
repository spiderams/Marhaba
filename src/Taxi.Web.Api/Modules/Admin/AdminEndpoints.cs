using Taxi.Application.Administration;
using Taxi.Application.Administration.Listing;
using Taxi.Application.Administration.Stats;
using Taxi.Application.Drivers;
using Taxi.Application.Pricing.Admin;
using Taxi.Application.Rides;
using Taxi.Domain.Identity;
using Taxi.SharedKernel.Messaging;
using Taxi.Web.Api.Endpoints;

namespace Taxi.Web.Api.Modules.Admin;

/// <summary>
/// Endpoints REST du module Admin (statistiques, listes d'utilisateurs, chauffeurs, courses et signalements).
/// </summary>
public sealed class AdminEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .RequireAuthorization(policy => policy.RequireRole(RoleNames.Admin))
            .WithTags(Tags.Admin);

        group.MapGet("/stats", async (
            IQueryHandler<GetAdminStatsQuery, AdminStatsDto> handler, CancellationToken ct) =>
                (await handler.Handle(new GetAdminStatsQuery(), ct)).ToHttpResult())
            .WithName("AdminStats").WithSummary("Statistiques globales");

        group.MapGet("/users", async (
            IQueryHandler<GetUsersQuery, IReadOnlyList<UserSummary>> handler, CancellationToken ct) =>
                (await handler.Handle(new GetUsersQuery(), ct)).ToHttpResult())
            .WithName("AdminUsers").WithSummary("Liste des utilisateurs");

        group.MapGet("/drivers", async (
            IQueryHandler<GetDriversQuery, IReadOnlyList<DriverDto>> handler, CancellationToken ct) =>
                (await handler.Handle(new GetDriversQuery(), ct)).ToHttpResult())
            .WithName("AdminDrivers").WithSummary("Liste des chauffeurs");

        group.MapGet("/rides", async (
            IQueryHandler<GetAllRidesQuery, IReadOnlyList<RideDto>> handler, CancellationToken ct) =>
                (await handler.Handle(new GetAllRidesQuery(), ct)).ToHttpResult())
            .WithName("AdminRides").WithSummary("Liste des courses");

        group.MapGet("/reports", async (
            IQueryHandler<GetReportsQuery, IReadOnlyList<ReportDto>> handler, CancellationToken ct) =>
                (await handler.Handle(new GetReportsQuery(), ct)).ToHttpResult())
            .WithName("AdminReports").WithSummary("Liste des signalements");

        // --- Tarifs par zone (CRUD réservé à l'admin) ---

        group.MapGet("/zone-prices", async (
            IQueryHandler<GetZonePricesQuery, IReadOnlyList<ZonePriceDto>> handler, CancellationToken ct) =>
                (await handler.Handle(new GetZonePricesQuery(), ct)).ToHttpResult())
            .WithName("AdminZonePrices").WithSummary("Liste des tarifs par zone");

        group.MapPost("/zone-prices", async (
            CreateZonePriceCommand command,
            ICommandHandler<CreateZonePriceCommand, ZonePriceDto> handler, CancellationToken ct) =>
                (await handler.Handle(command, ct)).ToHttpResult())
            .WithName("AdminCreateZonePrice").WithSummary("Créer un tarif de zone");

        group.MapPut("/zone-prices/{id}", async (
            int id, UpdateZonePriceRequest body,
            ICommandHandler<UpdateZonePriceCommand, ZonePriceDto> handler, CancellationToken ct) =>
                (await handler.Handle(new UpdateZonePriceCommand(id, body.Price), ct)).ToHttpResult())
            .WithName("AdminUpdateZonePrice").WithSummary("Modifier un tarif de zone");

        group.MapDelete("/zone-prices/{id}", async (
            int id,
            ICommandHandler<DeleteZonePriceCommand, bool> handler, CancellationToken ct) =>
                (await handler.Handle(new DeleteZonePriceCommand(id), ct)).ToHttpResult())
            .WithName("AdminDeleteZonePrice").WithSummary("Supprimer un tarif de zone");
    }
}

/// <summary>
/// Corps de la requête de mise à jour d'un tarif de zone : nouveau montant.
/// </summary>
public sealed record UpdateZonePriceRequest(decimal Price);
