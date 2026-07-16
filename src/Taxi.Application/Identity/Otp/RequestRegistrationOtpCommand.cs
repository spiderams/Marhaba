using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Identity.Otp;

/// <summary>
/// Demande l'envoi d'un OTP SMS court pour valider le numéro avant inscription.
/// </summary>
public sealed record RequestRegistrationOtpCommand(string PhoneNumber) : ICommand<bool>;
