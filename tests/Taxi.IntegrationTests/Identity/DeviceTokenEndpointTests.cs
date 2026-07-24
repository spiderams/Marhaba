using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Taxi.Domain.Identity;

namespace Taxi.IntegrationTests.Identity;

public sealed class DeviceTokenEndpointTests(PostgisContainerFixture fixture)
    : IClassFixture<PostgisContainerFixture>
{
    private const string TestAuthScheme = "Test";

    private sealed class DeviceTokenApiFactory(PostgisContainerFixture fixture)
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
                !string.Equals(header.Scheme, TestAuthScheme, StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(header.Parameter))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, header.Parameter!),
                new Claim(ClaimTypes.Role, RoleNames.Driver)
            };
            var identity = new ClaimsIdentity(claims, TestAuthScheme);
            var principal = new ClaimsPrincipal(identity);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, TestAuthScheme)));
        }
    }

    private static HttpClient AuthenticatedClient(DeviceTokenApiFactory factory, string userId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthScheme, userId);
        return client;
    }

    private static async Task<ApplicationUser> SeedUserAsync(PostgisContainerFixture fixture, string? userId = null)
    {
        var id = userId ?? Guid.NewGuid().ToString("N");
        var phoneNumber = $"+25377{id[..8]}";
        var user = new ApplicationUser
        {
            Id = id,
            UserName = phoneNumber,
            NormalizedUserName = phoneNumber,
            PhoneNumber = phoneNumber,
            PhoneNumberConfirmed = true,
            FullName = "Driver Push",
            SecurityStamp = Guid.NewGuid().ToString("N")
        };

        await using var db = fixture.CreateContext();
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task PutDeviceToken_ShouldReturnUnauthorized_WhenJwtIsMissing()
    {
        await using var factory = new DeviceTokenApiFactory(fixture);
        using var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync("/api/auth/device-token", new { deviceToken = "fcm-device-token" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PutDeviceToken_ShouldPersistDeviceToken_ForAuthenticatedUser()
    {
        var user = await SeedUserAsync(fixture);
        await using var factory = new DeviceTokenApiFactory(fixture);
        using var client = AuthenticatedClient(factory, user.Id);

        var response = await client.PutAsJsonAsync("/api/auth/device-token", new { deviceToken = "  fcm-device-token  " });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var db = fixture.CreateContext();
        var persisted = await db.Users.SingleAsync(u => u.Id == user.Id);
        persisted.DeviceToken.Should().Be("fcm-device-token");
    }

    [Fact]
    public async Task PutDeviceToken_ShouldReturnNotFound_WhenAuthenticatedUserDoesNotExist()
    {
        await using var factory = new DeviceTokenApiFactory(fixture);
        using var client = AuthenticatedClient(factory, Guid.NewGuid().ToString("N"));

        var response = await client.PutAsJsonAsync("/api/auth/device-token", new { deviceToken = "fcm-device-token" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PutDeviceToken_ShouldReturnBadRequest_WhenDeviceTokenIsEmpty()
    {
        var user = await SeedUserAsync(fixture);
        await using var factory = new DeviceTokenApiFactory(fixture);
        using var client = AuthenticatedClient(factory, user.Id);

        var response = await client.PutAsJsonAsync("/api/auth/device-token", new { deviceToken = string.Empty });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
