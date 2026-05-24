using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;

public sealed record GetActivityTypeEditorQuery(int? ActivityTypeId, ClaimsPrincipal Principal) : IRequest<ActivityTypeEditorDto>;

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
            { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject", "requires_fields": ["title"] }
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
        if (request.ActivityTypeId is null or <= 0)
        {
            return new ActivityTypeEditorDto(
                0,
                string.Empty,
                string.Empty,
                null,
                ActivityScope.Global,
                null,
                true,
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
                []);
        }

        var activityType = await _dbContext.Set<ActivityType>()
            .AsNoTracking()
            .Include(entity => entity.Versions)
            .SingleOrDefaultAsync(entity => entity.Id == request.ActivityTypeId.Value, cancellationToken)
            ?? throw new InvalidOperationException("The activity type could not be found.");

        if (!await CanReadAsync(request.Principal, activityType, cancellationToken))
        {
            throw new InvalidOperationException("The activity type could not be found.");
        }

        return Map(activityType);
    }

    internal static async Task<bool> CanReadAsync(ClaimsPrincipal principal, ActivityType activityType, CancellationToken cancellationToken, IApplicationDbContext? dbContext = null)
    {
        if (principal.IsAdministrator())
        {
            return true;
        }

        if (!principal.IsInstitutionalAdmin())
        {
            return false;
        }

        var callerInstitutionId = principal.GetInstitutionId();
        if (!callerInstitutionId.HasValue)
        {
            return false;
        }

        switch (activityType.Scope)
        {
            case ActivityScope.Global:
                return true;
            case ActivityScope.Institution:
                return activityType.ScopeId == callerInstitutionId.Value;
            case ActivityScope.Speciality:
                if (dbContext is null || !activityType.ScopeId.HasValue)
                {
                    return false;
                }
                return await dbContext.Set<Speciality>()
                    .AnyAsync(entity => entity.Id == activityType.ScopeId.Value && entity.InstitutionId == callerInstitutionId.Value, cancellationToken);
            case ActivityScope.SubSpeciality:
                if (dbContext is null || !activityType.ScopeId.HasValue)
                {
                    return false;
                }
                return await dbContext.Set<SubSpeciality>()
                    .AnyAsync(entity => entity.Id == activityType.ScopeId.Value && entity.Speciality.InstitutionId == callerInstitutionId.Value, cancellationToken);
            default:
                return false;
        }
    }

    // Wrapper kept for callsites that already have the dbContext handy.
    public Task<bool> CanReadAsync(ClaimsPrincipal principal, ActivityType activityType, CancellationToken cancellationToken)
        => CanReadAsync(principal, activityType, cancellationToken, _dbContext);

    internal static ActivityTypeEditorDto Map(ActivityType activityType)
    {
        return new ActivityTypeEditorDto(
            activityType.Id,
            activityType.Key,
            activityType.Name,
            activityType.Description,
            activityType.Scope,
            activityType.ScopeId,
            activityType.IsActive,
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
                .ToList());
    }
}
