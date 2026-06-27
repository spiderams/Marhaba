using FluentAssertions;
using FluentValidation.TestHelper;
using Taxi.Application.Drivers.UpdateLocation;
using Xunit;

namespace Taxi.Application.Tests.Drivers;

public class UpdateMyLocationCommandValidatorTests
{
    private readonly UpdateMyLocationCommandValidator _validator = new();

    [Fact]
    public void Valid_coordinates_should_pass()
    {
        var result = _validator.TestValidate(new UpdateMyLocationCommand("u-1", 11.58, 43.14));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Out_of_range_coordinates_should_fail()
    {
        var result = _validator.TestValidate(new UpdateMyLocationCommand("u-1", 200, -300));

        result.ShouldHaveValidationErrorFor(c => c.Latitude);
        result.ShouldHaveValidationErrorFor(c => c.Longitude);
    }
}
