using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Pricing.Admin;

/// <summary>
/// Projection d'un tarif zonal exposée à l'administration.
/// </summary>
public sealed record ZonePriceDto(int Id, string FromZone, string ToZone, decimal Price);

/// <summary>
/// Crée un nouveau tarif entre deux zones. Échoue si un tarif existe déjà pour cette paire orientée.
/// </summary>
public sealed record CreateZonePriceCommand(string FromZone, string ToZone, decimal Price) : ICommand<ZonePriceDto>;

/// <summary>
/// Met à jour le montant d'un tarif zonal existant, identifié par son <paramref name="Id"/>.
/// </summary>
public sealed record UpdateZonePriceCommand(int Id, decimal Price) : ICommand<ZonePriceDto>;

/// <summary>
/// Supprime un tarif zonal existant, identifié par son <paramref name="Id"/>.
/// </summary>
public sealed record DeleteZonePriceCommand(int Id) : ICommand<bool>;

/// <summary>
/// Liste tous les tarifs zonaux configurés.
/// </summary>
public sealed record GetZonePricesQuery : IQuery<IReadOnlyList<ZonePriceDto>>;
