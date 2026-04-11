using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Activities.Commands.RebuildCurriculumProgress;

public sealed record RebuildCurriculumProgressCommand() : IRequest<int>;

public sealed class RebuildCurriculumProgressCommandHandler : IRequestHandler<RebuildCurriculumProgressCommand, int>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICreditApplier _creditApplier;

    public RebuildCurriculumProgressCommandHandler(IApplicationDbContext dbContext, ICreditApplier creditApplier)
    {
        _dbContext = dbContext;
        _creditApplier = creditApplier;
    }

    public async Task<int> Handle(RebuildCurriculumProgressCommand request, CancellationToken cancellationToken)
    {
        var existingRows = await _dbContext.Set<CurriculumItemProgress>().ToListAsync(cancellationToken);
        _dbContext.Set<CurriculumItemProgress>().RemoveRange(existingRows);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var terminalActivities = await _dbContext.Set<Activity>()
            .Include(activity => activity.ActivityType)
            .Include(activity => activity.Transitions)
            .ToListAsync(cancellationToken);

        var rebuiltCount = 0;

        foreach (var activity in terminalActivities)
        {
            var workflow = WorkflowParser.Parse(activity.ActivityType.WorkflowJson);
            var state = workflow.States.SingleOrDefault(candidate =>
                string.Equals(candidate.Key, activity.CurrentState, StringComparison.Ordinal));

            if (state is null || !state.Terminal)
            {
                continue;
            }

            rebuiltCount += (await _creditApplier.ApplyAsync(activity, activity.ActivityType, cancellationToken)).Count;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return rebuiltCount;
    }
}
