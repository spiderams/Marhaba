using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Taxi.Application.Abstractions;
using Taxi.Application.Dispatch;
using Taxi.Application.Identity.Otp;
using Taxi.Infrastructure.Dispatch;
using Taxi.Infrastructure.Persistence;
using Taxi.Infrastructure.Sms;

namespace Taxi.Infrastructure;

/// <summary>
/// Enregistrement des services d'infrastructure dans le conteneur DI.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IDriverLocator, DriverLocator>();
        services.AddScoped<ISmsSender, LoggingSmsSender>();
        services.AddHostedService<OfferTimeoutService>();
        return services;
    }
}
