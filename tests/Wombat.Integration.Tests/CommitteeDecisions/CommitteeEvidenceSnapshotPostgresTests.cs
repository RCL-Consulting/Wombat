using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Common.Security;
using Wombat.Domain.Activities;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Identity;
using Wombat.Tests.Shared;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.CommitteeDecisions;

/// <summary>
/// T167 on a real PostgreSQL server, migrated and seeded with the v11.1 catalogue: a committee snapshot line records its
/// EPA, instrument, rung and encounter date in the columns T167's migration adds, and a STAR is held to the trainee's
/// curriculum by the same query the page's picker runs. T131: a staged decision names the snapshot line it rests on,
/// ratifying copies that line onto the STAR and supersedes the prior STAR in one save, and the table holds one staged
/// decision per EPA at a review.
/// </summary>
/// <remarks>
/// The unit suites run on EF InMemory, which neither applies the migration nor translates the preferred-profile
/// predicate the curriculum lookup shares with <see cref="TraineeScopeResolver" />. The schema helpers follow
/// <c>MsfCampaignScopePostgresTests</c>: a schema of the test's own, registered before it is created and dropped in a
/// <c>finally</c>, with <see cref="DisposeAsync" /> as a backstop.
/// </remarks>
public sealed class CommitteeEvidenceSnapshotPostgresTests : IAsyncLifetime
{
    private const string TraineeUserId = "trainee-t167";
    private const string ChairUserId = "chair-t167";
    private const string MemberUserId = "member-t167";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task TheSnapshotNamesEachLine_AndAStarIsHeldToTheTraineesCurriculum_OnPostgres()
    {
        try
        {
            var schema = await SeededSchemaAsync();
            int reviewId, paed001, demoEpa, ladderId, activityId, lineId, hostId;

            await using (var db = NewContext(schema))
            {
                var host = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
                hostId = host;
                var curriculumId = await db.Curricula
                    .Where(entity => entity.Name == "Paediatric EPA Curriculum" && entity.Version == "11.1")
                    .Select(entity => entity.Id)
                    .SingleAsync();
                paed001 = await db.Epas.Where(epa => epa.Code == "PAED-001" && epa.OwningInstitutionId == null).Select(epa => epa.Id).SingleAsync();
                demoEpa = await db.Epas.Where(epa => epa.Code == "EPA-001").Select(epa => epa.Id).SingleAsync();
                ladderId = await db.EntrustmentScales
                    .Where(scale => scale.Name == "CPSA Paediatric Entrustment Scale v11.1")
                    .Select(scale => scale.Id)
                    .SingleAsync();
                var cca = await db.ActivityTypes.Where(type => type.Key == "cca_cpsa").Select(type => new { type.Id, type.Version }).SingleAsync();

                db.TraineeProfiles.Add(new TraineeProfile
                {
                    UserId = TraineeUserId,
                    InstitutionId = host,
                    CurriculumId = curriculumId,
                    ProgrammeStartDate = new DateOnly(2025, 1, 15),
                    ExpectedCompletionDate = new DateOnly(2029, 1, 14)
                });

                var panel = new DecisionPanel
                {
                    Name = "T167 CCC",
                    Scope = DecisionPanelScope.Institution,
                    InstitutionId = host,
                    CreatedOn = DateTime.UtcNow,
                    // The chair and one other: a committee decision needs a quorum (T165, D46).
                    Members =
                    [
                        new DecisionPanelMember { UserId = ChairUserId, Role = DecisionPanelMemberRole.Chair },
                        new DecisionPanelMember { UserId = MemberUserId, Role = DecisionPanelMemberRole.Member }
                    ]
                };
                db.DecisionPanels.Add(panel);
                var review = new CommitteeReview
                {
                    AcademicYear = 2026,
                    Semester = 1,
                    Panel = panel,
                    TraineeUserId = TraineeUserId,
                    ReviewPeriodFrom = new DateOnly(2026, 1, 1),
                    ReviewPeriodTo = new DateOnly(2026, 6, 30),
                    ScheduledOn = new DateOnly(2026, 7, 2)
                };
                db.CommitteeReviews.Add(review);

                var filed = new DateTime(2026, 2, 11, 9, 0, 0, DateTimeKind.Utc);
                var activity = new Activity
                {
                    ActivityTypeId = cca.Id,
                    SchemaVersion = cca.Version,
                    InstitutionId = host,
                    SubjectUserId = TraineeUserId,
                    CreatedByUserId = TraineeUserId,
                    CurrentState = "completed",
                    DataJson = $$"""{ "epa_id": {{paed001.ToString(CultureInfo.InvariantCulture)}}, "assessor_user_id": "assessor-1", "observed_on": "2026-02-10", "overall_level": 3 }""",
                    EpaId = paed001,
                    CreatedOn = filed,
                    UpdatedOn = filed,
                    ObservedOn = new DateOnly(2026, 2, 10),
                    ObservedOnSource = ObservationDateSource.Declared
                };
                db.Activities.Add(activity);
                await db.SaveChangesAsync();
                reviewId = review.Id;
                activityId = activity.Id;
            }

            await using (var db = NewContext(schema))
            {
                await new StartCommitteeReviewCommandHandler(db, FakeUserDirectory.PanelMembersOf(db)).Handle(
                    new StartCommitteeReviewCommand(reviewId, Chair()), CancellationToken.None);
            }

            await using (var db = NewContext(schema))
            {
                var review = await new GetCommitteeReviewByIdQueryHandler(db, FakeUserDirectory.PanelMembersOf(db)).Handle(
                    new GetCommitteeReviewByIdQuery(reviewId, Chair()), CancellationToken.None);

                var line = review.EvidenceItems.Should().ContainSingle(item => item.ActivityId == activityId).Subject;
                lineId = line.Id;
                line.CanGroundADecision.Should().BeTrue("an activity line may be named as the evidence a STAR rests on");
                line.EpaCode.Should().Be("PAED-001");
                line.InstrumentKey.Should().Be("cca");
                line.InstrumentName.Should().Be("CCA");
                line.RatingOrder.Should().Be(3);
                line.RatingLabel.Should().Be("3a");
                line.ObservedOn.Should().Be(new DateOnly(2026, 2, 10));
                line.ObservedOnDeclared.Should().BeTrue();
                line.SourceState.Should().Be("completed");
                line.SourceStateLabel.Should().Be("Completed", "T220: the state's label is frozen with it, in its own column");
                line.SourceFinished.Should().BeTrue("T131: whether a line was finished work is frozen with its state");

                // T131 slice 4: Start planned the agenda from the v11.1 cadence. A semester-1 sitting holds all fifteen
                // EPAs; the six decided each semester are closing, and the nine annual ones are due by year end.
                var agenda = review.Agenda!;
                agenda.PeriodLabel.Should().Be("2026 S1");
                agenda.Lines.Should().HaveCount(15);
                agenda.Lines.Where(agendaLine => agendaLine.IsClosing).Select(agendaLine => agendaLine.EpaCode)
                    .Should().Equal("PAED-001", "PAED-002", "PAED-004", "PAED-005", "PAED-010", "PAED-012");
                agenda.Lines.Where(agendaLine => !agendaLine.IsClosing)
                    .Should().OnlyContain(agendaLine => agendaLine.Status == CommitteeAgendaLineStatus.DueByYearEnd);

                var options = await new ListStarEpaOptionsForReviewQueryHandler(db, FakeUserDirectory.PanelMembersOf(db)).Handle(
                    new ListStarEpaOptionsForReviewQuery(reviewId, Chair()), CancellationToken.None);
                options.Should().HaveCount(15, "the v11.1 curriculum's national core");
                options.Should().OnlyContain(option => option.ScaleId == ladderId, "every v11.1 item is pinned to the v11.1 ladder");
                options.Should().NotContain(option => option.EpaId == demoEpa, "EPA-001 is the demo curriculum's, not this trainee's");
            }

            var rung3b = await RungAsync(schema, ladderId, "3b");

            await using (var db = NewContext(schema))
            {
                var offCurriculum = () => StageAsync(db, reviewId, demoEpa, rung3b, lineId);
                await offCurriculum.Should().ThrowAsync<InvalidOperationException>().WithMessage("*EPA-001 is not on this trainee's curriculum*");
                await db.SaveChangesAsync();
            }

            await using (var db = NewContext(schema))
            {
                (await db.PendingEntrustmentDecisions.CountAsync()).Should().Be(0, "a refusal writes nothing, even through the audit save");

                var staged = await StageAsync(db, reviewId, paed001, rung3b, lineId);
                staged.EpaCode.Should().Be("PAED-001");
                staged.EvidenceItemIds.Should().Equal(lineId);
            }

            // T131: ratifying issues the STAR with a link copied from the frozen line, and supersedes the trainee's current
            // STAR on the EPA in the same save, through the new self-referencing foreign key.
            var rung3a = await RungAsync(schema, ladderId, "3a");
            int priorId;
            await using (var db = NewContext(schema))
            {
                var prior = EntrustmentDecision.Issue(
                    TraineeUserId, paed001, rung3a, new DateOnly(2026, 1, 8), null, reviewId, ChairUserId, "An earlier sitting.",
                    StarEvidence.One(lineId, activityId));
                db.EntrustmentDecisions.Add(prior);
                await db.SaveChangesAsync();
                priorId = prior.Id;
            }

            // A quorate sitting, the chair and the other member present, each a committee member at the host (T165).
            RecordCommitteeDecisionCommandHandler Record(ApplicationDbContext db)
                => new(db, FakeUserDirectory.CommitteeMembersAt(hostId, ChairUserId, MemberUserId));
            var recordCommand = new RecordCommitteeDecisionCommand(
                reviewId, CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, [ChairUserId, MemberUserId], Chair());

            // T131 slice 4: a closing line is staged or deferred before the decision is recorded, which fixes the agenda
            // (T165), and so before ratify. PAED-001 is staged; the chair defers the other five semester EPAs, through the
            // handler, as the page does.
            await using (var db = NewContext(schema))
            {
                var closingUnstaged = await db.CommitteeAgendaLines
                    .Where(agendaLine => agendaLine.ReviewId == reviewId && agendaLine.IsClosing && agendaLine.EpaId != paed001)
                    .Select(agendaLine => agendaLine.Id)
                    .ToListAsync();
                closingUnstaged.Should().HaveCount(5);

                var early = () => Record(db).Handle(recordCommand, CancellationToken.None);
                await early.Should().ThrowAsync<InvalidOperationException>()
                    .WithMessage("The committee's decision cannot be recorded yet: PAED-002, PAED-004, PAED-005, PAED-010 and PAED-012 must be decided at this sitting.*");
                await db.SaveChangesAsync();
                (await db.CommitteeDecisions.CountAsync(decision => decision.ReviewId == reviewId))
                    .Should().Be(0, "a refused recording writes nothing, even through the audit save");

                foreach (var agendaLineId in closingUnstaged)
                {
                    await new DeferAgendaLineCommandHandler(db, FakeUserDirectory.PanelMembersOf(db)).Handle(
                        new DeferAgendaLineCommand(reviewId, agendaLineId, "Not yet observed enough to decide.", Chair()),
                        CancellationToken.None);
                }
            }

            await using (var db = NewContext(schema))
            {
                await Record(db).Handle(recordCommand, CancellationToken.None);
            }

            await using (var db = NewContext(schema))
            {
                await new RatifyCommitteeDecisionCommandHandler(db, FakeUserDirectory.PanelMembersOf(db)).Handle(
                    new RatifyCommitteeDecisionCommand(reviewId, Chair()), CancellationToken.None);
            }

            await using (var db = NewContext(schema))
            {
                var frozen = await db.Set<CommitteeEvidence>().AsNoTracking().SingleAsync(item => item.Id == lineId);
                var star = await db.EntrustmentDecisions.AsNoTracking()
                    .Include(decision => decision.EvidenceLinks)
                    .SingleAsync(decision => decision.Id != priorId);
                star.Status.Should().Be(EntrustmentDecisionStatus.Active);
                star.AuthorisedLevelId.Should().Be(rung3b);

                var link = star.EvidenceLinks.Should().ContainSingle().Subject;
                link.CommitteeEvidenceId.Should().Be(lineId);
                link.SourceType.Should().Be(EntrustmentEvidenceSourceType.Activity);
                link.ActivityId.Should().Be(activityId);
                link.SourceLabel.Should().Be(frozen.SourceLabel);
                link.Summary.Should().Be(frozen.Summary);
                link.SourceRecordedOn.Should().Be(frozen.SourceRecordedOn);

                var prior = await db.EntrustmentDecisions.AsNoTracking().SingleAsync(decision => decision.Id == priorId);
                prior.Status.Should().Be(EntrustmentDecisionStatus.Superseded);
                prior.SupersededByDecisionId.Should().Be(star.Id);
                (await db.PendingEntrustmentDecisions.CountAsync()).Should().Be(0);

                // The agenda closed in the same save as the STAR was issued: the staged line names it (the check constraint
                // holds within the one save), the deferred lines keep their reason, and the optional ones were not decided.
                var lines = await db.CommitteeAgendaLines.AsNoTracking().Where(agendaLine => agendaLine.ReviewId == reviewId).ToListAsync();
                var decided = lines.Should().ContainSingle(agendaLine => agendaLine.State == CommitteeAgendaLineState.Decided).Subject;
                decided.EpaId.Should().Be(paed001);
                decided.EntrustmentDecisionId.Should().Be(star.Id);
                lines.Count(agendaLine => agendaLine.State == CommitteeAgendaLineState.Deferred).Should().Be(5);
                lines.Where(agendaLine => agendaLine.State == CommitteeAgendaLineState.Deferred)
                    .Should().OnlyContain(agendaLine => agendaLine.DeferralReason == "Not yet observed enough to decide.");
                lines.Count(agendaLine => agendaLine.State == CommitteeAgendaLineState.NotDecided).Should().Be(9);
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// One staged decision per EPA at a review is held by the table, not only by the handler's check: two chairs staging
    /// at once pass the check together, and the second insert fails on the unique index. (T131)
    /// </summary>
    [Fact]
    public async Task OneStagedDecisionPerEpaAtAReview_IsHeldByTheUniqueIndex_OnPostgres()
    {
        try
        {
            var schema = await SeededSchemaAsync();
            int reviewId, paed001, paed002, levelId;

            await using (var db = NewContext(schema))
            {
                var host = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
                paed001 = await db.Epas.Where(epa => epa.Code == "PAED-001" && epa.OwningInstitutionId == null).Select(epa => epa.Id).SingleAsync();
                paed002 = await db.Epas.Where(epa => epa.Code == "PAED-002" && epa.OwningInstitutionId == null).Select(epa => epa.Id).SingleAsync();
                levelId = await db.EntrustmentLevels.Select(level => level.Id).FirstAsync();

                var panel = new DecisionPanel
                {
                    Name = "T131 CCC",
                    Scope = DecisionPanelScope.Institution,
                    InstitutionId = host,
                    CreatedOn = DateTime.UtcNow,
                    Members = [new DecisionPanelMember { UserId = ChairUserId, Role = DecisionPanelMemberRole.Chair }]
                };
                var review = new CommitteeReview
                {
                    AcademicYear = 2026,
                    Semester = 1,
                    Panel = panel,
                    TraineeUserId = TraineeUserId,
                    ReviewPeriodFrom = new DateOnly(2026, 1, 1),
                    ReviewPeriodTo = new DateOnly(2026, 6, 30),
                    ScheduledOn = new DateOnly(2026, 7, 2)
                };
                db.CommitteeReviews.Add(review);
                await db.SaveChangesAsync();
                reviewId = review.Id;

                db.PendingEntrustmentDecisions.Add(Pending(reviewId, paed001, levelId));
                await db.SaveChangesAsync();
            }

            await using (var db = NewContext(schema))
            {
                db.PendingEntrustmentDecisions.Add(Pending(reviewId, paed001, levelId));
                var second = () => db.SaveChangesAsync();

                (await second.Should().ThrowAsync<DbUpdateException>()).Which.InnerException
                    .Should().BeOfType<PostgresException>().Which.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);
            }

            await using (var db = NewContext(schema))
            {
                db.PendingEntrustmentDecisions.Add(Pending(reviewId, paed002, levelId));
                await db.SaveChangesAsync();
                (await db.PendingEntrustmentDecisions.CountAsync(pending => pending.ReviewId == reviewId))
                    .Should().Be(2, "another EPA at the same review is its own decision");
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static PendingEntrustmentDecision Pending(int reviewId, int epaId, int levelId)
        => PendingEntrustmentDecision.Stage(
            reviewId, epaId, levelId, new DateOnly(2026, 7, 2), null, "Target met.", [1], ChairUserId, DateTime.UtcNow);

    private static Task<PendingEntrustmentDecisionDto> StageAsync(
        ApplicationDbContext db, int reviewId, int epaId, int levelId, params int[] evidenceItemIds)
        => new StagePendingEntrustmentDecisionCommandHandler(db, FakeUserDirectory.PanelMembersOf(db)).Handle(
            new StagePendingEntrustmentDecisionCommand(
                reviewId,
                null,
                epaId,
                levelId,
                new DateOnly(2026, 7, 2),
                null,
                "Target met.",
                evidenceItemIds,
                Chair()),
            CancellationToken.None);

    private async Task<int> RungAsync(string schema, int ladderId, string label)
    {
        await using var db = NewContext(schema);
        return await db.EntrustmentLevels
            .Where(level => level.ScaleId == ladderId && level.Label == label)
            .Select(level => level.Id)
            .SingleAsync();
    }

    private static ClaimsPrincipal Chair()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, ChairUserId),
                new Claim(ClaimTypes.Role, WombatRoles.CommitteeMember)
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    private async Task<string> SeededSchemaAsync()
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

        await using (var db = NewContext(schema))
        {
            await new PaediatricCatalogueSeeder(db).SeedAsync();
        }

        return schema;
    }

    private ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema)).Options);
}
