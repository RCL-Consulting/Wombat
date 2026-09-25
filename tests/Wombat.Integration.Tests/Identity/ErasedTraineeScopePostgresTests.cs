using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.DataRights;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.Identity;

/// <summary>
/// T238 on a real PostgreSQL server, after a real erasure (<see cref="ErasureExecutor" />) and through the real user
/// store (<see cref="UserAdministrationService" />): an erased trainee can be put before no panel and made the subject of
/// no feedback campaign, by their pseudonym or by the id they had, and no picker offers either.
/// </summary>
/// <remarks>
/// <para>
/// Every handler test of the rule fakes the erasure: it adds a <c>deleted_user_*</c> profile, and answers the account
/// half from a fake directory. This is the check that the rule holds against what an erasure actually leaves: the
/// profile moved to a pseudonym that no account row holds, and the account itself kept under its old id with every role
/// taken away. Since T258 the erasure also ends the profile, so an active one is put back under each id before the
/// checks: otherwise the profile half alone refuses both, and the account half goes untested.
/// </para>
/// <para>
/// Isolated the way <c>SsoErasurePostgresTests</c> is: a migrated schema of its own (<c>it_&lt;guid&gt;</c>), registered
/// before it is created and dropped in a finally and again on dispose. The v11.1 catalogue is seeded, as
/// <c>EntrustmentDecisionsDuePostgresTests</c> seeds it, for the curriculum a trainee profile names.
/// </para>
/// </remarks>
public sealed class ErasedTraineeScopePostgresTests : IAsyncLifetime
{
    private const string ErasedUserId = "trainee-t238-erased";
    private const string ControlUserId = "trainee-t238-control";

    private const string NotSchedulable =
        "A review can only be scheduled on a panel of your institution that covers the trainee's programme, for a " +
        "trainee at that institution whose programme you oversee.";

    private const string NotCurrentTrainee =
        "Only a trainee in a programme now can be put before a panel: someone whose trainee profile is active and who " +
        "still holds the Trainee role.";

