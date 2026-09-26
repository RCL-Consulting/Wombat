using System.Security.Claims;
using FluentAssertions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Users;
using Wombat.Application.Features.Users.Commands.AddRoleToUser;
using Wombat.Application.Features.Users.Commands.RemoveRoleFromUser;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;

namespace Wombat.Application.Tests.Features.Users;

/// <summary>
/// T303: the Trainee role is system-managed, as PendingTrainee is. Admission (<c>AdmitTrainee</c>) grants it and marking
/// the programme complete (<c>CompleteTraineeProfile</c>) takes it away; the Users surface neither adds nor removes it.
/// </summary>
/// <remarks>
/// <para>
/// Until T303 Add role offered Trainee. Pressed for a pending registrar, it left an account holding Trainee beside
/// PendingTrainee with no profile and no adoption pin, still listed under Pending admission; a later admission then saved
/// the profile and the scope and failed on "User already in role 'Trainee'". Pressed for a graduate, it gave the role back
/// against a completed profile. Remove role, the mirror, took Trainee from someone whose programme was still running.
/// </para>
/// <para>
/// Each refusal is checked against the audit trap as <see cref="UserAdministrationSelfAndTraineeTests" /> checks its own:
/// the store writes each change through <c>UserManager</c>, which saves at once, so nothing may be asked of it before the
/// refusal; and the refusal is asked of the role alone, so it comes before the user is looked up.
/// <c>TraineeRoleSystemManagedPostgresTests</c> holds the same commands to the real store and the real audit pipeline.
/// </para>
/// <para>The refusal is pinned as a literal, so a change to the words the page would show is a deliberate one.</para>
/// </remarks>
public sealed class TraineeRoleSystemManagedTests
{
    private const int InstitutionA = 1;
    private const string PendingRegistrar = "molefe";
    private const string Graduate = "graduate";
    private const string Trainee = "trainee";

    private const string TraineeRoleByAdmissionOnly =
        "The Trainee role cannot be added or removed here. A registrar becomes a trainee when admitted to a curriculum: " +
        "open Trainees and choose 'Admit to curriculum'. Marking their programme complete takes the role away.";

    public static TheoryData<string> UserAdministrationRoles => new()
    {
        WombatRoles.Administrator,
        WombatRoles.InstitutionalAdmin
    };

    /// <summary>
    /// Each administrator role against each account Add role could have made a trainee: a pending registrar, and a
    /// graduate whose role Mark complete took away. The refusal is asked of the role alone, so the two accounts run the same
    /// path; the theory says so rather than two tests that differ only in their names. The bUnit and Postgres tests tell
    /// the account states apart.
    /// </summary>
    public static TheoryData<string, string> AddRoleCases => new()
    {
        { WombatRoles.Administrator, PendingRegistrar },
        { WombatRoles.Administrator, Graduate },
        { WombatRoles.InstitutionalAdmin, PendingRegistrar },
        { WombatRoles.InstitutionalAdmin, Graduate }
    };

    [Theory]
    [MemberData(nameof(AddRoleCases))]
    public async Task AddRole_RefusesTrainee_WhateverTheAccount_BeforeTheLookup_AndChangesNothing(string role, string account)
    {
        var users = Store();

        var act = () => new AddRoleToUserCommandHandler(users).Handle(
            new AddRoleToUserCommand(account, WombatRoles.Trainee, Caller(role)), CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>(role)).Which.Message.Should().Be(TraineeRoleByAdmissionOnly);
        users.Changes.Should().Be(0, "the audit trap: nothing is asked of the store before the refusal");
        users.Lookups.Should().BeEmpty("the refusal is asked of the role, before the user is looked up");
    }

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public async Task RemoveRole_RefusesTrainee_FromATraineeWhoseProgrammeIsRunning_BeforeTheLookup_AndChangesNothing(string role)
    {
        var users = Store();

        var act = () => new RemoveRoleFromUserCommandHandler(users).Handle(
            new RemoveRoleFromUserCommand(Trainee, WombatRoles.Trainee, Caller(role)), CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>(role)).Which.Message.Should().Be(TraineeRoleByAdmissionOnly);
        users.Changes.Should().Be(0, "the audit trap: nothing is asked of the store before the refusal");
        users.Lookups.Should().BeEmpty("the refusal is asked of the role, before the user is looked up");
    }

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public async Task TheControl_TheSameCaller_AddsAndRemovesARoleTheSurfaceManages_OnTheSameAccounts(string role)
    {
        var users = Store();
        var caller = Caller(role);

        await new AddRoleToUserCommandHandler(users).Handle(
            new AddRoleToUserCommand(PendingRegistrar, WombatRoles.Assessor, caller), CancellationToken.None);
        await new RemoveRoleFromUserCommandHandler(users).Handle(
            new RemoveRoleFromUserCommand(Trainee, WombatRoles.Coordinator, caller), CancellationToken.None);

        users.AddRoleCalls.Should().Equal((PendingRegistrar, WombatRoles.Assessor));
        users.RemoveRoleCalls.Should().Equal((Trainee, WombatRoles.Coordinator));
    }

    [Fact]
    public void TheRolesTheSurfaceManages_AreInstitutionalAdminToAssessor_InTheOrderThePageOffersThem()
    {
        // Neither Trainee nor PendingTrainee, which admission manages; nor Administrator, which is never assigned here.
        UserAdministrationRules.AssignableRoles.Should().Equal(
            WombatRoles.InstitutionalAdmin,
            WombatRoles.SpecialityAdmin,
            WombatRoles.SubSpecialityAdmin,
            WombatRoles.Coordinator,
            WombatRoles.CommitteeMember,
            WombatRoles.Assessor);
        UserAdministrationRules.IsAssignableRole(WombatRoles.Trainee).Should().BeFalse();
    }

    private static ClaimsPrincipal Caller(string role)
        => role == WombatRoles.Administrator
            ? TestPrincipals.Administrator()
            : TestPrincipals.InstitutionalAdmin(InstitutionA);

    /// <summary>
    /// Three accounts at A: Dr Molefe, invited and awaiting admission; a graduate, whose Trainee role Mark complete took
    /// away and who holds none; and a trainee in a running programme, who also coordinates.
    /// </summary>
    private static RecordingUserAdministrationService Store()
    {
        var users = new RecordingUserAdministrationService();
        users.Add(new UserIdentityDetails(
            PendingRegistrar, "molefe@a.test", "Lerato", "Molefe", InstitutionA, [], [], [WombatRoles.PendingTrainee]));
        users.Add(new UserIdentityDetails(Graduate, "graduate@a.test", "Grace", "Graduate", InstitutionA, [], [], []));
        users.Add(new UserIdentityDetails(
            Trainee, "trainee@a.test", "Tia", "Trainee", InstitutionA, [], [], [WombatRoles.Trainee, WombatRoles.Coordinator]));
        return users;
    }
}
