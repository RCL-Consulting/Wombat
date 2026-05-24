using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Commands.PublishActivityTypeDraft;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Domain.Activities;

namespace Wombat.Application.Features.Activities.Commands.DiscardActivityTypeDraft;

/// <summary>No validator: carries a single non-nullable int ID; EF lookup enforces existence.</summary>
[NoValidator]
public sealed record DiscardActivityTypeDraftCommand(int ActivityTypeId, ClaimsPrincipal Principal) : IRequest<ActivityTypeEditorDto>;

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

        await ActivityTypeScopeGuard.EnsureCallerCanWriteAsync(_dbContext, request.Principal, activityType.Scope, activityType.ScopeId, cancellationToken);

        activityType.DiscardDraft();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return GetActivityTypeEditorQueryHandler.Map(activityType);
    }
}
