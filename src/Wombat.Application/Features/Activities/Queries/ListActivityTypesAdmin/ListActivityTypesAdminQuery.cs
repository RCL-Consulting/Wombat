using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;

namespace Wombat.Application.Features.Activities.Queries.ListActivityTypesAdmin;

public sealed record ListActivityTypesAdminQuery(ClaimsPrincipal Principal) : IRequest<ActivityTypeAdminListDto>;

public sealed class ListActivityTypesAdminQueryHandler : IRequestHandler<ListActivityTypesAdminQuery, ActivityTypeAdminListDto>
{
    private readonly IApplicationDbContext _dbContext;

    public ListActivityTypesAdminQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// Each row carries <c>CanWrite</c>, the rule the commands refuse by (<see cref="ActivityTypeAdminScope.MayWrite" />,
    /// T300), and its scope's target by name, both from one read of the rows' targets
    /// (<see cref="ActivityTypeAdminScope.ScopeTargetsAsync" />): the page names no target of its own. A row the editor
    /// would not open to the caller (<see cref="GetActivityTypeEditorQueryHandler.CanReadAsync" />: a draft or inactive type they may
    /// not write, other than a Global one) is left out, so no View leads to "could not be found" (T211).
    /// </remarks>
    public async Task<ActivityTypeAdminListDto> Handle(ListActivityTypesAdminQuery request, CancellationToken cancellationToken)
    {
        var principal = request.Principal;
        var query = _dbContext.Set<ActivityType>().AsNoTracking();

        if (!principal.IsAdministrator())
        {
            // Speciality/sub-speciality scopes are now national (College-owned) disciplines (T091), so a
            // non-admin sees the global + all national discipline types, plus their own institution's types.
            // Adoption-based narrowing of national types arrives in phase 4.
            var scopedInstitutionId = principal.GetInstitutionId();
            query = query.Where(entity =>
                entity.Scope == ActivityScope.Global ||
                (entity.Scope == ActivityScope.Institution && scopedInstitutionId != null && entity.ScopeId == scopedInstitutionId.Value) ||
                entity.Scope == ActivityScope.Speciality ||
                entity.Scope == ActivityScope.SubSpeciality);
        }

        var rows = await query
            .OrderBy(entity => entity.Name)
            .Select(entity => new
            {
                entity.Id,
                entity.Key,
                entity.Name,
                entity.Description,
                entity.Scope,
                entity.ScopeId,
                entity.Version,
                entity.IsActive,
                HasDraft = entity.StagingSchemaJson != null && entity.StagingWorkflowJson != null && entity.StagingCreditRulesJson != null,
                entity.StagingUpdatedOn
            })
            .ToListAsync(cancellationToken);

        var targets = await ActivityTypeAdminScope.ScopeTargetsAsync(
            _dbContext, rows.Select(row => (row.Scope, row.ScopeId)).ToList(), cancellationToken);

        var items = rows
            .Select(row => (Row: row, Target: targets(row.Scope, row.ScopeId)))
            .Select(pair => new ActivityTypeAdminListItemDto(
                pair.Row.Id,
                pair.Row.Key,
                pair.Row.Name,
                pair.Row.Description,
                pair.Row.Scope,
                pair.Row.ScopeId,
                pair.Row.Version,
                pair.Row.IsActive,
                pair.Row.HasDraft,
                pair.Row.StagingUpdatedOn,
                ActivityTypeAdminScope.MayWrite(principal, pair.Row.Scope, pair.Row.ScopeId, pair.Target.OwningCollegeId),
                pair.Target.Name))
            // The editor's read rule, CanReadAsync, on what the rows already carry.
            .Where(item => (item.IsActive && item.PublishedVersion > 0) || item.Scope == ActivityScope.Global || item.CanWrite)
            .ToList();

        var canCreate = (await ActivityTypeAdminScope.WritableScopesAsync(_dbContext, principal, cancellationToken)).Count > 0;

        return new ActivityTypeAdminListDto(items, canCreate);
    }
}
