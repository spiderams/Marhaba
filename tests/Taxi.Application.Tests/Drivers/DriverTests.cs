using FluentAssertions;
using Taxi.Domain.Drivers;
using Xunit;

namespace Taxi.Application.Tests.Drivers;

public class DriverTests
{
    [Fact]
    public void Create_should_set_fields_with_defaults()
    {
        var driver = Driver.Create("u-1", "LIC-001", "DJ-1234", "Taxi");

        driver.UserId.Should().Be("u-1");
        driver.LicenseNumber.Should().Be("LIC-001");
        driver.VehiclePlate.Should().Be("DJ-1234");
        driver.VehicleType.Should().Be("Taxi");
        driver.IsAvailable.Should().BeFalse();
        driver.ApprovalStatus.Should().Be(DriverApprovalStatus.PendingApproval);
        driver.CanReceiveRides.Should().BeFalse();
        driver.AverageRating.Should().Be(0);
        driver.Status.Should().Be(DriverStatus.PendingApproval);
    }

    [Fact]
    public void UpdateProfile_should_change_profile_fields()
    {
        var driver = Driver.Create("u-1", "LIC-001", "DJ-1234", "Taxi");

        driver.UpdateProfile("LIC-002", "DJ-9999", "Minibus");

        driver.LicenseNumber.Should().Be("LIC-002");
        driver.VehiclePlate.Should().Be("DJ-9999");
        driver.VehicleType.Should().Be("Minibus");
    }

    [Fact]
    public void SetAvailability_should_toggle_availability()
    {
        var driver = Driver.Create("u-1", "LIC-001", "DJ-1234", "Taxi");

        driver.SetAvailability(true);
        driver.IsAvailable.Should().BeTrue();

        driver.SetAvailability(false);
        driver.IsAvailable.Should().BeFalse();
    }
    [Fact]
    public void Approve_should_allow_available_driver_to_receive_rides()
    {
        var driver = Driver.Create("u-1", "LIC-001", "DJ-1234", "Taxi");
        driver.SetAvailability(true);

        var result = driver.Approve();

        result.IsSuccess.Should().BeTrue();
        driver.ApprovalStatus.Should().Be(DriverApprovalStatus.Approved);
        driver.CanReceiveRides.Should().BeTrue();
    }

    [Fact]
    public void Suspend_should_make_driver_unavailable_and_block_ride_reception()
    {
        var driver = Driver.Create("u-1", "LIC-001", "DJ-1234", "Taxi");
        driver.SetAvailability(true);
        driver.Approve();

        var result = driver.Suspend();

        result.IsSuccess.Should().BeTrue();
        driver.ApprovalStatus.Should().Be(DriverApprovalStatus.Suspended);
        driver.IsAvailable.Should().BeFalse();
        driver.CanReceiveRides.Should().BeFalse();
    }
    [Fact]
    public void UpdateAverageRating_sets_the_average()
    {
        var driver = Driver.Create("u-1", "LIC", "PLATE", "Taxi");
        driver.UpdateAverageRating(4.5);
        driver.AverageRating.Should().Be(4.5);
    }

    [Fact]
    public void GoOnline_should_set_position_and_make_available()
    {
        var driver = Driver.Create("u-1", "LIC-001", "DJ-1234", "Taxi");

        var result = driver.GoOnline(11.58, 43.14);

        result.IsSuccess.Should().BeTrue();
        driver.IsAvailable.Should().BeTrue();
        driver.LastLatitude.Should().Be(11.58);
        driver.LastLongitude.Should().Be(43.14);
        driver.LastLocationAt.Should().NotBeNull();
    }