    private const string CampaignNotRunByCaller =
        "A multi-source feedback campaign can only be run for a trainee in a programme at your own institution.";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task OnPostgres_AnErasedTrainee_IsScheduledForNoReview_AndMadeTheSubjectOfNoCampaign_ByEitherId()
    {
        var schema = await _schemas.CreateAsync();

        try
        {
            await using var root = await MigratedAndSeededServicesAsync(schema);

            int hostId, panelId, templateId, curriculumId;
            await using (var arrange = root.CreateAsyncScope())
            {
                var db = arrange.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                hostId = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
                curriculumId = await db.Curricula
                    .Where(entity => entity.Name == "Paediatric EPA Curriculum" && entity.Version == "11.1")
                    .Select(entity => entity.Id)
                    .SingleAsync();

                NomineeSeed.AddUser(db, ErasedUserId, hostId, WombatRoles.Trainee);
                NomineeSeed.AddUser(db, ControlUserId, hostId, WombatRoles.Trainee);
                foreach (var userId in new[] { ErasedUserId, ControlUserId })
                {
                    db.TraineeProfiles.Add(new TraineeProfile
                    {
                        UserId = userId,
                        InstitutionId = hostId,
                        CurriculumId = curriculumId,
                        ProgrammeStartDate = new DateOnly(2025, 1, 15),
                        ExpectedCompletionDate = new DateOnly(2029, 1, 14)
                    });
                }

                var panel = new DecisionPanel
                {
                    Name = "T238 CCC",
                    Scope = DecisionPanelScope.Institution,
                    InstitutionId = hostId,
                    CreatedOn = DateTime.UtcNow,
                    Members =
                    [
                        new DecisionPanelMember { UserId = "chair-t238", Role = DecisionPanelMemberRole.Chair },
                        new DecisionPanelMember { UserId = "member-t238", Role = DecisionPanelMemberRole.Member }
                    ]
                };
                db.DecisionPanels.Add(panel);

                var template = new MsfTemplate
                {
                    Name = "T238 MSF",
                    Questions = [new MsfQuestion { Order = 1, Prompt = "Overall", Type = MsfQuestionType.Scale, Required = true }]
                };
                db.Set<MsfTemplate>().Add(template);

                await db.SaveChangesAsync();
                panelId = panel.Id;
                templateId = template.Id;
            }

            string pseudonym;
            await using (var erase = root.CreateAsyncScope())
            {
                var db = erase.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var request = DataRightsRequest.Create(
                    ErasedUserId, "Erased Trainee", DataRightsRequestType.Erasure, "Leaving the programme.", DateTime.UtcNow);
                db.Set<DataRightsRequest>().Add(request);
                await db.SaveChangesAsync();

                pseudonym = (await erase.ServiceProvider.GetRequiredService<ErasureExecutor>()
                    .ExecuteAsync(request, "salt-for-tests", CancellationToken.None)).Pseudonym;
            }

            // Guard: the erasure left what the rule has to hold against.
            await using (var read = root.CreateAsyncScope())
            {
                var db = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                (await db.TraineeProfiles.SingleAsync(profile => profile.UserId == pseudonym)).IsActive
                    .Should().BeFalse("an erasure moves the profile to the pseudonym and ends it (T258)");
                (await db.Users.AnyAsync(user => user.Id == pseudonym)).Should().BeFalse("no account holds the pseudonym");
                (await db.UserRoles.AnyAsync(link => link.UserId == ErasedUserId)).Should().BeFalse("the account keeps no role");
            }

            // The account half of the rule, held against the real user store. Since T258 the erasure ends the profile, so
            // the profile half alone would refuse both ids and this test would pass with no account check at all (the T258
            // review, by mutation). So an active profile is put back under each id, the shape an erasure left before T258:
            // the pseudonym's, which no account holds, and one under the id they had, whose account holds no role. Only
            // the account half can refuse them now.
            await using (var stale = root.CreateAsyncScope())
            {
                var db = stale.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                (await db.Database.ExecuteSqlInterpolatedAsync(
                        $"UPDATE \"TraineeProfiles\" SET \"IsActive\" = TRUE, \"DeactivatedOn\" = NULL WHERE \"UserId\" = {pseudonym}"))
                    .Should().Be(1);
                db.TraineeProfiles.Add(new TraineeProfile
                {
                    UserId = ErasedUserId,
                    InstitutionId = hostId,
                    CurriculumId = curriculumId,
                    ProgrammeStartDate = new DateOnly(2025, 1, 15),
                    ExpectedCompletionDate = new DateOnly(2029, 1, 14)
                });
                await db.SaveChangesAsync();
            }

            await using var act = root.CreateAsyncScope();
            var context = act.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var users = new UserAdministrationService(
                act.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>(), context);
            var coordinator = Coordinator(hostId);
            var administrator = Administrator();

            foreach (var target in new[] { pseudonym, ErasedUserId })
            {
                foreach (var (caller, expected) in new[] { (coordinator, NotSchedulable), (administrator, NotCurrentTrainee) })
                {
                    var schedule = () => new ScheduleCommitteeReviewCommandHandler(context, users).Handle(
                        new ScheduleCommitteeReviewCommand(
                            target, panelId, 2026, 2, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31),
                            new DateOnly(2026, 9, 25), caller),
                        CancellationToken.None);

                    (await schedule.Should().ThrowAsync<UnauthorizedAccessException>(target))
                        .Which.Message.Should().Be(expected);
                }

                var create = () => new CreateMsfCampaignCommandHandler(context, new ActivityReferenceDataService(context), users)
                    .Handle(Campaign(target, templateId, coordinator), CancellationToken.None);

                (await create.Should().ThrowAsync<UnauthorizedAccessException>(target))
                    .Which.Message.Should().Be(CampaignNotRunByCaller);

                // As the audit pipeline would, from its catch: each refusal came before anything was added.
                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();
            }

            await using (var read = root.CreateAsyncScope())
            {
                var db = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                (await db.CommitteeReviews.CountAsync()).Should().Be(0);
                (await db.MsfCampaigns.CountAsync()).Should().Be(0);
            }

            // Neither picker offers either id; each offers the trainee who was not erased.
            (await new ListSchedulableTraineesQueryHandler(context, users).Handle(
                    new ListSchedulableTraineesQuery(panelId, coordinator), CancellationToken.None))
                .Select(trainee => trainee.UserId).Should().Equal(ControlUserId);
            (await new ListMsfCampaignSubjectsQueryHandler(context, users).Handle(
                    new ListMsfCampaignSubjectsQuery(coordinator), CancellationToken.None))
                .Select(trainee => trainee.UserId).Should().Equal(ControlUserId);

            // The control, through the same real store: the trainee who was not erased is scheduled, and a campaign about
            // them is created.
            await new ScheduleCommitteeReviewCommandHandler(context, users).Handle(
                new ScheduleCommitteeReviewCommand(
                    ControlUserId, panelId, 2026, 2, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31),
                    new DateOnly(2026, 9, 25), coordinator),
                CancellationToken.None);
            await new CreateMsfCampaignCommandHandler(context, new ActivityReferenceDataService(context), users)
                .Handle(Campaign(ControlUserId, templateId, coordinator), CancellationToken.None);

            await using (var read = root.CreateAsyncScope())
            {
                var db = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                (await db.CommitteeReviews.Select(review => review.TraineeUserId).ToListAsync()).Should().Equal(ControlUserId);
                (await db.MsfCampaigns.Select(campaign => campaign.SubjectUserId).ToListAsync()).Should().Equal(ControlUserId);
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static CreateMsfCampaignCommand Campaign(string subjectUserId, int templateId, ClaimsPrincipal caller)
        => new(
            subjectUserId,
            templateId,
            new DateOnly(2026, 9, 25),
            new DateOnly(2026, 10, 15),
            MinimumResponses: 1,
            MinimumCategoryResponses: 1,
            MinimumRespondentCategories: 1,
            EpaIds: [],
            CreatedByUserId: caller.FindFirst(ClaimTypes.NameIdentifier)!.Value,
            caller);

    private static ClaimsPrincipal Coordinator(int institutionId)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "coordinator-t238"),
                new Claim(ClaimTypes.Role, WombatRoles.Coordinator),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    private static ClaimsPrincipal Administrator()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "admin-t238"),
                new Claim(ClaimTypes.Role, WombatRoles.Administrator)
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    private async Task<ServiceProvider> MigratedAndSeededServicesAsync(string schema)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(TestDatabase.SchemaConnectionString(schema)));
        services.AddIdentity<WombatIdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        services.AddScoped<ErasureExecutor>();

        var root = services.BuildServiceProvider();

        await using (var scope = root.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
        }

        await using (var scope = root.CreateAsyncScope())
        {
            await new DataSeeder(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()).SeedAsync();
        }

        await using (var scope = root.CreateAsyncScope())
        {
            await new PaediatricCatalogueSeeder(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()).SeedAsync();
        }

        return root;
    }
}
