namespace Taxi.Application.Identity.Otp;

/// <summary>
/// Abstraction applicative d'envoi SMS, implémentée en infrastructure par le fournisseur retenu.
/// </summary>
public interface ISmsSender
{
    Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}
