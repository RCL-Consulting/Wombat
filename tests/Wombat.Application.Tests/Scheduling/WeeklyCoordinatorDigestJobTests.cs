using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.Activities;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling;
using Wombat.Infrastructure.Scheduling.Jobs;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Scheduling;

/// <summary>
/// <see cref="WeeklyCoordinatorDigestJob" />: each coordinator is mailed their own institution's trainees, campaigns and
/// reviews, and nobody else's. (T117)
/// </summary>
/// <remarks>
/// <para>
/// Until T117 the job built one body over every institution and mailed it to every coordinator. Every fixture here has
/// two institutions, A and B, each with a coordinator, and asserts what each coordinator's body lists section by section,
/// so a test that expects a row to be missing always has a row beside it that is present.
/// </para>
/// <para>
/// Each recipient's principal is built by the app's own claims factory (<see cref="WombatUserClaimsPrincipalFactory" />),
/// wired here as the Web host wires it, so the roles, institution and scopes the rules see are the ones seeded as rows.
/// </para>
/// </remarks>
public sealed class WeeklyCoordinatorDigestJobTests
{
    /// <summary>A Monday, 08:00 UTC: when the job's cron fires.</summary>
    private static readonly DateTime Now = new(2026, 9, 28, 8, 0, 0, DateTimeKind.Utc);

    private static readonly DateOnly Today = DateOnly.FromDateTime(Now);

    private const int InstitutionA = 10;
    private const int InstitutionB = 20;

    private const int PanelA = 1;
    private const int PanelB = 2;

    // Two programmes, each followed at both institutions: a speciality id is national (T113).
    private const int SurgerySpeciality = 1;
    private const int PaediatricsSpeciality = 2;
    private const int SurgerySubSpeciality = 11;
    private const int PaediatricsSubSpeciality = 21;
    private const int SurgeryCurriculum = 101;
    private const int PaediatricsCurriculum = 201;

    private const string CoordinatorA = "coord-a";
    private const string CoordinatorB = "coord-b";

    private const string TraineesAtRisk = "Trainees at risk:";
    private const string MsfCampaigns = "MSF campaigns needing review:";
    private const string ReviewsThisWeek = "Committee reviews this week:";

