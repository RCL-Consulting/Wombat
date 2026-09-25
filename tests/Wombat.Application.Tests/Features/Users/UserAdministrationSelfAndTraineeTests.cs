using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Users;
using Wombat.Application.Features.Users.Commands.AddRoleToUser;
using Wombat.Application.Features.Users.Commands.RemoveRoleFromUser;
using Wombat.Application.Features.Users.Commands.ResetUserPassword;
using Wombat.Application.Features.Users.Commands.RevokePendingInvitationsForEmail;
using Wombat.Application.Features.Users.Commands.SetUserLockout;
using Wombat.Application.Features.Users.Queries.GetUserById;
using Wombat.Application.Features.Users.Queries.ListUsers;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.Invitations;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Users;

/// <summary>
/// T278: nobody changes their own roles, lockout or password from the Users surface, and anyone who holds Trainee
/// administers no user at all, whatever other role they hold (<see cref="UserAdministrationRules" />).
/// </summary>
/// <remarks>
/// <para>
/// Until T278 the Users commands asked only whether the caller could reach the user's institution. A Trainee who also
/// held InstitutionalAdmin could remove their own Trainee role, sign in again and administer the panel that reviews them,
/// undoing T256; or, since every chair action reads the user store, take CommitteeMember from their own panel's chair or
/// lock the chair out, and so stop the ratification of their own review.
/// </para>
/// <para>
/// Each refusal is checked against the audit trap. The store writes each change through <c>UserManager</c>, which saves
/// at once, so every refusal must come before the store is asked for a change (<see cref="RecordingUserAdministrationService.Changes" />);
/// and both refusals are asked of the caller alone, so they come before the user is looked up, and say nothing about the
/// id (<see cref="RecordingUserAdministrationService.Lookups" />). The revocation, which writes through the DbContext, is
/// saved and its tracker cleared as the audit pipeline's catch would. Each rule has a control: the same caller, without
/// Trainee, or acting on someone else, is admitted.
/// </para>
/// <para>
/// The refusals are pinned as literals, so a change to the words the pages show is a deliberate one.
/// </para>
/// </remarks>
public sealed class UserAdministrationSelfAndTraineeTests
{
    private const int InstitutionA = 1;
    private const string Self = "registrar-a";
    private const string Other = "colleague-a";

    private const string TraineeAdministersNoUser =
        "You hold the Trainee role, so you cannot view or change user accounts, including your own.";

    private const string MayNotAdministerUsers = "You do not have permission to administer users.";
    private const string OwnRolesNotChangeable = "You cannot change your own roles. Another administrator must change them.";
    private const string OwnLockoutNotChangeable = "You cannot lock out or reactivate your own account.";

    private const string OwnPasswordNotResettable =
        "You cannot reset your own password here. Change it on the Change password page, which asks for your current password.";

    public static TheoryData<string> UserAdministrationRoles => new()
    {
        WombatRoles.Administrator,
        WombatRoles.InstitutionalAdmin
    };

