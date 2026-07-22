using System.Security.Cryptography;
using System.Text;
using Taxi.SharedKernel;

namespace Taxi.Domain.Identity;

/// <summary>
/// Challenge OTP SMS pour vérifier qu'un utilisateur contrôle son numéro de téléphone lors d'un flux sensible.
/// </summary>
public sealed class PhoneOtpChallenge : Entity
{
    private PhoneOtpChallenge() { }

    private PhoneOtpChallenge(string phoneNumber, string purpose, string codeHash, DateTime expiresAt)
    {
        PhoneNumber = phoneNumber;
        Purpose = purpose;
        CodeHash = codeHash;
        ExpiresAt = expiresAt;
    }

    public const string RegistrationPurpose = "Registration";
    public const string PasswordResetPurpose = "PasswordReset";

    public string PhoneNumber { get; private set; } = string.Empty;
    public string Purpose { get; private set; } = RegistrationPurpose;
    public string CodeHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public int FailedAttempts { get; private set; }

    public bool IsExpired(DateTime now) => now >= ExpiresAt;
    public bool IsVerified => VerifiedAt is not null;

    public static PhoneOtpChallenge CreateRegistration(string phoneNumber, string code, DateTime expiresAt)
        => new(phoneNumber, RegistrationPurpose, Hash(phoneNumber, RegistrationPurpose, code), expiresAt);

    public static PhoneOtpChallenge CreatePasswordReset(string phoneNumber, string code, DateTime expiresAt)
        => new(phoneNumber, PasswordResetPurpose, Hash(phoneNumber, PasswordResetPurpose, code), expiresAt);

    public bool Verify(string code, DateTime now, int maxAttempts)
    {
        if (IsVerified || IsExpired(now) || FailedAttempts >= maxAttempts)
            return false;

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(CodeHash),
                Encoding.UTF8.GetBytes(Hash(PhoneNumber, Purpose, code))))
        {
            FailedAttempts++;
            return false;
        }

        VerifiedAt = now;
        return true;
    }

    public static string Hash(string phoneNumber, string purpose, string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{phoneNumber.Trim()}:{purpose.Trim()}:{code.Trim()}"));
        return Convert.ToHexString(bytes);
    }
}