using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Accounts;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Accounts;

/// <summary>
/// The account page reads and renames the signed-in user, and removes their institutional sign-ins, and nobody else's.
/// (T185; T339, flow 02)
/// </summary>
/// <remarks>
/// Until T185 both requests took a user id, which the page read off the principal. The handlers answered for whatever
/// id they were handed, and nothing in the request named the caller to check it against. They now take the caller and
/// read the id from it, so no request can name another account. T339's removal is held to the same.
/// </remarks>
public sealed class CurrentUserProfileTests
{
    private const string Caller = "user-1";

    [Fact]
    public void NoRequestCarriesAUserIdOfItsOwn_OnlyTheCaller()
    {
        foreach (var request in new[]
                 {
                     typeof(GetCurrentUserProfileQuery), typeof(UpdateCurrentUserProfileCommand), typeof(RemoveMyInstitutionalSignInCommand)
                 })
        {
            request.GetProperty("UserId").Should().BeNull(request.Name);
            request.GetProperty("Principal")!.PropertyType.Should().Be<ClaimsPrincipal>(request.Name);
        }
    }

    [Fact]
    public async Task TheProfile_IsTheCallersOwn()
    {
        var users = Users();

        var profile = await new GetCurrentUserProfileQueryHandler(users.Object, Db()).Handle(
            new GetCurrentUserProfileQuery(SignedIn()), CancellationToken.None);

        profile.UserId.Should().Be(Caller);
        profile.FirstName.Should().Be("Thandi");
        users.Verify(service => service.GetByIdAsync(Caller, It.IsAny<CancellationToken>()), Times.Once);
        users.Verify(service => service.GetSignInMethodsAsync(Caller, It.IsAny<CancellationToken>()), Times.Once,
            "how the account signs in is the caller's too (T339)");
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
    public async Task ACallerWithNoUserId_IsRefused_AndNothingIsRenamedOrRemoved()
    {
        var users = Users();
        var nameless = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, WombatRoles.Trainee)], "test"));

        var read = () => new GetCurrentUserProfileQueryHandler(users.Object, Db()).Handle(
            new GetCurrentUserProfileQuery(nameless), CancellationToken.None);
        var rename = () => new UpdateCurrentUserProfileCommandHandler(users.Object).Handle(
            new UpdateCurrentUserProfileCommand(nameless, "Someone", "Else"), CancellationToken.None);
        var remove = () => new RemoveMyInstitutionalSignInCommandHandler(users.Object).Handle(
            new RemoveMyInstitutionalSignInCommand(nameless, "kgk"), CancellationToken.None);

        await read.Should().ThrowAsync<UnauthorizedAccessException>();
        await rename.Should().ThrowAsync<UnauthorizedAccessException>();
        await remove.Should().ThrowAsync<UnauthorizedAccessException>();
        users.VerifyNoOtherCalls();
    }

    [Fact]
    public void TheValidators_RequireTheCaller()
    {
        new GetCurrentUserProfileQueryValidator().Validate(new GetCurrentUserProfileQuery(null!))
            .IsValid.Should().BeFalse();
        new UpdateCurrentUserProfileCommandValidator().Validate(new UpdateCurrentUserProfileCommand(null!, "A", "B"))
            .IsValid.Should().BeFalse();
        new RemoveMyInstitutionalSignInCommandValidator().Validate(new RemoveMyInstitutionalSignInCommand(null!, "kgk"))
            .IsValid.Should().BeFalse();
        new RemoveMyInstitutionalSignInCommandValidator().Validate(new RemoveMyInstitutionalSignInCommand(SignedIn(), " "))
            .IsValid.Should().BeFalse("a removal names the provider");
        new RemoveMyInstitutionalSignInCommandValidator().Validate(new RemoveMyInstitutionalSignInCommand(SignedIn(), "kgk"))
            .IsValid.Should().BeTrue();
    }

    // ---- T339, flow 02: the Account and How you sign in cards ----

    [Fact]
    public async Task TheProfile_NamesTheInstitution_AndHowTheAccountSignsIn()
    {
        var db = Db();
        db.Set<Institution>().AddRange(
            new Institution { Id = 1, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK" },
            new Institution { Id = 2, Name = "Another Hospital", ShortCode = "AH" });
        await db.SaveChangesAsync();
        var users = Users(methods: new AccountSignInMethods(true, [new InstitutionalSignIn("kgk", "Kgosi Kgari Teaching Hospital")]));

        var profile = await new GetCurrentUserProfileQueryHandler(users.Object, db).Handle(
            new GetCurrentUserProfileQuery(SignedIn()), CancellationToken.None);

        profile.InstitutionName.Should().Be("Kgosi Kgari Teaching Hospital");
        profile.HasLocalPassword.Should().BeTrue();
        profile.InstitutionalSignIns.Should().Equal(new InstitutionalSignIn("kgk", "Kgosi Kgari Teaching Hospital"));
    }

    [Fact]
    public async Task AnAccountWithNoInstitution_HasNoInstitutionName_AndOneThatSignsInOnlyThroughItsInstitution_NoPassword()
    {
        var users = Users(institutionId: null, methods: new AccountSignInMethods(false, [new InstitutionalSignIn("kgk", "KGK")]));

        var profile = await new GetCurrentUserProfileQueryHandler(users.Object, Db()).Handle(
            new GetCurrentUserProfileQuery(SignedIn()), CancellationToken.None);

        profile.InstitutionName.Should().BeNull("the platform Administrator and a College admin have no Institution row (C4)");
        profile.HasLocalPassword.Should().BeFalse();
    }

    [Fact]
    public void TheRoles_AreLabelled_InTheOrderWombatListsThem_AnUnknownKeyLastAsItIs()
    {
        var profile = new UserProfileDto("u", "u@example.test", "A", "B",
        [
            "Mystery", WombatRoles.Assessor, WombatRoles.PendingTrainee, WombatRoles.CommitteeMember, WombatRoles.Coordinator,
            WombatRoles.SpecialityAdmin
        ]);

        profile.RoleLabels.Should().Equal("Speciality admin", "Coordinator", "Committee member", "Assessor", "Pending trainee", "Mystery");
    }

    // ---- T339: removing an institutional sign-in ----

    [Fact]
    public void TheRemoval_IsACommand_SoTheAuditPipelineWritesItsRow_AndCarriesNoPassword()
    {
        nameof(RemoveMyInstitutionalSignInCommand).Should().EndWith("Command", "AuditPipelineBehavior audits a request by its name");
        typeof(RemoveMyInstitutionalSignInCommand).GetProperties().Select(property => property.Name)
            .Should().BeEquivalentTo(["Principal", "Provider"], "the endpoint checks the password; the audit row must never hold one");
    }

    [Fact]
    public async Task ARemoval_RemovesTheCallersSignIn()
    {
        var users = Users();

        await new RemoveMyInstitutionalSignInCommandHandler(users.Object).Handle(
            new RemoveMyInstitutionalSignInCommand(SignedIn(), "kgk"), CancellationToken.None);

        users.Verify(service => service.RemoveInstitutionalSignInAsync(Caller, "kgk", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(InstitutionalSignInRemoval.NotLinked)]
    [InlineData(InstitutionalSignInRemoval.LastWayIn)]
    [InlineData(InstitutionalSignInRemoval.Conflict)]
    public async Task ARemovalThatDidNotHappen_Throws_SoTheAuditRowRecordsAFailure_WithItsReason(InstitutionalSignInRemoval answer)
    {
        var users = Users(removal: answer);

        var remove = () => new RemoveMyInstitutionalSignInCommandHandler(users.Object).Handle(
            new RemoveMyInstitutionalSignInCommand(SignedIn(), "kgk"), CancellationToken.None);

        (await remove.Should().ThrowAsync<InstitutionalSignInRemovalRefusedException>()).Which.Reason.Should().Be(answer);
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

    private static Mock<IUserAdministrationService> Users(
        int? institutionId = 1,
        AccountSignInMethods? methods = null,
        InstitutionalSignInRemoval removal = InstitutionalSignInRemoval.Removed)
    {
        var users = new Mock<IUserAdministrationService>();
        users
            .Setup(service => service.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string userId, CancellationToken _) =>
                new UserIdentityDetails(userId, $"{userId}@example.test", "Thandi", "Nkosi", institutionId, [], [], [WombatRoles.Trainee]));
        users
            .Setup(service => service.GetSignInMethodsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(methods ?? new AccountSignInMethods(true, []));
        users
            .Setup(service => service.RemoveInstitutionalSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(removal);
        return users;
    }

    private static ApplicationDbContext Db()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
}
