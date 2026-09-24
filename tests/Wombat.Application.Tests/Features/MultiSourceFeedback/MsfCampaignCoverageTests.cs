using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Activities;
using Wombat.Domain.Epas;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// The one reading of which EPAs a released MSF campaign covered (T186): the EPAs its evidence rows carry, the rows its
/// release wrote. Shared by the committee snapshot's campaign line and the coverage grid, whose own tests read it through
/// them; these pin the rule itself.
/// </summary>
public sealed class MsfCampaignCoverageTests
{
    private const string Trainee = "trainee-1";
    private const int MsfTypeId = 60;
    private const int LearnerFeedbackTypeId = 61;
    private const string LearnerFeedbackKey = "learner_feedback_cpsa";
    private const int Paed001 = 1;
    private const int Paed002 = 2;
    private const int Paed010 = 10;

    [Fact]
    public async Task AReleasedCampaign_CoversTheEpasItsEvidenceRowsCarry_InCodeOrder_EachOnce()
    {
        await using var db = await SeedAsync();
        AddRow(db, MsfTypeId, Evidence(50, Paed010), Paed010);
        AddRow(db, MsfTypeId, Evidence(50, Paed001), Paed001);
        AddRow(db, MsfTypeId, Evidence(50, Paed001), Paed001);
        await db.SaveChangesAsync();

        var recorded = await ReadAsync(db, [(50, MsfCampaignState.Released)]);

        recorded[50].Should().Equal(new MsfRecordedEpa(Paed001, "PAED-001"), new MsfRecordedEpa(Paed010, "PAED-010"));
    }

    [Theory]
    [InlineData(MsfCampaignState.Draft)]
    [InlineData(MsfCampaignState.Open)]
    [InlineData(MsfCampaignState.Closed)]
    [InlineData(MsfCampaignState.UnderReview)]
    [InlineData(MsfCampaignState.Withdrawn)]
    public async Task ACampaignThatIsNotReleased_CoversNothing_EvenWithEvidenceRowsNamingIt(MsfCampaignState state)
    {
        await using var db = await SeedAsync();
        AddRow(db, MsfTypeId, Evidence(50, Paed001), Paed001);
        AddRow(db, MsfTypeId, Evidence(51, Paed002), Paed002);
        await db.SaveChangesAsync();

        var recorded = await ReadAsync(db, [(50, state), (51, MsfCampaignState.Released)]);

        recorded[50].Should().BeEmpty("the rule is the helper's, not only its callers' queries");
        recorded[51].Should().ContainSingle().Which.EpaId.Should().Be(Paed002);
    }

    [Fact]
    public async Task TheEvidenceTypeIsAParameter_AndOnlyThatTypesRowsAreRead()
    {
        await using var db = await SeedAsync();
        AddRow(db, MsfTypeId, Evidence(50, Paed001), Paed001);
        AddRow(db, LearnerFeedbackTypeId, Evidence(50, Paed002), Paed002, state: "acknowledged");
        await db.SaveChangesAsync();

        var asMsf = await ReadAsync(db, [(50, MsfCampaignState.Released)]);
        var asLearnerFeedback = await ReadAsync(db, [(50, MsfCampaignState.Released)], LearnerFeedbackKey);

        asMsf[50].Select(epa => epa.EpaId).Should().Equal(Paed001);
        asLearnerFeedback[50].Select(epa => epa.EpaId).Should().Equal(Paed002);
    }

    [Fact]
    public async Task ARowCountsOnlyForTheCampaignItsDataNames_AboutTheSubject_WithAStampedEpa()
    {
        await using var db = await SeedAsync();
        AddRow(db, MsfTypeId, Evidence(50, Paed001), Paed001);
        // Another campaign's row, another trainee's row naming campaign 50, and a row whose EPA stamp is null.
        AddRow(db, MsfTypeId, Evidence(99, Paed002), Paed002);
        AddRow(db, MsfTypeId, Evidence(50, Paed002), Paed002, subjectUserId: "trainee-2");
        AddRow(db, MsfTypeId, Evidence(50, Paed010), epaId: null);
        // Data that names no campaign a release could have written: a string, a fraction, no key, not an object.
        AddRow(db, MsfTypeId, """{ "campaign_id": "50" }""", Paed010);
        AddRow(db, MsfTypeId, """{ "campaign_id": 50.5 }""", Paed010);
        AddRow(db, MsfTypeId, """{ "epa_id": 10 }""", Paed010);
        AddRow(db, MsfTypeId, "[50]", Paed010);
        await db.SaveChangesAsync();

        var recorded = await ReadAsync(db, [(50, MsfCampaignState.Released), (99, MsfCampaignState.Withdrawn)]);

        recorded[50].Select(epa => epa.EpaId).Should().Equal([Paed001], "only the first row is campaign 50's evidence about this trainee");
        recorded[99].Should().BeEmpty();
    }

