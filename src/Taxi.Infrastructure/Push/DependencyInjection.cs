using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using Taxi.Application.Realtime;

namespace Taxi.Infrastructure.Push;

/// <summary>
/// Enregistrement des services de notification push (FCM) dans le conteneur DI :
/// client Refit typé vers l'API FCM, fournisseur de jeton OAuth2 et implémentation d'<see cref="IPushNotifier"/>.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddPushInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FcmSettings>(configuration.GetSection(FcmSettings.SectionName));

        var settings = configuration.GetSection(FcmSettings.SectionName).Get<FcmSettings>() ?? new FcmSettings();

        services.AddRefitClient<IFcmApi>()
            .ConfigureHttpClient(http => http.BaseAddress = new Uri(settings.BaseUrl));

        // Fournisseur de jeton FCM : compte de service Google si des credentials sont configurés,
        // sinon repli sur le stub non configuré (le projet démarre en local sans clé, le push est alors inopérant).
        if (!string.IsNullOrWhiteSpace(settings.CredentialsPath))
            services.AddSingleton<IFcmTokenProvider, GoogleFcmTokenProvider>();
        else
            services.AddSingleton<IFcmTokenProvider, NotConfiguredFcmTokenProvider>();

        services.AddScoped<IPushNotifier, FcmPushNotifier>();

        return services;
    }
}
