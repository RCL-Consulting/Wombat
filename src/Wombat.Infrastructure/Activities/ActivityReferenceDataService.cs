using System.Globalization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Activities;

public sealed class ActivityReferenceDataService : IActivityReferenceDataService
{
    public const string ProcedureCatalogueKey = "procedure_catalogue";

    private readonly ApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userAdministrationService;

    public ActivityReferenceDataService(
        ApplicationDbContext dbContext,
        IUserAdministrationService userAdministrationService)
    {
        _dbContext = dbContext;
        _userAdministrationService = userAdministrationService;
    }

    public async Task<IReadOnlyList<ActivityCatalogueOption>> GetCatalogueOptionsAsync(
        string catalogueKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(catalogueKey);

        if (!string.Equals(catalogueKey, ProcedureCatalogueKey, StringComparison.Ordinal))
        {
            return [];
        }

        return await _dbContext.Set<ProcedureCatalogueEntry>()
            .OrderBy(entity => entity.Category)
            .ThenBy(entity => entity.Name)
            .Select(entity => new ActivityCatalogueOption(entity.Key, entity.Name))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// EPA options for a picker. Two quite different filters live here, and only one applies at a time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default is the claims filter: sub-speciality/speciality scope, Administrator sees everything.
    /// It answers "which EPAs is this person allowed to look at" and is what the activity-type builder's
    /// live preview needs, because no subject exists there.
    /// </para>
    /// <para>
    /// When <paramref name="scope" /> says this field is the one the credit engine reads, and the
    /// subject resolves to a trainee whose curriculum has items, the claims filter is REPLACED by the
    /// credit predicate: there exists a curriculum item whose <c>EpaId</c> is this EPA, whose
    /// <c>CurriculumId</c> is the subject's, and whose <c>OwningInstitutionId</c> is either null
    /// (national core) or the subject's institution (a T091 local extra — those credit, so they are
    /// offered). That is the item predicate of <c>CreditTargetResolver</c>, INTERSECTED with the
    /// write path's EPA→tool gate (T122): an item whose tool list refuses this activity type's
    /// instrument is not offered, because the write path would refuse it. <c>CreditApplier</c> itself
    /// never checks tools (D20), so this is no longer a plain mirror of the engine, and must not be
    /// "fixed" into one.
    /// </para>
    /// <para>
    /// Replaced, not added: the two filters are kept apart deliberately. The curriculum-item join
    /// already implies the right discipline, so intersecting it with the VIEWER's claims could only
    /// over-hide — and the viewer is routinely the assessor, a different person in a different
    /// sub-speciality from the subject.
    /// </para>
    /// <para>
    /// Nothing here consults <c>InstitutionCurriculumAdoptions</c>, <c>Curriculum.IsActive</c>,
    /// <c>TraineeProfile.IsActive</c> or <c>TraineeProfile.AdoptionId</c>, because <c>CreditApplier</c>
    /// does not either. Scoping by the institution's current active adoption — the obvious reading of
    /// T108's own fix text — would hide creditable EPAs from every trainee pinned to a superseded
    /// version, and from every trainee whose institution has no adoption row at all. (T108)
    /// </para>
    /// </remarks>
    public async Task<IReadOnlyList<ActivityCatalogueOption>> GetEpaOptionsAsync(
        ClaimsPrincipal principal,
        EpaOptionScope? scope = null,
        CancellationToken cancellationToken = default)
    {
        // The tool key reaches the narrowing arm only. The claims arm serves the builder preview (no subject) and
        // fields credit never reads, where narrowing by instrument would hide choices that change nothing.
        var creditableEpaIds = scope is { NarrowToCreditable: true }
            ? await ResolveCreditableEpaIdsAsync(scope.SubjectUserId, scope.WbaToolKey, cancellationToken)
            : null;

        var query = _dbContext.Set<Epa>().AsNoTracking().Where(epa => epa.IsActive);

        if (creditableEpaIds is not null)
        {
            query = query.Where(epa => creditableEpaIds.Contains(epa.Id));
        }
        else if (!principal.IsAdministrator())
        {
            // EPAs hang off sub-specialities. Scope to the caller's sub-speciality claims, plus any
            // sub-specialities under their speciality claims (covers speciality-scoped callers).
            var subSpecialityIds = principal.GetSubSpecialityIds().ToHashSet();
            var specialityIds = principal.GetSpecialityIds();
            if (specialityIds.Count > 0)
            {
                var subsUnderSpecialities = await _dbContext.Set<SubSpeciality>()
                    .AsNoTracking()
                    .Where(sub => specialityIds.Contains(sub.SpecialityId))
                    .Select(sub => sub.Id)
                    .ToListAsync(cancellationToken);
                foreach (var id in subsUnderSpecialities)
                {
                    subSpecialityIds.Add(id);
                }
            }

            // An InstitutionalAdmin isn't scoped to a speciality/sub-speciality but should see every
            // EPA in their institution (e.g. when designing a form in the builder).
            if (principal.IsInstitutionalAdmin())
            {
                // EPAs/sub-specialities are national now (T091); an InstitutionalAdmin building a form sees
                // the whole national catalogue. Adoption-based narrowing arrives in phase 4.
                var allSubs = await _dbContext.Set<SubSpeciality>()
                    .AsNoTracking()
                    .Select(sub => sub.Id)
                    .ToListAsync(cancellationToken);
                foreach (var id in allSubs)
                {
                    subSpecialityIds.Add(id);
                }
            }

            if (subSpecialityIds.Count == 0)
            {
                return await WithStoredValueAsync([], scope?.CurrentValue, cancellationToken);
            }

            query = query.Where(epa => subSpecialityIds.Contains(epa.SubSpecialityId));
        }

        var options = await query
            .OrderBy(epa => epa.Code)
            .Select(epa => new ActivityCatalogueOption(
                epa.Id.ToString(),
                epa.Code + " — " + epa.Title))
            .ToListAsync(cancellationToken);

        return await WithStoredValueAsync(options, scope?.CurrentValue, cancellationToken);
    }

    /// <summary>
    /// The EPA ids that would actually earn credit for the subject, or <c>null</c> when no narrowing
    /// should happen at all.
    /// </summary>
    /// <remarks>
    /// Null — fall back to the claims filter — in three cases, all of them real, none of which should
    /// empty a picker:
    /// <list type="number">
    ///   <item>No subject: the activity-type builder's live preview designs a form for a future cohort.</item>
    ///   <item>No <c>TraineeProfile</c>: a PendingTrainee has none until admission, and /activities/new
    ///   is open to them. <c>CreditApplier</c> credits them nothing either way, but a picker with no
    ///   options and no explanation is a worse failure than a permissive one.</item>
    ///   <item>A profile whose curriculum has no items: narrowing to the empty set would make a required
    ///   EPA field unsubmittable. The completion-time signal reports the consequence honestly instead.</item>
    /// </list>
    /// The profile is picked by <c>CreditTargetResolver.PickProfileAsync</c>, the same pick credit and
    /// the write-path gate make: active first, then the most recent programme start, and deliberately NOT
    /// filtered on <c>IsActive</c> — a graduated trainee still credits, so they must still be offered what
    /// credits.
    /// <para>
    /// Then the EPA→tool intersection (T122), which never empties the set either:
    /// <list type="bullet">
    ///   <item>No <paramref name="wbaToolKey" />: the creditable set unchanged (D21).</item>
    ///   <item>An item with no tool list, or one that does not parse, stays in: it is unrestricted.</item>
    ///   <item>A tool no item permits: the creditable set, NOT the empty set and NOT the claims filter. An empty
    ///   required select cannot be submitted and explains nothing; the claims filter would re-offer EPAs that
    ///   credit nothing, which is T108's defect. Every choice is then refused at submit with a message naming the
    ///   instruments the curriculum accepts, which is the one answer the trainee can act on. No seeded tool reaches
    ///   this case on the v11.1 catalogue.</item>
    /// </list>
    /// </para>
    /// </remarks>
    private async Task<HashSet<int>?> ResolveCreditableEpaIdsAsync(
        string? subjectUserId,
        string? wbaToolKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(subjectUserId))
        {
            return null;
        }

        var items = await ResolveSubjectCurriculumItemsAsync(subjectUserId.Trim(), cancellationToken);
        if (items.Count == 0)
        {
            return null;
        }

        var creditable = items.Select(item => item.EpaId).ToHashSet();
        if (WbaTool.NormalizeKey(wbaToolKey) is null)
        {
            return creditable;
        }

        var permitted = items
            .Where(item => ToolPermission.Evaluate(CurriculumItem.ParsePermittedTools(item.PermittedToolsJson), wbaToolKey)
                != ToolPermissionVerdict.NotPermitted)
            .Select(item => item.EpaId)
            .ToHashSet();

        return permitted.Count == 0 ? creditable : permitted;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ActivityCatalogueOption>> GetSubjectCurriculumEpaOptionsAsync(
        string subjectUserId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subjectUserId);

        var epaIds = (await ResolveSubjectCurriculumItemsAsync(subjectUserId.Trim(), cancellationToken))
            .Select(item => item.EpaId)
            .ToList();
        if (epaIds.Count == 0)
        {
            return [];
        }

        return await _dbContext.Set<Epa>()
            .AsNoTracking()
            .Where(epa => epaIds.Contains(epa.Id))
            .OrderBy(epa => epa.Code)
            .Select(epa => new ActivityCatalogueOption(
                epa.Id.ToString(),
                epa.Code + " — " + epa.Title))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// The raw predicate both readings share: the items on the subject's curriculum that are either
    /// national core or their own institution's local extra, and whose EPA is active, with each item's
    /// tool list. At most one per EPA: <c>CurriculumItems</c> is unique on (CurriculumId, EpaId).
    /// </summary>
    /// <remarks>
    /// One query for both readings, so the tool list is judged on exactly the rows the caller renders. The MSF
    /// reading ignores the lists; the narrowing arm applies them.
    /// </remarks>
    private async Task<IReadOnlyList<SubjectCurriculumItem>> ResolveSubjectCurriculumItemsAsync(
        string subjectUserId,
        CancellationToken cancellationToken)
    {
        var profile = await CreditTargetResolver.PickProfileAsync(_dbContext, subjectUserId, cancellationToken);
        if (profile is null)
        {
            return [];
        }

        // The IsActive join is the difference between "no curriculum items" and "no OFFERABLE curriculum
        // items". DeactivateEpaCommandHandler does not check for referencing items, so a curriculum can
        // hold items whose EPAs are all deactivated — which the caller then filters out, leaving an empty
        // required <select> that cannot be submitted. Resolving emptiness here, against the same
        // IsActive predicate the caller applies, is what makes the fallback guard mean what it says.
        //
        // CreditApplier itself does not check Epa.IsActive, so a deactivated EPA would still credit. That
        // divergence is deliberate and one-directional: it can only make the picker offer less than the
        // engine would credit, never more, and an admin who deactivates an EPA means it not to be chosen.
        return await _dbContext.Set<CurriculumItem>()
            .AsNoTracking()
            .Where(entity => entity.CurriculumId == profile.CurriculumId
                && (entity.OwningInstitutionId == null || entity.OwningInstitutionId == profile.InstitutionId))
            .Join(
                _dbContext.Set<Epa>().AsNoTracking().Where(epa => epa.IsActive),
                item => item.EpaId,
                epa => epa.Id,
                (item, epa) => new SubjectCurriculumItem(epa.Id, item.PermittedToolsJson))
            .ToListAsync(cancellationToken);
    }

    private sealed record SubjectCurriculumItem(int EpaId, string? PermittedToolsJson);

    /// <summary>
    /// Guarantees that whatever is already stored in the field stays in the option list.
    /// </summary>
    /// <remarks>
    /// An <c>epa</c> field renders as a <c>select</c>. An option list that omits the stored value does
    /// not merely narrow a choice — it renders the field as an unset "Select…" and erases recorded
    /// evidence from the page, on the read-only render as much as the editable one. Every encounter
    /// filed against an EPA outside the subject's curriculum before this fix is exactly that case.
    /// </remarks>
    private async Task<IReadOnlyList<ActivityCatalogueOption>> WithStoredValueAsync(
        List<ActivityCatalogueOption> options,
        string? currentValue,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(currentValue) ||
            options.Any(option => string.Equals(option.Value, currentValue, StringComparison.Ordinal)) ||
            !int.TryParse(currentValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var storedEpaId))
        {
            return options;
        }

        // Deliberately unfiltered: not by IsActive, not by scope. If it is on the record it is shown.
        var stored = await _dbContext.Set<Epa>()
            .AsNoTracking()
            .Where(epa => epa.Id == storedEpaId)
            .Select(epa => new ActivityCatalogueOption(
                epa.Id.ToString(),
                epa.Code + " — " + epa.Title))
            .FirstOrDefaultAsync(cancellationToken);

        if (stored is null)
        {
            return options;
        }

        options.Add(stored);
        return options.OrderBy(option => option.Label, StringComparer.Ordinal).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ActivityCatalogueOption>> GetRatedLevelOptionsForActivityTypeAsync(
        string activityTypeKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(activityTypeKey);

        var key = activityTypeKey.Trim();
        var schemaJson = await _dbContext.Set<ActivityType>()
            .AsNoTracking()
            .Where(entity => entity.Key == key && entity.Version > 0)
            .Select(entity => entity.SchemaJson)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(schemaJson))
        {
            return [];
        }

        string? scaleKey;
        try
        {
            var schema = FormSchemaParser.Parse(schemaJson);
            scaleKey = schema.RatedLevelField is null
                ? null
                : schema.Sections
                    .SelectMany(section => section.Fields)
                    .FirstOrDefault(field => string.Equals(field.Key, schema.RatedLevelField, StringComparison.Ordinal))
                    ?.ScaleKey;
        }
        catch (SchemaParseException)
        {
            // A stored version that no longer parses is a defect, but not this picker's to raise: an
            // empty option list renders as "not stated", which is a legitimate answer here anyway.
            return [];
        }

        return await GetEntrustmentScaleLevelOptionsAsync(scaleKey, cancellationToken);
    }

