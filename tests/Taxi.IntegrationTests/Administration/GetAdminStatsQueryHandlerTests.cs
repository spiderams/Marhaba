using FluentAssertions;
using Taxi.Application.Administration;
using Taxi.Application.Administration.Stats;
using Taxi.Domain.Rides;
using Taxi.Infrastructure.Persistence;
using Xunit;

namespace Taxi.IntegrationTests.Administration;

/// <summary>
/// Tests d'intégration de <see cref="GetAdminStatsQueryHandler"/> contre un vrai PostgreSQL :
/// vérifie que le chiffre d'affaires et le nombre de courses terminées sont agrégés en base
/// à partir du tarif final réel figé à la complétion.
/// </summary>
public sealed class GetAdminStatsQueryHandlerTests(PostgisContainerFixture fixture)
    : IClassFixture<PostgisContainerFixture>
{
    // Annuaire d'utilisateurs minimal : le compte n'est pas au cœur de ce test.
    private sealed class FakeUserDirectory : IUserDirectory
    {
        public Task<int> CountAsync(CancellationToken cancellationToken) => Task.FromResult(0);
        public Task<IReadOnlyList<UserSummary>> ListAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<UserSummary>>([]);
    }

    private static Ride CompletedRide(string clientId, decimal finalPrice)
    {
        var ride = Ride.Request(clientId, "A", "B", "Z1", "Z2", 11.58, 43.14, 11.60, 43.15, 1000m);
        ride.Accept(1);
        ride.MarkArrived();
        ride.Start();
        ride.Complete(finalPrice, PaymentMethod.Cash);
        return ride;
    }

    [Fact]
    public async Task Handle_should_aggregate_revenue_from_completed_rides_final_price()
    {
        await fixture.ResetRidesAsync();
        await using var db = fixture.CreateContext();

        // Deux courses terminées (1500 + 2000) et une course en attente (ignorée du CA).
        db.Rides.Add(CompletedRide("client-1", 1500m));
        db.Rides.Add(CompletedRide("client-2", 2000m));
        db.Rides.Add(Ride.Request("client-3", "A", "B", "Z1", "Z2", 11.58, 43.14, 11.60, 43.15, 1000m));
        await db.SaveChangesAsync();

        var handler = new GetAdminStatsQueryHandler(
            new FakeUserDirectory(),
            new Repository<Domain.Drivers.Driver>(db),
            new Repository<Ride>(db),
            new Repository<Report>(db));

        var result = await handler.Handle(new GetAdminStatsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CompletedRides.Should().Be(2);
        result.Value.TotalRevenue.Should().Be(3500m);
        result.Value.Rides.Should().Be(3); // total, toutes courses confondues
    }
}
