using System.Security.Claims;
using FluentAssertions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Users.Commands.AddRoleToUser;
using Wombat.Application.Features.Users.Commands.RemoveRoleFromUser;
using Wombat.Application.Features.Users.Commands.ResetUserPassword;
using Wombat.Application.Features.Users.Commands.SetUserLockout;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;

namespace Wombat.Application.Tests.Features.Users;

/// <summary>
/// T290: the Users commands say nothing about an id outside the caller's scope. An InstitutionalAdmin of A is given one
/// refusal whether the id names nobody, someone at B, or a global administrator, and it comes before any check of what the
/// user holds (T194's rule). A global Administrator, who sees every user, is told plainly that an id names nobody.
/// </summary>
public sealed class UserCommandsExistenceTests
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;
    private const string AtB = "someone-at-b";
    private const string GlobalAdmin = "global-admin";
    private const string Nobody = "no-such-user";

    private const string MayNotModify = "You do not have permission to modify this user.";
    private const string MayNotReset = "You do not have permission to reset this user's password.";

    public static TheoryData<string> OutOfScopeIds => new() { Nobody, AtB, GlobalAdmin };

    [Theory]
    [MemberData(nameof(OutOfScopeIds))]
    public async Task AnInstitutionalAdmin_IsGivenOneRefusal_WhateverTheIdNames(string userId)
    {
        var users = Store();
        var caller = TestPrincipals.InstitutionalAdmin(InstitutionA);

        (await Refusal(() => new AddRoleToUserCommandHandler(users).Handle(
            new AddRoleToUserCommand(userId, WombatRoles.Assessor, caller), CancellationToken.None))).Should().Be(MayNotModify);
        (await Refusal(() => new RemoveRoleFromUserCommandHandler(users).Handle(
            new RemoveRoleFromUserCommand(userId, WombatRoles.Assessor, caller), CancellationToken.None))).Should().Be(MayNotModify);
        (await Refusal(() => new SetUserLockoutCommandHandler(users).Handle(
            new SetUserLockoutCommand(userId, true, caller), CancellationToken.None))).Should().Be(MayNotModify);
        (await Refusal(() => new ResetUserPasswordCommandHandler(users).Handle(
            new ResetUserPasswordCommand(userId, "a-long-new-password", caller), CancellationToken.None))).Should().Be(MayNotReset);

        users.Changes.Should().Be(0);
    }

    [Fact]
    public async Task AnAdministrator_IsToldThatAnIdNamesNobody()
    {
        var users = Store();
        var act = () => new AddRoleToUserCommandHandler(users).Handle(
            new AddRoleToUserCommand(Nobody, WombatRoles.Assessor, TestPrincipals.Administrator()), CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be("The user could not be found.");
    }

    [Fact]
    public async Task AnInstitutionalAdmin_StillChangesSomeoneAtTheirOwnInstitution()
    {
        var users = Store();
        await new AddRoleToUserCommandHandler(users).Handle(
            new AddRoleToUserCommand("someone-at-a", WombatRoles.Assessor, TestPrincipals.InstitutionalAdmin(InstitutionA)),
            CancellationToken.None);

        users.AddRoleCalls.Should().Equal(("someone-at-a", WombatRoles.Assessor));
    }

    private static async Task<string> Refusal(Func<Task> act)
        => (await act.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message;

    private static RecordingUserAdministrationService Store()
    {
        var users = new RecordingUserAdministrationService();
        users.Add(new UserIdentityDetails("someone-at-a", "a@a.test", "A", "Person", InstitutionA, [], [], [WombatRoles.Coordinator]));
        users.Add(new UserIdentityDetails(AtB, "b@b.test", "B", "Person", InstitutionB, [], [], [WombatRoles.Coordinator]));
        users.Add(new UserIdentityDetails(GlobalAdmin, "g@x.test", "G", "Admin", null, [], [], [WombatRoles.Administrator]));
        return users;
    }
}
