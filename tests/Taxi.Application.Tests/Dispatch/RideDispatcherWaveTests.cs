using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Taxi.Application.Abstractions;
using Taxi.Application.Dispatch;
using Taxi.Application.Realtime;
using Taxi.Application.Rides;
using Taxi.Domain.Rides;
using Xunit;

namespace Taxi.Application.Tests.Dispatch;

public class RideDispatcherWaveTests
{
    private static Ride PendingRideWithGps(int id = 1)
    {
        var ride = Ride.Request("client-1", "A", "B", "Z1", "Z2", 11.58, 43.14, 11.60, 43.15, 1000m);
        // Set the Id using reflection since the property has a protected setter
        typeof(Ride).BaseType?.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)
            ?.SetValue(ride, id);
        return ride;
    }

    private static NearbyDriver Driver(int id)
        => new(id, $"user-{id}", 100 * id, 11.58, 43.14, "Taxi");

    private static (Mock<IDriverLocator>, Mock<IRepository<Ride>>, Mock<IRealtimeNotifier>) Mocks(
        Ride ride, IReadOnlyList<NearbyDriver> candidates)
    {
        var locator = new Mock<IDriverLocator>();
        locator.Setup(l => l.FindNearestAsync(
                It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidates);

        var rides = new Mock<IRepository<Ride>>();
        rides.Setup(r => r.FirstOrDefaultAsync(It.IsAny<RideByIdSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ride);

        var notifier = new Mock<IRealtimeNotifier>();
        return (locator, rides, notifier);
    }

    [Fact]
    public async Task Offre_a_min_3_candidats()
    {
        var ride = PendingRideWithGps(1);
        var (locator, rides, notifier) = Mocks(ride, [Driver(1), Driver(2), Driver(3), Driver(4), Driver(5)]);
        var sut = new RideDispatcher(locator.Object, rides.Object, notifier.Object, NullLogger<RideDispatcher>.Instance);

        await sut.DispatchAsync(1, CancellationToken.None);

        ride.OfferedDriverIds.Should().BeEquivalentTo([1, 2, 3]);
        notifier.Verify(n => n.RideOfferedAsync(It.IsAny<string>(), 1, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task Offre_a_tous_si_moins_de_3_candidats()
    {
        var ride = PendingRideWithGps(1);
        var (locator, rides, notifier) = Mocks(ride, [Driver(1), Driver(2)]);
        var sut = new RideDispatcher(locator.Object, rides.Object, notifier.Object, NullLogger<RideDispatcher>.Instance);

        await sut.DispatchAsync(1, CancellationToken.None);

        ride.OfferedDriverIds.Should().BeEquivalentTo([1, 2]);
    }

    [Fact]
    public async Task Exclut_les_chauffeurs_deja_essayes()
    {
        var ride = PendingRideWithGps(1);
        ride.MarkDriverTried(1);
        ride.MarkDriverTried(2);
        var (locator, rides, notifier) = Mocks(ride, [Driver(1), Driver(2), Driver(3), Driver(4)]);
        var sut = new RideDispatcher(locator.Object, rides.Object, notifier.Object, NullLogger<RideDispatcher>.Instance);

        await sut.DispatchAsync(1, CancellationToken.None);

        ride.OfferedDriverIds.Should().BeEquivalentTo([3, 4]);
    }

    [Fact]
    public async Task Aucun_candidat_libre_sans_vague_prealable_notifie_les_admins()
    {
        // WaveCount == 0 : aucune vague n'a encore eu lieu → on garde le flux manuel (admins), course récupérable.
        var ride = PendingRideWithGps(1);
        ride.MarkDriverTried(1);
        var (locator, rides, notifier) = Mocks(ride, [Driver(1)]);
        var sut = new RideDispatcher(locator.Object, rides.Object, notifier.Object, NullLogger<RideDispatcher>.Instance);

        await sut.DispatchAsync(1, CancellationToken.None);

        ride.Status.Should().Be(RideStatus.Pending);
        notifier.Verify(n => n.NewPendingRideAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Plus_aucun_candidat_apres_une_vague_abandonne_la_course()
    {
        // WaveCount > 0 : une vague a déjà eu lieu et il ne reste plus de candidat non essayé → abandon propre.
        var ride = PendingRideWithGps(1);
        ride.OfferWave([1], DateTime.UtcNow.AddSeconds(15)); // WaveCount = 1, driver 1 essayé
        ride.ReturnToPending();                              // retour Pending (comme à l'expiration)
        var (locator, rides, notifier) = Mocks(ride, [Driver(1)]); // seul candidat déjà essayé
        var sut = new RideDispatcher(locator.Object, rides.Object, notifier.Object, NullLogger<RideDispatcher>.Instance);

        await sut.DispatchAsync(1, CancellationToken.None);

        ride.Status.Should().Be(RideStatus.NoDriverFound);
    }

    [Fact]
    public async Task Abandon_notifie_le_client_du_changement_de_statut()
    {
        var ride = PendingRideWithGps(1);
        ride.OfferWave([1], DateTime.UtcNow.AddSeconds(15));
        ride.ReturnToPending();
        var (locator, rides, notifier) = Mocks(ride, [Driver(1)]);
        var sut = new RideDispatcher(locator.Object, rides.Object, notifier.Object, NullLogger<RideDispatcher>.Instance);

        await sut.DispatchAsync(1, CancellationToken.None);

        notifier.Verify(n => n.RideStatusChangedAsync(
            1, "client-1", null, nameof(RideStatus.NoDriverFound), It.IsAny<CancellationToken>()), Times.Once);
    }
}
