using System.Security.Claims;
using Taxi.Application.Abstractions;
using Taxi.Application.Documents;
using Taxi.Application.Drivers;
using Taxi.Domain.Drivers;
using Taxi.Domain.Identity;
using Taxi.Web.Api.Endpoints;

namespace Taxi.Web.Api.Modules.Drivers;

/// <summary>
/// Résumé d’un document Chauffeur.
///
/// Le contenu et la clé privée de stockage ne sont jamais exposés
/// dans cette réponse.
/// </summary>
public sealed record DriverDocumentDto(
    string Type,
    bool Uploaded);

/// <summary>
/// Endpoints sécurisés de gestion des documents KYC Chauffeur.
///
/// Fonctionnalités :
/// - consultation des documents déjà transmis par le Chauffeur ;
/// - upload ou remplacement d’un justificatif ;
/// - vérification de la taille et du type MIME ;
/// - vérification de la signature binaire réelle du fichier ;
/// - contrôle antivirus ;
/// - stockage privé ;
/// - suppression de l’ancien document après remplacement réussi ;
/// - téléchargement sécurisé par un administrateur ;
/// - journalisation des opérations sensibles.
/// </summary>
public sealed class DriverDocumentEndpoints : IEndpoint
{
    /// <summary>
    /// Taille maximale autorisée : 10 Mo.
    /// </summary>
    private const long MaxFileSize =
        10 * 1024 * 1024;

    /// <summary>
    /// Types MIME acceptés pour les justificatifs KYC.
    ///
    /// La vérification du MIME ne suffit pas :
    /// DriverDocumentSecurity vérifie également les octets
    /// caractéristiques du fichier.
    /// </summary>
    private static readonly HashSet<string>
        AllowedContentTypes =
        [
            "image/jpeg",
            "image/png",
            "application/pdf",
        ];

