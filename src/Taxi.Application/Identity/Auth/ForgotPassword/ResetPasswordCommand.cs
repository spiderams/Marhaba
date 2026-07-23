using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Identity.Auth.ForgotPassword;

/// <summary>
/// Réinitialise le mot de passe après validation du code OTP SMS envoyé au téléphone.
/// </summary>
public sealed record ResetPasswordCommand(string PhoneNumber, string OtpCode, string NewPassword) : ICommand<bool>;
