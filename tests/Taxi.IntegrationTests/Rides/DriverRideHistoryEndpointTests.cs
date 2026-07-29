using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Taxi.Application.Rides;
using Taxi.Domain.Drivers;
using Taxi.Domain.Rides;

namespace Taxi.IntegrationTests.Rides;

public sealed class DriverRideHistoryEndpointTests(PostgisContainerFixture fixture)
    : IClassFixture<PostgisContainerFixture>
{
    private const string DriverUserId = "history-driver-user";
    private const string TestAuthScheme = "Test";

    private sealed class DriverRideHistoryApiFactory(PostgisContainerFixture fixture)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:taxidb"] = fixture.ConnectionString
                });
            });

            builder.ConfigureServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthScheme;
                    options.DefaultChallengeScheme = TestAuthScheme;
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthScheme, _ => { });
            });
        }
    }

    private sealed class TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authorization) ||
                !AuthenticationHeaderValue.TryParse(authorization, out var header) ||
                !string.Equals(header.Scheme, TestAuthScheme, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var userId = string.IsNullOrWhiteSpace(header.Parameter) ? DriverUserId : header.Parameter;
            var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, "Driver")
            ], TestAuthScheme);
            return Task.FromResult(AuthenticateResult.Success(
                new AuthenticationTicket(new ClaimsPrincipal(identity), TestAuthScheme)));
        }
    }

    [Fact]
    public async Task GetHistory_ShouldReturnUnauthorized_WhenJwtIsMissing()
    {
        await ResetAsync();
        await using var factory = new DriverRideHistoryApiFactory(fixture);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/drivers/me/rides/history");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetHistory_ShouldReturnCompletedRides_ForAuthenticatedDriver()
    {
        await ResetAsync();
        var driver = await SeedDriverAsync(DriverUserId, "DJ-101");
        await SeedRidesAsync(CreateCompletedRide(driver.Id, "Place Menelik", 1500m));
        await using var factory = new DriverRideHistoryApiFactory(fixture);
        using var client = AuthenticatedClient(factory);

        var history = await client.GetFromJsonAsync<List<RideDto>>("/api/drivers/me/rides/history");

        history.Should().ContainSingle();
        history![0].DriverId.Should().Be(driver.Id);
        history[0].Status.Should().Be(nameof(RideStatus.Completed));
    }

    [Fact]
    public async Task GetHistory_ShouldNotReturnOtherDriverRides()
    {
        await ResetAsync();
        var currentDriver = await SeedDriverAsync(DriverUserId, "DJ-102");
        var otherDriver = await SeedDriverAsync("other-driver-user", "DJ-103");
        await SeedRidesAsync(
            CreateCompletedRide(currentDriver.Id, "Place Menelik", 1500m),
            CreateCompletedRide(otherDriver.Id, "Haramous", 2500m));
        await using var factory = new DriverRideHistoryApiFactory(fixture);
        using var client = AuthenticatedClient(factory);

        var history = await client.GetFromJsonAsync<List<RideDto>>("/api/drivers/me/rides/history");

        history.Should().ContainSingle();
        history![0].DriverId.Should().Be(currentDriver.Id);
        history[0].PickupAddress.Should().Be("Place Menelik");
    }

    [Fact]
    public async Task GetHistory_ShouldExcludeActiveRides()
    {
        await ResetAsync();
        var driver = await SeedDriverAsync(DriverUserId, "DJ-104");
        var activeRide = Ride.Request("client-active", "Balbala", "Centre-ville", "Balbala", "Centre",
            11.57, 43.09, 11.59, 43.14, 900m);
        activeRide.Accept(driver.Id);
        await SeedRidesAsync(CreateCompletedRide(driver.Id, "Place Menelik", 1500m), activeRide);
        await using var factory = new DriverRideHistoryApiFactory(fixture);
        using var client = AuthenticatedClient(factory);

        var history = await client.GetFromJsonAsync<List<RideDto>>("/api/drivers/me/rides/history");

        history.Should().ContainSingle();
        history![0].Status.Should().Be(nameof(RideStatus.Completed));
    }

    [Fact]
    public async Task GetHistory_ShouldReturnFinalPriceAndPaymentMethod()
    {
        await ResetAsync();
        var driver = await SeedDriverAsync(DriverUserId, "DJ-105");
        await SeedRidesAsync(CreateCompletedRide(driver.Id, "Place Menelik", 1800m, estimatedPrice: 500m));
        await using var factory = new DriverRideHistoryApiFactory(fixture);
        using var client = AuthenticatedClient(factory);

        var history = await client.GetFromJsonAsync<List<RideDto>>("/api/drivers/me/rides/history");

        history.Should().ContainSingle();
        history![0].EstimatedPrice.Should().Be(500m);
        history[0].FinalPrice.Should().Be(1800m);
        history[0].PaymentMethod.Should().Be(nameof(PaymentMethod.Cash));
    }

    private async Task ResetAsync()
    {
        await fixture.ResetRidesAsync();
        await fixture.ResetDriversAsync();
    }

    private async Task<Driver> SeedDriverAsync(string userId, string vehicleRegistration)
    {
        await using var db = fixture.CreateContext();
        var driver = Driver.Create(userId, $"LIC-{vehicleRegistration}", vehicleRegistration, "Taxi");
        driver.Approve();
        db.Drivers.Add(driver);
        await db.SaveChangesAsync();
        return driver;
    }

    private async Task SeedRidesAsync(params Ride[] rides)
    {
        await using var db = fixture.CreateContext();
        db.Rides.AddRange(rides);
        await db.SaveChangesAsync();
    }

    private static Ride CreateCompletedRide(
        int driverId,
        string pickupAddress,
        decimal finalPrice,
        decimal estimatedPrice = 1000m)
    {
        var ride = Ride.Request("client-history", pickupAddress, "Aéroport", "Centre", "Aéroport",
            11.59, 43.14, 11.55, 43.16, estimatedPrice);
        ride.Accept(driverId);
        ride.MarkArrived();
        ride.Start();
        ride.Complete(finalPrice, PaymentMethod.Cash);
        return ride;
    }

    private static HttpClient AuthenticatedClient(DriverRideHistoryApiFactory factory)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthScheme, DriverUserId);
        return client;
    }
}
