using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Accounts;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Users;
using Wombat.Application.Features.Users.Commands.RemoveUserInstitutionalSignIn;
using Wombat.Application.Features.Users.Commands.ResetUserPassword;
using Wombat.Application.Features.Users.Queries.GetRequestsWaitingForUser;
using Wombat.Application.Features.Users.Queries.GetUserById;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Users;

/// <summary>
/// The user page's other reads and commands (T376, flow 12): the reset refused to an account that may not use a password
/// (F3); another person's institutional sign-in removed (F7: the caller, the scope, the last way in, each in its words);
/// the requests waiting for a person (F6: the count, the oldest, five rows, out of scope refused, and the nudge's set);
/// and what GetUserById reads for the page of sections (S1, S2, F3).
/// </summary>
public sealed class UserPageHandlersTests
{
    private const int Kgk = 1;
    private const int Other = 2;
    private const string Patel = "patel";
    private const string Trainee = "duplessis";

    // ─── F3 ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task F3_AnAccountThatMayNotUseAPassword_IsNotReset_AndNothingIsWritten()
    {
        var users = Store();
        users.SignInMethods[Patel] = new AccountSignInMethods(false, [new InstitutionalSignIn("kgk-sso", "KGK sign-in")], AllowsLocalPassword: false);

        var act = () => new ResetUserPasswordCommandHandler(users).Handle(
            new ResetUserPasswordCommand(Patel, "A-long-new-password-1", TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(UserAdministrationRules.PasswordNotSettable);
        users.ResetPasswordCalls.Should().BeEmpty();
    }

    [Fact]
    public async Task F3_TheControl_AnAccountWithAPassword_IsReset()
    {
        var users = Store();

        await new ResetUserPasswordCommandHandler(users).Handle(
            new ResetUserPasswordCommand(Patel, "A-long-new-password-1", TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        users.ResetPasswordCalls.Should().Equal((Patel, "A-long-new-password-1"));
    }

    // ─── F7 ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task F7_AnotherPersonsSignIn_IsRemoved()
    {
        var users = Store();

        await new RemoveUserInstitutionalSignInCommandHandler(users).Handle(
            new RemoveUserInstitutionalSignInCommand(Patel, "kgk-sso", TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        users.RemoveSignInCalls.Should().Equal((Patel, "kgk-sso"));
    }

    [Fact]
    public async Task F7_TheCallersOwnSignIn_IsRefusedBeforeTheLookup()
    {
        var users = Store();

        var act = () => new RemoveUserInstitutionalSignInCommandHandler(users).Handle(
            new RemoveUserInstitutionalSignInCommand(Patel, "kgk-sso", TestPrincipals.InstitutionalAdmin(Kgk, userId: Patel)),
            CancellationToken.None);

        (await act.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(UserAdministrationRules.OwnSignInNotRemovable);
        users.Lookups.Should().BeEmpty();
        users.Changes.Should().Be(0);
    }

    [Fact]
    public async Task F7_SomeoneAtAnotherInstitution_IsRefusedInTheOneSentence()
    {
        var users = Store();

        var act = () => new RemoveUserInstitutionalSignInCommandHandler(users).Handle(
            new RemoveUserInstitutionalSignInCommand(Patel, "kgk-sso", TestPrincipals.InstitutionalAdmin(Other)), CancellationToken.None);

        (await act.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be("You do not have permission to modify this user.");
        users.Changes.Should().Be(0);
    }

    [Theory]
    [InlineData(InstitutionalSignInRemoval.LastWayIn)]
    [InlineData(InstitutionalSignInRemoval.NotLinked)]
    [InlineData(InstitutionalSignInRemoval.Conflict)]
    public async Task F7_TheDirectorysRefusal_IsTyped_SoThePageWordsEachReason(InstitutionalSignInRemoval answer)
    {
        var users = Store();
        users.RemovalAnswer = answer;

        var act = () => new RemoveUserInstitutionalSignInCommandHandler(users).Handle(
            new RemoveUserInstitutionalSignInCommand(Patel, "kgk-sso", TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        (await act.Should().ThrowAsync<InstitutionalSignInRemovalRefusedException>()).Which.Reason.Should().Be(answer);
        users.RemoveSignInCalls.Should().BeEmpty();
    }

    // ─── F6 ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task F6_OneRequest_IsCountedWithItsMomentAndItsRow()
    {
        await using var db = Db();
        var waited = Now.AddDays(-11);
        AddWaiting(db, Patel, waited, epaId: 15);
        await db.SaveChangesAsync();

        var result = await Handler(db).Handle(new GetRequestsWaitingForUserQuery(Patel, TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        result.Count.Should().Be(1);
        result.OldestWaitingSince!.Value.UtcDateTime.Should().Be(waited);
        var row = result.FirstFive.Should().ContainSingle().Which;
        row.TraineeName.Should().Be("Pieter du Plessis");
        row.ActivityTypeName.Should().Be("Portfolio and Logbook Review (Paediatrics)");
        row.Reference.Should().Be("PAED-015");
        row.IsOverdue.Should().BeTrue("eleven days is past the seven the inbox calls overdue");
    }

    [Fact]
    public async Task F6_MoreThanFive_AreCounted_TheOldestFirst_AndFiveListed()
    {
        await using var db = Db();
        for (var day = 1; day <= 8; day++)
        {
            AddWaiting(db, Patel, Now.AddDays(-day));
        }

        AddWaiting(db, "someone-else", Now.AddDays(-30));
        AddWaiting(db, Patel, Now.AddDays(-40), state: "completed");
        await db.SaveChangesAsync();

        var result = await Handler(db).Handle(new GetRequestsWaitingForUserQuery(Patel, TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        result.Count.Should().Be(8, "another nominee's request and a finished one are not his");
        result.OldestWaitingSince!.Value.UtcDateTime.Should().Be(Now.AddDays(-8));
        result.FirstFive.Should().HaveCount(5);
        result.FirstFive.Select(row => row.WaitingSince).Should().BeInAscendingOrder();
        result.FirstFive[^1].IsOverdue.Should().BeFalse("four days is not yet overdue");
    }

    [Fact]
    public async Task F6_SomeoneOutsideTheCallersScope_IsRefused_AndNoCountIsGiven()
    {
        await using var db = Db();
        AddWaiting(db, Patel, Now.AddDays(-3));
        await db.SaveChangesAsync();

        var act = () => Handler(db).Handle(new GetRequestsWaitingForUserQuery(Patel, TestPrincipals.InstitutionalAdmin(Other)), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task F6_TheNudgesSet_IsTheQuerysSet_ForTheSameData()
    {
        // AssessorPendingNudgeJob reads AwaitingNominees over the activities older than its days; the query over every one.
        await using var db = Db();
        AddWaiting(db, Patel, Now.AddDays(-10));
        AddWaiting(db, Patel, Now.AddDays(-9));
        AddWaiting(db, "someone-else", Now.AddDays(-12));
        AddWaiting(db, Patel, Now.AddDays(-20), state: "completed");
        await db.SaveChangesAsync();

        var nudge = (await AwaitingNominees.LoadAsync(db, db.Set<Activity>().Where(activity => activity.UpdatedOn < Now.AddDays(-5))))
            .Where(row => row.NomineeUserId == Patel)
            .Select(row => row.Activity.Id)
            .ToList();
        var query = await Handler(db).Handle(new GetRequestsWaitingForUserQuery(Patel, TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        nudge.Should().HaveCount(2);
        query.Count.Should().Be(nudge.Count);
        query.OldestWaitingSince!.Value.UtcDateTime.Should().Be(Now.AddDays(-10));
    }

    [Fact]
    public async Task B1_AnAssessorMovedToAnotherInstitution_ShowsTheNewAdminNoneOfTheOldInstitutionsRequests()
    {
        // Requests filed at KGK that still name Patel stay his after he moves to Other (T376 build review, B1); Other's
        // Institutional admin is shown none of them, neither the count nor a KGK registrar's name.
        await using var db = Db();
        AddWaiting(db, Patel, Now.AddDays(-3));
        AddWaiting(db, Patel, Now.AddDays(-2), institutionId: Other);
        await db.SaveChangesAsync();
        var moved = MovedStore();

        var result = await Handler(db, moved).Handle(new GetRequestsWaitingForUserQuery(Patel, TestPrincipals.InstitutionalAdmin(Other)), CancellationToken.None);

        result.Count.Should().Be(1, "only the request filed at his new institution is the new admin's to see");
        result.OldestWaitingSince!.Value.UtcDateTime.Should().Be(Now.AddDays(-2));
    }

    [Fact]
    public async Task B1_TheAdministrator_IsShownEveryRequestWaitingOnTheMovedAssessor()
    {
        await using var db = Db();
        AddWaiting(db, Patel, Now.AddDays(-3));
        AddWaiting(db, Patel, Now.AddDays(-2), institutionId: Other);
        await db.SaveChangesAsync();

        var result = await Handler(db, MovedStore()).Handle(new GetRequestsWaitingForUserQuery(Patel, TestPrincipals.Administrator()), CancellationToken.None);

        result.Count.Should().Be(2);
    }

    // ─── GetUserById ────────────────────────────────────────────────────────

    [Fact]
    public async Task ThePage_ReadsTheScopeInWords_HowTheAccountSignsIn_AndTheAdministrativeLock()
    {
        await using var db = Db();
        var users = Store();
        users.Add(new UserIdentityDetails(Patel, "patel@kgk", "Mohammed", "Patel", Kgk, [10], [20], [WombatRoles.Assessor],
            IsLockedOut: true, IsDeactivated: false));
        users.SignInMethods[Patel] = new AccountSignInMethods(true, [new InstitutionalSignIn("kgk-sso", "KGK sign-in")]);

        var page = await new GetUserByIdQueryHandler(users, db).Handle(
            new GetUserByIdQuery(Patel, TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        page!.Scope.Should().Be("Paediatrics / Paediatrics");
        page.InstitutionName.Should().Be("Kgosi Kgari Teaching Hospital");
        page.SpecialityNames.Should().Equal("Paediatrics");
        page.InstitutionalSignIns.Should().ContainSingle().Which.DisplayName.Should().Be("KGK sign-in");
        page.AllowsLocalPassword.Should().BeTrue();
        page.IsLockedOut.Should().BeFalse("a brute-force lockout that lifts itself is not \"Locked out\" (the administrative lock only)");
        page.EndedProgramme.Should().BeNull();
    }

    [Fact]
    public async Task S2_APersonWithNoRole_ReadsTheirLatestEndedProgramme()
    {
        await using var db = Db();
        var profile = new TraineeProfile
        {
            UserId = Trainee, InstitutionId = Kgk, CurriculumId = 1, ProgrammeStartDate = new DateOnly(2024, 1, 1)
        };
        profile.Deactivate(new DateOnly(2026, 10, 18), today: new DateOnly(2026, 10, 18));
        db.Set<TraineeProfile>().Add(profile);
        await db.SaveChangesAsync();
        var users = Store();
        users.Add(new UserIdentityDetails(Trainee, "duplessis@kgk", "Pieter", "du Plessis", Kgk, [10], [20], []));

        var page = await new GetUserByIdQueryHandler(users, db).Handle(
            new GetUserByIdQuery(Trainee, TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        page!.EndedProgramme.Should().Be(new EndedProgrammeDto(new DateOnly(2026, 10, 18), Completed: false));
    }

    // ─── Fixture ────────────────────────────────────────────────────────────

    private static readonly DateTime Now = new(2026, 10, 23, 10, 0, 0, DateTimeKind.Utc);

    private static RecordingUserAdministrationService Store()
    {
        var users = new RecordingUserAdministrationService();
        users.Add(new UserIdentityDetails(Patel, "patel@kgk", "Mohammed", "Patel", Kgk, [10], [20], [WombatRoles.Assessor]));
        users.Add(new UserIdentityDetails(Trainee, "duplessis@kgk", "Pieter", "du Plessis", Kgk, [10], [20], [WombatRoles.Trainee]));
        return users;
    }

    private static GetRequestsWaitingForUserQueryHandler Handler(ApplicationDbContext db, RecordingUserAdministrationService? users = null)
        => new(db, users ?? Store(), Options.Create(new DashboardThresholds()), new FixedClock(Now));

    /// <summary>Patel moved from KGK to Other; du Plessis still at KGK.</summary>
    private static RecordingUserAdministrationService MovedStore()
    {
        var users = new RecordingUserAdministrationService();
        users.Add(new UserIdentityDetails(Patel, "patel@other", "Mohammed", "Patel", Other, [], [], [WombatRoles.Assessor]));
        users.Add(new UserIdentityDetails(Trainee, "duplessis@kgk", "Pieter", "du Plessis", Kgk, [10], [20], [WombatRoles.Trainee]));
        return users;
    }

    private static ApplicationDbContext Db()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        db.Institutions.AddRange(
            new Institution { Id = Kgk, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK", IsActive = true, CreatedOn = Now },
            new Institution { Id = Other, Name = "Other", ShortCode = "O", IsActive = true, CreatedOn = Now });
        db.Set<Speciality>().Add(new Speciality { Id = 10, CollegeId = 1, Name = "Paediatrics" });
        db.Set<SubSpeciality>().Add(new SubSpeciality { Id = 20, SpecialityId = 10, Name = "Paediatrics" });
        db.Set<Epa>().Add(new Epa { Id = 15, Code = "PAED-015", Title = "Portfolio", SubSpecialityId = 20 });

        var activityType = new ActivityType
        {
            Id = 1, Key = "portfolio_review", Name = "Portfolio and Logbook Review (Paediatrics)", Scope = ActivityScope.Institution,
            ScopeId = Kgk, Version = 1, SchemaJson = SchemaJson, WorkflowJson = WorkflowJson,
            CreditRulesJson = """{ "counts_for": [] }""", DisplayFieldsJson = "[]", OwnerUserId = "admin-1", CreatedOn = Now
        };
        activityType.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = 1, Version = 1, SchemaJson = SchemaJson, WorkflowJson = WorkflowJson,
            CreditRulesJson = """{ "counts_for": [] }""", DisplayFieldsJson = "[]", PublishedByUserId = "admin-1", PublishedOn = Now.AddYears(-1)
        });
        db.ActivityTypes.Add(activityType);
        db.SaveChanges();
        return db;
    }

    private static void AddWaiting(ApplicationDbContext db, string nominee, DateTime updatedOn, string state = "requested", int? epaId = null, int institutionId = Kgk)
        => db.Activities.Add(new Activity
        {
            ActivityTypeId = 1,
            SchemaVersion = 1,
            SubjectUserId = Trainee,
            CreatedByUserId = Trainee,
            CurrentState = state,
            DataJson = $$"""{ "assessor_user_id": "{{nominee}}" }""",
            InstitutionId = institutionId,
            EpaId = epaId,
            CreatedOn = updatedOn,
            UpdatedOn = updatedOn
        });

    private sealed class FixedClock(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }

    private const string SchemaJson = """
        {
          "version": 1,
          "sections": [
            { "key": "request", "title": "Request", "fields": [ { "key": "assessor_user_id", "type": "user", "label": "Assessor" } ] }
          ]
        }
        """;

    private const string WorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id" }
          ]
        }
        """;
}
