using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Domain.Activities;

namespace Wombat.Application.Features.Activities.Commands.DiscardActivityTypeDraft;

public sealed record DiscardActivityTypeDraftCommand(int ActivityTypeId) : IRequest<ActivityTypeEditorDto>;

public sealed class DiscardActivityTypeDraftCommandHandler : IRequestHandler<DiscardActivityTypeDraftCommand, ActivityTypeEditorDto>
{
    private readonly IApplicationDbContext _dbContext;

    public DiscardActivityTypeDraftCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ActivityTypeEditorDto> Handle(DiscardActivityTypeDraftCommand request, CancellationToken cancellationToken)
    {
        var activityType = await _dbContext.Set<ActivityType>()
            .Include(entity => entity.Versions)
            .SingleOrDefaultAsync(entity => entity.Id == request.ActivityTypeId, cancellationToken)
            ?? throw new InvalidOperationException("The activity type could not be found.");

        activityType.DiscardDraft();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return GetActivityTypeEditorQueryHandler.Map(activityType);
    }
}