    public async Task<IReadOnlyList<ActivityCatalogueOption>> GetAssessorOptionsAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var users = await _userAdministrationService.ListUsersInRoleAsync(WombatRoles.Assessor, cancellationToken);
        var candidates = users.AsEnumerable();

        if (!principal.IsAdministrator())
        {
            var institutionId = principal.GetInstitutionId();
            if (!institutionId.HasValue)
            {
                return [];
            }

            candidates = candidates.Where(user => user.InstitutionId == institutionId.Value);
        }

        return candidates
            .OrderBy(user => user.LastName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(user => user.FirstName, StringComparer.OrdinalIgnoreCase)
            .Select(user => new ActivityCatalogueOption(user.UserId, FormatUserLabel(user)))
            .ToArray();
    }

    public async Task<IReadOnlyList<ActivityCatalogueOption>> GetEntrustmentScaleOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<EntrustmentScale>()
            .AsNoTracking()
            .OrderBy(scale => scale.Name)
            .Select(scale => new ActivityCatalogueOption(scale.Id.ToString(), scale.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ActivityCatalogueOption>> GetEntrustmentScaleLevelOptionsAsync(
        string? scaleKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(scaleKey))
        {
            return [];
        }

        // The id-or-exact-name rule lives in EntrustmentRungLabels so the picker, the portfolio PDF and
        // the trajectory chart cannot drift apart about which ladder a scale_key names.
        var rungs = await EntrustmentRungLabels.LoadForScaleKeysAsync(
            _dbContext, [scaleKey], cancellationToken);

        // Value is the Order because that is what gets stored in DataJson and compared by
        // CreditApplier; Label is the rung as the College prints it. T100: the two are different
        // numbers on the CPSA ladder (Order 5 is rung "4"), so concatenating them told an assessor
        // two things and let them believe either.
        return rungs
            .RungsOf(rungs.ResolveScaleKey(scaleKey))
            .Select(rung => new ActivityCatalogueOption(
                rung.Order.ToString(),
                rung.Label))
            .ToList();
    }

    private static string FormatUserLabel(UserIdentityDetails user)
    {
        var name = string.Join(" ", new[] { user.FirstName, user.LastName }
            .Where(part => !string.IsNullOrWhiteSpace(part)));
        return string.IsNullOrWhiteSpace(name) ? user.Email : $"{name} ({user.Email})";
    }
}
