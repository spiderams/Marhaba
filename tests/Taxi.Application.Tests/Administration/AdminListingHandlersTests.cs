using FluentAssertions;
using Moq;
using Taxi.Application.Abstractions;
using Taxi.Application.Administration;
using Taxi.Application.Administration.Listing;
using Taxi.Domain.Drivers;
using Taxi.Domain.Rides;
using Xunit;

namespace Taxi.Application.Tests.Administration;

public class AdminListingHandlersTests
{
    [Fact]
    public async Task GetUsers_returns_directory_list()
    {
        var users = new Mock<IUserDirectory>();
        users.Setup(u => u.ListAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<UserSummary> { new("u-1", "Client Test", "77000002", new[] { "Client" }) });
        var handler = new GetUsersQueryHandler(users.Object);

        var result = await handler.Handle(new GetUsersQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].FullName.Should().Be("Client Test");
    }

    [Fact]
    public async Task GetDrivers_maps_to_dtos()
    {
        var drivers = new Mock<IRepository<Driver>>();
        drivers.Setup(r => r.ListAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(new List<Driver> { Driver.Create("u-1", "LIC", "PLATE", "Taxi") });
        var handler = new GetDriversQueryHandler(drivers.Object);

        var result = await handler.Handle(new GetDriversQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllRides_exposes_cancellation_reason_for_moderation()
    {
        // Une course annulée par le client avec un motif : le support doit voir qui a annulé et pourquoi.
        var ride = Ride.Request("client-1", "A", "B", "Centre-ville", "Balbala", 11.58, 43.14, 11.60, 43.15, 1500m);
        ride.CancelByClient(CancellationReason.TooLongWait, "attente de 20 minutes");

        var rides = new Mock<IRepository<Ride>>();
        rides.Setup(r => r.ListAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Ride> { ride });
        var handler = new GetAllRidesQueryHandler(rides.Object);

        var result = await handler.Handle(new GetAllRidesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].CancelledBy.Should().Be("Client");
        result.Value[0].CancellationReason.Should().Be("TooLongWait");
        result.Value[0].CancellationNote.Should().Be("attente de 20 minutes");
    }
}
