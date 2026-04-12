using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Domain.Activities;

namespace Wombat.Application.Features.Activities.Commands.PublishActivityTypeDraft;

public sealed record PublishActivityTypeDraftCommand(
    int ActivityTypeId,
    string ActorUserId) : IRequest<ActivityTypeEditorDto>;

public sealed class PublishActivityTypeDraftCommandHandler : IRequestHandler<PublishActivityTypeDraftCommand, ActivityTypeEditorDto>
{
    private readonly IApplicationDbContext _dbContext;

    public PublishActivityTypeDraftCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ActivityTypeEditorDto> Handle(PublishActivityTypeDraftCommand request, CancellationToken cancellationToken)
    {
        var activityType = await _dbContext.Set<ActivityType>()
            .Include(entity => entity.Versions)
            .SingleOrDefaultAsync(entity => entity.Id == request.ActivityTypeId, cancellationToken)
            ?? throw new InvalidOperationException("The activity type could not be found.");

        activityType.PublishDraft(request.ActorUserId);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return GetActivityTypeEditorQueryHandler.Map(activityType);
    }
}
