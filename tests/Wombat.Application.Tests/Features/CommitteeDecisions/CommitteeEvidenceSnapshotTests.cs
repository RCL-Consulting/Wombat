using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.Activities;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Tests.Shared;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.CommitteeDecisions;

/// <summary>
/// Each line of a committee's evidence snapshot says which EPA it is about, by which instrument, at which rung and on
/// which day, frozen at Start (T167).
/// </summary>
/// <remarks>
/// Built on the SHIPPED seeds, read off disk and pinned at v1, so the fields read are the ones the product publishes:
/// <c>cca_cpsa</c> and <c>mini_cex_cpsa</c> rate on the v11.1 ladder, <c>reflective_exercise_cpsa</c> is unrated, and
/// <c>msf_cpsa</c> is the per-EPA record a released campaign writes.
/// </remarks>
public sealed class CommitteeEvidenceSnapshotTests
{
    private const int ReviewId = 30;
    private const int Paed001 = 7;
    private const int Paed002 = 8;
    private const string V11Ladder = "CPSA Paediatric Entrustment Scale v11.1";

    private static readonly DateOnly EncounterDay = new(2026, 2, 10);

    [Fact]
    public async Task ARatedLine_NamesItsEpaInstrumentRungAndEncounterDate()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var cca = await SeedTypeAsync(db, "cca_cpsa", "cca");
        var activity = AddActivity(db, cca, "completed", Rated(3), Paed001);
        await db.SaveChangesAsync();

        var line = (await StartAsync(db)).EvidenceItems.Should().ContainSingle(item => item.ActivityId == activity.Id).Subject;

