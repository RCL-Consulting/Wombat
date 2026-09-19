using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Activities.Queries.ListActivityTypes;

/// <summary>
/// The activity types offered to a person creating an activity.
/// </summary>
/// <param name="SubjectUserId">
/// The person the activity will be ABOUT, when that is known. Supplying it narrows the list to types
/// whose rating ladder the subject's curriculum could actually credit (T123 defect 3). Omit it — the
/// builder's preview, an admin surface — and only the claims filter applies.
/// </param>
public sealed record ListActivityTypesQuery(ClaimsPrincipal Principal, string? SubjectUserId = null)
    : IRequest<IReadOnlyList<ActivityTypeListItemDto>>;

public sealed class ListActivityTypesQueryHandler : IRequestHandler<ListActivityTypesQuery, IReadOnlyList<ActivityTypeListItemDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListActivityTypesQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ActivityTypeListItemDto>> Handle(ListActivityTypesQuery request, CancellationToken cancellationToken)
    {
        var institutionId = request.Principal.GetInstitutionId();
        var specialityIds = request.Principal.GetSpecialityIds();
        var subSpecialityIds = request.Principal.GetSubSpecialityIds();

        var candidates = await _dbContext.Set<ActivityType>()
            .AsNoTracking()
            .Where(activityType =>
                activityType.Version > 0 &&
                activityType.IsActive &&
                (
                    activityType.Scope == ActivityScope.Global ||
                    (activityType.Scope == ActivityScope.Institution && institutionId.HasValue && activityType.ScopeId == institutionId.Value) ||
                    (activityType.Scope == ActivityScope.Speciality && specialityIds.Contains(activityType.ScopeId ?? 0)) ||
                    (activityType.Scope == ActivityScope.SubSpeciality && subSpecialityIds.Contains(activityType.ScopeId ?? 0))
                ))
            .OrderBy(activityType => activityType.Name)
            .Select(activityType => new
            {
                activityType.Id,
                activityType.Key,
                activityType.Name,
                activityType.Scope,
                activityType.ScopeId,
                activityType.Version,
                activityType.IsActive,
                activityType.SchemaJson,
                activityType.CreditRulesJson
            })
            .ToListAsync(cancellationToken);

        var offered = await NarrowToSubjectLadderAsync(
            candidates.Select(c => (c.Id, c.SchemaJson, c.CreditRulesJson)).ToList(),
            request.SubjectUserId,
            cancellationToken);

        return candidates
            .Where(candidate => offered is null || offered.Contains(candidate.Id))
            .Select(candidate => new ActivityTypeListItemDto(
                candidate.Id,
                candidate.Key,
                candidate.Name,
                candidate.Scope,
                candidate.ScopeId,
                candidate.Version,
                candidate.IsActive))
            .ToList();
    }

    /// <summary>
    /// The ids still offerable once the subject's entrustment ladder is taken into account, or
    /// <c>null</c> when no narrowing should happen at all.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The rule: offer type <c>T</c> to subject <c>S</c> iff T's scope matches S's claims (the filter
    /// above) <b>and</b> either T declares no resolvable rating ladder, or S's curriculum pins none, or
    /// the two sets intersect.
    /// </para>
    /// <para>
    /// This is [T109]'s unshipped option 2 and the companion to [T108]'s EPA narrowing, but note what
    /// does <b>not</b> carry over from T108: its principle of <i>replacing</i> the claims filter rather
    /// than intersecting it. T108 could replace, because a curriculum-item join already implies the right
    /// discipline. A ladder match implies nothing about scope, so this is a conjunction. Do not "fix" it
    /// into a replacement; that would drop the scope filter entirely.
    /// </para>
    /// <para>
    /// What T108's principle 1 does carry over is mirroring the engine rather than inventing a second
    /// rule — so a type's ladders are the <c>scale_key</c>s of the fields its credit directives actually
    /// name in <c>minimum_level_field</c>, which is the only thing <c>CreditApplier</c> compares. Not
    /// every <c>scale</c>-typed field: the generic <c>mini_cex</c> seed carries six, and five of them are
    /// never compared with anything.
    /// </para>
    /// <para>
    /// Both sides are SETS. A type may name several rated fields, and <c>CurriculumItem.ScaleId</c> is
    /// per item — <c>EnsureScaleCanExpressMinimaAsync</c> never compares an item's pin against its
    /// siblings', so a curriculum may legitimately hold items on two ladders.
    /// </para>
    /// <para>
    /// Every unresolved answer falls through permissively, which is T108's principle 4 and the thing that
    /// stops this emptying a required picker. ⚠ [T122] narrows the same menu from the other direction
    /// (which EPAs a tool may credit); <b>both must keep this fallback or between them they will empty
    /// one</b>.
    /// </para>
    /// <para>
    /// Resolved against <c>ActivityType.SchemaJson</c> — the current PUBLISHED schema, not a pinned
    /// version — because the subject is about to create a NEW activity at the current version. Contrast
    /// <c>CreditApplier</c>, which must read the pinned version because it is scoring an activity that
    /// already exists. Both are right; do not make one into the other.
    /// </para>
    /// <para>
    /// ⚠ Sequencing with [T110]: the four generic seeds declare <c>scale_key: "or_scale"</c>, which
    /// resolves to nothing, so they fall through permissively today. T110 proposes renaming those keys to
    /// the exact scale name — at which point they acquire a five-rung ladder and this predicate begins
    /// hiding them from every six-rung trainee. That is arguably correct (T109 would refuse their minimum
    /// anyway) but it is a visible change, and whichever lands second owns re-checking it.
    /// </para>
    /// <para>
    /// This is a MENU filter, not an authorization gate. <c>GetActivityTypeEditorQuery.CanReadAsync</c>
    /// returns true for any active published type to any authenticated caller, and
    /// <c>CreateActivityCommand</c> has no ladder check. Nothing here should become one.
    /// </para>
    /// </remarks>
    private async Task<HashSet<int>?> NarrowToSubjectLadderAsync(
        IReadOnlyList<(int Id, string? SchemaJson, string? CreditRulesJson)> candidates,
        string? subjectUserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(subjectUserId) || candidates.Count == 0)
        {
            return null;
        }

        var subjectScaleIds = await ResolveSubjectScaleIdsAsync(subjectUserId.Trim(), cancellationToken);
        if (subjectScaleIds.Count == 0)
        {
            // No profile, no curriculum items, or nothing pinned. All three are real and none of them
            // is a reason to hide a tool.
            return null;
        }

        var scaleKeysByType = new Dictionary<int, IReadOnlyList<string>>(candidates.Count);
        foreach (var candidate in candidates)
        {
            scaleKeysByType[candidate.Id] = RatedScaleKeysOf(candidate.SchemaJson, candidate.CreditRulesJson);
        }

        var rungs = await EntrustmentRungLabels.LoadForScaleKeysAsync(
            _dbContext, scaleKeysByType.Values.SelectMany(keys => keys), cancellationToken);

        var offered = new HashSet<int>();
        foreach (var candidate in candidates)
        {
            var typeScaleIds = scaleKeysByType[candidate.Id]
                .Select(rungs.ResolveScaleKey)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToHashSet();

            if (typeScaleIds.Count == 0 || typeScaleIds.Overlaps(subjectScaleIds))
            {
                offered.Add(candidate.Id);
            }
        }

        return offered;
    }

    /// <summary>
    /// The <c>scale_key</c>s of the fields this type's credit directives name as the rated one.
    /// </summary>
    /// <remarks>
    /// Unparseable JSON on either side yields an empty set, i.e. the type is not narrowed. A schema this
    /// handler cannot read is a data-quality problem; refusing to offer the tool would not fix it and
    /// would take a working instrument off a registrar's menu.
    /// </remarks>
    private static IReadOnlyList<string> RatedScaleKeysOf(string? schemaJson, string? creditRulesJson)
    {
        if (string.IsNullOrWhiteSpace(creditRulesJson) || string.IsNullOrWhiteSpace(schemaJson))
        {
            return [];
        }

        CreditRules creditRules;
        FormSchema schema;
        try
        {
            creditRules = CreditRulesParser.Parse(creditRulesJson);
            schema = FormSchemaParser.Parse(schemaJson);
        }
        catch
        {
            return [];
        }

        var ratedFieldKeys = creditRules.CountsFor
            .Select(directive => directive.MinimumLevelField)
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Select(key => key!)
            .ToHashSet(StringComparer.Ordinal);

        if (ratedFieldKeys.Count == 0)
        {
            return [];
        }

        return schema.Sections
            .SelectMany(section => section.Fields)
            .Where(field => field.Type == FieldType.Scale
                            && ratedFieldKeys.Contains(field.Key)
                            && !string.IsNullOrWhiteSpace(field.ScaleKey))
            .Select(field => field.ScaleKey!)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    /// The entrustment scales the subject's curriculum items are pinned to.
    /// </summary>
    /// <remarks>
    /// The profile is resolved active-first then by latest <c>ProgrammeStartDate</c> and is NOT filtered
    /// on <c>IsActive</c>, and the items are scoped by <c>OwningInstitutionId</c> — both the same rules
    /// as <c>ActivityReferenceDataService.ResolveCreditableEpaIdsAsync</c>, so this picker and the EPA
    /// picker beside it on the same page cannot disagree about which curriculum items are in force.
    /// </remarks>
    private async Task<HashSet<int>> ResolveSubjectScaleIdsAsync(
        string subjectUserId,
        CancellationToken cancellationToken)
    {
        var profile = await _dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .Where(entity => entity.UserId == subjectUserId)
            .OrderByDescending(entity => entity.IsActive)
            .ThenByDescending(entity => entity.ProgrammeStartDate)
            .Select(entity => new { entity.CurriculumId, entity.InstitutionId })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return [];
        }

        // Scoped by owner exactly as CreditApplier and ResolveCreditableEpaIdsAsync are. A national
        // curriculum row is shared across adopting institutions and CurriculumItems is unique on
        // (CurriculumId, EpaId), so another institution's local item is the only row for its EPA —
        // and without this predicate its ladder would decide which tools this trainee is offered.
        var scaleIds = await _dbContext.Set<CurriculumItem>()
            .AsNoTracking()
            .Where(item => item.CurriculumId == profile.CurriculumId
                && (item.OwningInstitutionId == null || item.OwningInstitutionId == profile.InstitutionId)
                && item.ScaleId != null)
            .Select(item => item.ScaleId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        return scaleIds.ToHashSet();
    }
}
