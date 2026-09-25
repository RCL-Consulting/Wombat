using System.Data.Common;
using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.MultiSourceFeedback;

/// <summary>
/// T248 on a real PostgreSQL server, through the real user store (<see cref="UserAdministrationService" />): the campaign
/// form's trainee picker (<see cref="ListMsfCampaignSubjectsQuery" />) offers a current trainee and not an otherwise
/// identical one whose account does not hold Trainee, and the create command agrees with it on both. The erased
/// trainee's case is <c>ErasedTraineeScopePostgresTests</c> (T238).
/// </summary>
/// <remarks>
/// <para>
/// Every unit test of the picker answers the role links and the contacts from <c>FakeUserDirectory</c>. This is the check
/// that the real store answers them the same way: the Trainee half of a current trainee is read from the role links of
/// exactly the ids the profiles name (<c>TraineeScopeResolver.HoldersAsync</c>, T238), and the names and emails of the
/// offered trainees in one statement (<see cref="UserAdministrationService.GetContactsAsync" />), never the account of
/// every holder of Trainee in the country.
/// </para>
/// <para>
/// Isolated the way <c>DecisionPanelTraineeSeatPostgresTests</c> is: a migrated schema of its own
/// (<c>it_&lt;guid&gt;</c>), registered before it is created and dropped in a finally and again on dispose.
/// </para>
/// </remarks>
public sealed class MsfCampaignSubjectsPostgresTests : IAsyncLifetime
{
    /// <summary>An active profile at the host, on an account that holds Trainee.</summary>
    private const string Current = "t248-current";

    /// <summary>The same, but the account holds Assessor and not Trainee: a profile that outlived its user's role.</summary>
    private const string RoleRemoved = "t248-role-removed";

    /// <summary>A past profile at the host, on an account that still holds Trainee.</summary>
    private const string Completed = "t248-completed";

    /// <summary>An account that holds Trainee and has no profile anywhere: never asked about.</summary>
    private const string NotAdmitted = "t248-not-admitted";

