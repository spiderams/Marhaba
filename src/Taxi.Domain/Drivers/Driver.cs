using NetTopologySuite.Geometries;
using Taxi.SharedKernel;

namespace Taxi.Domain.Drivers;

/// <summary>
/// Agrégat représentant un chauffeur enregistré dans la plateforme : regroupe son profil (véhicule, numéro de licence),
/// sa disponibilité, sa position géographique en temps réel et sa note moyenne calculée à partir des évaluations clients.
/// </summary>
public sealed class Driver : Entity
{
    public string UserId { get; private set; } = string.Empty;
    public string LicenseNumber { get; private set; } = string.Empty;
    public string VehiclePlate { get; private set; } = string.Empty;
    public string VehicleType { get; private set; } = "Taxi";
    public bool IsAvailable { get; private set; }
    public DriverApprovalStatus ApprovalStatus { get; private set; } = DriverApprovalStatus.PendingApproval;
    public DriverStatus Status { get; private set; }
    public double AverageRating { get; private set; }
    public Point? LastLocation { get; private set; }
    public DateTime? LastLocationAt { get; private set; }

    public double? LastLatitude => LastLocation?.Y;
    public double? LastLongitude => LastLocation?.X;
    public bool CanReceiveRides => ApprovalStatus == DriverApprovalStatus.Approved && IsAvailable;

    /// <summary>
    /// Garde métier d'éligibilité au dispatch : un chauffeur ne peut recevoir de courses que s'il est
    /// à la fois approuvé par un administrateur (<see cref="DriverStatus.Approved"/>) et disponible.
    /// Source de vérité unique pour cette règle, afin qu'elle ne soit pas dupliquée dans le dispatch ou ailleurs.
    /// </summary>
    public bool CanReceiveRides => Status == DriverStatus.Approved && IsAvailable;

    private Driver() { } // EF

    /// <summary>
    /// Crée un nouveau profil chauffeur associé à un utilisateur identité existant.
    /// Le chauffeur naît en <see cref="DriverStatus.PendingApproval"/> : il ne pourra
    /// recevoir de courses qu'après approbation explicite d'un administrateur (KYC).
    /// </summary>
    public static Driver Create(string userId, string licenseNumber, string vehiclePlate, string vehicleType)
        => new()
        {
            UserId = userId,
            LicenseNumber = licenseNumber,
            VehiclePlate = vehiclePlate,
            VehicleType = vehicleType,
            Status = DriverStatus.PendingApproval
        };

    /// <summary>
    /// Approbation par un administrateur : autorise le chauffeur à recevoir des courses.
    /// Applicable à un chauffeur en attente de validation (<see cref="DriverStatus.PendingApproval"/>)
    /// ou à un chauffeur suspendu que l'on réactive (<see cref="DriverStatus.Suspended"/>).
    /// Échoue si le chauffeur est déjà approuvé ou définitivement rejeté.
    /// </summary>
    public Result Approve()
    {
        if (Status is not (DriverStatus.PendingApproval or DriverStatus.Suspended))
            return Result.Failure(DriverErrors.InvalidStatusTransition);

        Status = DriverStatus.Approved;
        return Result.Success();
    }

    /// <summary>
    /// Suspension par un administrateur : le chauffeur cesse immédiatement de recevoir des offres.
    /// Applicable uniquement à un chauffeur actuellement approuvé (<see cref="DriverStatus.Approved"/>).
    /// </summary>
    public Result Suspend()
    {
        if (Status != DriverStatus.Approved)
            return Result.Failure(DriverErrors.InvalidStatusTransition);

        Status = DriverStatus.Suspended;
        return Result.Success();
    }

    /// <summary>
    /// Rejet définitif d'une candidature par un administrateur : statut terminal.
    /// Applicable uniquement à un chauffeur en attente de validation (<see cref="DriverStatus.PendingApproval"/>).
    /// </summary>
    public Result Reject()
    {
        if (Status != DriverStatus.PendingApproval)
            return Result.Failure(DriverErrors.InvalidStatusTransition);

        Status = DriverStatus.Rejected;
        return Result.Success();
    }

    /// <summary>
    /// Met à jour les informations du véhicule et de la licence du chauffeur.
    /// </summary>
    public void UpdateProfile(string licenseNumber, string vehiclePlate, string vehicleType)
    {
        LicenseNumber = licenseNumber;
        VehiclePlate = vehiclePlate;
        VehicleType = vehicleType;
    }

    /// <summary>
    /// Bascule la disponibilité du chauffeur : seul un chauffeur disponible peut recevoir de nouvelles courses.
    /// Utilisé par le cycle de vie des courses pour marquer le chauffeur occupé (false) puis libre (true).
    /// </summary>
    public void SetAvailability(bool isAvailable) => IsAvailable = isAvailable;

    /// <summary>
    /// Action manuelle du chauffeur qui se met en ligne : capte sa position GPS courante (indispensable
    /// pour être éligible au dispatch de proximité) et le rend disponible. Retourne un <see cref="Result"/>
    /// afin de pouvoir, à terme, refuser la mise en ligne d'un chauffeur non approuvé ou suspendu.
    /// </summary>
    public Result GoOnline(double latitude, double longitude)
    {
        UpdateLocation(latitude, longitude);
        IsAvailable = true;
        return Result.Success();
    }

    /// <summary>
    /// Action manuelle du chauffeur qui se met hors-ligne : il cesse de recevoir de nouvelles offres.
    /// </summary>
    public void GoOffline() => IsAvailable = false;

    /// <summary>
    /// Recalcule et enregistre la note moyenne du chauffeur après chaque nouvelle évaluation client.
    /// </summary>
    public void UpdateAverageRating(double average) => AverageRating = average;

    /// <summary>
    /// Enregistre la position GPS courante du chauffeur en coordonnées WGS-84 (SRID 4326),
    /// utilisée pour le suivi temps réel et l'affectation des courses à proximité.
    /// </summary>
    public void UpdateLocation(double latitude, double longitude)
    {
        LastLocation = new Point(longitude, latitude) { SRID = 4326 };
        LastLocationAt = DateTime.UtcNow;
    }
    public Result Approve()
    {
        if (ApprovalStatus == DriverApprovalStatus.Rejected)
            return Result.Failure(Error.Conflict("Driver.Rejected", "Un chauffeur rejeté ne peut pas être approuvé."));

        ApprovalStatus = DriverApprovalStatus.Approved;
        return Result.Success();
    }

    public Result Suspend()
    {
        if (ApprovalStatus != DriverApprovalStatus.Approved)
            return Result.Failure(Error.Conflict("Driver.NotApproved", "Seul un chauffeur approuvé peut être suspendu."));

        ApprovalStatus = DriverApprovalStatus.Suspended;
        IsAvailable = false;
        return Result.Success();
    }

    public Result Reject()
    {
        if (ApprovalStatus == DriverApprovalStatus.Approved)
            return Result.Failure(Error.Conflict("Driver.Approved", "Un chauffeur approuvé doit être suspendu plutôt que rejeté."));

        ApprovalStatus = DriverApprovalStatus.Rejected;
        IsAvailable = false;
        return Result.Success();
    }
}
