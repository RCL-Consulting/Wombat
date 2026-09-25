using System.Data.Common;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.MultiSourceFeedback;

/// <summary>
/// T168 on a real PostgreSQL server: per EPA and semester, whether a released MSF campaign covering the EPA closed in it
/// (<see cref="GetMsfCoverageForTraineeQuery" />).
/// </summary>
/// <remarks>
/// <para>
/// The unit suite runs on EF InMemory, which evaluates the state rule, the recorded-EPA rule and the close-day bounds in
/// memory. Here they must translate: <c>ClosedOn</c> is a <c>timestamptz</c>, so the semester edges are compared on the
/// server as UTC instants, the last instant of 30 June in semester 1 and the first of 1 July in semester 2, and at the
/// span's far end the December fold (D40): a campaign closed in December, as late as its last instant, is semester 2's,
/// and one closed at the first instant of the next year is outside. The schema helpers follow
/// <c>MsfCampaignsOutsideSnapshotPostgresTests</c>.
/// </para>
/// <para>
/// Since T186 a campaign covers the EPAs its <c>msf_cpsa</c> evidence rows carry, not those its per-EPA stamp marks. The
/// rows here are written as the release writes them, with the campaign id inside a <c>jsonb</c> document, which Postgres
/// re-renders on the way in; the stamp is left null on every campaign but one, which is stamped with no row behind it.
/// One more row names a released campaign from a draft, and is not evidence (D44).
/// </para>
/// </remarks>
public sealed class MsfCoveragePostgresTests : IAsyncLifetime
{
    private const string Trainee = "trainee-t168";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task Coverage_OnPostgres_IsReleasedCampaignsEvidenceRowsOnly_BucketedByTheUtcCloseDay_FilteredOnTheServer()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            int host, first, second, closedLastInstantOfJune, closedFirstInstantOfJuly, closedMidDecember, closedLastInstantOfDecember;

