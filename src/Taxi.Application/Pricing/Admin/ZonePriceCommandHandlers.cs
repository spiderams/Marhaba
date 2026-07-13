using Taxi.Application.Abstractions;
using Taxi.Application.Pricing.EstimatePrice;
using Taxi.Domain.Pricing;
using Taxi.SharedKernel;
using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Pricing.Admin;

/// <summary>
/// Gère <see cref="CreateZonePriceCommand"/> : crée un tarif zonal après avoir vérifié qu'aucun tarif
/// n'existe déjà pour la paire orientée (départ → arrivée).
/// </summary>
internal sealed class CreateZonePriceCommandHandler(IRepository<ZonePrice> repository)
    : ICommandHandler<CreateZonePriceCommand, ZonePriceDto>
{
    public async Task<Result<ZonePriceDto>> Handle(CreateZonePriceCommand command, CancellationToken cancellationToken)
    {
        var existing = await repository.FirstOrDefaultAsync(
            new ZonePriceByZonesSpec(command.FromZone, command.ToZone), cancellationToken);
        if (existing is not null)
            return Result.Failure<ZonePriceDto>(PricingErrors.DuplicatePair);

        var zonePrice = ZonePrice.Create(command.FromZone, command.ToZone, command.Price);
        await repository.AddAsync(zonePrice, cancellationToken);

        return new ZonePriceDto(zonePrice.Id, zonePrice.FromZone, zonePrice.ToZone, zonePrice.Price);
    }
}

/// <summary>
/// Gère <see cref="UpdateZonePriceCommand"/> : met à jour le montant d'un tarif zonal existant.
/// </summary>
internal sealed class UpdateZonePriceCommandHandler(IRepository<ZonePrice> repository)
    : ICommandHandler<UpdateZonePriceCommand, ZonePriceDto>
{
    public async Task<Result<ZonePriceDto>> Handle(UpdateZonePriceCommand command, CancellationToken cancellationToken)
    {
        var zonePrice = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (zonePrice is null)
            return Result.Failure<ZonePriceDto>(PricingErrors.NotFound);

        zonePrice.UpdatePrice(command.Price);
        await repository.UpdateAsync(zonePrice, cancellationToken);

        return new ZonePriceDto(zonePrice.Id, zonePrice.FromZone, zonePrice.ToZone, zonePrice.Price);
    }
}

/// <summary>
/// Gère <see cref="DeleteZonePriceCommand"/> : supprime un tarif zonal existant.
/// </summary>
internal sealed class DeleteZonePriceCommandHandler(IRepository<ZonePrice> repository)
    : ICommandHandler<DeleteZonePriceCommand, bool>
{
    public async Task<Result<bool>> Handle(DeleteZonePriceCommand command, CancellationToken cancellationToken)
    {
        var zonePrice = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (zonePrice is null)
            return Result.Failure<bool>(PricingErrors.NotFound);

        await repository.DeleteAsync(zonePrice, cancellationToken);
        return true;
    }
}

/// <summary>
/// Gère <see cref="GetZonePricesQuery"/> : retourne tous les tarifs zonaux configurés.
/// </summary>
internal sealed class GetZonePricesQueryHandler(IRepository<ZonePrice> repository)
    : IQueryHandler<GetZonePricesQuery, IReadOnlyList<ZonePriceDto>>
{
    public async Task<Result<IReadOnlyList<ZonePriceDto>>> Handle(
        GetZonePricesQuery query, CancellationToken cancellationToken)
    {
        var list = await repository.ListAsync(new AllZonePricesSpec(), cancellationToken);
        return list
            .Select(z => new ZonePriceDto(z.Id, z.FromZone, z.ToZone, z.Price))
            .ToList();
    }
}
