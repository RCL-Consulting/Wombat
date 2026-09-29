using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Queries.GetFileAgainSource;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T342, R6: File it again follows a decline only, meaning a dead end entered by a move that only the person the author
/// named may make (a <c>field:</c> arm and no arm of the author's). A legacy Mini-CEX cancelled by its assessor (the
/// cancel is <c>subject|field:assessor_user_id</c>) and a research output rejected by the SpecialityAdmin (a
/// <c>role:</c> arm) are not declines; the CPSA Mini-CEX declined by its assessor still is.
/// </summary>
public sealed class FileAgainDeclineTests
{
    private const string TraineeId = "trainee-1";
    private const string AssessorId = "assessor-1";
    private const string AdminId = "speciality-admin-1";
    private const int InstitutionId = 10;
    private const int TypeId = 1;
    private const int ActivityId = 100;

    private static readonly DateTime Now = new(2026, 9, 29, 8, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData("mini_cex", "requested", "cancelled", "cancel", AssessorId)]
    [InlineData("research_output", "submitted", "rejected", "reject", AdminId)]
    public async Task ACancelOrARejectionByAnyoneElse_IsNotADecline(
        string seed, string from, string to, string move, string moverId)
    {
        var options = await SeededAsync(seed, from, to, move, moverId);

        (await FileAgainAsync(options)).Should().BeNull();
    }

    [Theory]
    [InlineData("mini_cex_cpsa", "requested")]
    [InlineData("mini_cex", "requested")]
    public async Task ADeclineByTheNamedAssessor_IsStillCopied(string seed, string from)
    {
        var options = await SeededAsync(seed, from, "declined", "decline", AssessorId);

        var copy = await FileAgainAsync(options);

        copy.Should().NotBeNull();
        copy!.DeclinedByName.Should().Be("Fatima Khumalo");
    }

    private static async Task<FileAgainSourceDto?> FileAgainAsync(DbContextOptions<ApplicationDbContext> options)
    {
        await using var db = new ApplicationDbContext(options);
        var users = new Mock<IUserAdministrationService>();
        users.Setup(mock => mock.GetDisplayNamesAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<string> ids, CancellationToken _) =>
                ids.Where(id => id == AssessorId).ToDictionary(id => id, _ => "Fatima Khumalo"));

        return await new GetFileAgainSourceQueryHandler(
                db, new FieldPermissionEvaluator(), new ActivityReferenceDataService(db), users.Object)
            .Handle(new GetFileAgainSourceQuery(ActivityId, Principal(TraineeId)), CancellationToken.None);
    }

    private static async Task<DbContextOptions<ApplicationDbContext>> SeededAsync(
        string seed, string from, string to, string move, string moverId)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);
        NomineeSeed.AddUser(db, TraineeId, InstitutionId, WombatRoles.Trainee);
        NomineeSeed.AddUser(db, AssessorId, InstitutionId, WombatRoles.Assessor);
        NomineeSeed.AddUser(db, AdminId, InstitutionId, WombatRoles.SpecialityAdmin);

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
            CreatedOn = Now
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
            PublishedOn = Now
        });
        db.ActivityTypes.Add(type);

        var activity = new Activity
        {
            Id = ActivityId,
            ActivityTypeId = TypeId,
            SchemaVersion = 1,
            SubjectUserId = TraineeId,
            CreatedByUserId = TraineeId,
            CurrentState = to,
            DataJson = $$"""{ "assessor_user_id": "{{AssessorId}}" }""",
            InstitutionId = InstitutionId,
            CreatedOn = Now.AddDays(-2),
            UpdatedOn = Now,
            ObservedOn = DateOnly.FromDateTime(Now.AddDays(-3))
        };
        activity.Transitions.Add(new ActivityTransition
        {
            FromState = from, ToState = from, TransitionKey = "create", ActorUserId = TraineeId, OccurredOn = Now.AddDays(-2)
        });
        activity.Transitions.Add(new ActivityTransition
        {
            FromState = from, ToState = to, TransitionKey = move, ActorUserId = moverId, OccurredOn = Now, Note = "A note."
        });
        db.Activities.Add(activity);

        await db.SaveChangesAsync();
        return options;
    }

    private static string ReadSeed(string folder, string file)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", folder, file));

    private static ClaimsPrincipal Principal(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));
}
