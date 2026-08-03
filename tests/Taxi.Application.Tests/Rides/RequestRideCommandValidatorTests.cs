using FluentValidation.TestHelper;
using Taxi.Application.Rides.Request;
using Xunit;

namespace Taxi.Application.Tests.Rides;

public sealed class RequestRideCommandValidatorTests
{
    private readonly RequestRideCommandValidator _validator = new();

    [Fact]
    public void Request_without_pickup_coordinates_should_fail()
    {
        var command = new RequestRideCommand(
            "client-1", "Départ", "Destination", "Zone A", "Zone B",
            null, null, null, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.PickupLatitude);
        result.ShouldHaveValidationErrorFor(c => c.PickupLongitude);
    }

    [Fact]
    public void Request_with_canadian_pickup_coordinates_should_pass()
    {
        var command = new RequestRideCommand(
            "client-1", "Montréal", "Laval", "Zone A", "Zone B",
            45.5019, -73.5674, 45.6066, -73.7124);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}