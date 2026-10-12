using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.Activities;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.CommitteeDecisions;

/// <summary>
/// T361 (flow 07; round 2 review 11) on a real PostgreSQL server: the Start dialog's count
/// (<see cref="GetStartPreviewQuery" />) is what Start then freezes, and it leaves out the campaign that closed in the window
/// unreleased (T173). The unit suites run on EF InMemory, which translates neither the window nor the campaigns'
/// close-day rule; here both run in SQL, and the preview stages nothing.
/// </summary>
/// <remarks>
/// The schema helpers follow <c>CommitteeEvidenceSnapshotPostgresTests</c>: a schema of the test's own, registered before
/// it is created and dropped in a <c>finally</c>, with <see cref="DisposeAsync" /> as a backstop.
/// </remarks>
public sealed class StartPreviewPostgresTests : IAsyncLifetime
{
    private const string TraineeUserId = "trainee-t361";
    private const string ChairUserId = "chair-t361";
    private const string MemberUserId = "member-t361";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task ThePreview_CountsWhatStartThenFreezes_AndTheCampaignItLeavesOut_OnPostgres()
    {
        try
        {
            var schema = await SeededSchemaAsync();
            int reviewId;

            await using (var db = NewContext(schema))
            {
                var host = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
                var curriculumId = await db.Curricula
                    .Where(entity => entity.Name == "Paediatric EPA Curriculum" && entity.Version == "11.1")
                    .Select(entity => entity.Id)
                    .SingleAsync();
                var paed001 = await db.Epas.Where(epa => epa.Code == "PAED-001" && epa.OwningInstitutionId == null).Select(epa => epa.Id).SingleAsync();
                var cca = await db.ActivityTypes.Where(type => type.Key == "cca_cpsa").Select(type => new { type.Id, type.Version }).SingleAsync();

                db.TraineeProfiles.Add(new TraineeProfile
                {
                    UserId = TraineeUserId,
                    InstitutionId = host,
                    CurriculumId = curriculumId,
                    ProgrammeStartDate = new DateOnly(2023, 2, 1),
                    ExpectedCompletionDate = new DateOnly(2027, 1, 31)
                });

                var panel = new DecisionPanel
                {
                    Name = "T361 CCC",
                    Scope = DecisionPanelScope.Institution,
                    InstitutionId = host,
                    CreatedOn = DateTime.UtcNow,
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
                    Semester = 2,
                    Panel = panel,
                    TraineeUserId = TraineeUserId,
                    ReviewPeriodFrom = new DateOnly(2026, 7, 1),
                    ReviewPeriodTo = new DateOnly(2026, 12, 31),
                    ScheduledOn = new DateOnly(2026, 10, 5)
                };
                db.CommitteeReviews.Add(review);

                // Three in the window, in two states; one the day before it.
                foreach (var (observed, state) in new[]
                         {
                             (new DateOnly(2026, 7, 1), "completed"),
                             (new DateOnly(2026, 9, 23), "completed"),
                             (new DateOnly(2026, 12, 31), "draft"),
                             (new DateOnly(2026, 6, 30), "completed")
                         })
                {
                    var filed = observed.ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc);
                    db.Activities.Add(new Activity
                    {
                        ActivityTypeId = cca.Id,
                        SchemaVersion = cca.Version,
                        InstitutionId = host,
                        SubjectUserId = TraineeUserId,
                        CreatedByUserId = TraineeUserId,
                        CurrentState = state,
                        DataJson = $$"""{ "epa_id": {{paed001.ToString(CultureInfo.InvariantCulture)}}, "assessor_user_id": "assessor-1", "overall_level": 3 }""",
                        EpaId = paed001,
                        CreatedOn = filed,
                        UpdatedOn = filed,
                        ObservedOn = observed,
                        ObservedOnSource = ObservationDateSource.Declared
                    });
                }

                var template = new MsfTemplate { Name = "T361 MSF" };
                db.MsfTemplates.Add(template);
                await db.SaveChangesAsync();
                reviewId = review.Id;

                db.MsfCampaigns.AddRange(
                    Campaign(template.Id, MsfCampaignState.Released, new DateTime(2026, 9, 10, 18, 0, 0, DateTimeKind.Utc)),
                    Campaign(template.Id, MsfCampaignState.UnderReview, new DateTime(2026, 9, 20, 18, 0, 0, DateTimeKind.Utc)),
                    Campaign(template.Id, MsfCampaignState.Released, new DateTime(2026, 6, 30, 18, 0, 0, DateTimeKind.Utc)));
                await db.SaveChangesAsync();
            }

            StartPreviewDto? preview;
            await using (var db = NewContext(schema))
            {
                preview = await new GetStartPreviewQueryHandler(db, FakeUserDirectory.PanelMembersOf(db)).Handle(
                    new GetStartPreviewQuery(reviewId, Chair()), CancellationToken.None);
            }

            preview.Should().Be(new StartPreviewDto(4, 3, 1, new DateOnly(2026, 7, 1), new DateOnly(2026, 12, 31), 1));

            await using (var db = NewContext(schema))
            {
                (await db.CommitteeEvidenceItems.CountAsync(item => item.ReviewId == reviewId)).Should().Be(0, "the preview stages nothing");
                var started = await new StartCommitteeReviewCommandHandler(db, FakeUserDirectory.PanelMembersOf(db)).Handle(
                    new StartCommitteeReviewCommand(reviewId, Chair()), CancellationToken.None);

                started.EvidenceItems.Should().HaveCount(preview!.ItemCount);
                started.EvidenceItems.Count(item => item.ActivityId is not null).Should().Be(preview.ActivityCount);
                started.EvidenceItems.Count(item => item.MsfCampaignId is not null).Should().Be(preview.CampaignReportCount);
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static MsfCampaign Campaign(int templateId, MsfCampaignState state, DateTime closed)
        => new()
        {
            SubjectUserId = TraineeUserId,
            TemplateId = templateId,
            CreatedByUserId = "coordinator-t361",
            CreatedOn = closed.AddDays(-30),
            OpensOn = DateOnly.FromDateTime(closed.AddDays(-30)),
            ClosesOn = DateOnly.FromDateTime(closed),
            State = state,
            OpenedOn = closed.AddDays(-30),
            ClosedOn = closed,
            ReleasedOn = state == MsfCampaignState.Released ? closed.AddDays(1) : null
        };

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
