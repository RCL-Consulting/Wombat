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
    private readonly IUserAdministrationService _users;

    public PublishActivityTypeDraftCommandHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<ActivityTypeEditorDto> Handle(PublishActivityTypeDraftCommand request, CancellationToken cancellationToken)
    {
        var activityType = await _dbContext.Set<ActivityType>()
            .Include(entity => entity.Versions)
            .SingleOrDefaultAsync(entity => entity.Id == request.ActivityTypeId, cancellationToken)
            ?? throw new InvalidOperationException("The activity type could not be found.");

        await ActivityTypeScopeGuard.EnsureCallerCanWriteAsync(_dbContext, request.Principal, activityType, activityType.Scope, activityType.ScopeId, cancellationToken);

        // T253 review: a draft may bind a scale deleted since it was saved (the delete asks only about published
        // versions), and publishing it would leave the new version's field with no ladder from its first activity on.
        // Asked before PublishDraft, the first mutation, because the audit pipeline's catch saves this context. With no
        // draft, PublishDraft refuses on its own. The draft is checked whole first (CheckDraft, assigning nothing), so a
        // staged draft wrong on a tab is refused as PublishDraft refuses it, every place at once, before the scale's
        // question (the fix pass, N-3).
        if (activityType.HasDraft)
        {
            await EntrustmentScaleBindings.ThrowIfAFieldBindsNoScaleAsync(_dbContext, activityType.CheckDraft(), cancellationToken);
        }

        activityType.PublishDraft(request.ActorUserId);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetActivityTypeEditorQueryHandler.ForCallerAsync(_dbContext, _users, request.Principal, activityType, cancellationToken);
    }
}
