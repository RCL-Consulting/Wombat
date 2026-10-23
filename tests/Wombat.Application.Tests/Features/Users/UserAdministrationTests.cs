using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Users;
using Wombat.Application.Features.Users.Commands.AddRoleToUser;
using Wombat.Application.Features.Users.Commands.RemoveRoleFromUser;
using Wombat.Application.Features.Users.Commands.ResetUserPassword;
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
/// T061: scope-guard + role-mutation + reset-password tests for
/// the admin Users surface (the revoke-invitation tests went with the command, T376, Q9). Uses an in-memory DbContext + a recording stub for
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
        result.Rows.Select(user => user.UserId).Should().BeEquivalentTo("u-a");
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

        result.Rows.Select(user => user.UserId).Should().BeEquivalentTo("u-a", "u-b", "u-g");
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

        await using var db = NewDb();
        var handler = new AddRoleToUserCommandHandler(users, db);
        var act = () => handler.Handle(new AddRoleToUserCommand("u-a", role, null, null, TestPrincipals.Administrator()), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cannot be assigned*");
        users.AddRoleCalls.Should().BeEmpty();
    }

    /// <remarks>
    /// Since T376 (M2) the role is written with the scope, the one the person holds when the role needs nothing more: a
    /// Committee member needs no speciality. An Assessor without a sub-speciality is refused (AddRoleToUserScopeTests).
    /// </remarks>
    [Fact]
    public async Task AddRole_ToAssignableRole_PassesThroughToService()
    {
        var users = new RecordingUserAdministrationService();
        users.Add(new UserIdentityDetails("u-a", "a@kgk", "A", "User", InstitutionA, [], [], [WombatRoles.Coordinator]));

        await using var db = NewDb();
        var handler = new AddRoleToUserCommandHandler(users, db);
        await handler.Handle(new AddRoleToUserCommand("u-a", WombatRoles.CommitteeMember, null, null, TestPrincipals.Administrator()), CancellationToken.None);

        users.AddRoleCalls.Should().ContainSingle().Which.Should().Be(("u-a", WombatRoles.CommitteeMember));
        users.UpdateScopeCalls.Should().ContainSingle().Which.InstitutionId.Should().Be(InstitutionA);
    }

    [Fact]
    public async Task AddRole_AcrossInstitution_IsRejected()
    {
        var users = new RecordingUserAdministrationService();
        users.Add(new UserIdentityDetails("u-b", "b@b", "B", "User", InstitutionB, [], [], [WombatRoles.Coordinator]));

        await using var db = NewDb();
        var handler = new AddRoleToUserCommandHandler(users, db);
        var act = () => handler.Handle(
            new AddRoleToUserCommand("u-b", WombatRoles.Assessor, null, null, TestPrincipals.InstitutionalAdmin(InstitutionA)),
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

    /// <summary>
    /// Q9 (T376, flow 12): the user page reads no invitation. An invitation to an address an account holds can neither be
    /// issued (T286) nor accepted, and Invitations lists every one; the DTO carries none.
    /// </summary>
    [Fact]
    public async Task TheUserPage_ReadsNoInvitation()
    {
        typeof(UserDetailDto).GetProperty("PendingInvitations").Should().BeNull();
        typeof(UserDetailDto).Assembly.GetTypes().Should().NotContain(type => type.Name == "UserPendingInvitationDto");

        await using var db = NewDb();
        SeedInstitutions(db);
        db.Set<Invitation>().Add(new Invitation
        {
            Email = "x@kgk", TokenHash = "h1", TargetRole = WombatRoles.Assessor, InstitutionId = InstitutionA,
            IssuedByUserId = "admin", IssuedOn = DateTime.UtcNow, ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(14)
        });
        await db.SaveChangesAsync();
        var users = new RecordingUserAdministrationService();
        users.Add(new UserIdentityDetails("u-x", "x@kgk", "X", "User", InstitutionA, [], [], [WombatRoles.Coordinator]));

        var page = await new GetUserByIdQueryHandler(users, db).Handle(
            new GetUserByIdQuery("u-x", TestPrincipals.InstitutionalAdmin(InstitutionA)), CancellationToken.None);

        page!.InstitutionName.Should().Be("Institution A");
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
