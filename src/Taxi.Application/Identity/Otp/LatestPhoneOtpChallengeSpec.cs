using Ardalis.Specification;
using Taxi.Domain.Identity;

namespace Taxi.Application.Identity.Otp;

internal sealed class LatestPhoneOtpChallengeSpec : Specification<PhoneOtpChallenge>
{
    public LatestPhoneOtpChallengeSpec(string phoneNumber)
    {
        Query.Where(c => c.PhoneNumber == phoneNumber)
            .OrderByDescending(c => c.CreatedAt)
            .Take(1);
    }
}