            await using (var db = NewContext(schema))
            {
                host = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
                var curriculum = await db.Curricula.Include(entity => entity.Items).SingleAsync(entity => entity.Name == "IM Core Curriculum");
                first = curriculum.Items.Single().EpaId;

                // A second EPA on the seeded curriculum, so one campaign can cover one and not the other.
                var epa = new Epa
                {
                    SubSpecialityId = curriculum.SubSpecialityId,
                    Code = "EPA-T168",
                    Title = "A second EPA for T168",
                    Description = "Integration test.",
                    RequiredKnowledgeSkills = "None.",
                    CreatedOn = DateTime.UtcNow
                };
                db.Epas.Add(epa);
                await db.SaveChangesAsync();
                second = epa.Id;
                curriculum.Items.Add(new CurriculumItem { EpaId = second, RequiredCount = 1, MinimumLevelOrder = 4, WindowMonths = 12 });

                db.TraineeProfiles.Add(new TraineeProfile
                {
                    UserId = Trainee,
                    InstitutionId = host,
                    CurriculumId = curriculum.Id,
                    ProgrammeStartDate = new DateOnly(2025, 1, 1),
                    ExpectedCompletionDate = new DateOnly(2029, 1, 1),
                    IsActive = true
                });

                var template = new MsfTemplate { Name = "T168 MSF" };
                db.MsfTemplates.Add(template);

                // The type the release records its evidence as. DataSeeder seeds only the generic types.
                db.ActivityTypes.Add(new ActivityType
                {
                    Key = MsfEvidenceKinds.MsfActivityTypeKey,
                    Name = "Multi-Source Feedback (Paediatrics)",
                    Scope = ActivityScope.Institution,
                    ScopeId = host,
                    Version = 1,
                    // The shipped workflow: its terminal state is the rows' `recorded`, and evidence is a finished
                    // activity (D44). No version row, so the type's own column is the pin's workflow.
                    WorkflowJson = File.ReadAllText(
                        Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", "msf_cpsa", "workflow.json")),
                    OwnerUserId = "seed-system",
                    CreatedOn = DateTime.UtcNow
                });
                await db.SaveChangesAsync();

                // Semester 1's last instant, declaring both and with a row for the first only. The second's per-EPA stamp
                // is set, with no row behind it: the stamp is not what is read.
                closedLastInstantOfJune = await AddCampaignAsync(
                    db, template.Id, MsfCampaignState.Released, "2026-06-30T23:59:30Z", (first, true), (second, false));
                (await db.MsfCampaignEpas.SingleAsync(entity => entity.CampaignId == closedLastInstantOfJune && entity.EpaId == second))
                    .RecordedOn = DateTime.UtcNow;
                await db.SaveChangesAsync();
                // Semester 2's first instant.
                closedFirstInstantOfJuly = await AddCampaignAsync(
                    db, template.Id, MsfCampaignState.Released, "2026-07-01T00:00:00Z", (second, true));
                // After the College's 30 November, and the bucket's last instant: semester 2 to the end of December.
                closedMidDecember = await AddCampaignAsync(
                    db, template.Id, MsfCampaignState.Released, "2026-12-15T12:00:00Z", (first, true));
                // An unfinished row naming it for the second EPA: in a draft, so not evidence, and semester 2's campaigns
                // for the second EPA below do not include it.
                db.Activities.Add(EvidenceRow(
                    await MsfTypeIdAsync(db), closedMidDecember, second, Trainee,
                    DateTime.Parse(
                        "2026-12-15T12:00:00Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal),
                    state: "draft"));
                await db.SaveChangesAsync();
                closedLastInstantOfDecember = await AddCampaignAsync(
                    db, template.Id, MsfCampaignState.Released, "2026-12-31T23:59:30Z", (second, true));
                // Withdrawn, before the year, and after it: none covers anything in 2026.
                await AddCampaignAsync(db, template.Id, MsfCampaignState.Withdrawn, "2026-03-01T12:00:00Z", (second, true));
                await AddCampaignAsync(db, template.Id, MsfCampaignState.Released, "2025-12-31T23:59:59Z", (first, true));
                await AddCampaignAsync(db, template.Id, MsfCampaignState.Released, "2027-01-01T00:00:00Z", (first, true), (second, true));
                // Another trainee's.
                await AddCampaignAsync(
                    db, template.Id, MsfCampaignState.Released, "2026-08-01T12:00:00Z", "another-trainee", (first, true));
            }

            var commands = new CommandLog();
            MsfCoverageDto coverage;
            await using (var db = NewContext(schema, commands))
            {
                coverage = (await new GetMsfCoverageForTraineeQueryHandler(db).Handle(
                    new GetMsfCoverageForTraineeQuery(Trainee, CommitteeMember(host), new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)),
                    CancellationToken.None))!;
            }

            coverage.Periods.Select(period => period.Name).Should().Equal("Semester 1, 2026", "Semester 2, 2026");
            Covered(coverage, first).Should().Equal((2026, 1, closedLastInstantOfJune), (2026, 2, closedMidDecember));
            Covered(coverage, second).Should().Equal((2026, 2, closedLastInstantOfDecember));
            coverage.Epas.Single(epa => epa.EpaId == first).For(2026, 1)!.Latest!.ClosedOn.Should().Be(new DateOnly(2026, 6, 30));
            coverage.Epas.Single(epa => epa.EpaId == second).For(2026, 2)!.Campaigns
                .Select(campaign => (campaign.CampaignId, campaign.ClosedOn))
                .Should().Equal(
                    [(closedLastInstantOfDecember, new DateOnly(2026, 12, 31)), (closedFirstInstantOfJuly, new DateOnly(2026, 7, 1))],
                    "semester 2 runs from July's first instant to December's last, newest first");

            commands.Texts.Should().NotContain(
                text => text.Contains("\"MsfCampaignEpas\"", StringComparison.Ordinal),
                "coverage is read from the evidence rows, not from the declared set or its per-EPA stamp (T186)");

            var campaigns = commands.Texts.Where(text => text.Contains("\"MsfCampaigns\"", StringComparison.Ordinal))
                .Should().ContainSingle("one read of the trainee's campaigns").Subject;
            campaigns.Should().Contain("\"State\"").And.Contain("\"ClosedOn\"").And.Contain("\"SubjectUserId\"",
                "the state, close-day and trainee rules are the campaign's, applied in SQL");

            var evidence = commands.Texts.Where(text => text.Contains("\"Activities\"", StringComparison.Ordinal))
                .Should().ContainSingle("one read of the trainee's evidence rows").Subject;
            evidence.Should().Contain("\"ActivityTypes\"").And.Contain("\"Key\"").And.Contain("\"SubjectUserId\"")
                .And.Contain("\"EpaId\"", "only this trainee's rows of the evidence type that carry an EPA leave the server");
            commands.Texts.Should().Contain(
                text => text.Contains("\"ActivityTypeVersions\"", StringComparison.Ordinal),
                "whether a row is finished is judged by the workflow it is pinned to (D44)");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// T210 on the server: the programme's "n of m trainees covered" is each trainee's card counted, and is read in one
    /// query of the campaigns and one of the evidence rows for every trainee at once, not one pair a trainee. The
    /// trainees are read through the preferred-profile rule with a list of ids, the campaigns and rows through lists of
    /// trainees, and the semester edges are the server's UTC instants, as for one trainee.
    /// </summary>
    [Fact]
    public async Task ProgrammeCoverage_OnPostgres_IsEachTraineesCardCounted_InOneReadOfTheCampaignsAndOneOfTheEvidence()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            int host, first, second;

            await using (var db = NewContext(schema))
            {
                host = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
                var other = new Institution { Name = "T210 Other Hospital", ShortCode = "T210" };
                db.Institutions.Add(other);

                var curriculum = await db.Curricula.Include(entity => entity.Items).SingleAsync(entity => entity.Name == "IM Core Curriculum");
                first = curriculum.Items.Single().EpaId;
                var epa = new Epa
                {
                    SubSpecialityId = curriculum.SubSpecialityId,
                    Code = "EPA-T210",
                    Title = "A second EPA for T210",
                    Description = "Integration test.",
                    RequiredKnowledgeSkills = "None.",
                    CreatedOn = DateTime.UtcNow
                };
                db.Epas.Add(epa);
                await db.SaveChangesAsync();
                second = epa.Id;
                curriculum.Items.Add(new CurriculumItem { EpaId = second, RequiredCount = 1, MinimumLevelOrder = 4, WindowMonths = 12 });

                // Three trainees here, one who has completed, and one at the other institution.
                AddProfile(db, "t210-ada", host, curriculum.Id, new DateOnly(2025, 1, 1), isActive: true);
                AddProfile(db, "t210-ben", host, curriculum.Id, new DateOnly(2025, 1, 1), isActive: true);
                AddProfile(db, "t210-cara", host, curriculum.Id, new DateOnly(2026, 8, 1), isActive: true);
                AddProfile(db, "t210-gus", host, curriculum.Id, new DateOnly(2021, 1, 1), isActive: false);
                AddProfile(db, "t210-eve", other.Id, curriculum.Id, new DateOnly(2025, 1, 1), isActive: true);

                var template = new MsfTemplate { Name = "T210 MSF" };
                db.MsfTemplates.Add(template);
                db.ActivityTypes.Add(new ActivityType
                {
                    Key = MsfEvidenceKinds.MsfActivityTypeKey,
                    Name = "Multi-Source Feedback (Paediatrics)",
                    Scope = ActivityScope.Institution,
                    ScopeId = host,
                    Version = 1,
                    WorkflowJson = File.ReadAllText(
                        Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", "msf_cpsa", "workflow.json")),
                    OwnerUserId = "seed-system",
                    CreatedOn = DateTime.UtcNow
                });
                await db.SaveChangesAsync();

                // Ada: semester 1's last instant, the first EPA. Ben: semester 2's first instant, both, though only the
                // second has a row. Cara, who started in August: a withdrawn campaign. Gus and Eve: covered, not counted
                // here.
                await AddCampaignAsync(db, template.Id, MsfCampaignState.Released, "2026-06-30T23:59:30Z", "t210-ada", (first, true));
                await AddCampaignAsync(db, template.Id, MsfCampaignState.Released, "2026-07-01T00:00:00Z", "t210-ben", (first, false), (second, true));
                await AddCampaignAsync(db, template.Id, MsfCampaignState.Withdrawn, "2026-08-20T12:00:00Z", "t210-cara", (first, true));
                await AddCampaignAsync(db, template.Id, MsfCampaignState.Released, "2026-03-01T12:00:00Z", "t210-gus", (first, true));
                await AddCampaignAsync(db, template.Id, MsfCampaignState.Released, "2026-03-01T12:00:00Z", "t210-eve", (first, true));
            }

            var asOf = new DateOnly(2026, 9, 1);
            var commands = new CommandLog();
            MsfProgrammeCoverageDto coverage;
            await using (var db = NewContext(schema, commands))
            {
                coverage = await new GetMsfProgrammeCoverageQueryHandler(db, FakeUserDirectory.TraineesOf(db)).Handle(
                    new GetMsfProgrammeCoverageQuery(Coordinator(host), asOf),
                    CancellationToken.None);
            }

            var programme = coverage.Programmes.Should().ContainSingle("the coordinator's institution follows one curriculum").Subject;
            programme.Trainees.Select(trainee => trainee.UserId).Should().BeEquivalentTo(["t210-ada", "t210-ben", "t210-cara"]);
            programme.Periods.Select(period => period.Trainees).Should().Equal(2, 3);
            programme.Epas.Single(epa => epa.EpaId == first).Periods.Select(period => (period.TraineesCovered, period.Trainees))
                .Should().Equal((1, 2), (0, 3));
            programme.Epas.Single(epa => epa.EpaId == second).Periods.Select(period => (period.TraineesCovered, period.Trainees))
                .Should().Equal((0, 2), (1, 3));

            var campaigns = commands.Texts.Where(text => text.Contains("\"MsfCampaigns\"", StringComparison.Ordinal))
                .Should().ContainSingle("one read of every trainee's campaigns").Subject;
            campaigns.Should().Contain("\"State\"").And.Contain("\"ClosedOn\"").And.Contain("\"SubjectUserId\"");
            commands.Texts.Where(text => text.Contains("\"Activities\"", StringComparison.Ordinal))
                .Should().ContainSingle("one read of every trainee's evidence rows");

            // An Administrator sees the curriculum at both institutions, two programmes, and every programme's EPA list is
            // drawn from one read of the curricula's items, not one read a programme.
            var administratorCommands = new CommandLog();
            MsfProgrammeCoverageDto everywhere;
            await using (var db = NewContext(schema, administratorCommands))
            {
                everywhere = await new GetMsfProgrammeCoverageQueryHandler(db, FakeUserDirectory.TraineesOf(db)).Handle(
                    new GetMsfProgrammeCoverageQuery(Administrator(), asOf),
                    CancellationToken.None);
            }

            everywhere.Programmes.Select(entry => entry.Trainees.Count).Should().BeEquivalentTo([3, 1]);
            everywhere.Programmes.Should().AllSatisfy(entry =>
                entry.Epas.Select(epa => epa.EpaId).Should().BeEquivalentTo([first, second]));
            administratorCommands.Texts.Where(text => text.Contains("\"CurriculumItems\"", StringComparison.Ordinal))
                .Should().ContainSingle("one read of the items for every programme");

            // Each trainee's own card on the same server says the same.
            await using (var db = NewContext(schema))
            {
                foreach (var trainee in programme.Trainees)
                {
                    var card = (await new GetMsfCoverageForTraineeQueryHandler(db).Handle(
                        new GetMsfCoverageForTraineeQuery(trainee.UserId, Coordinator(host), AsOf: asOf),
                        CancellationToken.None))!;

                    foreach (var period in trainee.Periods.Where(period => period.HadStarted))
                    {
                        period.EpasCovered.Should().Be(
                            card.Periods.Single(entry => entry.Year == period.Year && entry.Semester == period.Semester).EpasCovered,
                            $"{trainee.UserId} {period.Year} S{period.Semester}");
                    }
                }
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static void AddProfile(
        ApplicationDbContext db, string userId, int institutionId, int curriculumId, DateOnly start, bool isActive)
        => db.TraineeProfiles.Add(new TraineeProfile
        {
            UserId = userId,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            ProgrammeStartDate = start,
            ExpectedCompletionDate = start.AddYears(4),
            IsActive = isActive
        });

    /// <summary>A global Administrator.</summary>
    private static ClaimsPrincipal Administrator()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "administrator-t210"),
                new Claim(ClaimTypes.Role, WombatRoles.Administrator)
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    /// <summary>A coordinator at the trainees' institution.</summary>
    private static ClaimsPrincipal Coordinator(int institutionId)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "coordinator-t210"),
                new Claim(ClaimTypes.Role, WombatRoles.Coordinator),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    private static IEnumerable<(int Year, int Semester, int CampaignId)> Covered(MsfCoverageDto coverage, int epaId)
        => coverage.Epas.Single(epa => epa.EpaId == epaId).Periods
            .Where(period => period.IsCovered)
            .Select(period => (period.Year, period.Semester, period.Latest!.CampaignId))
            .ToArray();

    private static Task<int> AddCampaignAsync(
        ApplicationDbContext db,
        int templateId,
        MsfCampaignState state,
        string closedOn,
        params (int EpaId, bool Evidence)[] epas)
        => AddCampaignAsync(db, templateId, state, closedOn, Trainee, epas);

    /// <summary>
    /// A campaign declaring each of <paramref name="epas" />, with an <c>msf_cpsa</c> evidence row naming it for each one
    /// marked <c>Evidence</c>, written as <c>ReleaseMsfCampaign</c> writes it. The per-EPA stamp is left null.
    /// </summary>
    private static async Task<int> AddCampaignAsync(
        ApplicationDbContext db,
        int templateId,
        MsfCampaignState state,
        string closedOn,
        string subjectUserId,
        params (int EpaId, bool Evidence)[] epas)
    {
        var closed = DateTime.Parse(closedOn, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
        var released = closed.AddDays(2);
        var campaign = new MsfCampaign
        {
            SubjectUserId = subjectUserId,
            TemplateId = templateId,
            CreatedByUserId = "coordinator-t168",
            CreatedOn = new DateTime(2025, 11, 1, 8, 0, 0, DateTimeKind.Utc),
            OpensOn = new DateOnly(2025, 11, 1),
            ClosesOn = DateOnly.FromDateTime(closed),
            State = state,
            OpenedOn = new DateTime(2025, 11, 1, 8, 0, 0, DateTimeKind.Utc),
            ClosedOn = closed,
            ReleasedOn = state == MsfCampaignState.Released ? released : null,
            WithdrawnOn = state == MsfCampaignState.Withdrawn ? released : null
        };

        foreach (var (epaId, _) in epas)
        {
            campaign.CoveredEpas.Add(new MsfCampaignEpa { EpaId = epaId });
        }

        db.MsfCampaigns.Add(campaign);
        await db.SaveChangesAsync();

        var evidenceTypeId = await MsfTypeIdAsync(db);

        foreach (var (epaId, _) in epas.Where(entry => entry.Evidence))
        {
            db.Activities.Add(EvidenceRow(evidenceTypeId, campaign.Id, epaId, subjectUserId, closed));
        }

        await db.SaveChangesAsync();
        return campaign.Id;
    }

    private static Task<int> MsfTypeIdAsync(ApplicationDbContext db)
        => db.ActivityTypes
            .Where(type => type.Key == MsfEvidenceKinds.MsfActivityTypeKey)
            .Select(type => type.Id)
            .SingleAsync();

    /// <summary>One <c>msf_cpsa</c> row as <c>ReleaseMsfCampaign</c> writes it, in <paramref name="state" />.</summary>
    private static Activity EvidenceRow(
        int evidenceTypeId, int campaignId, int epaId, string subjectUserId, DateTime closed, string state = "recorded")
        => new()
        {
            ActivityTypeId = evidenceTypeId,
            SchemaVersion = 1,
            SubjectUserId = subjectUserId,
            CreatedByUserId = "coordinator-t168",
            CurrentState = state,
            DataJson = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                ["epa_id"] = epaId,
                [MsfCampaignCoverage.CampaignIdField] = campaignId,
                ["observed_on"] = DateOnly.FromDateTime(closed).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["respondent_count"] = 8
            }),
            EpaId = epaId,
            CreatedOn = closed.AddDays(2),
            UpdatedOn = closed.AddDays(2),
            ObservedOn = DateOnly.FromDateTime(closed),
            ObservedOnSource = ObservationDateSource.Declared
        };

