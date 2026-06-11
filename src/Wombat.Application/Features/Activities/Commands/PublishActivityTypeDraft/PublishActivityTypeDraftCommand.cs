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

        switch (scope)
        {
            case ActivityScope.Global:
                throw new UnauthorizedAccessException("Only global administrators may edit a globally-scoped activity type.");
            case ActivityScope.Institution:
                // Institution-scoped types are the institution's own (the schema-driven builder).
                var callerInstitutionId = principal.GetInstitutionId();
                if (!principal.IsInstitutionalAdmin() || !callerInstitutionId.HasValue || scopeId != callerInstitutionId.Value)
                {
                    throw new UnauthorizedAccessException("You do not have permission to modify activity types in that institution.");
                }
                return;
            case ActivityScope.Speciality:
                // Speciality/sub-speciality scopes now reference a national (College-owned) discipline (T091),
                // so they are authored by the owning College's CollegeAdmin (or a global Administrator).
                var specialityCollegeId = scopeId.HasValue
                    ? await dbContext.Set<Speciality>().Where(entity => entity.Id == scopeId.Value).Select(entity => (int?)entity.CollegeId).SingleOrDefaultAsync(cancellationToken)
                    : null;
                if (!specialityCollegeId.HasValue || !principal.CanAccessCollege(specialityCollegeId.Value))
                {
                    throw new UnauthorizedAccessException("You do not have permission to modify activity types in that speciality.");
                }
                return;
            case ActivityScope.SubSpeciality:
                var subSpecialityCollegeId = scopeId.HasValue
                    ? await dbContext.Set<SubSpeciality>().Where(entity => entity.Id == scopeId.Value).Select(entity => (int?)entity.Speciality.CollegeId).SingleOrDefaultAsync(cancellationToken)
                    : null;
                if (!subSpecialityCollegeId.HasValue || !principal.CanAccessCollege(subSpecialityCollegeId.Value))
                {
                    throw new UnauthorizedAccessException("You do not have permission to modify activity types in that sub-speciality.");
                }
                return;
            default:
                throw new UnauthorizedAccessException("Unknown activity-type scope.");
        }
    }
}
