using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Activities.Queries.GetProgrammeStartForTrainee;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T160: an encounter date may not be after today or before the trainee's programme began, and a filing more than
/// fourteen days after the encounter is recorded on the filing's history row, never refused (D15). Over the shipped
/// <c>mini_cex_cpsa</c> seed (draft-born: the filing is the trainee's <c>submit</c>) and the generic <c>mini_cex</c>
/// seed (born in <c>requested</c>: the create is the filing, T127). The system-written MSF path is covered in
/// <c>MsfEvidenceFanOutTests</c>.
/// </summary>
/// <remarks>
/// The programme-start bound and the lateness record protect credit, so they apply only to a type whose pinned credit
/// rules can credit (<see cref="EncounterDatePolicy.CanCredit" />). The shipped <c>research_output</c>,
/// <c>journal_club</c> and <c>reflective_exercise_cpsa</c> seeds credit nothing, and get the future check only.
/// </remarks>
/// <remarks>
/// "Today" is the South African calendar date of the service's clock, which every test here fixes (<see cref="Now" />),
/// so no run can straddle a midnight; every date is relative to it. Every refusal is checked against the audit trap, as <see cref="NomineeGateWritePathTests" /> does: nothing is left dirty in
/// the change tracker, the audit pipeline's save of the same context writes nothing, and no row is found afterwards.
/// </remarks>
public sealed class EncounterDateBoundsTests
{
    private const string TraineeId = "trainee-1";
    private const string AssessorId = "assessor-1";
    private const string UnadmittedId = "registrar-unadmitted";
    private const int InstitutionId = 10;

    private const int CpsaTypeId = 1;
    private const int RequestBornTypeId = 2;
    private const int UndatedTypeId = 3;
    private const int ReturnableTypeId = 4;
    private const int CreditingReturnableTypeId = 5;
    private const int ResearchOutputTypeId = 6;
    private const int JournalClubTypeId = 7;
    private const int PortfolioReviewTypeId = 8;

    private const int EpaId = 5000;

    /// <summary>Credit rules that can credit: one directive, through the form's EPA.</summary>
    private const string CreditingRules = """
        { "counts_for": [ { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1 } ] }
        """;

