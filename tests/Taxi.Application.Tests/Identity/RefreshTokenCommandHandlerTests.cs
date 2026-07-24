using Ardalis.Specification;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Taxi.Application.Identity.Abstractions;
using Taxi.Application.Identity.Auth;
using Taxi.Application.Identity.Auth.Refresh;
using Taxi.Application.Abstractions;
using Taxi.Domain.Identity;
using Taxi.SharedKernel;
using Xunit;

namespace Taxi.Application.Tests.Identity;

public sealed class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IRepository<RefreshToken>> _repo = new();
    private readonly Mock<ITokenService> _tokens = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManager = IdentityMocks.UserManager();
    private RefreshTokenCommandHandler CreateHandler()
    {
        _tokens.Setup(t => t.HashRefreshToken(It.IsAny<string>())).Returns("hashed");
        _tokens.Setup(t => t.CreateAccessToken(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()))
           .Returns(new AccessToken("access-token", DateTime.UtcNow.AddMinutes(15)));
        _tokens.Setup(t => t.CreateRefreshToken())
            .Returns(new RefreshTokenValue("new-raw-refresh", "new-refresh-hash", DateTime.UtcNow.AddDays(30)));

        var issuer = new AuthTokenIssuer(_tokens.Object, _repo.Object);
        return new RefreshTokenCommandHandler(
              _repo.Object,
              _userManager.Object,
              _tokens.Object,
              issuer,
              NullLogger<RefreshTokenCommandHandler>.Instance);
    }

    [Fact]
    public async Task Should_fail_when_token_not_found()
    {
        _repo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<RefreshToken>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((RefreshToken?)null);

        var result = await CreateHandler().Handle(new RefreshTokenCommand("raw"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidToken");
    }

    [Fact]
    public async Task Handle_ShouldRevokeFamily_WhenRefreshTokenReuseIsDetected()
    {
        var familyId = Guid.NewGuid();
        var revoked = RefreshToken.Create("u-1", "hashed", DateTime.UtcNow.AddDays(7), familyId);
        revoked.Revoke("Rotation");
        var familyMember = RefreshToken.Create("u-1", "other", DateTime.UtcNow.AddDays(7), familyId);
        _repo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<RefreshToken>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(revoked);
        _repo.Setup(r => r.ListAsync(It.IsAny<ISpecification<RefreshToken>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<RefreshToken> { revoked, familyMember });

        var result = await CreateHandler().Handle(new RefreshTokenCommand("raw"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.TokenReuse");
        familyMember.IsRevoked.Should().BeTrue();
        familyMember.RevokedReason.Should().Be("TokenReuse");
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnauthorized_WhenRefreshTokenIsExpired()
    {
        var expired = RefreshToken.Create("u-1", "hashed", DateTime.UtcNow.AddSeconds(-1), Guid.NewGuid());
        _repo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<RefreshToken>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(expired);

        var result = await CreateHandler().Handle(new RefreshTokenCommand("raw"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.ExpiredToken");
    }
    [Fact]
    public async Task Handle_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        var stored = RefreshToken.Create("missing-user", "hashed", DateTime.UtcNow.AddDays(7), Guid.NewGuid());
        _repo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<RefreshToken>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);
        _userManager.Setup(um => um.FindByIdAsync("missing-user"))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await CreateHandler().Handle(new RefreshTokenCommand("raw"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidToken");
        _repo.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRotateRefreshToken_WhenTokenIsValid()
    {
        var familyId = Guid.NewGuid();
        var stored = RefreshToken.Create("u-1", "hashed", DateTime.UtcNow.AddDays(7), familyId);
        var user = new ApplicationUser
        {
            Id = "u-1",
            FullName = "Ali Moussa",
            PhoneNumber = "+25377123456"
        };

        _repo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<RefreshToken>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);
        _repo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken token, CancellationToken _) => token);
        _userManager.Setup(um => um.FindByIdAsync("u-1"))
            .ReturnsAsync(user);
        _userManager.Setup(um => um.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { RoleNames.Client });

        var result = await CreateHandler().Handle(new RefreshTokenCommand("raw"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.RefreshToken.Should().Be("new-raw-refresh");
        result.Value.User.Id.Should().Be("u-1");
        stored.IsRevoked.Should().BeTrue();
        stored.RevokedReason.Should().Be("Rotation");
        _repo.Verify(r => r.AddAsync(
            It.Is<RefreshToken>(token => token.UserId == "u-1" && token.TokenHash == "new-refresh-hash" && token.FamilyId == familyId),
            It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateAsync(stored, It.IsAny<CancellationToken>()), Times.Once);
    }
}
