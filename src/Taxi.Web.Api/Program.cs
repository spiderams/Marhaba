using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Taxi.Application;
using Taxi.Infrastructure;
using Taxi.Infrastructure.Identity;
using Taxi.Infrastructure.Persistence;
using Taxi.Infrastructure.Push;
using Taxi.Web.Api.Endpoints;
using Taxi.Web.Api.Middleware;
using Taxi.Web.Api.OpenApi;
using Taxi.Application.Realtime;
using Taxi.Web.Api.Realtime;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Taxi.Web.Api.Modules.Drivers;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi(options =>
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.AddNpgsqlDbContext<AppDbContext>(
    "taxidb",
    configureDbContextOptions: options => options
        .UseNpgsql(npgsql => npgsql.UseNetTopologySuite())
        .UseSnakeCaseNamingConvention()
        // EF Core 10 raises this as an exception during startup migrations.
        // We keep startup migration enabled and log/ignore this warning so local development is not blocked
        // when the database is behind while the committed migration files are being applied.
        .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddPushInfrastructure(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddEndpoints();
builder.Services.AddSignalR();
builder.Services.AddScoped<IRealtimeNotifier, SignalRRealtimeNotifier>();

builder.Services.AddScoped<IRealtimeNotifier, SignalRRealtimeNotifier>();

if (builder.Environment.IsDevelopment() ||
    builder.Environment.IsEnvironment("Testing") ||
    builder.Environment.IsEnvironment("Preview"))
{
    builder.Services.AddSingleton<
        IDriverDocumentMalwareScanner,
        DevelopmentDocumentMalwareScanner>();
}
else
{
    var malwareScannerEndpoint =
        builder.Configuration[
            "DriverDocuments:MalwareScanner:Endpoint"];

    if (string.IsNullOrWhiteSpace(malwareScannerEndpoint))
    {
        throw new InvalidOperationException(
            "DriverDocuments:MalwareScanner:Endpoint doit être configuré hors développement.");
    }

    builder.Services.AddHttpClient<
        IDriverDocumentMalwareScanner,
        HttpDriverDocumentMalwareScanner>();
}

const string DevCorsPolicy = "DevCors";

builder.Services.AddCors(options =>
    options.AddPolicy(
        DevCorsPolicy,
        policy => policy
            .WithOrigins(
                "http://localhost:8081",
                "http://localhost:19006")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));

// IMPORTANT : avant builder.Build()
var port =
    Environment.GetEnvironmentVariable("PORT")
    ?? "5004";

builder.WebHost.UseUrls(
    $"http://0.0.0.0:{port}");

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<SecurityHeadersMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await IdentitySeeder.SeedRolesAsync(scope.ServiceProvider);
}

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment() ||
    app.Environment.IsEnvironment("Preview"))
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseCors(DevCorsPolicy);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();
app.MapHub<RideHub>("/hubs/ride");

app.Run();


/// <summary>Point d'entrée exposé aux tests d'intégration via WebApplicationFactory.</summary>
public partial class Program { }
