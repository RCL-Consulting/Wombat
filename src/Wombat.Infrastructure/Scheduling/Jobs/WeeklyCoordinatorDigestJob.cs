using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.Activities;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Identity;

namespace Wombat.Infrastructure.Scheduling.Jobs;

public sealed class WeeklyCoordinatorDigestJob : IScheduledJob
{
    private readonly IServiceScopeFactory _scopeFactory;

    public WeeklyCoordinatorDigestJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public string Key => "weekly-coordinator-digest";
    public string CronExpression => "0 8 * * 1";
    public string Description => "Sends weekly digest emails to Coordinators on Mondays at 08:00 UTC.";

    public async Task ExecuteAsync(ScheduledJobContext context, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();

        var coordinators = await userManager.GetUsersInRoleAsync(WombatRoles.Coordinator);
        if (coordinators.Count == 0)
        {
            context.Logger.LogInformation("WeeklyCoordinatorDigestJob: no coordinators found.");
            return;
        }

        var inactivityCutoff = context.UtcNow.AddDays(-30);
        var weekEnd = DateOnly.FromDateTime(context.UtcNow).AddDays(7);
        var weekStart = DateOnly.FromDateTime(context.UtcNow);

        var allTrainees = await userManager.GetUsersInRoleAsync(WombatRoles.Trainee);
        var traineeIds = allTrainees.Select(t => t.Id).ToHashSet();

        var activeTraineeIds = await dbContext.Set<Activity>()
            .Where(a => a.CreatedOn >= inactivityCutoff && traineeIds.Contains(a.SubjectUserId))
            .Select(a => a.SubjectUserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var activeSet = activeTraineeIds.ToHashSet();
        var inactiveTrainees = allTrainees
            .Where(t => !activeSet.Contains(t.Id))
            .Select(t => $"{t.FirstName} {t.LastName}")
            .OrderBy(n => n)
            .ToList();

        var msfNeedingReview = await dbContext.Set<MsfCampaign>()
            .Include(c => c.Template)
            .Where(c => c.State == MsfCampaignState.UnderReview)
            .Select(c => $"{c.Template.Name} (campaign #{c.Id})")
            .ToListAsync(cancellationToken);

        var committeeReviews = await dbContext.Set<CommitteeReview>()
            .Where(r => r.ScheduledOn >= weekStart && r.ScheduledOn <= weekEnd &&
                        r.State == CommitteeReviewState.Scheduled)
            .Select(r => new { r.TraineeUserId, r.ScheduledOn })
            .ToListAsync(cancellationToken);

        var reviewTraineeIds = committeeReviews.Select(r => r.TraineeUserId).Distinct().ToList();
        var reviewTrainees = await dbContext.Set<WombatIdentityUser>()
            .Where(u => reviewTraineeIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        var reviewDescriptions = committeeReviews
            .Select(r =>
            {
                var name = reviewTrainees.TryGetValue(r.TraineeUserId, out var trainee)
                    ? $"{trainee.FirstName} {trainee.LastName}"
                    : "Unknown trainee";
                return $"{name} on {r.ScheduledOn:yyyy-MM-dd}";
            })
            .ToList();

        foreach (var coordinator in coordinators)
        {
            if (string.IsNullOrWhiteSpace(coordinator.Email))
                continue;

            var email = CoordinatorDigestEmail.Build(
                coordinator.Email,
                coordinator.FirstName,
                inactiveTrainees,
                msfNeedingReview,
                reviewDescriptions);

            await emailSender.SendAsync(email, cancellationToken);
        }

        context.Logger.LogInformation("WeeklyCoordinatorDigestJob: sent digest to {Count} coordinators.", coordinators.Count);
    }
}
