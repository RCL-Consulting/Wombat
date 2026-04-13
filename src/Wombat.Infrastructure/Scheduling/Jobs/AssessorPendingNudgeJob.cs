using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;
using Wombat.Infrastructure.Identity;

namespace Wombat.Infrastructure.Scheduling.Jobs;

public sealed class AssessorPendingNudgeJob : IScheduledJob
{
    private readonly IServiceScopeFactory _scopeFactory;

    public AssessorPendingNudgeJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public string Key => "assessor-pending-nudge";
    public string CronExpression => "0 9 * * *";
    public string Description => "Nudges assessors about activities waiting for their assessment for more than 5 days (daily at 09:00 UTC).";

    public async Task ExecuteAsync(ScheduledJobContext context, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var cutoff = context.UtcNow.AddDays(-5);

        var activities = await dbContext.Set<Activity>()
            .Include(a => a.ActivityType)
            .Where(a => a.UpdatedOn < cutoff)
            .Where(a => !a.ActivityType.WorkflowJson!.Contains("\"terminal\":true") || true)
            .ToListAsync(cancellationToken);

        var pendingItems = new List<(string AssessorUserId, string ActivityTypeName, string TraineeUserId, int DaysWaiting)>();

        foreach (var activity in activities)
        {
            if (string.IsNullOrWhiteSpace(activity.ActivityType.WorkflowJson))
                continue;

            Workflow workflow;
            try
            {
                workflow = WorkflowParser.Parse(activity.ActivityType.WorkflowJson);
            }
            catch
            {
                continue;
            }

            var currentState = workflow.States.FirstOrDefault(s => s.Key == activity.CurrentState);
            if (currentState is null || currentState.Terminal)
                continue;

            var outgoingTransitions = workflow.Transitions
                .Where(t => t.From.Contains(activity.CurrentState))
                .ToList();

            var assessorFieldTransition = outgoingTransitions
                .FirstOrDefault(t => HasFieldUserActor(t.Actor));

            if (assessorFieldTransition is null)
                continue;

            var fieldName = GetFieldName(assessorFieldTransition.Actor);
            if (fieldName is null)
                continue;

            var assessorUserId = ExtractFieldValue(activity.DataJson, fieldName);
            if (string.IsNullOrWhiteSpace(assessorUserId))
                continue;

            var daysWaiting = (int)(context.UtcNow - activity.UpdatedOn).TotalDays;
            pendingItems.Add((assessorUserId, activity.ActivityType.Name, activity.SubjectUserId, daysWaiting));
        }

        if (pendingItems.Count == 0)
        {
            context.Logger.LogInformation("AssessorPendingNudgeJob: no pending activities found.");
            return;
        }

        var grouped = pendingItems.GroupBy(p => p.AssessorUserId);
        var userIds = grouped.Select(g => g.Key)
            .Concat(pendingItems.Select(p => p.TraineeUserId))
            .Distinct()
            .ToList();

        var users = await dbContext.Set<WombatIdentityUser>()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Email, u.FirstName, u.LastName })
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        foreach (var group in grouped)
        {
            if (!users.TryGetValue(group.Key, out var assessor) || string.IsNullOrWhiteSpace(assessor.Email))
                continue;

            var items = group.Select(p =>
            {
                var traineeName = users.TryGetValue(p.TraineeUserId, out var trainee)
                    ? $"{trainee.FirstName} {trainee.LastName}"
                    : "Unknown trainee";
                return (p.ActivityTypeName, TraineeName: traineeName, p.DaysWaiting);
            }).ToList();

            var email = AssessorPendingNudgeEmail.Build(assessor.Email, assessor.FirstName, items);
            await emailSender.SendAsync(email, cancellationToken);
        }

        context.Logger.LogInformation("AssessorPendingNudgeJob: sent nudges to {Count} assessors.", grouped.Count());
    }

    private static bool HasFieldUserActor(ActorRule rule) => rule switch
    {
        FieldUserActorRule => true,
        CombinedActorRule combined => combined.Rules.Any(HasFieldUserActor),
        _ => false
    };

    private static string? GetFieldName(ActorRule rule) => rule switch
    {
        FieldUserActorRule field => field.Field,
        CombinedActorRule combined => combined.Rules.OfType<FieldUserActorRule>().FirstOrDefault()?.Field,
        _ => null
    };

    private static string? ExtractFieldValue(string dataJson, string fieldName)
    {
        try
        {
            using var doc = JsonDocument.Parse(dataJson);
            return doc.RootElement.TryGetProperty(fieldName, out var value) ? value.GetString() : null;
        }
        catch
        {
            return null;
        }
    }
}
