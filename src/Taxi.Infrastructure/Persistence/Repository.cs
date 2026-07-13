using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Taxi.Application.Abstractions;
using Taxi.SharedKernel;

namespace Taxi.Infrastructure.Persistence;

/// <summary>
/// Implémentation générique du dépôt (Ardalis) au-dessus d'EF Core.
/// </summary>
public sealed class Repository<T>(AppDbContext context)
    : RepositoryBase<T>(context), IRepository<T> where T : Entity
{
    /// <inheritdoc />
    public IQueryable<T> Query(ISpecification<T> specification)
        => Ardalis.Specification.EntityFrameworkCore.SpecificationEvaluator.Default
            .GetQuery(context.Set<T>().AsNoTracking(), specification);
}
