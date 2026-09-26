using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;

namespace Wombat.Application.Features.Activities.Commands.PublishActivityTypeDraft;

/// <summary>
/// No validator: ActivityTypeId is a non-nullable int (structural guarantee); ActorUserId is
/// the authenticated user's ID set by the caller — validated by Identity middleware, not here.
/// </summary>
[NoValidator]
public sealed record PublishActivityTypeDraftCommand(
    int ActivityTypeId,
    string ActorUserId,
    ClaimsPrincipal Principal) : IRequest<ActivityTypeEditorDto>;

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

        await ActivityTypeScopeGuard.EnsureCallerCanWriteAsync(_dbContext, request.Principal, activityType.Scope, activityType.ScopeId, cancellationToken);

        // T253 review: a draft may bind a scale deleted since it was saved (the delete asks only about published
        // versions), and publishing it would leave the new version's field with no ladder from its first activity on.
        // Asked before PublishDraft, the first mutation, because the audit pipeline's catch saves this context. With no
        // draft, PublishDraft refuses on its own.
        if (activityType.HasDraft)
        {
            await EntrustmentScaleBindings.ThrowIfAFieldBindsNoScaleAsync(
                _dbContext, FormSchemaParser.Parse(activityType.StagingSchemaJson!), cancellationToken);
        }

        activityType.PublishDraft(request.ActorUserId);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetActivityTypeEditorQueryHandler.ForCallerAsync(_dbContext, request.Principal, activityType, cancellationToken);
    }
}