    /// <summary>A committee member at the trainee's institution: overseen, so the scope lookup runs on the server too.</summary>
    private static ClaimsPrincipal CommitteeMember(int institutionId)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "committee-t168"),
                new Claim(ClaimTypes.Role, WombatRoles.CommitteeMember),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    /// <summary>A migrated schema with the demo institution and its curriculum seeded (<see cref="DataSeeder" />).</summary>
    private async Task<string> MigratedSchemaAsync()
    {
        var schema = await _schemas.CreateAsync();

        await using (var db = NewContext(schema))
        {
            await db.Database.MigrateAsync();
        }

        await using (var db = NewContext(schema))
        {
            await new DataSeeder(db).SeedAsync();
        }

        return schema;
    }

    private ApplicationDbContext NewContext(string schema, params IInterceptor[] interceptors)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema));
        if (interceptors.Length > 0)
        {
            options.AddInterceptors(interceptors);
        }

        return new ApplicationDbContext(options.Options);
    }

    /// <summary>Every query EF sends through one context, so "one round trip, all in SQL" is asserted on the wire.</summary>
    private sealed class CommandLog : DbCommandInterceptor
    {
        public List<string> Texts { get; } = [];

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            Texts.Add(command.CommandText);
            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            Texts.Add(command.CommandText);
            return ValueTask.FromResult(result);
        }
    }
}
