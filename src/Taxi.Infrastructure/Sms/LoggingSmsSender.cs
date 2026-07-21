using Microsoft.Extensions.Logging;
using Taxi.Application.Identity.Otp;

namespace Taxi.Infrastructure.Sms;

/// <summary>
/// Implémentation SMS de démarrage : journalise les SMS en attendant le choix Djibouti Telecom/agrégateur.
/// </summary>
internal sealed partial class LoggingSmsSender(ILogger<LoggingSmsSender> logger) : ISmsSender
{
    public Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        LogSmsQueued(logger, phoneNumber, message);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "SMS OTP prêt pour {PhoneNumber}: {Message}")]
    private static partial void LogSmsQueued(ILogger logger, string phoneNumber, string message);
}
