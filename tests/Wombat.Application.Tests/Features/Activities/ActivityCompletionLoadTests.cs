using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Activities;

/// <summary>
/// <see cref="ActivityCompletion.LoadFinishedStatesAsync" /> resolves each pin the way the portfolio export and the
/// rated-evidence profiles do: the pinned version row, else the type's own columns (T203).
/// </summary>
public sealed class ActivityCompletionLoadTests
{
    [Fact]
    public async Task APinReadsItsOwnVersionRow_NotTheTypesCurrentWorkflow()
    {
        await using var db = CreateDb();
        db.ActivityTypes.Add(new ActivityType
        {
            Id = 1, Key = "reflective_exercise", Name = "Reflective exercise", Scope = ActivityScope.Global,
            Version = 2, WorkflowJson = FinishingWorkflows.ReflectiveExerciseWithSignOff
        });
        db.Set<ActivityTypeVersion>().AddRange(
            new ActivityTypeVersion { Id = 1, ActivityTypeId = 1, Version = 1, WorkflowJson = FinishingWorkflows.ReflectiveExercise },
            new ActivityTypeVersion { Id = 2, ActivityTypeId = 1, Version = 2, WorkflowJson = FinishingWorkflows.ReflectiveExerciseWithSignOff });
        await db.SaveChangesAsync();

        var finished = await ActivityCompletion.LoadFinishedStatesAsync(db, [(1, 1), (1, 2), (1, 1)]);

        finished.Should().HaveCount(2);
        finished[(1, 1)].Should().BeEquivalentTo("discussed");
        finished[(1, 2)].Should().BeEquivalentTo("signed_off");
    }

    [Fact]
    public async Task APinWithNoVersionRow_ReadsTheTypesOwnColumns_AndAGoneOrUnreadableTypeFinishesInCompleted()
    {
        await using var db = CreateDb();
        db.ActivityTypes.AddRange(
            new ActivityType
            {
                Id = 1, Key = "teaching_session", Name = "Teaching session", Scope = ActivityScope.Global,
                Version = 1, WorkflowJson = FinishingWorkflows.TeachingSession
            },
            new ActivityType
            {
                Id = 2, Key = "broken", Name = "Broken", Scope = ActivityScope.Global,
                Version = 1, WorkflowJson = """{ "states": "not a list" }"""
            });
        await db.SaveChangesAsync();

        var finished = await ActivityCompletion.LoadFinishedStatesAsync(db, [(1, 1), (2, 1), (99, 1)]);

        finished[(1, 1)].Should().BeEquivalentTo("accepted");
        finished[(2, 1)].Should().BeEquivalentTo(ActivityCompletion.NoWorkflowFinishedState);
        finished[(99, 1)].Should().BeEquivalentTo(ActivityCompletion.NoWorkflowFinishedState);
    }

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
