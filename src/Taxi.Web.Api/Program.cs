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
if (!string.IsNullOrWhiteSpace(builder.Configuration["DriverDocuments:MalwareScanner:Endpoint"]))
    builder.Services.AddHttpClient<IDriverDocumentMalwareScanner, HttpDriverDocumentMalwareScanner>();
else if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
    builder.Services.AddSingleton<IDriverDocumentMalwareScanner, DevelopmentDocumentMalwareScanner>();
else
    throw new InvalidOperationException("DriverDocuments:MalwareScanner:Endpoint doit être configuré hors développement.");

// CORS de développement : autorise l'app mobile Expo (mode web) à appeler l'API
// et le hub SignalR. AllowCredentials est requis par SignalR et interdit le
// wildcard "*", d'où la liste explicite des origines de développement.
const string DevCorsPolicy = "DevCors";
builder.Services.AddCors(options =>
    options.AddPolicy(DevCorsPolicy, policy => policy
        .WithOrigins(
            "http://localhost:8081",  // Expo web (Metro)
            "http://localhost:19006") // Expo web (ancien port)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

var app = builder.Build();
builder.WebHost.UseUrls("http://0.0.0.0:5004");

app.UseExceptionHandler();
app.UseMiddleware<SecurityHeadersMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await IdentitySeeder.SeedRolesAsync(scope.ServiceProvider);
}

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
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