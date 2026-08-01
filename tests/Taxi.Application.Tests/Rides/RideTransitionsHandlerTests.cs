using Ardalis.Specification;
using FluentAssertions;
using Moq;
using Taxi.Application.Abstractions;
using Taxi.Application.Realtime;
using Taxi.Application.Rides.Transitions;
using Taxi.Domain.Drivers;
using Taxi.Domain.Rides;
using Xunit;

namespace Taxi.Application.Tests.Rides;

public class RideTransitionsHandlerTests
{
    private readonly Mock<IRepository<Ride>> _rides = new();
    private readonly Mock<IRepository<Driver>> _drivers = new();
    private readonly Mock<IRealtimeNotifier> _notifier = new();

    private static Driver DriverWithId(int id)
    {
        var d = Driver.Create("driver-user", "LIC", "PLATE", "Taxi");
        typeof(Taxi.SharedKernel.Entity).GetProperty("Id")!.SetValue(d, id);
        d.SetAvailability(true);
        return d;
    }

    private static Ride AcceptedRide(int driverId)
    {
        var r = Ride.Request("c", "A", "B", "Z1", "Z2", null, null, null, null, 1000m);
        r.Accept(driverId);
        return r;
    }

    [Fact]
    public async Task MarkArrived_should_succeed_for_assigned_driver()
    {
        _drivers.Setup(d => d.FirstOrDefaultAsync(It.IsAny<ISpecification<Driver>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DriverWithId(7));
        _rides.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Ride>>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(AcceptedRide(7));
        var handler = new MarkArrivedCommandHandler(_rides.Object, _drivers.Object, _notifier.Object);

        var result = await handler.Handle(new MarkArrivedCommand(1, "driver-user"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("DriverArrived");
        _notifier.Verify(n => n.RideStatusChangedAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int?>(), "DriverArrived", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkArrived_should_forbid_when_not_assigned_driver()
    {
        _drivers.Setup(d => d.FirstOrDefaultAsync(It.IsAny<ISpecification<Driver>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DriverWithId(9)); // different driver
        _rides.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Ride>>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(AcceptedRide(7));
        var handler = new MarkArrivedCommandHandler(_rides.Object, _drivers.Object, _notifier.Object);

        var result = await handler.Handle(new MarkArrivedCommand(1, "driver-user"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RideErrors.NotAssignedDriver);
    }
    [Fact]
    public async Task Full_driver_execution_cycle_should_persist_each_transition_and_restore_availability()
    {
        var driver = DriverWithId(7);
        driver.SetAvailability(false);
        var ride = AcceptedRide(7);

        _drivers.Setup(d => d.FirstOrDefaultAsync(It.IsAny<ISpecification<Driver>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(driver);
        _rides.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Ride>>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(ride);

        var arrived = new MarkArrivedCommandHandler(_rides.Object, _drivers.Object, _notifier.Object);
        var started = new StartRideCommandHandler(_rides.Object, _drivers.Object, _notifier.Object);
        var completed = new CompleteRideCommandHandler(_rides.Object, _drivers.Object, _notifier.Object);

        (await arrived.Handle(new MarkArrivedCommand(ride.Id, "driver-user"), CancellationToken.None))
            .Value.Status.Should().Be("DriverArrived");
        (await started.Handle(new StartRideCommand(ride.Id, "driver-user"), CancellationToken.None))
            .Value.Status.Should().Be("InProgress");
        var result = await completed.Handle(
            new CompleteRideCommand(ride.Id, "driver-user", 1250m, PaymentMethod.DMoney),
            CancellationToken.None);

        result.Value.Status.Should().Be("Completed");
        ride.FinalPrice.Should().Be(1250m);
        ride.PaymentMethod.Should().Be(PaymentMethod.DMoney);
        driver.IsAvailable.Should().BeTrue();
        _rides.Verify(r => r.UpdateAsync(ride, It.IsAny<CancellationToken>()), Times.Exactly(3));
        _drivers.Verify(d => d.UpdateAsync(driver, It.IsAny<CancellationToken>()), Times.Once);
        _notifier.Verify(n => n.RideStatusChangedAsync(
            ride.Id, ride.ClientId, ride.DriverId, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }
}
