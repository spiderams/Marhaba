using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Taxi.Application.Abstractions;
using Taxi.Application.Dispatch;
using Taxi.Application.Documents;
using Taxi.Application.Identity.Otp;
using Taxi.Infrastructure.Dispatch;
using Taxi.Infrastructure.Documents;
using Taxi.Infrastructure.Persistence;
using Taxi.Infrastructure.Sms;

/// <summary>
/// Enregistrement des services d'infrastructure dans le conteneur DI.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration,IHostEnvironment hostEnvironment)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IDriverLocator, DriverLocator>();
        services.AddScoped<ISmsSender, LoggingSmsSender>();
        services.AddHostedService<OfferTimeoutService>();
        var azureSection = configuration.GetSection(AzureBlobStorageOptions.SectionName);
        if (!string.IsNullOrWhiteSpace(azureSection["AccountName"]) &&
            !string.IsNullOrWhiteSpace(azureSection["AccountKey"]))
        {
            services.Configure<AzureBlobStorageOptions>(azureSection);
            services.AddHttpClient<AzureBlobDocumentStorage>();
            services.AddTransient<IDocumentStorage>(provider =>
                provider.GetRequiredService<AzureBlobDocumentStorage>());
        }
        else if (hostEnvironment.IsDevelopment() || hostEnvironment.IsEnvironment("Testing"))
        {
            services.AddSingleton<IDocumentStorage, LocalDocumentStorage>();
        }
        else
        {
            throw new InvalidOperationException(
                "DriverDocuments:AzureBlob doit être configuré hors développement ; le stockage local est interdit.");
        }
        return services;
    }
}
