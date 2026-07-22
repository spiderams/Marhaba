using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Taxi.Application.Identity.Otp;
using Taxi.Domain.Identity;
using Taxi.Infrastructure.Persistence;

namespace Taxi.IntegrationTests.Identity;

/// <summary>
/// Tests d'intégration de persistance des challenges OTP téléphone contre PostgreSQL/PostGIS.
/// </summary>
public sealed class PhoneOtpPersistenceTests(PostgisContainerFixture fixture) : IClassFixture<PostgisContainerFixture>
{
    private const string PhoneNumber = "+25377123456";

    [Fact]
    public async Task PhoneOtpChallenge_ShouldPersist_WithExpectedColumns()
    {
        await ResetPhoneOtpChallengesAsync();
        var expiresAt = DateTime.UtcNow.AddMinutes(5);
        var challenge = PhoneOtpChallenge.CreateRegistration(PhoneNumber, "123456", expiresAt);

        await using (var db = fixture.CreateContext())
        {
            db.PhoneOtpChallenges.Add(challenge);
            await db.SaveChangesAsync();
        }

        await using (var db = fixture.CreateContext())
        {
            var saved = await db.PhoneOtpChallenges.SingleAsync();

            saved.Id.Should().BeGreaterThan(0);
            saved.PhoneNumber.Should().Be(PhoneNumber);
            saved.Purpose.Should().Be(PhoneOtpChallenge.RegistrationPurpose);
            saved.CodeHash.Should().Be(challenge.CodeHash);
            saved.CodeHash.Should().NotBe("123456");
            saved.ExpiresAt.Should().BeCloseTo(expiresAt, TimeSpan.FromSeconds(1));
            saved.VerifiedAt.Should().BeNull();
            saved.FailedAttempts.Should().Be(0);
        }
    }

    [Fact]
    public async Task PhoneOtpChallenge_ShouldPersist_VerifiedAt_And_FailedAttempts()
    {
        await ResetPhoneOtpChallengesAsync();
        var now = DateTime.UtcNow;
        var challenge = PhoneOtpChallenge.CreateRegistration(PhoneNumber, "123456", now.AddMinutes(5));

        challenge.Verify("000000", now, maxAttempts: 5).Should().BeFalse();
        challenge.Verify("123456", now.AddSeconds(1), maxAttempts: 5).Should().BeTrue();

        await using (var db = fixture.CreateContext())
        {
            db.PhoneOtpChallenges.Add(challenge);
            await db.SaveChangesAsync();
        }

        await using (var db = fixture.CreateContext())
        {
            var saved = await db.PhoneOtpChallenges.SingleAsync();

            saved.FailedAttempts.Should().Be(1);
            saved.IsVerified.Should().BeTrue();
            saved.VerifiedAt.Should().BeCloseTo(now.AddSeconds(1), TimeSpan.FromSeconds(1));
        }
    }

    [Fact]
    public async Task LatestPhoneOtpChallengeSpec_ShouldReturnLatestRegistrationChallenge_ForPhoneAndPurpose()
    {
        await ResetPhoneOtpChallengesAsync();
        var olderRegistration = PhoneOtpChallenge.CreateRegistration(PhoneNumber, "111111", DateTime.UtcNow.AddMinutes(5));
        var passwordReset = PhoneOtpChallenge.CreatePasswordReset(PhoneNumber, "222222", DateTime.UtcNow.AddMinutes(5));

        await using (var db = fixture.CreateContext())
        {
            db.PhoneOtpChallenges.AddRange(olderRegistration, passwordReset);
            await db.SaveChangesAsync();
        }

        await Task.Delay(20);
        var latestRegistration = PhoneOtpChallenge.CreateRegistration(PhoneNumber, "333333", DateTime.UtcNow.AddMinutes(5));

        await using (var db = fixture.CreateContext())
        {
            db.PhoneOtpChallenges.Add(latestRegistration);
            await db.SaveChangesAsync();
        }

        await using (var db = fixture.CreateContext())
        {
            var repository = new Repository<PhoneOtpChallenge>(db);
            var result = await repository.FirstOrDefaultAsync(
                new LatestPhoneOtpChallengeSpec(PhoneNumber, PhoneOtpChallenge.RegistrationPurpose));

            result.Should().NotBeNull();
            result!.Id.Should().Be(latestRegistration.Id);
            result.Purpose.Should().Be(PhoneOtpChallenge.RegistrationPurpose);
            result.CodeHash.Should().Be(latestRegistration.CodeHash);
        }
    }

    [Fact]
    public async Task LatestPhoneOtpChallengeSpec_ShouldIgnore_OtherPhoneNumbers()
    {
        await ResetPhoneOtpChallengesAsync();
        var expected = PhoneOtpChallenge.CreateRegistration(PhoneNumber, "123456", DateTime.UtcNow.AddMinutes(5));
        var otherPhone = PhoneOtpChallenge.CreateRegistration("+25377000000", "999999", DateTime.UtcNow.AddMinutes(5));

        await using (var db = fixture.CreateContext())
        {
            db.PhoneOtpChallenges.AddRange(expected, otherPhone);
            await db.SaveChangesAsync();
        }

        await using (var db = fixture.CreateContext())
        {
            var repository = new Repository<PhoneOtpChallenge>(db);
            var result = await repository.FirstOrDefaultAsync(
                new LatestPhoneOtpChallengeSpec(PhoneNumber, PhoneOtpChallenge.RegistrationPurpose));

            result.Should().NotBeNull();
            result!.PhoneNumber.Should().Be(PhoneNumber);
            result.Id.Should().Be(expected.Id);
        }
    }
    private async Task ResetPhoneOtpChallengesAsync()
    {
        await using var db = fixture.CreateContext();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE phone_otp_challenges RESTART IDENTITY CASCADE;");
    }
}