    // ─── Nobody changes their own account ────────────────────────────────────

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public async Task NobodyAddsOrRemovesTheirOwnRoles_AndIsRefusedBeforeTheLookup(string role)
    {
        var users = Store();
        var caller = Caller(role, holdsTrainee: false);

        var add = () => new AddRoleToUserCommandHandler(users).Handle(
            new AddRoleToUserCommand(Self, WombatRoles.CommitteeMember, caller), CancellationToken.None);
        (await add.Should().ThrowAsync<UnauthorizedAccessException>(role)).Which.Message.Should().Be(OwnRolesNotChangeable);

        var remove = () => new RemoveRoleFromUserCommandHandler(users).Handle(
            new RemoveRoleFromUserCommand(Self, WombatRoles.Coordinator, caller), CancellationToken.None);
        (await remove.Should().ThrowAsync<UnauthorizedAccessException>(role)).Which.Message.Should().Be(OwnRolesNotChangeable);

        users.Changes.Should().Be(0, "the audit trap: nothing is asked of the store before the refusal");
        users.Lookups.Should().BeEmpty("the refusal is asked of the caller, before the user is looked up");
    }

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public async Task NobodyRemovesTheirOwnInstitutionalAdminRole_TheRoleThatAdmitsThem(string role)
    {
        // Not only Trainee: an administrator who removes their own administration role locks themselves out, and one who
        // adds a role to themselves grants what nobody else has decided.
        var users = Store();
        var caller = Caller(role, holdsTrainee: false);

        var remove = () => new RemoveRoleFromUserCommandHandler(users).Handle(
            new RemoveRoleFromUserCommand(Self, WombatRoles.InstitutionalAdmin, caller), CancellationToken.None);

        (await remove.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(OwnRolesNotChangeable);
        users.Changes.Should().Be(0);
    }

    [Theory]
    [InlineData(WombatRoles.Administrator, true)]
    [InlineData(WombatRoles.Administrator, false)]
    [InlineData(WombatRoles.InstitutionalAdmin, true)]
    [InlineData(WombatRoles.InstitutionalAdmin, false)]
    public async Task NobodyLocksOutOrReactivatesTheirOwnAccount(string role, bool locked)
    {
        // Until T278 a reactivation of one's own account was not refused, and a lock only after the lookup.
        var users = Store();

        var act = () => new SetUserLockoutCommandHandler(users).Handle(
            new SetUserLockoutCommand(Self, locked, Caller(role, holdsTrainee: false)), CancellationToken.None);

        (await act.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(OwnLockoutNotChangeable);
        users.LockoutCalls.Should().BeEmpty();
        users.Lookups.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public async Task NobodyResetsTheirOwnPasswordHere_WhichAsksForNoCurrentPassword(string role)
    {
        var users = Store();

        var act = () => new ResetUserPasswordCommandHandler(users).Handle(
            new ResetUserPasswordCommand(Self, "A-new-password-1", Caller(role, holdsTrainee: false)), CancellationToken.None);

        (await act.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(OwnPasswordNotResettable);
        users.ResetPasswordCalls.Should().BeEmpty();
        users.Lookups.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public async Task TheControl_TheSameCaller_ChangesSomeoneElsesRoles_Lockout_AndPassword(string role)
    {
        var users = Store();
        var caller = Caller(role, holdsTrainee: false);

        await new AddRoleToUserCommandHandler(users).Handle(
            new AddRoleToUserCommand(Other, WombatRoles.Assessor, caller), CancellationToken.None);
        await new RemoveRoleFromUserCommandHandler(users).Handle(
            new RemoveRoleFromUserCommand(Other, WombatRoles.CommitteeMember, caller), CancellationToken.None);
        await new SetUserLockoutCommandHandler(users).Handle(
            new SetUserLockoutCommand(Other, true, caller), CancellationToken.None);
        await new SetUserLockoutCommandHandler(users).Handle(
            new SetUserLockoutCommand(Other, false, caller), CancellationToken.None);
        await new ResetUserPasswordCommandHandler(users).Handle(
            new ResetUserPasswordCommand(Other, "A-new-password-1", caller), CancellationToken.None);

        users.AddRoleCalls.Should().Equal((Other, WombatRoles.Assessor));
        users.RemoveRoleCalls.Should().Equal((Other, WombatRoles.CommitteeMember));
        users.LockoutCalls.Should().Equal((Other, true), (Other, false));
        users.ResetPasswordCalls.Should().Equal((Other, "A-new-password-1"));
    }

    // ─── Someone who holds Trainee administers no user ──────────────────────

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public async Task ATraineeWhoAdministersUsers_ChangesNobodysRoles_Lockout_OrPassword_AndIsRefusedBeforeTheLookup(string role)
    {
        // A colleague at their own institution: the panel's chair, say, whom T256 made every chair action read.
        var users = Store();
        var caller = Caller(role, holdsTrainee: true);

        foreach (var (name, act) in Commands(users, Other, caller))
        {
            (await act.Should().ThrowAsync<UnauthorizedAccessException>($"{role}: {name}"))
                .Which.Message.Should().Be(TraineeAdministersNoUser, $"{role}: {name}");
        }

        users.Changes.Should().Be(0, "the audit trap: nothing is asked of the store before the refusal");
        users.Lookups.Should().BeEmpty("the refusal is asked of the caller, before the user is looked up");
    }

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public async Task ATraineeWhoAdministersUsers_CannotRemoveTheirOwnTraineeRole_AndIsToldItIsTheTraineeRole(string role)
    {
        // The lever T278 closes. Trainee first: the refusal names the Trainee role, which stands in the way of every
        // user, not only of themselves.
        var users = Store();
        var caller = Caller(role, holdsTrainee: true);

        var remove = () => new RemoveRoleFromUserCommandHandler(users).Handle(
            new RemoveRoleFromUserCommand(Self, WombatRoles.Trainee, caller), CancellationToken.None);

        (await remove.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(TraineeAdministersNoUser);
        users.RemoveRoleCalls.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public async Task ATraineeWhoAdministersUsers_ListsAndOpensNobody_TheirOwnAccountIncluded(string role)
    {
        await using var db = NewDb();
        var users = Store();
        var caller = Caller(role, holdsTrainee: true);

        var list = () => new ListUsersQueryHandler(users, db).Handle(new ListUsersQuery(caller), CancellationToken.None);
        (await list.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(TraineeAdministersNoUser);

        foreach (var userId in new[] { Other, Self, "no-such-user" })
        {
            var open = () => new GetUserByIdQueryHandler(users, db).Handle(new GetUserByIdQuery(userId, caller), CancellationToken.None);
            (await open.Should().ThrowAsync<UnauthorizedAccessException>(userId))
                .Which.Message.Should().Be(TraineeAdministersNoUser, "the same refusal for any id, so it says nothing about one");
        }

        users.Lookups.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public async Task TheControl_TheSameCallerWithoutTrainee_ListsAndOpensTheInstitutionsUsers(string role)
    {
        await using var db = NewDb();
        var users = Store();
        var caller = Caller(role, holdsTrainee: false);

        var listed = await new ListUsersQueryHandler(users, db).Handle(new ListUsersQuery(caller), CancellationToken.None);
        var opened = await new GetUserByIdQueryHandler(users, db).Handle(new GetUserByIdQuery(Other, caller), CancellationToken.None);

        listed.Select(user => user.UserId).Should().BeEquivalentTo([Self, Other]);
        opened!.UserId.Should().Be(Other);
    }

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public async Task ATraineeWhoAdministersUsers_RevokesNoInvitation_AndNothingIsLeftForTheAuditSave(string role)
    {
        await using var db = NewDb();
        db.Set<Invitation>().Add(PendingInvitation("colleague@a.test"));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var revoke = () => new RevokePendingInvitationsForEmailCommandHandler(db).Handle(
            new RevokePendingInvitationsForEmailCommand("colleague@a.test", Caller(role, holdsTrainee: true)),
            CancellationToken.None);

        (await revoke.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(TraineeAdministersNoUser);

        // As the audit pipeline's catch would: it saves the request's context.
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        (await db.Set<Invitation>().SingleAsync()).RevokedOn.Should().BeNull();

        // The control: the same caller without Trainee revokes it.
        var revoked = await new RevokePendingInvitationsForEmailCommandHandler(db).Handle(
            new RevokePendingInvitationsForEmailCommand("colleague@a.test", Caller(role, holdsTrainee: false)),
            CancellationToken.None);
        revoked.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public async Task TheirOwnEmailsInvitations_AreStillRevocable_ByAnAdministratorWhoIsNotATrainee(string role)
    {
        // Not refused: an invitation creates an account, and one addressed to an account that exists cannot be accepted.
        await using var db = NewDb();
        db.Set<Invitation>().Add(PendingInvitation($"{Self}@a.test"));
        await db.SaveChangesAsync();

        var revoked = await new RevokePendingInvitationsForEmailCommandHandler(db).Handle(
            new RevokePendingInvitationsForEmailCommand($"{Self}@a.test", Caller(role, holdsTrainee: false)),
            CancellationToken.None);

        revoked.Should().Be(1);
    }

    public static TheoryData<string> NonAdministrationRoles => new()
    {
        WombatRoles.Trainee,
        WombatRoles.Coordinator,
        WombatRoles.CommitteeMember,
        WombatRoles.SpecialityAdmin
    };

    [Theory]
    [MemberData(nameof(NonAdministrationRoles))]
    public async Task ACallerWhoHoldsNoRoleThatAdministersUsers_IsToldSo_NotThatTheyAreATrainee(string role)
    {
        var users = Store();
        var caller = TestPrincipals.InRole(role, Self, InstitutionA);

        foreach (var (name, act) in Commands(users, Other, caller))
        {
            (await act.Should().ThrowAsync<UnauthorizedAccessException>($"{role}: {name}"))
                .Which.Message.Should().Be(MayNotAdministerUsers, $"{role}: {name}");
        }

        users.Changes.Should().Be(0);
        users.Lookups.Should().BeEmpty();
        UserAdministrationRules.TraineeNoteOnUserPages(caller).Should().BeNull(role);
    }

    // ─── The rule the pages ask ─────────────────────────────────────────────

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public void ThePagesNote_IsTheHandlersRefusal_ForATraineeWhoAdministersUsers_AndNullWithoutTrainee(string role)
    {
        UserAdministrationRules.TraineeNoteOnUserPages(Caller(role, holdsTrainee: true)).Should().Be(TraineeAdministersNoUser);
        UserAdministrationRules.TraineeNoteOnUserPages(Caller(role, holdsTrainee: false)).Should().BeNull();
        UserAdministrationRules.MayAdministerUsers(Caller(role, holdsTrainee: true)).Should().BeFalse();
        UserAdministrationRules.MayAdministerUsers(Caller(role, holdsTrainee: false)).Should().BeTrue();
    }

    [Fact]
    public void ACallerWithNoUserId_IsNoAccountsCaller()
    {
        // An empty id is refused by each command's validator; the rule must not read it as the caller's own.
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, WombatRoles.Administrator)], "test"));

        UserAdministrationRules.IsCaller(anonymous, string.Empty).Should().BeFalse();
        UserAdministrationRules.IsCaller(Caller(WombatRoles.Administrator, holdsTrainee: false), Self).Should().BeTrue();
        UserAdministrationRules.IsCaller(Caller(WombatRoles.Administrator, holdsTrainee: false), Self.ToUpperInvariant())
            .Should().BeFalse("an id is compared exactly");
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    /// <summary>Every Users command, each changing <paramref name="userId" />, as <paramref name="caller" />.</summary>
    private static IEnumerable<(string Name, Func<Task> Act)> Commands(
        RecordingUserAdministrationService users, string userId, ClaimsPrincipal caller)
    {
        yield return ("add a role", () => new AddRoleToUserCommandHandler(users).Handle(
            new AddRoleToUserCommand(userId, WombatRoles.Assessor, caller), CancellationToken.None));
        yield return ("remove CommitteeMember", () => new RemoveRoleFromUserCommandHandler(users).Handle(
            new RemoveRoleFromUserCommand(userId, WombatRoles.CommitteeMember, caller), CancellationToken.None));
        yield return ("lock out", () => new SetUserLockoutCommandHandler(users).Handle(
            new SetUserLockoutCommand(userId, true, caller), CancellationToken.None));
        yield return ("reactivate", () => new SetUserLockoutCommandHandler(users).Handle(
            new SetUserLockoutCommand(userId, false, caller), CancellationToken.None));
        yield return ("reset the password", () => new ResetUserPasswordCommandHandler(users).Handle(
            new ResetUserPasswordCommand(userId, "A-new-password-1", caller), CancellationToken.None));
    }

    /// <summary>
    /// The registrar at A in <paramref name="role" />, and Trainee beside it when <paramref name="holdsTrainee" />. A global
    /// Administrator carries no institution.
    /// </summary>
    private static ClaimsPrincipal Caller(string role, bool holdsTrainee)
        => TestPrincipals.InRoles(
            holdsTrainee ? [WombatRoles.Trainee, role] : [role],
            Self,
            role == WombatRoles.Administrator ? null : InstitutionA);

    /// <summary>
    /// The registrar and a colleague at A: the colleague a committee member who sits as a panel's chair, and so holds no
    /// Trainee role (T237: a trainee sits on no panel), and the registrar a trainee who also coordinates and administers
    /// their institution.
    /// </summary>
    private static RecordingUserAdministrationService Store()
    {
        var users = new RecordingUserAdministrationService();
        users.Add(new UserIdentityDetails(
            Self, $"{Self}@a.test", "Rene", "Registrar", InstitutionA, [], [],
            [WombatRoles.Trainee, WombatRoles.Coordinator, WombatRoles.InstitutionalAdmin]));
        users.Add(new UserIdentityDetails(
            Other, "colleague@a.test", "Chris", "Chair", InstitutionA, [], [],
            [WombatRoles.CommitteeMember]));
        return users;
    }

    private static Invitation PendingInvitation(string email)
        => new()
        {
            Email = email,
            TokenHash = Guid.NewGuid().ToString("N"),
            TargetRole = WombatRoles.Assessor,
            InstitutionId = InstitutionA,
            IssuedByUserId = "admin",
            IssuedOn = DateTime.UtcNow,
            ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(14)
        };

    private static ApplicationDbContext NewDb()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        db.Institutions.Add(new Institution { Id = InstitutionA, Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.SaveChanges();
        return db;
    }
}