    /// <summary>
    /// Evidence is a finished activity (D44), judged by the workflow each row is pinned to. The release writes
    /// <c>msf_cpsa</c> rows straight into <c>recorded</c>, so for MSF this is belt and braces; the type is a parameter, and
    /// a type whose rows can sit in a draft or end declined must not have those read as coverage.
    /// </summary>
    [Fact]
    public async Task OnlyAFinishedRowCounts_ByTheWorkflowOfItsOwnType()
    {
        await using var db = await SeedAsync();
        AddRow(db, MsfTypeId, Evidence(50, Paed001), Paed001);
        AddRow(db, MsfTypeId, Evidence(50, Paed002), Paed002, state: "draft");
        AddRow(db, LearnerFeedbackTypeId, Evidence(50, Paed001), Paed001, state: "acknowledged");
        AddRow(db, LearnerFeedbackTypeId, Evidence(50, Paed002), Paed002, state: "declined");
        AddRow(db, LearnerFeedbackTypeId, Evidence(50, Paed010), Paed010, state: "recorded");
        await db.SaveChangesAsync();

        var asMsf = await ReadAsync(db, [(50, MsfCampaignState.Released)]);
        var asLearnerFeedback = await ReadAsync(db, [(50, MsfCampaignState.Released)], LearnerFeedbackKey);

        asMsf[50].Select(epa => epa.EpaId).Should().Equal([Paed001], "a draft msf_cpsa row is not evidence");
        asLearnerFeedback[50].Select(epa => epa.EpaId).Should().Equal(
            [Paed001],
            "declined is a dead end, not a finish, and recorded is MSF's terminal state, not this type's");
    }

    [Fact]
    public async Task NoReleasedCampaign_ReadsNothing()
    {
        await using var db = await SeedAsync();
        AddRow(db, MsfTypeId, Evidence(50, Paed001), Paed001);
        await db.SaveChangesAsync();

        (await ReadAsync(db, [])).Should().BeEmpty();
        (await ReadAsync(db, [(50, MsfCampaignState.Withdrawn)])).Should().BeEmpty();
    }

    // ─── Fixtures ────────────────────────────────────────────────────────────

    private static Task<ILookup<int, MsfRecordedEpa>> ReadAsync(
        ApplicationDbContext db,
        (int CampaignId, MsfCampaignState State)[] campaigns,
        string evidenceTypeKey = MsfCampaignCoverage.MsfEvidenceTypeKey)
        => MsfCampaignCoverage.RecordedEpasAsync(db, Trainee, campaigns, evidenceTypeKey, CancellationToken.None);

    private static string Evidence(int campaignId, int epaId)
        => $$"""{ "epa_id": {{epaId}}, "campaign_id": {{campaignId}}, "respondent_count": 8 }""";

    private static void AddRow(
        ApplicationDbContext db,
        int activityTypeId,
        string dataJson,
        int? epaId,
        string subjectUserId = Trainee,
        string state = "recorded")
        => db.Activities.Add(new Activity
        {
            ActivityTypeId = activityTypeId,
            SchemaVersion = 1,
            SubjectUserId = subjectUserId,
            CreatedByUserId = "coord-1",
            CurrentState = state,
            DataJson = dataJson,
            EpaId = epaId,
            CreatedOn = new DateTime(2026, 3, 13, 9, 0, 0, DateTimeKind.Utc),
            UpdatedOn = new DateTime(2026, 3, 13, 9, 0, 0, DateTimeKind.Utc),
            ObservedOn = new DateOnly(2026, 3, 10),
            ObservedOnSource = ObservationDateSource.Declared
        });

    private static async Task<ApplicationDbContext> SeedAsync()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        db.Epas.AddRange(
            new Epa { Id = Paed001, SubSpecialityId = 1, Code = "PAED-001", Title = "PAED-001", IsActive = true },
            new Epa { Id = Paed002, SubSpecialityId = 1, Code = "PAED-002", Title = "PAED-002", IsActive = true },
            new Epa { Id = Paed010, SubSpecialityId = 1, Code = "PAED-010", Title = "PAED-010", IsActive = true });
        db.ActivityTypes.AddRange(
            Type(MsfTypeId, MsfCampaignCoverage.MsfEvidenceTypeKey, MsfSeedWorkflow()),
            Type(LearnerFeedbackTypeId, LearnerFeedbackKey, LearnerFeedbackWorkflow));
        await db.SaveChangesAsync();
        return db;
    }

    private static ActivityType Type(int id, string key, string workflowJson)
        => new()
        {
            Id = id,
            Key = key,
            Name = key,
            Version = 1,
            WorkflowJson = workflowJson,
            OwnerUserId = "seed-system",
            CreatedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

    /// <summary>The shipped <c>msf_cpsa</c> workflow: <c>draft</c>, then <c>recorded</c>, terminal.</summary>
    private static string MsfSeedWorkflow()
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", "msf_cpsa", "workflow.json"));

    /// <summary>
    /// A stand-in for a type whose rows are not system-written into their finish (T164 is not built): the subject
    /// acknowledges the feedback, terminal, or declines it, a dead end as every seed's <c>declined</c> is.
    /// </summary>
    private const string LearnerFeedbackWorkflow = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "acknowledged", "label": "Acknowledged", "terminal": true },
            { "key": "declined", "label": "Declined" }
          ],
          "transitions": [
            { "key": "acknowledge", "from": "draft", "to": "acknowledged", "actor": "subject", "validation": "all" },
            { "key": "decline", "from": "draft", "to": "declined", "actor": "subject", "validation": "draft" }
          ]
        }
        """;
}
