using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling.Jobs;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Scheduling;

/// <summary>
/// <see cref="AssessorPendingNudgeJob" />: whom it writes to about activities waiting on them, and what it says it skipped.
/// </summary>
/// <remarks>
/// <para>
/// T102: the job reads the workflow of the version each activity is PINNED to, as the inbox does, never the type's live
/// one. The nominee gate judged the pinned version's <c>field:</c> rules when the activity was filed, so the pinned
/// version's field is the one holding a vetted person. A republish that moves the assessor rule to another field must
/// not redirect the reminder for activities already in flight: the other field may be empty, or name someone the gate
/// never judged as the actor.
/// </para>
/// <para>
/// T151, D50: it does not write to a deactivated account or to a user who opted out of digest emails, and it does write
/// to a nominee who has since lost the Assessor role or moved institution, because T102 lets them complete what was
/// handed to them. Every run logs one line counting whom it nudged and whom it skipped, by reason. Each test below has a
/// nominee who IS written to beside the one who is not, so "nothing was sent" can never pass for the wrong reason.
/// </para>
/// </remarks>
public sealed class AssessorPendingNudgeJobTests
{
    private const int ActivityTypeId = 1;
    private const int InstitutionId = 10;

    private const string TraineeId = "trainee-1";
    private const string PinnedAssessorId = "assessor-v1";
    private const string LiveVersionAssessorId = "assessor-v2";

    /// <summary>The nominee every T151 test nudges beside the one it skips: active, an Assessor, at the activity's institution.</summary>
    private const string ActiveAssessorId = "assessor-active";

    [Fact]
    public async Task AnActivityPinnedToV1_IsNudgedThroughV1sAssessorField_EvenThoughTheLiveVersionNamesAnotherField()
    {
        // Both fields are filled with different people, so reading the live version would not merely skip the nudge;
        // it would email the wrong person. That is the failure this pins.
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, (SchemaVersion: 1, DaysAgo: 10));

        await RunAsync(provider);

