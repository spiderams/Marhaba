using FluentAssertions;
using Moq;
using Taxi.Application.Abstractions;
using Taxi.Application.Pricing.Admin;
using Taxi.Application.Pricing.EstimatePrice;
using Taxi.Domain.Pricing;
using Xunit;

namespace Taxi.Application.Tests.Pricing;

public class ZonePriceAdminHandlersTests
{
    private static Mock<IRepository<ZonePrice>> RepoWith(ZonePrice? existingPair = null, ZonePrice? byId = null)
    {
        var repo = new Mock<IRepository<ZonePrice>>();
        repo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ZonePriceByZonesSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPair);
        repo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(byId);
        return repo;
    }

    [Fact]
    public async Task Create_should_add_a_new_zone_price()
    {
        var repo = RepoWith(existingPair: null);
        var handler = new CreateZonePriceCommandHandler(repo.Object);

        var result = await handler.Handle(new CreateZonePriceCommand("Centre-ville", "Balbala", 1500m), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.FromZone.Should().Be("Centre-ville");
        result.Value.Price.Should().Be(1500m);
        repo.Verify(r => r.AddAsync(It.IsAny<ZonePrice>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_should_fail_when_pair_already_exists()
    {
        var repo = RepoWith(existingPair: ZonePrice.Create("Centre-ville", "Balbala", 1500m));
        var handler = new CreateZonePriceCommandHandler(repo.Object);

        var result = await handler.Handle(new CreateZonePriceCommand("Centre-ville", "Balbala", 1800m), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PricingErrors.DuplicatePair);
        repo.Verify(r => r.AddAsync(It.IsAny<ZonePrice>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_should_change_the_price()
    {
        var existing = ZonePrice.Create("Centre-ville", "Balbala", 1500m);
        var repo = RepoWith(byId: existing);
        var handler = new UpdateZonePriceCommandHandler(repo.Object);

        var result = await handler.Handle(new UpdateZonePriceCommand(1, 2000m), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Price.Should().Be(2000m);
        repo.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_should_fail_when_not_found()
    {
        var repo = RepoWith(byId: null);
        var handler = new UpdateZonePriceCommandHandler(repo.Object);

        var result = await handler.Handle(new UpdateZonePriceCommand(99, 2000m), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PricingErrors.NotFound);
    }

    [Fact]
    public async Task Delete_should_remove_the_zone_price()
    {
        var existing = ZonePrice.Create("Centre-ville", "Balbala", 1500m);
        var repo = RepoWith(byId: existing);
        var handler = new DeleteZonePriceCommandHandler(repo.Object);

        var result = await handler.Handle(new DeleteZonePriceCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repo.Verify(r => r.DeleteAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_should_fail_when_not_found()
    {
        var repo = RepoWith(byId: null);
        var handler = new DeleteZonePriceCommandHandler(repo.Object);

        var result = await handler.Handle(new DeleteZonePriceCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PricingErrors.NotFound);
    }
}
