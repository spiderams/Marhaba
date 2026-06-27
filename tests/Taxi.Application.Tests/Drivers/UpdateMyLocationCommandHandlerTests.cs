using Ardalis.Specification;
using FluentAssertions;
using Moq;
using Taxi.Application.Abstractions;
using Taxi.Application.Drivers.UpdateLocation;
using Taxi.Domain.Drivers;
using Taxi.SharedKernel;
using Xunit;

namespace Taxi.Application.Tests.Drivers;

public class UpdateMyLocationCommandHandlerTests
{
    private readonly Mock<IRepository<Driver>> _repo = new();

    private UpdateMyLocationCommandHandler Handler() => new(_repo.Object);

    [Fact]
    public async Task Should_refresh_position_without_ride()
    {
        var driver = Driver.Create("u-1", "LIC", "PLATE", "Taxi");
        _repo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Driver>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(driver);

        var result = await Handler().Handle(new UpdateMyLocationCommand("u-1", 11.58, 43.14), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        driver.LastLatitude.Should().Be(11.58);
        driver.LastLongitude.Should().Be(43.14);
        driver.LastLocationAt.Should().NotBeNull();
        _repo.Verify(r => r.UpdateAsync(driver, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_fail_notfound_when_driver_absent()
    {
        _repo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Driver>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Driver?)null);

        var result = await Handler().Handle(new UpdateMyLocationCommand("u-x", 11.58, 43.14), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }
}
