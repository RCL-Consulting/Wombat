using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.CommitteeDecisions;

/// <summary>
/// Which rows the committee sampling report counts, and where each one lands (T135, T150, D44).
/// </summary>
/// <remarks>
/// <para>
/// These run on the SHIPPED <c>mini_cex_cpsa</c> and <c>msf_cpsa</c> seeds, read off disk and pinned at v1, so the
/// states and fields under test are the ones the product actually publishes.
/// </para>
/// <para>
/// <b>The partition:</b> every activity of a rated type in the review window whose state is terminal in its pinned
/// workflow is in exactly one of Total, Withheld, Unreadable and Unattributed. Nothing else is in any of them.
/// </para>
/// </remarks>
public sealed class SamplingEvidenceStateAndAttributionTests
{
    private const int HostInstitution = 1;
    private const int OtherInstitution = 2;
    private const int ReviewId = 42;
    private const int EpaId = 7;

    private static readonly DateTime InWindow = new(2026, 2, 10, 9, 0, 0, DateTimeKind.Utc);

    // ---- D44: only a terminal state of the pinned workflow is evidence -------------------------------------------

    /// <summary>
    /// A draft, a request, a declined rating and a cancelled request each name an assessor, and the declined one even
    /// carries a rating. None of them may move a count: before T135 each raised the distinct-assessor count.
    /// </summary>
    [Theory]
    [InlineData("draft")]
    [InlineData("requested")]
    [InlineData("declined")]
    [InlineData("cancelled")]
    public async Task ARowThatIsNotInATerminalState_MovesNoCount(string state)
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var miniCex = await SeedTypeFromSeedFolderAsync(db, "mini_cex_cpsa");

        AddActivity(db, miniCex, "completed", Rated("assessor-a"));
        AddActivity(db, miniCex, "completed", Rated("assessor-a"));
        AddActivity(db, miniCex, "completed", Rated("assessor-b"));
        await db.SaveChangesAsync();
        var before = await ReportAsync(db, Administrator());

        AddActivity(db, miniCex, state, Rated("assessor-z"));
        await db.SaveChangesAsync();
        var after = await ReportAsync(db, Administrator());

