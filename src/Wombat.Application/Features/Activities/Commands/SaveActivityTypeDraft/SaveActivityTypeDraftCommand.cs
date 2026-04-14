using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Domain.Activities;

using Wombat.Application.Common;

namespace Wombat.Application.Features.Activities.Commands.SaveActivityTypeDraft;

/// <summary>
/// No validator: the domain-layer parsers (FormSchemaParser, WorkflowParser, CreditRulesParser)
/// validate the JSON content and throw on malformed input. The Blazor form pre-validates before
/// dispatch. A separate FluentValidation validator would duplicate those checks without adding value.
/// </summary>
[NoValidator]
public sealed record SaveActivityTypeDraftCommand(
    int? ActivityTypeId,
    string Key,
    string Name,
    string? Description,
    ActivityScope Scope,
    int? ScopeId,
    bool IsActive,
    string DraftSchemaJson,
    string DraftWorkflowJson,
    string DraftCreditRulesJson,
    string DraftDisplayFieldsJson,
    string ActorUserId) : IRequest<ActivityTypeEditorDto>;

public sealed class SaveActivityTypeDraftCommandHandler : IRequestHandler<SaveActivityTypeDraftCommand, ActivityTypeEditorDto>
{
    private readonly IApplicationDbContext _dbContext;

    public SaveActivityTypeDraftCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ActivityTypeEditorDto> Handle(SaveActivityTypeDraftCommand request, CancellationToken cancellationToken)
    {
        ActivityType activityType;

        if (request.ActivityTypeId is > 0)
        {
            activityType = await _dbContext.Set<ActivityType>()
                .Include(entity => entity.Versions)
                .SingleOrDefaultAsync(entity => entity.Id == request.ActivityTypeId.Value, cancellationToken)
                ?? throw new InvalidOperationException("The activity type could not be found.");
        }
        else
        {
            var normalizedKey = request.Key.Trim();
            var exists = await _dbContext.Set<ActivityType>()
                .AnyAsync(entity => entity.Key == normalizedKey, cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException($"An activity type with key '{normalizedKey}' already exists.");
            }

            activityType = new ActivityType
            {
                Key = normalizedKey,
                OwnerUserId = request.ActorUserId.Trim(),
                CreatedOn = DateTime.UtcNow
            };

            _dbContext.Set<ActivityType>().Add(activityType);
        }

        if (activityType.Version > 0 && !string.Equals(activityType.Key, request.Key.Trim(), StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Published activity type keys cannot be changed.");
        }

        activityType.Key = request.Key.Trim();
        activityType.Name = request.Name.Trim();
        activityType.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        activityType.Scope = request.Scope;
        activityType.ScopeId = request.ScopeId;
        activityType.IsActive = request.IsActive;
        activityType.SaveDraft(
            request.DraftSchemaJson,
            request.DraftWorkflowJson,
            request.DraftCreditRulesJson,
            request.DraftDisplayFieldsJson,
            request.ActorUserId);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return GetActivityTypeEditorQueryHandler.Map(activityType);
    }
}
