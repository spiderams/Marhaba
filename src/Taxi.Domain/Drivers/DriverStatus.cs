namespace Taxi.Domain.Drivers;

/// <summary>
/// Statut d'approbation d'un chauffeur dans le processus de vérification (KYC) :
/// détermine s'il est autorisé à recevoir des courses. Un chauffeur nouvellement
/// inscrit est <see cref="PendingApproval"/> et ne peut être dispatché tant qu'un
/// administrateur ne l'a pas approuvé.
/// </summary>
public enum DriverStatus { PendingApproval, Approved, Suspended, Rejected }
