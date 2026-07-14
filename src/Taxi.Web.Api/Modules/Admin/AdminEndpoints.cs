using Taxi.Application.Administration;
using Taxi.Application.Administration.Drivers;
using Taxi.Application.Administration.Listing;
using Taxi.Application.Administration.Stats;
using Taxi.Application.Drivers;
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

             group.MapPost("/drivers/{driverId:int}/approve", async (
            int driverId,
            ICommandHandler<ApproveDriverCommand, DriverDto> handler,
            CancellationToken ct) =>
                (await handler.Handle(new ApproveDriverCommand(driverId), ct)).ToHttpResult())
            .WithName("AdminApproveDriver").WithSummary("Approuve un chauffeur");

        group.MapPost("/drivers/{driverId:int}/suspend", async (
            int driverId,
            ICommandHandler<SuspendDriverCommand, DriverDto> handler,
            CancellationToken ct) =>
                (await handler.Handle(new SuspendDriverCommand(driverId), ct)).ToHttpResult())
            .WithName("AdminSuspendDriver").WithSummary("Suspend un chauffeur");

        group.MapPost("/drivers/{driverId:int}/reject", async (
            int driverId,
            ICommandHandler<RejectDriverCommand, DriverDto> handler,
            CancellationToken ct) =>
                (await handler.Handle(new RejectDriverCommand(driverId), ct)).ToHttpResult())
            .WithName("AdminRejectDriver").WithSummary("Rejette un chauffeur");
        // Les tarifs par zone (CRUD Admin) sont dans Modules/Pricing/ZonePriceAdminEndpoints.
    }
}
