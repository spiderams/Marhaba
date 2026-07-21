using FluentAssertions;
using Xunit;
using Taxi.Domain.Identity;

namespace Taxi.Application.Tests.Identity;

public sealed class PhoneOtpChallengeTests
{
    [Fact]
    public void Verify_Accepts_Correct_Code_Before_Expiration()
    {
        var now = DateTime.UtcNow;
        var challenge = PhoneOtpChallenge.Create("+25377123456", "123456", now.AddMinutes(5));

        var verified = challenge.Verify("123456", now, maxAttempts: 5);

        verified.Should().BeTrue();
        challenge.IsVerified.Should().BeTrue();
    }

    [Fact]
    public void Verify_Rejects_Wrong_Code_And_Counts_Attempt()
    {
        var now = DateTime.UtcNow;
        var challenge = PhoneOtpChallenge.Create("+25377123456", "123456", now.AddMinutes(5));

        var verified = challenge.Verify("000000", now, maxAttempts: 5);

        verified.Should().BeFalse();
        challenge.FailedAttempts.Should().Be(1);
        challenge.IsVerified.Should().BeFalse();
    }

    [Fact]
    public void Verify_Rejects_Expired_Code()
    {
        var now = DateTime.UtcNow;
        var challenge = PhoneOtpChallenge.Create("+25377123456", "123456", now.AddMinutes(-1));

        var verified = challenge.Verify("123456", now, maxAttempts: 5);

        verified.Should().BeFalse();
        challenge.IsVerified.Should().BeFalse();
    }
}
