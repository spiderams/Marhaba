using Microsoft.AspNetCore.Identity;

namespace Taxi.Domain.Identity;

/// <summary>
/// Utilisateur de la plateforme TaxiDjibouti : étend IdentityUser d'ASP.NET Core Identity
/// pour y ajouter le nom complet et la date d'inscription, communes à tous les rôles (Client, Chauffeur, Admin).
/// </summary>
public sealed class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Jeton d'appareil FCM du dernier appareil enregistré par l'utilisateur, utilisé pour lui envoyer
    /// des notifications push (offres de course, app fermée). <c>null</c> tant qu'aucun appareil n'a été
    /// enregistré. Un utilisateur = un appareil dans le périmètre MVP.
    /// </summary>
    public string? DeviceToken { get; set; }
}
