using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Application.Features.Activities.Queries.ListActivityTypes;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T120's evidence run, repeated for each new instrument over its shipped seed files: a trainee files it against an EPA
/// whose College list names it, an assessor rates it at 3a, and it credits on the CPSA ladder. Plus the three unrated
/// instruments, which credit nothing (D6, D7): T120's reflective exercise, and T154's clinical audit (its report a link,
/// D34) and portfolio and logbook review (a signed review of a period, read from the portfolio export). Each unrated
/// instrument is held to the College's lists by the EPA it is evidence for (T154): it credits nothing, but a list says
/// which instruments are evidence for its EPA.
/// </summary>
public sealed class RemainingWbaToolsTests
{
    private const string TraineeId = "trainee-1";
    private const string AssessorId = "assessor-1";
    private const int InstitutionId = 10;

    private const int CpsaScaleId = 901;
    private const string CpsaScaleName = "CPSA Paediatric Entrustment Scale v11.1";
    private const string CpsaScaleSeedKey = "cpsa:scale:v11.1";

    private const int CurriculumId = 3000;
    private const int PermittingEpaId = 5000;
    private const int PermittingItemId = 4000;
    private const int ForbiddingEpaId = 5001;
    private const int ForbiddingItemId = 4001;

    // Order 3 on the CPSA ladder is rung "3a"; the curriculum's minimum is the same rung.
    private const int RungThreeA = 3;

    private static readonly DateOnly ProgrammeStart = new(2026, 1, 12);

    public static TheoryData<string, string, string> RatedTools => new()
    {
        { "cca_cpsa", "cca", """{ "case_reference": "Bed 4, 12 March", "documents_reviewed": ["admission_notes", "discharge_summary"], "setting": "ward", "reasoning_discussed": "Fluid plan in bronchiolitis." }""" },
        { "rca_cpsa", "rca", """{ "case_reference": "Clinic list, 3 March", "selection_method": "chosen_at_random_by_assessor", "setting": "outpatient_clinic" }""" },
        { "chart_stimulated_recall_cpsa", "chart_stimulated_recall", """{ "record_reference": "Ward round, 5 March", "setting": "ward", "decisions_probed": "Why oral antibiotics were stopped on day two." }""" },
    };

    [Theory]
    [MemberData(nameof(RatedTools))]
    public async Task ARatedTool_FiledAgainstAPermittingEpa_AndRatedAt3a_Credits(string seedKey, string toolKey, string requestFieldsJson)
    {
        var options = await SeededAsync(seedKey, toolKey);

        var draft = await CreateAsync(options, WithRequest(requestFieldsJson, PermittingEpaId));
        await TransitionAsync(options, draft.Id, "submit", TraineeId);
        var completed = await TransitionAsync(options, draft.Id, "complete", AssessorId, patch: $$"""
            { "overall_level": {{RungThreeA}}, "strengths": "Clear.", "improvements": "Earlier escalation.", "plan": "Repeat." }
            """);

        completed.CurrentState.Should().Be("completed");
        var record = completed.Transitions.Single(transition => transition.TransitionKey == "complete");
        record.CreditedItemCount.Should().Be(1);
        record.CreditScaleMismatchCount.Should().Be(0, "the seed binds the CPSA ladder, by its seed key since T253");

        await using var db = new ApplicationDbContext(options);
        var progress = await db.CurriculumItemProgresses.SingleAsync();
        progress.CurriculumItemId.Should().Be(PermittingItemId);
        progress.CountsSoFar.Should().Be(1);
        progress.MinimumLevelReachedCount.Should().Be(1, "3a meets a 3a minimum on the same ladder");
        // The seed's binding resolved: a key that bound nothing would still pass, unverified, as bare ordinals.
        progress.UnverifiedLevelCount.Should().Be(0);
        progress.MinimumLevelScaleId.Should().Be(CpsaScaleId, "the rating was scored on the ladder the seed binds");

        // The guard for the unrated instruments' empty trajectories below: on this fixture a rated tool does chart.
        var trajectory = (await TrajectoryAsync(options)).Should().ContainSingle().Subject;
        trajectory.EpaId.Should().Be(PermittingEpaId);
        trajectory.Points.Select(point => point.Rating).Should().Equal(RungThreeA);
    }

