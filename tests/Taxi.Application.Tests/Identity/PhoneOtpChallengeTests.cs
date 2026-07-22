using FluentAssertions;
using Xunit;
using Taxi.Domain.Identity;

namespace Taxi.Application.Tests.Identity;

public sealed class PhoneOtpChallengeTests
{

    private static readonly DateTime Now = new(2026, 7, 21, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CreateRegistration_ShouldSetRegistrationPurpose_AndStoreHash_NotPlainCode()
    {
        var challenge = PhoneOtpChallenge.CreateRegistration("+25377123456", "123456", Now.AddMinutes(5));

        challenge.PhoneNumber.Should().Be("+25377123456");
        challenge.Purpose.Should().Be(PhoneOtpChallenge.RegistrationPurpose);
        challenge.CodeHash.Should().NotBeNullOrWhiteSpace();
        challenge.CodeHash.Should().NotBe("123456");
        challenge.ExpiresAt.Should().Be(Now.AddMinutes(5));
        challenge.IsVerified.Should().BeFalse();
        challenge.FailedAttempts.Should().Be(0);
    }

    [Fact]
    public void CreatePasswordReset_ShouldSetPasswordResetPurpose_AndStoreHash_NotPlainCode()
    {
        var challenge = PhoneOtpChallenge.CreatePasswordReset("+25377123456", "123456", Now.AddMinutes(5));

        challenge.PhoneNumber.Should().Be("+25377123456");
        challenge.Purpose.Should().Be(PhoneOtpChallenge.PasswordResetPurpose);
        challenge.CodeHash.Should().NotBeNullOrWhiteSpace();
        challenge.CodeHash.Should().NotBe("123456");
        challenge.ExpiresAt.Should().Be(Now.AddMinutes(5));
    }
    [Fact]
    public void Verify_Accepts_Correct_Code_Before_Expiration()
    {

        var challenge = PhoneOtpChallenge.CreateRegistration("+25377123456", "123456", Now.AddMinutes(5));
        var verified = challenge.Verify("123456", Now, maxAttempts: 5);


        verified.Should().BeTrue();
        challenge.IsVerified.Should().BeTrue();
        challenge.VerifiedAt.Should().Be(Now);
        challenge.FailedAttempts.Should().Be(0);
    }

    [Fact]
    public void Verify_Rejects_Wrong_Code_And_Counts_Attempt()
    {
        var challenge = PhoneOtpChallenge.CreateRegistration("+25377123456", "123456", Now.AddMinutes(5));


        var verified = challenge.Verify("000000", Now, maxAttempts: 5);

        verified.Should().BeFalse();
        challenge.FailedAttempts.Should().Be(1);
        challenge.IsVerified.Should().BeFalse();
        challenge.VerifiedAt.Should().BeNull();
    }

    [Fact]
    public void Verify_Rejects_Expired_Code()
    {
        var challenge = PhoneOtpChallenge.CreateRegistration("+25377123456", "123456", Now.AddMinutes(-1));

        var verified = challenge.Verify("123456", Now, maxAttempts: 5);

        verified.Should().BeFalse();
        challenge.IsVerified.Should().BeFalse();
        challenge.FailedAttempts.Should().Be(0);
        challenge.VerifiedAt.Should().BeNull();
    }
    [Fact]
    public void Verify_Rejects_When_Max_Attempts_Reached()
    {
        var challenge = PhoneOtpChallenge.CreateRegistration("+25377123456", "123456", Now.AddMinutes(5));

        challenge.Verify("000000", Now, maxAttempts: 2).Should().BeFalse();
        challenge.Verify("111111", Now, maxAttempts: 2).Should().BeFalse();
        var verifiedAfterLimit = challenge.Verify("123456", Now, maxAttempts: 2);

        verifiedAfterLimit.Should().BeFalse();
        challenge.FailedAttempts.Should().Be(2);
        challenge.IsVerified.Should().BeFalse();
    }

    [Fact]
    public void Verify_Rejects_When_Already_Verified()
    {
        var challenge = PhoneOtpChallenge.CreateRegistration("+25377123456", "123456", Now.AddMinutes(5));

        challenge.Verify("123456", Now, maxAttempts: 5).Should().BeTrue();
        var secondVerification = challenge.Verify("123456", Now.AddSeconds(1), maxAttempts: 5);

        secondVerification.Should().BeFalse();
        challenge.IsVerified.Should().BeTrue();
        challenge.VerifiedAt.Should().Be(Now);
    }

    [Fact]
    public void Registration_And_Password_Reset_Codes_Have_Different_Hashes()
    {
      
        var expiresAt = Now.AddMinutes(5);

        var registration = PhoneOtpChallenge.CreateRegistration("+25377123456", "123456", expiresAt);
        var passwordReset = PhoneOtpChallenge.CreatePasswordReset("+25377123456", "123456", expiresAt);

        registration.Purpose.Should().Be(PhoneOtpChallenge.RegistrationPurpose);
        passwordReset.Purpose.Should().Be(PhoneOtpChallenge.PasswordResetPurpose);
        registration.CodeHash.Should().NotBe(passwordReset.CodeHash);
    }
    [Fact]
    public void Hash_ShouldNormalize_Whitespace_Around_Inputs()
    {
        var trimmed = PhoneOtpChallenge.Hash("+25377123456", PhoneOtpChallenge.RegistrationPurpose, "123456");
        var padded = PhoneOtpChallenge.Hash("  +25377123456  ", $"  {PhoneOtpChallenge.RegistrationPurpose}  ", "  123456  ");

        padded.Should().Be(trimmed);
    }
}