    [Fact]
    public async Task EachCoordinator_IsSentOnlyTheirOwnInstitutionsTrainees_CampaignsAndReviews()
    {
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddCoordinators(db);

            AddTrainee(db, "a-idle", "Aisha", "Idle", InstitutionA);
            AddTrainee(db, "a-busy", "Andile", "Busy", InstitutionA);
            AddActivity(db, "a-busy", daysAgo: 3);
            AddTrainee(db, "b-idle", "Bongani", "Idle", InstitutionB);
            AddTrainee(db, "b-busy", "Busisiwe", "Busy", InstitutionB);
            AddActivity(db, "b-busy", daysAgo: 3);

            AddCampaign(db, 101, "a-busy");
            AddCampaign(db, 201, "b-busy");

            AddPanels(db);
            AddReview(db, 11, PanelA, "a-idle", inDays: 2);
            AddReview(db, 21, PanelB, "b-idle", inDays: 3);
        });

        await RunAsync(provider);

        emailSender.Recipients.Should().BeEquivalentTo([EmailOf(CoordinatorA), EmailOf(CoordinatorB)]);

        var toA = emailSender.To(CoordinatorA);
        Section(toA, TraineesAtRisk).Should().Equal("Aisha Idle");
        Section(toA, MsfCampaigns).Should().Equal("Annual MSF (campaign #101)");
        Section(toA, ReviewsThisWeek).Should().Equal($"Aisha Idle on {Today.AddDays(2):yyyy-MM-dd}");

        var toB = emailSender.To(CoordinatorB);
        Section(toB, TraineesAtRisk).Should().Equal("Bongani Idle");
        Section(toB, MsfCampaigns).Should().Equal("Annual MSF (campaign #201)");
        Section(toB, ReviewsThisWeek).Should().Equal($"Bongani Idle on {Today.AddDays(3):yyyy-MM-dd}");

        // Nothing of the other institution's anywhere in either body, the HTML included.
        NamesNoneOf(toA, "Bongani", "Busisiwe", "#201");
        NamesNoneOf(toB, "Aisha", "Andile", "#101");
    }

    [Fact]
    public async Task ATrainee_IsListedWhereTheirPreferredProfileIs_NotWhereAPastOneWas()
    {
        // Thabo left A's programme and is on B's now. The resolver's preferred profile is the active one (T185), so he
        // is B's trainee. A join on "any profile at the institution" would list him to both coordinators.
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddCoordinators(db);
            AddTrainee(db, "a-idle", "Aisha", "Idle", InstitutionA);
            AddTrainee(db, "b-idle", "Bongani", "Idle", InstitutionB);

            AddTrainee(db, "moved", "Thabo", "Moved", InstitutionA, isActive: false);
            AddProfile(db, "moved", InstitutionB, PaediatricsCurriculum, isActive: true);
        });

        await RunAsync(provider);

        Section(emailSender.To(CoordinatorA), TraineesAtRisk).Should().Equal("Aisha Idle");
        Section(emailSender.To(CoordinatorB), TraineesAtRisk).Should().Equal("Bongani Idle", "Thabo Moved");
    }

    [Fact]
    public async Task ATraineeWithNoProfile_IsListedToNoCoordinator()
    {
        // No programme, so no institution to be at risk in. The old job listed every Trainee to every coordinator.
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddCoordinators(db);
            AddTrainee(db, "a-idle", "Aisha", "Idle", InstitutionA);
            AddTrainee(db, "b-idle", "Bongani", "Idle", InstitutionB);

            var homeless = NomineeSeed.AddUser(db, "no-profile", InstitutionA, WombatRoles.Trainee);
            homeless.FirstName = "Nomsa";
            homeless.LastName = "Nowhere";
        });

        await RunAsync(provider);

        Section(emailSender.To(CoordinatorA), TraineesAtRisk).Should().Equal("Aisha Idle");
        Section(emailSender.To(CoordinatorB), TraineesAtRisk).Should().Equal("Bongani Idle");
    }

    [Fact]
    public async Task AReview_IsListedByItsPanelsInstitution()
    {
        // A's trainee comes before B's panel. It is B's panel's review: B's coordinator opens it on the reviews page, and
        // A's does not (CommitteeDecisionAuthorization.WorksOnPanel), so the digest lists it to B only.
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddCoordinators(db);
            AddTrainee(db, "a-idle", "Aisha", "Idle", InstitutionA);
            AddTrainee(db, "b-idle", "Bongani", "Idle", InstitutionB);

            AddPanels(db);
            AddReview(db, 11, PanelA, "a-idle", inDays: 1);
            AddReview(db, 12, PanelB, "a-idle", inDays: 4);
        });

        await RunAsync(provider);

        Section(emailSender.To(CoordinatorA), ReviewsThisWeek).Should().Equal($"Aisha Idle on {Today.AddDays(1):yyyy-MM-dd}");
        Section(emailSender.To(CoordinatorB), ReviewsThisWeek).Should().Equal($"Aisha Idle on {Today.AddDays(4):yyyy-MM-dd}");
    }

    [Fact]
    public async Task ACoordinatorWithNoInstitution_IsSentNothing_AndIsCounted()
    {
        // They oversee nobody. The old job sent them the national roster; an empty digest would tell them "no items
        // requiring attention", which is not what the job knows.
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddCoordinators(db);
            NomineeSeed.AddUser(db, "coord-nowhere", institutionId: null, WombatRoles.Coordinator);
            AddTrainee(db, "a-idle", "Aisha", "Idle", InstitutionA);
            AddTrainee(db, "b-idle", "Bongani", "Idle", InstitutionB);
        });

        var logger = await RunAsync(provider);

        emailSender.Recipients.Should().BeEquivalentTo([EmailOf(CoordinatorA), EmailOf(CoordinatorB)]);
        Summary(logger).Should().Be(new DigestSummary(Sent: 2, NoInstitution: 1));
    }

    [Fact]
    public async Task ACoordinatorWithNoEmailAddress_IsSentNothing_AndIsCounted()
    {
        // Nothing to write to: one with no address and one with a blank one. The recipients are asserted exactly, so a
        // message handed over with a null or blank To would show as a third entry.
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddCoordinators(db);
            var unaddressed = NomineeSeed.AddUser(db, "coord-unaddressed", InstitutionA, WombatRoles.Coordinator);
            unaddressed.Email = null;
            unaddressed.NormalizedEmail = null;
            var blank = NomineeSeed.AddUser(db, "coord-blank", InstitutionA, WombatRoles.Coordinator);
            blank.Email = "   ";
            blank.NormalizedEmail = "   ";
            AddTrainee(db, "a-idle", "Aisha", "Idle", InstitutionA);
        });

        var logger = await RunAsync(provider);

        emailSender.Recipients.Should().BeEquivalentTo([EmailOf(CoordinatorA), EmailOf(CoordinatorB)]);
        Summary(logger).Should().Be(new DigestSummary(Sent: 2, NoEmail: 2));
    }

    [Fact]
    public async Task EachList_KeepsItsWindowAndItsState()
    {
        // The filters the job had before T117, each with a row just inside and just outside it. Inactive is nothing
        // filed since 30 days before the run, that instant included. A campaign waits on a review only while
        // UnderReview. A review is this week's from today to seven days on, both included, and only while Scheduled.
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddCoordinators(db);

            AddTrainee(db, "a-lapsed", "Amara", "Lapsed", InstitutionA);
            AddActivity(db, "a-lapsed", daysAgo: 31);
            AddTrainee(db, "a-edge", "Ayanda", "Edge", InstitutionA);
            AddActivity(db, "a-edge", daysAgo: 30);

            AddCampaign(db, 101, "a-edge");
            AddCampaign(db, 102, "a-edge", MsfCampaignState.Draft);
            AddCampaign(db, 103, "a-edge", MsfCampaignState.Open);
            AddCampaign(db, 104, "a-edge", MsfCampaignState.Closed);
            AddCampaign(db, 105, "a-edge", MsfCampaignState.Released);
            AddCampaign(db, 106, "a-edge", MsfCampaignState.Withdrawn);

            AddPanels(db);
            AddReview(db, 11, PanelA, "a-lapsed", inDays: 0);
            AddReview(db, 12, PanelA, "a-lapsed", inDays: 7);
            AddReview(db, 13, PanelA, "a-edge", inDays: -1);
            AddReview(db, 14, PanelA, "a-edge", inDays: 8);
            AddReview(db, 15, PanelA, "a-edge", inDays: 3).Start([], [], "chair-a", Now);
        });

        await RunAsync(provider);

        var toA = emailSender.To(CoordinatorA);
        Section(toA, TraineesAtRisk).Should().Equal("Amara Lapsed");
        Section(toA, MsfCampaigns).Should().Equal("Annual MSF (campaign #101)");
        Section(toA, ReviewsThisWeek).Should().Equal(
            $"Amara Lapsed on {Today:yyyy-MM-dd}",
            $"Amara Lapsed on {Today.AddDays(7):yyyy-MM-dd}");
        NamesNoneOf(toA, "Ayanda");
    }

    [Fact]
    public async Task ACoordinatorWhoHoldsTrainee_IsSentNothing_AndIsCounted()
    {
        // A registrar who also coordinates reads no peer's record (T185), and their own is on their own pages.
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddCoordinators(db);
            AddTrainee(db, "a-idle", "Aisha", "Idle", InstitutionA);
            AddTrainee(db, "registrar", "Rethabile", "Registrar", InstitutionA);
            NomineeSeed.AddRole(db, "registrar", WombatRoles.Coordinator);
        });

        var logger = await RunAsync(provider);

        emailSender.Recipients.Should().BeEquivalentTo([EmailOf(CoordinatorA), EmailOf(CoordinatorB)]);
        Section(emailSender.To(CoordinatorA), TraineesAtRisk).Should().Equal("Aisha Idle", "Rethabile Registrar");
        Summary(logger).Should().Be(new DigestSummary(Sent: 2, HoldsTrainee: 1));
    }

    [Fact]
    public async Task AnAdministratorWhoAlsoCoordinates_IsSentOnlyTheirInstitutionsDigest()
    {
        // The Administrator role reads every trainee, campaign and review in the country. The digest is an institution's,
        // so it holds every list to the recipient's institution whatever another role lends them.
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddCoordinators(db);
            NomineeSeed.AddUser(db, "admin-a", InstitutionA, WombatRoles.Administrator, WombatRoles.Coordinator);

            AddTrainee(db, "a-idle", "Aisha", "Idle", InstitutionA);
            AddTrainee(db, "b-idle", "Bongani", "Idle", InstitutionB);
            AddCampaign(db, 101, "a-idle");
            AddCampaign(db, 201, "b-idle");
            AddPanels(db);
            AddReview(db, 11, PanelA, "a-idle", inDays: 2);
            AddReview(db, 21, PanelB, "b-idle", inDays: 3);
        });

        await RunAsync(provider);

        var toAdmin = emailSender.To("admin-a");
        Section(toAdmin, TraineesAtRisk).Should().Equal("Aisha Idle");
        Section(toAdmin, MsfCampaigns).Should().Equal("Annual MSF (campaign #101)");
        Section(toAdmin, ReviewsThisWeek).Should().Equal($"Aisha Idle on {Today.AddDays(2):yyyy-MM-dd}");
        NamesNoneOf(toAdmin, "Bongani", "#201");
    }

    [Fact]
    public async Task ACoordinatorsSpecialityScopes_NeitherNarrowNorWidenTheirRoster()
    {
        // The Coordinator arm of the read ladder is the whole institution and reads no speciality claim (T182). A
        // coordinator whose account carries a Surgery scope is sent A's Paediatrics trainee, and not B's Surgery one.
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddCoordinators(db);
            db.UserSpecialityScopes.Add(new WombatIdentityUserSpecialityScope { UserId = CoordinatorA, SpecialityId = SurgerySpeciality });
            db.UserSubSpecialityScopes.Add(new WombatIdentityUserSubSpecialityScope { UserId = CoordinatorA, SubSpecialityId = SurgerySubSpeciality });

            AddTrainee(db, "a-paeds", "Aisha", "Paeds", InstitutionA, curriculumId: PaediatricsCurriculum);
            AddTrainee(db, "a-surgery", "Amahle", "Surgery", InstitutionA, curriculumId: SurgeryCurriculum);
            AddTrainee(db, "b-surgery", "Bongani", "Surgery", InstitutionB, curriculumId: SurgeryCurriculum);
        });

        await RunAsync(provider);

        Section(emailSender.To(CoordinatorA), TraineesAtRisk).Should().Equal("Aisha Paeds", "Amahle Surgery");
    }

    [Fact]
    public async Task ACoordinator_IsNotToldOfACampaignAboutThemselves()
    {
        // The campaign list keeps a caller off campaigns about themselves (MsfCampaignRules.WhereRunBy, T224). A
        // coordinator who holds a profile without the Trainee role is on their institution's roster, so only that rule
        // keeps their own campaign out of their digest.
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddCoordinators(db);
            AddProfile(db, CoordinatorA, InstitutionA, PaediatricsCurriculum, isActive: true);
            AddTrainee(db, "a-idle", "Aisha", "Idle", InstitutionA);

            AddCampaign(db, 101, "a-idle");
            AddCampaign(db, 102, CoordinatorA);
        });

        await RunAsync(provider);

        Section(emailSender.To(CoordinatorA), MsfCampaigns).Should().Equal("Annual MSF (campaign #101)");
    }

    // ---- T284: only a current trainee is inactive -------------------------------------------------------------

    [Fact]
    public async Task ATraineeWhoIsNotCurrent_IsNotListedAsInactive_ButTheirCampaignAndReviewUnderWayStillAre()
    {
        // Each has logged nothing in 30 days and is on A's roster (their record stays readable, T113). Before T284 the
        // locked trainee and the one who withdrew, keeping the role, were listed as inactive every Monday. A campaign
        // about one still waits on its review, and a review of the other already scheduled still happens.
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddCoordinators(db);
            AddTrainee(db, "a-idle", "Aisha", "Idle", InstitutionA);

            // An administrator's lock (UserAdministrationService.SetLockoutAsync writes this).
            AddTrainee(db, "a-locked", "Lerato", "Locked", InstitutionA).LockoutEnd = UserDeactivation.IndefiniteLockoutEnd;

            // Withdrew: the profile ended, the role kept.
            AddTrainee(db, "a-withdrew", "Wandile", "Withdrew", InstitutionA, isActive: false);

            // Locked out for minutes by wrong passwords: still a current trainee, so still followed up.
            AddTrainee(db, "a-mistyped", "Mpho", "Mistyped", InstitutionA).LockoutEnd = Now.AddMinutes(15);

            AddCampaign(db, 101, "a-locked");
            AddPanels(db);
            AddReview(db, 11, PanelA, "a-withdrew", inDays: 2);
        });

        await RunAsync(provider);

        var toA = emailSender.To(CoordinatorA);
        Section(toA, TraineesAtRisk).Should().Equal("Aisha Idle", "Mpho Mistyped");
        Section(toA, MsfCampaigns).Should().Equal("Annual MSF (campaign #101)");
        Section(toA, ReviewsThisWeek).Should().Equal($"Wandile Withdrew on {Today.AddDays(2):yyyy-MM-dd}");
    }

    // ---- T240: the shared reminder policy -------------------------------------------------------------------

    [Fact]
    public async Task ADeactivatedCoordinator_IsSentNothing_AndIsCounted()
    {
        // An administrator's lock: indefinite, as UserAdministrationService writes it. The account keeps its role and its
        // institution, so before T240 it was sent the Monday digest like any other.
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddCoordinators(db);
            NomineeSeed.AddUser(db, "coord-locked", InstitutionA, UserDeactivation.IndefiniteLockoutEnd, WombatRoles.Coordinator);
            AddTrainee(db, "a-idle", "Aisha", "Idle", InstitutionA);
        });

        var logger = await RunAsync(provider);

        emailSender.Recipients.Should().BeEquivalentTo([EmailOf(CoordinatorA), EmailOf(CoordinatorB)]);
        Summary(logger).Should().Be(new DigestSummary(Sent: 2, Deactivated: 1));
    }

    [Fact]
    public async Task ACoordinatorLockedOutByFailedPasswords_IsStillSentTheirDigest()
    {
        // Identity's brute-force lockout writes the same column minutes out and lifts itself. Treating it as a
        // deactivation would let anyone silence a coordinator's digest by typing five wrong passwords at their account.
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddCoordinators(db);
            NomineeSeed.AddUser(db, "coord-locked-out", InstitutionA, Now.AddMinutes(15), WombatRoles.Coordinator);
            AddTrainee(db, "a-idle", "Aisha", "Idle", InstitutionA);
        });

        var logger = await RunAsync(provider);

        emailSender.Recipients.Should().BeEquivalentTo(
            [EmailOf(CoordinatorA), EmailOf(CoordinatorB), EmailOf("coord-locked-out")]);
        Section(emailSender.To("coord-locked-out"), TraineesAtRisk).Should().Equal("Aisha Idle");
        Summary(logger).Should().Be(new DigestSummary(Sent: 3));
    }

    [Fact]
    public async Task ACoordinatorWhoOptedOutOfDigestEmails_IsSentNothing_AndIsCounted()
    {
        // This is the email the T026 objection flag is named after. Before T240 the job never read it.
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddCoordinators(db);
            NomineeSeed.AddUser(db, "coord-opted-out", InstitutionA, WombatRoles.Coordinator).OptOutOfDigestEmails = true;
            AddTrainee(db, "a-idle", "Aisha", "Idle", InstitutionA);
        });

        var logger = await RunAsync(provider);

        emailSender.Recipients.Should().BeEquivalentTo([EmailOf(CoordinatorA), EmailOf(CoordinatorB)]);
        Summary(logger).Should().Be(new DigestSummary(Sent: 2, OptedOut: 1));
    }

    [Fact]
    public async Task AReasonAboutTheAccount_IsCountedBeforeTheDigestsOwn()
    {
        // One reason each. A locked coordinator with no institution is counted as deactivated, and an opted-out one who
        // also holds Trainee as opted out: the shared policy is asked first, so every mailing job counts an account alike.
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddCoordinators(db);
            NomineeSeed.AddUser(db, "coord-locked-nowhere", institutionId: null, UserDeactivation.IndefiniteLockoutEnd, WombatRoles.Coordinator);
            AddTrainee(db, "registrar", "Rethabile", "Registrar", InstitutionA).OptOutOfDigestEmails = true;
            NomineeSeed.AddRole(db, "registrar", WombatRoles.Coordinator);
        });

        var logger = await RunAsync(provider);

        emailSender.Recipients.Should().BeEquivalentTo([EmailOf(CoordinatorA), EmailOf(CoordinatorB)]);
        Summary(logger).Should().Be(new DigestSummary(Sent: 2, Deactivated: 1, OptedOut: 1));
    }

    [Fact]
    public async Task ARunWithNoCoordinators_StillLogsItsOneLine()
    {
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db => AddTrainee(db, "a-idle", "Aisha", "Idle", InstitutionA));

        var logger = await RunAsync(provider);

        emailSender.Sent.Should().BeEmpty();
        Summary(logger).Should().Be(new DigestSummary());
    }

    // ---- T283: what became of the digests ------------------------------------------------------------------------

    [Fact]
    public async Task EachDigest_IsHandedOverKeyedForItsRun_AndTheRunCountsThoseNotDelivered()
    {
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddCoordinators(db);
            AddTrainee(db, "a-idle", "Aisha", "Idle", InstitutionA);
            AddTrainee(db, "b-idle", "Bongani", "Idle", InstitutionB);
        });

        var logger = await RunAsync(provider);

        emailSender.Sent.ShouldAllCarryOneRunKey();
        Summary(logger).Should().Be(new DigestSummary(Sent: 2));

        await JobMailReports.ReportAsync(provider, emailSender.Sent, EmailOf(CoordinatorB));

        logger.Entries.Should().HaveCount(2);
        logger.Entries[0].Values.Should().ContainKey("SentCount").And.NotContainKey("NotDeliveredCount");
        var line = logger.DeliveryLine();
        line.Level.Should().Be(LogLevel.Warning);
        line.Values["JobName"].Should().Be(nameof(WeeklyCoordinatorDigestJob));
        line.Count("SentCount").Should().Be(1);
        line.Count("NotDeliveredCount").Should().Be(1);
    }

    // ---- helpers ----------------------------------------------------------------------------------------------

    /// <summary>
    /// The job's dependencies, with Identity wired as the Web host wires it (DependencyInjection.AddInfrastructure):
    /// EF stores and <see cref="WombatUserClaimsPrincipalFactory" />, over an in-memory database.
    /// </summary>
    private static (ServiceProvider Provider, RecordingEmailSender EmailSender) BuildServices()
    {
        var emailSender = new RecordingEmailSender();
        var dbName = Guid.NewGuid().ToString();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(dbName));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IEmailSender>(_ => emailSender);
        services.AddSingleton<ScheduledJobMailTally>();
        services.AddIdentityCore<WombatIdentityUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddClaimsPrincipalFactory<WombatUserClaimsPrincipalFactory>();

        // The real user store, as the host registers it: whether a trainee is current is asked of it (T284).
        services.AddScoped<IUserAdministrationService, UserAdministrationService>();

        return (services.BuildServiceProvider(), emailSender);
    }

    private static async Task<CapturingLogger> RunAsync(ServiceProvider provider)
    {
        var logger = new CapturingLogger();
        await new WeeklyCoordinatorDigestJob(provider.GetRequiredService<IServiceScopeFactory>())
            .ExecuteAsync(new ScheduledJobContext(Now, logger), CancellationToken.None);
        return logger;
    }

    /// <summary>Seeds the programme structure every fixture shares, then the fixture's own rows, in one save.</summary>
    private static async Task SeedAsync(ServiceProvider provider, Action<ApplicationDbContext> seed)
    {
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        db.Specialities.AddRange(
            new Speciality { Id = SurgerySpeciality, CollegeId = 1, Name = "Surgery" },
            new Speciality { Id = PaediatricsSpeciality, CollegeId = 1, Name = "Paediatrics" });
        db.SubSpecialities.AddRange(
            new SubSpeciality { Id = SurgerySubSpeciality, SpecialityId = SurgerySpeciality, Name = "General Surgery" },
            new SubSpeciality { Id = PaediatricsSubSpeciality, SpecialityId = PaediatricsSpeciality, Name = "General Paediatrics" });
        db.Curricula.AddRange(
            new Curriculum { Id = SurgeryCurriculum, SubSpecialityId = SurgerySubSpeciality, Name = "Surgery", Version = "1", EffectiveFrom = new DateOnly(2025, 1, 1) },
            new Curriculum { Id = PaediatricsCurriculum, SubSpecialityId = PaediatricsSubSpeciality, Name = "Paediatrics", Version = "11.1", EffectiveFrom = new DateOnly(2025, 1, 1) });
        db.MsfTemplates.Add(new MsfTemplate { Id = 1, Name = "Annual MSF" });

        seed(db);
        await db.SaveChangesAsync();
    }

    private static void AddCoordinators(ApplicationDbContext db)
    {
        NomineeSeed.AddUser(db, CoordinatorA, InstitutionA, WombatRoles.Coordinator);
        NomineeSeed.AddUser(db, CoordinatorB, InstitutionB, WombatRoles.Coordinator);
    }

    /// <summary>A Trainee with one profile, at <paramref name="institutionId" />, and no activity unless one is added.</summary>
    private static WombatIdentityUser AddTrainee(
        ApplicationDbContext db,
        string userId,
        string firstName,
        string lastName,
        int institutionId,
        int curriculumId = PaediatricsCurriculum,
        bool isActive = true)
    {
        var trainee = NomineeSeed.AddUser(db, userId, institutionId, WombatRoles.Trainee);
        trainee.FirstName = firstName;
        trainee.LastName = lastName;
        AddProfile(db, userId, institutionId, curriculumId, isActive);
        return trainee;
    }

    private static void AddProfile(ApplicationDbContext db, string userId, int institutionId, int curriculumId, bool isActive)
        => db.TraineeProfiles.Add(new TraineeProfile
        {
            UserId = userId,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            IsActive = isActive,
            ProgrammeStartDate = new DateOnly(2025, 1, 1),
            ExpectedCompletionDate = new DateOnly(2029, 1, 1)
        });

    private static void AddActivity(ApplicationDbContext db, string subjectUserId, int daysAgo)
    {
        var createdOn = Now.AddDays(-daysAgo);
        db.Activities.Add(new Activity
        {
            ActivityTypeId = 1,
            SchemaVersion = 1,
            SubjectUserId = subjectUserId,
            CreatedByUserId = subjectUserId,
            CurrentState = "draft",
            DataJson = "{}",
            CreatedOn = createdOn,
            UpdatedOn = createdOn,
            ObservedOn = DateOnly.FromDateTime(createdOn)
        });
    }

    private static void AddCampaign(
        ApplicationDbContext db,
        int id,
        string subjectUserId,
        MsfCampaignState state = MsfCampaignState.UnderReview)
        => db.MsfCampaigns.Add(new MsfCampaign
        {
            Id = id,
            SubjectUserId = subjectUserId,
            TemplateId = 1,
            CreatedByUserId = "someone",
            CreatedOn = Now.AddDays(-40),
            OpensOn = Today.AddDays(-40),
            ClosesOn = Today.AddDays(-5),
            State = state,
            OpenedOn = Now.AddDays(-40),
            ClosedOn = Now.AddDays(-5)
        });

    private static void AddPanels(ApplicationDbContext db)
        => db.DecisionPanels.AddRange(
            new DecisionPanel { Id = PanelA, Name = "A's panel", Scope = DecisionPanelScope.Institution, InstitutionId = InstitutionA, CreatedOn = Now },
            new DecisionPanel { Id = PanelB, Name = "B's panel", Scope = DecisionPanelScope.Institution, InstitutionId = InstitutionB, CreatedOn = Now });

    /// <summary>A Scheduled review; the caller may move it on.</summary>
    private static CommitteeReview AddReview(ApplicationDbContext db, int id, int panelId, string traineeUserId, int inDays)
    {
        var review = new CommitteeReview
        {
            Id = id,
            AcademicYear = 2026,
            Semester = 2,
            PanelId = panelId,
            TraineeUserId = traineeUserId,
            ReviewPeriodFrom = new DateOnly(2026, 7, 1),
            ReviewPeriodTo = new DateOnly(2026, 12, 31),
            ScheduledOn = Today.AddDays(inDays)
        };
        db.CommitteeReviews.Add(review);
        return review;
    }

    private static string EmailOf(string userId) => RecordingEmailSender.EmailOf(userId);

    /// <summary>The items listed under one heading of a digest's text body, in order; empty when the heading is absent.</summary>
    private static IReadOnlyList<string> Section(EmailMessage message, string heading)
    {
        var lines = message.TextBody.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var start = Array.FindIndex(lines, line => line.Trim() == heading);
        if (start < 0)
        {
            return [];
        }

        return lines
            .Skip(start + 1)
            .TakeWhile(line => line.StartsWith("  - ", StringComparison.Ordinal))
            .Select(line => line[4..])
            .ToList();
    }

    private static void NamesNoneOf(EmailMessage message, params string[] fragments)
    {
        foreach (var fragment in fragments)
        {
            message.TextBody.Should().NotContain(fragment);
            message.HtmlBody.Should().NotContain(fragment);
        }
    }

    /// <summary>The run's one log line, read from its structured values rather than from the rendered text.</summary>
    private static DigestSummary Summary(CapturingLogger logger)
    {
        var entry = logger.OneLine();
        return new DigestSummary(
            Sent: entry.Count("SentCount"),
            Deactivated: entry.Count("DeactivatedCount"),
            OptedOut: entry.Count("OptedOutCount"),
            NoEmail: entry.Count("NoEmailCount"),
            HoldsTrainee: entry.Count("HoldsTraineeCount"),
            NoInstitution: entry.Count("NoInstitutionCount"));
    }

    private sealed record DigestSummary(
        int Sent = 0,
        int Deactivated = 0,
        int OptedOut = 0,
        int NoEmail = 0,
        int HoldsTrainee = 0,
        int NoInstitution = 0);
}
