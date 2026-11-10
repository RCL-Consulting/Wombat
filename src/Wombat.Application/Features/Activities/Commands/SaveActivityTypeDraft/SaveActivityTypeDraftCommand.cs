using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Domain.Activities;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Activities.Commands.SaveActivityTypeDraft;

/// <summary>
/// Saves an activity type's draft, creating the type on its first save. Details' Key and Name are checked by
/// <see cref="SaveActivityTypeDraftCommandValidator" /> (T398, S10); the three payloads by the domain parsers, every
/// problem at once (<see cref="ActivityTypeDraftRefusedException" />, P2).
/// </summary>
/// <param name="WbaToolKey">
/// Which College-named instrument this type is, or null for "Not a WBA instrument" (T122, D21). Positional and
/// not defaulted, like <c>QuotaPeriod</c> on the curriculum item commands: this command writes every metadata
/// column on every save, so a caller that forgot a defaulted argument would silently withdraw the type's
/// instrument, and under D21 that failure is permissive and therefore invisible.
/// </param>
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

/// <summary>
/// The words Details' fields are refused in (T398, flow 17, S10): under Key and Name, as the page shows them. Key's two
/// are NEW; Name's are the College's (<c>CollegeHolders</c>, N1).
/// </summary>
public static class ActivityTypeFieldMessages
{
    /// <summary>The key's column (<c>ActivityTypeConfiguration</c>).</summary>
    public const int KeyMaxLength = 100;

    /// <summary>The name's column.</summary>
    public const int NameMaxLength = 200;

    public const string KeyRequired = "Enter a key.";

    public const string KeyTooLong = "A key is at most 100 characters.";

    public const string NameRequired = "Enter a name.";

    public const string NameTooLong = "A name is at most 200 characters.";

    /// <summary>The description's column (<c>ActivityTypeConfiguration</c>): the page's counter stops there too.</summary>
    public const int DescriptionMaxLength = 2000;

    /// <summary>NEW (the fix pass, code review N-1): the class S10 closes for Key and Name, under Description.</summary>
    public const string DescriptionTooLong = "A description is at most 2,000 characters.";

    /// <summary>The built sentence (R3-C22), now under Key.</summary>
    public static string KeyTaken(string key) => $"An activity type with key '{key}' already exists.";
}

/// <summary>
/// Details' Key and Name (T398, S10): blank or too long, in the words the page shows under each. Lengths are judged as the
/// handler stores them, trimmed. Until T398 the command had none, so a blank name saved and a long key reached the
/// database as a failure.
/// </summary>
public sealed class SaveActivityTypeDraftCommandValidator : AbstractValidator<SaveActivityTypeDraftCommand>
{
    public SaveActivityTypeDraftCommandValidator()
    {
        RuleFor(command => command.Key)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ActivityTypeFieldMessages.KeyRequired)
            .Must(key => key.Trim().Length <= ActivityTypeFieldMessages.KeyMaxLength).WithMessage(ActivityTypeFieldMessages.KeyTooLong);

        RuleFor(command => command.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ActivityTypeFieldMessages.NameRequired)
            .Must(name => name.Trim().Length <= ActivityTypeFieldMessages.NameMaxLength).WithMessage(ActivityTypeFieldMessages.NameTooLong);

        // T398 fix (N-1): a description past its column was a database failure, not a refusal.
        RuleFor(command => command.Description)
            .Must(description => description is null || description.Trim().Length <= ActivityTypeFieldMessages.DescriptionMaxLength)
            .WithMessage(ActivityTypeFieldMessages.DescriptionTooLong);
    }
}

public sealed class SaveActivityTypeDraftCommandHandler : IRequestHandler<SaveActivityTypeDraftCommand, ActivityTypeEditorDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public SaveActivityTypeDraftCommandHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
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

            await ActivityTypeScopeGuard.EnsureCallerCanWriteAsync(_dbContext, request.Principal, existing, existing.Scope, existing.ScopeId, cancellationToken);
        }

        // The requested new scope (which may differ from the existing) must also be within the
        // caller's reach — InstitutionalAdmin cannot retarget a type into another institution
        // or up to Global.
        await ActivityTypeScopeGuard.EnsureCallerCanWriteAsync(_dbContext, request.Principal, existing, request.Scope, request.ScopeId, cancellationToken);

        if (existing is not null)
        {
            if (existing.Version > 0 && !string.Equals(existing.Key, normalizedKey, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Published activity type keys cannot be changed.");
            }
        }
        else if (await _dbContext.Set<ActivityType>().AnyAsync(entity => entity.Key == normalizedKey, cancellationToken))
        {
            // Under Key (T398, S10, R3-C22), still before anything is staged.
            throw new FieldRefusalException(nameof(SaveActivityTypeDraftCommand.Key), ActivityTypeFieldMessages.KeyTaken(normalizedKey));
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
        return await GetActivityTypeEditorQueryHandler.ForCallerAsync(_dbContext, _users, request.Principal, activityType, cancellationToken);
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
