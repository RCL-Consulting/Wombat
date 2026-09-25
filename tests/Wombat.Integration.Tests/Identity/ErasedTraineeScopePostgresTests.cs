using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
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
/// profile moved to a pseudonym that no account row holds and still active, and the account itself kept under its old
/// id with every role taken away.
/// </para>
/// <para>
/// Isolated the way <c>SsoErasurePostgresTests</c> is: a migrated schema of its own (<c>it_&lt;guid&gt;</c>), registered
/// before it is created and dropped in a finally and again on dispose. The v11.1 catalogue is seeded, as
/// <c>EntrustmentDecisionsDuePostgresTests</c> seeds it, for the curriculum a trainee profile names.
/// </para>
/// </remarks>
public sealed class ErasedTraineeScopePostgresTests : IAsyncLifetime
{
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";

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

    private readonly List<string> _schemas = [];
    private string _baseConnectionString = null!;

    public Task InitializeAsync()
    {
        _baseConnectionString = ResolveBaseConnectionString();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => DropSchemasAsync();

    [Fact]
    public async Task OnPostgres_AnErasedTrainee_IsScheduledForNoReview_AndMadeTheSubjectOfNoCampaign_ByEitherId()
    {
        var schema = $"it_{Guid.NewGuid():N}";
        _schemas.Add(schema);

        try
        {
            await using var root = await MigratedAndSeededServicesAsync(schema);

            int hostId, panelId, templateId;
            await using (var arrange = root.CreateAsyncScope())
            {
                var db = arrange.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                hostId = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
                var curriculumId = await db.Curricula
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
                    .Should().BeTrue("an erasure moves the profile to the pseudonym and leaves it active");
                (await db.Users.AnyAsync(user => user.Id == pseudonym)).Should().BeFalse("no account holds the pseudonym");
                (await db.UserRoles.AnyAsync(link => link.UserId == ErasedUserId)).Should().BeFalse("the account keeps no role");
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
            await DropSchemasAsync();
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
        await using (var connection = new NpgsqlConnection(_baseConnectionString))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"CREATE SCHEMA \"{schema}\"";
            await command.ExecuteNonQueryAsync();
        }

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(SchemaConnectionString(schema)));
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

    private string SchemaConnectionString(string schema)
        => new NpgsqlConnectionStringBuilder(_baseConnectionString) { SearchPath = schema, Pooling = false }.ConnectionString;

    private async Task DropSchemasAsync()
    {
        if (_schemas.Count == 0)
        {
            return;
        }

        await using var connection = new NpgsqlConnection(_baseConnectionString);
        await connection.OpenAsync();

        foreach (var schema in _schemas.ToList())
        {
            if (schema.StartsWith("it_", StringComparison.Ordinal))
            {
                await using var drop = connection.CreateCommand();
                drop.CommandText = $"DROP SCHEMA IF EXISTS \"{schema}\" CASCADE";
                await drop.ExecuteNonQueryAsync();
            }

            _schemas.Remove(schema);
        }
    }

    /// <summary>The same resolution order as the other PostgreSQL tests.</summary>
    private static string ResolveBaseConnectionString()
    {
        var environmentConnectionString = Environment.GetEnvironmentVariable("WOMBAT_TEST_CONNECTION");
        if (!string.IsNullOrWhiteSpace(environmentConnectionString))
        {
            return environmentConnectionString;
        }

        var secretsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Microsoft",
            "UserSecrets",
            WombatWebUserSecretsId,
            "secrets.json");

        if (File.Exists(secretsPath))
        {
            using var document = JsonDocument.Parse(File.ReadAllText(secretsPath));
            if (document.RootElement.TryGetProperty("ConnectionStrings:DefaultConnection", out var property)
                && property.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(property.GetString()))
            {
                return property.GetString()!;
            }
        }

        return "Host=localhost;Port=5432;Database=wombat;Username=postgres;Password=postgres";
    }
}
