using Taxi.Application.Documents;

namespace Taxi.Infrastructure.Documents;

/// <summary>Stockage local privé, adapté au développement et remplaçable par Azure Blob.</summary>
public sealed class LocalDocumentStorage : IDocumentStorage
{
    private readonly string _root = Path.Combine(AppContext.BaseDirectory, "private-driver-documents");

    public async Task<DocumentUploadResult> UploadAsync(Stream content, string fileName,
        string contentType, string? folder = null, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(Path.GetFileName(fileName));
        var safeFolder = string.Join('/', (folder ?? string.Empty).Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(segment => Path.GetFileName(segment)));
        var key = $"{safeFolder}/{Guid.NewGuid():N}{extension}".TrimStart('/');
        var target = Resolve(key);
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        await using var output = File.Create(target);
        await content.CopyToAsync(output, cancellationToken);
        return new DocumentUploadResult(key, output.Length, contentType);
    }   

    public Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default)
    {
        Stream stream = File.OpenRead(Resolve(key));
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        File.Delete(Resolve(key));
        return Task.CompletedTask;
    }

    private string Resolve(string key)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_root, key.Replace('/', Path.DirectorySeparatorChar)));
        if (!fullPath.StartsWith(Path.GetFullPath(_root), StringComparison.Ordinal))
            throw new InvalidOperationException("Clé de document invalide.");
        return fullPath;
    }
}