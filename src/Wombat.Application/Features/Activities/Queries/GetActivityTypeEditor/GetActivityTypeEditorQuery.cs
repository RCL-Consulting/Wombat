using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Users;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;

/// <summary>
/// The activity type as the builder or the filing page opens it; null for an id that names no type, or a type the caller
/// may not read (T398, flow 17, N3): the builder says Unavailable, never forbidden, so the two read alike (CLAUDE.md § Roles:
/// out of scope is not found). Until T398 both threw.
/// </summary>
/// <param name="ActivityTypeId">The type to open; null for a new one. Any other id that names no type is not found.</param>
/// <param name="ForBuilder">
/// Whether to judge what the caller may do with the type in the builder: <c>CanWrite</c>, <c>WritableScopes</c> and
/// <c>ScopeTargetName</c> (T300). The filing page (<c>/activities/new</c>) reads only the type's definition, so it passes
/// false and pays for none of it: the scopes an Administrator may write read every institution, speciality and
/// sub-speciality (the T300 review). With false those three are false, empty and null, and mean nothing. Whether the
/// caller may open the type at all is judged either way.
/// </param>
public sealed record GetActivityTypeEditorQuery(int? ActivityTypeId, ClaimsPrincipal Principal, bool ForBuilder = true)
    : IRequest<ActivityTypeEditorDto?>;

public sealed class GetActivityTypeEditorQueryHandler : IRequestHandler<GetActivityTypeEditorQuery, ActivityTypeEditorDto?>
{
    /// <summary>
    /// Who published a seeded version, or saved a seeded draft, as the version strip names them (T398, Q3, T16): the
    /// seeder is no person.
    /// </summary>
    public const string SeederName = "Wombat's catalogue";

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
    private readonly IUserAdministrationService _users;

    public GetActivityTypeEditorQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<ActivityTypeEditorDto?> Handle(GetActivityTypeEditorQuery request, CancellationToken cancellationToken)
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
            .SingleOrDefaultAsync(entity => entity.Id == activityTypeId, cancellationToken);

        // T398 (N3): an unknown id and one the caller may not read are both null, and alike, so neither says which.
        if (activityType is null || !await CanReadAsync(_dbContext, request.Principal, activityType, cancellationToken))
        {
            return null;
        }

        return request.ForBuilder
            ? await ForCallerAsync(_dbContext, _users, request.Principal, activityType, cancellationToken)
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
        var scopeTarget = await ActivityTypeAdminScope.ScopeTargetAsync(_dbContext, scope, target?.Id, cancellationToken);

