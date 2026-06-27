using FluentAssertions;
using FluentValidation.TestHelper;
using Taxi.Application.Drivers.SetAvailability;
using Xunit;

namespace Taxi.Application.Tests.Drivers;

public class SetAvailabilityCommandValidatorTests
{
    private readonly SetAvailabilityCommandValidator _validator = new();

    [Fact]
    public void Online_without_coordinates_should_fail()
    {
        var result = _validator.TestValidate(new SetAvailabilityCommand("u-1", true, null, null));

        result.ShouldHaveValidationErrorFor(c => c.Latitude);
        result.ShouldHaveValidationErrorFor(c => c.Longitude);
    }

    [Fact]
    public void Online_with_valid_coordinates_should_pass()
    {
        var result = _validator.TestValidate(new SetAvailabilityCommand("u-1", true, 11.58, 43.14));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Offline_without_coordinates_should_pass()
    {
        var result = _validator.TestValidate(new SetAvailabilityCommand("u-1", false, null, null));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Online_with_out_of_range_coordinates_should_fail()
    {
        var result = _validator.TestValidate(new SetAvailabilityCommand("u-1", true, 200, -300));

        result.ShouldHaveValidationErrorFor(c => c.Latitude);
        result.ShouldHaveValidationErrorFor(c => c.Longitude);
    }
}
