using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Taxi.Domain.Drivers;
using Taxi.Domain.Rides;
using Taxi.IntegrationTests.Identity;

namespace Taxi.IntegrationTests.Rides;

public sealed class RideTransitionEndpointTests(PostgisContainerFixture fixture)
    : IClassFixture<PostgisContainerFixture>
{
    private const string DriverUserId = "driver-user-ride-cycle";
    private const string TestAuthScheme = "Test";

    private sealed class RideTransitionApiFactory(PostgisContainerFixture fixture)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting(
                 "ConnectionStrings:taxidb",
                 fixture.ConnectionString);

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
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, "Driver")
            };
            var identity = new ClaimsIdentity(claims, TestAuthScheme);
            var principal = new ClaimsPrincipal(identity);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, TestAuthScheme)));
        }
    }

    private static HttpClient AuthenticatedClient(RideTransitionApiFactory factory, string userId = DriverUserId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthScheme, userId);
        return client;
    }

    private static async Task<(Driver Driver, Ride Ride)> SeedAcceptedRideAsync(
        PostgisContainerFixture fixture,
        string driverUserId = DriverUserId)
    {
        await using var db = fixture.CreateContext();
        var driver = Driver.Create(driverUserId, "LIC-TR-001", "DJ-TR-001", "Taxi");
        driver.Approve();
        driver.SetAvailability(false);
        db.Drivers.Add(driver);
        await db.SaveChangesAsync();

        var ride = Ride.Request("client-1", "A", "B", "Z1", "Z2", 11.58, 43.14, 11.60, 43.15, 1000m);
        ride.Accept(driver.Id);
        db.Rides.Add(ride);
        await db.SaveChangesAsync();

        return (driver, ride);
    }

    [Fact]
    public async Task RideCycle_ShouldMarkArrived_Start_AndComplete_WithFinalPrice()
    {
        await fixture.ResetRidesAsync();
        await fixture.ResetDriversAsync();
        var (_, ride) = await SeedAcceptedRideAsync(fixture);
        await using var factory = new RideTransitionApiFactory(fixture);
        using var client = AuthenticatedClient(factory);

        var arrived = await client.PostAsync($"/api/rides/{ride.Id}/arrived", content: null);
        var started = await client.PostAsync($"/api/rides/{ride.Id}/start", content: null);
        var completed = await client.PostAsJsonAsync($"/api/rides/{ride.Id}/complete", new
        {
            finalPrice = 1750m,
            paymentMethod = PaymentMethod.Cash
        });

        arrived.StatusCode.Should().Be(HttpStatusCode.OK);
        started.StatusCode.Should().Be(HttpStatusCode.OK);
        completed.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await completed.Content.ReadFromJsonAsync<RideResponse>();
        body.Should().NotBeNull();
        body!.Status.Should().Be(nameof(RideStatus.Completed));
        body.FinalPrice.Should().Be(1750m);
        body.PaymentMethod.Should().Be(nameof(PaymentMethod.Cash));

        await using var db = fixture.CreateContext();
        var persistedRide = await db.Rides.SingleAsync(r => r.Id == ride.Id);
        var persistedDriver = await db.Drivers.SingleAsync(d => d.Id == persistedRide.DriverId);
        persistedRide.Status.Should().Be(RideStatus.Completed);
        persistedRide.FinalPrice.Should().Be(1750m);
        persistedRide.PaymentMethod.Should().Be(PaymentMethod.Cash);
        persistedRide.CompletedAt.Should().NotBeNull();
        persistedDriver.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task RideCycle_ShouldReturnUnauthorized_WhenDriverJwtIsMissing()
    {
        await fixture.ResetRidesAsync();
        await fixture.ResetDriversAsync();
        var (_, ride) = await SeedAcceptedRideAsync(fixture);
        await using var factory = new RideTransitionApiFactory(fixture);
        using var client = factory.CreateClient();

        var response = await client.PostAsync($"/api/rides/{ride.Id}/arrived", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Complete_ShouldReject_WhenRideHasNotStarted()
    {
        await fixture.ResetRidesAsync();
        await fixture.ResetDriversAsync();
        var (_, ride) = await SeedAcceptedRideAsync(fixture);
        await using var factory = new RideTransitionApiFactory(fixture);
        using var client = AuthenticatedClient(factory);

        var response = await client.PostAsJsonAsync($"/api/rides/{ride.Id}/complete", new
        {
            finalPrice = 1750m,
            paymentMethod = PaymentMethod.Cash
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await using var db = fixture.CreateContext();
        var persistedRide = await db.Rides.SingleAsync(r => r.Id == ride.Id);
        persistedRide.Status.Should().Be(RideStatus.Accepted);
        persistedRide.FinalPrice.Should().BeNull();
    }

    [Fact]
    public async Task Complete_ShouldReturnForbidden_WhenRideIsAssignedToAnotherDriver()
    {
        await fixture.ResetRidesAsync();
        await fixture.ResetDriversAsync();
        var (_, ride) = await SeedAcceptedRideAsync(fixture);
        await SeedAcceptedRideAsync(fixture, driverUserId: "other-driver-user");
        await using (var db = fixture.CreateContext())
        {
            var persistedRide = await db.Rides.SingleAsync(r => r.Id == ride.Id);
            persistedRide.MarkArrived();
            persistedRide.Start();
            await db.SaveChangesAsync();
        }
        await using var factory = new RideTransitionApiFactory(fixture);
        using var client = AuthenticatedClient(factory, "other-driver-user");

        var response = await client.PostAsJsonAsync($"/api/rides/{ride.Id}/complete", new
        {
            finalPrice = 1750m,
            paymentMethod = PaymentMethod.Cash
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private sealed record RideResponse(
        int Id,
        decimal EstimatedPrice,
        decimal? FinalPrice,
        string? PaymentMethod,
        string Status);
}
