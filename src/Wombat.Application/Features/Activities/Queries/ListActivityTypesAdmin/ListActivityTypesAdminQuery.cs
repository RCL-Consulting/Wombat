using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;
using Wombat.Domain.Institutions;
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
    /// <para>
    /// T398 (flow 17, Q7, Q9, Q12, N12): each row also says whose it is (<see cref="ActivityTypeAdminScope.OwnerOf" />, the
    /// College by its short code, from the same read of the targets) and which release writes it when it is
    /// system-managed; the list says, for a College admin, the College's name and whether it holds any speciality, so the
    /// page's subtitle can say why nothing is theirs to change. The order stays by name; the page puts the caller's own
    /// first (N13).
    /// </para>
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
                entity.SystemManaged,
                HasDraft = entity.StagingSchemaJson != null && entity.StagingWorkflowJson != null && entity.StagingCreditRulesJson != null,
                entity.StagingUpdatedOn
            })
            .ToListAsync(cancellationToken);

        var targets = await ActivityTypeAdminScope.ScopeTargetsAsync(
            _dbContext, rows.Select(row => (row.Scope, row.ScopeId)).ToList(), cancellationToken);

        var items = rows
            .Select(row => (Row: row, Target: targets(row.Scope, row.ScopeId)))
            .Select(pair => (Item: new ActivityTypeAdminListItemDto(
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
                ActivityTypeAdminScope.MayWrite(principal, pair.Row.Scope, pair.Row.ScopeId, pair.Target.OwningCollegeId, pair.Row.SystemManaged),
                pair.Target.Name)
                {
                    Owner = ActivityTypeAdminScope.OwnerOf(pair.Row.Scope, pair.Target),
                    SystemManaged = pair.Row.SystemManaged,
                    // A system-managed key the table does not know reads as the guard's refusal names it (N10).
                    WrittenBy = pair.Row.SystemManaged
                        ? SystemManagedReleases.WrittenBy(pair.Row.Key) ?? "a release of Wombat"
                        : null
                },
                // The editor's read rule (MayRead, which CanReadAsync asks), on what the rows already carry: the scope rule
                // alone, so a system-managed row CanWrite refuses stays listed to whoever read it before (T398, Q19).
                Readable: ActivityTypeAdminScope.MayRead(
                    principal, pair.Row.Scope, pair.Row.ScopeId, pair.Target.OwningCollegeId, pair.Row.IsActive, pair.Row.Version)))
            .Where(pair => pair.Readable)
            .Select(pair => pair.Item)
            .ToList();

        var canCreate = (await ActivityTypeAdminScope.WritableScopesAsync(_dbContext, principal, cancellationToken)).Count > 0;

        var (collegeName, collegeHasSpeciality) = await CallerCollegeAsync(principal, cancellationToken);

        return new ActivityTypeAdminListDto(items, canCreate)
        {
            CallerCollegeName = collegeName,
            CallerCollegeHasSpeciality = collegeHasSpeciality
        };
    }

    /// <summary>
    /// For a College admin (and not an Administrator, whose subtitle is the platform's), the College's name and whether it
    /// holds any speciality (T398, N12): a College with none has no type for its admin to change. Null and true for
    /// everyone else, and for a College admin whose account names no College or a College that no longer exists.
    /// </summary>
    private async Task<(string? Name, bool HasSpeciality)> CallerCollegeAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        if (principal.IsAdministrator() || !principal.IsCollegeAdmin() || principal.GetCollegeId() is not int collegeId)
        {
            return (null, true);
        }

        var name = await _dbContext.Set<College>()
            .AsNoTracking()
            .Where(entity => entity.Id == collegeId)
            .Select(entity => entity.Name)
            .SingleOrDefaultAsync(cancellationToken);
        if (name is null)
        {
            return (null, true);
        }

        var hasSpeciality = await _dbContext.Set<Speciality>()
            .AsNoTracking()
            .AnyAsync(entity => entity.CollegeId == collegeId, cancellationToken);

        return (name, hasSpeciality);
    }
}
