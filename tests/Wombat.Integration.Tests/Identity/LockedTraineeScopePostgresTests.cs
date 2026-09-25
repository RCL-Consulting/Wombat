using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.Identity;

/// <summary>
/// T268 on a real PostgreSQL server, through the real user store (<see cref="UserAdministrationService" />): a trainee
/// whose account an administrator has locked is not a current trainee. They are put before no panel, made the subject of
/// no feedback campaign, and offered by neither picker, until the lock is lifted; a brute-force lockout that lifts itself
/// after minutes takes nobody off the programme.
/// </summary>
/// <remarks>
/// <para>
/// Every handler test of the rule answers the account half from a fake directory's <c>IsDeactivated</c> flag. This is the
/// check that the store's one query (<see cref="UserAdministrationService.WhichActivelyHoldRoleAsync" />) translates the
/// deactivation threshold (<see cref="UserDeactivation" />) against what an administrator's lock actually writes
/// (<see cref="UserAdministrationService.SetLockoutAsync" />), and against a lockout a few minutes out.
/// </para>
/// <para>
/// Isolated the way <c>ErasedTraineeScopePostgresTests</c> is: a migrated schema of its own (<c>it_&lt;guid&gt;</c>) from
/// <see cref="TestSchemas" />, dropped in a finally and again on dispose, with the v11.1 catalogue seeded for the
/// curriculum a trainee profile names.
/// </para>
/// </remarks>
public sealed class LockedTraineeScopePostgresTests : IAsyncLifetime
{
    private const string LockedUserId = "trainee-t268-locked";
    private const string LockedOutForMinutesUserId = "trainee-t268-minutes";
    private const string ControlUserId = "trainee-t268-control";

    private const string NotSchedulable =
        "A review can only be scheduled on a panel of your institution that covers the trainee's programme, for a " +
        "trainee at that institution whose programme you oversee.";

    private const string NotCurrentTrainee =
        "Only a trainee in a programme now can be put before a panel: someone whose trainee profile is active, who " +
        "still holds the Trainee role, and who has not been locked out by an administrator.";

    private const string CampaignNotRunByCaller =
        "A multi-source feedback campaign can only be run for a trainee in a programme at your own institution.";

    private const string CampaignSubjectNotCurrent =
        "A multi-source feedback campaign can only be run for a trainee in a programme now: someone whose trainee " +
        "profile is active, who still holds the Trainee role, and who has not been locked out by an administrator.";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task OnPostgres_ALockedTrainee_IsScheduledForNoReview_AndMadeTheSubjectOfNoCampaign_UntilUnlocked()
    {
        var schema = await _schemas.CreateAsync();

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

                NomineeSeed.AddUser(db, LockedUserId, hostId, WombatRoles.Trainee);
                NomineeSeed.AddUser(db, ControlUserId, hostId, WombatRoles.Trainee);
                // What Identity's failed-password lockout writes: a time a few minutes out.
                NomineeSeed.AddUser(
                    db, LockedOutForMinutesUserId, hostId, DateTimeOffset.UtcNow.AddMinutes(15), WombatRoles.Trainee);
                foreach (var userId in new[] { LockedUserId, ControlUserId, LockedOutForMinutesUserId })
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
                    Name = "T268 CCC",
                    Scope = DecisionPanelScope.Institution,
                    InstitutionId = hostId,
                    CreatedOn = DateTime.UtcNow,
                    Members =
                    [
                        new DecisionPanelMember { UserId = "chair-t268", Role = DecisionPanelMemberRole.Chair },
                        new DecisionPanelMember { UserId = "member-t268", Role = DecisionPanelMemberRole.Member }
                    ]
                };
                db.DecisionPanels.Add(panel);

                var template = new MsfTemplate
                {
                    Name = "T268 MSF",
                    Questions = [new MsfQuestion { Order = 1, Prompt = "Overall", Type = MsfQuestionType.Scale, Required = true }]
                };
                db.Set<MsfTemplate>().Add(template);

