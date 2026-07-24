using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Identity;
using Moq;
using Taxi.Application.Identity.Auth.Profile;
using Taxi.Domain.Identity;
using Xunit;

namespace Taxi.Application.Tests.Identity;

public sealed class ProfileTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManager = IdentityMocks.UserManager();

    [Fact]
    public async Task UpdateProfile_ShouldUpdateFullName_AndReturnUpdatedUserInfo_WhenUserExists()
    {
        var user = new ApplicationUser
        {
            Id = "u-1",
            FullName = "Ancien Nom",
            PhoneNumber = "+25377123456"
        };

        _userManager.Setup(um => um.FindByIdAsync("u-1"))
            .ReturnsAsync(user);
        _userManager.Setup(um => um.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(um => um.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { RoleNames.Client });

        var handler = new UpdateProfileCommandHandler(_userManager.Object);

        var result = await handler.Handle(new UpdateProfileCommand("u-1", "  Nouveau Nom  "), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.FullName.Should().Be("Nouveau Nom");
        result.Value.Id.Should().Be("u-1");
        result.Value.FullName.Should().Be("Nouveau Nom");
        result.Value.PhoneNumber.Should().Be("+25377123456");
        result.Value.Roles.Should().ContainSingle().Which.Should().Be(RoleNames.Client);
        _userManager.Verify(um => um.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task UpdateProfile_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        _userManager.Setup(um => um.FindByIdAsync("missing-user"))
            .ReturnsAsync((ApplicationUser?)null);

        var handler = new UpdateProfileCommandHandler(_userManager.Object);

        var result = await handler.Handle(new UpdateProfileCommand("missing-user", "Nouveau Nom"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.UserNotFound");
        _userManager.Verify(um => um.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProfile_ShouldReturnValidation_WhenIdentityUpdateFails()
    {
        var user = new ApplicationUser
        {
            Id = "u-1",
            FullName = "Ancien Nom",
            PhoneNumber = "+25377123456"
        };

        _userManager.Setup(um => um.FindByIdAsync("u-1"))
            .ReturnsAsync(user);
        _userManager.Setup(um => um.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));

        var handler = new UpdateProfileCommandHandler(_userManager.Object);

        var result = await handler.Handle(new UpdateProfileCommand("u-1", "Nouveau Nom"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.ProfileUpdateFailed");
        result.Error.Description.Should().Be("Update failed");
    }

    [Fact]
    public void Validator_ShouldReject_WhenUserIdIsEmpty()
    {
        var validator = new UpdateProfileCommandValidator();

        var result = validator.TestValidate(new UpdateProfileCommand(string.Empty, "Nouveau Nom"));

        result.ShouldHaveValidationErrorFor(c => c.UserId);
    }

    [Fact]
    public void Validator_ShouldReject_WhenFullNameIsEmpty()
    {
        var validator = new UpdateProfileCommandValidator();

        var result = validator.TestValidate(new UpdateProfileCommand("u-1", string.Empty));

        result.ShouldHaveValidationErrorFor(c => c.FullName);
    }

    [Fact]
    public void Validator_ShouldReject_WhenFullNameIsTooLong()
    {
        var validator = new UpdateProfileCommandValidator();
        var tooLong = new string('A', 121);

        var result = validator.TestValidate(new UpdateProfileCommand("u-1", tooLong));

        result.ShouldHaveValidationErrorFor(c => c.FullName);
    }

    [Fact]
    public void Validator_ShouldAccept_WhenProfilePayloadIsValid()
    {
        var validator = new UpdateProfileCommandValidator();

        var result = validator.TestValidate(new UpdateProfileCommand("u-1", "Ali Moussa"));

        result.IsValid.Should().BeTrue();
    }
}