    /// <summary>The one refusal a coordinator gets for any subject they may not start a campaign about (T238).</summary>
    private const string NotRunByCaller =
        "A multi-source feedback campaign can only be run for a trainee in a programme at your own institution.";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task OnPostgres_ThePickerOffersACurrentTrainee_NotOneWithoutTheRole_AndTheCreateAgrees_WithNothingWritten()
    {
        var schema = await _schemas.CreateAsync();
        var commands = new CommandLog();

        try
        {
            await using var root = await MigratedServicesAsync(schema, commands);

            int host;
            await using (var arrange = root.CreateAsyncScope())
            {
                var db = arrange.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await new DataSeeder(db).SeedAsync();

                host = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
                var curriculumId = await db.Curricula.OrderBy(entity => entity.Id).Select(entity => entity.Id).FirstAsync();

                Name(NomineeSeed.AddUser(db, Current, host, WombatRoles.Trainee), "Thandi", "Mokoena");
                Name(NomineeSeed.AddUser(db, RoleRemoved, host, WombatRoles.Assessor), "Nomsa", "Zulu");
                Name(NomineeSeed.AddUser(db, Completed, host, WombatRoles.Trainee), "Pieter", "Botha");
                Name(NomineeSeed.AddUser(db, NotAdmitted, host, WombatRoles.Trainee), "Kagiso", "Molefe");

                AddProfile(db, Current, host, curriculumId, isActive: true);
                AddProfile(db, RoleRemoved, host, curriculumId, isActive: true);
                AddProfile(db, Completed, host, curriculumId, isActive: false);

                db.MsfTemplates.Add(new MsfTemplate
                {
                    Name = "T248 MSF",
                    Questions = [new MsfQuestion { Order = 1, Prompt = "Overall", Type = MsfQuestionType.Scale, Required = true }]
                });

                await db.SaveChangesAsync();
            }

            await using var act = root.CreateAsyncScope();
            var context = act.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var users = new UserAdministrationService(
                act.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>(), context);
            var coordinator = Coordinator(host);

            // The real store's contacts lookup: exactly the users asked about, in one statement.
            commands.Texts.Clear();
            (await users.GetContactsAsync([Current, "t248-nobody"])).Should().BeEquivalentTo(
                new Dictionary<string, UserContact>
                {
                    [Current] = new(Current, "Thandi", "Mokoena", $"{Current}@test.local")
                },
                "a user who does not exist is left out, and nobody not asked about is read");
            commands.Texts.Should().ContainSingle("one round trip for every id, not one per user")
                .Which.Should().Contain("FROM \"AspNetUsers\"").And.Contain("WHERE");

            var offered = await new ListMsfCampaignSubjectsQueryHandler(context, users).Handle(
                new ListMsfCampaignSubjectsQuery(coordinator), CancellationToken.None);

            offered.Select(trainee => (trainee.UserId, trainee.FirstName, trainee.LastName, trainee.Email)).Should().Equal(
                [(Current, "Thandi", "Mokoena", $"{Current}@test.local")],
                "the one current trainee is offered, by the name and email on their account; the profile whose user lost " +
                "Trainee is not, nor the completed one, nor the Trainee account with no profile");

            var templateId = await context.MsfTemplates.Select(template => template.Id).SingleAsync();

            foreach (var refused in new[] { RoleRemoved, Completed, NotAdmitted })
            {
                var create = () => CreateAsync(context, users, templateId, refused, coordinator);

                (await create.Should().ThrowAsync<UnauthorizedAccessException>(refused))
                    .Which.Message.Should().Be(NotRunByCaller);

                // As the audit pipeline would, from its catch: the refusal came before anything was added.
                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();
            }

            await using (var read = root.CreateAsyncScope())
            {
                (await read.ServiceProvider.GetRequiredService<ApplicationDbContext>().MsfCampaigns.CountAsync())
                    .Should().Be(0, "no refused create stored a campaign");
            }

            // The control: the trainee the picker offered is accepted, through the same real store.
            var created = await CreateAsync(context, users, templateId, Current, coordinator);

            await using (var read = root.CreateAsyncScope())
            {
                (await read.ServiceProvider.GetRequiredService<ApplicationDbContext>().MsfCampaigns
                        .Select(campaign => campaign.SubjectUserId)
                        .SingleAsync())
                    .Should().Be(Current);
            }

            created.SubjectUserId.Should().Be(Current);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static Task<MsfCampaignSummaryDto> CreateAsync(
        ApplicationDbContext db,
        IUserAdministrationService users,
        int templateId,
        string subjectUserId,
        ClaimsPrincipal principal)
        => new CreateMsfCampaignCommandHandler(db, new ActivityReferenceDataService(db), users).Handle(
            new CreateMsfCampaignCommand(
                subjectUserId,
                templateId,
                new DateOnly(2026, 10, 1),
                new DateOnly(2026, 10, 15),
                MinimumResponses: 1,
                MinimumCategoryResponses: 1,
                MinimumRespondentCategories: 1,
                EpaIds: [],
                CreatedByUserId: principal.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                principal),
            CancellationToken.None);

    private static void AddProfile(ApplicationDbContext db, string userId, int institutionId, int curriculumId, bool isActive)
        => db.TraineeProfiles.Add(new TraineeProfile
        {
            UserId = userId,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            ProgrammeStartDate = new DateOnly(2025, 1, 1),
            ExpectedCompletionDate = new DateOnly(2029, 1, 1),
            IsActive = isActive
        });

    private static void Name(WombatIdentityUser user, string firstName, string lastName)
    {
        user.FirstName = firstName;
        user.LastName = lastName;
    }

    private static ClaimsPrincipal Coordinator(int institutionId)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, $"coordinator-{institutionId}"),
                new Claim(ClaimTypes.Role, WombatRoles.Coordinator),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    private async Task<ServiceProvider> MigratedServicesAsync(string schema, CommandLog commands)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options
            .UseNpgsql(TestDatabase.SchemaConnectionString(schema))
            .AddInterceptors(commands));
        services.AddIdentity<WombatIdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var root = services.BuildServiceProvider();

        await using (var migrationScope = root.CreateAsyncScope())
        {
            await migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
        }

        return root;
    }

    /// <summary>Every query EF sends through the context, so "one statement" is asserted on the wire.</summary>
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
