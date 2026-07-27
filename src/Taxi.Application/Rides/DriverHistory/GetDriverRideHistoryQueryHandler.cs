using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using FluentAssertions;
using Moq;
using Taxi.Application.Abstractions;
using Taxi.Application.Rides.DriverHistory;
using Taxi.Domain.Drivers;
using Taxi.Domain.Rides;

namespace Taxi.Application.Tests.Rides;

public sealed class DriverRideHistoryTests
{
    private const int DriverId = 42;

    [Fact]
    public async Task Handle_ShouldReturnCompletedRides_ForDriver()
    {
        var completedRide = CreateCompletedRide(DriverId, 1000m, 1500m, DateTime.UtcNow);
        var handler = CreateHandler([completedRide], out _, out _);

        var result = await handler.Handle(new GetDriverRideHistoryQuery("driver-user"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value[0].DriverId.Should().Be(DriverId);
        result.Value[0].Status.Should().Be(nameof(RideStatus.Completed));
    }

    [Fact]
    public async Task Handle_ShouldOrderRides_ByCompletedAtDescending()
    {
        var older = CreateCompletedRide(DriverId, 1000m, 1200m, DateTime.UtcNow.AddDays(-2), id: 1);
        var newer = CreateCompletedRide(DriverId, 1000m, 1800m, DateTime.UtcNow.AddDays(-1), id: 2);
        var handler = CreateHandler([older, newer], out _, out _);

        var result = await handler.Handle(new GetDriverRideHistoryQuery("driver-user"), CancellationToken.None);

        result.Value.Select(ride => ride.Id).Should().ContainInOrder(2, 1);
    }

    [Fact]
    public async Task Handle_ShouldExcludeActiveRides()
    {
        var completed = CreateCompletedRide(DriverId, 1000m, 1500m, DateTime.UtcNow, id: 1);
        var active = CreateAcceptedRide(DriverId, id: 2);
        var handler = CreateHandler([active, completed], out _, out _);

        var result = await handler.Handle(new GetDriverRideHistoryQuery("driver-user"), CancellationToken.None);

        result.Value.Should().ContainSingle().Which.Id.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenDriverHasNoHistory()
    {
        var handler = CreateHandler([], out _, out _);

        var result = await handler.Handle(new GetDriverRideHistoryQuery("driver-user"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenDriverProfileDoesNotExist()
    {
        var drivers = new Mock<IRepository<Driver>>();
        var rides = new Mock<IRepository<Ride>>();
        drivers.Setup(repository => repository.FirstOrDefaultAsync(
                It.IsAny<ISpecification<Driver>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Driver?)null);
        var handler = new GetDriverRideHistoryQueryHandler(drivers.Object, rides.Object);

        var result = await handler.Handle(new GetDriverRideHistoryQuery("missing-driver"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        rides.Verify(repository => repository.ListAsync(
            It.IsAny<ISpecification<Ride>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUseFinalPrice_NotEstimatedPrice()
    {
        var handler = CreateHandler(
            [CreateCompletedRide(DriverId, estimatedPrice: 500m, finalPrice: 1800m, completedAt: DateTime.UtcNow)],
            out _, out _);

        var result = await handler.Handle(new GetDriverRideHistoryQuery("driver-user"), CancellationToken.None);

        result.Value.Should().ContainSingle();
        result.Value[0].EstimatedPrice.Should().Be(500m);
        result.Value[0].FinalPrice.Should().Be(1800m);
        result.Value[0].PaymentMethod.Should().Be(nameof(PaymentMethod.Cash));
    }

    private static GetDriverRideHistoryQueryHandler CreateHandler(
        IReadOnlyCollection<Ride> availableRides,
        out Mock<IRepository<Driver>> drivers,
        out Mock<IRepository<Ride>> rides)
    {
        var driver = Driver.Create("driver-user", "LIC-001", "DJ-001", "Taxi");
        SetProperty(driver, nameof(Driver.Id), DriverId);

        drivers = new Mock<IRepository<Driver>>();
        drivers.Setup(repository => repository.FirstOrDefaultAsync(
                It.IsAny<ISpecification<Driver>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(driver);

        rides = new Mock<IRepository<Ride>>();
        rides.Setup(repository => repository.ListAsync(
                It.IsAny<ISpecification<Ride>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<Ride> specification, CancellationToken _) =>
                SpecificationEvaluator.Default
                    .GetQuery(availableRides.AsQueryable(), specification)
                    .ToList());

        return new GetDriverRideHistoryQueryHandler(drivers.Object, rides.Object);
    }

    private static Ride CreateCompletedRide(
        int driverId,
        decimal estimatedPrice,
        decimal finalPrice,
        DateTime completedAt,
        int id = 1)
    {
        var ride = CreateAcceptedRide(driverId, id, estimatedPrice);
        ride.MarkArrived();
        ride.Start();
        ride.Complete(finalPrice, PaymentMethod.Cash);
        SetProperty(ride, nameof(Ride.CompletedAt), completedAt);
        return ride;
    }

    private static Ride CreateAcceptedRide(int driverId, int id, decimal estimatedPrice = 1000m)
    {
        var ride = Ride.Request("client", "Place Menelik", "Aéroport", "Centre", "Aéroport",
            11.59, 43.14, 11.55, 43.16, estimatedPrice);
        SetProperty(ride, nameof(Ride.Id), id);
        ride.Accept(driverId);
        return ride;
    }

    private static void SetProperty(object entity, string propertyName, object value)
        => entity.GetType().GetProperty(propertyName)!.SetValue(entity, value);
}
