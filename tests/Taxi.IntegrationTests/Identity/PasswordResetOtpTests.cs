using Ardalis.Specification;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Timers;
using Taxi.Application.Abstractions;
using Taxi.Application.Identity.Auth.ForgotPassword;
using Taxi.Application.Identity.Otp;
using Taxi.Domain.Identity;
using Xunit;


namespace Taxi.Application.Tests.Identity;

public sealed class PasswordResetOtpTests
{
    private const string PhoneNumber = "+25377123456";

    private readonly Mock<UserManager<ApplicationUser>> _userManager = IdentityMocks.UserManager();
    private readonly Mock<IRepository<PhoneOtpChallenge>> _otpRepository = new();
    private readonly Mock<ISmsSender> _smsSender = new();

    [Fact]
    public async Task RequestPasswordReset_ShouldCreatePasswordResetOtp_AndSendSms_WhenUserExists()
    {
        var user = User();
        PhoneOtpChallenge? createdChallenge = null;
        string? sentMessage = null;

        _userManager.Setup(um => um.FindByNameAsync(PhoneNumber))
            .ReturnsAsync(user);
        _otpRepository.Setup(r => r.AddAsync(It.IsAny<PhoneOtpChallenge>(), It.IsAny<CancellationToken>()))
            .Callback<PhoneOtpChallenge, CancellationToken>((challenge, _) => createdChallenge = challenge)
            .ReturnsAsync((PhoneOtpChallenge challenge, CancellationToken _) => challenge);
        _smsSender.Setup(s => s.SendAsync(PhoneNumber, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((_, message, _) => sentMessage = message)
            .Returns(Task.CompletedTask);

        var handler = new RequestPasswordResetCommandHandler(
            _userManager.Object,
            _otpRepository.Object,
            _smsSender.Object);

        var result = await handler.Handle(new RequestPasswordResetCommand(PhoneNumber), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        createdChallenge.Should().NotBeNull();
        createdChallenge!.PhoneNumber.Should().Be(PhoneNumber);
        createdChallenge.Purpose.Should().Be(PhoneOtpChallenge.PasswordResetPurpose);
        createdChallenge.CodeHash.Should().NotBeNullOrWhiteSpace();
        createdChallenge.CodeHash.Should().NotMatchRegex(@"^\d{6}$");
        createdChallenge.ExpiresAt.Should().BeAfter(DateTime.UtcNow.AddMinutes(4));
        createdChallenge.ExpiresAt.Should().BeBefore(DateTime.UtcNow.AddMinutes(6));
        sentMessage.Should().NotBeNull();
        sentMessage.Should().Contain("réinitialisation");
        sentMessage.Should().MatchRegex(@"\d{6}");
        _smsSender.Verify(s => s.SendAsync(PhoneNumber, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RequestPasswordReset_ShouldReturnSuccess_WithoutCreatingOtp_WhenUserDoesNotExist()
    {
        _userManager.Setup(um => um.FindByNameAsync(PhoneNumber))
            .ReturnsAsync((ApplicationUser?)null);

        var handler = new RequestPasswordResetCommandHandler(
            _userManager.Object,
            _otpRepository.Object,
            _smsSender.Object);

        var result = await handler.Handle(new RequestPasswordResetCommand(PhoneNumber), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        _otpRepository.Verify(r => r.AddAsync(It.IsAny<PhoneOtpChallenge>(), It.IsAny<CancellationToken>()), Times.Never);
        _smsSender.Verify(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        _userManager.Setup(um => um.FindByNameAsync(PhoneNumber))
            .ReturnsAsync((ApplicationUser?)null);

        var handler = new ResetPasswordCommandHandler(_userManager.Object, _otpRepository.Object);

        var result = await handler.Handle(new ResetPasswordCommand(PhoneNumber, "123456", "NewPassword123!"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.UserNotFound");
        _otpRepository.Verify(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<PhoneOtpChallenge>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnValidation_WhenOtpChallengeDoesNotExist()
    {
        _userManager.Setup(um => um.FindByNameAsync(PhoneNumber))
            .ReturnsAsync(User());
        _otpRepository.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<PhoneOtpChallenge>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PhoneOtpChallenge?)null);

        var handler = new ResetPasswordCommandHandler(_userManager.Object, _otpRepository.Object);

        var result = await handler.Handle(new ResetPasswordCommand(PhoneNumber, "123456", "NewPassword123!"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidResetCode");
        _otpRepository.Verify(r => r.UpdateAsync(It.IsAny<PhoneOtpChallenge>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnValidation_AndPersistFailedAttempt_WhenOtpIsInvalid()
    {
        var challenge = PhoneOtpChallenge.CreatePasswordReset(PhoneNumber, "123456", DateTime.UtcNow.AddMinutes(5));
        _userManager.Setup(um => um.FindByNameAsync(PhoneNumber))
            .ReturnsAsync(User());
        _otpRepository.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<PhoneOtpChallenge>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(challenge);

        var handler = new ResetPasswordCommandHandler(_userManager.Object, _otpRepository.Object);

        var result = await handler.Handle(new ResetPasswordCommand(PhoneNumber, "000000", "NewPassword123!"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidResetCode");
        challenge.FailedAttempts.Should().Be(1);
        challenge.IsVerified.Should().BeFalse();
        _otpRepository.Verify(r => r.UpdateAsync(challenge, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnValidation_WhenOtpIsExpired()
    {
        var challenge = PhoneOtpChallenge.CreatePasswordReset(PhoneNumber, "123456", DateTime.UtcNow.AddMinutes(-1));
        _userManager.Setup(um => um.FindByNameAsync(PhoneNumber))
            .ReturnsAsync(User());
        _otpRepository.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<PhoneOtpChallenge>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(challenge);

        var handler = new ResetPasswordCommandHandler(_userManager.Object, _otpRepository.Object);

        var result = await handler.Handle(new ResetPasswordCommand(PhoneNumber, "123456", "NewPassword123!"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidResetCode");
        challenge.IsVerified.Should().BeFalse();
        _otpRepository.Verify(r => r.UpdateAsync(challenge, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_ShouldResetPassword_WhenOtpIsValid()
    {
        var user = User();
        var challenge = PhoneOtpChallenge.CreatePasswordReset(PhoneNumber, "123456", DateTime.UtcNow.AddMinutes(5));
        _userManager.Setup(um => um.FindByNameAsync(PhoneNumber))
            .ReturnsAsync(user);
        _userManager.Setup(um => um.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("identity-reset-token");
        _userManager.Setup(um => um.ResetPasswordAsync(user, "identity-reset-token", "NewPassword123!"))
            .ReturnsAsync(IdentityResult.Success);
        _otpRepository.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<PhoneOtpChallenge>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(challenge);

        var handler = new ResetPasswordCommandHandler(_userManager.Object, _otpRepository.Object);

        var result = await handler.Handle(new ResetPasswordCommand(PhoneNumber, "123456", "NewPassword123!"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        challenge.IsVerified.Should().BeTrue();
        _otpRepository.Verify(r => r.UpdateAsync(challenge, It.IsAny<CancellationToken>()), Times.Once);
        _userManager.Verify(um => um.GeneratePasswordResetTokenAsync(user), Times.Once);
        _userManager.Verify(um => um.ResetPasswordAsync(user, "identity-reset-token", "NewPassword123!"), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnValidation_WhenIdentityResetFails()
    {
        var user = User();
        var challenge = PhoneOtpChallenge.CreatePasswordReset(PhoneNumber, "123456", DateTime.UtcNow.AddMinutes(5));
        _userManager.Setup(um => um.FindByNameAsync(PhoneNumber))
            .ReturnsAsync(user);
        _userManager.Setup(um => um.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("identity-reset-token");
        _userManager.Setup(um => um.ResetPasswordAsync(user, "identity-reset-token", "weak"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));
        _otpRepository.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<PhoneOtpChallenge>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(challenge);

        var handler = new ResetPasswordCommandHandler(_userManager.Object, _otpRepository.Object);

        var result = await handler.Handle(new ResetPasswordCommand(PhoneNumber, "123456", "weak"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.PasswordResetFailed");
        result.Error.Description.Should().Be("Password too weak");
    }

    private static ApplicationUser User() => new()
    {
        Id = "u-1",
        UserName = PhoneNumber,
        PhoneNumber = PhoneNumber,
        FullName = "Ayanleh Moussa"
    };
}