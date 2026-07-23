using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Identity.Auth.ForgotPassword;

/// <summary>
/// Demande l'envoi d'un code OTP SMS pour réinitialiser un mot de passe.
/// </summary>
public sealed record RequestPasswordResetCommand(string PhoneNumber) : ICommand<bool>;
