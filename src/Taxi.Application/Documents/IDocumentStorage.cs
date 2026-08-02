namespace Taxi.Application.Documents;

/// <summary>
/// Stockage des pièces justificatives des chauffeurs (permis, carte grise…) sans que la couche Application
/// n'en connaisse l'implémentation. Le chauffeur dépose ses documents ; l'administrateur les consulte
/// via une URL d'accès temporaire pour décider de l'approbation. Implémentée en Infrastructure (Azure Blob).
/// </summary>
public interface IDocumentStorage
{
    /// <summary>Ouvre le contenu via l'API autorisée, sans rendre le conteneur public.</summary>
    Task<DocumentUploadResult> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken cancellationToken = default);

    /// <summary>Ouvre le contenu via l'API autorisée, sans rendre le conteneur public.</summary>
    Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Supprime définitivement un objet remplacé ou arrivé en fin de rétention.</summary>
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}

/// <summary>
/// Résultat d'un dépôt de document : référence de stockage et métadonnées utiles à l'affichage.
/// </summary>
public sealed record DocumentUploadResult(string Key, long Size, string ContentType);

