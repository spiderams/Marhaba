using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Taxi.Application.Documents;

namespace Taxi.Infrastructure.Documents;

public sealed class AzureBlobStorageOptions
{
    public const string SectionName = "DriverDocuments:AzureBlob";
    public string AccountName { get; init; } = string.Empty;
    public string AccountKey { get; init; } = string.Empty;
    public string Container { get; init; } = "driver-documents";
    public string? EncryptionScope { get; init; }
}

/// <summary>
/// Stockage Azure Blob privé sans URL publique. Les lectures passent toujours par
/// l'API autorisée. Azure chiffre les blobs au repos ; un EncryptionScope peut être
/// imposé pour utiliser une clé gérée par le client.
/// </summary>
public sealed class AzureBlobDocumentStorage(
    HttpClient httpClient,
    IOptions<AzureBlobStorageOptions> options) : IDocumentStorage
{
    private const string ApiVersion = "2023-11-03";
    private readonly AzureBlobStorageOptions _options = options.Value;

    public async Task<DocumentUploadResult> UploadAsync(Stream content, string fileName,
        string contentType, string? folder = null, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(Path.GetFileName(fileName)).ToLowerInvariant();
        var safeFolder = string.Join('/', (folder ?? string.Empty)
            .Split('/', StringSplitOptions.RemoveEmptyEntries).Select(Path.GetFileName));
        var key = $"{safeFolder}/{Guid.NewGuid():N}{extension}".TrimStart('/');
        await using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();

        using var request = CreateRequest(HttpMethod.Put, key, bytes.Length, contentType);
        request.Headers.Add("x-ms-blob-type", "BlockBlob");
        if (!string.IsNullOrWhiteSpace(_options.EncryptionScope))
            request.Headers.Add("x-ms-encryption-scope", _options.EncryptionScope);
        request.Content = new ByteArrayContent(bytes);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        request.Content.Headers.ContentLength = bytes.Length;
        Sign(request, bytes.Length, contentType);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return new DocumentUploadResult(key, bytes.LongLength, contentType);
    }

    public async Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, key);
        Sign(request);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = new MemoryStream();
        await response.Content.CopyToAsync(result, cancellationToken);
        result.Position = 0;
        return result;
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Delete, key);
        Sign(request);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
            response.EnsureSuccessStatusCode();
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string key, long? length = null,
        string? contentType = null)
    {
        var request = new HttpRequestMessage(method, BuildUri(key));
        request.Headers.Add("x-ms-date", DateTimeOffset.UtcNow.ToString("R", CultureInfo.InvariantCulture));
        request.Headers.Add("x-ms-version", ApiVersion);
        return request;
    }

    private Uri BuildUri(string key)
    {
        var encoded = string.Join('/', key.Split('/').Select(Uri.EscapeDataString));
        return new Uri($"https://{_options.AccountName}.blob.core.windows.net/{_options.Container}/{encoded}");
    }

    private void Sign(HttpRequestMessage request, long? contentLength = null, string? contentType = null)
    {
        var xmsHeaders = request.Headers
            .Where(header => header.Key.StartsWith("x-ms-", StringComparison.OrdinalIgnoreCase))
            .OrderBy(header => header.Key, StringComparer.OrdinalIgnoreCase)
            .Select(header => $"{header.Key.ToLowerInvariant()}:{string.Join(",", header.Value)}\n");
        var canonicalHeaders = string.Concat(xmsHeaders);
        var canonicalResource = $"/{_options.AccountName}/{_options.Container}/{Uri.UnescapeDataString(request.RequestUri!.AbsolutePath.Split('/', 3)[2])}";
        var stringToSign = $"{request.Method.Method}\n\n\n{contentLength?.ToString(CultureInfo.InvariantCulture) ?? string.Empty}\n\n{contentType ?? string.Empty}\n\n\n\n\n\n\n{canonicalHeaders}{canonicalResource}";
        using var hmac = new HMACSHA256(Convert.FromBase64String(_options.AccountKey));
        var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
        request.Headers.Authorization = new AuthenticationHeaderValue("SharedKey", $"{_options.AccountName}:{signature}");
    }
}
