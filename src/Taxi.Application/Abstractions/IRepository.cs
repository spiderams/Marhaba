using Ardalis.Specification;
using Taxi.SharedKernel;

namespace Taxi.Application.Abstractions;

/// <summary>
/// Dépôt générique (Repository + Specification d'Ardalis) pour une entité du domaine. Abstrait l'accès aux données : la couche Application ne dépend pas d'EF Core.
/// </summary>
public interface IRepository<T> : IRepositoryBase<T> where T : Entity
{
    /// <summary>
    /// Retourne un <see cref="IQueryable{T}"/> (en lecture seule, non suivi) filtré par la spécification,
    /// permettant aux handlers d'appliquer des agrégations exécutées en base (Count, Sum…) sans matérialiser
    /// les entités ni exposer EF Core à la couche Application.
    /// </summary>
    IQueryable<T> Query(ISpecification<T> specification);
}
