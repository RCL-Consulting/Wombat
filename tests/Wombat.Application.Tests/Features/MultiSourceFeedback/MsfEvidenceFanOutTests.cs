using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// Releasing a multi-source feedback campaign leaves per-EPA evidence on the trainee's record. (T121)
/// </summary>
/// <remarks>
/// <para>
/// Before this, running a campaign end to end — create, invite, open, collect, close, release — moved
/// nothing. MSF is the only instrument Annexure A names for every one of the fifteen EPAs, it was the
/// only one of the thirteen that already existed as a complete working aggregate, and it carried no EPA
/// reference at all, so there was nothing to connect it to a curriculum with.
/// </para>
/// <para>
/// The activity type is loaded from the REAL seed folder rather than a fixture. A fixture would prove
/// that some schema works; what has to be true is that the schema the product ships works, including
/// its actor rules and its empty <c>counts_for</c>.
/// </para>
/// </remarks>
public sealed class MsfEvidenceFanOutTests
{
    private const int CurriculumId = 3000;
    private const int InstitutionId = 10;
    private const int SpecialityId = 1;
    private const int EpaOnCurriculum = 5001;
    private const int SecondEpaOnCurriculum = 5002;
    private const int EpaOffCurriculum = 6000;

    private const int MsfTypeId = 100;

    private const string TraineeUserId = "trainee-1";
    private const string CoordinatorUserId = "coord-1";

