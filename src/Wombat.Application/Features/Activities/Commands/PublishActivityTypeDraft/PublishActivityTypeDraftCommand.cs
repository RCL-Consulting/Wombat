using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Domain.Activities;
using Wombat.Domain.Institutions;

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

        activityType.PublishDraft(request.ActorUserId);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return GetActivityTypeEditorQueryHandler.Map(activityType);
    }
}

internal static class ActivityTypeScopeGuard
{
    public static async Task EnsureCallerCanWriteAsync(IApplicationDbContext dbContext, ClaimsPrincipal principal, ActivityScope scope, int? scopeId, CancellationToken cancellationToken)
    {
        if (principal.IsAdministrator())
        {
            return;
        }

        if (!principal.IsInstitutionalAdmin())
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this activity type.");
        }

        var callerInstitutionId = principal.GetInstitutionId();
        if (!callerInstitutionId.HasValue)
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this activity type.");
        }

        switch (scope)
        {
            case ActivityScope.Global:
                throw new UnauthorizedAccessException("Only global administrators may edit a globally-scoped activity type.");
            case ActivityScope.Institution:
                if (scopeId != callerInstitutionId.Value)
                {
                    throw new UnauthorizedAccessException("You do not have permission to modify activity types in that institution.");
                }
                return;
            case ActivityScope.Speciality:
                if (!scopeId.HasValue ||
                    !await dbContext.Set<Speciality>().AnyAsync(entity => entity.Id == scopeId.Value && entity.InstitutionId == callerInstitutionId.Value, cancellationToken))
                {
                    throw new UnauthorizedAccessException("You do not have permission to modify activity types in that speciality.");
                }
                return;
            case ActivityScope.SubSpeciality:
                if (!scopeId.HasValue ||
                    !await dbContext.Set<SubSpeciality>().AnyAsync(entity => entity.Id == scopeId.Value && entity.Speciality.InstitutionId == callerInstitutionId.Value, cancellationToken))
                {
                    throw new UnauthorizedAccessException("You do not have permission to modify activity types in that sub-speciality.");
                }
                return;
            default:
                throw new UnauthorizedAccessException("Unknown activity-type scope.");
        }
    }
}
