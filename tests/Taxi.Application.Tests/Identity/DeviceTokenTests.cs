using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Identity;
using Moq;
using Taxi.Application.Identity.Auth.DeviceToken;
using Taxi.Domain.Identity;
using Xunit;

namespace Taxi.Application.Tests.Identity;

public sealed class DeviceTokenTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManager = IdentityMocks.UserManager();

    [Fact]
    public async Task UpdateDeviceToken_ShouldSaveDeviceToken_WhenUserExists()
    {
        var user = new ApplicationUser
        {
            Id = "u-1",
            FullName = "Ali Moussa",
            PhoneNumber = "+25377123456"
        };
        _userManager.Setup(um => um.FindByIdAsync("u-1"))
            .ReturnsAsync(user);
        _userManager.Setup(um => um.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var handler = new UpdateDeviceTokenCommandHandler(_userManager.Object);

        var result = await handler.Handle(new UpdateDeviceTokenCommand("u-1", "fcm-device-token"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        user.DeviceToken.Should().Be("fcm-device-token");
        _userManager.Verify(um => um.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task UpdateDeviceToken_ShouldTrimDeviceToken_WhenSaving()
    {
        var user = new ApplicationUser
        {
            Id = "u-1",
            FullName = "Ali Moussa",
            PhoneNumber = "+25377123456"
        };
        _userManager.Setup(um => um.FindByIdAsync("u-1"))
            .ReturnsAsync(user);
        _userManager.Setup(um => um.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var handler = new UpdateDeviceTokenCommandHandler(_userManager.Object);

        var result = await handler.Handle(new UpdateDeviceTokenCommand("u-1", "  fcm-device-token  "), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.DeviceToken.Should().Be("fcm-device-token");
    }

    [Fact]
    public async Task UpdateDeviceToken_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        _userManager.Setup(um => um.FindByIdAsync("missing-user"))
            .ReturnsAsync((ApplicationUser?)null);

        var handler = new UpdateDeviceTokenCommandHandler(_userManager.Object);

        var result = await handler.Handle(new UpdateDeviceTokenCommand("missing-user", "fcm-device-token"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.UserNotFound");
        _userManager.Verify(um => um.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task UpdateDeviceToken_ShouldReturnValidation_WhenIdentityUpdateFails()
    {
        var user = new ApplicationUser
        {
            Id = "u-1",
            FullName = "Ali Moussa",
            PhoneNumber = "+25377123456"
        };
        _userManager.Setup(um => um.FindByIdAsync("u-1"))
            .ReturnsAsync(user);
        _userManager.Setup(um => um.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));

        var handler = new UpdateDeviceTokenCommandHandler(_userManager.Object);

        var result = await handler.Handle(new UpdateDeviceTokenCommand("u-1", "fcm-device-token"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.DeviceTokenUpdateFailed");
        result.Error.Description.Should().Be("Update failed");
    }

    [Fact]
    public void Validator_ShouldReject_WhenUserIdIsEmpty()
    {
        var validator = new UpdateDeviceTokenCommandValidator();

        var result = validator.TestValidate(new UpdateDeviceTokenCommand(string.Empty, "fcm-device-token"));

        result.ShouldHaveValidationErrorFor(c => c.UserId);
    }

    [Fact]
    public void Validator_ShouldReject_WhenDeviceTokenIsEmpty()
    {
        var validator = new UpdateDeviceTokenCommandValidator();

        var result = validator.TestValidate(new UpdateDeviceTokenCommand("u-1", string.Empty));

        result.ShouldHaveValidationErrorFor(c => c.DeviceToken);
    }

    [Fact]
    public void Validator_ShouldReject_WhenDeviceTokenIsTooLong()
    {
        var validator = new UpdateDeviceTokenCommandValidator();
        var tooLong = new string('A', 4097);

        var result = validator.TestValidate(new UpdateDeviceTokenCommand("u-1", tooLong));

        result.ShouldHaveValidationErrorFor(c => c.DeviceToken);
    }

    [Fact]
    public void Validator_ShouldAccept_WhenPayloadIsValid()
    {
        var validator = new UpdateDeviceTokenCommandValidator();

        var result = validator.TestValidate(new UpdateDeviceTokenCommand("u-1", "fcm-device-token"));

        result.IsValid.Should().BeTrue();
    }
}