        before.TotalRatedActivities.Should().Be(3);
        before.DistinctAssessorCount.Should().Be(2);
        after.Should().BeEquivalentTo(before, $"a {state} activity is not evidence (D44)");
        after.PerEpa.Should().ContainSingle().Which.DistinctAssessorCount.Should().Be(2);
    }

    /// <summary>
    /// T150's padding: five drafts and requests naming five eligible assessors used to clear the fewer-than-three
    /// warning without anyone assessing anything.
    /// </summary>
    [Fact]
    public async Task NamingAssessorsOnUnfinishedRequests_DoesNotClearTheWarning()
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var miniCex = await SeedTypeFromSeedFolderAsync(db, "mini_cex_cpsa");

        AddActivity(db, miniCex, "completed", Rated("assessor-a"));
        AddActivity(db, miniCex, "draft", Named("assessor-b"));
        AddActivity(db, miniCex, "draft", Named("assessor-c"));
        AddActivity(db, miniCex, "requested", Named("assessor-d"));
        AddActivity(db, miniCex, "requested", Named("assessor-e"));
        AddActivity(db, miniCex, "cancelled", Named("assessor-f"));
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Administrator());

        report.TotalRatedActivities.Should().Be(1);
        report.DistinctAssessorCount.Should().Be(1);
        report.PerEpa.Should().ContainSingle().Which.FewerThanThreeAssessors.Should().BeTrue();
    }

    [Fact]
    public async Task ARowThatIsNotInATerminalState_IsNotWithheldEither()
    {
        // The denominator counts what the caller may not read, so it must apply the same state rule or a withheld
        // draft would report the sample as incomplete.
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var miniCex = await SeedTypeFromSeedFolderAsync(db, "mini_cex_cpsa");

        AddActivity(db, miniCex, "completed", Rated("assessor-a"), OtherInstitution);
        AddActivity(db, miniCex, "completed", Rated("assessor-b"), OtherInstitution);
        AddActivity(db, miniCex, "draft", Rated("assessor-c"), OtherInstitution);
        AddActivity(db, miniCex, "declined", Rated("assessor-d"), OtherInstitution);
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Principal("member-1", WombatRoles.CommitteeMember, HostInstitution));

        report.WithheldRatedActivities.Should().Be(2);
        report.TotalRatedActivities.Should().Be(0);
    }

    // ---- The PINNED version decides, not the type's current one ------------------------------------------------

    /// <summary>
    /// The type is now <c>mini_cex_cpsa</c> as shipped (v2). Its v1 made <c>declined</c> terminal and let a field called
    /// <c>observer</c> write the rating. A v1 row is read by v1: its declined row counts and each is attributed to its
    /// observer, not to the v2 assessor field it also carries. A v2 declined row does not count.
    /// </summary>
    [Fact]
    public async Task EachRowIsReadByThePinnedVersion()
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var type = await SeedTwoVersionTypeAsync(db);

        AddActivity(db, type, "completed", VersionOneRow("obs-a"), schemaVersion: 1);
        AddActivity(db, type, "declined", VersionOneRow("obs-b"), schemaVersion: 1);
        AddActivity(db, type, "completed", Rated("assessor-c"), schemaVersion: 2);
        AddActivity(db, type, "declined", Rated("assessor-d"), schemaVersion: 2);
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Administrator());

        report.TotalRatedActivities.Should().Be(3, "v1's declined is terminal; v2's is a dead end");
        report.DistinctAssessorCount.Should().Be(3, "obs-a and obs-b by v1's rule, assessor-c by v2's; never the v1 decoy");
        report.PerEpa.Should().ContainSingle().Which.DominantAssessorUserId.Should().NotBe("decoy-v1");
        report.UnreadableRatedActivities.Should().Be(0);
    }

    /// <summary>
    /// A version that declares no rated field, and lacks the one its type now rates on, cannot hold the rating. That is
    /// not "the rating was left empty"; the report must say it could not read those rows.
    /// </summary>
    [Fact]
    public async Task AVersionMissingTheTypesRatedField_IsUnreadable()
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var type = await SeedTypeAsync(db, "ward_round_review", BuilderSchema, BuilderWorkflow, BuilderCredit);
        db.ActivityTypeVersions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = type.Id,
            Version = 2,
            SchemaJson = BuilderSchema,
            WorkflowJson = BuilderWorkflow,
            CreditRulesJson = BuilderCredit,
            PublishedByUserId = "seed-system",
            PublishedOn = DateTime.UtcNow
        });
        var versionOne = await db.ActivityTypeVersions.SingleAsync(version => version.ActivityTypeId == type.Id && version.Version == 1);
        versionOne.SchemaJson = PreRatedFieldSchema;
        type.Version = 2;
        await db.SaveChangesAsync();

        AddActivity(db, type, "signed_off", Builder("sup-a"), schemaVersion: 2);
        AddActivity(db, type, "signed_off", """{ "target_epa": 7, "supervisor": "sup-b", "overall": 3 }""", schemaVersion: 1);
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Administrator());

        report.TotalRatedActivities.Should().Be(1);
        report.UnreadableRatedActivities.Should().Be(1, "v1 has no 'entrustment' field to hold the rating");
        report.UnattributedRatedActivities.Should().Be(0);
        report.EvidenceComplete.Should().BeFalse();
    }

    // ---- Unreadable: its own count, and the report is incomplete ------------------------------------------------

    /// <summary>
    /// The first row names no EPA and the seventh one no EPA exists for, so each is stamped with none (T137) and is a
    /// rating of nothing; the last two are not data at all, and stamp none either. The rest have their EPA and are
    /// broken in the rating or the assessor.
    /// </summary>
    [Theory]
    [InlineData("""{ "assessor_user_id": "assessor-z", "overall_level": 3 }""")]
    [InlineData("""{ "epa_id": 7, "overall_level": 3 }""")]
    [InlineData("""{ "epa_id": 7, "assessor_user_id": "", "overall_level": 3 }""")]
    [InlineData("""{ "epa_id": 7, "assessor_user_id": 12, "overall_level": 3 }""")]
    [InlineData("""{ "epa_id": 7, "assessor_user_id": "assessor-z", "overall_level": "3a" }""")]
    [InlineData("""{ "epa_id": 7, "assessor_user_id": "assessor-z", "overall_level": 2026 }""")]
    [InlineData("""{ "epa_id": 99, "assessor_user_id": "assessor-z", "overall_level": 3 }""")]
    [InlineData("""[ 7 ]""")]
    [InlineData("""not json""")]
    public async Task AnUnreadableRow_IsCountedAndMakesTheReportIncomplete(string dataJson)
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var miniCex = await SeedTypeFromSeedFolderAsync(db, "mini_cex_cpsa");

        AddActivity(db, miniCex, "completed", Rated("assessor-a"));
        AddActivity(db, miniCex, "completed", Rated("assessor-b"));
        AddActivity(db, miniCex, "completed", dataJson);
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Administrator());

        report.UnreadableRatedActivities.Should().Be(1);
        report.EvidenceComplete.Should().BeFalse("a row nobody could read is evidence the figures do not cover");
        report.TotalRatedActivities.Should().Be(2);
        report.WithheldRatedActivities.Should().Be(0);
        report.UnattributedRatedActivities.Should().Be(0);
    }

    /// <summary>
    /// The stamp names an EPA that existed when it was written; <c>Activity.EpaId</c> carries no foreign key, so one
    /// deleted since would dangle. That row is unreadable, not a rating of nothing and not an exception.
    /// </summary>
    [Fact]
    public async Task AStampedEpaThatNoLongerExists_IsUnreadable()
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var miniCex = await SeedTypeFromSeedFolderAsync(db, "mini_cex_cpsa");

        AddActivity(db, miniCex, "completed", Rated("assessor-a"));
        AddActivity(db, miniCex, "completed", Rated("assessor-z")).EpaId = 99;
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Administrator());

        report.UnreadableRatedActivities.Should().Be(1);
        report.TotalRatedActivities.Should().Be(1);
        report.DistinctAssessorCount.Should().Be(1, "assessor-z's rating is of an EPA nobody can name");
        report.EvidenceComplete.Should().BeFalse();
    }

    /// <summary>
    /// <c>mini_cex_cpsa</c>'s <c>overall_level</c> is required, and <c>complete</c> checks every required field
    /// (T105). A completed mini-CEX with no rating is therefore broken data, not "a rating left blank".
    /// </summary>
    [Theory]
    [InlineData("""{ "epa_id": 7, "assessor_user_id": "assessor-z" }""")]
    [InlineData("""{ "epa_id": 7, "assessor_user_id": "assessor-z", "overall_level": null }""")]
    [InlineData("""{ "epa_id": 7, "assessor_user_id": "assessor-z", "overall_level": " " }""")]
    public async Task ARequiredRatingLeftEmpty_IsUnreadable(string dataJson)
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var miniCex = await SeedTypeFromSeedFolderAsync(db, "mini_cex_cpsa");

        AddActivity(db, miniCex, "completed", Rated("assessor-a"));
        AddActivity(db, miniCex, "completed", dataJson);
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Administrator());

        report.UnreadableRatedActivities.Should().Be(1);
        report.UnattributedRatedActivities.Should().Be(0);
        report.TotalRatedActivities.Should().Be(1);
        report.DistinctAssessorCount.Should().Be(1, "assessor-z gave no rating to count");
        report.EvidenceComplete.Should().BeFalse();
    }

    /// <summary>
    /// "Required" means required by the move that got the row there. v1's <c>declined</c> is terminal, so it is
    /// evidence, but <c>decline</c> checks formats only: a required rating left empty there was allowed.
    /// </summary>
    [Fact]
    public async Task ARequiredRatingEmptyInAStateNoMoveChecked_IsUnattributed()
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var type = await SeedTwoVersionTypeAsync(db);

        AddActivity(db, type, "completed", VersionOneRow("obs-a"), schemaVersion: 1);
        AddActivity(db, type, "declined", """{ "epa_id": 7, "observer": "obs-b" }""", schemaVersion: 1);
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Administrator());

        report.UnattributedRatedActivities.Should().Be(1);
        report.UnreadableRatedActivities.Should().Be(0);
        report.EvidenceComplete.Should().BeTrue();
    }

    /// <summary>
    /// A row can be created straight into a state its type is born in, so no move checked it, even when every move
    /// back into that state does.
    /// </summary>
    [Fact]
    public async Task ARequiredRatingEmptyInAStateARowCanBeBornIn_IsUnattributed()
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var type = await SeedTypeAsync(db, "procedure_rating", BuilderSchema, BornTerminalWorkflow, BuilderCredit);

        AddActivity(db, type, "logged", Builder("sup-a"));
        AddActivity(db, type, "logged", """{ "target_epa": 7, "supervisor": "sup-b" }""");
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Administrator());

        report.TotalRatedActivities.Should().Be(1);
        report.UnattributedRatedActivities.Should().Be(1);
        report.UnreadableRatedActivities.Should().Be(0);
    }

    /// <summary>
    /// <c>requires_fields</c> adds a field to what a move insists on under every <c>validation</c> value (T105), so a
    /// rating the schema leaves optional is still required on a row that move produced.
    /// </summary>
    [Fact]
    public async Task ARatingTheMoveListsInRequiresFields_LeftEmpty_IsUnreadable()
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var type = await SeedTypeAsync(db, "optional_review", OptionalSchema, RequiresRatingWorkflow, BuilderCredit);

        AddActivity(db, type, "signed_off", Builder("sup-a"));
        AddActivity(db, type, "signed_off", """{ "target_epa": 7, "supervisor": "sup-b" }""");
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Administrator());

        report.UnreadableRatedActivities.Should().Be(1);
        report.UnattributedRatedActivities.Should().Be(0);
    }

    /// <summary>
    /// Only a version KNOWN to name no assessor moves a withheld row out of Withheld. One nobody can read might have
    /// named one, so its row stays withheld.
    /// </summary>
    [Fact]
    public async Task AWithheldRowOfAnUnreadableVersion_StaysWithheld()
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var type = await SeedTypeAsync(db, "ward_round_review", BuilderSchema, BuilderWorkflow, BuilderCredit);
        var versionOne = await db.ActivityTypeVersions.SingleAsync(version => version.ActivityTypeId == type.Id);
        versionOne.SchemaJson = "{ not a schema";
        await db.SaveChangesAsync();

        AddActivity(db, type, "signed_off", Builder("sup-a"), OtherInstitution);
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Principal("member-1", WombatRoles.CommitteeMember, HostInstitution));

        report.WithheldRatedActivities.Should().Be(1);
        report.UnattributedRatedActivities.Should().Be(0);
        report.EvidenceComplete.Should().BeFalse();
    }

    /// <summary>
    /// The validator skips a hidden field, required or not, so a required rating that can be hidden (by its own
    /// <c>show_if</c> or its section's) may be empty on a finished row.
    /// </summary>
    [Theory]
    [InlineData(SectionHiddenRatingSchema)]
    [InlineData(FieldHiddenRatingSchema)]
    public async Task ARequiredRatingThatCanBeHidden_LeftEmpty_IsUnattributed(string schemaJson)
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var type = await SeedTypeAsync(db, "conditional_review", schemaJson, BuilderWorkflow, BuilderCredit);

        AddActivity(db, type, "signed_off", """{ "target_epa": 7, "supervisor": "sup-a", "observed": "yes", "entrustment": 3 }""");
        AddActivity(db, type, "signed_off", """{ "target_epa": 7, "supervisor": "sup-b", "observed": "no" }""");
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Administrator());

        report.TotalRatedActivities.Should().Be(1);
        report.UnattributedRatedActivities.Should().Be(1);
        report.UnreadableRatedActivities.Should().Be(0);
    }

    // ---- Unattributed: its own count, and the report stays complete ---------------------------------------------

    /// <summary>
    /// An MSF asserts a level but names no observing assessor (D36): its rated field is written by
    /// <c>role:Coordinator|role:Administrator</c> and the schema has no nominee field. Nothing was withheld and nothing
    /// is broken, so it must not make the report incomplete — and it is not an assessor's rating to count. That holds
    /// whatever the row holds, since no reading of it could enter the figures.
    /// </summary>
    [Fact]
    public async Task AReleasedMsfRecord_IsUnattributedAndLeavesTheReportComplete()
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var miniCex = await SeedTypeFromSeedFolderAsync(db, "mini_cex_cpsa");
        var msf = await SeedTypeFromSeedFolderAsync(db, "msf_cpsa");

        AddActivity(db, miniCex, "completed", Rated("assessor-a"));
        AddActivity(db, msf, "recorded", Msf(overallLevel: 4));
        AddActivity(db, msf, "recorded", Msf(overallLevel: null));
        AddActivity(db, msf, "recorded", """{ "campaign_id": 1, "overall_level": "garbled" }""");
        AddActivity(db, msf, "draft", Msf(overallLevel: 5));
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Administrator());

        report.UnattributedRatedActivities.Should().Be(3, "every released record; the draft is not evidence");
        report.EvidenceComplete.Should().BeTrue();
        report.UnreadableRatedActivities.Should().Be(0, "a garbled MSF could not have entered the figures either");
        report.TotalRatedActivities.Should().Be(1);
        report.DistinctAssessorCount.Should().Be(1);
    }

    /// <summary>
    /// The T135 review's transferred trainee: an MSF released at the old institution, every WBA at the new one. A
    /// panel member at the new institution may not read the MSF, but its version names no assessor, so the figures are
    /// the same with it. Calling it withheld would put up a banner saying they are not.
    /// </summary>
    [Fact]
    public async Task AWithheldMsfRecord_IsUnattributedNotWithheld()
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var miniCex = await SeedTypeFromSeedFolderAsync(db, "mini_cex_cpsa");
        var msf = await SeedTypeFromSeedFolderAsync(db, "msf_cpsa");

        AddActivity(db, miniCex, "completed", Rated("assessor-a"));
        AddActivity(db, msf, "recorded", Msf(overallLevel: 4), OtherInstitution);
        AddActivity(db, msf, "recorded", Msf(overallLevel: 3), OtherInstitution);
        AddActivity(db, miniCex, "completed", Rated("assessor-b"), OtherInstitution);
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Principal("member-1", WombatRoles.CommitteeMember, HostInstitution));

        report.WithheldRatedActivities.Should().Be(1, "only the mini-CEX could have entered the figures");
        report.UnattributedRatedActivities.Should().Be(2);
        report.TotalRatedActivities.Should().Be(1);

        db.Activities.RemoveRange(db.Activities.Where(activity => activity.ActivityTypeId == miniCex.Id && activity.InstitutionId == OtherInstitution));
        await db.SaveChangesAsync();
        var msfOnly = await ReportAsync(db, Principal("member-1", WombatRoles.CommitteeMember, HostInstitution));

        msfOnly.WithheldRatedActivities.Should().Be(0);
        msfOnly.EvidenceComplete.Should().BeTrue("nothing the caller cannot read could have changed a figure");
    }

    /// <summary>
    /// On a form whose rating and assessor are optional, leaving either empty is allowed: the row has no named
    /// assessor's rating in it, and nothing is broken.
    /// </summary>
    [Theory]
    [InlineData("""{ "target_epa": 7, "supervisor": "sup-z" }""")]
    [InlineData("""{ "target_epa": 7, "supervisor": "sup-z", "entrustment": null }""")]
    [InlineData("""{ "target_epa": 7, "supervisor": "sup-z", "entrustment": " " }""")]
    [InlineData("""{ "target_epa": 7, "entrustment": 3 }""")]
    [InlineData("""{ "target_epa": 7, "supervisor": "", "entrustment": 3 }""")]
    public async Task AnOptionalRatingOrAssessorLeftEmpty_IsUnattributed(string dataJson)
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var type = await SeedTypeAsync(db, "optional_review", OptionalSchema, BuilderWorkflow, BuilderCredit);

        AddActivity(db, type, "signed_off", Builder("sup-a"));
        AddActivity(db, type, "signed_off", dataJson);
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Administrator());

        report.UnattributedRatedActivities.Should().Be(1);
        report.UnreadableRatedActivities.Should().Be(0);
        report.TotalRatedActivities.Should().Be(1);
        report.DistinctAssessorCount.Should().Be(1);
        report.EvidenceComplete.Should().BeTrue();
    }

    /// <summary>
    /// An optional form excuses an EMPTY value only. No EPA (the first two rows name none, so none is stamped) is still a
    /// rating of nothing, whatever else is missing, and an assessor field holding something that is not an id is
    /// malformed.
    /// </summary>
    [Theory]
    [InlineData("""{ "supervisor": "sup-z" }""")]
    [InlineData("""{ "supervisor": "sup-z", "entrustment": 3 }""")]
    [InlineData("""{ "target_epa": 7, "supervisor": 12, "entrustment": 3 }""")]
    public async Task OnAnOptionalForm_NoEpaOrAMalformedAssessor_IsStillUnreadable(string dataJson)
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var type = await SeedTypeAsync(db, "optional_review", OptionalSchema, BuilderWorkflow, BuilderCredit);

        AddActivity(db, type, "signed_off", Builder("sup-a"));
        AddActivity(db, type, "signed_off", dataJson);
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Administrator());

        report.UnreadableRatedActivities.Should().Be(1);
        report.UnattributedRatedActivities.Should().Be(0);
        report.EvidenceComplete.Should().BeFalse();
    }

    // ---- The declared fields, not literal keys (T150) -----------------------------------------------------------

    /// <summary>
    /// A builder type whose EPA, assessor and rating live under other keys, finishing in a state called something
    /// other than "completed". Each row also carries a decoy <c>assessor_user_id</c> the report must not read.
    /// </summary>
    [Fact]
    public async Task ATypeWhoseFieldsHaveOtherKeys_IsCountedByWhatItDeclares()
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var wardRound = await SeedTypeAsync(db, "ward_round_review", BuilderSchema, BuilderWorkflow, BuilderCredit);

        AddActivity(db, wardRound, "signed_off", Builder("sup-a"));
        AddActivity(db, wardRound, "signed_off", Builder("sup-b"));
        AddActivity(db, wardRound, "signed_off", Builder("sup-c"));
        AddActivity(db, wardRound, "draft", Builder("sup-d"));
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Administrator());

        report.TotalRatedActivities.Should().Be(3);
        report.DistinctAssessorCount.Should().Be(3, "the supervisor field, not the decoy literal");
        report.UnreadableRatedActivities.Should().Be(0);
        report.EvidenceComplete.Should().BeTrue();
        var epa = report.PerEpa.Should().ContainSingle().Subject;
        epa.EpaId.Should().Be(EpaId, "stamped through the evidence_epa_field it declares (T137)");
        epa.FewerThanThreeAssessors.Should().BeFalse();
    }

    /// <summary>
    /// A rated type that credits nothing still says which EPA its rows are about, through its <c>evidence_epa_field</c>
    /// (T137), and they are counted under the EPA stamped from it. Before the stamp this pinned the reader's fallback to
    /// the schema's <c>epa</c> field when the credit rules named none.
    /// </summary>
    [Fact]
    public async Task ATypeThatCreditsNothing_IsCountedUnderItsStampedEpa()
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var type = await SeedTypeAsync(db, "ungraded_review", BuilderSchema, BuilderWorkflow, CreditsNothing);

        AddActivity(db, type, "signed_off", Builder("sup-a"));
        AddActivity(db, type, "signed_off", Builder("sup-b"));
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Administrator());

        report.TotalRatedActivities.Should().Be(2);
        report.UnreadableRatedActivities.Should().Be(0);
        report.PerEpa.Should().ContainSingle().Which.EpaId.Should().Be(EpaId);
    }

    // ---- The EPA is the one stamped on the activity (T137) -------------------------------------------------------

    /// <summary>
    /// A row is counted under the EPA stamped on it, never under one read out of its data. In the product the two cannot
    /// disagree, since the stamp is re-read from the data on every write, so this is the test that shows which one the
    /// report consults: two rows carry the same data, one is stamped with another EPA, and it is counted there.
    /// </summary>
    [Fact]
    public async Task TheEpaIsTheOneStampedOnTheRow_NotOneReadFromItsData()
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        db.Epas.Add(new Epa { Id = 3, SubSpecialityId = 1, Code = "EPA-03", Title = "Ward round", IsActive = true });
        await db.SaveChangesAsync();
        var wardRound = await SeedTypeAsync(db, "ward_round_review", BuilderSchema, BuilderWorkflow, BuilderCredit);

        AddActivity(db, wardRound, "signed_off", Builder("sup-a"));
        AddActivity(db, wardRound, "signed_off", Builder("sup-b")).EpaId = 3;
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Administrator());

        report.TotalRatedActivities.Should().Be(2);
        report.PerEpa.Select(epa => epa.EpaId).Should().Equal(3, EpaId);
        report.PerEpa.Should().OnlyContain(epa => epa.RatingCount == 1);
    }

    /// <summary>
    /// A rated type about no single EPA: its form declares no <c>evidence_epa_field</c> and it credits a fixed curriculum
    /// item, a shape T137 accepts. Its rows are stamped with no EPA, so each is unreadable however plainly its data holds
    /// one: an EPA the form does not declare is not one the report may guess. Before the stamp, the reader fell back to
    /// the schema's <c>epa</c> field and counted this row.
    /// </summary>
    [Fact]
    public async Task ARowStampedWithNoEpa_IsUnreadable_WhateverItsDataHolds()
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var type = await SeedTypeAsync(db, "item_review", NoEvidenceEpaSchema, BuilderWorkflow, CreditsAFixedItem);

        var row = AddActivity(db, type, "signed_off", Builder("sup-a"));
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Administrator());

        row.EpaId.Should().BeNull("the pinned schema names no EPA field for the stamp to read");
        report.UnreadableRatedActivities.Should().Be(1);
        report.TotalRatedActivities.Should().Be(0);
        report.EvidenceComplete.Should().BeFalse();
    }

    /// <summary>
    /// T150's rule, option (b): the assessor is whoever may WRITE the rating — the field the rated field's
    /// <c>editable_by</c> names — not merely the first person the form mentions.
    /// </summary>
    [Fact]
    public async Task TheAssessorIsTheFieldThatMayWriteTheRating()
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var type = await SeedTypeAsync(db, "sign_off_review", TwoPeopleSchema, BuilderWorkflow, BuilderCredit);

        AddActivity(db, type, "signed_off", """{ "target_epa": 7, "consultant": "consultant-a", "supervisor": "sup-a", "entrustment": 3 }""");
        AddActivity(db, type, "signed_off", """{ "target_epa": 7, "consultant": "consultant-a", "supervisor": "sup-b", "entrustment": 3 }""");
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Administrator());

        report.DistinctAssessorCount.Should().Be(2, "the supervisor rated each; the consultant, named first, rated neither");
        report.PerEpa.Should().ContainSingle().Which.OneAssessorOverHalf.Should().BeFalse();
    }

    /// <summary>
    /// The field's own <c>editable_by</c> beats its section's, as <c>FieldPermissionEvaluator</c> resolves it: the
    /// section says the consultant writes, the rating says the supervisor does.
    /// </summary>
    [Fact]
    public async Task TheRatedFieldsOwnRule_BeatsItsSections()
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var type = await SeedTypeAsync(db, "sign_off_review", FieldRuleOverridesSectionSchema, RoleFinishedWorkflow, BuilderCredit);

        AddActivity(db, type, "signed_off", """{ "target_epa": 7, "consultant": "consultant-a", "supervisor": "sup-a", "entrustment": 3 }""");
        AddActivity(db, type, "signed_off", """{ "target_epa": 7, "consultant": "consultant-a", "supervisor": "sup-b", "entrustment": 3 }""");
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Administrator());

        report.DistinctAssessorCount.Should().Be(2, "the rating's own rule names the supervisor");
    }

    /// <summary>
    /// When the rule on the rating names nobody (here <c>role:Assessor</c>), the person who finishes the record — the
    /// <c>field:</c> actor of the move into the terminal state — is the assessor, before every nominee field is.
    /// </summary>
    [Fact]
    public async Task WhenTheRatingsRuleNamesNobody_WhoeverFinishesTheRecordIsTheAssessor()
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var type = await SeedTypeAsync(db, "sign_off_review", TwoPeopleRoleRatedSchema, BuilderWorkflow, BuilderCredit);

        AddActivity(db, type, "signed_off", """{ "target_epa": 7, "consultant": "consultant-a", "supervisor": "sup-a", "entrustment": 3 }""");
        AddActivity(db, type, "signed_off", """{ "target_epa": 7, "consultant": "consultant-a", "supervisor": "sup-b", "entrustment": 3 }""");
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Administrator());

        report.DistinctAssessorCount.Should().Be(2, "the supervisor signs off; the consultant is only named first");
    }

    /// <summary>
    /// When the rating's rule names nobody and nobody named finishes the record, but a role writes the rating, every
    /// nominee field is the assessor.
    /// </summary>
    [Fact]
    public async Task ARoleRuleOnTheRatedField_FallsBackToTheNomineeFields()
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var type = await SeedTypeAsync(db, "observed_review", RoleRatedSchema, RoleFinishedWorkflow, BuilderCredit);

        AddActivity(db, type, "signed_off", """{ "target_epa": 7, "observer": "obs-a", "entrustment": 3 }""");
        AddActivity(db, type, "signed_off", """{ "target_epa": 7, "observer": "obs-b", "entrustment": 3 }""");
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Administrator());

        report.TotalRatedActivities.Should().Be(2);
        report.DistinctAssessorCount.Should().Be(2);
    }

    /// <summary>
    /// T150's padding again, by another route: a type whose rating the trainee writes (the <c>subject|creator</c>
    /// default) and finishes, with a supervisor field on the form. Naming three supervisors must not clear the
    /// fewer-than-three warning, because none of them rated anything. The version names no assessor.
    /// </summary>
    [Fact]
    public async Task ARatingTheTraineeWritesAndFinishes_NamesNoAssessor()
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var type = await SeedTypeAsync(db, "self_rated_review", SelfRatedSchema, SelfFinishedWorkflow, BuilderCredit);

        AddActivity(db, type, "done", Builder("sup-a"));
        AddActivity(db, type, "done", Builder("sup-b"));
        AddActivity(db, type, "done", Builder("sup-c"));
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Administrator());

        report.TotalRatedActivities.Should().Be(0);
        report.DistinctAssessorCount.Should().Be(0);
        report.UnattributedRatedActivities.Should().Be(3);
        report.EvidenceComplete.Should().BeTrue();
    }

    [Fact]
    public async Task AssessorIdsAreComparedExactly()
    {
        // As the actor grammar and T102's gate compare them. The old reader trimmed, so " assessor-a" and
        // "assessor-a" were one person here and two everywhere else.
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var miniCex = await SeedTypeFromSeedFolderAsync(db, "mini_cex_cpsa");

        AddActivity(db, miniCex, "completed", Rated("assessor-a"));
        AddActivity(db, miniCex, "completed", Rated(" assessor-a"));
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, Administrator());

        report.DistinctAssessorCount.Should().Be(2);
    }

    // ---- The partition, asserted directly -----------------------------------------------------------------------

    /// <summary>
    /// Total + Withheld + Unreadable + Unattributed is exactly the rated rows in the window in a terminal state of
    /// their pinned workflow — the invariant both of T135's defects broke. Each seed generates a different mix of
    /// states, institutions, dates, types and data shapes, and the expected figure is computed from the fixture's own
    /// description of each row, not from the report.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    public async Task EveryEvidenceRowIsInExactlyOneCount(int seed)
    {
        await using var db = CreateDb();
        await SeedReviewAsync(db);
        var miniCex = await SeedTypeFromSeedFolderAsync(db, "mini_cex_cpsa");
        var msf = await SeedTypeFromSeedFolderAsync(db, "msf_cpsa");
        var builder = await SeedTypeAsync(db, "ward_round_review", BuilderSchema, BuilderWorkflow, BuilderCredit);
        var optional = await SeedTypeAsync(db, "optional_review", OptionalSchema, BuilderWorkflow, BuilderCredit);
        var reflective = await SeedTypeFromSeedFolderAsync(db, "reflective_exercise_cpsa");

        var random = new Random(seed);
        var expected = new ExpectedCounts();
        var rows = 20 + random.Next(20);
        for (var index = 0; index < rows; index++)
        {
            var institution = random.Next(3) == 0 ? OtherInstitution : HostInstitution;
            var inWindow = random.Next(6) != 0;
            var observedOn = inWindow ? InWindow : new DateTime(2025, 11, 1, 9, 0, 0, DateTimeKind.Utc);
            var withheld = institution == OtherInstitution;

            switch (random.Next(5))
            {
                case 0:
                {
                    string[] states = ["draft", "requested", "completed", "declined", "cancelled"];
                    var state = states[random.Next(states.Length)];
                    var (data, outcome) = MiniCexData(random);
                    AddActivity(db, miniCex, state, data, institution, observedOn);
                    expected.Add(inWindow && state == "completed", withheld, namesNoAssessor: false, outcome);
                    break;
                }

                case 1:
                {
                    var state = random.Next(3) == 0 ? "draft" : "recorded";
                    var data = random.Next(4) switch
                    {
                        0 => """{ "campaign_id": 1 }""",
                        1 => Msf(null),
                        _ => Msf(4)
                    };
                    AddActivity(db, msf, state, data, institution, observedOn);
                    expected.Add(inWindow && state == "recorded", withheld, namesNoAssessor: true, Outcome.Unattributed);
                    break;
                }

                case 2:
                {
                    var state = random.Next(3) == 0 ? "draft" : "signed_off";
                    var readable = random.Next(4) != 0;
                    AddActivity(
                        db,
                        builder,
                        state,
                        readable ? Builder($"sup-{random.Next(5)}") : """{ "target_epa": 7, "entrustment": 3 }""",
                        institution,
                        observedOn);
                    expected.Add(inWindow && state == "signed_off", withheld, namesNoAssessor: false, readable ? Outcome.Attributed : Outcome.Unreadable);
                    break;
                }

                case 3:
                {
                    var state = random.Next(3) == 0 ? "draft" : "signed_off";
                    var (data, outcome) = random.Next(4) switch
                    {
                        0 => ("""{ "target_epa": 7, "supervisor": "sup-z" }""", Outcome.Unattributed),
                        1 => ("""{ "target_epa": 7, "entrustment": 2 }""", Outcome.Unattributed),
                        2 => ("""{ "supervisor": "sup-z", "entrustment": 2 }""", Outcome.Unreadable),
                        _ => (Builder($"sup-{random.Next(5)}"), Outcome.Attributed)
                    };
                    AddActivity(db, optional, state, data, institution, observedOn);
                    expected.Add(inWindow && state == "signed_off", withheld, namesNoAssessor: false, outcome);
                    break;
                }

                default:
                {
                    // Unrated: never in any count, in any state.
                    AddActivity(db, reflective, random.Next(2) == 0 ? "draft" : "discussed", Rated("assessor-a"), institution, observedOn);
                    break;
                }
            }
        }

        await db.SaveChangesAsync();
        expected.Evidence.Should().BePositive("a seed that generated no evidence would prove nothing");

        var report = await ReportAsync(db, Principal("member-1", WombatRoles.CommitteeMember, HostInstitution));

        (report.TotalRatedActivities + report.WithheldRatedActivities + report.UnreadableRatedActivities + report.UnattributedRatedActivities)
            .Should().Be(expected.Evidence, "every evidence row is in exactly one count");
        report.TotalRatedActivities.Should().Be(expected.Attributed);
        report.WithheldRatedActivities.Should().Be(expected.Withheld);
        report.UnreadableRatedActivities.Should().Be(expected.Unreadable);
        report.UnattributedRatedActivities.Should().Be(expected.Unattributed);
        report.EvidenceComplete.Should().Be(expected.Withheld == 0 && expected.Unreadable == 0);
    }

    private enum Outcome
    {
        Attributed,
        Unattributed,
        Unreadable
    }

    private sealed class ExpectedCounts
    {
        public int Evidence { get; private set; }
        public int Attributed { get; private set; }
        public int Withheld { get; private set; }
        public int Unreadable { get; private set; }
        public int Unattributed { get; private set; }

        /// <param name="isEvidence">In the window and in a terminal state of its pinned workflow.</param>
        /// <param name="withheld">Outside what the caller may read.</param>
        /// <param name="namesNoAssessor">Its version names nobody who writes the rating (MSF): unattributed whether
        /// read or not, because no reading of it could enter the figures.</param>
        /// <param name="outcome">What the row reads as, when the caller may read it.</param>
        public void Add(bool isEvidence, bool withheld, bool namesNoAssessor, Outcome outcome)
        {
            if (!isEvidence)
            {
                return;
            }

            Evidence++;
            if (withheld && !namesNoAssessor)
            {
                Withheld++;
                return;
            }

            switch (outcome)
            {
                case Outcome.Attributed:
                    Attributed++;
                    break;
                case Outcome.Unattributed:
                    Unattributed++;
                    break;
                default:
                    Unreadable++;
                    break;
            }
        }
    }

    private static (string Data, Outcome Outcome) MiniCexData(Random random)
        => random.Next(4) switch
        {
            // The rating is required and `complete` checks it, so an empty one is broken data.
            0 => ("""{ "epa_id": 7, "assessor_user_id": "assessor-z" }""", Outcome.Unreadable),
            1 => ("""{ "epa_id": 7, "overall_level": 3 }""", Outcome.Unreadable),
            _ => (Rated($"assessor-{random.Next(5)}"), Outcome.Attributed)
        };

    // ---- Fixture -------------------------------------------------------------------------------------------------

    private static string Rated(string assessor)
        => $$"""{ "epa_id": 7, "assessor_user_id": "{{assessor}}", "overall_level": 3 }""";

    private static string Named(string assessor)
        => $$"""{ "epa_id": 7, "assessor_user_id": "{{assessor}}" }""";

    private static string Builder(string supervisor)
        => $$"""{ "target_epa": 7, "supervisor": "{{supervisor}}", "assessor_user_id": "decoy", "entrustment": 4 }""";

    /// <summary>A v1 row: the observer wrote the rating, and it also carries v2's assessor key as a decoy.</summary>
    private static string VersionOneRow(string observer)
        => $$"""{ "epa_id": 7, "observer": "{{observer}}", "assessor_user_id": "decoy-v1", "overall_level": 3 }""";

    /// <summary>The shape <c>ReleaseMsfCampaign</c> writes.</summary>
    private static string Msf(int? overallLevel)
        => overallLevel is null
            ? """{ "epa_id": 7, "campaign_id": 1, "observed_on": "2026-02-10", "respondent_count": 8 }"""
            : $$"""{ "epa_id": 7, "campaign_id": 1, "observed_on": "2026-02-10", "respondent_count": 8, "overall_level": {{overallLevel.Value.ToString(CultureInfo.InvariantCulture)}} }""";

    /// <summary>
    /// A builder type as T105 would have it: its assessor and rating required, checked by the sign-off. Every builder
    /// schema here names its EPA field in <c>evidence_epa_field</c>, as T137 requires of a form whose credit reads that
    /// field and as its migration gave every stored version whose EPA field was determined; without it no row of the
    /// type is stamped with an EPA.
    /// </summary>
    private const string BuilderSchema = """
        {
          "version": 1,
          "rated_level_field": "entrustment",
          "evidence_epa_field": "target_epa",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "target_epa", "type": "epa", "label": "EPA", "required": true },
                { "key": "supervisor", "type": "user", "label": "Supervisor", "required": true }
              ]
            },
            {
              "key": "judgement",
              "title": "Judgement",
              "editable_by": "field:supervisor",
              "fields": [
                { "key": "entrustment", "type": "scale", "label": "Entrustment", "required": true, "scale_key": "CPSA Paediatric Entrustment Scale v11.1" }
              ]
            }
          ]
        }
        """;

    /// <summary>The same shape with the assessor and the rating optional.</summary>
    private const string OptionalSchema = """
        {
          "version": 1,
          "rated_level_field": "entrustment",
          "evidence_epa_field": "target_epa",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "target_epa", "type": "epa", "label": "EPA" },
                { "key": "supervisor", "type": "user", "label": "Supervisor" }
              ]
            },
            {
              "key": "judgement",
              "title": "Judgement",
              "editable_by": "field:supervisor",
              "fields": [
                { "key": "entrustment", "type": "scale", "label": "Entrustment", "scale_key": "CPSA Paediatric Entrustment Scale v11.1" }
              ]
            }
          ]
        }
        """;

    private const string SectionHiddenRatingSchema = """
        {
          "version": 1,
          "rated_level_field": "entrustment",
          "evidence_epa_field": "target_epa",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "target_epa", "type": "epa", "label": "EPA", "required": true },
                { "key": "supervisor", "type": "user", "label": "Supervisor", "required": true },
                { "key": "observed", "type": "choice", "label": "Observed?", "options": ["yes", "no"] }
              ]
            },
            {
              "key": "judgement",
              "title": "Judgement",
              "editable_by": "field:supervisor",
              "show_if": { "field": "observed", "operator": "equals", "value": "yes" },
              "fields": [
                { "key": "entrustment", "type": "scale", "label": "Entrustment", "required": true, "scale_key": "CPSA Paediatric Entrustment Scale v11.1" }
              ]
            }
          ]
        }
        """;

    private const string FieldHiddenRatingSchema = """
        {
          "version": 1,
          "rated_level_field": "entrustment",
          "evidence_epa_field": "target_epa",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "target_epa", "type": "epa", "label": "EPA", "required": true },
                { "key": "supervisor", "type": "user", "label": "Supervisor", "required": true },
                { "key": "observed", "type": "choice", "label": "Observed?", "options": ["yes", "no"] }
              ]
            },
            {
              "key": "judgement",
              "title": "Judgement",
              "editable_by": "field:supervisor",
              "fields": [
                { "key": "entrustment", "type": "scale", "label": "Entrustment", "required": true, "scale_key": "CPSA Paediatric Entrustment Scale v11.1",
                  "show_if": { "field": "observed", "operator": "equals", "value": "yes" } }
              ]
            }
          ]
        }
        """;

    /// <summary>What <see cref="AVersionMissingTheTypesRatedField_IsUnreadable" />'s v1 looked like: no rated field
    /// declared, and no <c>entrustment</c> field at all.</summary>
    private const string PreRatedFieldSchema = """
        {
          "version": 1,
          "evidence_epa_field": "target_epa",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "target_epa", "type": "epa", "label": "EPA" },
                { "key": "supervisor", "type": "user", "label": "Supervisor" }
              ]
            },
            {
              "key": "judgement",
              "title": "Judgement",
              "editable_by": "field:supervisor",
              "fields": [
                { "key": "overall", "type": "scale", "label": "Overall", "scale_key": "CPSA Paediatric Entrustment Scale v11.1" }
              ]
            }
          ]
        }
        """;

    private const string TwoPeopleSchema = """
        {
          "version": 1,
          "rated_level_field": "entrustment",
          "evidence_epa_field": "target_epa",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "target_epa", "type": "epa", "label": "EPA" },
                { "key": "consultant", "type": "user", "label": "Consultant on call" },
                { "key": "supervisor", "type": "user", "label": "Supervisor" }
              ]
            },
            {
              "key": "judgement",
              "title": "Judgement",
              "editable_by": "field:supervisor",
              "fields": [
                { "key": "entrustment", "type": "scale", "label": "Entrustment", "scale_key": "CPSA Paediatric Entrustment Scale v11.1" }
              ]
            }
          ]
        }
        """;

    private const string FieldRuleOverridesSectionSchema = """
        {
          "version": 1,
          "rated_level_field": "entrustment",
          "evidence_epa_field": "target_epa",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "target_epa", "type": "epa", "label": "EPA" },
                { "key": "consultant", "type": "user", "label": "Consultant on call" },
                { "key": "supervisor", "type": "user", "label": "Supervisor" }
              ]
            },
            {
              "key": "judgement",
              "title": "Judgement",
              "editable_by": "field:consultant",
              "fields": [
                { "key": "entrustment", "type": "scale", "label": "Entrustment", "scale_key": "CPSA Paediatric Entrustment Scale v11.1", "editable_by": "field:supervisor" }
              ]
            }
          ]
        }
        """;

    private const string TwoPeopleRoleRatedSchema = """
        {
          "version": 1,
          "rated_level_field": "entrustment",
          "evidence_epa_field": "target_epa",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "target_epa", "type": "epa", "label": "EPA" },
                { "key": "consultant", "type": "user", "label": "Consultant on call" },
                { "key": "supervisor", "type": "user", "label": "Supervisor" }
              ]
            },
            {
              "key": "judgement",
              "title": "Judgement",
              "editable_by": "role:Assessor",
              "fields": [
                { "key": "entrustment", "type": "scale", "label": "Entrustment", "scale_key": "CPSA Paediatric Entrustment Scale v11.1" }
              ]
            }
          ]
        }
        """;

    private const string RoleRatedSchema = """
        {
          "version": 1,
          "rated_level_field": "entrustment",
          "evidence_epa_field": "target_epa",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "target_epa", "type": "epa", "label": "EPA" },
                { "key": "observer", "type": "user", "label": "Observer" }
              ]
            },
            {
              "key": "judgement",
              "title": "Judgement",
              "editable_by": "role:Assessor",
              "fields": [
                { "key": "entrustment", "type": "scale", "label": "Entrustment", "scale_key": "CPSA Paediatric Entrustment Scale v11.1" }
              ]
            }
          ]
        }
        """;

    /// <summary>No <c>editable_by</c> anywhere: the trainee writes the rating (the <c>subject|creator</c> default).</summary>
    private const string SelfRatedSchema = """
        {
          "version": 1,
          "rated_level_field": "entrustment",
          "evidence_epa_field": "target_epa",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "target_epa", "type": "epa", "label": "EPA" },
                { "key": "supervisor", "type": "user", "label": "Supervisor" },
                { "key": "entrustment", "type": "scale", "label": "Entrustment", "scale_key": "CPSA Paediatric Entrustment Scale v11.1" }
              ]
            }
          ]
        }
        """;

    private const string BuilderWorkflow = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "signed_off", "label": "Signed off", "terminal": true }
          ],
          "transitions": [
            { "key": "sign_off", "from": "draft", "to": "signed_off", "actor": "field:supervisor", "validation": "all" }
          ]
        }
        """;

    /// <summary>Signed off by a role, so the move into the terminal state names nobody.</summary>
    private const string RoleFinishedWorkflow = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "signed_off", "label": "Signed off", "terminal": true }
          ],
          "transitions": [
            { "key": "sign_off", "from": "draft", "to": "signed_off", "actor": "role:Assessor", "validation": "all" }
          ]
        }
        """;

    private const string SelfFinishedWorkflow = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "done", "label": "Done", "terminal": true }
          ],
          "transitions": [
            { "key": "finish", "from": "draft", "to": "done", "actor": "subject|creator", "validation": "all" }
          ]
        }
        """;

    /// <summary>Checks formats only, but names the rating in <c>requires_fields</c>.</summary>
    private const string RequiresRatingWorkflow = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "signed_off", "label": "Signed off", "terminal": true }
          ],
          "transitions": [
            { "key": "sign_off", "from": "draft", "to": "signed_off", "actor": "field:supervisor", "validation": "draft", "requires_fields": ["entrustment"] }
          ]
        }
        """;

    /// <summary>Born in its terminal state; reopened and refiled with every required field checked.</summary>
    private const string BornTerminalWorkflow = """
        {
          "version": 1,
          "initial_state": "logged",
          "states": [
            { "key": "logged", "label": "Logged", "terminal": true },
            { "key": "editing", "label": "Editing" }
          ],
          "transitions": [
            { "key": "reopen", "from": "logged", "to": "editing", "actor": "subject|creator", "validation": "draft" },
            { "key": "refile", "from": "editing", "to": "logged", "actor": "field:supervisor", "validation": "all" }
          ]
        }
        """;

    private const string BuilderCredit = """
        {
          "counts_for": [
            { "curriculum_item_match": { "epa_field": "target_epa" }, "amount": 1, "minimum_level_field": "entrustment" }
          ]
        }
        """;

    private const string CreditsNothing = """
        {
          "counts_for": []
        }
        """;

    /// <summary><see cref="BuilderSchema" /> about no single EPA: the same <c>epa</c> field, and no pointer at it.</summary>
    private const string NoEvidenceEpaSchema = """
        {
          "version": 1,
          "rated_level_field": "entrustment",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "target_epa", "type": "epa", "label": "EPA", "required": true },
                { "key": "supervisor", "type": "user", "label": "Supervisor", "required": true }
              ]
            },
            {
              "key": "judgement",
              "title": "Judgement",
              "editable_by": "field:supervisor",
              "fields": [
                { "key": "entrustment", "type": "scale", "label": "Entrustment", "required": true, "scale_key": "CPSA Paediatric Entrustment Scale v11.1" }
              ]
            }
          ]
        }
        """;

    /// <summary>Credits a fixed curriculum item, which T137 allows only of a form that names no EPA field.</summary>
    private const string CreditsAFixedItem = """
        {
          "counts_for": [
            { "curriculum_item_match": { "curriculum_item_id": 555 }, "amount": 1, "minimum_level_field": "entrustment" }
          ]
        }
        """;

    /// <summary>
    /// <c>mini_cex_cpsa</c>'s v1 as it might have been: an <c>observer</c> field writes the rating, and <c>declined</c>
    /// is terminal, as the generic seeds before <c>c33c14b</c> had it.
    /// </summary>
    private const string VersionOneSchema = """
        {
          "version": 1,
          "rated_level_field": "overall_level",
          "evidence_epa_field": "epa_id",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "epa_id", "type": "epa", "label": "EPA", "required": true },
                { "key": "observer", "type": "user", "label": "Observer", "required": true },
                { "key": "assessor_user_id", "type": "user", "label": "Assessor" }
              ]
            },
            {
              "key": "assessment",
              "title": "Assessment",
              "editable_by": "field:observer",
              "fields": [
                { "key": "overall_level", "type": "scale", "label": "Overall", "required": true, "scale_key": "CPSA Paediatric Entrustment Scale v11.1" }
              ]
            }
          ]
        }
        """;

    private const string VersionOneWorkflow = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested", "editable_by": "field:observer" },
            { "key": "completed", "label": "Completed", "terminal": true },
            { "key": "declined", "label": "Declined", "terminal": true }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator", "validation": "owned" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:observer", "validation": "all" },
            { "key": "decline", "from": "requested", "to": "declined", "actor": "field:observer", "validation": "draft" }
          ]
        }
        """;

    private static async Task<SamplingConcentrationReportDto> ReportAsync(ApplicationDbContext db, ClaimsPrincipal principal)
        => await new GetSamplingConcentrationWarningsQueryHandler(db, FakeUserDirectory.PanelMembersOf(db))
            .Handle(new GetSamplingConcentrationWarningsQuery(ReviewId, principal), CancellationToken.None);

    private static ClaimsPrincipal Administrator() => Principal("admin-1", WombatRoles.Administrator);

    private static ClaimsPrincipal Principal(string userId, string role, int? institutionId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, role)
        };

        if (institutionId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, institutionId.Value.ToString(CultureInfo.InvariantCulture)));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static async Task SeedReviewAsync(ApplicationDbContext db)
    {
        db.Epas.Add(new Epa { Id = EpaId, SubSpecialityId = 1, Code = "EPA-07", Title = "Emergency triage", IsActive = true });
        db.DecisionPanels.Add(new DecisionPanel
        {
            Id = 1,
            Name = "Paediatrics CCC",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = HostInstitution,
            CreatedOn = DateTime.UtcNow,
            Members =
            [
                new DecisionPanelMember { UserId = "member-1", Role = DecisionPanelMemberRole.Member }
            ]
        });
        db.CommitteeReviews.Add(new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 1,
            Id = ReviewId,
            PanelId = 1,
            TraineeUserId = "trainee-1",
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 3, 31),
            ScheduledOn = new DateOnly(2026, 4, 1)
        });
        await db.SaveChangesAsync();
    }

    private static Task<ActivityType> SeedTypeFromSeedFolderAsync(ApplicationDbContext db, string key)
        => SeedTypeAsync(db, key, ReadSeedFile(key, "schema.json"), ReadSeedFile(key, "workflow.json"), ReadSeedFile(key, "credit.json"));

    private static string ReadSeedFile(string key, string fileName)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", key, fileName));

    /// <summary>
    /// <c>mini_cex_cpsa</c> as shipped at v2, the type's current columns, with a v1 that differs in its terminal states
    /// and in who writes the rating.
    /// </summary>
    private static async Task<ActivityType> SeedTwoVersionTypeAsync(ApplicationDbContext db)
    {
        var schema = ReadSeedFile("mini_cex_cpsa", "schema.json");
        var workflow = ReadSeedFile("mini_cex_cpsa", "workflow.json");
        var credit = ReadSeedFile("mini_cex_cpsa", "credit.json");

        var type = await SeedTypeAsync(db, "mini_cex_cpsa", VersionOneSchema, VersionOneWorkflow, credit);
        type.Version = 2;
        type.SchemaJson = schema;
        type.WorkflowJson = workflow;
        type.Versions.Add(new ActivityTypeVersion
        {
            Version = 2,
            SchemaJson = schema,
            WorkflowJson = workflow,
            CreditRulesJson = credit,
            PublishedByUserId = "seed-system",
            PublishedOn = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return type;
    }

    /// <summary>Published at v1 with its version row, as <c>ActivityService</c> would pin an activity to it.</summary>
    private static async Task<ActivityType> SeedTypeAsync(
        ApplicationDbContext db,
        string key,
        string schemaJson,
        string workflowJson,
        string creditRulesJson)
    {
        var type = new ActivityType
        {
            Key = key,
            Name = key,
            Version = 1,
            IsActive = true,
            OwnerUserId = "seed-system",
            CreatedOn = DateTime.UtcNow,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditRulesJson
        };
        type.Versions.Add(new ActivityTypeVersion
        {
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditRulesJson,
            PublishedByUserId = "seed-system",
            PublishedOn = DateTime.UtcNow
        });
        db.ActivityTypes.Add(type);
        await db.SaveChangesAsync();
        return type;
    }

    /// <summary>
    /// A row as <c>ActivityService</c> leaves it, including the EPA it stamps from the pinned version's pointer (T137),
    /// which is the only place the report reads the EPA from. Returned so a test can set a stamp the data does not
    /// imply, which is how the tests below prove the data is not consulted.
    /// </summary>
    private static Activity AddActivity(
        ApplicationDbContext db,
        ActivityType type,
        string state,
        string dataJson,
        int institutionId = HostInstitution,
        DateTime? observedOn = null,
        int schemaVersion = 1)
    {
        var on = observedOn ?? InWindow;
        var activity = new Activity
        {
            ActivityTypeId = type.Id,
            SchemaVersion = schemaVersion,
            InstitutionId = institutionId,
            SubjectUserId = "trainee-1",
            CreatedByUserId = "trainee-1",
            CurrentState = state,
            DataJson = dataJson,
            EpaId = EvidenceEpaStamp.For(db, type.Id, schemaVersion, dataJson),
            CreatedOn = on,
            ObservedOn = DateOnly.FromDateTime(on),
            UpdatedOn = on
        };
        db.Activities.Add(activity);
        return activity;
    }
}
