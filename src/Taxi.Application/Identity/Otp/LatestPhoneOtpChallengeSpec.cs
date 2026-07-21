using Ardalis.Specification;
using Taxi.Domain.Identity;

namespace Taxi.Application.Identity.Otp;

public sealed class LatestPhoneOtpChallengeSpec : Specification<PhoneOtpChallenge>
{
    public LatestPhoneOtpChallengeSpec(string phoneNumber)
    {
        Query.Where(c => c.PhoneNumber == phoneNumber)
            .OrderByDescending(c => c.CreatedAt)
            .Take(1);
    }
    public LatestPhoneOtpChallengeSpec(string phoneNumber, string purpose)
    {
        Query.Where(c => c.PhoneNumber == phoneNumber && c.Purpose == purpose)
            .OrderByDescending(c => c.CreatedAt)
            .Take(1);
    }
}
