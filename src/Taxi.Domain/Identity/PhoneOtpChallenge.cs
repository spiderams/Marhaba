using System.Security.Cryptography;
using System.Text;
using Taxi.SharedKernel;

namespace Taxi.Domain.Identity;

/// <summary>
/// Challenge OTP SMS pour vérifier qu'un utilisateur contrôle son numéro de téléphone avant l'inscription.
/// </summary>
public sealed class PhoneOtpChallenge : Entity
{
    private PhoneOtpChallenge() { }

    private PhoneOtpChallenge(string phoneNumber, string codeHash, DateTime expiresAt)
    {
        PhoneNumber = phoneNumber;
        CodeHash = codeHash;
        ExpiresAt = expiresAt;
    }

    public string PhoneNumber { get; private set; } = string.Empty;
    public string CodeHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public int FailedAttempts { get; private set; }

    public bool IsExpired(DateTime now) => now >= ExpiresAt;
    public bool IsVerified => VerifiedAt is not null;

    public static PhoneOtpChallenge Create(string phoneNumber, string code, DateTime expiresAt)
        => new(phoneNumber, Hash(phoneNumber, code), expiresAt);

    public bool Verify(string code, DateTime now, int maxAttempts)
    {
        if (IsVerified || IsExpired(now) || FailedAttempts >= maxAttempts)
            return false;

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(CodeHash),
                Encoding.UTF8.GetBytes(Hash(PhoneNumber, code))))
        {
            FailedAttempts++;
            return false;
        }

        VerifiedAt = now;
        return true;
    }

    public static string Hash(string phoneNumber, string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{phoneNumber.Trim()}:{code.Trim()}"));
        return Convert.ToHexString(bytes);
    }
}
