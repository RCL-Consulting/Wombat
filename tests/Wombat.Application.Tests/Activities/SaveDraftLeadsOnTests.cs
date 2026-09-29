using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T342, R1: a draft save needs a move that leads on out of the activity's state, as well as a field to write. Over the
/// shipped seeds whose reviewing state declares no <c>editable_by</c>, so that the author's default <c>subject|creator</c>
/// still gives her fields there: the demo <c>teaching_session</c>, <c>qi_project</c>, <c>research_output</c> and
/// <c>reflective_note</c> in <c>submitted</c>, and the legacy <c>acat</c>, <c>cbd</c>, <c>dops</c> and <c>mini_cex</c> in
/// <c>requested</c>, where the author may only cancel. And the saves that must stay: every author's draft, a declined
/// reflective note (she may submit it again), the CPSA assessor's request and the legacy assessor's accepted one.
/// </summary>
public sealed class SaveDraftLeadsOnTests
{
    private const string TraineeId = "trainee-1";
    private const string AssessorId = "assessor-1";
    private const int InstitutionId = 10;
    private const int TypeId = 1;
    private const int ActivityId = 100;
    private const string StoredData = $$"""{ "assessor_user_id": "{{AssessorId}}" }""";

    private static readonly DateTimeOffset Now = new(2026, 9, 29, 8, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("teaching_session", "submitted", "Submitted")]
    [InlineData("qi_project", "submitted", "Submitted")]
    [InlineData("research_output", "submitted", "Submitted")]
    [InlineData("reflective_note", "submitted", "Submitted")]
    [InlineData("acat", "requested", "Requested")]
    [InlineData("cbd", "requested", "Requested")]
    [InlineData("dops", "requested", "Requested")]
    [InlineData("mini_cex", "requested", "Requested")]
    public async Task TheAuthor_CannotSave_WorkHandedToItsReviewer(string seed, string state, string label)
    {
        var options = await SeededAsync(seed, state);

        await using (var db = new ApplicationDbContext(options))
        {
            // Guard: she still has a field to write there, so only the leading-on check refuses her.
            var activity = await db.Activities.Include(entity => entity.ActivityType).SingleAsync();
            new FieldPermissionEvaluator().GetWritableFieldKeys(
                    Wombat.Domain.Activities.Schema.FormSchemaParser.Parse(ReadSeed(seed, "schema.json")),
                    Wombat.Domain.Activities.Workflow.WorkflowParser.Parse(ReadSeed(seed, "workflow.json")),
                    activity,
                    Principal(TraineeId))
                .Should().NotBeEmpty("the state declares no editable_by, so the author's default applies");
        }

        await using (var db = new ApplicationDbContext(options))
        {
            var attempt = async () => await Service(db).SaveDraftAsync(
                new SaveActivityDraftInput(ActivityId, TraineeId, Principal(TraineeId), "{}"));

            (await attempt.Should().ThrowAsync<InvalidOperationException>()).Which.Message
                .Should().Be($"You cannot change this activity while it is {label}.");

            db.ChangeTracker.Entries()
                .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .Should().BeEmpty("the audit pipeline's catch saves this context");
        }

        await using (var db = new ApplicationDbContext(options))
        {
            var stored = await db.Activities.AsNoTracking().SingleAsync();
            stored.UpdatedOn.Should().Be(Now.UtcDateTime.AddDays(-1), "nothing was written");
        }
    }

    [Theory]
    [InlineData("teaching_session", "draft", TraineeId)]
    [InlineData("qi_project", "draft", TraineeId)]
    [InlineData("research_output", "draft", TraineeId)]
    [InlineData("reflective_note", "draft", TraineeId)]
    [InlineData("reflective_note", "declined", TraineeId)]
    [InlineData("mini_cex", "accepted", AssessorId)]
    [InlineData("mini_cex_cpsa", "draft", TraineeId)]
    [InlineData("mini_cex_cpsa", "requested", AssessorId)]
    [InlineData("reflective_exercise_cpsa", "draft", TraineeId)]
    public async Task TheCaller_WithAMoveThatLeadsOn_StillSaves(string seed, string state, string callerId)
    {
        var options = await SeededAsync(seed, state);

        await using (var db = new ApplicationDbContext(options))
        {
            var saved = await Service(db).SaveDraftAsync(
                new SaveActivityDraftInput(ActivityId, callerId, Principal(callerId), "{}"));
            saved.CurrentState.Should().Be(state);
        }

        await using (var db = new ApplicationDbContext(options))
        {
            (await db.Activities.AsNoTracking().SingleAsync()).UpdatedOn.Should().Be(Now.UtcDateTime);
        }
    }

    [Fact]
    public async Task TheCpsaAssessor_SavesHisOwnFields_InRequested()
    {
        var options = await SeededAsync("mini_cex_cpsa", "requested");

        await using (var db = new ApplicationDbContext(options))
        {
            await Service(db).SaveDraftAsync(
                new SaveActivityDraftInput(ActivityId, AssessorId, Principal(AssessorId), """{ "strengths": "Clear." }"""));
        }

        await using (var db = new ApplicationDbContext(options))
        {
            using var data = JsonDocument.Parse((await db.Activities.AsNoTracking().SingleAsync()).DataJson);
            data.RootElement.GetProperty("strengths").GetString().Should().Be("Clear.");
        }
    }

    private static async Task<DbContextOptions<ApplicationDbContext>> SeededAsync(string seed, string state)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);
        NomineeSeed.AddUser(db, TraineeId, InstitutionId, WombatRoles.Trainee);
        NomineeSeed.AddUser(db, AssessorId, InstitutionId, WombatRoles.Assessor);

        var type = new ActivityType
        {
            Id = TypeId,
            Key = seed,
            Name = seed,
            Scope = ActivityScope.Global,
            Version = 1,
            SchemaJson = ReadSeed(seed, "schema.json"),
            WorkflowJson = ReadSeed(seed, "workflow.json"),
            CreditRulesJson = ReadSeed(seed, "credit.json"),
            DisplayFieldsJson = "[]",
            OwnerUserId = "system",
            CreatedOn = Now.UtcDateTime
        };
        type.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = TypeId,
            Version = 1,
            SchemaJson = type.SchemaJson,
            WorkflowJson = type.WorkflowJson,
            CreditRulesJson = type.CreditRulesJson,
            DisplayFieldsJson = "[]",
            PublishedByUserId = "system",
            PublishedOn = Now.UtcDateTime
        });
        db.ActivityTypes.Add(type);

        db.Activities.Add(new Activity
        {
            Id = ActivityId,
            ActivityTypeId = TypeId,
            SchemaVersion = 1,
            SubjectUserId = TraineeId,
            CreatedByUserId = TraineeId,
            CurrentState = state,
            DataJson = StoredData,
            InstitutionId = InstitutionId,
            CreatedOn = Now.UtcDateTime.AddDays(-1),
            UpdatedOn = Now.UtcDateTime.AddDays(-1),
            ObservedOn = DateOnly.FromDateTime(Now.UtcDateTime.AddDays(-2))
        });

        await db.SaveChangesAsync();
        return options;
    }

    private static string ReadSeed(string folder, string file)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", folder, file));

    private static ActivityService Service(ApplicationDbContext db)
        => new(db, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator(),
            new FixedClock(Now));

    private static ClaimsPrincipal Principal(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