    [Fact]
    public void GoOffline_should_clear_availability()
    {
        var driver = Driver.Create("u-1", "LIC-001", "DJ-1234", "Taxi");
        driver.GoOnline(11.58, 43.14);

        driver.GoOffline();

        driver.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void Approve_should_move_pending_driver_to_approved()
    {
        var driver = Driver.Create("u-1", "LIC-001", "DJ-1234", "Taxi");

        var result = driver.Approve();

        result.IsSuccess.Should().BeTrue();
        driver.Status.Should().Be(DriverStatus.Approved);
    }

    [Fact]
    public void Approve_should_reactivate_a_suspended_driver()
    {
        var driver = Driver.Create("u-1", "LIC-001", "DJ-1234", "Taxi");
        driver.Approve();
        driver.Suspend();

        var result = driver.Approve();

        result.IsSuccess.Should().BeTrue();
        driver.Status.Should().Be(DriverStatus.Approved);
    }

    [Fact]
    public void Approve_should_fail_when_already_approved()
    {
        var driver = Driver.Create("u-1", "LIC-001", "DJ-1234", "Taxi");
        driver.Approve();

        var result = driver.Approve();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DriverErrors.InvalidStatusTransition);
        driver.Status.Should().Be(DriverStatus.Approved);
    }

    [Fact]
    public void Approve_should_fail_when_driver_is_rejected()
    {
        var driver = Driver.Create("u-1", "LIC-001", "DJ-1234", "Taxi");
        driver.Reject();

        var result = driver.Approve();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DriverErrors.InvalidStatusTransition);
        driver.Status.Should().Be(DriverStatus.Rejected);
    }

    [Fact]
    public void Suspend_should_move_approved_driver_to_suspended()
    {
        var driver = Driver.Create("u-1", "LIC-001", "DJ-1234", "Taxi");
        driver.Approve();

        var result = driver.Suspend();

        result.IsSuccess.Should().BeTrue();
        driver.Status.Should().Be(DriverStatus.Suspended);
    }

    [Fact]
    public void Suspend_should_fail_when_driver_is_not_approved()
    {
        var driver = Driver.Create("u-1", "LIC-001", "DJ-1234", "Taxi");

        var result = driver.Suspend();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DriverErrors.InvalidStatusTransition);
        driver.Status.Should().Be(DriverStatus.PendingApproval);
    }

    [Fact]
    public void Reject_should_move_pending_driver_to_rejected()
    {
        var driver = Driver.Create("u-1", "LIC-001", "DJ-1234", "Taxi");

        var result = driver.Reject();

        result.IsSuccess.Should().BeTrue();
        driver.Status.Should().Be(DriverStatus.Rejected);
    }

    [Fact]
    public void Reject_should_fail_when_driver_is_already_approved()
    {
        var driver = Driver.Create("u-1", "LIC-001", "DJ-1234", "Taxi");
        driver.Approve();

        var result = driver.Reject();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DriverErrors.InvalidStatusTransition);
        driver.Status.Should().Be(DriverStatus.Approved);
    }

    [Fact]
    public void CanReceiveRides_is_true_when_approved_and_available()
    {
        var driver = Driver.Create("u-1", "LIC-001", "DJ-1234", "Taxi");
        driver.Approve();
        driver.SetAvailability(true);

        driver.CanReceiveRides.Should().BeTrue();
    }

    [Fact]
    public void CanReceiveRides_is_false_when_approved_but_unavailable()
    {
        var driver = Driver.Create("u-1", "LIC-001", "DJ-1234", "Taxi");
        driver.Approve();
        driver.SetAvailability(false);

        driver.CanReceiveRides.Should().BeFalse();
    }

    [Fact]
    public void CanReceiveRides_is_false_when_available_but_not_approved()
    {
        var driver = Driver.Create("u-1", "LIC-001", "DJ-1234", "Taxi");
        driver.SetAvailability(true); // reste PendingApproval

        driver.CanReceiveRides.Should().BeFalse();
    }

    [Fact]
    public void CanReceiveRides_is_false_when_suspended_even_if_available()
    {
        var driver = Driver.Create("u-1", "LIC-001", "DJ-1234", "Taxi");
        driver.Approve();
        driver.Suspend();
        driver.SetAvailability(true);

        driver.CanReceiveRides.Should().BeFalse();
    }
}