        // T398: the starting draft as the builder shows it, laid out and read in words (R3-D05), owned where it starts.
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
            ActivityTypeReadout.LaidOutSchema(DefaultSchemaJson),
            ActivityTypeReadout.LaidOutWorkflow(DefaultWorkflowJson),
            ActivityTypeReadout.LaidOutCreditRules(DefaultCreditRulesJson),
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
            target?.Name)
        {
            Owner = ActivityTypeAdminScope.OwnerOf(scope, scopeTarget),
            PreviewEpaOptions = await PreviewEpaOptionsAsync(_dbContext, scope, target?.Id, cancellationToken),
            WorkflowRead = ActivityTypeReadout.Workflow(DefaultWorkflowJson, DefaultSchemaJson),
            CreditRead = ActivityTypeReadout.Credit(DefaultCreditRulesJson, DefaultSchemaJson)
        };
    }

    /// <summary>
    /// The editor as the caller sees it (T300): whether they may write the type (<see cref="ActivityTypeAdminScope.MayWriteAsync" />,
    /// the rule the commands refuse by), the scopes they may save it in, and its scope's target by name. Every path that
    /// hands an <see cref="ActivityTypeEditorDto" /> to the builder goes through here, the commands' returned editors
    /// included: the page redraws from them.
    /// </summary>
    /// <remarks>
    /// T398 (flow 17) fills the builder's members here, so the commands' returned editors carry them too: the owner, the
    /// release, the draft's saver and each version's publisher by name (one name lookup), seed ownership, the preview's
    /// EPAs, the workflow and the credit rules in words, and the draft's JSON laid out.
    /// </remarks>
    internal static async Task<ActivityTypeEditorDto> ForCallerAsync(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        ClaimsPrincipal principal,
        ActivityType activityType,
        CancellationToken cancellationToken)
    {
        // A system-managed type is its release's alone, so CanWrite is false on it for everyone (T398, Q9, T334).
        var canWrite = await ActivityTypeAdminScope.MayWriteAsync(
            dbContext, principal, activityType.Scope, activityType.ScopeId, activityType.SystemManaged, cancellationToken);

        // The scopes offered are the ones this type may be saved in, so none to a caller who may not save it.
        IReadOnlyList<ActivityTypeScopeChoiceDto> writableScopes = canWrite
            ? await ActivityTypeAdminScope.WritableScopesAsync(dbContext, principal, cancellationToken)
            : [];

        // Whether the type's own scope admits the caller, the system-managed flag aside: a writer, or a reader of a
        // release's type whom the scope would admit (T398 fix, code review M-1 and S-2). MayRead admits anyone to an
        // active published type, so this, not CanRead, decides what of the type's scope a reader is shown.
        var inScope = canWrite
            || (activityType.SystemManaged
                && await ActivityTypeAdminScope.MayWriteAsync(
                    dbContext, principal, activityType.Scope, activityType.ScopeId, systemManaged: false, cancellationToken));

        var target = await ActivityTypeAdminScope.ScopeTargetAsync(dbContext, activityType.Scope, activityType.ScopeId, cancellationToken);

        // People by name only to a caller the scope admits (S-2): an outsider reads the versions' days, the seeder's
        // "Wombat's catalogue" (no person), and no saver.
        var names = await UserDisplayNames.ResolveAsync(
            users,
            inScope
                ? activityType.Versions
                    .Select(version => version.PublishedByUserId)
                    .Append(activityType.StagingUpdatedByUserId)
                    .Where(userId => !IsSeeder(userId))
                : [],
            cancellationToken);
        string? NameOf(string userId) => IsSeeder(userId) ? SeederName : inScope ? names.NameOf(userId) : null;

        var editor = Map(activityType, canWrite, writableScopes, target.Name, NameOf);

        // What the page shows and the reads read (Q2): a writer the saved draft, as the page edits it; a reader the version
        // registrars file, never a draft only its writers may see, or the draft of a type nobody has published yet (a
        // Global one any reader may open).
        var schemaJson = canWrite ? editor.DraftSchemaJson : activityType.SchemaJson ?? editor.DraftSchemaJson;
        var workflowJson = canWrite ? editor.DraftWorkflowJson : activityType.WorkflowJson ?? editor.DraftWorkflowJson;
        var creditRulesJson = canWrite ? editor.DraftCreditRulesJson : activityType.CreditRulesJson ?? editor.DraftCreditRulesJson;
        // The display fields beside them, so a reader's DTO holds no part of the draft (S-2).
        var displayFieldsJson = canWrite || activityType.SchemaJson is null ? editor.DraftDisplayFieldsJson : activityType.DisplayFieldsJson;

        // The preview's EPAs by the caller's scope (M-1): the type's own for a caller its scope admits; for an outsider
        // reading another institution's type, the caller's own institution stands in for it, so no institution's own
        // EPAs, nor the curricula it adopted, reach another's admin. A College type's are national, the same for all.
        var previewScopeId = inScope || activityType.Scope != ActivityScope.Institution
            ? activityType.ScopeId
            : principal.GetInstitutionId();

        return editor with
        {
            DraftSchemaJson = ActivityTypeReadout.LaidOutSchema(schemaJson),
            DraftWorkflowJson = ActivityTypeReadout.LaidOutWorkflow(workflowJson),
            DraftCreditRulesJson = ActivityTypeReadout.LaidOutCreditRules(creditRulesJson),
            DraftDisplayFieldsJson = displayFieldsJson,
            StagingUpdatedByUserId = inScope ? activityType.StagingUpdatedByUserId : null,
            Owner = ActivityTypeAdminScope.OwnerOf(activityType.Scope, target),
            SystemManaged = activityType.SystemManaged,
            WrittenBy = activityType.SystemManaged
                ? SystemManagedReleases.WrittenBy(activityType.Key) ?? "a release of Wombat"
                : null,
            DraftSavedByName = activityType.HasDraft && activityType.StagingUpdatedByUserId is { } savedBy ? NameOf(savedBy) : null,
            CanDiscardDraft = activityType.HasDraft && inScope,
            IsSeedOwned = ActivityTypeSeedOwnership.IsSeedOwned(activityType),
            PreviewEpaOptions = await PreviewEpaOptionsAsync(dbContext, activityType.Scope, previewScopeId, cancellationToken),
            WorkflowRead = ActivityTypeReadout.Workflow(workflowJson, schemaJson),
            CreditRead = ActivityTypeReadout.Credit(creditRulesJson, schemaJson)
        };
    }

    private static bool IsSeeder(string? userId)
        => string.Equals(userId, ActivityTypeSeedOwnership.SeederUserId, StringComparison.Ordinal);

    /// <summary>
    /// The EPAs the preview's EPA field offers (T398, S8, J11, T291 item 6), by the type's own scope rather than the
    /// caller's claims: a Speciality or Sub-speciality type, that discipline's active national EPAs; an Institution type,
    /// the active EPAs on the curricula the institution has adopted, national and its own (never another institution's,
    /// T211); a Global type, none. Labelled and ordered as the filing page's picker labels them (<see cref="EpaOptionLabel" />,
    /// by code). Before T398 the preview read the caller's claims, so an Administrator's preview of a CPSA type offered the
    /// Demo College's EPA-001. The caller passes the target: the type's own, or, for an outsider reading an institution's
    /// type, the caller's institution (the fix pass, M-1).
    /// </summary>
    private static async Task<IReadOnlyList<ActivityCatalogueOption>> PreviewEpaOptionsAsync(
        IApplicationDbContext dbContext,
        ActivityScope scope,
        int? scopeId,
        CancellationToken cancellationToken)
    {
        if (scopeId is not int id)
        {
            return [];
        }

        var active = dbContext.Set<Epa>().AsNoTracking().Where(epa => epa.IsActive);
        IQueryable<Epa> epas;
        switch (scope)
        {
            case ActivityScope.Speciality:
                epas = active.Where(epa => epa.OwningInstitutionId == null && epa.SubSpeciality.SpecialityId == id);
                break;
            case ActivityScope.SubSpeciality:
                epas = active.Where(epa => epa.OwningInstitutionId == null && epa.SubSpecialityId == id);
                break;
            case ActivityScope.Institution:
                var adopted = dbContext.Set<InstitutionCurriculumAdoption>()
                    .Where(adoption => adoption.InstitutionId == id && adoption.IsActive)
                    .Select(adoption => adoption.CurriculumId);
                var onAdopted = dbContext.Set<CurriculumItem>()
                    .Where(item => adopted.Contains(item.CurriculumId)
                                   && (item.OwningInstitutionId == null || item.OwningInstitutionId == id))
                    .Select(item => item.EpaId);
                epas = active.Where(epa => onAdopted.Contains(epa.Id)
                                           && (epa.OwningInstitutionId == null || epa.OwningInstitutionId == id));
                break;
            default:
                return [];
        }

        var rows = await epas
            .OrderBy(epa => epa.Code)
            .Select(epa => new { epa.Id, epa.Code, epa.Title })
            .ToListAsync(cancellationToken);

        return rows
            .Select(epa => new ActivityCatalogueOption(epa.Id.ToString(), EpaOptionLabel.For(epa.Code, epa.Title, inForce: true)))
            .ToList();
    }

    /// <summary>
    /// Whether the caller may open the type in the editor: <see cref="ActivityTypeAdminScope.MayReadAsync" />. A published,
    /// active type is a form template any authenticated user must be able to read in order to log an activity against it
    /// (a trainee on /activities/new). Anything else, a draft or an inactive type, is read by whoever the scope rule admits
    /// to write it (T300), and a Global one by anyone. Whether it is system-managed is not asked (T398, Q19).
    /// </summary>
    internal static Task<bool> CanReadAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        ActivityType activityType,
        CancellationToken cancellationToken)
        => ActivityTypeAdminScope.MayReadAsync(
            dbContext, principal, activityType.Scope, activityType.ScopeId, activityType.IsActive, activityType.Version, cancellationToken);

    /// <param name="publisherName">Each version's publisher by name on the builder's path (T398, Q3); none on the filing page's.</param>
    private static ActivityTypeEditorDto Map(
        ActivityType activityType,
        bool canWrite,
        IReadOnlyList<ActivityTypeScopeChoiceDto> writableScopes,
        string? scopeTargetName,
        Func<string, string?>? publisherName = null)
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
                    version.PublishedByUserId)
                {
                    PublishedByName = publisherName?.Invoke(version.PublishedByUserId)
                })
                .ToList(),
            canWrite,
            writableScopes,
            scopeTargetName);
    }
}
