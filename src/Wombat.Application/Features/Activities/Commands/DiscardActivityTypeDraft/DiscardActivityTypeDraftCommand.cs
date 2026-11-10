using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Common.Interfaces;
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
    private readonly IUserAdministrationService _users;

    public DiscardActivityTypeDraftCommandHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<ActivityTypeEditorDto> Handle(DiscardActivityTypeDraftCommand request, CancellationToken cancellationToken)
    {
        var activityType = await _dbContext.Set<ActivityType>()
            .Include(entity => entity.Versions)
            .SingleOrDefaultAsync(entity => entity.Id == request.ActivityTypeId, cancellationToken)
            ?? throw new InvalidOperationException("The activity type could not be found.");

        // A system-managed type's draft may be discarded by whoever its scope admits (T398, F1): nothing else of it may.
        await ActivityTypeScopeGuard.EnsureCallerCanWriteAsync(
            _dbContext, request.Principal, activityType, activityType.Scope, activityType.ScopeId, cancellationToken, discard: true);

        activityType.DiscardDraft();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetActivityTypeEditorQueryHandler.ForCallerAsync(_dbContext, _users, request.Principal, activityType, cancellationToken);
    }
}