    [Theory]
    [MemberData(nameof(RatedTools))]
    public async Task ARatedTool_FiledAgainstAnEpaWhoseListDoesNotNameIt_IsRefused(string seedKey, string toolKey, string requestFieldsJson)
    {
        // The T122 gate applies from the first line: the seed's catalogue entry declares which instrument it is.
        var options = await SeededAsync(seedKey, toolKey);

        var attempt = () => CreateAsync(options, WithRequest(requestFieldsJson, ForbiddingEpaId));

        (await attempt.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Contain("cannot be used as evidence");
    }

    [Fact]
    public async Task AReflectiveExercise_IsDiscussedByTheNamedMentor_AndCreditsNothing()
    {
        var options = await SeededAsync("reflective_exercise_cpsa", "reflective_exercise");

        var draft = await CreateAsync(options, ReflectionData());
        await TransitionAsync(options, draft.Id, "submit", TraineeId);
        var discussed = await TransitionAsync(options, draft.Id, "record_discussion", AssessorId, patch: """
            { "discussion_notes": "Agreed a debrief checklist for future resuscitations." }
            """);

        discussed.CurrentState.Should().Be("discussed");
        discussed.Transitions.Single(transition => transition.TransitionKey == "record_discussion")
            .CreditedItemCount.Should().BeNull("an empty counts_for declares no credit, so none is evaluated (D7)");

        await using var db = new ApplicationDbContext(options);
        (await db.CurriculumItemProgresses.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task AReflectiveExercise_TheMentorCanReturnItForRevision_WithANote_AndTheTraineeCanResubmit()
    {
        var options = await SeededAsync("reflective_exercise_cpsa", "reflective_exercise");
        var draft = await CreateAsync(options, ReflectionData());
        await TransitionAsync(options, draft.Id, "submit", TraineeId);

        var withoutNote = () => TransitionAsync(options, draft.Id, "return", AssessorId);
        await withoutNote.Should().ThrowAsync<InvalidOperationException>();

        (await TransitionAsync(options, draft.Id, "return", AssessorId, note: "Say more about what you would change."))
            .CurrentState.Should().Be("draft");
        (await TransitionAsync(options, draft.Id, "submit", TraineeId))
            .CurrentState.Should().Be("submitted");
    }

    [Fact]
    public async Task AReflectiveExercise_TheMentorCannotRecordTheDiscussion_WithoutWritingIt()
    {
        var options = await SeededAsync("reflective_exercise_cpsa", "reflective_exercise");
        var draft = await CreateAsync(options, ReflectionData());
        await TransitionAsync(options, draft.Id, "submit", TraineeId);

        var attempt = () => TransitionAsync(options, draft.Id, "record_discussion", AssessorId);

        (await attempt.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Contain("Discussion: A value is required.");
    }

    // ---- T154: the clinical audit and the portfolio review, unrated evidence that credits nothing (D6, D7) --------

    [Fact]
    public async Task AClinicalAudit_IsSignedOffByTheNamedSupervisor_IsEvidenceForItsEpa_AndCreditsNothing()
    {
        var options = await SeededAsync("clinical_audit_cpsa", "clinical_audit");

        var draft = await CreateAsync(options, AuditData());
        await TransitionAsync(options, draft.Id, "submit", TraineeId);
        var signedOff = await TransitionAsync(options, draft.Id, "sign_off", AssessorId, patch: """
            { "supervisor_comments": "A complete cycle. The re-audit is booked; present it at the unit meeting." }
            """);

        signedOff.CurrentState.Should().Be("signed_off");
        signedOff.Transitions.Single(transition => transition.TransitionKey == "sign_off")
            .CreditedItemCount.Should().BeNull("an empty counts_for declares no credit, so none is evaluated (D7)");

        await using var db = new ApplicationDbContext(options);
        (await db.CurriculumItemProgresses.CountAsync()).Should().Be(0);

        var stored = await db.Activities.AsNoTracking().SingleAsync(activity => activity.Id == draft.Id);
        stored.EpaId.Should().Be(PermittingEpaId, "evidence_epa_field stamps the EPA the audit is evidence for (T137)");
        stored.ObservedOn.Should().Be(new DateOnly(2026, 3, 10), "the date of the audit is the encounter date (T119)");
        stored.ObservedOnSource.Should().Be(ObservationDateSource.Declared);

        (await TrajectoryAsync(options)).Should().BeEmpty("an unrated instrument is on no entrustment trajectory (D6)");
    }

    /// <summary>
    /// D34: the report is a link, because Wombat stores no files. The link is held to <c>http(s)://</c> so a note like
    /// "on the shared drive" is refused at the first save, and a <c>javascript:</c> URL can never be stored.
    /// </summary>
    [Theory]
    [InlineData("On the department drive")]
    [InlineData("javascript:alert(1)")]
    [InlineData("ftp://files.example.org/audit.pdf")]
    [InlineData("https://")]
    [InlineData("https://intranet.example.org/audit report.pdf")]
    // .NET's `$` matches before a final newline; the seed anchors with `\z`, so a forged trailing newline is refused.
    [InlineData("https://intranet.example.org/audit.pdf\n")]
    public async Task AClinicalAudit_WhoseReportIsNotALink_IsRefused(string reportLink)
    {
        var options = await SeededAsync("clinical_audit_cpsa", "clinical_audit");

        var attempt = () => CreateAsync(options, AuditData(reportLink));

        (await attempt.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Contain("Audit report (link): Value does not match the required format.");
    }

    [Theory]
    [InlineData("https://intranet.example.org/audits/hand-hygiene-2026.pdf")]
    [InlineData("http://sharepoint.hospital.local/sites/paeds/Audit%20Report.docx")]
    [InlineData("HTTPS://INTRANET.EXAMPLE.ORG/AUDITS/HAND-HYGIENE.PDF")]
    public async Task AClinicalAudit_WhoseReportIsALink_IsAccepted(string reportLink)
    {
        var options = await SeededAsync("clinical_audit_cpsa", "clinical_audit");

        var draft = await CreateAsync(options, AuditData(reportLink));

        (await TransitionAsync(options, draft.Id, "submit", TraineeId)).CurrentState.Should().Be("submitted");
    }

    [Fact]
    public async Task AClinicalAudit_CanBeDraftedHalfDone_ButIsSubmittedOnlyWhole()
    {
        // T105: `submit` checks the author's own required fields (`owned`), and the supervisor's comments are not the
        // author's to write, so they are not asked for here.
        var options = await SeededAsync("clinical_audit_cpsa", "clinical_audit");
        var partial = System.Text.Json.Nodes.JsonNode.Parse(AuditData())!.AsObject();
        partial.Remove("findings");
        partial.Remove("report_link");

        var draft = await CreateAsync(options, partial.ToJsonString());
        var submit = () => TransitionAsync(options, draft.Id, "submit", TraineeId);

        var message = (await submit.Should().ThrowAsync<InvalidOperationException>()).Which.Message;
        message.Should().Contain("Findings: A value is required.").And.Contain("Audit report (link): A value is required.");
        message.Should().NotContain("Supervisor comments");
    }

    [Fact]
    public async Task AClinicalAudit_TheSupervisorCannotSignItOff_WithoutComments_OrRewriteTheAudit()
    {
        var options = await SeededAsync("clinical_audit_cpsa", "clinical_audit");
        var draft = await CreateAsync(options, AuditData());
        await TransitionAsync(options, draft.Id, "submit", TraineeId);

        var bare = () => TransitionAsync(options, draft.Id, "sign_off", AssessorId);
        (await bare.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Contain("Supervisor comments: A value is required.");

        // The audit section is the trainee's: a sign-off patch that moves its EPA or its findings is refused.
        var rewrite = () => TransitionAsync(options, draft.Id, "sign_off", AssessorId, patch: $$"""
            { "supervisor_comments": "Fine.", "epa_id": {{ForbiddingEpaId}} }
            """);
        (await rewrite.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Be("EPA: you cannot change this while the activity is Awaiting supervisor.");
    }

    /// <summary>
    /// The record is dated the last day of the period it reviews, from the moment it is filed. It is evidence about that
    /// period, so it falls in that period's committee window (D27), and it does not move when the reviewer signs it off
    /// weeks later. The clinical audit (the day practice was measured) and the reflective exercise (the day of the case)
    /// are dated by what they are about in the same way, not by the day someone signed them.
    /// </summary>
    [Fact]
    public async Task APortfolioReview_IsSignedOffByTheNamedReviewer_DatedTheLastDayOfThePeriodReviewed_AndCreditsNothing()
    {
        var options = await SeededAsync("portfolio_review_cpsa", "portfolio_review");

        var draft = await CreateAsync(options, PortfolioReviewRequest());

        var filed = await StoredAsync(options, draft.Id);
        filed.ObservedOn.Should().Be(PeriodTo, "the last day of the period reviewed is the record's date (T119)");
        filed.ObservedOnSource.Should().Be(ObservationDateSource.Declared);

        await TransitionAsync(options, draft.Id, "submit", TraineeId);
        var signedOff = await TransitionAsync(options, draft.Id, "sign_off", AssessorId, patch: ReviewPatch("2026-07-15"));

        signedOff.CurrentState.Should().Be("signed_off");
        signedOff.Transitions.Single(transition => transition.TransitionKey == "sign_off")
            .CreditedItemCount.Should().BeNull("an empty counts_for declares no credit, so none is evaluated (D7)");

        var stored = await StoredAsync(options, draft.Id);
        stored.ObservedOn.Should().Be(PeriodTo, "the day of the review does not move the record out of the period it reviewed");
        stored.ObservedOnSource.Should().Be(ObservationDateSource.Declared);
        stored.EpaId.Should().Be(PermittingEpaId);

        await using var db = new ApplicationDbContext(options);
        (await db.CurriculumItemProgresses.CountAsync()).Should().Be(0);
        (await TrajectoryAsync(options)).Should().BeEmpty("an unrated instrument is on no entrustment trajectory (D6)");
    }

    [Fact]
    public async Task APortfolioReview_TheReviewerCannotSignItOff_WithoutTheDateWhatWasReviewedAndComments()
    {
        var options = await SeededAsync("portfolio_review_cpsa", "portfolio_review");
        var draft = await CreateAsync(options, PortfolioReviewRequest());
        await TransitionAsync(options, draft.Id, "submit", TraineeId);

        var bare = () => TransitionAsync(options, draft.Id, "sign_off", AssessorId);

        var message = (await bare.Should().ThrowAsync<InvalidOperationException>()).Which.Message;
        message.Should().Contain("Date of the review: A value is required.")
            .And.Contain("Evidence reviewed: A value is required.")
            .And.Contain("Review comments: A value is required.");
        message.Should().NotContain("Agreed actions", "a review may agree no actions");
    }

    [Fact]
    public async Task APortfolioReview_OfAPeriodThatHasNotEnded_IsRefused()
    {
        // The encounter-date bound (T160) judges the record's date, the period's last day: a period still running cannot
        // have been reviewed.
        var options = await SeededAsync("portfolio_review_cpsa", "portfolio_review");
        var request = System.Text.Json.Nodes.JsonNode.Parse(PortfolioReviewRequest())!.AsObject();
        request["period_to"] = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(40)
            .ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

        var attempt = () => CreateAsync(options, request.ToJsonString());

        (await attempt.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Contain("cannot be after today");
    }

    [Fact]
    public async Task APortfolioReview_IsTheReviewersToSign_NotTheTrainees()
    {
        var options = await SeededAsync("portfolio_review_cpsa", "portfolio_review");

        // A review the trainee writes into their own request is dropped at create: the review section is the reviewer's.
        var selfReviewed = System.Text.Json.Nodes.JsonNode.Parse(PortfolioReviewRequest())!.AsObject();
        selfReviewed["review_comments"] = "Excellent throughout.";
        var draft = await CreateAsync(options, selfReviewed.ToJsonString());
        (await StoredAsync(options, draft.Id)).DataJson.Should().NotContain("review_comments");

        // Nor can the trainee write it at submit, or sign the review off themselves.
        var writeAtSubmit = () => TransitionAsync(options, draft.Id, "submit", TraineeId, patch: """
            { "review_comments": "Excellent throughout." }
            """);
        (await writeAtSubmit.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Be("Review comments: you cannot change this while the activity is Draft.");

        await TransitionAsync(options, draft.Id, "submit", TraineeId);
        var selfSignOff = () => TransitionAsync(options, draft.Id, "sign_off", TraineeId, patch: ReviewPatch("2026-07-15"));
        await selfSignOff.Should().ThrowAsync<InvalidOperationException>();
    }

    /// <summary>
    /// The portfolio export's summary (T169) and the committee's readers count an activity as finished by its pinned
    /// workflow's terminal states, not the literal <c>completed</c>. Both finish in <c>signed_off</c> and nowhere else:
    /// a cancelled one is not finished work.
    /// </summary>
    [Theory]
    [InlineData("clinical_audit_cpsa")]
    [InlineData("portfolio_review_cpsa")]
    public void AT154Instrument_IsFinishedWhenSignedOff_AndOnlyThen(string seedKey)
    {
        var workflowJson = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", seedKey, "workflow.json"));

        Wombat.Application.Features.Activities.Services.ActivityCompletion.FinishedStates(workflowJson)
            .Should().BeEquivalentTo(["signed_off"]);
    }

    [Theory]
    [InlineData("clinical_audit_cpsa", "clinical_audit")]
    [InlineData("portfolio_review_cpsa", "portfolio_review")]
    public async Task AT154Instrument_TheReviewerCanReturnIt_WithANote_AndTheTraineeCanResubmit(string seedKey, string toolKey)
    {
        var options = await SeededAsync(seedKey, toolKey);
        var draft = await CreateAsync(options, seedKey == "clinical_audit_cpsa" ? AuditData() : PortfolioReviewRequest());
        await TransitionAsync(options, draft.Id, "submit", TraineeId);

        var withoutNote = () => TransitionAsync(options, draft.Id, "return", AssessorId);
        await withoutNote.Should().ThrowAsync<InvalidOperationException>();

        (await TransitionAsync(options, draft.Id, "return", AssessorId, note: "Add the second half of the period."))
            .CurrentState.Should().Be("draft");
        (await TransitionAsync(options, draft.Id, "submit", TraineeId))
            .CurrentState.Should().Be("submitted");
    }

    // ---- T154: an unrated instrument is held to the College's lists by the EPA it is evidence for ------------------

    public static TheoryData<string, string> UnratedInstruments => new()
    {
        { "reflective_exercise_cpsa", "reflective_exercise" },
        { "clinical_audit_cpsa", "clinical_audit" },
        { "portfolio_review_cpsa", "portfolio_review" },
    };

    /// <summary>
    /// It credits nothing (D7), but it is stamped as evidence for its EPA (T137), and the committee reads that. The list
    /// for an EPA names the instruments that are evidence for it, so the write path refuses the rest exactly as it
    /// refuses a rated tool: at the first save, with the same message, led by the EPA field's label.
    /// </summary>
    [Theory]
    [MemberData(nameof(UnratedInstruments))]
    public async Task AnUnratedInstrument_FiledAgainstAnEpaWhoseListDoesNotNameIt_IsRefused(string seedKey, string toolKey)
    {
        var options = await SeededAsync(seedKey, toolKey);

        var attempt = () => CreateAsync(options, UnratedData(seedKey, ForbiddingEpaId));

        (await attempt.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().StartWith($"EPA: {toolKey} cannot be used as evidence for PAED-010");

        await using var db = new ApplicationDbContext(options);
        (await db.Activities.CountAsync()).Should().Be(0, "a refused create leaves no draft behind");
    }

    [Theory]
    [MemberData(nameof(UnratedInstruments))]
    public async Task AnUnratedInstrument_WhoseEpaIsChangedToOneItsListDoesNotName_IsRefusedAtSubmit(string seedKey, string toolKey)
    {
        var options = await SeededAsync(seedKey, toolKey);
        var draft = await CreateAsync(options, UnratedData(seedKey, PermittingEpaId));

        var attempt = () => TransitionAsync(options, draft.Id, "submit", TraineeId, patch: $$"""{ "epa_id": {{ForbiddingEpaId}} }""");

        (await attempt.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Contain("cannot be used as evidence for PAED-010");
        var stored = await StoredAsync(options, draft.Id);
        stored.CurrentState.Should().Be("draft");
        stored.EpaId.Should().Be(PermittingEpaId);
    }

    /// <summary>
    /// D20's rule for an unchanged target applies unchanged: the list is read again when the author hands the activity on
    /// while still able to correct the EPA, and never on the reviewer's move, since the reviewer cannot.
    /// </summary>
    [Theory]
    [MemberData(nameof(UnratedInstruments))]
    public async Task AnUnratedInstrument_WhoseListDropsItAfterFiling_IsRefusedAtTheAuthorsHandOn_ButNotAtTheReviewersMove(
        string seedKey,
        string toolKey)
    {
        var options = await SeededAsync(seedKey, toolKey);
        var handedOn = await CreateAsync(options, UnratedData(seedKey, PermittingEpaId));
        await TransitionAsync(options, handedOn.Id, "submit", TraineeId);
        var stillDraft = await CreateAsync(options, UnratedData(seedKey, PermittingEpaId));

        await SetPermittingListAsync(options, """["cbd"]""");

        var submit = () => TransitionAsync(options, stillDraft.Id, "submit", TraineeId);
        (await submit.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Contain("cannot be used as evidence for PAED-001");

        var (finish, patch) = FinishingMove(seedKey);
        (await TransitionAsync(options, handedOn.Id, finish, AssessorId, patch: patch))
            .CurrentState.Should().NotBe("submitted", "the reviewer cannot correct the EPA, so the list is not read again on their move");
    }

    /// <summary>D21: a type that is no College instrument is held to no list, whatever its form looks like.</summary>
    [Theory]
    [MemberData(nameof(UnratedInstruments))]
    public async Task AnUnratedTypeWithNoInstrument_FiledAgainstAnyEpa_IsAccepted(string seedKey, string toolKey)
    {
        var options = await SeededAsync(seedKey, toolKey, typeIsAnInstrument: false);

        var draft = await CreateAsync(options, UnratedData(seedKey, ForbiddingEpaId));

        (await TransitionAsync(options, draft.Id, "submit", TraineeId)).CurrentState.Should().Be("submitted");
        (await StoredAsync(options, draft.Id)).EpaId.Should().Be(ForbiddingEpaId);
    }

    /// <summary>
    /// T105: `cancel` checks formats only, so a withdrawal never demands the fields the author is withdrawing instead of
    /// finishing. The handoff's trap: a seed that forgets to declare it gets `all`, which refuses both of these.
    /// </summary>
    [Theory]
    [MemberData(nameof(UnratedInstruments))]
    public async Task AnUnratedInstrument_CanBeCancelled_HalfFilled_AndOnceSubmitted(string seedKey, string toolKey)
    {
        var options = await SeededAsync(seedKey, toolKey);

        var halfFilled = await CreateAsync(options, $$"""{ "epa_id": {{PermittingEpaId}} }""");
        (await TransitionAsync(options, halfFilled.Id, "cancel", TraineeId)).CurrentState.Should().Be("cancelled");

        var submitted = await CreateAsync(options, UnratedData(seedKey, PermittingEpaId));
        await TransitionAsync(options, submitted.Id, "submit", TraineeId);
        (await TransitionAsync(options, submitted.Id, "cancel", TraineeId)).CurrentState.Should().Be("cancelled");
    }

    /// <summary>
    /// The type picker on <c>/activities/new</c> offers both instruments to a trainee on the v11.1 curriculum, under
    /// names no other type shares ([T120] checklist step 4). Neither commits to a ladder, so the ladder narrowing
    /// (T123 d3) leaves them on the menu beside the rated tools it keeps.
    /// </summary>
    [Fact]
    public async Task TheTypePicker_OffersBothT154Instruments_ToAV11Trainee_UnderNamesNoOtherTypeShares()
    {
        const int paediatricsSpecialityId = 7;
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using (var db = new ApplicationDbContext(options))
        {
            db.EntrustmentScales.Add(new EntrustmentScale { Id = CpsaScaleId, Name = CpsaScaleName, SeedKey = CpsaScaleSeedKey });
            var rungs = new[] { "1", "2", "3a", "3b", "4", "5" };
            for (var order = 1; order <= rungs.Length; order++)
            {
                db.Set<EntrustmentLevel>().Add(new EntrustmentLevel { Id = 900 + order, ScaleId = CpsaScaleId, Order = order, Label = rungs[order - 1] });
            }

            db.Epas.Add(new Epa { Id = PermittingEpaId, Code = "PAED-001", Title = "An EPA on the ladder" });
            db.CurriculumItems.Add(new CurriculumItem
            {
                Id = PermittingItemId,
                CurriculumId = CurriculumId,
                EpaId = PermittingEpaId,
                RequiredCount = 3,
                MinimumLevelOrder = RungThreeA,
                WindowMonths = 12,
                ScaleId = CpsaScaleId
            });
            db.Set<TraineeProfile>().Add(new TraineeProfile
            {
                Id = 1,
                UserId = TraineeId,
                InstitutionId = InstitutionId,
                CurriculumId = CurriculumId,
                ProgrammeStartDate = ProgrammeStart,
                ExpectedCompletionDate = ProgrammeStart.AddYears(4),
                IsActive = true
            });

            var id = 1;
            foreach (var entry in ActivityTypeSeedCatalogue.For(ActivityTypeSeedSource.PaediatricCollege))
            {
                db.ActivityTypes.Add(TypeFromSeed(id++, entry.Key, entry.Name, entry.WbaToolKey, paediatricsSpecialityId));
            }

            // The scope filter is live: a type of another speciality is not offered.
            db.ActivityTypes.Add(TypeFromSeed(
                id, "other_speciality_audit", "Another discipline's audit", "clinical_audit", paediatricsSpecialityId + 1,
                seedFolder: "clinical_audit_cpsa"));

            await db.SaveChangesAsync();
        }

        var trainee = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, TraineeId),
            new Claim(ClaimTypes.Role, WombatRoles.Trainee),
            new Claim(WombatClaimTypes.InstitutionId, InstitutionId.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new Claim(WombatClaimTypes.SpecialityId, paediatricsSpecialityId.ToString(System.Globalization.CultureInfo.InvariantCulture))
        ], "test"));

        await using var read = new ApplicationDbContext(options);
        var offered = await new ListActivityTypesQueryHandler(read)
            .Handle(new ListActivityTypesQuery(trainee, TraineeId), CancellationToken.None);

        offered.Select(item => item.Key).Should().Contain(["clinical_audit_cpsa", "portfolio_review_cpsa", "mini_cex_cpsa"])
            .And.NotContain("other_speciality_audit");
        offered.Single(item => item.Key == "clinical_audit_cpsa").Name.Should().Be("Clinical Audit (Paediatrics)");
        offered.Single(item => item.Key == "portfolio_review_cpsa").Name.Should().Be("Portfolio and Logbook Review (Paediatrics)");
        offered.Select(item => item.Name).Should().OnlyHaveUniqueItems("a trainee who picks the wrong one of two same-named types gets no credit");
    }

    // ---- helpers -------------------------------------------------------------------------------------------------

    private static string WithRequest(string requestFieldsJson, int epaId)
    {
        var fields = System.Text.Json.Nodes.JsonNode.Parse(requestFieldsJson)!.AsObject();
        fields["epa_id"] = epaId;
        fields["assessor_user_id"] = AssessorId;
        fields["observed_on"] = "2026-03-10";
        return fields.ToJsonString();
    }

    private static readonly DateOnly PeriodTo = new(2026, 6, 30);

    /// <summary>A whole filing of one of the three unrated instruments, against <paramref name="epaId" />.</summary>
    private static string UnratedData(string seedKey, int epaId)
    {
        var data = System.Text.Json.Nodes.JsonNode.Parse(seedKey switch
        {
            "reflective_exercise_cpsa" => ReflectionData(),
            "clinical_audit_cpsa" => AuditData(),
            "portfolio_review_cpsa" => PortfolioReviewRequest(),
            _ => throw new ArgumentOutOfRangeException(nameof(seedKey), seedKey, null)
        })!.AsObject();
        data["epa_id"] = epaId;
        return data.ToJsonString();
    }

    /// <summary>The reviewer's finishing move for each unrated instrument, with what it requires.</summary>
    private static (string TransitionKey, string Patch) FinishingMove(string seedKey) => seedKey switch
    {
        "reflective_exercise_cpsa" => ("record_discussion", """{ "discussion_notes": "Agreed a debrief checklist." }"""),
        "clinical_audit_cpsa" => ("sign_off", """{ "supervisor_comments": "A complete cycle." }"""),
        "portfolio_review_cpsa" => ("sign_off", ReviewPatch("2026-07-15")),
        _ => throw new ArgumentOutOfRangeException(nameof(seedKey), seedKey, null)
    };

    private static async Task SetPermittingListAsync(DbContextOptions<ApplicationDbContext> options, string permittedToolsJson)
    {
        await using var db = new ApplicationDbContext(options);
        var item = await db.CurriculumItems.SingleAsync(entity => entity.Id == PermittingItemId);
        item.PermittedToolsJson = permittedToolsJson;
        await db.SaveChangesAsync();
    }

    private static string ReflectionData() => $$"""
        {
          "epa_id": {{PermittingEpaId}},
          "assessor_user_id": "{{AssessorId}}",
          "observed_on": "2026-03-10",
          "prompt": "critical_incident",
          "what_happened": "A delayed escalation during a resuscitation.",
          "analysis": "Roles were clear; the escalation call came late.",
          "learning": "Name the escalation trigger at the start.",
          "action_plan": "Use the debrief checklist."
        }
        """;

    private static string AuditData(string reportLink = "https://intranet.example.org/audits/hand-hygiene-2026.pdf")
    {
        var data = System.Text.Json.Nodes.JsonNode.Parse($$"""
            {
              "epa_id": {{PermittingEpaId}},
              "assessor_user_id": "{{AssessorId}}",
              "observed_on": "2026-03-10",
              "audit_title": "Hand hygiene before resuscitation",
              "standard": "The WHO five moments, as adopted in the unit's infection control policy.",
              "sample": "Forty consecutive resuscitation-bay episodes in February 2026.",
              "findings": "31 of 40 compliant, against a standard of 95%.",
              "change_made": "Alcohol rub at the bay entrance, and a prompt on the resuscitation checklist.",
              "reaudit_on": "2026-06-10"
            }
            """)!.AsObject();
        data["report_link"] = reportLink;
        return data.ToJsonString();
    }

    private static string PortfolioReviewRequest() => $$"""
        {
          "epa_id": {{PermittingEpaId}},
          "assessor_user_id": "{{AssessorId}}",
          "period_from": "2026-01-12",
          "period_to": "2026-06-30",
          "export_file_name": "portfolio-3f9a2c61b0de.pdf",
          "notes_for_reviewer": "Two of the teaching sessions were delivered at the district hospital."
        }
        """;

    private static string ReviewPatch(string reviewedOn) => $$"""
        {
          "reviewed_on": "{{reviewedOn}}",
          "evidence_reviewed": ["teaching_sessions", "journal_club_presentations", "reflective_entries"],
          "review_comments": "A full half-year of teaching. Feedback from learners is thin; collect it next semester."
        }
        """;

    private static async Task<Activity> StoredAsync(DbContextOptions<ApplicationDbContext> options, int activityId)
    {
        await using var db = new ApplicationDbContext(options);
        return await db.Activities.AsNoTracking().SingleAsync(activity => activity.Id == activityId);
    }

    private static async Task<IReadOnlyList<EpaTrajectoryDto>> TrajectoryAsync(DbContextOptions<ApplicationDbContext> options)
    {
        await using var db = new ApplicationDbContext(options);
        return await new GetEpaTrajectoryForTraineeQueryHandler(db).Handle(
            new GetEpaTrajectoryForTraineeQuery(TraineeId, Principal(TraineeId)), CancellationToken.None);
    }

    /// <summary>A published type built from a seed folder on disk, as the seeders build it, at speciality scope.</summary>
    private static ActivityType TypeFromSeed(
        int id,
        string typeKey,
        string name,
        string? toolKey,
        int specialityId,
        string? seedFolder = null)
    {
        string Read(string file) => File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", seedFolder ?? typeKey, file));

        return new ActivityType
        {
            Id = id,
            Key = typeKey,
            Name = name,
            Scope = ActivityScope.Speciality,
            ScopeId = specialityId,
            Version = 1,
            IsActive = true,
            WbaToolKey = toolKey,
            SchemaJson = Read("schema.json"),
            WorkflowJson = Read("workflow.json"),
            CreditRulesJson = Read("credit.json"),
            DisplayFieldsJson = "[]",
            OwnerUserId = ActivityTypeSeedCatalogue.SeedActorUserId,
            CreatedOn = DateTime.UtcNow
        };
    }

    private static async Task<ActivityDto> CreateAsync(DbContextOptions<ApplicationDbContext> options, string dataJson)
    {
        await using var db = new ApplicationDbContext(options);
        return await Service(db).CreateDraftAsync(new CreateActivityInput(1, TraineeId, TraineeId, dataJson, Principal(TraineeId)));
    }

    private static async Task<ActivityDto> TransitionAsync(
        DbContextOptions<ApplicationDbContext> options,
        int activityId,
        string transitionKey,
        string actorUserId,
        string? patch = null,
        string? note = null)
    {
        await using var db = new ApplicationDbContext(options);
        return await Service(db).TransitionAsync(new TransitionActivityInput(
            activityId, transitionKey, actorUserId, Principal(actorUserId), patch, note));
    }

    /// <param name="typeIsAnInstrument">False stores the type with no <c>WbaToolKey</c> (D21); the lists are unchanged.</param>
    private static async Task<DbContextOptions<ApplicationDbContext>> SeededAsync(
        string seedKey,
        string toolKey,
        bool typeIsAnInstrument = true)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);

        NomineeSeed.AddUser(db, TraineeId, InstitutionId, WombatRoles.Trainee);
        NomineeSeed.AddUser(db, AssessorId, InstitutionId, WombatRoles.Assessor);

        db.EntrustmentScales.Add(new EntrustmentScale { Id = CpsaScaleId, Name = CpsaScaleName, SeedKey = CpsaScaleSeedKey });
        db.Epas.AddRange(
            new Epa { Id = PermittingEpaId, Code = "PAED-001", Title = "Permits this tool" },
            new Epa { Id = ForbiddingEpaId, Code = "PAED-010", Title = "Does not permit this tool" });
        db.CurriculumItems.AddRange(
            new CurriculumItem
            {
                Id = PermittingItemId,
                CurriculumId = CurriculumId,
                EpaId = PermittingEpaId,
                RequiredCount = 3,
                MinimumLevelOrder = RungThreeA,
                WindowMonths = 12,
                ScaleId = CpsaScaleId,
                PermittedToolsJson = $$"""["cbd","{{toolKey}}"]"""
            },
            new CurriculumItem
            {
                Id = ForbiddingItemId,
                CurriculumId = CurriculumId,
                EpaId = ForbiddingEpaId,
                RequiredCount = 3,
                MinimumLevelOrder = RungThreeA,
                WindowMonths = 12,
                ScaleId = CpsaScaleId,
                PermittedToolsJson = """["direct_observation","msf"]"""
            });
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = TraineeId,
            InstitutionId = InstitutionId,
            CurriculumId = CurriculumId,
            ProgrammeStartDate = ProgrammeStart,
            ExpectedCompletionDate = ProgrammeStart.AddYears(4),
            IsActive = true
        });

        string Read(string file) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", seedKey, file));
        var schemaJson = Read("schema.json");
        var workflowJson = Read("workflow.json");
        var creditJson = Read("credit.json");

        var type = new ActivityType
        {
            Id = 1,
            Key = seedKey,
            Name = seedKey,
            Scope = ActivityScope.Global,
            Version = 1,
            WbaToolKey = typeIsAnInstrument ? toolKey : null,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditJson,
            DisplayFieldsJson = "[]",
            OwnerUserId = "system",
            CreatedOn = DateTime.UtcNow
        };
        type.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = 1,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditJson,
            DisplayFieldsJson = "[]",
            PublishedByUserId = "system",
            PublishedOn = DateTime.UtcNow
        });
        db.ActivityTypes.Add(type);

        await db.SaveChangesAsync();
        return options;
    }

    private static ActivityService Service(ApplicationDbContext db)
        => new(db, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator());

    private static ClaimsPrincipal Principal(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));
}