    public void MapEndpoint(
        IEndpointRouteBuilder app)
    {
        /*
         * Groupe réservé au Chauffeur connecté.
         */
        var driverGroup = app
            .MapGroup(
                "/api/drivers/me/documents")
            .RequireAuthorization(
                policy =>
                    policy.RequireRole(
                        RoleNames.Driver))
            .WithTags(Tags.Drivers);

        /*
         * GET /api/drivers/me/documents
         *
         * Retourne la liste des catégories de documents
         * ainsi que leur état de transmission.
         */
        driverGroup.MapGet(
            string.Empty,
            async (
                ClaimsPrincipal principal,
                IRepository<Driver> drivers,
                CancellationToken ct) =>
            {
                var driver =
                    await GetDriver(
                        principal,
                        drivers,
                        ct);

                if (driver is null)
                {
                    return Results.NotFound(
                        new
                        {
                            message =
                                "Profil chauffeur introuvable.",
                        });
                }

                return Results.Ok(
                    Summaries(driver));
            })
            .WithName(
                "GetMyDriverDocuments")
            .WithSummary(
                "Liste les documents du Chauffeur connecté");

        /*
         * POST /api/drivers/me/documents/{type}
         *
         * Exemples :
         * - /api/drivers/me/documents/License
         * - /api/drivers/me/documents/VehicleRegistration
         * - /api/drivers/me/documents/Identity
         */
        driverGroup.MapPost(
            "/{type}",
            async (
                string type,
                IFormFile file,
                ClaimsPrincipal principal,
                IRepository<Driver> drivers,
                IDocumentStorage storage,
                IDriverDocumentMalwareScanner
                    malwareScanner,
                ILogger<DriverDocumentEndpoints>
                    logger,
                CancellationToken ct) =>
            {
                /*
                 * Validation du type métier.
                 */
                if (!Enum.TryParse<
                        DriverDocumentType>(
                        type,
                        ignoreCase: true,
                        out var documentType))
                {
                    return Results.BadRequest(
                        new
                        {
                            message =
                                "Type de document inconnu.",
                        });
                }

                /*
                 * Validation préliminaire :
                 * - fichier non vide ;
                 * - maximum 10 Mo ;
                 * - MIME autorisé.
                 */
                if (
                    file.Length is <= 0 or > MaxFileSize ||
                    !AllowedContentTypes.Contains(
                        file.ContentType))
                {
                    return Results.BadRequest(
                        new
                        {
                            message =
                                "Utilisez une image JPEG/PNG " +
                                "ou un PDF de 10 Mo maximum.",
                        });
                }

                /*
                 * Copie du fichier dans un buffer limité.
                 *
                 * La taille a déjà été contrôlée avant cette copie.
                 * Le buffer est ensuite réutilisé pour :
                 * - la signature binaire ;
                 * - l’antivirus ;
                 * - le stockage.
                 */
                await using var uploadedContent =
                    new MemoryStream(
                        capacity: checked(
                            (int)file.Length));

                await file.CopyToAsync(
                    uploadedContent,
                    ct);

                var bytes =
                    uploadedContent
                        .GetBuffer()
                        .AsMemory(
                            0,
                            checked(
                                (int)uploadedContent
                                    .Length));

                /*
                 * Vérification du contenu réel.
                 *
                 * Cette étape empêche, par exemple, un exécutable
                 * renommé en ".pdf" d’être accepté uniquement parce
                 * que le client a envoyé "application/pdf".
                 */
                if (
                    !DriverDocumentSecurity
                        .HasValidSignature(
                            bytes.Span,
                            file.ContentType))
                {
                    logger.LogWarning(
                        "KYC signature rejected: " +
                        "UserId={UserId}, " +
                        "Type={DocumentType}, " +
                        "ContentType={ContentType}",
                        principal.GetUserId(),
                        documentType,
                        file.ContentType);

                    return Results.BadRequest(
                        new
                        {
                            message =
                                "Le contenu réel du fichier " +
                                "ne correspond pas au format annoncé.",
                        });
                }

                /*
                 * Contrôle antivirus.
                 *
                 * En production, l’implémentation HTTP doit appeler
                 * le service antivirus configuré.
                 */
                var isClean =
                    await malwareScanner
                        .IsCleanAsync(
                            bytes,
                            file.ContentType,
                            ct);

                if (!isClean)
                {
                    logger.LogWarning(
                        "KYC malware rejected: " +
                        "UserId={UserId}, " +
                        "Type={DocumentType}",
                        principal.GetUserId(),
                        documentType);

                    return Results.BadRequest(
                        new
                        {
                            message =
                                "Le document a été refusé " +
                                "par le contrôle de sécurité.",
                        });
                }

                /*
                 * Retour au début du flux avant l’upload.
                 */
                uploadedContent.Position = 0;

                /*
                 * Recherche du profil Chauffeur authentifié.
                 */
                var driver =
                    await GetDriver(
                        principal,
                        drivers,
                        ct);

                if (driver is null)
                {
                    return Results.NotFound(
                        new
                        {
                            message =
                                "Profil chauffeur introuvable.",
                        });
                }

                /*
                 * Conservation de l’ancienne clé.
                 *
                 * L’ancien objet ne doit être supprimé qu’après :
                 * 1. le nouvel upload ;
                 * 2. la mise à jour réussie du Chauffeur.
                 */
                var previousKey =
                    driver.GetDocumentKey(
                        documentType);

                /*
                 * Upload du nouveau document.
                 *
                 * Le nom final est généré par le stockage.
                 * Le nom fourni par le téléphone n’est pas utilisé
                 * comme chemin définitif.
                 */
                var uploaded =
                    await storage.UploadAsync(
                        uploadedContent,
                        file.FileName,
                        file.ContentType,
                        $"drivers/{driver.Id}/{documentType}",
                        ct);

                /*
                 * Enregistrement de la nouvelle clé.
                 *
                 * Si le dossier avait été rejeté, SetDocument
                 * le replace automatiquement en PendingApproval.
                 */
                driver.SetDocument(
                    documentType,
                    uploaded.Key);

                await drivers.UpdateAsync(
                    driver,
                    ct);

                /*
                 * Suppression de l’ancien fichier seulement après
                 * la persistance de la nouvelle clé.
                 */
                if (previousKey is not null)
                {
                    try
                    {
                        await storage.DeleteAsync(
                            previousKey,
                            ct);
                    }
                    catch (Exception deletionError)
                    {
                        /*
                         * L’upload reste valide.
                         *
                         * Une suppression en erreur est journalisée
                         * afin qu’un traitement de nettoyage puisse
                         * supprimer ultérieurement l’objet orphelin.
                         */
                        logger.LogError(
                            deletionError,
                            "KYC previous document deletion failed: " +
                            "DriverId={DriverId}, " +
                            "Type={DocumentType}, " +
                            "PreviousKey={PreviousKey}",
                            driver.Id,
                            documentType,
                            previousKey);
                    }
                }

                /*
                 * Journal d’audit structuré.
                 */
                logger.LogInformation(
                    "KYC document uploaded: " +
                    "DriverId={DriverId}, " +
                    "Type={DocumentType}, " +
                    "Key={StorageKey}, " +
                    "Replaced={Replaced}, " +
                    "Size={FileSize}, " +
                    "ContentType={ContentType}",
                    driver.Id,
                    documentType,
                    uploaded.Key,
                    previousKey is not null,
                    uploaded.Size,
                    uploaded.ContentType);

                return Results.Ok(
                    new DriverDocumentDto(
                        documentType.ToString(),
                        Uploaded: true));
            })
            /*
             * L’endpoint utilise l’authentification Bearer et reçoit
             * un multipart/form-data depuis l’application mobile.
             */
            .DisableAntiforgery()
            .WithName(
                "UploadMyDriverDocument")
            .WithSummary(
                "Envoie ou remplace un document Chauffeur");

        /*
         * GET /api/admin/drivers/{driverId}/documents/{type}
         *
         * Endpoint réservé aux administrateurs.
         *
         * Le conteneur de documents reste privé :
         * le flux traverse l’API après vérification du rôle Admin.
         */
        app.MapGet(
            "/api/admin/drivers/{driverId:int}/documents/{type}",
            async (
                int driverId,
                string type,
                ClaimsPrincipal principal,
                IRepository<Driver> drivers,
                IDocumentStorage storage,
                ILogger<DriverDocumentEndpoints>
                    logger,
                CancellationToken ct) =>
            {
                /*
                 * Validation du type demandé.
                 */
                if (!Enum.TryParse<
                        DriverDocumentType>(
                        type,
                        ignoreCase: true,
                        out var documentType))
                {
                    return Results.BadRequest(
                        new
                        {
                            message =
                                "Type de document inconnu.",
                        });
                }

                /*
                 * Recherche du Chauffeur puis de la clé privée.
                 */
                var driver =
                    await drivers
                        .FirstOrDefaultAsync(
                            new DriverByIdSpec(
                                driverId),
                            ct);

                if (driver is null)
                {
                    return Results.NotFound(
                        new
                        {
                            message =
                                "Profil chauffeur introuvable.",
                        });
                }

                var key =
                    driver.GetDocumentKey(
                        documentType);

                if (key is null)
                {
                    return Results.NotFound(
                        new
                        {
                            message =
                                "Ce document n'a pas encore été envoyé.",
                        });
                }

                /*
                 * Lecture depuis le stockage privé.
                 */
                Stream stream;

                try
                {
                    stream =
                        await storage.OpenReadAsync(
                            key,
                            ct);
                }
                catch (FileNotFoundException)
                {
                    logger.LogWarning(
                        "KYC storage object missing: " +
                        "DriverId={DriverId}, " +
                        "Type={DocumentType}, " +
                        "Key={StorageKey}",
                        driverId,
                        documentType,
                        key);

                    return Results.NotFound(
                        new
                        {
                            message =
                                "Le document est référencé " +
                                "mais son fichier est introuvable.",
                        });
                }

                /*
                 * Journal d’audit de la consultation.
                 */
                logger.LogInformation(
                    "KYC document accessed: " +
                    "DriverId={DriverId}, " +
                    "Type={DocumentType}, " +
                    "AdminUserId={AdminUserId}",
                    driverId,
                    documentType,
                    principal.GetUserId());

                /*
                 * Le type exact n’est pas conservé dans Driver.
                 * Le téléchargement utilise donc le type générique
                 * application/octet-stream.
                 */
                return Results.File(
                    stream,
                    contentType:
                        "application/octet-stream",
                    fileDownloadName:
                        $"driver-{driverId}-{documentType}",
                    enableRangeProcessing: true);
            })
            .RequireAuthorization(
                policy =>
                    policy.RequireRole(
                        RoleNames.Admin))
            .WithTags(Tags.Admin)
            .WithName(
                "DownloadDriverDocument")
            .WithSummary(
                "Télécharge un document KYC Chauffeur");
    }

    /// <summary>
    /// Retourne le profil Chauffeur lié à l’utilisateur
    /// authentifié.
    /// </summary>
    private static async Task<Driver?>
        GetDriver(
            ClaimsPrincipal principal,
            IRepository<Driver> drivers,
            CancellationToken ct)
    {
        var userId =
            principal.GetUserId();

        if (string.IsNullOrWhiteSpace(
                userId))
        {
            return null;
        }

        return await drivers
            .FirstOrDefaultAsync(
                new DriverByUserIdSpec(
                    userId),
                ct);
    }

    /// <summary>
    /// Produit la liste des documents attendus avec leur
    /// état de transmission.
    /// </summary>
    private static IReadOnlyList<
        DriverDocumentDto> Summaries(
            Driver driver)
    {
        return Enum
            .GetValues<DriverDocumentType>()
            .Select(type =>
                new DriverDocumentDto(
                    type.ToString(),
                    driver.GetDocumentKey(type)
                        is not null))
            .ToList();
    }
}