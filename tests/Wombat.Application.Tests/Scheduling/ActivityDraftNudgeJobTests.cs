using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.DataRights;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling;
using Wombat.Infrastructure.Scheduling.Jobs;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Scheduling;

/// <summary>
/// <see cref="ActivityDraftNudgeJob" />: which trainees it reminds of drafts untouched for 14 days, and what it says it
/// skipped.
/// </summary>
/// <remarks>
/// T240: the reminder goes through the shared reminder policy, so it is not written to a deactivated account or to a
/// user who opted out of digest emails (the reminder counts as a digest), and every run logs one line counting whom it
/// reminded and whom it skipped, by reason. Each skip test has a trainee who IS reminded beside the one who is not, so
/// "nothing was sent" can never pass for the wrong reason.
/// </remarks>
public sealed class ActivityDraftNudgeJobTests
{
    private static readonly DateTime Now = new(2026, 9, 25, 7, 0, 0, DateTimeKind.Utc);

    private const int InstitutionId = 10;

    /// <summary>The trainee every skip test reminds beside the one it skips.</summary>
    private const string ActiveTraineeId = "trainee-active";

    [Fact]
    public async Task AStaleDraft_IsRemindedToItsSubject()
    {
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddTrainee(db, ActiveTraineeId);
            AddDraft(db, ActiveTraineeId, MiniCex, daysAgo: 20);
        });

        var logger = await RunAsync(provider);

        var sent = emailSender.To(ActiveTraineeId);
        sent.Subject.Should().Contain("draft");
        sent.TextBody.Should().Contain("Mini-CEX");
        Summary(logger).Should().Be(new DraftSummary(Reminded: 1, RemindedDrafts: 1));
    }

    [Fact]
    public async Task ADraftTouchedWithin14Days_IsNotReminded()
    {
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddTrainee(db, ActiveTraineeId);
            AddDraft(db, ActiveTraineeId, MiniCex, daysAgo: 2);
        });

        var logger = await RunAsync(provider);

        emailSender.Sent.Should().BeEmpty();
        Summary(logger).Should().Be(new DraftSummary());
    }

    [Fact]
    public async Task ATraineesStaleDrafts_AreListedInOneEmail()
    {
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddTrainee(db, ActiveTraineeId);
            AddDraft(db, ActiveTraineeId, MiniCex, daysAgo: 20);
            AddDraft(db, ActiveTraineeId, Dops, daysAgo: 18);
        });

        var logger = await RunAsync(provider);

        emailSender.To(ActiveTraineeId).TextBody.Should().Contain("Mini-CEX").And.Contain("DOPS");
        Summary(logger).Should().Be(new DraftSummary(Reminded: 1, RemindedDrafts: 2));
    }

    // ---- T240: the shared reminder policy -------------------------------------------------------------------

    [Fact]
    public async Task ADeactivatedTrainee_IsNotReminded_AndIsCountedAsDeactivated()
    {
        // An administrator's lock: indefinite, as UserAdministrationService writes it. Before T240 only a missing email
        // stopped this job.
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddTrainee(db, ActiveTraineeId);
            AddDraft(db, ActiveTraineeId, MiniCex, daysAgo: 20);
            NomineeSeed.AddUser(db, "trainee-deactivated", InstitutionId, UserDeactivation.IndefiniteLockoutEnd, WombatRoles.Trainee);
            AddDraft(db, "trainee-deactivated", MiniCex, daysAgo: 20);
        });

        var logger = await RunAsync(provider);

        emailSender.Recipients.Should().Equal(EmailOf(ActiveTraineeId));
        Summary(logger).Should().Be(new DraftSummary(Reminded: 1, RemindedDrafts: 1, Deactivated: 1));
    }

    [Fact]
    public async Task ATraineeLockedOutByFailedPasswords_IsStillReminded()
    {
        // Identity's brute-force lockout lifts itself after minutes; it is not a deactivation.
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddTrainee(db, ActiveTraineeId);
            AddDraft(db, ActiveTraineeId, MiniCex, daysAgo: 20);
            NomineeSeed.AddUser(db, "trainee-locked-out", InstitutionId, Now.AddMinutes(15), WombatRoles.Trainee);
            AddDraft(db, "trainee-locked-out", MiniCex, daysAgo: 20);
        });

        var logger = await RunAsync(provider);

        emailSender.Recipients.Should().BeEquivalentTo([EmailOf(ActiveTraineeId), EmailOf("trainee-locked-out")]);
        Summary(logger).Should().Be(new DraftSummary(Reminded: 2, RemindedDrafts: 2));
    }

    [Fact]
    public async Task ATraineeWhoOptedOutOfDigestEmails_IsNotReminded_AndIsCountedAsOptedOut()
    {
        // The draft reminder is a periodic, unsolicited summary, so the T026 objection flag covers it (T240).
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddTrainee(db, ActiveTraineeId);
            AddDraft(db, ActiveTraineeId, MiniCex, daysAgo: 20);
            AddTrainee(db, "trainee-opted-out").OptOutOfDigestEmails = true;
            AddDraft(db, "trainee-opted-out", MiniCex, daysAgo: 20);
        });

        var logger = await RunAsync(provider);

        emailSender.Recipients.Should().Equal(EmailOf(ActiveTraineeId));
        Summary(logger).Should().Be(new DraftSummary(Reminded: 1, RemindedDrafts: 1, OptedOut: 1));
    }

    [Fact]
    public async Task ALockedTraineeWhoHadAlsoOptedOut_IsCountedOnce_AsDeactivated()
    {
        // One person skipped is one count, under the reason about the account that says the most: the lock, asked
        // before the opt-out.
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddTrainee(db, ActiveTraineeId);
            AddDraft(db, ActiveTraineeId, MiniCex, daysAgo: 20);
            NomineeSeed.AddUser(db, "trainee-locked-opted-out", InstitutionId, UserDeactivation.IndefiniteLockoutEnd, WombatRoles.Trainee)
                .OptOutOfDigestEmails = true;
            AddDraft(db, "trainee-locked-opted-out", MiniCex, daysAgo: 20);
        });

        var logger = await RunAsync(provider);

        emailSender.Recipients.Should().Equal(EmailOf(ActiveTraineeId));
        Summary(logger).Should().Be(new DraftSummary(Reminded: 1, RemindedDrafts: 1, Deactivated: 1));
    }

    [Fact]
    public async Task AnErasedTrainee_IsNotReminded_AndTheirDrafts_AreCountedAsNamingNoAccount()
    {
        // As ErasureExecutor leaves it: the account deactivated, opted out, without an email and without roles, and the
        // drafts kept on the record under the pseudonym it writes into each activity's subject. The drafts no longer
        // name the account, so the reminder cannot reach it through them, and they are counted once, under "no such
        // account", not "deactivated" (T240 review).
        const string erasedId = "trainee-erased";
        var pseudonym = ErasureExecutor.GeneratePseudonym(erasedId, "test-salt");

        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddTrainee(db, ActiveTraineeId);
            AddDraft(db, ActiveTraineeId, MiniCex, daysAgo: 20);
            var erased = NomineeSeed.AddUser(db, erasedId, institutionId: null, UserDeactivation.IndefiniteLockoutEnd);
            erased.UserName = pseudonym;
            erased.NormalizedUserName = pseudonym.ToUpperInvariant();
            erased.Email = null;
            erased.NormalizedEmail = null;
            erased.FirstName = string.Empty;
            erased.LastName = string.Empty;
            erased.OptOutOfDigestEmails = true;
            AddDraft(db, pseudonym, MiniCex, daysAgo: 20);
            AddDraft(db, pseudonym, Dops, daysAgo: 30);
        });

        var logger = await RunAsync(provider);

        emailSender.Recipients.Should().Equal(EmailOf(ActiveTraineeId));
        Summary(logger).Should().Be(new DraftSummary(Reminded: 1, RemindedDrafts: 1, UnknownUser: 1));
    }

    [Fact]
    public async Task ASubjectNamingNoAccount_AndOneWithNoEmail_AreEachCounted()
    {
        // Before T240 both were a silent `continue`: the job could not say it had skipped anyone.
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddTrainee(db, ActiveTraineeId);
            AddDraft(db, ActiveTraineeId, MiniCex, daysAgo: 20);
            AddDraft(db, "no-such-user", MiniCex, daysAgo: 20);
            var unaddressed = AddTrainee(db, "trainee-no-email");
            unaddressed.Email = "   ";
            unaddressed.NormalizedEmail = "   ";
            AddDraft(db, "trainee-no-email", MiniCex, daysAgo: 20);
        });

        var logger = await RunAsync(provider);

        emailSender.Recipients.Should().Equal(EmailOf(ActiveTraineeId));
        Summary(logger).Should().Be(new DraftSummary(Reminded: 1, RemindedDrafts: 1, UnknownUser: 1, NoEmail: 1));
    }

    [Fact]
    public async Task ARunWithNoStaleDrafts_StillLogsItsOneLine()
    {
        // One line per run, whatever happened, as the other reminders log it.
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db => AddTrainee(db, ActiveTraineeId));

        var logger = await RunAsync(provider);

        emailSender.Sent.Should().BeEmpty();
        Summary(logger).Should().Be(new DraftSummary());
    }

    // ---- T283: what became of the reminders ----------------------------------------------------------------------

    [Fact]
    public async Task EachReminder_IsHandedOverKeyedForItsRun_AndTheRunCountsThoseNotDelivered()
    {
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, db =>
        {
            AddTrainee(db, ActiveTraineeId);
            AddDraft(db, ActiveTraineeId, MiniCex, daysAgo: 20);
            AddTrainee(db, "trainee-unreachable");
            AddDraft(db, "trainee-unreachable", MiniCex, daysAgo: 20);
        });

        var logger = await RunAsync(provider);

        emailSender.Sent.ShouldAllCarryOneRunKey();
        Summary(logger).Should().Be(new DraftSummary(Reminded: 2, RemindedDrafts: 2));

        await JobMailReports.ReportAsync(provider, emailSender.Sent, EmailOf("trainee-unreachable"));

        logger.Entries.Should().HaveCount(2);
        logger.Entries[0].Values.Should().ContainKey("RemindedCount", "the job line comes first");
        var line = logger.DeliveryLine();
        line.Level.Should().Be(LogLevel.Warning);
        line.Values["JobName"].Should().Be(nameof(ActivityDraftNudgeJob));
        line.Count("SentCount").Should().Be(1);
        line.Count("NotDeliveredCount").Should().Be(1);
    }

    // ---- helpers ----------------------------------------------------------------------------------------------

    private const int MiniCex = 1;
    private const int Dops = 2;

    private static (ServiceProvider Provider, RecordingEmailSender EmailSender) BuildServices()
    {
        var emailSender = new RecordingEmailSender();
        var dbName = Guid.NewGuid().ToString();

        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(dbName));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IEmailSender>(_ => emailSender);
        services.AddSingleton<ScheduledJobMailTally>();

        return (services.BuildServiceProvider(), emailSender);
    }

    private static async Task<CapturingLogger> RunAsync(ServiceProvider provider)
    {
        var logger = new CapturingLogger();
        await new ActivityDraftNudgeJob(provider.GetRequiredService<IServiceScopeFactory>())
            .ExecuteAsync(new ScheduledJobContext(Now, logger), CancellationToken.None);
        return logger;
    }

    /// <summary>Seeds the two activity types, then the fixture's own rows, in one save.</summary>
    private static async Task SeedAsync(ServiceProvider provider, Action<ApplicationDbContext> seed)
    {
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        db.ActivityTypes.AddRange(
            new ActivityType { Id = MiniCex, Key = "mini-cex", Name = "Mini-CEX", OwnerUserId = "admin", CreatedOn = Now },
            new ActivityType { Id = Dops, Key = "dops", Name = "DOPS", OwnerUserId = "admin", CreatedOn = Now });

        seed(db);
        await db.SaveChangesAsync();
    }

    private static WombatIdentityUser AddTrainee(ApplicationDbContext db, string userId)
        => NomineeSeed.AddUser(db, userId, InstitutionId, WombatRoles.Trainee);

    private static void AddDraft(ApplicationDbContext db, string subjectUserId, int activityTypeId, int daysAgo)
    {
        var updatedOn = Now.AddDays(-daysAgo);
        db.Activities.Add(new Activity
        {
            ActivityTypeId = activityTypeId,
            SchemaVersion = 1,
            SubjectUserId = subjectUserId,
            CreatedByUserId = subjectUserId,
            CurrentState = "draft",
            DataJson = "{}",
            CreatedOn = updatedOn,
            UpdatedOn = updatedOn
        });
    }

    private static string EmailOf(string userId) => RecordingEmailSender.EmailOf(userId);

    /// <summary>The run's one log line, read from its structured values rather than from the rendered text.</summary>
    private static DraftSummary Summary(CapturingLogger logger)
    {
        var entry = logger.OneLine();
        return new DraftSummary(
            Reminded: entry.Count("RemindedCount"),
            RemindedDrafts: entry.Count("RemindedDraftCount"),
            UnknownUser: entry.Count("UnknownUserCount"),
            Deactivated: entry.Count("DeactivatedCount"),
            OptedOut: entry.Count("OptedOutCount"),
            NoEmail: entry.Count("NoEmailCount"));
    }

    private sealed record DraftSummary(
        int Reminded = 0,
        int RemindedDrafts = 0,
        int UnknownUser = 0,
        int Deactivated = 0,
        int OptedOut = 0,
        int NoEmail = 0);
}
