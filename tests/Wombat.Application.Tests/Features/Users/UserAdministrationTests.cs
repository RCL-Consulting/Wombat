using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Users;
using Wombat.Application.Features.Users.Commands.AddRoleToUser;
using Wombat.Application.Features.Users.Commands.RemoveRoleFromUser;
using Wombat.Application.Features.Users.Commands.ResetUserPassword;
using Wombat.Application.Features.Users.Commands.RevokePendingInvitationsForEmail;
using Wombat.Application.Features.Users.Commands.SetUserLockout;
using Wombat.Application.Features.Users.Queries.GetUserById;
using Wombat.Application.Features.Users.Queries.ListUsers;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.Invitations;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Users;

/// <summary>
/// T061: scope-guard + role-mutation + reset-password + revoke-invitation tests for
/// the admin Users surface. Uses an in-memory DbContext + a recording stub for
/// <see cref="IUserAdministrationService"/> so the tests don't carry Identity infra.
/// </summary>
public sealed class UserAdministrationTests
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;

    [Fact]
    public async Task ListUsers_AsInstitutionalAdmin_OnlyReturnsOwnInstitution()
    {
        await using var db = NewDb();
        SeedInstitutions(db);

        var users = new RecordingUserAdministrationService();
        users.Add(new UserIdentityDetails("u-a", "a@kgk", "A", "User", InstitutionA, [], [], [WombatRoles.Coordinator]));
        users.Add(new UserIdentityDetails("u-b", "b@b", "B", "User", InstitutionB, [], [], [WombatRoles.Coordinator]));
        users.Add(new UserIdentityDetails("u-g", "g@global", "G", "User", InstitutionId: null, [], [], [WombatRoles.Administrator]));

        var handler = new ListUsersQueryHandler(users, db);
        var result = await handler.Handle(new ListUsersQuery(TestPrincipals.InstitutionalAdmin(InstitutionA)), CancellationToken.None);

        // Unscoped global accounts (e.g. Administrators) are NOT exposed to a tenant-level admin.
        result.Select(user => user.UserId).Should().BeEquivalentTo("u-a");
    }

    [Fact]
    public async Task ListUsers_AsAdministrator_ReturnsAllIncludingUnscoped()
    {
        await using var db = NewDb();
        SeedInstitutions(db);

        var users = new RecordingUserAdministrationService();
        users.Add(new UserIdentityDetails("u-a", "a@kgk", "A", "User", InstitutionA, [], [], [WombatRoles.Coordinator]));
        users.Add(new UserIdentityDetails("u-b", "b@b", "B", "User", InstitutionB, [], [], [WombatRoles.Coordinator]));
        users.Add(new UserIdentityDetails("u-g", "g@global", "G", "User", InstitutionId: null, [], [], [WombatRoles.Administrator]));

        var handler = new ListUsersQueryHandler(users, db);
        var result = await handler.Handle(new ListUsersQuery(TestPrincipals.Administrator()), CancellationToken.None);

        result.Select(user => user.UserId).Should().BeEquivalentTo("u-a", "u-b", "u-g");
    }

    [Fact]
    public async Task GetUserById_UnscopedGlobalUser_AsInstitutionalAdmin_ReturnsNull()
    {
        await using var db = NewDb();
        SeedInstitutions(db);

        var users = new RecordingUserAdministrationService();
        users.Add(new UserIdentityDetails("u-g", "g@global", "G", "User", InstitutionId: null, [], [], [WombatRoles.Administrator]));

        var handler = new GetUserByIdQueryHandler(users, db);
        var result = await handler.Handle(new GetUserByIdQuery("u-g", TestPrincipals.InstitutionalAdmin(InstitutionA)), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserById_UnscopedGlobalUser_AsAdministrator_IsVisible()
    {
        await using var db = NewDb();
        SeedInstitutions(db);

        var users = new RecordingUserAdministrationService();
        users.Add(new UserIdentityDetails("u-g", "g@global", "G", "User", InstitutionId: null, [], [], [WombatRoles.Administrator]));

        var handler = new GetUserByIdQueryHandler(users, db);
        var result = await handler.Handle(new GetUserByIdQuery("u-g", TestPrincipals.Administrator()), CancellationToken.None);

        result.Should().NotBeNull();
        result!.UserId.Should().Be("u-g");
    }

    [Fact]
    public async Task ResetPassword_UnscopedNonAdministrator_AsInstitutionalAdmin_IsRejected()
    {
        // Latent write gap: an unscoped (no-institution) account that is NOT a global Administrator
        // must still be unreachable by a tenant-level admin. The role guard alone did not cover this.
        var users = new RecordingUserAdministrationService();
        users.Add(new UserIdentityDetails("u-g", "g@global", "G", "User", InstitutionId: null, [], [], [WombatRoles.Coordinator]));

        var handler = new ResetUserPasswordCommandHandler(users);
        var act = () => handler.Handle(
            new ResetUserPasswordCommand("u-g", "NewPass!2026", TestPrincipals.InstitutionalAdmin(InstitutionA)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        users.ResetPasswordCalls.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserById_OutOfScope_ReturnsNull()
    {
        await using var db = NewDb();
        SeedInstitutions(db);

        var users = new RecordingUserAdministrationService();
        users.Add(new UserIdentityDetails("u-b", "b@b", "B", "User", InstitutionB, [], [], [WombatRoles.Coordinator]));

        var handler = new GetUserByIdQueryHandler(users, db);
        var result = await handler.Handle(new GetUserByIdQuery("u-b", TestPrincipals.InstitutionalAdmin(InstitutionA)), CancellationToken.None);

        result.Should().BeNull();
    }

    [Theory]
    [InlineData(WombatRoles.Administrator)]
    [InlineData(WombatRoles.PendingTrainee)]
    public async Task AddRole_RejectsAdministratorAndPendingTrainee(string role)
    {
        var users = new RecordingUserAdministrationService();
        users.Add(new UserIdentityDetails("u-a", "a@kgk", "A", "User", InstitutionA, [], [], [WombatRoles.Coordinator]));

        var handler = new AddRoleToUserCommandHandler(users);
        var act = () => handler.Handle(new AddRoleToUserCommand("u-a", role, TestPrincipals.Administrator()), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cannot be assigned*");
        users.AddRoleCalls.Should().BeEmpty();
    }

    [Fact]
    public async Task AddRole_ToAssignableRole_PassesThroughToService()
    {
        var users = new RecordingUserAdministrationService();
        users.Add(new UserIdentityDetails("u-a", "a@kgk", "A", "User", InstitutionA, [], [], [WombatRoles.Coordinator]));

        var handler = new AddRoleToUserCommandHandler(users);
        await handler.Handle(new AddRoleToUserCommand("u-a", WombatRoles.Assessor, TestPrincipals.Administrator()), CancellationToken.None);

        users.AddRoleCalls.Should().ContainSingle().Which.Should().Be(("u-a", WombatRoles.Assessor));
    }

    [Fact]
    public async Task AddRole_AcrossInstitution_IsRejected()
    {
        var users = new RecordingUserAdministrationService();
        users.Add(new UserIdentityDetails("u-b", "b@b", "B", "User", InstitutionB, [], [], [WombatRoles.Coordinator]));

        var handler = new AddRoleToUserCommandHandler(users);
        var act = () => handler.Handle(
            new AddRoleToUserCommand("u-b", WombatRoles.Assessor, TestPrincipals.InstitutionalAdmin(InstitutionA)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        users.AddRoleCalls.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveRole_RejectsAdministrator()
    {
        var users = new RecordingUserAdministrationService();
        users.Add(new UserIdentityDetails("u-a", "a@kgk", "A", "User", InstitutionA, [], [], [WombatRoles.Coordinator, WombatRoles.Administrator]));

        var handler = new RemoveRoleFromUserCommandHandler(users);
        var act = () => handler.Handle(
            new RemoveRoleFromUserCommand("u-a", WombatRoles.Administrator, TestPrincipals.Administrator()),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cannot be removed*");
    }

    [Fact]
    public async Task ResetPassword_ForwardsToServiceWithNewPassword()
    {
        var users = new RecordingUserAdministrationService();
        users.Add(new UserIdentityDetails("u-a", "a@kgk", "A", "User", InstitutionA, [], [], [WombatRoles.Coordinator]));

        var handler = new ResetUserPasswordCommandHandler(users);
        await handler.Handle(
            new ResetUserPasswordCommand("u-a", "NewPass!2026", TestPrincipals.Administrator()),
            CancellationToken.None);

        users.ResetPasswordCalls.Should().ContainSingle().Which.Should().Be(("u-a", "NewPass!2026"));
    }

    [Fact]
    public async Task ResetPassword_AcrossInstitution_IsRejected()
    {
        var users = new RecordingUserAdministrationService();
        users.Add(new UserIdentityDetails("u-b", "b@b", "B", "User", InstitutionB, [], [], [WombatRoles.Coordinator]));

        var handler = new ResetUserPasswordCommandHandler(users);
        var act = () => handler.Handle(
            new ResetUserPasswordCommand("u-b", "NewPass!2026", TestPrincipals.InstitutionalAdmin(InstitutionA)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        users.ResetPasswordCalls.Should().BeEmpty();
    }

    [Fact]
    public async Task SetLockout_RefusesSelfLockout()
    {
        var users = new RecordingUserAdministrationService();
        users.Add(new UserIdentityDetails("self-id", "me@kgk", "Me", "User", InstitutionA, [], [], [WombatRoles.InstitutionalAdmin]));

        var handler = new SetUserLockoutCommandHandler(users);
        var act = () => handler.Handle(
            new SetUserLockoutCommand("self-id", true, TestPrincipals.InstitutionalAdmin(InstitutionA, userId: "self-id")),
            CancellationToken.None);

        // Since T278 a refusal of the caller's own account is a permission refusal, given before the lookup, and a
        // reactivation is refused as a lock is (UserAdministrationSelfAndTraineeTests).
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*your own*");
        users.LockoutCalls.Should().BeEmpty();
    }

    [Fact]
    public async Task SetLockout_RefusesAdministrator()
    {
        var users = new RecordingUserAdministrationService();
        users.Add(new UserIdentityDetails("admin-2", "admin2@global", "Admin", "Two", InstitutionId: null, [], [], [WombatRoles.Administrator]));

        var handler = new SetUserLockoutCommandHandler(users);
        var act = () => handler.Handle(
            new SetUserLockoutCommand("admin-2", true, TestPrincipals.Administrator()),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*administrator*");
    }

    [Fact]
    public async Task RevokePendingInvitations_SweepsOnlyActiveSameEmail()
    {
        await using var db = NewDb();
        SeedInstitutions(db);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        db.Set<Invitation>().AddRange(
            new Invitation { Email = "x@kgk", TokenHash = "h1", TargetRole = WombatRoles.Assessor, InstitutionId = InstitutionA, IssuedByUserId = "admin", IssuedOn = DateTime.UtcNow.AddDays(-1), ExpiresOn = today.AddDays(13) },
            new Invitation { Email = "x@kgk", TokenHash = "h2", TargetRole = WombatRoles.Coordinator, InstitutionId = InstitutionA, IssuedByUserId = "admin", IssuedOn = DateTime.UtcNow.AddDays(-2), ExpiresOn = today.AddDays(12) },
            new Invitation { Email = "x@kgk", TokenHash = "h3", TargetRole = WombatRoles.Assessor, InstitutionId = InstitutionA, IssuedByUserId = "admin", IssuedOn = DateTime.UtcNow.AddDays(-3), ExpiresOn = today.AddDays(11), RevokedOn = DateTime.UtcNow },
            new Invitation { Email = "other@kgk", TokenHash = "h4", TargetRole = WombatRoles.Assessor, InstitutionId = InstitutionA, IssuedByUserId = "admin", IssuedOn = DateTime.UtcNow, ExpiresOn = today.AddDays(14) });
        await db.SaveChangesAsync();

        var handler = new RevokePendingInvitationsForEmailCommandHandler(db);
        var count = await handler.Handle(
            new RevokePendingInvitationsForEmailCommand("x@kgk", TestPrincipals.InstitutionalAdmin(InstitutionA)),
            CancellationToken.None);

        count.Should().Be(2);
        var remaining = await db.Set<Invitation>().Where(entity => entity.Email == "x@kgk" && !entity.RevokedOn.HasValue).CountAsync();
        remaining.Should().Be(0);
        var other = await db.Set<Invitation>().SingleAsync(entity => entity.Email == "other@kgk");
        other.RevokedOn.Should().BeNull();
    }

    [Fact]
    public async Task RevokePendingInvitations_CrossInstitution_IsRejected()
    {
        await using var db = NewDb();
        SeedInstitutions(db);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        db.Set<Invitation>().Add(new Invitation
        {
            Email = "x@b",
            TokenHash = "h1",
            TargetRole = WombatRoles.Assessor,
            InstitutionId = InstitutionB,
            IssuedByUserId = "admin",
            IssuedOn = DateTime.UtcNow,
            ExpiresOn = today.AddDays(14)
        });
        await db.SaveChangesAsync();

        var handler = new RevokePendingInvitationsForEmailCommandHandler(db);
        var act = () => handler.Handle(
            new RevokePendingInvitationsForEmailCommand("x@b", TestPrincipals.InstitutionalAdmin(InstitutionA)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    /// <summary>
    /// T373 fix pass, B3 (S4): an invitation stored as typed, in another case and with spaces round it, is the account's
    /// address to the user page and to its revoke. Until the fix both compared the stored text byte for byte.
    /// </summary>
    [Fact]
    public async Task TheUserPage_AndItsRevoke_FindAnInvitationToTheAccountsAddressInAnotherCase()
    {
        await using var db = NewDb();
        SeedInstitutions(db);
        var issued = new DateTime(2026, 10, 19, 6, 0, 0, DateTimeKind.Utc);
        db.Set<Invitation>().Add(new Invitation
        {
            Email = "Molefe@KGK.wombat.local", TokenHash = "h1", TargetRole = WombatRoles.Assessor, InstitutionId = InstitutionA,
            IssuedByUserId = "admin", IssuedOn = issued, ExpiresOn = new DateOnly(2026, 11, 2)
        });
        await db.SaveChangesAsync();

        var users = new RecordingUserAdministrationService();
        users.Add(new UserIdentityDetails("u-m", "molefe@kgk.wombat.local", "Lerato", "Molefe", InstitutionA, [], [], [WombatRoles.Coordinator]));

        var page = await new GetUserByIdQueryHandler(users, db, new FixedClock(issued)).Handle(
            new GetUserByIdQuery("u-m", TestPrincipals.InstitutionalAdmin(InstitutionA)), CancellationToken.None);
        page!.PendingInvitations.Should().ContainSingle("the address is one, whatever its case");

        var revoked = await new RevokePendingInvitationsForEmailCommandHandler(db).Handle(
            new RevokePendingInvitationsForEmailCommand(" molefe@kgk.wombat.local ", TestPrincipals.InstitutionalAdmin(InstitutionA)),
            CancellationToken.None);
        revoked.Should().Be(1);
    }

    /// <summary>
    /// T373 fix pass, B3 (F7): a link works through its last day on the South African calendar. At 22:30 UTC on its last
    /// day it is already the next day in South Africa, and the user page no longer lists it, as Register, the list and the
    /// resend do not; at 21:30 UTC it still does.
    /// </summary>
    [Theory]
    [InlineData(21, true)]
    [InlineData(22, false)]
    public async Task TheUserPage_ListsAPendingInvitation_ThroughItsLastSouthAfricanDay(int utcHour, bool listed)
    {
        await using var db = NewDb();
        SeedInstitutions(db);
        var lastDay = new DateOnly(2026, 11, 2);
        db.Set<Invitation>().Add(new Invitation
        {
            Email = "x@kgk", TokenHash = "h1", TargetRole = WombatRoles.Assessor, InstitutionId = InstitutionA,
            IssuedByUserId = "admin", IssuedOn = new DateTime(2026, 10, 19, 6, 0, 0, DateTimeKind.Utc), ExpiresOn = lastDay
        });
        await db.SaveChangesAsync();

        var users = new RecordingUserAdministrationService();
        users.Add(new UserIdentityDetails("u-x", "x@kgk", "X", "User", InstitutionA, [], [], [WombatRoles.Coordinator]));
        var now = new DateTime(2026, 11, 2, utcHour, 30, 0, DateTimeKind.Utc);

        var page = await new GetUserByIdQueryHandler(users, db, new FixedClock(now)).Handle(
            new GetUserByIdQuery("u-x", TestPrincipals.InstitutionalAdmin(InstitutionA)), CancellationToken.None);

        page!.PendingInvitations.Should().HaveCount(listed ? 1 : 0);
    }

    private sealed class FixedClock(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }

    [Fact]
    public void AssignableRoles_ExcludesAdministratorPendingTraineeAndTrainee()
    {
        UserAdministrationRules.AssignableRoles.Should().NotContain(WombatRoles.Administrator);
        UserAdministrationRules.AssignableRoles.Should().NotContain(WombatRoles.PendingTrainee);
        UserAdministrationRules.AssignableRoles.Should().Contain(WombatRoles.Assessor);

        // T303: admission grants Trainee and Mark complete takes it away (TraineeRoleSystemManagedTests).
        UserAdministrationRules.AssignableRoles.Should().NotContain(WombatRoles.Trainee);
    }

    private static ApplicationDbContext NewDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static void SeedInstitutions(ApplicationDbContext db)
    {
        db.Institutions.AddRange(
            new Institution { Id = InstitutionA, Name = "Institution A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = InstitutionB, Name = "Institution B", ShortCode = "B", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.SaveChanges();
    }
}