        line.EpaId.Should().Be(Paed001);
        line.EpaCode.Should().Be("PAED-001");
        line.EpaTitle.Should().Be("Acute admission");
        line.InstrumentKey.Should().Be("cca");
        line.InstrumentName.Should().Be("CCA");
        line.IsRatedInstrument.Should().BeTrue();
        line.RatingOrder.Should().Be(3);
        line.RatingLabel.Should().Be("3a", "ordinal 3 is rung 3a on the v11.1 ladder the rated field names");
        line.ObservedOn.Should().Be(EncounterDay);
        line.ObservedOnDeclared.Should().BeTrue();
        line.SourceState.Should().Be("completed");
        line.SourceStateLabel.Should().Be("Completed");
        // T220: the summary names the state as the page does, by its label.
        line.Summary.Should().Contain("EPA PAED-001").And.Contain("CCA").And.Contain("rated 3a")
            .And.Contain("encounter 2026-02-10;").And.Contain("State: Completed")
            .And.NotContain("not recorded");
    }

    /// <summary>
    /// T220: a line names its state by the label its PINNED workflow gives it, frozen with the key, and quotes the same
    /// words in its summary. A submitted clinical audit is "Awaiting supervisor", as on its own page, not "submitted".
    /// </summary>
    [Fact]
    public async Task ALine_NamesItsStateByThePinnedWorkflowsLabel_AndKeepsTheKey()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var audit = await SeedTypeAsync(db, "clinical_audit_cpsa", "clinical_audit");
        var activity = AddActivity(db, audit, "submitted", """{ "epa_id": 7, "assessor_user_id": "assessor-a", "observed_on": "2026-02-10" }""", Paed001);
        await db.SaveChangesAsync();

        var line = (await StartAsync(db)).EvidenceItems.Single(item => item.ActivityId == activity.Id);

        line.SourceState.Should().Be("submitted");
        line.SourceStateLabel.Should().Be("Awaiting supervisor");
        line.Summary.Should().Contain("State: Awaiting supervisor").And.NotContain("submitted");

        var stored = await db.Set<CommitteeEvidence>().AsNoTracking().SingleAsync(item => item.ActivityId == activity.Id);
        stored.SourceStateLabel.Should().Be("Awaiting supervisor", "the label is frozen with the line");
    }

    /// <summary>
    /// T220: the label is the pinned version's, not the type's current one. A later version that renames the state does
    /// not reach an activity filed against the first.
    /// </summary>
    [Fact]
    public async Task ALine_IsLabelledByItsPinnedVersion_NotByTheTypesCurrentOne()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var audit = await SeedTypeAsync(db, "clinical_audit_cpsa", "clinical_audit");
        var renamed = audit.WorkflowJson!.Replace("Awaiting supervisor", "With the reviewer", StringComparison.Ordinal);
        audit.WorkflowJson = renamed;
        audit.Version = 2;
        audit.Versions.Add(new ActivityTypeVersion
        {
            Version = 2,
            SchemaJson = audit.SchemaJson!,
            WorkflowJson = renamed,
            CreditRulesJson = audit.CreditRulesJson!,
            DisplayFieldsJson = "[]",
            PublishedByUserId = "seed-system",
            PublishedOn = DateTime.UtcNow
        });
        var activity = AddActivity(db, audit, "submitted", """{ "epa_id": 7, "assessor_user_id": "assessor-a", "observed_on": "2026-02-10" }""", Paed001);
        activity.SchemaVersion = 1;
        await db.SaveChangesAsync();

        var line = (await StartAsync(db)).EvidenceItems.Single(item => item.ActivityId == activity.Id);

        line.SourceStateLabel.Should().Be("Awaiting supervisor");
    }

    /// <summary>
    /// T220: a line frozen before labels were has none, and the page is handed its key rather than nothing.
    /// </summary>
    [Fact]
    public async Task ALineFrozenBeforeLabels_IsShownByItsKey()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var cca = await SeedTypeAsync(db, "cca_cpsa", "cca");
        var activity = AddActivity(db, cca, "completed", Rated(3), Paed001);
        await db.SaveChangesAsync();
        await StartAsync(db);

        var stored = await db.Set<CommitteeEvidence>().SingleAsync(item => item.ActivityId == activity.Id);
        stored.SourceStateLabel = null;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var refreshed = await new GetCommitteeReviewByIdQueryHandler(db, FakeUserDirectory.Empty).Handle(
            new GetCommitteeReviewByIdQuery(ReviewId, Chair()), CancellationToken.None);

        refreshed.EvidenceItems.Single(item => item.ActivityId == activity.Id).SourceStateLabel.Should().Be("completed");
    }

    [Fact]
    public async Task TheLineIsFrozen_ALaterChangeToTheActivityDoesNotReachIt()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var cca = await SeedTypeAsync(db, "cca_cpsa", "cca");
        var activity = AddActivity(db, cca, "completed", Rated(3), Paed001);
        await db.SaveChangesAsync();
        await StartAsync(db);

        activity.EpaId = Paed002;
        activity.DataJson = """{ "epa_id": 8, "assessor_user_id": "assessor-a", "observed_on": "2026-03-01", "overall_level": 5 }""";
        activity.ObservedOn = new DateOnly(2026, 3, 1);
        await db.SaveChangesAsync();

        var refreshed = await new GetCommitteeReviewByIdQueryHandler(db, FakeUserDirectory.Empty).Handle(
            new GetCommitteeReviewByIdQuery(ReviewId, Chair()), CancellationToken.None);

        var line = refreshed.EvidenceItems.Single(item => item.ActivityId == activity.Id);
        line.EpaCode.Should().Be("PAED-001");
        line.RatingLabel.Should().Be("3a");
        line.ObservedOn.Should().Be(EncounterDay);
    }

    [Fact]
    public async Task AnUndatedActivity_IsMarkedNotRecorded_WithTheDayItWasCreated()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var cca = await SeedTypeAsync(db, "cca_cpsa", "cca");
        var activity = AddActivity(db, cca, "completed", Rated(3), Paed001);
        activity.ObservedOnSource = ObservationDateSource.CreatedOn;
        await db.SaveChangesAsync();

        var line = (await StartAsync(db)).EvidenceItems.Single(item => item.ActivityId == activity.Id);

        line.ObservedOn.Should().Be(EncounterDay);
        line.ObservedOnDeclared.Should().BeFalse();
        // The one wording of an undated encounter (T161, D28): the page's Encounter column and the trajectory say the same.
        line.Summary.Should().Contain("encounter not recorded (created 2026-02-10);")
            .And.NotContain("encounter 2026-02-10;");
    }

    [Fact]
    public async Task AnUnratedInstrument_SaysSo_AndIsStillUnderItsEpa()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var reflective = await SeedTypeAsync(db, "reflective_exercise_cpsa", "reflective_exercise");
        var activity = AddActivity(db, reflective, "completed", """{ "epa_id": 7, "observed_on": "2026-02-10" }""", Paed001);
        await db.SaveChangesAsync();

        var line = (await StartAsync(db)).EvidenceItems.Single(item => item.ActivityId == activity.Id);

        line.EpaCode.Should().Be("PAED-001");
        line.InstrumentName.Should().Be("Reflective exercise");
        line.IsRatedInstrument.Should().BeFalse();
        line.RatingOrder.Should().BeNull();
        line.RatingLabel.Should().BeNull();
        line.Summary.Should().Contain("unrated");
    }

    /// <summary>
    /// Every state is kept and labelled (T135): a request with no rating yet is a rated instrument whose rating nobody
    /// recorded, and a declined rating is printed beside its state rather than dropped.
    /// </summary>
    [Fact]
    public async Task EveryStateIsKept_WithWhatItsRatedFieldHolds()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var miniCex = await SeedTypeAsync(db, "mini_cex_cpsa", "mini_cex");
        var requested = AddActivity(db, miniCex, "requested", """{ "epa_id": 7, "assessor_user_id": "assessor-a", "observed_on": "2026-02-10" }""", Paed001);
        var declined = AddActivity(db, miniCex, "declined", Rated(2), Paed001);
        await db.SaveChangesAsync();

        var lines = (await StartAsync(db)).EvidenceItems;

        var request = lines.Single(item => item.ActivityId == requested.Id);
        request.IsRatedInstrument.Should().BeTrue();
        request.RatingLabel.Should().BeNull();
        request.SourceState.Should().Be("requested");
        request.Summary.Should().Contain("no rating recorded");

        var decline = lines.Single(item => item.ActivityId == declined.Id);
        decline.RatingLabel.Should().Be("2");
        decline.SourceState.Should().Be("declined");
    }

    /// <summary>
    /// Each line records whether it was finished work when the review started: D44's terminal state of the PINNED
    /// workflow, not the literal <c>completed</c>. A request nobody filled in and a declined one are not; a reflective
    /// exercise finishes in <c>discussed</c>, an MSF record in <c>recorded</c>, and a campaign line is always a released
    /// report. The committee page says when a staged decision names only unfinished lines (T131 review).
    /// </summary>
    [Fact]
    public async Task EachLineSaysWhetherItWasFinished_ByItsPinnedWorkflow()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var miniCex = await SeedTypeAsync(db, "mini_cex_cpsa", "mini_cex");
        var reflective = await SeedTypeAsync(db, "reflective_exercise_cpsa", "reflective_exercise");
        var msf = await SeedTypeAsync(db, "msf_cpsa", "msf");
        var completed = AddActivity(db, miniCex, "completed", Rated(3), Paed001);
        var requested = AddActivity(db, miniCex, "requested", """{ "epa_id": 7, "assessor_user_id": "assessor-a", "observed_on": "2026-02-10" }""", Paed001);
        var declined = AddActivity(db, miniCex, "declined", Rated(2), Paed001);
        var discussed = AddActivity(db, reflective, "discussed", """{ "epa_id": 7, "observed_on": "2026-02-10" }""", Paed001);
        var recorded = AddActivity(db, msf, "recorded", """{ "epa_id": 7, "campaign_id": 50, "observed_on": "2026-02-10", "respondent_count": 8 }""", Paed001);
        db.MsfTemplates.Add(new MsfTemplate { Id = 40, Name = "Annual MSF" });
        db.MsfCampaigns.Add(new MsfCampaign
        {
            Id = 50,
            SubjectUserId = "trainee-1",
            TemplateId = 40,
            CreatedByUserId = "coord-1",
            CreatedOn = new DateTime(2026, 2, 1, 8, 0, 0, DateTimeKind.Utc),
            OpensOn = new DateOnly(2026, 2, 1),
            ClosesOn = new DateOnly(2026, 2, 9),
            State = MsfCampaignState.Released,
            OpenedOn = new DateTime(2026, 2, 1, 8, 0, 0, DateTimeKind.Utc),
            ClosedOn = new DateTime(2026, 2, 10, 1, 0, 0, DateTimeKind.Utc),
            ReleasedOn = new DateTime(2026, 2, 12, 8, 0, 0, DateTimeKind.Utc)
        });
        await db.SaveChangesAsync();

        var lines = (await StartAsync(db)).EvidenceItems;

        bool? Finished(int activityId) => lines.Single(item => item.ActivityId == activityId).SourceFinished;
        Finished(completed.Id).Should().BeTrue();
        Finished(requested.Id).Should().BeFalse("a request nobody has filled in holds no assessment");
        Finished(declined.Id).Should().BeFalse("every seed makes declined a non-terminal dead end");
        Finished(discussed.Id).Should().BeTrue("a reflective exercise finishes in discussed, not completed");
        Finished(recorded.Id).Should().BeTrue("an MSF record finishes in recorded");
        lines.Single(item => item.MsfCampaignId == 50).SourceFinished.Should().BeTrue("only released campaigns are frozen");
    }

    /// <summary>
    /// A released campaign's per-EPA records are ordinary activity lines, each under its own EPA. The campaign's own
    /// line is a report across them and names no single EPA.
    /// </summary>
    [Fact]
    public async Task AnMsfRecordIsUnderItsEpa_AndTheCampaignLineNamesNone()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var msf = await SeedTypeAsync(db, "msf_cpsa", "msf");
        var forPaed001 = AddActivity(db, msf, "recorded", """{ "epa_id": 7, "campaign_id": 50, "observed_on": "2026-02-10", "respondent_count": 8 }""", Paed001);
        var forPaed002 = AddActivity(db, msf, "recorded", """{ "epa_id": 8, "campaign_id": 50, "observed_on": "2026-02-10", "respondent_count": 8, "overall_level": 4 }""", Paed002);
        db.MsfTemplates.Add(new MsfTemplate { Id = 40, Name = "Annual MSF" });
        db.MsfCampaigns.Add(new MsfCampaign
        {
            Id = 50,
            SubjectUserId = "trainee-1",
            TemplateId = 40,
            CreatedByUserId = "coord-1",
            CreatedOn = new DateTime(2026, 2, 1, 8, 0, 0, DateTimeKind.Utc),
            OpensOn = new DateOnly(2026, 2, 1),
            ClosesOn = new DateOnly(2026, 2, 9),
            State = MsfCampaignState.Released,
            OpenedOn = new DateTime(2026, 2, 1, 8, 0, 0, DateTimeKind.Utc),
            ClosedOn = new DateTime(2026, 2, 10, 1, 0, 0, DateTimeKind.Utc),
            ReleasedOn = new DateTime(2026, 2, 12, 8, 0, 0, DateTimeKind.Utc)
        });
        await db.SaveChangesAsync();

        var lines = (await StartAsync(db)).EvidenceItems;

        lines.Single(item => item.ActivityId == forPaed001.Id).Should().Match<CommitteeEvidenceDto>(line =>
            line.EpaCode == "PAED-001" && line.InstrumentName == "MSF" && line.RatingLabel == null);
        lines.Single(item => item.ActivityId == forPaed002.Id).Should().Match<CommitteeEvidenceDto>(line =>
            line.EpaCode == "PAED-002" && line.InstrumentName == "MSF" && line.RatingLabel == "3b");

        var campaign = lines.Should().ContainSingle(item => item.MsfCampaignId == 50).Subject;
        campaign.EpaId.Should().BeNull();
        campaign.SourceState.Should().Be("Released");
        campaign.SourceStateLabel.Should().Be("Released");
        campaign.FrozenBeforeLinesNamedTheirEpa.Should().BeFalse("a campaign line never carried an EPA");
    }

    /// <summary>
    /// T164: a released learner-feedback campaign's record is a line under its EPA named as learner feedback, not MSF, and
    /// the campaign's own line says it is learner feedback and how many teaching contexts its learners answered from:
    /// EPA 15's "at least two teaching contexts", for the panel to weigh. Counted, never named: the line is frozen and
    /// copied onto any STAR resting on it, whose certificate the trainee holds (T164 review).
    /// </summary>
    /// <remarks>
    /// An MSF campaign sits in the same snapshot, so the one read of both campaigns' coverage must take each from the rows
    /// of its own kind's type: PAED-001 from the learner-feedback record, PAED-002 from the MSF one (T186, T164 review).
    /// </remarks>
    [Fact]
    public async Task ALearnerFeedbackRecord_IsUnderItsEpaAsLearnerFeedback_AndTheCampaignLineCountsTheTeachingContexts()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        db.WbaTools.Add(new WbaTool { Id = 5, Key = "learner_feedback", Name = "Learner feedback" });
        var learnerFeedback = await SeedTypeAsync(db, "learner_feedback_cpsa", "learner_feedback");
        var msf = await SeedTypeAsync(db, "msf_cpsa", "msf");
        var record = AddActivity(db, learnerFeedback, "recorded",
            """{ "epa_id": 7, "campaign_id": 51, "observed_on": "2026-02-10", "respondent_count": 3, "teaching_context_count": 2 }""", Paed001);
        AddActivity(db, msf, "recorded",
            """{ "epa_id": 8, "campaign_id": 50, "observed_on": "2026-02-10", "respondent_count": 8 }""", Paed002);
        db.MsfTemplates.Add(new MsfTemplate { Id = 40, Name = "Annual MSF" });
        db.MsfCampaigns.Add(ReleasedCampaign(50, templateId: 40, Paed002));
        AddLearnerFeedbackCampaign(db, 51, Paed001);
        await db.SaveChangesAsync();

        var lines = (await StartAsync(db)).EvidenceItems;

        lines.Single(item => item.ActivityId == record.Id).Should().Match<CommitteeEvidenceDto>(line =>
            line.EpaCode == "PAED-001" && line.InstrumentName == "Learner feedback" && line.RatingLabel == null);

        var campaignLine = lines.Should().ContainSingle(item => item.MsfCampaignId == 51).Subject;
        campaignLine.Summary.Should().StartWith("Learner feedback.")
            .And.Contain("responses 3 from 2 teaching contexts;")
            .And.Contain("Evidence recorded for PAED-001, one activity each.")
            .And.NotContain("Ward round").And.NotContain("Student tutorial");

        lines.Should().ContainSingle(item => item.MsfCampaignId == 50).Which.Summary.Should()
            .StartWith("Multi-source feedback.")
            .And.Contain("Evidence recorded for PAED-002, one activity each.")
            .And.NotContain("teaching context");
    }

    /// <summary>
    /// The other direction: an <c>msf_cpsa</c> row naming a learner-feedback campaign, which no release writes, does not
    /// make its declared EPA recorded. (T164 review)
    /// </summary>
    [Fact]
    public async Task ALearnerFeedbackCampaignLine_IsNotRecordedByAnMsfRecordNamingIt()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var msf = await SeedTypeAsync(db, "msf_cpsa", "msf");
        AddActivity(db, msf, "recorded",
            """{ "epa_id": 7, "campaign_id": 51, "observed_on": "2026-02-10", "respondent_count": 3 }""", Paed001);
        AddLearnerFeedbackCampaign(db, 51, Paed001);
        await db.SaveChangesAsync();

        var campaignLine = (await StartAsync(db)).EvidenceItems.Should().ContainSingle(item => item.MsfCampaignId == 51).Subject;

        campaignLine.Summary.Should().Contain("Declared but not recorded: PAED-001.").And.NotContain("Evidence recorded for");
    }
    /// <summary>
    /// The rating is read by the profile of the version the activity is PINNED to (T135's reader), not the type's
    /// current one: a v1 row's rating lives in v1's rated field, and the key the current version rates on is a decoy.
    /// </summary>
    [Fact]
    public async Task TheRatingIsReadFromThePinnedVersionsRatedField()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var cca = await SeedTypeAsync(db, "cca_cpsa", "cca");
        var versionOneSchema = ReadSeedFile("cca_cpsa", "schema.json")
            .Replace("\"rated_level_field\": \"overall_level\"", "\"rated_level_field\": \"first_level\"", StringComparison.Ordinal)
            .Replace("\"key\": \"overall_level\"", "\"key\": \"first_level\"", StringComparison.Ordinal);
        var versionOne = await db.ActivityTypeVersions.SingleAsync(version => version.ActivityTypeId == cca.Id && version.Version == 1);
        versionOne.SchemaJson = versionOneSchema;
        cca.Version = 2;
        cca.Versions.Add(new ActivityTypeVersion
        {
            Version = 2,
            SchemaJson = ReadSeedFile("cca_cpsa", "schema.json"),
            WorkflowJson = ReadSeedFile("cca_cpsa", "workflow.json"),
            CreditRulesJson = ReadSeedFile("cca_cpsa", "credit.json"),
            PublishedByUserId = "seed-system",
            PublishedOn = DateTime.UtcNow
        });
        var activity = AddActivity(db, cca, "completed",
            """{ "epa_id": 7, "assessor_user_id": "assessor-a", "observed_on": "2026-02-10", "first_level": 4, "overall_level": 6 }""",
            Paed001);
        await db.SaveChangesAsync();

        var line = (await StartAsync(db)).EvidenceItems.Single(item => item.ActivityId == activity.Id);

        line.RatingOrder.Should().Be(4);
        line.RatingLabel.Should().Be("3b");
    }

    [Fact]
    public async Task ARatingOnALadderThatResolvesToNothing_IsTheBareOrdinal()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var cca = await SeedTypeAsync(
            db,
            "cca_cpsa",
            "cca",
            schema => schema.Replace(V11Ladder, "A ladder nobody seeded", StringComparison.Ordinal));
        var activity = AddActivity(db, cca, "completed", Rated(3), Paed001);
        await db.SaveChangesAsync();

        var line = (await StartAsync(db)).EvidenceItems.Single(item => item.ActivityId == activity.Id);

        line.RatingOrder.Should().Be(3);
        line.RatingLabel.Should().Be("3");
    }

    [Fact]
    public async Task ATypeThatDeclaresNoInstrument_IsNamedByItsTypeAndAnActivityAboutNoEpaNamesNone()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var reflective = await SeedTypeAsync(db, "reflective_exercise_cpsa", wbaToolKey: null);
        var activity = AddActivity(db, reflective, "draft", """{ "observed_on": "2026-02-10" }""", epaId: null);
        await db.SaveChangesAsync();

        var line = (await StartAsync(db)).EvidenceItems.Single(item => item.ActivityId == activity.Id);

        line.InstrumentKey.Should().BeNull();
        line.InstrumentName.Should().Be("Reflective exercise (Paediatrics)");
        line.EpaId.Should().BeNull();
        line.EpaCode.Should().BeNull();
        line.Summary.Should().Contain("about no EPA");
        line.FrozenBeforeLinesNamedTheirEpa.Should().BeFalse("it was frozen with its encounter date");
    }

    // ---- The assessor, and the chair who rated everything (T165) -------------------------------------------------

    [Fact]
    public async Task ALine_FreezesTheAssessorItsVersionNames()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var cca = await SeedTypeAsync(db, "cca_cpsa", "cca");
        var activity = AddActivity(db, cca, "completed", Rated(3), Paed001);
        await db.SaveChangesAsync();

        (await StartAsync(db)).EvidenceItems.Single(item => item.ActivityId == activity.Id)
            .AssessorUserId.Should().Be("assessor-a");

        activity.DataJson = Rated(3, assessor: "assessor-b");
        await db.SaveChangesAsync();
        var refreshed = await new GetCommitteeReviewByIdQueryHandler(db, FakeUserDirectory.Empty).Handle(
            new GetCommitteeReviewByIdQuery(ReviewId, Chair()), CancellationToken.None);

        refreshed.EvidenceItems.Single(item => item.ActivityId == activity.Id).AssessorUserId
            .Should().Be("assessor-a", "the snapshot records who the panel was shown, not who the activity names now");
    }

    [Fact]
    public async Task AnMsfRecord_NamesNoAssessor()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var msf = await SeedTypeAsync(db, "msf_cpsa", "msf");
        var activity = AddActivity(db, msf, "recorded",
            """{ "epa_id": 7, "campaign_id": 50, "observed_on": "2026-02-10", "respondent_count": 8, "overall_level": 4 }""", Paed001);
        await db.SaveChangesAsync();

        var line = (await StartAsync(db)).EvidenceItems.Single(item => item.ActivityId == activity.Id);

        line.RatingOrder.Should().Be(4);
        line.AssessorUserId.Should().BeNull("an MSF's version names nobody who writes its rating (D36)");
    }

    [Fact]
    public async Task WhenTheChairRatedEveryLine_TheReviewNamesThem()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var cca = await SeedTypeAsync(db, "cca_cpsa", "cca");
        var reflective = await SeedTypeAsync(db, "reflective_exercise_cpsa", "reflective_exercise");
        AddActivity(db, cca, "completed", Rated(3, assessor: "chair-1"), Paed001);
        AddActivity(db, cca, "declined", Rated(2, assessor: "chair-1"), Paed001);
        // Unrated: no rating in it to be anyone's, so it neither sets the flag nor clears it.
        AddActivity(db, reflective, "completed", """{ "epa_id": 7, "observed_on": "2026-02-10" }""", Paed001);
        await db.SaveChangesAsync();

        var review = await StartAsync(db);

        review.ChairRatedEveryLine.Should().NotBeNull();
        review.ChairRatedEveryLine!.UserId.Should().Be("chair-1");
    }

    public static TheoryData<string, string> ALineThatIsNotTheChairs => new()
    {
        { "another assessor's rating", Rated(3, assessor: "assessor-a") },
        { "a rating with no assessor named", """{ "epa_id": 7, "observed_on": "2026-02-10", "overall_level": 3 }""" }
    };

    [Theory]
    [MemberData(nameof(ALineThatIsNotTheChairs))]
    public async Task OneRatedLineThatIsNotTheChairs_ClearsTheFlag(string because, string otherLine)
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var cca = await SeedTypeAsync(db, "cca_cpsa", "cca");
        AddActivity(db, cca, "completed", Rated(3, assessor: "chair-1"), Paed001);
        AddActivity(db, cca, "completed", otherLine, Paed001);
        await db.SaveChangesAsync();

        (await StartAsync(db)).ChairRatedEveryLine.Should().BeNull(because);
    }

    [Fact]
    public async Task ASnapshotWithNoRatedLine_RaisesNoFlag()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var reflective = await SeedTypeAsync(db, "reflective_exercise_cpsa", "reflective_exercise");
        AddActivity(db, reflective, "completed", """{ "epa_id": 7, "observed_on": "2026-02-10" }""", Paed001);
        await db.SaveChangesAsync();

        (await StartAsync(db)).ChairRatedEveryLine.Should().BeNull("the chair rated nothing, so they did not rate everything");
    }

    // ---- Fixture -------------------------------------------------------------------------------------------------

    private static string Rated(int level, string assessor = "assessor-a")
        => $$"""{ "epa_id": 7, "assessor_user_id": "{{assessor}}", "observed_on": "2026-02-10", "overall_level": {{level}} }""";

    private static async Task<CommitteeReviewDetailDto> StartAsync(ApplicationDbContext db)
        => await new StartCommitteeReviewCommandHandler(db).Handle(
            new StartCommitteeReviewCommand(ReviewId, Chair()), CancellationToken.None);

    private static ClaimsPrincipal Chair()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "chair-1"),
                new Claim(ClaimTypes.Role, WombatRoles.CommitteeMember),
                new Claim(WombatClaimTypes.InstitutionId, "1")
            ],
            "test"));

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static async Task SeedAsync(ApplicationDbContext db)
    {
        db.EntrustmentScales.Add(new EntrustmentScale { Id = 1, Name = V11Ladder });
        db.EntrustmentLevels.AddRange(
            new[] { "1", "2", "3a", "3b", "4", "5" }.Select((label, index) => new EntrustmentLevel
            {
                Id = index + 1,
                ScaleId = 1,
                Order = index + 1,
                Label = label
            }));
        db.Epas.AddRange(
            new Epa { Id = Paed001, SubSpecialityId = 1, Code = "PAED-001", Title = "Acute admission", IsActive = true },
            new Epa { Id = Paed002, SubSpecialityId = 1, Code = "PAED-002", Title = "Ward round", IsActive = true });
        db.WbaTools.AddRange(
            new WbaTool { Id = 1, Key = "cca", Name = "CCA" },
            new WbaTool { Id = 2, Key = "mini_cex", Name = "Mini-CEX" },
            new WbaTool { Id = 3, Key = "msf", Name = "MSF" },
            new WbaTool { Id = 4, Key = "reflective_exercise", Name = "Reflective exercise" });
        // The trainee trains at the panel's institution: a panel acts only on its own institution's trainees (T182).
        db.Specialities.Add(new Speciality { Id = 1, CollegeId = 1, Name = "Paediatrics", IsActive = true });
        db.SubSpecialities.Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "General paediatrics", IsActive = true });
        db.Curricula.Add(new Curriculum { Id = 1, SubSpecialityId = 1, Name = "Paediatrics", Version = "v11.1" });
        db.TraineeProfiles.Add(new TraineeProfile
        {
            UserId = "trainee-1",
            InstitutionId = 1,
            CurriculumId = 1,
            ProgrammeStartDate = new DateOnly(2025, 1, 15),
            ExpectedCompletionDate = new DateOnly(2029, 12, 31)
        });
        db.DecisionPanels.Add(new DecisionPanel
        {
            Id = 20,
            Name = "Paediatrics CCC",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = 1,
            CreatedOn = DateTime.UtcNow,
            Members = [new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair }]
        });
        db.CommitteeReviews.Add(new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 1,
            Id = ReviewId,
            PanelId = 20,
            TraineeUserId = "trainee-1",
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 3, 31),
            ScheduledOn = new DateOnly(2026, 4, 1)
        });
        await db.SaveChangesAsync();
    }

    /// <summary>A shipped seed, published at v1 with its version row, as <c>ActivityService</c> would pin to it.</summary>
    private static async Task<ActivityType> SeedTypeAsync(
        ApplicationDbContext db,
        string key,
        string? wbaToolKey,
        Func<string, string>? editSchema = null)
    {
        var schema = ReadSeedFile(key, "schema.json");
        if (editSchema is not null)
        {
            schema = editSchema(schema);
        }

        var workflow = ReadSeedFile(key, "workflow.json");
        var credit = ReadSeedFile(key, "credit.json");
        var type = new ActivityType
        {
            Key = key,
            Name = key == "reflective_exercise_cpsa" ? "Reflective exercise (Paediatrics)" : key,
            Version = 1,
            IsActive = true,
            OwnerUserId = "seed-system",
            CreatedOn = DateTime.UtcNow,
            SchemaJson = schema,
            WorkflowJson = workflow,
            CreditRulesJson = credit,
            WbaToolKey = wbaToolKey
        };
        type.Versions.Add(new ActivityTypeVersion
        {
            Version = 1,
            SchemaJson = schema,
            WorkflowJson = workflow,
            CreditRulesJson = credit,
            PublishedByUserId = "seed-system",
            PublishedOn = DateTime.UtcNow
        });
        db.ActivityTypes.Add(type);
        await db.SaveChangesAsync();
        return type;
    }

    private static string ReadSeedFile(string key, string fileName)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", key, fileName));

    /// <summary>A campaign released in the review window, declared evidence for <paramref name="coveredEpaIds" />.</summary>
    private static MsfCampaign ReleasedCampaign(int id, int templateId, params int[] coveredEpaIds)
        => new()
        {
            Id = id,
            SubjectUserId = "trainee-1",
            TemplateId = templateId,
            CreatedByUserId = "coord-1",
            CreatedOn = new DateTime(2026, 2, 1, 8, 0, 0, DateTimeKind.Utc),
            OpensOn = new DateOnly(2026, 2, 1),
            ClosesOn = new DateOnly(2026, 2, 9),
            State = MsfCampaignState.Released,
            OpenedOn = new DateTime(2026, 2, 1, 8, 0, 0, DateTimeKind.Utc),
            ClosedOn = new DateTime(2026, 2, 10, 1, 0, 0, DateTimeKind.Utc),
            ReleasedOn = new DateTime(2026, 2, 12, 8, 0, 0, DateTimeKind.Utc),
            CoveredEpas = coveredEpaIds.Select(epaId => new MsfCampaignEpa { EpaId = epaId }).ToList()
        };

    /// <summary>
    /// A released learner-feedback campaign, declared evidence for <paramref name="coveredEpaId" />, answered by three
    /// learners from two teaching contexts ("Ward round" typed two ways, and "Student tutorial").
    /// </summary>
    private static void AddLearnerFeedbackCampaign(ApplicationDbContext db, int id, int coveredEpaId)
    {
        db.MsfTemplates.Add(new MsfTemplate { Id = 41, Name = "Learner feedback (interim questionnaire)", Kind = MsfTemplateKind.LearnerFeedback });

        var campaign = ReleasedCampaign(id, templateId: 41, coveredEpaId);
        foreach (var context in new[] { "Ward round", "ward round", "Student tutorial" })
        {
            var invitation = new MsfInvitation
            {
                RespondentCategory = MsfRespondentCategory.Learner,
                TeachingContext = context,
                TokenHash = Guid.NewGuid().ToString("N"),
                RespondedOn = new DateTime(2026, 2, 5, 8, 0, 0, DateTimeKind.Utc)
            };
            invitation.Responses.Add(new MsfResponse { Campaign = campaign, SubmittedOn = new DateTime(2026, 2, 5, 8, 0, 0, DateTimeKind.Utc) });
            campaign.Invitations.Add(invitation);
        }

        db.MsfCampaigns.Add(campaign);
    }

    /// <summary>
    /// A row as <c>ActivityService</c> leaves it. The EPA is set as the stamp (T137) the snapshot reads; the data's
    /// <c>epa_id</c> is never consulted, which <see cref="TheLineIsFrozen_ALaterChangeToTheActivityDoesNotReachIt" />
    /// leans on.
    /// </summary>
    private static Activity AddActivity(ApplicationDbContext db, ActivityType type, string state, string dataJson, int? epaId)
    {
        var filed = new DateTime(2026, 2, 10, 9, 0, 0, DateTimeKind.Utc);
        var activity = new Activity
        {
            ActivityTypeId = type.Id,
            SchemaVersion = 1,
            InstitutionId = 1,
            SubjectUserId = "trainee-1",
            CreatedByUserId = "trainee-1",
            CurrentState = state,
            DataJson = dataJson,
            EpaId = epaId,
            CreatedOn = filed,
            ObservedOn = EncounterDay,
            ObservedOnSource = ObservationDateSource.Declared,
            UpdatedOn = filed
        };
        db.Activities.Add(activity);
        return activity;
    }
}
