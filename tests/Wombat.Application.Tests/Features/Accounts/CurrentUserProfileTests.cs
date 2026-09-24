using System.Security.Claims;
using FluentAssertions;
using Moq;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Accounts;
using Wombat.Domain.Identity;

namespace Wombat.Application.Tests.Features.Accounts;

/// <summary>
/// The account page reads and renames the signed-in user, and nobody else. (T185)
/// </summary>
/// <remarks>
/// Until T185 both requests took a user id, which the page read off the principal. The handlers answered for whatever
/// id they were handed, and nothing in the request named the caller to check it against. They now take the caller and
/// read the id from it, so no request can name another account.
/// </remarks>
public sealed class CurrentUserProfileTests
{
    private const string Caller = "user-1";

    [Fact]
    public void NeitherRequestCarriesAUserIdOfItsOwn_OnlyTheCaller()
    {
        foreach (var request in new[] { typeof(GetCurrentUserProfileQuery), typeof(UpdateCurrentUserProfileCommand) })
        {
            request.GetProperty("UserId").Should().BeNull(request.Name);
            request.GetProperty("Principal")!.PropertyType.Should().Be<ClaimsPrincipal>(request.Name);
        }
    }

    [Fact]
    public async Task TheProfile_IsTheCallersOwn()
    {
        var users = Users();

        var profile = await new GetCurrentUserProfileQueryHandler(users.Object).Handle(
            new GetCurrentUserProfileQuery(SignedIn()), CancellationToken.None);

        profile.UserId.Should().Be(Caller);
        profile.FirstName.Should().Be("Thandi");
        users.Verify(service => service.GetByIdAsync(Caller, It.IsAny<CancellationToken>()), Times.Once);
        users.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ARename_RenamesTheCaller()
    {
        var users = Users();

        await new UpdateCurrentUserProfileCommandHandler(users.Object).Handle(
            new UpdateCurrentUserProfileCommand(SignedIn(), "Thandeka", "Nkosi"),
            CancellationToken.None);

        users.Verify(service => service.UpdateNamesAsync(Caller, "Thandeka", "Nkosi", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ACallerWithNoUserId_IsRefused_AndNothingIsRenamed()
    {
        var users = Users();
        var nameless = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, WombatRoles.Trainee)], "test"));

        var read = () => new GetCurrentUserProfileQueryHandler(users.Object).Handle(
            new GetCurrentUserProfileQuery(nameless), CancellationToken.None);
        var rename = () => new UpdateCurrentUserProfileCommandHandler(users.Object).Handle(
            new UpdateCurrentUserProfileCommand(nameless, "Someone", "Else"), CancellationToken.None);

        await read.Should().ThrowAsync<UnauthorizedAccessException>();
        await rename.Should().ThrowAsync<UnauthorizedAccessException>();
        users.VerifyNoOtherCalls();
    }

    [Fact]
    public void TheValidators_RequireTheCaller()
    {
        new GetCurrentUserProfileQueryValidator().Validate(new GetCurrentUserProfileQuery(null!))
            .IsValid.Should().BeFalse();
        new UpdateCurrentUserProfileCommandValidator().Validate(new UpdateCurrentUserProfileCommand(null!, "A", "B"))
            .IsValid.Should().BeFalse();
    }

    /// <summary>
    /// The signed-in caller. The user id is the NameIdentifier claim, and the display name differs from it, so a handler
    /// that read the account from the name would be caught.
    /// </summary>
    private static ClaimsPrincipal SignedIn()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, Caller),
                new Claim(ClaimTypes.Name, "thandi@example.test"),
                new Claim(ClaimTypes.Role, WombatRoles.Trainee)
            ],
            "test"));

    private static Mock<IUserAdministrationService> Users()
    {
        var users = new Mock<IUserAdministrationService>();
        users
            .Setup(service => service.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string userId, CancellationToken _) =>
                new UserIdentityDetails(userId, $"{userId}@example.test", "Thandi", "Nkosi", 1, [], [], [WombatRoles.Trainee]));
        return users;
    }
}
