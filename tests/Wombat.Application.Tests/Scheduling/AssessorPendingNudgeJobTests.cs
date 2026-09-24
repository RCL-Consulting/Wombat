using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.Activities;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling.Jobs;

namespace Wombat.Application.Tests.Scheduling;

/// <summary>
/// T102: <see cref="AssessorPendingNudgeJob" /> reads the workflow of the version each activity is PINNED to, as the
/// inbox does, never the type's live one.
/// </summary>
/// <remarks>
/// The nominee gate judged the pinned version's <c>field:</c> rules when the activity was filed, so the pinned
/// version's field is the one holding a vetted person. A republish that moves the assessor rule to another field must
/// not redirect the reminder for activities already in flight: the other field may be empty, or name someone the gate
/// never judged as the actor.
/// </remarks>
public sealed class AssessorPendingNudgeJobTests
{
    private const int ActivityTypeId = 1;

    private const string TraineeId = "trainee-1";
    private const string PinnedAssessorId = "assessor-v1";
    private const string LiveVersionAssessorId = "assessor-v2";

    [Fact]
    public async Task AnActivityPinnedToV1_IsNudgedThroughV1sAssessorField_EvenThoughTheLiveVersionNamesAnotherField()
    {
        // Both fields are filled with different people, so reading the live version would not merely skip the nudge;
        // it would email the wrong person. That is the failure this pins.
        var (provider, emailSender) = BuildServices();
        await SeedAsync(provider, (SchemaVersion: 1, DaysAgo: 10));

        await RunAsync(provider);

        var sent = SentMessages(emailSender);
        sent.Should().ContainSingle();
        sent[0].To.Should().Be($"{PinnedAssessorId}@test.local");
        sent[0].TextBody.Should().Contain("Mini-CEX").And.Contain("Thandi Trainee");
        sent.Should().NotContain(message => message.To == $"{LiveVersionAssessorId}@test.local");
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

        var sent = SentMessages(emailSender);
        sent.Select(message => message.To).Should().BeEquivalentTo([$"{PinnedAssessorId}@test.local", $"{LiveVersionAssessorId}@test.local"]);
        sent.Single(message => message.To == $"{PinnedAssessorId}@test.local").TextBody.Should().Contain("waiting 10 days").And.NotContain("waiting 8 days");
        sent.Single(message => message.To == $"{LiveVersionAssessorId}@test.local").TextBody.Should().Contain("waiting 8 days").And.NotContain("waiting 10 days");
    }

    // ---- helpers ----------------------------------------------------------------------------------------------

    private static (ServiceProvider Provider, Mock<IEmailSender> EmailSender) BuildServices()
    {
        var emailSender = new Mock<IEmailSender>();
        var dbName = Guid.NewGuid().ToString();

        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(dbName));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped(_ => emailSender.Object);

        return (services.BuildServiceProvider(), emailSender);
    }

    private static Task RunAsync(ServiceProvider provider)
        => new AssessorPendingNudgeJob(provider.GetRequiredService<IServiceScopeFactory>())
            .ExecuteAsync(new ScheduledJobContext(DateTime.UtcNow, NullLogger.Instance), CancellationToken.None);

    private static List<EmailMessage> SentMessages(Mock<IEmailSender> emailSender)
        => emailSender.Invocations
            .Where(invocation => invocation.Method.Name == nameof(IEmailSender.SendAsync))
            .Select(invocation => (EmailMessage)invocation.Arguments[0])
            .ToList();

    private static async Task SeedAsync(ServiceProvider provider, params (int SchemaVersion, int DaysAgo)[] activities)
    {
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var publishedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var activityType = new ActivityType
        {
            Id = ActivityTypeId,
            Key = "mini_cex_republished",
            Name = "Mini-CEX",
            Scope = ActivityScope.Institution,
            ScopeId = 10,
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

        foreach (var (schemaVersion, daysAgo) in activities)
        {
            var updatedOn = DateTime.UtcNow.AddDays(-daysAgo).AddHours(-1);
            db.Activities.Add(new Activity
            {
                ActivityTypeId = ActivityTypeId,
                SchemaVersion = schemaVersion,
                SubjectUserId = TraineeId,
                CreatedByUserId = TraineeId,
                CurrentState = "requested",
                DataJson = $$"""{ "assessor_user_id": "{{PinnedAssessorId}}", "supervisor_user_id": "{{LiveVersionAssessorId}}" }""",
                InstitutionId = 10,
                CreatedOn = updatedOn,
                UpdatedOn = updatedOn
            });
        }

        db.Users.AddRange(
            User(TraineeId, "Thandi", "Trainee"),
            User(PinnedAssessorId, "Pinned", "Assessor"),
            User(LiveVersionAssessorId, "Live", "Assessor"));

        await db.SaveChangesAsync();
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
