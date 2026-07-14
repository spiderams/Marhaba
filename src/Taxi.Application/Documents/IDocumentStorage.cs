namespace Taxi.Application.Documents;

/// <summary>
/// Stockage des pièces justificatives des chauffeurs (permis, carte grise…) sans que la couche Application
/// n'en connaisse l'implémentation. Le chauffeur dépose ses documents ; l'administrateur les consulte
/// via une URL d'accès temporaire pour décider de l'approbation. Implémentée en Infrastructure (Azure Blob).
/// </summary>
public interface IDocumentStorage
{
    /// <summary>
    /// Enregistre un document et retourne sa référence de stockage (clé) ainsi que ses métadonnées.
    /// </summary>
    /// <param name="content">Contenu binaire du document.</param>
    /// <param name="fileName">Nom d'origine du fichier (sert à déterminer l'extension).</param>
    /// <param name="contentType">Type MIME (ex. <c>image/jpeg</c>, <c>application/pdf</c>).</param>
    /// <param name="folder">Dossier logique de rangement (ex. <c>drivers/{driverId}</c>), facultatif.</param>
    Task<DocumentUploadResult> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Génère une URL d'accès en lecture, temporaire et signée, vers un document existant identifié par
    /// sa <paramref name="key"/>. L'admin ouvre ce lien directement ; il expire après <paramref name="expiry"/>.
    /// </summary>
    Uri GetReadUrl(string key, TimeSpan expiry);
}

/// <summary>
/// Résultat d'un dépôt de document : référence de stockage et métadonnées utiles à l'affichage.
/// </summary>
public sealed record DocumentUploadResult(string Key, long Size, string ContentType);
