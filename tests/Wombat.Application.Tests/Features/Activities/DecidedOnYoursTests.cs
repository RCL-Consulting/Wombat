using System.Security.Claims;
using FluentAssertions;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Tests.Shared;
using static Wombat.Application.Tests.TestHelpers.AssessorReads;
using Counts = Wombat.Application.Tests.TestHelpers.TraineeCounts;

namespace Wombat.Application.Tests.Features.Activities;

/// <summary>
/// Recent decisions on the Trainee's Home (T355, B1; E6; note 1): her own requests whose last move someone else made and
/// that are finished or have no move left, by each pinned workflow (T297), never a create (flow 04's R1 fix) and never a
/// system-managed record (MSF); newest decision first; named as My activities names them (E7, over her whole list); each
/// with its latest credit (T108) and the count it made (E5).
/// </summary>
public sealed class DecidedOnYoursTests
{
    private static readonly DateTime Now = Counts.TodayUtc;

    private const int TeachingSessionTypeId = 24;
    private const int ProcedureTypeId = 25;

    /// <summary>
    /// E6: the rule, not a list. A completion, a decline, a discussion, a sign-off and an accept by someone else are each
    /// a decision, whatever the state is called.
    /// </summary>
    [Fact]
    public async Task ACompletionADeclineADiscussionASignOffAndAnAccept_BySomeoneElse_AreDecisions()
    {
        await using var db = CreateDb();
        SeedCpsaTypes(db);
        SeedLogs(db);
        // The fixture's types credit nothing; the Mini-CEX's pinned rules credit its EPA's item, as the shipped seed's do.
        db.ActivityTypes.Local.Single(type => type.Id == CpsaMiniCexTypeId).Versions.Single().CreditRulesJson =
            """{ "counts_for": [ { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1 } ] }""";
        AddFiled(db, 1, CpsaMiniCexTypeId, "completed", TraineeId, NamesTheAssessor, Now.AddHours(-1),
            Move("submit", "draft", "requested", TraineeId, Now.AddDays(-2)),
            Move("complete", "requested", "completed", AssessorId, Now.AddHours(-1)));
        AddFiled(db, 2, CpsaMiniCexTypeId, "declined", TraineeId, NamesTheAssessor, Now.AddHours(-2),
            Move("submit", "draft", "requested", TraineeId, Now.AddDays(-2)),
            Move("decline", "requested", "declined", AssessorId, Now.AddHours(-2)));
        AddFiled(db, 3, CpsaReflectiveTypeId, "discussed", TraineeId, NamesTheAssessor, Now.AddHours(-3),
            Move("submit", "draft", "submitted", TraineeId, Now.AddDays(-2)),
            Move("record_discussion", "submitted", "discussed", AssessorId, Now.AddHours(-3)));
        AddFiled(db, 4, CpsaPortfolioReviewTypeId, "signed_off", TraineeId, NamesTheAssessor, Now.AddHours(-4),
            Move("submit", "draft", "submitted", TraineeId, Now.AddDays(-2)),
            Move("sign_off", "submitted", "signed_off", AssessorId, Now.AddHours(-4)));
        AddFiled(db, 5, TeachingSessionTypeId, "accepted", TraineeId, NamesTheAssessor, Now.AddHours(-5),
            Move("submit", "draft", "submitted", TraineeId, Now.AddDays(-2)),
            Move("accept", "submitted", "accepted", AssessorId, Now.AddHours(-5)));
        await db.SaveChangesAsync();

        var result = await DecidedAsync(db, take: 10);

        result.Select(item => (item.Id, item.CurrentStateLabel, item.IsFinished)).Should().Equal(
            (1, "Completed", true), (2, "Declined", false), (3, "Discussed", true), (4, "Signed off", true), (5, "Accepted", true));
        result[0].DecidedOn.Should().Be(Now.AddHours(-1));

        // T355, build review R4: the decline is the pinned workflow's fact (the assessor's move into a dead end), the one
        // File it again copies by; and whether each type can credit is its pinned rules' (a reflection credits nothing).
        result.Select(item => (item.Id, item.Declined)).Should().Equal((1, false), (2, true), (3, false), (4, false), (5, false));
        result.Single(item => item.Id == 1).CanCredit.Should().BeTrue("the CPSA Mini-CEX credits its EPA's item");
        result.Single(item => item.Id == 3).CanCredit.Should().BeFalse("the reflective exercise credits nothing");
    }