        var sent = emailSender.Sent;
        sent.Should().ContainSingle();
        sent[0].To.Should().Be(EmailOf(PinnedAssessorId));
        sent[0].TextBody.Should().Contain("Mini-CEX").And.Contain("Thandi Trainee");
        sent.Should().NotContain(message => message.To == EmailOf(LiveVersionAssessorId));
    }

    [Fact]
    public async Task ActivitiesPinnedToDifferentVersions_AreEachNudgedThroughTheirOwnVersionsField()
    {
        // The control: the job reads the version per activity. Reading the live workflow for both would send both to the
        // v2 assessor; reading the first version for both would send both to the v1 assessor. Either way one person
        // would get two items and the other none.
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, (SchemaVersion: 1, DaysAgo: 10), (SchemaVersion: 2, DaysAgo: 8));

        await RunAsync(provider);

        var sent = emailSender.Sent;
        sent.Select(message => message.To).Should().BeEquivalentTo([EmailOf(PinnedAssessorId), EmailOf(LiveVersionAssessorId)]);
        sent.Single(message => message.To == EmailOf(PinnedAssessorId)).TextBody.Should().Contain("waiting 10 days").And.NotContain("waiting 8 days");
        sent.Single(message => message.To == EmailOf(LiveVersionAssessorId)).TextBody.Should().Contain("waiting 8 days").And.NotContain("waiting 10 days");
    }

    // ---- T151, D50: whom the nudge writes to ------------------------------------------------------------------

    [Fact]
    public async Task ADeactivatedNominee_IsNotWrittenTo_AndIsCountedAsDeactivated()
    {
        // An administrator's lock: indefinite, as UserAdministrationService writes it.
        var (provider, emailSender) = BuildServices();
        await SeedNomineesAsync(provider, [ActiveAssessorId, "assessor-deactivated"], db =>
        {
            NomineeSeed.AddUser(db, ActiveAssessorId, InstitutionId, WombatRoles.Assessor);
            NomineeSeed.AddUser(db, "assessor-deactivated", InstitutionId, UserDeactivation.IndefiniteLockoutEnd, WombatRoles.Assessor);
        });

        var logger = await RunAsync(provider);

        emailSender.Recipients.Should().Equal(EmailOf(ActiveAssessorId));
        Summary(logger).Should().Be(new NudgeSummary(Nudged: 1, NudgedActivities: 1, Deactivated: 1));
    }

    [Fact]
    public async Task ANomineeLockedOutByFailedPasswords_IsStillNudged()
    {
        // Identity's brute-force lockout writes the same column minutes out and lifts itself. Treating it as a
        // deactivation would let anyone silence an assessor's reminder by typing five wrong passwords at their account.
        var (provider, emailSender) = BuildServices();
        await SeedNomineesAsync(provider, [ActiveAssessorId, "assessor-locked-out"], db =>
        {
            NomineeSeed.AddUser(db, ActiveAssessorId, InstitutionId, WombatRoles.Assessor);
            NomineeSeed.AddUser(db, "assessor-locked-out", InstitutionId, DateTimeOffset.UtcNow.AddMinutes(15), WombatRoles.Assessor);
        });

        var logger = await RunAsync(provider);

        emailSender.Recipients.Should().BeEquivalentTo([EmailOf(ActiveAssessorId), EmailOf("assessor-locked-out")]);
        Summary(logger).Should().Be(new NudgeSummary(Nudged: 2, NudgedActivities: 2));
    }

    [Fact]
    public async Task ANomineeWhoOptedOutOfDigestEmails_IsNotWrittenTo_AndIsCountedAsOptedOut()
    {
        // The nudge is a digest (D50), so the T026 objection flag covers it.
        var (provider, emailSender) = BuildServices();
        await SeedNomineesAsync(provider, [ActiveAssessorId, "assessor-opted-out"], db =>
        {
            NomineeSeed.AddUser(db, ActiveAssessorId, InstitutionId, WombatRoles.Assessor);
            NomineeSeed.AddUser(db, "assessor-opted-out", InstitutionId, WombatRoles.Assessor).OptOutOfDigestEmails = true;
        });

        var logger = await RunAsync(provider);

        emailSender.Recipients.Should().Equal(EmailOf(ActiveAssessorId));
        Summary(logger).Should().Be(new NudgeSummary(Nudged: 1, NudgedActivities: 1, OptedOut: 1));
    }

    [Fact]
    public async Task AnErasedNominee_IsCountedOnce_AsDeactivated()
    {
        // An erasure leaves an account deactivated, opted out and without an email all at once (ErasureExecutor).
        // It is one person skipped, so it is one count, under the reason that says the most.
        var (provider, emailSender) = BuildServices();
        await SeedNomineesAsync(provider, [ActiveAssessorId, "assessor-erased"], db =>
        {
            NomineeSeed.AddUser(db, ActiveAssessorId, InstitutionId, WombatRoles.Assessor);
            var erased = NomineeSeed.AddUser(db, "assessor-erased", institutionId: null, UserDeactivation.IndefiniteLockoutEnd);
            erased.Email = null;
            erased.NormalizedEmail = null;
            erased.FirstName = string.Empty;
            erased.LastName = string.Empty;
            erased.OptOutOfDigestEmails = true;
        });

        var logger = await RunAsync(provider);

        emailSender.Recipients.Should().Equal(EmailOf(ActiveAssessorId));
        Summary(logger).Should().Be(new NudgeSummary(Nudged: 1, NudgedActivities: 1, Deactivated: 1));
    }

    [Fact]
    public async Task AnIdNamingNoAccount_IsCountedAsUnknown()
    {
        // Before T151 this was a silent `continue`: the job could not say it had skipped anyone.
        var (provider, emailSender) = BuildServices();
        await SeedNomineesAsync(provider, [ActiveAssessorId, "no-such-user"], db =>
            NomineeSeed.AddUser(db, ActiveAssessorId, InstitutionId, WombatRoles.Assessor));

        var logger = await RunAsync(provider);

        emailSender.Recipients.Should().Equal(EmailOf(ActiveAssessorId));
        Summary(logger).Should().Be(new NudgeSummary(Nudged: 1, NudgedActivities: 1, UnknownUser: 1));
    }

    [Fact]
    public async Task ANomineeWithoutAnEmailAddress_IsCountedAsHavingNone()
    {
        var (provider, emailSender) = BuildServices();
        await SeedNomineesAsync(provider, [ActiveAssessorId, "assessor-no-email"], db =>
        {
            NomineeSeed.AddUser(db, ActiveAssessorId, InstitutionId, WombatRoles.Assessor);
            var noEmail = NomineeSeed.AddUser(db, "assessor-no-email", InstitutionId, WombatRoles.Assessor);
            noEmail.Email = null;
            noEmail.NormalizedEmail = null;
        });

        var logger = await RunAsync(provider);

        emailSender.Recipients.Should().Equal(EmailOf(ActiveAssessorId));
        Summary(logger).Should().Be(new NudgeSummary(Nudged: 1, NudgedActivities: 1, NoEmail: 1));
    }

    [Fact]
    public async Task ANomineeWhoHasSinceLostTheAssessorRoleAndMovedInstitution_IsStillNudged()
    {
        // D50: the nominee gate judged them eligible when the activity was handed to them, and T102 lets them complete
        // it afterwards. They are the one person able to act, so re-reading NomineeDirectory here would silence the
        // reminder for exactly the activity that is stuck.
        var (provider, emailSender) = BuildServices();
        await SeedNomineesAsync(provider, [ActiveAssessorId, "assessor-moved"], db =>
        {
            NomineeSeed.AddUser(db, ActiveAssessorId, InstitutionId, WombatRoles.Assessor);
            NomineeSeed.AddUser(db, "assessor-moved", institutionId: 99, WombatRoles.Trainee);
        });

        var logger = await RunAsync(provider);

        emailSender.Recipients.Should().BeEquivalentTo([EmailOf(ActiveAssessorId), EmailOf("assessor-moved")]);
        emailSender.Sent.Single(message => message.To == EmailOf("assessor-moved")).TextBody.Should().Contain("Thandi Trainee");
        Summary(logger).Should().Be(new NudgeSummary(Nudged: 2, NudgedActivities: 2));
    }

    [Fact]
    public async Task SkipsAreCountedPerNominee_NotPerActivity()
    {
        // Two activities each wait on the deactivated nominee and on the active one. One person skipped is one count,
        // and the active nominee gets one email listing both.
        var (provider, emailSender) = BuildServices();
        await SeedNomineesAsync(provider, [ActiveAssessorId, ActiveAssessorId, "assessor-deactivated", "assessor-deactivated"], db =>
        {
            NomineeSeed.AddUser(db, ActiveAssessorId, InstitutionId, WombatRoles.Assessor);
            NomineeSeed.AddUser(db, "assessor-deactivated", InstitutionId, UserDeactivation.IndefiniteLockoutEnd, WombatRoles.Assessor);
        });

        var logger = await RunAsync(provider);

        emailSender.Recipients.Should().Equal(EmailOf(ActiveAssessorId));
        Summary(logger).Should().Be(new NudgeSummary(Nudged: 1, NudgedActivities: 2, Deactivated: 1));
    }

    [Fact]
    public async Task ARunWithNothingPending_StillLogsItsOneLine()
    {
        // One line per run, whatever happened: a quiet day must read apart from a day on which every nominee was skipped.
        var (provider, emailSender) = BuildServices();
        await SeedNomineesAsync(provider, [], db => NomineeSeed.AddUser(db, ActiveAssessorId, InstitutionId, WombatRoles.Assessor));

        var logger = await RunAsync(provider);

        emailSender.Sent.Should().BeEmpty();
        Summary(logger).Should().Be(new NudgeSummary());
    }

    // ---- helpers ----------------------------------------------------------------------------------------------

    private static (ServiceProvider Provider, RecordingEmailSender EmailSender) BuildServices()
    {
        var emailSender = new RecordingEmailSender();
        var dbName = Guid.NewGuid().ToString();

        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(dbName));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IEmailSender>(_ => emailSender);

        return (services.BuildServiceProvider(), emailSender);
    }

    private static async Task<CapturingLogger> RunAsync(ServiceProvider provider)
    {
        var logger = new CapturingLogger();
        await new AssessorPendingNudgeJob(provider.GetRequiredService<IServiceScopeFactory>())
            .ExecuteAsync(new ScheduledJobContext(DateTime.UtcNow, logger), CancellationToken.None);
        return logger;
    }

    private static string EmailOf(string userId) => $"{userId}@test.local";

    /// <summary>The run's one log line, read from its structured values rather than from the rendered text.</summary>
    private static NudgeSummary Summary(CapturingLogger logger)
    {
        var entry = logger.OneLine();
        return new NudgeSummary(
            Nudged: entry.Count("NudgedCount"),
            NudgedActivities: entry.Count("NudgedActivityCount"),
            UnknownUser: entry.Count("UnknownUserCount"),
            Deactivated: entry.Count("DeactivatedCount"),
            OptedOut: entry.Count("OptedOutCount"),
            NoEmail: entry.Count("NoEmailCount"));
    }

    private sealed record NudgeSummary(
        int Nudged = 0,
        int NudgedActivities = 0,
        int UnknownUser = 0,
        int Deactivated = 0,
        int OptedOut = 0,
        int NoEmail = 0);

    /// <summary>
    /// One activity per id in <paramref name="nomineeIds" />, each pinned to v1, waiting in <c>requested</c> on its
    /// <c>assessor_user_id</c>. The trainee is seeded; <paramref name="addUsers" /> seeds the nominees.
    /// </summary>
    private static async Task SeedNomineesAsync(
        ServiceProvider provider,
        IReadOnlyList<string> nomineeIds,
        Action<ApplicationDbContext> addUsers)
    {
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        AddActivityType(db);
        foreach (var nomineeId in nomineeIds)
        {
            AddPendingActivity(db, schemaVersion: 1, daysAgo: 10, $$"""{ "assessor_user_id": "{{nomineeId}}" }""");
        }

        var trainee = NomineeSeed.AddUser(db, TraineeId, InstitutionId, WombatRoles.Trainee);
        trainee.FirstName = "Thandi";
        trainee.LastName = "Trainee";
        addUsers(db);

        await db.SaveChangesAsync();
    }

    private static async Task SeedAsync(ServiceProvider provider, params (int SchemaVersion, int DaysAgo)[] activities)
    {
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        AddActivityType(db);
        foreach (var (schemaVersion, daysAgo) in activities)
        {
            AddPendingActivity(
                db,
                schemaVersion,
                daysAgo,
                $$"""{ "assessor_user_id": "{{PinnedAssessorId}}", "supervisor_user_id": "{{LiveVersionAssessorId}}" }""");
        }

        db.Users.AddRange(
            User(TraineeId, "Thandi", "Trainee"),
            User(PinnedAssessorId, "Pinned", "Assessor"),
            User(LiveVersionAssessorId, "Live", "Assessor"));

        await db.SaveChangesAsync();
    }

    private static void AddActivityType(ApplicationDbContext db)
    {
        var publishedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var activityType = new ActivityType
        {
            Id = ActivityTypeId,
            Key = "mini_cex_republished",
            Name = "Mini-CEX",
            Scope = ActivityScope.Institution,
            ScopeId = InstitutionId,
            // v2 is live: the type row carries its payload, as a publish leaves it.
            Version = 2,
            SchemaJson = SchemaJson,
            WorkflowJson = WorkflowNaming("supervisor_user_id"),
            CreditRulesJson = """{ "counts_for": [] }""",
            DisplayFieldsJson = "[]",
            OwnerUserId = "admin-1",
            CreatedOn = publishedOn
        };

        activityType.Versions.Add(Version(1, WorkflowNaming("assessor_user_id"), publishedOn));
        activityType.Versions.Add(Version(2, WorkflowNaming("supervisor_user_id"), publishedOn.AddDays(30)));
        db.ActivityTypes.Add(activityType);
    }

    private static void AddPendingActivity(ApplicationDbContext db, int schemaVersion, int daysAgo, string dataJson)
    {
        var updatedOn = DateTime.UtcNow.AddDays(-daysAgo).AddHours(-1);
        db.Activities.Add(new Activity
        {
            ActivityTypeId = ActivityTypeId,
            SchemaVersion = schemaVersion,
            SubjectUserId = TraineeId,
            CreatedByUserId = TraineeId,
            CurrentState = "requested",
            DataJson = dataJson,
            InstitutionId = InstitutionId,
            CreatedOn = updatedOn,
            UpdatedOn = updatedOn
        });
    }

    private static ActivityTypeVersion Version(int version, string workflowJson, DateTime publishedOn)
        => new()
        {
            ActivityTypeId = ActivityTypeId,
            Version = version,
            SchemaJson = SchemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = """{ "counts_for": [] }""",
            DisplayFieldsJson = "[]",
            PublishedByUserId = "admin-1",
            PublishedOn = publishedOn
        };

    private static WombatIdentityUser User(string id, string firstName, string lastName)
        => new()
        {
            Id = id,
            UserName = $"{id}@test.local",
            NormalizedUserName = $"{id}@TEST.LOCAL".ToUpperInvariant(),
            Email = $"{id}@test.local",
            NormalizedEmail = $"{id}@TEST.LOCAL".ToUpperInvariant(),
            FirstName = firstName,
            LastName = lastName
        };

    /// <summary>Both user fields exist in both versions; only which one the workflow's rules name differs.</summary>
    private const string SchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "assessor_user_id", "type": "user", "label": "Assessor" },
                { "key": "supervisor_user_id", "type": "user", "label": "Supervisor" }
              ]
            }
          ]
        }
        """;

    /// <summary>The CPSA shape: the named person completes or declines out of `requested`.</summary>
    private static string WorkflowNaming(string field) => $$"""
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested", "editable_by": "field:{{field}}" },
            { "key": "completed", "label": "Completed", "terminal": true },
            { "key": "declined", "label": "Declined" }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:{{field}}" },
            { "key": "decline", "from": "requested", "to": "declined", "actor": "field:{{field}}", "requires_note": true }
          ]
        }
        """;
}
