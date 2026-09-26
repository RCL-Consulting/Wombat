using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;

namespace Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;

/// <param name="ActivityTypeId">The type to open; null for a new one. Any other id that names no type is not found.</param>
/// <param name="ForBuilder">
/// Whether to judge what the caller may do with the type in the builder: <c>CanWrite</c>, <c>WritableScopes</c> and
/// <c>ScopeTargetName</c> (T300). The filing page (<c>/activities/new</c>) reads only the type's definition, so it passes
/// false and pays for none of it: the scopes an Administrator may write read every institution, speciality and
/// sub-speciality (the T300 review). With false those three are false, empty and null, and mean nothing. Whether the
/// caller may open the type at all is judged either way.
/// </param>
public sealed record GetActivityTypeEditorQuery(int? ActivityTypeId, ClaimsPrincipal Principal, bool ForBuilder = true)
    : IRequest<ActivityTypeEditorDto>;

public sealed class GetActivityTypeEditorQueryHandler : IRequestHandler<GetActivityTypeEditorQuery, ActivityTypeEditorDto>
{
    internal const string DefaultSchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "details",
              "title": "Details",
              "fields": [
                {
                  "key": "title",
                  "type": "text",
                  "label": "Title",
                  "required": true
                }
              ]
            }
          ]
        }
        """;

    internal const string DefaultWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "submitted", "label": "Submitted", "terminal": true }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject", "validation": "all" }
          ]
        }
        """;

    internal const string DefaultCreditRulesJson = """
        {
          "counts_for": []
        }
        """;

    private readonly IApplicationDbContext _dbContext;

    public GetActivityTypeEditorQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ActivityTypeEditorDto> Handle(GetActivityTypeEditorQuery request, CancellationToken cancellationToken)
    {
        // Only no id is a new type. /admin/activity-types/0 matches the builder's {ActivityTypeId:int} route, and it read
        // "Edit " over a new type's form, or a reader's notice, before the T300 review: an id is found or it is not.
        if (request.ActivityTypeId is not int activityTypeId)
        {
            return await NewTypeAsync(request.Principal, cancellationToken);
        }

        var activityType = await _dbContext.Set<ActivityType>()
            .AsNoTracking()
            .Include(entity => entity.Versions)
            .SingleOrDefaultAsync(entity => entity.Id == activityTypeId, cancellationToken)
            ?? throw new InvalidOperationException("The activity type could not be found.");

        if (!await CanReadAsync(_dbContext, request.Principal, activityType, cancellationToken))
        {
            throw new InvalidOperationException("The activity type could not be found.");
        }

        return request.ForBuilder
            ? await ForCallerAsync(_dbContext, request.Principal, activityType, cancellationToken)
            : Map(activityType, canWrite: false, writableScopes: [], scopeTargetName: null);
    }

    /// <summary>
    /// A type not yet saved, in the first scope the caller may write and at its first target (T300): Institution with her
    /// own institution for an InstitutionalAdmin, the College's first speciality for a CollegeAdmin, Global for an
    /// Administrator. Before T300 every new type started Global, which the guard refuses from anyone but an Administrator,
    /// so an InstitutionalAdmin's first Save draft was refused unless she changed Scope. A caller with no scope to write in
    /// gets it with <c>CanWrite</c> false, and the page offers them nothing to save.
    /// </summary>
    private async Task<ActivityTypeEditorDto> NewTypeAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var writableScopes = await ActivityTypeAdminScope.WritableScopesAsync(_dbContext, principal, cancellationToken);
        var scope = writableScopes.Count > 0 ? writableScopes[0].Scope : ActivityScope.Global;
        var target = writableScopes.Count > 0 ? writableScopes[0].Targets.FirstOrDefault() : null;

        return new ActivityTypeEditorDto(
            0,
            string.Empty,
            string.Empty,
            null,
            scope,
            target?.Id,
            true,
            // A new type is "Not a WBA instrument" until an administrator says otherwise (D21).
            null,
            0,
            false,
            DefaultSchemaJson,
            DefaultWorkflowJson,
            DefaultCreditRulesJson,
            "[]",
            null,
            null,
            null,
            "[]",
            string.Empty,
            null,
            null,
            [],
            writableScopes.Count > 0,
            writableScopes,
            target?.Name);
    }

    /// <summary>
    /// The editor as the caller sees it (T300): whether they may write the type (<see cref="ActivityTypeAdminScope.MayWriteAsync" />,
    /// the rule the commands refuse by), the scopes they may save it in, and its scope's target by name. Every path that
    /// hands an <see cref="ActivityTypeEditorDto" /> to the builder goes through here, the commands' returned editors
    /// included: the page redraws from them.
    /// </summary>
    internal static async Task<ActivityTypeEditorDto> ForCallerAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        ActivityType activityType,
        CancellationToken cancellationToken)
    {
        var canWrite = await ActivityTypeAdminScope.MayWriteAsync(dbContext, principal, activityType.Scope, activityType.ScopeId, cancellationToken);

        // The scopes offered are the ones this type may be saved in, so none to a caller who may not save it.
        IReadOnlyList<ActivityTypeScopeChoiceDto> writableScopes = canWrite
            ? await ActivityTypeAdminScope.WritableScopesAsync(dbContext, principal, cancellationToken)
            : [];

        return Map(
            activityType,
            canWrite,
            writableScopes,
            await ActivityTypeAdminScope.ScopeTargetNameAsync(dbContext, activityType.Scope, activityType.ScopeId, cancellationToken));
    }

    /// <summary>
    /// Whether the caller may open the type in the editor. A published, active type is a form template any authenticated
    /// user must be able to read in order to log an activity against it (a trainee on /activities/new). Anything else, a
    /// draft or an inactive type, is read by whoever may write it (<see cref="ActivityTypeAdminScope.MayWriteAsync" />, the
    /// one rule, T300), and a Global one by anyone.
    /// </summary>
    internal static async Task<bool> CanReadAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        ActivityType activityType,
        CancellationToken cancellationToken)
        => (activityType.IsActive && activityType.Version > 0)
           || activityType.Scope == ActivityScope.Global
           || await ActivityTypeAdminScope.MayWriteAsync(dbContext, principal, activityType.Scope, activityType.ScopeId, cancellationToken);

    private static ActivityTypeEditorDto Map(
        ActivityType activityType,
        bool canWrite,
        IReadOnlyList<ActivityTypeScopeChoiceDto> writableScopes,
        string? scopeTargetName)
    {
        return new ActivityTypeEditorDto(
            activityType.Id,
            activityType.Key,
            activityType.Name,
            activityType.Description,
            activityType.Scope,
            activityType.ScopeId,
            activityType.IsActive,
            activityType.WbaToolKey,
            activityType.Version,
            activityType.HasDraft,
            activityType.StagingSchemaJson ?? activityType.SchemaJson ?? DefaultSchemaJson,
            activityType.StagingWorkflowJson ?? activityType.WorkflowJson ?? DefaultWorkflowJson,
            activityType.StagingCreditRulesJson ?? activityType.CreditRulesJson ?? DefaultCreditRulesJson,
            activityType.StagingDisplayFieldsJson ?? activityType.DisplayFieldsJson,
            activityType.SchemaJson,
            activityType.WorkflowJson,
            activityType.CreditRulesJson,
            activityType.DisplayFieldsJson,
            activityType.OwnerUserId,
            activityType.StagingUpdatedByUserId,
            activityType.StagingUpdatedOn,
            activityType.Versions
                .OrderByDescending(version => version.Version)
                .Select(version => new ActivityTypeVersionDto(
                    version.Version,
                    version.PublishedOn,
                    version.PublishedByUserId))
                .ToList(),
            canWrite,
            writableScopes,
            scopeTargetName);
    }
}