    /// <summary>
    /// A return to her draft has a move left (hers), so it is not decided; a request she withdrew after it was returned was
    /// moved last by her; her own logged procedure was created by her.
    /// </summary>
    [Fact]
    public async Task AReturnToHerDraft_HerOwnWithdrawal_AndHerOwnLoggedProcedure_AreNotDecisions()
    {
        await using var db = CreateDb();
        SeedCpsaTypes(db);
        SeedLogs(db);
        AddFiled(db, 1, CpsaReflectiveTypeId, "draft", TraineeId, NamesTheAssessor, Now.AddHours(-1),
            Move("submit", "draft", "submitted", TraineeId, Now.AddDays(-2)),
            Move("return", "submitted", "draft", AssessorId, Now.AddHours(-1)));
        AddFiled(db, 2, CpsaReflectiveTypeId, "cancelled", TraineeId, NamesTheAssessor, Now.AddHours(-1),
            Move("submit", "draft", "submitted", TraineeId, Now.AddDays(-2)),
            Move("return", "submitted", "draft", AssessorId, Now.AddHours(-2)),
            Move("cancel", "draft", "cancelled", TraineeId, Now.AddHours(-1)));
        AddFiled(db, 3, ProcedureTypeId, "logged", TraineeId, "{}", Now.AddHours(-1),
            Move(Wombat.Domain.Activities.Workflow.Workflow.CreateTransitionKey, "logged", "logged", TraineeId, Now.AddHours(-1)));
        await db.SaveChangesAsync();

        (await DecidedAsync(db)).Should().BeEmpty();
    }

    /// <summary>
    /// A create is not a decision, whoever makes it (flow 04's R1 fix): a supervisor who logs a procedure on her behalf, on
    /// a type born terminal, recorded it. An MSF record is written by its release, and is never listed ("MSF records are not
    /// listed", R1).
    /// </summary>
    [Fact]
    public async Task ASupervisorsCreateOnHerBehalf_EvenOnATypeBornTerminal_AndAnMsfRecord_AreNotDecisions()
    {
        await using var db = CreateDb();
        SeedLogs(db);
        ShippedSeeds.AddType(db, 31, "msf_cpsa", "Multi-Source Feedback (Paediatrics)").SystemManaged = true;
        var logged = AddFiled(db, 1, ProcedureTypeId, "logged", TraineeId, "{}", Now.AddHours(-1),
            Move(Wombat.Domain.Activities.Workflow.Workflow.CreateTransitionKey, "logged", "logged", AssessorId, Now.AddHours(-1)));
        logged.CreatedByUserId = AssessorId;
        AddFiled(db, 2, 31, "recorded", TraineeId, "{}", Now.AddHours(-2),
            Move("record", "draft", "recorded", "coordinator-1", Now.AddHours(-2)));
        await db.SaveChangesAsync();

        (await DecidedAsync(db)).Should().BeEmpty();
    }

    [Fact]
    public async Task NewestDecisionFirst_ThenTheHigherId_AndOnlyAsManyAsAsked()
    {
        await using var db = CreateDb();
        SeedCpsaTypes(db);
        var tie = Now.AddHours(-5);
        AddCompleted(db, 1, Now.AddDays(-3));
        AddCompleted(db, 2, tie);
        AddCompleted(db, 3, tie);
        AddCompleted(db, 4, Now.AddHours(-1));
        AddCompleted(db, 5, Now.AddDays(-1));
        AddCompleted(db, 6, Now.AddDays(-2));
        await db.SaveChangesAsync();

        var result = await DecidedAsync(db, take: 5);

        result.Select(item => item.Id).Should().Equal(4, 3, 2, 5, 6);
    }

    /// <summary>The credit is the latest transition that evaluated it, ties to the later row (T108).</summary>
    [Fact]
    public async Task EachDecision_CarriesTheLatestEvaluatedCredit()
    {
        await using var db = CreateDb();
        SeedCpsaTypes(db);
        var credited = Move("complete", "requested", "completed", AssessorId, Now.AddHours(-1));
        credited.CreditedItemCount = 1;
        AddFiled(db, 1, CpsaMiniCexTypeId, "completed", TraineeId, NamesTheAssessor, Now.AddHours(-1),
            Move("submit", "draft", "requested", TraineeId, Now.AddDays(-2)), credited);
        AddFiled(db, 2, CpsaMiniCexTypeId, "declined", TraineeId, NamesTheAssessor, Now.AddHours(-2),
            Move("decline", "requested", "declined", AssessorId, Now.AddHours(-2)));
        await db.SaveChangesAsync();

        var result = await DecidedAsync(db);

        result.Select(item => (item.Id, item.CreditedItemCount)).Should().Equal((1, 1), (2, (int?)null));
    }

