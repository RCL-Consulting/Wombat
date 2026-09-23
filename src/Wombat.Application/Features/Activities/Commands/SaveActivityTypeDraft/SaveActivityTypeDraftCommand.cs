using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Commands.PublishActivityTypeDraft;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Domain.Activities;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Activities.Commands.SaveActivityTypeDraft;

/// <summary>
/// No validator: the domain-layer parsers (FormSchemaParser, WorkflowParser, CreditRulesParser)
/// validate the JSON content and throw on malformed input. The Blazor form pre-validates before
/// dispatch. A separate FluentValidation validator would duplicate those checks without adding value.
/// </summary>
/// <param name="WbaToolKey">
/// Which College-named instrument this type is, or null for "Not a WBA instrument" (T122, D21). Positional and
/// not defaulted, like <c>QuotaPeriod</c> on the curriculum item commands: this command writes every metadata
/// column on every save, so a caller that forgot a defaulted argument would silently withdraw the type's
/// instrument, and under D21 that failure is permissive and therefore invisible.
/// </param>
[NoValidator]
public sealed record SaveActivityTypeDraftCommand(
    int? ActivityTypeId,
    string Key,
    string Name,
    string? Description,
    ActivityScope Scope,
    int? ScopeId,
    bool IsActive,
    string? WbaToolKey,
    string DraftSchemaJson,
    string DraftWorkflowJson,
    string DraftCreditRulesJson,
    string DraftDisplayFieldsJson,
    string ActorUserId,
    ClaimsPrincipal Principal) : IRequest<ActivityTypeEditorDto>;

public sealed class SaveActivityTypeDraftCommandHandler : IRequestHandler<SaveActivityTypeDraftCommand, ActivityTypeEditorDto>
{
    private readonly IApplicationDbContext _dbContext;

    public SaveActivityTypeDraftCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// <para>
    /// Every check runs before the first mutation, and the order is load-bearing (T122). The audit pipeline's catch
    /// saves this request's DbContext, so anything left dirty when a check throws is COMMITTED. Before T122 a new
    /// type was <c>Add</c>ed before the scope guard ran, so an InstitutionalAdmin's refused create committed a
    /// blank-named, Version-0 Global type that squatted the key, and a seeder skips keys that already exist. The
    /// metadata was also written before the DSL parse, so a malformed draft committed a rename.
    /// </para>
    /// <para>
    /// The scope guards run before any parse or lookup, so an out-of-scope caller learns nothing about the
    /// vocabulary and is refused as unauthorised whatever else is wrong with the request.
    /// </para>
    /// </remarks>
    public async Task<ActivityTypeEditorDto> Handle(SaveActivityTypeDraftCommand request, CancellationToken cancellationToken)
    {
        ActivityType? existing = null;
        var normalizedKey = request.Key.Trim();

        if (request.ActivityTypeId is > 0)
        {
            existing = await _dbContext.Set<ActivityType>()
                .Include(entity => entity.Versions)
                .SingleOrDefaultAsync(entity => entity.Id == request.ActivityTypeId.Value, cancellationToken)
                ?? throw new InvalidOperationException("The activity type could not be found.");

            await ActivityTypeScopeGuard.EnsureCallerCanWriteAsync(_dbContext, request.Principal, existing.Scope, existing.ScopeId, cancellationToken);
        }

        // The requested new scope (which may differ from the existing) must also be within the
        // caller's reach — InstitutionalAdmin cannot retarget a type into another institution
        // or up to Global.
        await ActivityTypeScopeGuard.EnsureCallerCanWriteAsync(_dbContext, request.Principal, request.Scope, request.ScopeId, cancellationToken);

        if (existing is not null)
        {
            if (existing.Version > 0 && !string.Equals(existing.Key, normalizedKey, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Published activity type keys cannot be changed.");
            }
        }
        else if (await _dbContext.Set<ActivityType>().AnyAsync(entity => entity.Key == normalizedKey, cancellationToken))
        {
            throw new InvalidOperationException($"An activity type with key '{normalizedKey}' already exists.");
        }

        var wbaToolKey = await ResolveWbaToolKeyAsync(request.WbaToolKey, cancellationToken);

        var activityType = existing ?? new ActivityType
        {
            Key = normalizedKey,
            OwnerUserId = request.ActorUserId.Trim(),
            CreatedOn = DateTime.UtcNow
        };

        // Parses all four payloads before assigning any of them, so a malformed draft throws here with the entity
        // untouched. Nothing below this line can throw.
        activityType.SaveDraft(
            request.DraftSchemaJson,
            request.DraftWorkflowJson,
            request.DraftCreditRulesJson,
            request.DraftDisplayFieldsJson,
            request.ActorUserId);

        activityType.Key = normalizedKey;
        activityType.Name = request.Name.Trim();
        activityType.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        activityType.Scope = request.Scope;
        activityType.ScopeId = request.ScopeId;
        activityType.IsActive = request.IsActive;
        activityType.WbaToolKey = wbaToolKey;

        if (existing is null)
        {
            _dbContext.Set<ActivityType>().Add(activityType);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return GetActivityTypeEditorQueryHandler.Map(activityType);
    }

    /// <summary>
    /// The normalised key, or null for "Not a WBA instrument". A key the vocabulary does not hold is refused: a
    /// recognised-but-unknown key would be NotPermitted on every EPA that has a tool list, so the type could never
    /// be filed against any of them, and nothing would say why.
    /// </summary>
    private async Task<string?> ResolveWbaToolKeyAsync(string? requestedKey, CancellationToken cancellationToken)
    {
        var key = WbaTool.NormalizeKey(requestedKey);
        if (key is null)
        {
            return null;
        }

        if (!await _dbContext.Set<WbaTool>().AnyAsync(tool => tool.Key == key, cancellationToken))
        {
            throw new InvalidOperationException($"'{key}' is not a workplace-based assessment instrument Wombat knows.");
        }

        return key;
    }
}
