using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Taxi.Domain.Drivers;
using Taxi.Domain.Rides;
using Taxi.IntegrationTests.Identity;

namespace Taxi.IntegrationTests.Drivers;

public sealed class DriverDashboardEndpointTests(PostgisContainerFixture fixture)
    : IClassFixture<PostgisContainerFixture>
{
    private const string DriverUserId = "driver-user-1";
    private const string TestAuthScheme = "Test";

    private sealed class DriverDashboardApiFactory(PostgisContainerFixture fixture)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:taxidb",
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

    private static Ride CompletedRide(int driverId, decimal estimatedPrice, decimal finalPrice)
    {
        var ride = Ride.Request("client-1", "A", "B", "Z1", "Z2", 11.58, 43.14, 11.60, 43.15, estimatedPrice);
        ride.Accept(driverId);
        ride.MarkArrived();
        ride.Start();
        ride.Complete(finalPrice, PaymentMethod.Cash);
        return ride;
    }

    private static async Task<Driver> SeedDriverAsync(PostgisContainerFixture fixture, string userId = DriverUserId)
    {
        await using var db = fixture.CreateContext();
        var driver = Driver.Create(userId, "LIC-001", "DJ-001", "Taxi");
        driver.Approve();
        db.Drivers.Add(driver);
        await db.SaveChangesAsync();
        return driver;
    }

    private static HttpClient AuthenticatedClient(DriverDashboardApiFactory factory, string userId = DriverUserId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthScheme, userId);
        return client;
    }

    [Fact]
    public async Task GetDriverDashboard_ShouldReturnUnauthorized_WhenJwtIsMissing()
    {
        await fixture.ResetRidesAsync();
        await fixture.ResetDriversAsync();
        await using var factory = new DriverDashboardApiFactory(fixture);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/drivers/me/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDriverDashboard_ShouldReturnNotFound_WhenDriverProfileDoesNotExist()
    {
        await fixture.ResetRidesAsync();
        await fixture.ResetDriversAsync();
        await using var factory = new DriverDashboardApiFactory(fixture);
        using var client = AuthenticatedClient(factory, "missing-driver");

        var response = await client.GetAsync("/api/drivers/me/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDriverDashboard_ShouldReturnRealEarnings_FromCompletedRidesFinalPrice()
    {
        fixture.ConnectionString.Should().NotBeNullOrWhiteSpace();

        await fixture.ResetRidesAsync();
        await fixture.ResetDriversAsync();
        var driver = await SeedDriverAsync(fixture);
        await using (var db = fixture.CreateContext())
        {
            db.Rides.Add(CompletedRide(driver.Id, estimatedPrice: 1000m, finalPrice: 1500m));
            db.Rides.Add(CompletedRide(driver.Id, estimatedPrice: 1000m, finalPrice: 2000m));
            db.Rides.Add(Ride.Request("client-2", "A", "B", "Z1", "Z2", 11.58, 43.14, 11.60, 43.15, 5000m));
            await db.SaveChangesAsync();
        }
        await using var factory = new DriverDashboardApiFactory(fixture);
        using var client = AuthenticatedClient(factory);

        var dashboard = await client.GetFromJsonAsync<DriverDashboardResponse>("/api/drivers/me/dashboard");

        dashboard.Should().NotBeNull();
        dashboard!.DriverId.Should().Be(driver.Id);
        dashboard.CompletedRides.Should().Be(2);
        dashboard.TotalEarnings.Should().Be(3500m);
    }

    [Fact]
    public async Task GetDriverDashboard_ShouldUseFinalPrice_NotEstimatedPrice()
    {
        await fixture.ResetRidesAsync();
        await fixture.ResetDriversAsync();
        var driver = await SeedDriverAsync(fixture);
        await using (var db = fixture.CreateContext())
        {
            db.Rides.Add(CompletedRide(driver.Id, estimatedPrice: 500m, finalPrice: 1800m));
            await db.SaveChangesAsync();
        }
        await using var factory = new DriverDashboardApiFactory(fixture);
        using var client = AuthenticatedClient(factory);

        var dashboard = await client.GetFromJsonAsync<DriverDashboardResponse>("/api/drivers/me/dashboard");

        dashboard.Should().NotBeNull();
        dashboard!.CompletedRides.Should().Be(1);
        dashboard.TotalEarnings.Should().Be(1800m);
    }

    private sealed record DriverDashboardResponse(int DriverId, int CompletedRides, decimal TotalEarnings);
}