    /// <summary>
    /// E7 over her whole list (note 1): a decided Mini-CEX that shares its type, EPA and date with a request still in hand
    /// is named with its assessor, as My activities names it, though the other row is no decision.
    /// </summary>
    [Fact]
    public async Task ANameCarriesItsNominee_WhenAnotherOfHerActivities_SharesTheRest()
    {
        await using var db = CreateDb();
        SeedCpsaTypes(db);
        var observedOn = new DateOnly(2026, 9, 13);
        var decided = AddCompleted(db, 1, Now.AddHours(-1));
        decided.EpaId = 5000;
        decided.ObservedOn = observedOn;
        decided.ObservedOnSource = ObservationDateSource.Declared;
        var inHand = AddFiled(db, 2, CpsaMiniCexTypeId, "requested", TraineeId, """{ "assessor_user_id": "assessor-2" }""",
            Now.AddHours(-1), Move("submit", "draft", "requested", TraineeId, Now.AddHours(-1)));
        inHand.EpaId = 5000;
        inHand.ObservedOn = observedOn;
        inHand.ObservedOnSource = ObservationDateSource.Declared;
        AddCompleted(db, 3, Now.AddHours(-2));
        await db.SaveChangesAsync();
        var directory = new FakeUserDirectory((AssessorId, "Sarah Botha"), ("assessor-2", "Fatima Khumalo"));

        var result = await DecidedAsync(db, directory: directory);

        result.Select(item => item.Id).Should().Equal(1, 3);
        result[0].DisplayNameHasNominee.Should().BeTrue();
        result[0].DisplayName.Should().EndWith(" · Sarah Botha");
        result[1].DisplayNameHasNominee.Should().BeFalse("no other activity of hers shares its type, EPA and date");
        result[1].NomineeName.Should().Be("Sarah Botha", "the link's second line, 'to Sarah Botha'");
        result.Should().AllSatisfy(item => item.SubjectName.Should().BeNull("her own list is all one person"));
    }

    /// <summary>
    /// Each decision about an in-force EPA of her curriculum carries the count it made (E5), the one
    /// <c>EpaCountLines</c> reads; a paused EPA and an activity about no EPA carry none.
    /// </summary>
    [Fact]
    public async Task ADecisionCarriesTheCountItMade_InItsEncountersWindow()
    {
        await using var db = CreateDb();
        Counts.SeedCurriculum(db);
        Counts.SeedTrainee(db, TraineeId, new DateOnly(2023, 1, 15), profileId: 1);
        Counts.Credit(db, TraineeId, Counts.Paed001, 2026, 1, 3);
        Counts.Credit(db, TraineeId, Counts.Paed001, 2026, 2, 1);
        SeedCpsaTypes(db);
        Stamp(AddCompleted(db, 1, Now.AddHours(-1)), Counts.Paed001, new DateOnly(2026, 9, 23));
        Stamp(AddCompleted(db, 2, Now.AddHours(-2)), Counts.Paed001, new DateOnly(2026, 6, 20));
        Stamp(AddCompleted(db, 3, Now.AddHours(-3)), Counts.Paed012, new DateOnly(2026, 9, 26));
        AddCompleted(db, 4, Now.AddHours(-4));
        await db.SaveChangesAsync();

        var result = await DecidedAsync(db, take: 10);

        var current = result.Single(item => item.Id == 1).CountLine!;
        (current.IsCurrentWindow, current.Window.Name, current.Window.Count).Should().Be((true, "Semester 2, 2026", 1));
        var older = result.Single(item => item.Id == 2).CountLine!;
        (older.IsCurrentWindow, older.Window.Name, older.Window.Count).Should().Be((false, "Semester 1, 2026", 3));
        result.Single(item => item.Id == 3).CountLine.Should().BeNull("PAED-012 is paused (D48)");
        result.Single(item => item.Id == 3).EpaInForce.Should().BeFalse();
        result.Single(item => item.Id == 4).CountLine.Should().BeNull("it is about no EPA");
    }

    [Fact]
    public async Task AnotherTraineesDecisions_AreNotHers()
    {
        await using var db = CreateDb();
        SeedCpsaTypes(db);
        AddCompleted(db, 1, Now.AddHours(-1));
        AddFiled(db, 2, CpsaMiniCexTypeId, "completed", "trainee-2", NamesTheAssessor, Now.AddHours(-1),
            Move("complete", "requested", "completed", AssessorId, Now.AddHours(-1)));
        await db.SaveChangesAsync();

        (await DecidedAsync(db)).Select(item => item.Id).Should().Equal(1);
    }

    /// <summary>The shipped teaching session (finishes in accepted) and procedure log (born terminal in logged).</summary>
    private static void SeedLogs(ApplicationDbContext db)
    {
        ShippedSeeds.AddType(db, TeachingSessionTypeId, "teaching_session", "Teaching session");
        ShippedSeeds.AddType(db, ProcedureTypeId, "procedure_log", "Procedure log");
    }

    private static Activity AddCompleted(ApplicationDbContext db, int id, DateTime decidedOn)
        => AddFiled(db, id, CpsaMiniCexTypeId, "completed", TraineeId, NamesTheAssessor, decidedOn,
            Move("submit", "draft", "requested", TraineeId, decidedOn.AddDays(-1)),
            Move("complete", "requested", "completed", AssessorId, decidedOn));

    private static void Stamp(Activity activity, int epaId, DateOnly observedOn)
    {
        activity.EpaId = epaId;
        activity.ObservedOn = observedOn;
        activity.ObservedOnSource = ObservationDateSource.Declared;
    }

    private static ClaimsPrincipal Trainee() => CreatePrincipal(TraineeId, "Trainee");

    private static Task<IReadOnlyList<ActivitySummaryDto>> DecidedAsync(
        ApplicationDbContext db, int take = 5, FakeUserDirectory? directory = null)
        => DecidedOnYours.ReadAsync(
            db, directory ?? FakeUserDirectory.Empty, Trainee(), take, Counts.Today, CancellationToken.None);
}