    [Fact]
    public async Task Release_RecordsOneTerminalActivityPerCoveredEpa()
    {
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum, SecondEpaOnCurriculum]);

        (await db.ActivityTypes.SingleAsync()).SystemManaged.Should().BeTrue(
            "guard: the release writes a type nobody may create by hand (T162), as the seeder stamps it");

        await ReleaseAsync(db, campaign.Id, entrustmentLevel: 4, narrative: "Consistently reliable under pressure.");

        var activities = await db.Activities.Include(activity => activity.Transitions).ToListAsync();
        activities.Should().HaveCount(2);
        activities.Should().OnlyContain(activity => activity.CurrentState == "recorded");
        activities.Select(activity => ReadInt(activity.DataJson, "epa_id"))
            .Should().BeEquivalentTo([EpaOnCurriculum, SecondEpaOnCurriculum]);

        var first = activities[0];
        ReadInt(first.DataJson, "campaign_id").Should().Be(campaign.Id);
        ReadInt(first.DataJson, "respondent_count").Should().Be(4);
        ReadInt(first.DataJson, "overall_level").Should().Be(4);
        ReadString(first.DataJson, "summary").Should().Be("Consistently reliable under pressure.");

        // T119: the encounter date is when the window ACTUALLY shut, not when the paperwork was filed
        // and not the date it was scheduled to shut. This is what [T130]'s period resolver buckets on.
        first.ObservedOn.Should().Be(DateOnly.FromDateTime(campaign.ClosedOn!.Value));

        // T101: stamped from the SUBJECT's profile, so oversight follows the trainee rather than the
        // coordinator who happened to release it.
        first.InstitutionId.Should().Be(InstitutionId);
        first.SubjectUserId.Should().Be(TraineeUserId);
        first.CreatedByUserId.Should().Be(CoordinatorUserId);

        var released = await db.MsfCampaigns.Include(c => c.CoveredEpas).SingleAsync();
        released.EvidenceRecordedOn.Should().NotBeNull();
        released.CoveredEpas.Should().OnlyContain(covered => covered.RecordedOn != null);
    }

    /// <summary>
    /// T137. The rows of one campaign share a type, a state and an encounter date, so the stamped EPA is the only thing
    /// a list can tell them apart by. <c>msf_cpsa</c> credits nothing (D8), so only its schema's pointer can supply it.
    /// </summary>
    [Fact]
    public async Task Release_StampsEachRowWithTheEpaItIsEvidenceFor()
    {
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum, SecondEpaOnCurriculum]);

        await ReleaseAsync(db, campaign.Id, entrustmentLevel: 4, narrative: null);

        var activities = await db.Activities.AsNoTracking().ToListAsync();
        activities.Select(activity => activity.EpaId)
            .Should().BeEquivalentTo(new int?[] { EpaOnCurriculum, SecondEpaOnCurriculum });
        activities.Should().OnlyContain(activity => activity.EpaId == ReadInt(activity.DataJson, "epa_id"),
            "each row is stamped with the EPA its own payload names, not a sibling's");
    }

    [Fact]
    public async Task Release_CreditsNothing_BecauseMsfConsumesNoneOfTheFiftyFiveEncounters()
    {
        // College decision D8. `counts_for` is permanent per pinned version — Activity.SchemaVersion is
        // assigned once and there is no re-pin path — so "ship [] now and switch later" was never
        // available, and this is the assertion that says which way it shipped.
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum]);

        await ReleaseAsync(db, campaign.Id, entrustmentLevel: 4, narrative: null);

        db.CurriculumItemProgresses.Should().BeEmpty();

        var record = (await db.Activities.Include(activity => activity.Transitions).SingleAsync())
            .Transitions.Single(transition => transition.TransitionKey == "record");

        // T108's three-valued contract: null is "credit was never evaluated", which is the honest
        // answer for a type that declares none. Zero would mean "evaluated and matched nothing".
        record.CreditedItemCount.Should().BeNull();
    }

    [Fact]
    public async Task Release_DatesTheEvidenceFromTheDayTheWindowActuallyShut()
    {
        // A coordinator can close a campaign before its scheduled ClosesOn. Dating the evidence from
        // the schedule would give it a FUTURE encounter date, which would put it outside the committee
        // review window it belongs to and into the wrong period for [T130]'s quota.
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum]);
        campaign.ClosesOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30);
        await db.SaveChangesAsync();

        await ReleaseAsync(db, campaign.Id, entrustmentLevel: null, narrative: null);

        var activity = await db.Activities.SingleAsync();
        activity.ObservedOn.Should().Be(DateOnly.FromDateTime(campaign.ClosedOn!.Value));
        activity.ObservedOn.Should().BeBefore(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1));
    }

    // ---- T160: the system-written path gets the future check only, and records no lateness ------------------------

    /// <summary>
    /// A release is never a filing: nobody typed the date, so nothing is late. Forty days after the window shut is well
    /// past D15's fourteen, and the release still records its evidence without a lateness figure.
    /// </summary>
    [Fact]
    public async Task Release_LongAfterTheWindowShut_IsNotRefused_AndRecordsNoLateness()
    {
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum, SecondEpaOnCurriculum]);
        campaign.ClosedOn = DateTime.UtcNow.AddDays(-40);
        await db.SaveChangesAsync();

        await ReleaseAsync(db, campaign.Id, entrustmentLevel: 4, narrative: null);

        var activities = await db.Activities.Include(activity => activity.Transitions).ToListAsync();
        activities.Should().HaveCount(2);
        activities.Should().OnlyContain(activity => activity.ObservedOn == DateOnly.FromDateTime(campaign.ClosedOn!.Value));
        activities.SelectMany(activity => activity.Transitions)
            .Should().OnlyContain(transition => transition.DaysAfterEncounter == null);
    }

    /// <summary>
    /// The programme-start bound is not applied to a system-written record: its date is the day the window shut, and
    /// nobody on the release can correct it.
    /// </summary>
    [Fact]
    public async Task Release_OfEvidenceDatedBeforeTheProgrammeStarted_IsNotRefused()
    {
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum]);
        var profile = await db.Set<TraineeProfile>().SingleAsync();
        profile.ProgrammeStartDate = DateOnly.FromDateTime(DateTime.UtcNow);
        await db.SaveChangesAsync();

        await ReleaseAsync(db, campaign.Id, entrustmentLevel: 4, narrative: null);

        (await db.Activities.SingleAsync()).ObservedOn.Should().BeBefore(profile.ProgrammeStartDate);
    }

    [Fact]
    public async Task TheSystemWrittenPath_RefusesAnEncounterDatedTomorrow_AndStagesNothing()
    {
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum]);

        // 22:30 UTC on the 24th is 00:30 on the 25th in South Africa, so "tomorrow" is the 26th.
        var stage = () => BuildActivityService(db, new FixedClock(LateEveningUtc)).StageCompletedAsync(
            new RecordCompletedActivitiesInput(
                "msf_cpsa", TraineeUserId, CoordinatorUserId, "record", [MsfPayload(campaign.Id, new DateOnly(2026, 9, 26))], Coordinator()));

        (await stage.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Be("Feedback window closed: The encounter date cannot be after today (2026-09-25).");
        db.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Should().BeEmpty("the caller's audit save would commit anything staged");
    }

    [Fact]
    public async Task TheSystemWrittenPath_JudgesTheFutureOnTheSouthAfricanCalendar()
    {
        // On the UTC calendar it is still the 24th, and a row dated the 25th would be refused as tomorrow.
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum]);

        var staged = await BuildActivityService(db, new FixedClock(LateEveningUtc)).StageCompletedAsync(
            new RecordCompletedActivitiesInput(
                "msf_cpsa", TraineeUserId, CoordinatorUserId, "record", [MsfPayload(campaign.Id, new DateOnly(2026, 9, 25))], Coordinator()));

        staged.Should().Be(1);
    }

    /// <summary>22:30 UTC on 24 September: already 00:30 on 25 September in South Africa.</summary>
    private static readonly DateTimeOffset LateEveningUtc = new(2026, 9, 24, 22, 30, 0, TimeSpan.Zero);

    private static string MsfPayload(int campaignId, DateOnly observedOn)
        => JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["epa_id"] = EpaOnCurriculum,
            ["campaign_id"] = campaignId,
            ["observed_on"] = observedOn.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            ["respondent_count"] = 4
        });

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    [Fact]
    public async Task Release_DropsAnEpaThatHasLeftTheSubjectsCurriculum_WithoutFailingTheRelease()
    {
        // A trainee can be moved between curricula while the response window is open. The feedback is
        // still real and must still reach them; only the evidence link for the departed EPA is lost.
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum, EpaOffCurriculum]);

        await ReleaseAsync(db, campaign.Id, entrustmentLevel: null, narrative: null);

        var activities = await db.Activities.ToListAsync();
        activities.Should().ContainSingle();
        ReadInt(activities[0].DataJson, "epa_id").Should().Be(EpaOnCurriculum);

        var released = await db.MsfCampaigns.Include(c => c.CoveredEpas).SingleAsync();
        released.State.Should().Be(MsfCampaignState.Released);

        // The dropped EPA stays declared and stays unrecorded, so nothing downstream can claim an
        // activity that was never written.
        released.CoveredEpas.Single(covered => covered.EpaId == EpaOnCurriculum).RecordedOn.Should().NotBeNull();
        released.CoveredEpas.Single(covered => covered.EpaId == EpaOffCurriculum).RecordedOn.Should().BeNull();

        // T186: the writer and the one reader of coverage, end to end. The rows this release wrote, read back by the
        // reader every screen shares, name exactly the EPA it recorded. A change to how the release links a row to its
        // campaign that a reader's fixture was updated to match would still fail here.
        var recorded = await MsfCampaignCoverage.RecordedEpasAsync(
            db,
            TraineeUserId,
            [(released.Id, released.State)],
            MsfCampaignCoverage.MsfEvidenceTypeKey,
            CancellationToken.None);
        recorded[released.Id].Select(epa => epa.EpaId).Should().Equal(EpaOnCurriculum);

        // And the report the coordinator and the trainee open says so per EPA.
        (await ReportAsync(db, released.Id)).CoveredEpas
            .Select(covered => (covered.EpaId, covered.Recorded))
            .Should().BeEquivalentTo([(EpaOnCurriculum, true), (EpaOffCurriculum, false)]);
    }

    /// <summary>
    /// The campaign report reads "recorded" from the evidence rows, as the committee snapshot and the coverage grid do,
    /// not from the per-EPA stamp. A campaign released before the stamp existed has its rows and a null stamp; its
    /// report said every EPA was "no longer on the trainee's curriculum". (T186)
    /// </summary>
    [Fact]
    public async Task TheReport_ReadsRecordedFromTheEvidenceRows_NotFromThePerEpaStamp()
    {
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum, EpaOffCurriculum]);
        await ReleaseAsync(db, campaign.Id, entrustmentLevel: null, narrative: null);

        // The dev case, and its mirror: the recorded EPA's stamp cleared, as on a campaign released before the stamp
        // existed; the dropped EPA stamped, with no row behind it.
        var released = await db.MsfCampaigns.Include(c => c.CoveredEpas).SingleAsync();
        released.CoveredEpas.Single(covered => covered.EpaId == EpaOnCurriculum).RecordedOn = null;
        released.CoveredEpas.Single(covered => covered.EpaId == EpaOffCurriculum).RecordedOn = DateTime.UtcNow;
        await db.SaveChangesAsync();

        (await ReportAsync(db, campaign.Id)).CoveredEpas
            .Select(covered => (covered.EpaId, covered.Recorded))
            .Should().BeEquivalentTo([(EpaOnCurriculum, true), (EpaOffCurriculum, false)]);
    }

    [Fact]
    public async Task Release_OmitsTheLevelEntirely_WhenTheReviewerStatesNone()
    {
        // D10 names the releasing reviewer as the author of the level, and blank is a legitimate answer:
        // MSF then records evidence and asserts nothing. An absent key is not the same as a zero.
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum]);

        await ReleaseAsync(db, campaign.Id, entrustmentLevel: null, narrative: null);

        var dataJson = (await db.Activities.SingleAsync()).DataJson;
        using var document = JsonDocument.Parse(dataJson);
        document.RootElement.TryGetProperty("overall_level", out _).Should().BeFalse();
        document.RootElement.TryGetProperty("summary", out _).Should().BeFalse();
    }

    [Fact]
    public async Task Release_IsRefused_WhenOnlyOneRespondentCategoryReports()
    {
        // College decision D11. Eight peer doctors clear MinimumResponses and show one category with the
        // rest suppressed, which is not multi-source feedback.
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum], respondingCategories: 1);

        var release = () => ReleaseAsync(db, campaign.Id, entrustmentLevel: null, narrative: null);

        await release.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*respondent categories*");
        db.Activities.Should().BeEmpty();
    }

    [Fact]
    public async Task Release_DoesNotRepeatTheFanOut_WhenEvidenceIsAlreadyRecorded()
    {
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum]);
        campaign.EvidenceRecordedOn = DateTime.UtcNow.AddDays(-1);
        await db.SaveChangesAsync();

        await ReleaseAsync(db, campaign.Id, entrustmentLevel: null, narrative: null);

        db.Activities.Should().BeEmpty();
    }

    /// <summary>
    /// T162. <c>msf_cpsa</c> is system-managed, so nobody files one by hand, whoever they are: the picker does not offer
    /// it, and a create that names its id anyway is refused before anything is written.
    /// </summary>
    /// <remarks>
    /// The coordinator matters as much as the trainee. A trainee's stray draft was merely stuck (nobody could write its
    /// evidence), but a coordinator owns both sections and the <c>record</c> transition, so a hand-made draft of theirs
    /// could have become MSF evidence that no campaign ever collected.
    /// </remarks>
    [Theory]
    [InlineData(TraineeUserId)]
    [InlineData(CoordinatorUserId)]
    public async Task NobodyCanCreateAnMsfRecordByHand_AndTheRefusalWritesNothing(string creatorUserId)
    {
        await using var db = CreateDb();
        Seed(db, [EpaOnCurriculum]);

        var service = BuildActivityService(db);
        var principal = creatorUserId == CoordinatorUserId ? Coordinator() : Principal(TraineeUserId);

        var create = () => service.CreateDraftAsync(
            new CreateActivityInput(
                MsfTypeId,
                TraineeUserId,
                creatorUserId,
                $$"""{ "epa_id": {{EpaOnCurriculum}}, "campaign_id": 1, "observed_on": "2026-06-01", "respondent_count": 8, "overall_level": 6 }""",
                principal),
            CancellationToken.None);

        await create.Should().ThrowAsync<InvalidOperationException>().WithMessage("*written by the system*");

        // The audit trap: AuditPipelineBehavior's catch saves this request's context, so the refusal must leave nothing
        // for that save to commit. Then read back past the tracker.
        db.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Should().BeEmpty();
        (await db.SaveChangesAsync()).Should().Be(0);
        db.ChangeTracker.Clear();

        (await db.Activities.CountAsync()).Should().Be(0);
        (await db.ActivityTransitions.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task EvenWithoutTheFlag_ATraineesStrayDraftCouldNeverBecomeEvidence()
    {
        // The second line behind SystemManaged, and what stood alone before T162: the type's own actor rules. With the
        // flag cleared (a hand-edited database, or a draft filed before T162 that the migration did not reach), a trainee
        // can create a draft, but owns none of its fields and cannot record it.
        await using var db = CreateDb();
        Seed(db, [EpaOnCurriculum]);
        (await db.ActivityTypes.SingleAsync()).SystemManaged = false;
        await db.SaveChangesAsync();

        var service = BuildActivityService(db);
        var trainee = Principal(TraineeUserId);

        var draft = await service.CreateDraftAsync(
            new CreateActivityInput(
                MsfTypeId,
                TraineeUserId,
                TraineeUserId,
                $$"""{ "epa_id": {{EpaOnCurriculum}}, "campaign_id": 1, "observed_on": "2026-06-01", "respondent_count": 8, "overall_level": 6 }""",
                trainee),
            CancellationToken.None);

        // Every field they submitted was dropped: both sections declare
        // `editable_by: role:Coordinator|role:Administrator`, so a trainee owns none of them.
        var stored = await db.Activities.SingleAsync(activity => activity.Id == draft.Id);
        stored.DataJson.Should().Be("{}");

        var record = () => service.TransitionAsync(
            new TransitionActivityInput(draft.Id, "record", TraineeUserId, trainee, null, null),
            CancellationToken.None);

        await record.Should().ThrowAsync<InvalidOperationException>();
        (await db.Activities.SingleAsync(activity => activity.Id == draft.Id)).CurrentState.Should().Be("draft");
    }

    [Fact]
    public async Task TheEvidencePayloadCarriesNothingThatIdentifiesARespondent()
    {
        // AccessReportBuilder puts an activity's whole DataJson into the subject's data-subject access
        // report, and the portfolio PDF prints it. MSF only works because it is confidential and
        // aggregated, so the payload is built from campaign fields and never from the aggregate report.
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum]);

        await ReleaseAsync(db, campaign.Id, entrustmentLevel: 4, narrative: "Released after review.");

        var dataJson = (await db.Activities.SingleAsync()).DataJson;
        dataJson.Should().NotContain("@");
        dataJson.Should().NotContain("Unfailingly patient");
        dataJson.Should().NotContain("Consultant");
    }

    [Fact]
    public async Task Release_RefusesToWriteAPaediatricRecordAboutATraineeOfAnotherSpeciality()
    {
        // `msf_cpsa` is Speciality-scoped, and the MSF trainee picker is scoped by INSTITUTION, so an
        // institution running Paediatrics alongside another discipline can reach a non-paediatric
        // registrar from this page. Without this guard the release would stamp a CPSA record, carrying
        // an ordinal pinned to the CPSA ladder, onto a trainee whose curriculum measures on another one
        // — and D8's empty `counts_for` means CreditApplier's ScaleMismatch refusal never runs.
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum]);

        var subSpeciality = await db.Set<SubSpeciality>().SingleAsync();
        subSpeciality.SpecialityId = 99;
        await db.SaveChangesAsync();

        var release = () => ReleaseAsync(db, campaign.Id, entrustmentLevel: null, narrative: null);

        await release.Should().ThrowAsync<InvalidOperationException>().WithMessage("*does not train there*");
        db.Activities.Should().BeEmpty();
        (await db.MsfCampaigns.SingleAsync()).State.Should().Be(MsfCampaignState.UnderReview);
    }

    [Fact]
    public async Task Release_RefusesACampaignAboutAnotherInstitutionsTrainee()
    {
        // Release is where the consequence is, so release has to check for itself rather than trust that the
        // caller reached it through a list and a report that are scoped (since T113) - the command is reachable
        // by campaign id alone.
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum]);

        var outsider = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "coord-elsewhere"),
                new Claim(ClaimTypes.Role, WombatRoles.Coordinator),
                new Claim(WombatClaimTypes.InstitutionId, "999")
            ],
            "test",
            ClaimTypes.Name,
            ClaimTypes.Role));

        var handler = new ReleaseMsfCampaignCommandHandler(
            db,
            new MsfAggregationService(),
            BuildActivityService(db),
            new ActivityReferenceDataService(db),
            NullLogger<ReleaseMsfCampaignCommandHandler>.Instance);

        var release = () => handler.Handle(
            new ReleaseMsfCampaignCommand(campaign.Id, "coord-elsewhere", null, null, outsider),
            CancellationToken.None);

        await release.Should().ThrowAsync<UnauthorizedAccessException>();
        db.Activities.Should().BeEmpty();
        (await db.MsfCampaigns.SingleAsync()).State.Should().Be(MsfCampaignState.UnderReview);
    }

    /// <summary>
    /// A release that throws leaves the campaign releasable. (T121)
    /// </summary>
    /// <remarks>
    /// <c>AuditWriter</c> shares the request's scoped <c>IApplicationDbContext</c> and saves, and
    /// <c>AuditPipelineBehavior</c> writes an audit row from its <c>catch</c> — so any mutation pending
    /// when an exception escapes is committed on the way out. Since <c>MsfCampaign.Release</c> refuses
    /// anything but <c>UnderReview</c>, a release committed by that path could never be retried. The
    /// handler therefore does every throwable thing before touching the aggregate, and this asserts the
    /// property rather than the ordering.
    /// </remarks>
    [Fact]
    public async Task AFailedReleaseLeavesTheCampaignUnreleasedAndRetryable()
    {
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum]);

        // An ordinal off the end of the CPSA ladder: the command validator has no upper bound, so this
        // reaches SchemaValidator's max and throws from inside the staging call.
        var release = () => ReleaseAsync(db, campaign.Id, entrustmentLevel: 99, narrative: null);
        await release.Should().ThrowAsync<InvalidOperationException>();

        // The audit pipeline would have flushed a pending mutation here. There is none.
        await db.SaveChangesAsync();

        var stored = await db.MsfCampaigns.SingleAsync();
        stored.State.Should().Be(MsfCampaignState.UnderReview);
        stored.ReleasedOn.Should().BeNull();
        stored.EvidenceRecordedOn.Should().BeNull();
        db.Activities.Should().BeEmpty();

        // And the retry still works.
        await ReleaseAsync(db, campaign.Id, entrustmentLevel: 4, narrative: null);
        (await db.MsfCampaigns.SingleAsync()).State.Should().Be(MsfCampaignState.Released);
        (await db.Activities.CountAsync()).Should().Be(1);
    }

    private static async Task ReleaseAsync(
        ApplicationDbContext db,
        int campaignId,
        int? entrustmentLevel,
        string? narrative)
    {
        var handler = new ReleaseMsfCampaignCommandHandler(
            db,
            new MsfAggregationService(),
            BuildActivityService(db),
            new ActivityReferenceDataService(db),
            NullLogger<ReleaseMsfCampaignCommandHandler>.Instance);

        await handler.Handle(
            new ReleaseMsfCampaignCommand(campaignId, CoordinatorUserId, narrative, entrustmentLevel, Coordinator()),
            CancellationToken.None);
    }

    /// <summary>The campaign's report as the coordinator who runs it reads it (<see cref="GetCampaignAggregateReportQuery" />).</summary>
    private static async Task<MsfCampaignAggregateReportDto> ReportAsync(ApplicationDbContext db, int campaignId)
        => (await new GetCampaignAggregateReportQueryHandler(db, new MsfAggregationService())
            .Handle(new GetCampaignAggregateReportQuery(campaignId, Coordinator()), CancellationToken.None))!;

    private static ActivityService BuildActivityService(ApplicationDbContext db, TimeProvider? clock = null)
        => new(db, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator(), clock);

    /// <summary>
    /// A curriculum, an admitted trainee, the real <c>msf_cpsa</c> type, and a campaign sitting in
    /// <c>UnderReview</c> with responses from <paramref name="respondingCategories" /> categories.
    /// </summary>
    private static MsfCampaign Seed(
        ApplicationDbContext db,
        IReadOnlyList<int> coveredEpaIds,
        int respondingCategories = 2)
    {
        db.Epas.Add(new Epa { Id = EpaOnCurriculum, SubSpecialityId = 1, Code = "EPA-1", Title = "Take a history", IsActive = true });
        db.Epas.Add(new Epa { Id = SecondEpaOnCurriculum, SubSpecialityId = 1, Code = "EPA-2", Title = "Lead a ward round", IsActive = true });
        db.Epas.Add(new Epa { Id = EpaOffCurriculum, SubSpecialityId = 1, Code = "EPA-X", Title = "From another curriculum", IsActive = true });

        db.CurriculumItems.Add(new CurriculumItem { Id = 4001, CurriculumId = CurriculumId, EpaId = EpaOnCurriculum, RequiredCount = 6, MinimumLevelOrder = 3 });
        db.CurriculumItems.Add(new CurriculumItem { Id = 4002, CurriculumId = CurriculumId, EpaId = SecondEpaOnCurriculum, RequiredCount = 6, MinimumLevelOrder = 3 });

        // The subject has to resolve to a SPECIALITY, not merely to an institution: `msf_cpsa` is
        // Speciality-scoped and StageCompletedAsync refuses to write a scoped type's record about a
        // subject outside that scope. That resolution walks TraineeProfile -> Curriculum ->
        // SubSpeciality -> Speciality, so all three rows have to exist.
        db.Set<Speciality>().Add(new Speciality { Id = SpecialityId, CollegeId = 1, Name = "Paediatrics" });
        db.Set<SubSpeciality>().Add(new SubSpeciality { Id = 1, SpecialityId = SpecialityId, Name = "General Paediatrics" });
        db.Set<Curriculum>().Add(new Curriculum { Id = CurriculumId, SubSpecialityId = 1, Name = "CPSA Paediatrics", Version = "11.1" });

        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = TraineeUserId,
            InstitutionId = InstitutionId,
            CurriculumId = CurriculumId,
            ProgrammeStartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-1),
            ExpectedCompletionDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(2),
            IsActive = true
        });

        db.ActivityTypes.Add(BuildSeededMsfType());

        var template = new MsfTemplate
        {
            Name = "Annual MSF",
            Questions =
            [
                new MsfQuestion { Order = 1, Prompt = "Professional performance", Type = MsfQuestionType.Scale, Required = true },
                new MsfQuestion { Order = 2, Prompt = "Comments", Type = MsfQuestionType.LongText, Required = false }
            ]
        };

        var campaign = new MsfCampaign
        {
            SubjectUserId = TraineeUserId,
            CreatedByUserId = CoordinatorUserId,
            CreatedOn = DateTime.UtcNow,
            OpensOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30),
            ClosesOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-2),
            MinimumResponses = 4,
            MinimumCategoryResponses = 2,
            MinimumRespondentCategories = 2,
            State = MsfCampaignState.UnderReview,
            // Closed EARLY, three days before the scheduled ClosesOn. The evidence date has to follow
            // the day the window actually shut: dating it from the schedule would stamp a future
            // ObservedOn on a campaign closed ahead of time.
            ClosedOn = DateTime.UtcNow.AddDays(-5),
            Template = template,
            CoveredEpas = coveredEpaIds.Select(epaId => new MsfCampaignEpa { EpaId = epaId }).ToList()
        };

        var categories = respondingCategories >= 2
            ? new[] { MsfRespondentCategory.Consultant, MsfRespondentCategory.Consultant, MsfRespondentCategory.Nurse, MsfRespondentCategory.Nurse }
            : [MsfRespondentCategory.PeerDoctor, MsfRespondentCategory.PeerDoctor, MsfRespondentCategory.PeerDoctor, MsfRespondentCategory.PeerDoctor];

        foreach (var category in categories)
        {
            var invitation = new MsfInvitation
            {
                RespondentCategory = category,
                RespondentEmailHash = "hash",
                TokenHash = Guid.NewGuid().ToString("N"),
                IssuedOn = DateTime.UtcNow.AddDays(-20),
                ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-2),
                RespondedOn = DateTime.UtcNow.AddDays(-5),
                AnonymizedOn = DateTime.UtcNow.AddDays(-2)
            };

            invitation.Responses.Add(new MsfResponse
            {
                SubmittedOn = DateTime.UtcNow.AddDays(-5),
                Campaign = campaign,
                Answers =
                [
                    new MsfResponseAnswer { QuestionId = 1, ScaleValue = 4 },
                    new MsfResponseAnswer { QuestionId = 2, LongText = "Unfailingly patient with families." }
                ]
            });

            campaign.Invitations.Add(invitation);
            foreach (var response in invitation.Responses)
            {
                campaign.Responses.Add(response);
            }
        }

        db.MsfCampaigns.Add(campaign);
        db.SaveChanges();

        return campaign;
    }

    /// <summary>The shipped <c>msf_cpsa</c> seed, read off disk and published at v1.</summary>
    private static ActivityType BuildSeededMsfType()
    {
        var schemaJson = ReadSeedFile("schema.json");
        var workflowJson = ReadSeedFile("workflow.json");
        var creditRulesJson = ReadSeedFile("credit.json");

        var activityType = new ActivityType
        {
            Id = MsfTypeId,
            Key = "msf_cpsa",
            Name = "Multi-Source Feedback (Paediatrics)",
            Scope = ActivityScope.Speciality,
            ScopeId = SpecialityId,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditRulesJson,
            DisplayFieldsJson = "[]",
            OwnerUserId = "seed-system",
            CreatedOn = DateTime.UtcNow,
            // As its ActivityTypeSeedEntry declares and the seeder stamps it (T162).
            SystemManaged = true
        };

        activityType.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = MsfTypeId,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditRulesJson,
            DisplayFieldsJson = "[]",
            PublishedByUserId = "seed-system",
            PublishedOn = DateTime.UtcNow
        });

        return activityType;
    }

    private static string ReadSeedFile(string fileName)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", "msf_cpsa", fileName));

    private static int ReadInt(string dataJson, string property)
    {
        using var document = JsonDocument.Parse(dataJson);
        return document.RootElement.GetProperty(property).GetInt32();
    }

    private static string? ReadString(string dataJson, string property)
    {
        using var document = JsonDocument.Parse(dataJson);
        return document.RootElement.GetProperty(property).GetString();
    }

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    /// <summary>
    /// The releasing reviewer. Built with <c>ClaimsIdentity</c> told which claim carries a role:
    /// <c>ClaimsPrincipal.IsInRole</c> is the BCL instance method and reads <c>RoleClaimType</c>, so an
    /// identity built without it matches `role:Coordinator` for nobody.
    /// </summary>
    private static ClaimsPrincipal Coordinator()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, CoordinatorUserId),
                new Claim(ClaimTypes.Role, WombatRoles.Coordinator),
                new Claim(WombatClaimTypes.InstitutionId, InstitutionId.ToString())
            ],
            "test",
            ClaimTypes.Name,
            ClaimTypes.Role));

    private static ClaimsPrincipal Principal(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test", ClaimTypes.Name, ClaimTypes.Role));
}
