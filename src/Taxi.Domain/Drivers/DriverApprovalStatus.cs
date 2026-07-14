namespace Taxi.Domain.Drivers;

/// <summary>
/// Statut de validation métier d'un chauffeur avant qu'il puisse recevoir des courses.
/// </summary>
public enum DriverApprovalStatus
{
    PendingApproval = 0,
    Approved = 1,
    Suspended = 2,
    Rejected = 3
}
