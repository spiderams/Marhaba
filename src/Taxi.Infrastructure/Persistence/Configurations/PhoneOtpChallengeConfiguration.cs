using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taxi.Domain.Identity;

namespace Taxi.Infrastructure.Persistence.Configurations;

internal sealed class PhoneOtpChallengeConfiguration : IEntityTypeConfiguration<PhoneOtpChallenge>
{
    public void Configure(EntityTypeBuilder<PhoneOtpChallenge> builder)
    {
        builder.ToTable("phone_otp_challenges");
        builder.Property(c => c.PhoneNumber).HasMaxLength(32).IsRequired();
        builder.Property(c => c.CodeHash).HasMaxLength(64).IsRequired();
        builder.HasIndex(c => new { c.PhoneNumber, c.CreatedAt });
    }
}