                await db.SaveChangesAsync();
                panelId = panel.Id;
                templateId = template.Id;
            }

            // An administrator's lock, through the real store: what the Users page's Lock button calls.
            await using (var lockScope = root.CreateAsyncScope())
            {
                await StoreIn(lockScope).SetLockoutAsync(LockedUserId, locked: true);
            }

            await using var act = root.CreateAsyncScope();
            var context = act.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var users = StoreIn(act);
            var coordinator = Coordinator(hostId);
            var administrator = Administrator();

            // Guard: the lock is a deactivation, the brute-force lockout is not, and all three still hold Trainee.
            (await users.GetByIdAsync(LockedUserId))!.IsDeactivated.Should().BeTrue();
            var minutes = (await users.GetByIdAsync(LockedOutForMinutesUserId))!;
            minutes.IsLockedOut.Should().BeTrue();
            minutes.IsDeactivated.Should().BeFalse();
            string[] asked = [LockedUserId, LockedOutForMinutesUserId, ControlUserId, "nobody-at-all"];
            (await users.WhichHoldRoleAsync(asked, WombatRoles.Trainee))
                .Should().BeEquivalentTo([LockedUserId, LockedOutForMinutesUserId, ControlUserId]);

            // The store's one query leaves out the locked account, and only it.
            (await users.WhichActivelyHoldRoleAsync(asked, WombatRoles.Trainee))
                .Should().BeEquivalentTo([LockedOutForMinutesUserId, ControlUserId]);

            foreach (var (caller, expected) in new[] { (coordinator, NotSchedulable), (administrator, NotCurrentTrainee) })
            {
                var schedule = () => ScheduleAsync(context, users, LockedUserId, panelId, caller);
                (await schedule.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(expected);
            }

            foreach (var (caller, expected) in new[]
                     {
                         (coordinator, CampaignNotRunByCaller), (administrator, CampaignSubjectNotCurrent)
                     })
            {
                var create = () => CreateCampaignAsync(context, users, LockedUserId, templateId, caller);
                (await create.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(expected);
            }

            // As the audit pipeline would, from its catch: each refusal came before anything was added.
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            (await CountsAsync(root)).Should().Be((0, 0));

            // Neither picker offers the locked trainee; each offers the other two, the one locked out for minutes included.
            (await new ListSchedulableTraineesQueryHandler(context, users).Handle(
                    new ListSchedulableTraineesQuery(panelId, coordinator), CancellationToken.None))
                .Select(trainee => trainee.UserId).Should().BeEquivalentTo([ControlUserId, LockedOutForMinutesUserId]);
            (await new ListMsfCampaignSubjectsQueryHandler(context, users).Handle(
                    new ListMsfCampaignSubjectsQuery(coordinator), CancellationToken.None))
                .Select(trainee => trainee.UserId).Should().BeEquivalentTo([ControlUserId, LockedOutForMinutesUserId]);

            // The lock was the reason: lifted, the same trainee is scheduled and a campaign about them is created.
            await users.SetLockoutAsync(LockedUserId, locked: false);
            context.ChangeTracker.Clear();

            await ScheduleAsync(context, users, LockedUserId, panelId, coordinator);
            await CreateCampaignAsync(context, users, LockedUserId, templateId, coordinator);

            await using var read = root.CreateAsyncScope();
            var fresh = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            (await fresh.CommitteeReviews.Select(review => review.TraineeUserId).ToListAsync()).Should().Equal(LockedUserId);
            (await fresh.MsfCampaigns.Select(campaign => campaign.SubjectUserId).ToListAsync()).Should().Equal(LockedUserId);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// T284 on the server: a draft written while the trainee was current is neither invited to nor opened once an
    /// administrator locks them, through the real store; lifted, the same draft opens and mails its respondent.
    /// </summary>
    [Fact]
    public async Task OnPostgres_ADraftAboutATraineeLockedSince_IsNeitherInvitedToNorOpened_UntilUnlocked()
    {
        var schema = await _schemas.CreateAsync();

        try
        {
            await using var root = await MigratedAndSeededServicesAsync(schema);

            int hostId, templateId;
            await using (var arrange = root.CreateAsyncScope())
            {
                var db = arrange.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                hostId = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
                var curriculumId = await db.Curricula
                    .Where(entity => entity.Name == "Paediatric EPA Curriculum" && entity.Version == "11.1")
                    .Select(entity => entity.Id)
                    .SingleAsync();

                var trainee = NomineeSeed.AddUser(db, LockedUserId, hostId, WombatRoles.Trainee);
                trainee.FirstName = "Lerato";
                trainee.LastName = "Locked";
                db.TraineeProfiles.Add(new TraineeProfile
                {
                    UserId = LockedUserId,
                    InstitutionId = hostId,
                    CurriculumId = curriculumId,
                    ProgrammeStartDate = new DateOnly(2025, 1, 15),
                    ExpectedCompletionDate = new DateOnly(2029, 1, 14)
                });

                var template = new MsfTemplate
                {
                    Name = "T284 MSF",
                    Questions = [new MsfQuestion { Order = 1, Prompt = "Overall", Type = MsfQuestionType.Scale, Required = true }]
                };
                db.Set<MsfTemplate>().Add(template);
                await db.SaveChangesAsync();
                templateId = template.Id;
            }

            var coordinator = Coordinator(hostId);
            var emailSender = new CapturingEmailSender();

            // The draft and its first invitee, while the trainee is current.
            int campaignId;
            await using (var draft = root.CreateAsyncScope())
            {
                var context = draft.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var users = StoreIn(draft);
                campaignId = (await CreateCampaignAsync(context, users, LockedUserId, templateId, coordinator)).Id;
                await InviteAsync(context, users, campaignId, "nurse-t284@example.test", coordinator);
            }

            await using (var lockScope = root.CreateAsyncScope())
            {
                await StoreIn(lockScope).SetLockoutAsync(LockedUserId, locked: true);
            }

            await using (var act = root.CreateAsyncScope())
            {
                var context = act.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var users = StoreIn(act);

                var invite = () => InviteAsync(context, users, campaignId, "peer-t284@example.test", coordinator);
                (await invite.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(CampaignSubjectNotCurrent);

                var open = () => OpenAsync(context, users, emailSender, campaignId, coordinator);
                (await open.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(CampaignSubjectNotCurrent);

                // As the audit pipeline would, from its catch: each refusal came before anything was touched.
                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();
            }

            emailSender.Sent.Should().BeEmpty();
            (await CampaignAsync(root, campaignId)).Should().Be((MsfCampaignState.Draft, 1, 0));

            // The lock was the reason: lifted, the same draft opens and mails its one respondent.
            await using (var reopen = root.CreateAsyncScope())
            {
                var context = reopen.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var users = StoreIn(reopen);
                await users.SetLockoutAsync(LockedUserId, locked: false);
                await OpenAsync(context, users, emailSender, campaignId, coordinator);
            }

            emailSender.Sent.Select(message => message.To).Should().Equal("nurse-t284@example.test");
            (await CampaignAsync(root, campaignId)).Should().Be((MsfCampaignState.Open, 1, 1));
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static Task<int> InviteAsync(
        ApplicationDbContext context, UserAdministrationService users, int campaignId, string email, ClaimsPrincipal caller)
        => new AddMsfInvitationCommandHandler(context, new InvitationTokenService(), users).Handle(
            new AddMsfInvitationCommand(campaignId, email, MsfRespondentCategory.Nurse, caller),
            CancellationToken.None);

    private static Task OpenAsync(
        ApplicationDbContext context, UserAdministrationService users, IEmailSender emailSender, int campaignId, ClaimsPrincipal caller)
        => new OpenMsfCampaignCommandHandler(
                context,
                emailSender,
                new InvitationTokenService(),
                users,
                Options.Create(new WombatOptions { MsfRespondUrl = "https://wombat.example/msf/respond" }))
            .Handle(new OpenMsfCampaignCommand(campaignId, caller), CancellationToken.None);

    /// <summary>The campaign's state, its invitees, and how many hold a link, read through a scope of their own.</summary>
    private static async Task<(MsfCampaignState State, int Invitees, int Linked)> CampaignAsync(ServiceProvider root, int campaignId)
    {
        await using var read = root.CreateAsyncScope();
        var db = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var campaign = await db.MsfCampaigns.AsNoTracking().Include(entity => entity.Invitations)
            .SingleAsync(entity => entity.Id == campaignId);
        return (campaign.State, campaign.Invitations.Count, campaign.Invitations.Count(invitation => invitation.TokenSelector != null));
    }

    private sealed class CapturingEmailSender : IEmailSender
    {
        public List<EmailMessage> Sent { get; } = [];

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            Sent.Add(message);
            return Task.CompletedTask;
        }
    }

    private static UserAdministrationService StoreIn(AsyncServiceScope scope)
        => new(
            scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>(),
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());

    private static Task<CommitteeReviewListItemDto> ScheduleAsync(
        ApplicationDbContext context, UserAdministrationService users, string traineeUserId, int panelId, ClaimsPrincipal caller)
        => new ScheduleCommitteeReviewCommandHandler(context, users).Handle(
            new ScheduleCommitteeReviewCommand(
                traineeUserId, panelId, 2026, 2, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31),
                new DateOnly(2026, 9, 25), caller),
            CancellationToken.None);

    private static Task<MsfCampaignSummaryDto> CreateCampaignAsync(
        ApplicationDbContext context, UserAdministrationService users, string subjectUserId, int templateId, ClaimsPrincipal caller)
        => new CreateMsfCampaignCommandHandler(context, new ActivityReferenceDataService(context), users).Handle(
            new CreateMsfCampaignCommand(
                subjectUserId,
                templateId,
                new DateOnly(2026, 9, 25),
                new DateOnly(2026, 10, 15),
                MinimumResponses: 1,
                MinimumCategoryResponses: 1,
                MinimumRespondentCategories: 1,
                EpaIds: [],
                CreatedByUserId: caller.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                caller),
            CancellationToken.None);

    /// <summary>Reviews and campaigns stored, read through a scope of their own so nothing tracked can mask a write.</summary>
    private static async Task<(int Reviews, int Campaigns)> CountsAsync(ServiceProvider root)
    {
        await using var read = root.CreateAsyncScope();
        var db = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return (await db.CommitteeReviews.CountAsync(), await db.MsfCampaigns.CountAsync());
    }

    private static ClaimsPrincipal Coordinator(int institutionId)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "coordinator-t268"),
                new Claim(ClaimTypes.Role, WombatRoles.Coordinator),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    private static ClaimsPrincipal Administrator()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "admin-t268"),
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
