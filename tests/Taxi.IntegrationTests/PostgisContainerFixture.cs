using Microsoft.EntityFrameworkCore;
using Taxi.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Taxi.IntegrationTests;

/// <summary>
/// Fixture xUnit démarrant un conteneur PostgreSQL + PostGIS jetable pour les tests d'intégration
/// de la couche Infrastructure. Applique les migrations EF au démarrage et fournit un
/// <see cref="AppDbContext"/> câblé exactement comme l'application (Npgsql + NetTopologySuite + snake_case).
/// Nécessite un daemon Docker actif.
/// </summary>
public sealed class PostgisContainerFixture : IAsyncLifetime
{
    // Image identique à celle utilisée par Aspire : l'extension PostGIS est indispensable
    // (colonne geography + HasPostgresExtension("postgis")).
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgis/postgis:16-3.4")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var db = CreateContext();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    /// <summary>
    /// Crée un nouveau <see cref="AppDbContext"/> pointant sur le conteneur, configuré comme en production.
    /// </summary>
    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString, npgsql => npgsql.UseNetTopologySuite())
            .UseSnakeCaseNamingConvention()
            .Options;

        return new AppDbContext(options);
    }

    /// <summary>
    /// Vide la table des chauffeurs afin que chaque test parte d'un état propre :
    /// le conteneur étant partagé par la classe, cette réinitialisation garantit l'isolation entre tests.
    /// </summary>
    public async Task ResetDriversAsync()
    {
        await using var db = CreateContext();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE drivers RESTART IDENTITY CASCADE;");
    }

    /// <summary>
    /// Vide la table des courses pour isoler chaque test qui manipule des courses.
    /// </summary>
    public async Task ResetRidesAsync()
    {
        await using var db = CreateContext();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE rides RESTART IDENTITY CASCADE;");
    }
}
