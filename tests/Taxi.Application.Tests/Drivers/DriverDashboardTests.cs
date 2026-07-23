using Ardalis.Specification;
using FluentAssertions;
using Moq;
using System.Timers;
using Taxi.Application.Abstractions;
using Taxi.Application.Dashboard;
using Taxi.Domain.Drivers;
using Taxi.Domain.Rides;
using Xunit;

namespace Taxi.Application.Tests.Drivers;

public sealed class DriverDashboardTests
{
    private static Driver DriverProfile()
    {
        var driver = Driver.Create("driver-user", "LIC-1", "DJ-001", "Taxi");
        SetEntityId(driver, 42);
        return driver;
    }

    private static Ride CompletedRide(int driverId, decimal estimatedPrice, decimal finalPrice)
    {
        var ride = Ride.Request("client-1", "A", "B", "Z1", "Z2", null, null, null, null, estimatedPrice);
        ride.Accept(driverId);
        ride.MarkArrived();
        ride.Start();
        ride.Complete(finalPrice, PaymentMethod.Cash);
        return ride;
    }

    private static void SetEntityId(object entity, int id)
    {
        var property = entity.GetType().GetProperty("Id")!;
        property.SetValue(entity, id);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenDriverProfileDoesNotExist()
    {
        var drivers = new Mock<IRepository<Driver>>();
        var rides = new Mock<IRepository<Ride>>();
        drivers.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<ISpecification<Driver>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Driver?)null);

        var handler = new GetDriverDashboardQueryHandler(drivers.Object, rides.Object);

        var result = await handler.Handle(new GetDriverDashboardQuery("missing-driver"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Driver.NotFound");
        rides.Verify(repo => repo.ListAsync(It.IsAny<ISpecification<Ride>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCalculateEarnings_FromCompletedRidesFinalPrice()
    {
        var driver = DriverProfile();
        var completedRides = new List<Ride>
        {
            CompletedRide(driver.Id, estimatedPrice: 1000m, finalPrice: 1500m),
            CompletedRide(driver.Id, estimatedPrice: 1000m, finalPrice: 2000m)
        };
        var drivers = new Mock<IRepository<Driver>>();
        var rides = new Mock<IRepository<Ride>>();
        drivers.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<ISpecification<Driver>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(driver);
        rides.Setup(repo => repo.ListAsync(It.IsAny<ISpecification<Ride>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(completedRides);

        var handler = new GetDriverDashboardQueryHandler(drivers.Object, rides.Object);

        var result = await handler.Handle(new GetDriverDashboardQuery("driver-user"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.DriverId.Should().Be(driver.Id);
        result.Value.CompletedRides.Should().Be(2);
        result.Value.TotalEarnings.Should().Be(3500m);
    }

    [Fact]
    public async Task Handle_ShouldUseFinalPrice_NotEstimatedPrice()
    {
        var driver = DriverProfile();
        var drivers = new Mock<IRepository<Driver>>();
        var rides = new Mock<IRepository<Ride>>();
        drivers.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<ISpecification<Driver>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(driver);
        rides.Setup(repo => repo.ListAsync(It.IsAny<ISpecification<Ride>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Ride> { CompletedRide(driver.Id, estimatedPrice: 500m, finalPrice: 1800m) });

        var handler = new GetDriverDashboardQueryHandler(drivers.Object, rides.Object);

        var result = await handler.Handle(new GetDriverDashboardQuery("driver-user"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalEarnings.Should().Be(1800m);
    }
}