    /// <summary>The instant every service here is clocked at: mid-morning, well clear of either calendar's midnight.</summary>
    private static readonly DateTimeOffset Now = new(2026, 9, 24, 8, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly Today = ProgrammeCalendar.DateOf(Now.UtcDateTime);

    /// <summary>
    /// 22:30 UTC on 24 September: already 00:30 on 25 September in South Africa, where the encounter date is typed.
    /// </summary>
    private static readonly DateTimeOffset LateEveningUtc = new(2026, 9, 24, 22, 30, 0, TimeSpan.Zero);

    private static readonly DateOnly SouthAfricanDayAtLateEvening = new(2026, 9, 25);

    /// <summary>Two years back, so a date 200 days old is inside the programme.</summary>
    private static readonly DateOnly ProgrammeStart = Today.AddYears(-2);

    // ---- the future ------------------------------------------------------------------------------------------------

    [Fact]
    public async Task Create_DatedTomorrow_IsRefusedWithAFieldErrorOnTheDate_AndLeavesNoRow()
    {
        var options = await SeededAsync();

        var message = await ShouldBeRefusedAsync(options, service => service.CreateDraftAsync(
            CreateInput(CpsaTypeId, TraineeId, CpsaRequest(Today.AddDays(1)))));

        message.Should().Be($"Date observed: The date cannot be after today ({Iso(Today)}).");
        await AssertNoActivitiesAsync(options);
    }

    [Fact]
    public async Task ARefusedDate_CarriesTheDateFieldsKey_SoThePageCanMarkIt()
    {
        // T263: the programme-start bound too, which the page predicts only while the date is typed.
        var options = await SeededAsync();

        var future = await RefusalAsync(options, service => service.CreateDraftAsync(
            CreateInput(CpsaTypeId, TraineeId, CpsaRequest(Today.AddDays(1)))));
        var early = await RefusalAsync(options, service => service.CreateDraftAsync(
            CreateInput(CpsaTypeId, TraineeId, CpsaRequest(ProgrammeStart.AddDays(-1)))));

        future.Should().BeOfType<ActivityFieldsRefusedException>().Which.FieldKeys.Should().Equal("observed_on");
        early.Should().BeOfType<ActivityFieldsRefusedException>().Which.FieldKeys.Should().Equal("observed_on");
    }

    [Fact]
    public async Task Create_DatedToday_IsAccepted()
    {
        // The boundary, and the control: a gate that refused everything would pass the test above.
        var options = await SeededAsync();

        var draft = await CreateAsync(options, CpsaTypeId, CpsaRequest(Today));

        (await StoredAsync(options, draft.Id)).ObservedOn.Should().Be(Today);
    }

    [Fact]
    public async Task Create_ThatIsItselfTheFiling_DatedTomorrow_IsRefused_AndLeavesNoRow()
    {
        var options = await SeededAsync();

        var message = await ShouldBeRefusedAsync(options, service => service.CreateDraftAsync(
            CreateInput(RequestBornTypeId, TraineeId, LegacyRequest(Today.AddDays(1)))));

        message.Should().StartWith("Date observed: ").And.Contain("cannot be after today");
        await AssertNoActivitiesAsync(options);
    }

    // ---- the programme start ---------------------------------------------------------------------------------------

    [Fact]
    public async Task Create_DatedBeforeTheProgrammeStarted_IsRefused_AndLeavesNoRow()
    {
        var options = await SeededAsync();

        var message = await ShouldBeRefusedAsync(options, service => service.CreateDraftAsync(
            CreateInput(CpsaTypeId, TraineeId, CpsaRequest(ProgrammeStart.AddDays(-1)))));

        message.Should().Be(
            $"Date observed: The date cannot be before your programme started ({Iso(ProgrammeStart)}).");
        await AssertNoActivitiesAsync(options);
    }

    [Fact]
    public async Task Create_DatedOnTheDayTheProgrammeStarted_IsAccepted()
    {
        var options = await SeededAsync();

        var draft = await CreateAsync(options, CpsaTypeId, CpsaRequest(ProgrammeStart));

        (await StoredAsync(options, draft.Id)).ObservedOn.Should().Be(ProgrammeStart);
    }

    [Fact]
    public async Task TheStartTheFormIsGiven_IsTheOneThisBoundRefusesBy()
    {
        // T192: the form's hint reads GetProgrammeStartForTraineeQuery, and this refusal reads the profile credit picks.
        // With a second, past profile that started a year later and has the higher id, a query that ranked by either
        // would hint at a day the server does not refuse by.
        var options = await SeededAsync();
        await using (var db = new ApplicationDbContext(options))
        {
            db.Set<TraineeProfile>().Add(new TraineeProfile
            {
                Id = 2,
                UserId = TraineeId,
                InstitutionId = InstitutionId,
                CurriculumId = 3000,
                ProgrammeStartDate = ProgrammeStart.AddYears(1),
                ExpectedCompletionDate = ProgrammeStart.AddYears(5),
                IsActive = false
            });
            await db.SaveChangesAsync();
        }

        DateOnly? hinted;
        await using (var db = new ApplicationDbContext(options))
        {
            hinted = await new GetProgrammeStartForTraineeQueryHandler(db).Handle(
                new GetProgrammeStartForTraineeQuery(TraineeId, Principal(TraineeId)), CancellationToken.None);
        }

        hinted.Should().Be(ProgrammeStart);
        (await ShouldBeRefusedAsync(options, service => service.CreateDraftAsync(
            CreateInput(CpsaTypeId, TraineeId, CpsaRequest(hinted!.Value.AddDays(-1))))))
            .Should().EndWith($"before your programme started ({Iso(hinted!.Value)}).");
        (await CreateAsync(options, CpsaTypeId, CpsaRequest(hinted!.Value))).ObservedOn.Should().Be(hinted);
    }

    [Fact]
    public async Task ASubjectWithNoTraineeProfile_GetsTheFutureCheckOnly()
    {
        var options = await SeededAsync();

        var old = await CreateAsync(options, CpsaTypeId, CpsaRequest(new DateOnly(2019, 3, 1)), UnadmittedId);
        (await StoredAsync(options, old.Id)).ObservedOn.Should().Be(new DateOnly(2019, 3, 1), "there is no programme to precede");

        var message = await ShouldBeRefusedAsync(options, service => service.CreateDraftAsync(
            CreateInput(CpsaTypeId, UnadmittedId, CpsaRequest(Today.AddDays(1)))));
        message.Should().Contain("cannot be after today");
    }

    // ---- a transition's patch --------------------------------------------------------------------------------------

    [Fact]
    public async Task Submit_WhosePatchMovesTheDateToTomorrow_IsRefused_AndChangesNothing()
    {
        var options = await SeededAsync();
        var draft = await CreateAsync(options, CpsaTypeId, CpsaRequest(Today.AddDays(-3)));

        var message = await ShouldBeRefusedAsync(options, service => service.TransitionAsync(
            TransitionInput(draft.Id, "submit", TraineeId, DatePatch(Today.AddDays(1)))));

        message.Should().StartWith("Date observed: ").And.Contain("cannot be after today");
        var stored = await StoredAsync(options, draft.Id);
        stored.CurrentState.Should().Be("draft");
        stored.ObservedOn.Should().Be(Today.AddDays(-3));
        stored.Transitions.Should().ContainSingle("the refused move left no history row");
    }

    [Fact]
    public async Task AChangedDate_IsJudgedOnAMoveIntoADeadEnd_Too()
    {
        // A changed date is judged on every move that writes it, as a changed nominee is (T102): the stamp is rewritten
        // from it on any move, withdrawal included.
        var options = await SeededAsync();
        var draft = await CreateAsync(options, CpsaTypeId, CpsaRequest(Today.AddDays(-3)));

        var message = await ShouldBeRefusedAsync(options, service => service.TransitionAsync(
            TransitionInput(draft.Id, "cancel", TraineeId, DatePatch(ProgrammeStart.AddDays(-10)))));

        message.Should().Contain("before your programme started");
        (await StoredAsync(options, draft.Id)).CurrentState.Should().Be("draft");
    }

    [Fact]
    public async Task Submit_WhosePatchCorrectsTheDate_IsAccepted_AndMovesTheStamp()
    {
        var options = await SeededAsync();
        var draft = await CreateAsync(options, CpsaTypeId, CpsaRequest(Today.AddDays(-3)));

        await TransitionAsync(options, draft.Id, "submit", TraineeId, DatePatch(Today.AddDays(-2)));

        (await StoredAsync(options, draft.Id)).ObservedOn.Should().Be(Today.AddDays(-2));
    }

    // ---- an unchanged date: judged at the author's hand-on only ---------------------------------------------------

    [Fact]
    public async Task AnUnchangedDate_ThatNowPrecedesTheProgramme_IsRefusedAtTheAuthorsSubmit()
    {
        // Saved while it was valid; the programme start then moved past it. The trainee's submit hands the date on,
        // and it is the last moment they can still correct it.
        var options = await SeededAsync();
        var draft = await CreateAsync(options, CpsaTypeId, CpsaRequest(Today.AddDays(-30)));
        await MoveProgrammeStartAsync(options, Today.AddDays(-10));

        var message = await ShouldBeRefusedAsync(options, service => service.TransitionAsync(
            TransitionInput(draft.Id, "submit", TraineeId, patch: null)));

        message.Should().Contain("before your programme started");
        (await StoredAsync(options, draft.Id)).CurrentState.Should().Be("draft");
    }

    [Fact]
    public async Task AnUnchangedDate_ThatNowPrecedesTheProgramme_DoesNotStrandTheAssessorsCompletion()
    {
        var options = await SeededAsync();
        var draft = await CreateAsync(options, CpsaTypeId, CpsaRequest(Today.AddDays(-30)));
        await TransitionAsync(options, draft.Id, "submit", TraineeId);
        await MoveProgrammeStartAsync(options, Today.AddDays(-10));

        // Guard: the date really is refusable now, or the completion below would pass for the wrong reason.
        (await ShouldBeRefusedAsync(options, service => service.CreateDraftAsync(
            CreateInput(CpsaTypeId, TraineeId, CpsaRequest(Today.AddDays(-30))))))
            .Should().Contain("before your programme started");

        var completed = await TransitionAsync(options, draft.Id, "complete", AssessorId, CompletionPatch);

        completed.CurrentState.Should().Be("completed", "the assessor cannot correct a date they cannot write");
    }

    // ---- lateness: recorded on the filing, never refused -----------------------------------------------------------

    [Fact]
    public async Task ASubmitFifteenDaysAfterTheEncounter_IsFiled_AndItsRowRecordsTheLateness()
    {
        var options = await SeededAsync();
        var draft = await CreateAsync(options, CpsaTypeId, CpsaRequest(Today.AddDays(-15)));

        var submitted = await TransitionAsync(options, draft.Id, "submit", TraineeId);

        submitted.CurrentState.Should().Be("requested", "lateness is never a refusal (D15)");
        var submit = submitted.Transitions.Single(transition => transition.TransitionKey == "submit");
        submit.DaysAfterEncounter.Should().Be(15);
        EncounterDatePolicy.IsLateFiling(submit.DaysAfterEncounter!.Value).Should().BeTrue();

        submitted.Transitions.Single(transition => transition.TransitionKey == "create").DaysAfterEncounter
            .Should().BeNull("a draft-born type's create is a draft save, not the filing");
        (await StoredAsync(options, draft.Id)).Transitions.Single(transition => transition.TransitionKey == "submit")
            .DaysAfterEncounter.Should().Be(15, "the record is on the stored row");
    }

    [Fact]
    public async Task ASubmitExactlyFourteenDaysAfterTheEncounter_IsOnTime()
    {
        var options = await SeededAsync();
        var draft = await CreateAsync(options, CpsaTypeId, CpsaRequest(Today.AddDays(-EncounterDatePolicy.LateFilingDays)));

        var submitted = await TransitionAsync(options, draft.Id, "submit", TraineeId);

        var submit = submitted.Transitions.Single(transition => transition.TransitionKey == "submit");
        submit.DaysAfterEncounter.Should().Be(14);
        EncounterDatePolicy.IsLateFiling(submit.DaysAfterEncounter!.Value).Should().BeFalse();
    }

    [Fact]
    public async Task AFilingTwoHundredDaysLate_IsStillAccepted()
    {
        var options = await SeededAsync();
        var draft = await CreateAsync(options, CpsaTypeId, CpsaRequest(Today.AddDays(-200)));

        var submitted = await TransitionAsync(options, draft.Id, "submit", TraineeId);

        submitted.CurrentState.Should().Be("requested");
        submitted.Transitions.Single(transition => transition.TransitionKey == "submit").DaysAfterEncounter.Should().Be(200);
    }

    [Fact]
    public async Task WhenTheCreateIsTheFiling_ItsOwnRowRecordsTheLateness_AndTheAssessorsMoveRecordsNothing()
    {
        var options = await SeededAsync();

        var filed = await CreateAsync(options, RequestBornTypeId, LegacyRequest(Today.AddDays(-20)));

        filed.CurrentState.Should().Be("requested");
        filed.Transitions.Single().DaysAfterEncounter.Should().Be(20);

        // The assessor's accept also leaves the initial state and leads on, but it is not the author's: not a filing.
        var accepted = await TransitionAsync(options, filed.Id, "accept", AssessorId);
        accepted.Transitions.Single(transition => transition.TransitionKey == "accept").DaysAfterEncounter.Should().BeNull();
    }

    [Fact]
    public async Task OnlyTheFilingRecordsIt_NotTheAssessorsCompletion()
    {
        var options = await SeededAsync();
        var draft = await CreateAsync(options, CpsaTypeId, CpsaRequest(Today.AddDays(-20)));
        await TransitionAsync(options, draft.Id, "submit", TraineeId);

        var completed = await TransitionAsync(options, draft.Id, "complete", AssessorId, CompletionPatch);

        completed.Transitions.Single(transition => transition.TransitionKey == "complete").DaysAfterEncounter.Should().BeNull();
    }

    [Fact]
    public async Task AFilingWithNoStatedDate_RecordsNothing()
    {
        // A pinned schema with no pointer: the encounter date is the filing date, and "0 days late" would be a claim
        // nobody made.
        var options = await SeededAsync();
        var draft = await CreateAsync(options, UndatedTypeId, CpsaRequest(Today.AddDays(-20)));

        var submitted = await TransitionAsync(options, draft.Id, "submit", TraineeId);

        submitted.Transitions.Single(transition => transition.TransitionKey == "submit").DaysAfterEncounter.Should().BeNull();
    }

    // ---- only the first filing records it ---------------------------------------------------------------------

    [Fact]
    public async Task AResubmissionAfterAReturn_IsNotASecondFiling()
    {
        // reflective_exercise_cpsa's workflow, on a type that can credit: the supervisor may `return` a submission to the
        // draft. The trainee filed on day 3; the delay before the re-submission is the supervisor's, and recording it
        // would call the filing late. (The shipped seed credits nothing, so it records no lateness at all: see below.)
        var options = await SeededAsync();
        var draft = await CreateAsync(options, CreditingReturnableTypeId, ReflectionRequest(Today.AddDays(-3)));
        await TransitionAsync(options, draft.Id, "submit", TraineeId);

        var returned = await TransitionAsync(options, draft.Id, "return", AssessorId, note: "Say more about the plan.");
        returned.CurrentState.Should().Be("draft", "guard: the activity really is back in its initial state");

        var resubmitted = await TransitionAsync(options, draft.Id, "submit", TraineeId, clock: new FixedClock(Now.AddDays(20)));

        resubmitted.Transitions
            .Select(transition => $"{transition.TransitionKey}:{transition.DaysAfterEncounter}")
            .Should().Equal("create:", "submit:3", "return:", "submit:");
    }

    // ---- a type that credits nothing: the future check only, and no lateness --------------------------------------

    [Theory]
    [InlineData("mini_cex_cpsa", true)]
    [InlineData("mini_cex", true)]
    [InlineData("research_output", false)]
    [InlineData("journal_club", false)]
    [InlineData("reflective_exercise_cpsa", false)]
    public void TheSeedsCreditAsTheseTestsAssume(string seed, bool credits)
    {
        // The premise of every test here, read from the seed directly rather than through the predicate under test: a
        // seed that starts or stops crediting fails here, not as a mystery in a test of the other kind.
        CreditRulesParser.Parse(ReadSeed(seed, "credit.json")).CountsFor.Any().Should().Be(credits);
    }

    [Fact]
    public async Task AResearchOutput_DatedBeforeTheProgrammeStarted_IsAccepted()
    {
        // Work from before the trainee was admitted. The bound protects the stage minimum credit picks (T119), and a
        // research output credits nothing.
        var options = await SeededAsync();
        var beforeTheProgramme = ProgrammeStart.AddDays(-100);

        // Guard: the same date on a type that can credit is refused, so the acceptance below is the predicate's doing.
        (await ShouldBeRefusedAsync(options, service => service.CreateDraftAsync(
            CreateInput(CpsaTypeId, TraineeId, CpsaRequest(beforeTheProgramme)))))
            .Should().Contain("before your programme started");

        var draft = await CreateAsync(options, ResearchOutputTypeId, ResearchOutput(beforeTheProgramme));
        (await StoredAsync(options, draft.Id)).ObservedOn.Should().Be(beforeTheProgramme);

        var submitted = await TransitionAsync(options, draft.Id, "submit", TraineeId);
        submitted.CurrentState.Should().Be("submitted", "the author's hand-on does not judge it against the programme either");
    }

    [Fact]
    public async Task AResearchOutput_DatedTomorrow_IsStillRefused()
    {
        // The future check is about the date being true, not about credit, so it applies to every dated type.
        var options = await SeededAsync();

        var message = await ShouldBeRefusedAsync(options, service => service.CreateDraftAsync(
            CreateInput(ResearchOutputTypeId, TraineeId, ResearchOutput(Today.AddDays(1)))));

        message.Should().Be($"Date: The date cannot be after today ({Iso(Today)}).");
        await AssertNoActivitiesAsync(options);
    }

    [Fact]
    public async Task APortfolioReview_DatedTomorrow_IsRefusedUnderItsOwnFieldsLabel_NotAsAnEncounterDate()
    {
        // T197: the date the gate judges is whatever the type calls it. A portfolio review is dated by the last day of
        // its period, and the refusal read "Review period to: The encounter date cannot be after today" there.
        var options = await SeededAsync();

        var message = await ShouldBeRefusedAsync(options, service => service.CreateDraftAsync(
            CreateInput(PortfolioReviewTypeId, TraineeId, PortfolioReview(Today.AddDays(1)))));

        message.Should().Be($"Review period to: The date cannot be after today ({Iso(Today)}).");
        message.Should().NotContainEquivalentOf("encounter");
        await AssertNoActivitiesAsync(options);
    }

    [Fact]
    public async Task AnUnchangedResearchOutputDate_ThatNowPrecedesTheProgramme_IsNotRefusedAtTheAuthorsSubmit()
    {
        // The crediting counterpart is refused at the same hand-on (AnUnchangedDate_ThatNowPrecedesTheProgramme_...).
        var options = await SeededAsync();
        var draft = await CreateAsync(options, ResearchOutputTypeId, ResearchOutput(Today.AddDays(-30)));
        await MoveProgrammeStartAsync(options, Today.AddDays(-10));

        var submitted = await TransitionAsync(options, draft.Id, "submit", TraineeId);

        submitted.CurrentState.Should().Be("submitted");
    }

    [Fact]
    public async Task AResearchOutputSubmittedLate_RecordsNoLateness()
    {
        // Thirty days after: a crediting type's submit records 30 (and its history calls it late). A type that credits
        // nothing is late for nobody (D15 is about WBA filing), so nothing is recorded and no label can appear.
        var options = await SeededAsync();
        var draft = await CreateAsync(options, ResearchOutputTypeId, ResearchOutput(Today.AddDays(-30)));

        var submitted = await TransitionAsync(options, draft.Id, "submit", TraineeId);

        submitted.Transitions.Should().OnlyContain(transition => transition.DaysAfterEncounter == null);
        (await StoredAsync(options, draft.Id)).Transitions.Should().OnlyContain(transition => transition.DaysAfterEncounter == null);
    }

    [Fact]
    public async Task AJournalClub_WhoseCreateIsTheFiling_MayPrecedeTheProgramme_AndRecordsNoLateness()
    {
        // Born terminal: the create is the whole record, and so the filing (T127). The create path asks the same
        // predicate as the submit.
        var options = await SeededAsync();
        var beforeTheProgramme = ProgrammeStart.AddDays(-5);

        var logged = await CreateAsync(options, JournalClubTypeId, JournalClub(beforeTheProgramme));

        logged.CurrentState.Should().Be("logged");
        logged.Transitions.Single().DaysAfterEncounter.Should().BeNull();
        (await StoredAsync(options, logged.Id)).ObservedOn.Should().Be(beforeTheProgramme);
    }

    [Fact]
    public async Task TheShippedReflectiveExercise_CreditsNothing_AndRecordsNoLateness()
    {
        var options = await SeededAsync();
        var draft = await CreateAsync(options, ReturnableTypeId, ReflectionRequest(Today.AddDays(-20)));

        var submitted = await TransitionAsync(options, draft.Id, "submit", TraineeId);

        submitted.CurrentState.Should().Be("submitted");
        submitted.Transitions.Should().OnlyContain(transition => transition.DaysAfterEncounter == null);
    }

    // ---- the South African calendar --------------------------------------------------------------------------------

    [Fact]
    public async Task AtHalfPastTenUtc_AnEncounterDatedTheSouthAfricanDay_IsNotTheFuture()
    {
        // On the UTC calendar it is still the 24th, so an encounter dated the 25th would be refused as tomorrow: a
        // registrar filing just after midnight would be told the encounter they just had has not happened.
        var options = await SeededAsync();
        var clock = new FixedClock(LateEveningUtc);

        var created = await CreateAsync(options, CpsaTypeId, CpsaRequest(SouthAfricanDayAtLateEvening), clock: clock);
        (await StoredAsync(options, created.Id)).ObservedOn.Should().Be(SouthAfricanDayAtLateEvening);

        var draft = await CreateAsync(options, CpsaTypeId, CpsaRequest(Today.AddDays(-3)), clock: clock);
        var submitted = await TransitionAsync(
            options, draft.Id, "submit", TraineeId, DatePatch(SouthAfricanDayAtLateEvening), clock: clock);
        submitted.CurrentState.Should().Be("requested");

        var refused = await ShouldBeRefusedAsync(options, service => service.CreateDraftAsync(
            CreateInput(CpsaTypeId, TraineeId, CpsaRequest(SouthAfricanDayAtLateEvening.AddDays(1)))), clock);
        refused.Should().Be($"Date observed: The date cannot be after today ({Iso(SouthAfricanDayAtLateEvening)}).");
    }

    [Fact]
    public async Task AtHalfPastTenUtc_TheLatenessIsCountedToTheSouthAfricanDay()
    {
        // Fifteen South African days after the encounter, and only fourteen UTC ones: late, and recorded as 15.
        var options = await SeededAsync();
        var clock = new FixedClock(LateEveningUtc);
        var encounteredOn = SouthAfricanDayAtLateEvening.AddDays(-15);

        var draft = await CreateAsync(options, CpsaTypeId, CpsaRequest(encounteredOn), clock: clock);
        var submitted = await TransitionAsync(options, draft.Id, "submit", TraineeId, clock: clock);

        var submit = submitted.Transitions.Single(transition => transition.TransitionKey == "submit");
        submit.OccurredOn.Should().Be(LateEveningUtc.UtcDateTime, "the row is stamped with the instant it was judged at");
        submit.DaysAfterEncounter.Should().Be(15);

        var filed = await CreateAsync(options, RequestBornTypeId, LegacyRequest(encounteredOn), clock: clock);
        filed.Transitions.Single().DaysAfterEncounter.Should().Be(15, "the create that files it counts the same way");
    }

    // ---- helpers ---------------------------------------------------------------------------------------------------

    private const string CompletionPatch = """
        { "overall_level": 3, "strengths": "Clear.", "improvements": "Earlier escalation.", "plan": "Repeat." }
        """;

    private static string Iso(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string DatePatch(DateOnly date) => $$"""{ "observed_on": "{{Iso(date)}}" }""";

    private static string CpsaRequest(DateOnly observedOn) => $$"""
        {
          "epa_id": {{EpaId}},
          "assessor_user_id": "{{AssessorId}}",
          "observed_on": "{{Iso(observedOn)}}",
          "setting": "ward",
          "presenting_problem": "Bronchiolitis",
          "complexity": "moderate"
        }
        """;

    private static string LegacyRequest(DateOnly observedOn) => $$"""
        {
          "epa_id": {{EpaId}},
          "assessor_user_id": "{{AssessorId}}",
          "observed_on": "{{Iso(observedOn)}}",
          "setting": "ward",
          "presenting_complaint": "Bronchiolitis",
          "complexity": "moderate"
        }
        """;

    private static string ReflectionRequest(DateOnly observedOn) => $$"""
        {
          "epa_id": {{EpaId}},
          "assessor_user_id": "{{AssessorId}}",
          "observed_on": "{{Iso(observedOn)}}",
          "prompt": "challenging_case",
          "what_happened": "A difficult conversation.",
          "analysis": "Why it went as it did.",
          "learning": "What I would keep.",
          "action_plan": "What I will change."
        }
        """;

    private static string ResearchOutput(DateOnly activityDate) => $$"""
        {
          "output_type": "journal_article",
          "title": "Bronchiolitis admissions over two winters",
          "authors": "A. Trainee, B. Supervisor",
          "venue": "South African Medical Journal",
          "activity_date": "{{Iso(activityDate)}}",
          "trainee_role": "first_author",
          "abstract": "A retrospective review."
        }
        """;

    /// <summary>A portfolio review request for the half-year ending <paramref name="periodTo" />.</summary>
    private static string PortfolioReview(DateOnly periodTo) => $$"""
        {
          "epa_id": {{EpaId}},
          "assessor_user_id": "{{AssessorId}}",
          "period_from": "{{Iso(periodTo.AddMonths(-6))}}",
          "period_to": "{{Iso(periodTo)}}"
        }
        """;

    private static string JournalClub(DateOnly sessionDate) => $$"""
        {
          "session_date": "{{Iso(sessionDate)}}",
          "article_reference": "A trial of high-flow oxygen in bronchiolitis.",
          "trainee_role": "presenter",
          "key_learning_points": "When high-flow helps."
        }
        """;

    private static CreateActivityInput CreateInput(int typeId, string subjectId, string dataJson)
        => new(typeId, subjectId, subjectId, dataJson, Principal(subjectId));

    private static TransitionActivityInput TransitionInput(
        int activityId,
        string transitionKey,
        string actorId,
        string? patch,
        string? note = null)
        => new(activityId, transitionKey, actorId, Principal(actorId), patch, note);

    private static async Task<ActivityDto> CreateAsync(
        DbContextOptions<ApplicationDbContext> options,
        int typeId,
        string dataJson,
        string subjectId = TraineeId,
        TimeProvider? clock = null)
    {
        await using var db = new ApplicationDbContext(options);
        return await Service(db, clock).CreateDraftAsync(CreateInput(typeId, subjectId, dataJson));
    }

    private static async Task<ActivityDto> TransitionAsync(
        DbContextOptions<ApplicationDbContext> options,
        int activityId,
        string transitionKey,
        string actorId,
        string? patch = null,
        string? note = null,
        TimeProvider? clock = null)
    {
        await using var db = new ApplicationDbContext(options);
        return await Service(db, clock).TransitionAsync(TransitionInput(activityId, transitionKey, actorId, patch, note));
    }

    /// <summary>
    /// Runs one refused request on its own context, asserts it left nothing for the audit save to commit, performs that
    /// save as the audit pipeline's catch would, and returns the refusal message.
    /// </summary>
    private static async Task<string> ShouldBeRefusedAsync(
        DbContextOptions<ApplicationDbContext> options,
        Func<ActivityService, Task> act,
        TimeProvider? clock = null)
        => (await RefusalAsync(options, act, clock)).Message;

    /// <summary><see cref="ShouldBeRefusedAsync" />, returning the refusal itself.</summary>
    private static async Task<InvalidOperationException> RefusalAsync(
        DbContextOptions<ApplicationDbContext> options,
        Func<ActivityService, Task> act,
        TimeProvider? clock = null)
    {
        await using var db = new ApplicationDbContext(options);

        var attempt = async () => await act(Service(db, clock));
        var thrown = await attempt.Should().ThrowAsync<InvalidOperationException>();

        db.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(entry => $"{entry.Metadata.ClrType.Name}: {entry.State}")
            .Should().BeEmpty("the audit pipeline's catch saves this context, so anything dirty here would be committed");

        (await db.SaveChangesAsync()).Should().Be(0);
        db.ChangeTracker.Clear();

        return thrown.Which;
    }

    private static async Task AssertNoActivitiesAsync(DbContextOptions<ApplicationDbContext> options)
    {
        await using var verify = new ApplicationDbContext(options);
        (await verify.Activities.CountAsync()).Should().Be(0, "a refused create must leave no activity behind");
        (await verify.ActivityTransitions.CountAsync()).Should().Be(0);
    }

    private static async Task<Activity> StoredAsync(DbContextOptions<ApplicationDbContext> options, int activityId)
    {
        await using var db = new ApplicationDbContext(options);
        return await db.Activities
            .AsNoTracking()
            .Include(entity => entity.Transitions)
            .SingleAsync(entity => entity.Id == activityId);
    }

    /// <summary>What an administrator's correction of the trainee's start date does, in its own request.</summary>
    private static async Task MoveProgrammeStartAsync(DbContextOptions<ApplicationDbContext> options, DateOnly startedOn)
    {
        await using var db = new ApplicationDbContext(options);
        var profile = await db.Set<TraineeProfile>().SingleAsync(entity => entity.UserId == TraineeId);
        profile.ProgrammeStartDate = startedOn;
        await db.SaveChangesAsync();
    }

    private static async Task<DbContextOptions<ApplicationDbContext>> SeededAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);

        NomineeSeed.AddUser(db, TraineeId, InstitutionId, WombatRoles.Trainee);
        NomineeSeed.AddUser(db, UnadmittedId, InstitutionId);
        NomineeSeed.AddUser(db, AssessorId, InstitutionId, WombatRoles.Assessor);

        // An EPA on no curriculum item: the tool gate is unrestricted for it (D21) and credit matches nothing, so these
        // tests exercise the date alone.
        db.Epas.Add(new Epa { Id = EpaId, Code = "PAED-001", Title = "An EPA" });
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = TraineeId,
            InstitutionId = InstitutionId,
            CurriculumId = 3000,
            ProgrammeStartDate = ProgrammeStart,
            ExpectedCompletionDate = ProgrammeStart.AddYears(4),
            IsActive = true
        });

        var cpsaSchema = ReadSeed("mini_cex_cpsa", "schema.json");
        db.ActivityTypes.Add(PublishedType(
            CpsaTypeId, "mini_cex_cpsa", cpsaSchema, ReadSeed("mini_cex_cpsa", "workflow.json"),
            ReadSeed("mini_cex_cpsa", "credit.json"), "mini_cex"));
        db.ActivityTypes.Add(PublishedType(
            RequestBornTypeId, "mini_cex", ReadSeed("mini_cex", "schema.json"), ReadSeed("mini_cex", "workflow.json"),
            ReadSeed("mini_cex", "credit.json"), wbaToolKey: null));

        // The CPSA form without its pointer, so nothing states the encounter date. With the seed's own credit rules, so
        // the type can credit and only the missing pointer keeps the lateness from being recorded.
        var undatedSchema = cpsaSchema.Replace("\"observation_date_field\": \"observed_on\",", string.Empty, StringComparison.Ordinal);
        undatedSchema.Should().NotBe(cpsaSchema, "guard: the pointer must really be gone");
        db.ActivityTypes.Add(PublishedType(
            UndatedTypeId, "mini_cex_cpsa_undated", undatedSchema, ReadSeed("mini_cex_cpsa", "workflow.json"),
            ReadSeed("mini_cex_cpsa", "credit.json"), wbaToolKey: null));

        // The one shipped seed whose workflow brings a filed activity back to its initial state (`return`). It credits
        // nothing, so the first-filing rule is shown on the same schema and workflow with rules that can credit.
        var reflectionSchema = ReadSeed("reflective_exercise_cpsa", "schema.json");
        var reflectionWorkflow = ReadSeed("reflective_exercise_cpsa", "workflow.json");
        var reflectionCredit = ReadSeed("reflective_exercise_cpsa", "credit.json");
        db.ActivityTypes.Add(PublishedType(
            ReturnableTypeId, "reflective_exercise_cpsa", reflectionSchema, reflectionWorkflow, reflectionCredit,
            wbaToolKey: null));
        db.ActivityTypes.Add(PublishedType(
            CreditingReturnableTypeId, "reflective_exercise_crediting", reflectionSchema, reflectionWorkflow, CreditingRules,
            wbaToolKey: null));

        // Two more shipped seeds that credit nothing (TheSeedsCreditAsTheseTestsAssume pins it).
        db.ActivityTypes.Add(PublishedType(
            ResearchOutputTypeId, "research_output", ReadSeed("research_output", "schema.json"),
            ReadSeed("research_output", "workflow.json"), ReadSeed("research_output", "credit.json"), wbaToolKey: null));
        db.ActivityTypes.Add(PublishedType(
            JournalClubTypeId, "journal_club", ReadSeed("journal_club", "schema.json"),
            ReadSeed("journal_club", "workflow.json"), ReadSeed("journal_club", "credit.json"), wbaToolKey: null));
        db.ActivityTypes.Add(PublishedType(
            PortfolioReviewTypeId, "portfolio_review_cpsa", ReadSeed("portfolio_review_cpsa", "schema.json"),
            ReadSeed("portfolio_review_cpsa", "workflow.json"), ReadSeed("portfolio_review_cpsa", "credit.json"),
            wbaToolKey: null));

        await db.SaveChangesAsync();
        return options;
    }

    private static ActivityType PublishedType(
        int id,
        string key,
        string schemaJson,
        string workflowJson,
        string creditJson,
        string? wbaToolKey)
    {
        var type = new ActivityType
        {
            Id = id,
            Key = key,
            Name = key,
            Scope = ActivityScope.Global,
            Version = 1,
            WbaToolKey = wbaToolKey,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditJson,
            DisplayFieldsJson = "[]",
            OwnerUserId = "system",
            CreatedOn = DateTime.UtcNow
        };
        type.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = id,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditJson,
            DisplayFieldsJson = "[]",
            PublishedByUserId = "system",
            PublishedOn = DateTime.UtcNow
        });
        return type;
    }

    private static string ReadSeed(string folder, string file)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", folder, file));

    private static ActivityService Service(ApplicationDbContext db, TimeProvider? clock = null)
        => new(
            db,
            new SchemaValidator(),
            new WorkflowEvaluator(),
            new CreditApplier(db),
            new FieldPermissionEvaluator(),
            clock ?? new FixedClock(Now));

    private static ClaimsPrincipal Principal(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
