using FluentAssertions;
using Taxi.Domain.Drivers;
using Taxi.Infrastructure.Dispatch;
using Taxi.IntegrationTests.Identity;

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
        await fixture.ResetDriversAsync();
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

    /// <summary>
    /// Un chauffeur qui n'est pas au statut Approved (PendingApproval, Suspended ou Rejected)
    /// ne doit jamais apparaître comme candidat, même s'il est disponible et proche :
    /// il ne pourra donc jamais être inclus dans une vague de dispatch.
    /// </summary>
    [Fact]
    public async Task FindNearestAsync_should_return_only_approved_among_all_statuses()
    {
        await fixture.ResetDriversAsync();
        await using var db = fixture.CreateContext();

        // Tous disponibles, au même endroit : seul le statut d'approbation les distingue.
        var approved = Driver.Create("u-approved", "LIC-A", "DJ-1000", "Taxi");
        approved.Approve();
        approved.GoOnline(PickupLatitude, PickupLongitude);

        var pending = Driver.Create("u-pending", "LIC-P", "DJ-2000", "Taxi");
        pending.GoOnline(PickupLatitude, PickupLongitude); // PendingApproval

        var suspended = Driver.Create("u-suspended", "LIC-S", "DJ-3000", "Taxi");
        suspended.Approve();
        suspended.Suspend();
        suspended.GoOnline(PickupLatitude, PickupLongitude); // Suspended

        var rejected = Driver.Create("u-rejected", "LIC-R", "DJ-4000", "Taxi");
        rejected.Reject();
        rejected.GoOnline(PickupLatitude, PickupLongitude); // Rejected

        db.Drivers.AddRange(approved, pending, suspended, rejected);
        await db.SaveChangesAsync();

        var locator = new DriverLocator(db);
        var result = await locator.FindNearestAsync(
            PickupLatitude, PickupLongitude, radiusMeters: 5000, max: 10, CancellationToken.None);

        result.Should().ContainSingle();
        result[0].UserId.Should().Be("u-approved");
    }
    [Fact]
    public async Task FindNearestAsync_should_find_an_online_driver_near_a_canadian_pickup()
    {
        await fixture.ResetDriversAsync();
        await using var db = fixture.CreateContext();

        const double montrealLatitude = 45.5019;
        const double montrealLongitude = -73.5674;
        var approved = Driver.Create("u-canada", "LIC-CA", "CA-0001", "Taxi");
        approved.Approve();
        approved.GoOnline(montrealLatitude, montrealLongitude);

        db.Drivers.Add(approved);
        await db.SaveChangesAsync();

        var locator = new DriverLocator(db);
        var result = await locator.FindNearestAsync(
            montrealLatitude, montrealLongitude, radiusMeters: 5000, max: 10, CancellationToken.None);

        result.Should().ContainSingle();
        result[0].UserId.Should().Be("u-canada");
    }
}
