using FluentAssertions;
using Taxi.Domain.Drivers;
using Taxi.Infrastructure.Dispatch;

namespace Taxi.IntegrationTests.Dispatch;

/// <summary>
/// Tests d'intégration du <see cref="DriverLocator"/> contre un vrai PostgreSQL + PostGIS :
/// vérifie que la recherche de proximité applique bien les règles d'éligibilité au dispatch.
/// </summary>
public sealed class DriverLocatorTests(PostgisContainerFixture fixture) : IClassFixture<PostgisContainerFixture>
{
    // Point de prise en charge de référence (Djibouti-ville).
    private const double PickupLatitude = 11.588;
    private const double PickupLongitude = 43.145;

    [Fact]
    public async Task FindNearestAsync_should_exclude_non_approved_drivers()
    {
        await using var db = fixture.CreateContext();

        // Deux chauffeurs au même endroit, tous deux disponibles : seul le statut diffère.
        var approved = Driver.Create("u-approved", "LIC-A", "DJ-0001", "Taxi");
        approved.Approve();
        approved.GoOnline(PickupLatitude, PickupLongitude);

        var pending = Driver.Create("u-pending", "LIC-P", "DJ-0002", "Taxi");
        pending.GoOnline(PickupLatitude, PickupLongitude); // reste PendingApproval

        db.Drivers.AddRange(approved, pending);
        await db.SaveChangesAsync();

        var locator = new DriverLocator(db);
        var result = await locator.FindNearestAsync(
            PickupLatitude, PickupLongitude, radiusMeters: 5000, max: 10, CancellationToken.None);

        result.Should().ContainSingle();
        result[0].UserId.Should().Be("u-approved");
    }
}
