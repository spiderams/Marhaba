using Taxi.Application.Pricing.Admin;
using Taxi.Domain.Identity;
using Taxi.SharedKernel.Messaging;
using Taxi.Web.Api.Endpoints;

namespace Taxi.Web.Api.Modules.Pricing;

/// <summary>
/// Endpoints REST d'administration des tarifs par zone (CRUD sur <see cref="ZonePriceDto"/>).
/// Regroupés avec le module Pricing par cohérence métier ; l'accès reste réservé au rôle Admin
/// et les routes sont exposées sous <c>/api/admin/zone-prices</c>.
/// </summary>
public sealed class ZonePriceAdminEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/zone-prices")
            .RequireAuthorization(policy => policy.RequireRole(RoleNames.Admin))
            .WithTags(Tags.Pricing);

        group.MapGet("/", async (
            IQueryHandler<GetZonePricesQuery, IReadOnlyList<ZonePriceDto>> handler, CancellationToken ct) =>
                (await handler.Handle(new GetZonePricesQuery(), ct)).ToHttpResult())
            .WithName("AdminZonePrices").WithSummary("Liste des tarifs par zone");

        group.MapPost("/", async (
            CreateZonePriceCommand command,
            ICommandHandler<CreateZonePriceCommand, ZonePriceDto> handler, CancellationToken ct) =>
                (await handler.Handle(command, ct)).ToHttpResult())
            .WithName("AdminCreateZonePrice").WithSummary("Créer un tarif de zone");

        group.MapPut("/{id}", async (
            int id, UpdateZonePriceRequest body,
            ICommandHandler<UpdateZonePriceCommand, ZonePriceDto> handler, CancellationToken ct) =>
                (await handler.Handle(new UpdateZonePriceCommand(id, body.Price), ct)).ToHttpResult())
            .WithName("AdminUpdateZonePrice").WithSummary("Modifier un tarif de zone");

        group.MapDelete("/{id}", async (
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
