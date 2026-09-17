using System.Globalization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
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
    /// credit predicate — a mirror of <c>CreditApplier.ResolveCurriculumItemsAsync</c>: there exists a
    /// curriculum item whose <c>EpaId</c> is this EPA, whose <c>CurriculumId</c> is the subject's, and
    /// whose <c>OwningInstitutionId</c> is either null (national core) or the subject's institution
    /// (a T091 local extra — those credit, so they are offered).
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
        var creditableEpaIds = scope is { NarrowToCreditable: true }
            ? await ResolveCreditableEpaIdsAsync(scope.SubjectUserId, cancellationToken)
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
    /// The profile is picked exactly as <c>CreditApplier.ResolveTraineeAsync</c> picks it: active first,
    /// then the most recent programme start, and deliberately NOT filtered on <c>IsActive</c> — a
    /// graduated trainee still credits, so they must still be offered what credits.
    /// </remarks>
    private async Task<HashSet<int>?> ResolveCreditableEpaIdsAsync(
        string? subjectUserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(subjectUserId))
        {
            return null;
        }

        var profile = await _dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .Where(entity => entity.UserId == subjectUserId)
            .OrderByDescending(entity => entity.IsActive)
            .ThenByDescending(entity => entity.ProgrammeStartDate)
            .Select(entity => new { entity.CurriculumId, entity.InstitutionId })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return null;
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
        var epaIds = await _dbContext.Set<CurriculumItem>()
            .AsNoTracking()
            .Where(entity => entity.CurriculumId == profile.CurriculumId
                && (entity.OwningInstitutionId == null || entity.OwningInstitutionId == profile.InstitutionId))
            .Join(
                _dbContext.Set<Epa>().AsNoTracking().Where(epa => epa.IsActive),
                item => item.EpaId,
                epa => epa.Id,
                (item, epa) => epa.Id)
            .Distinct()
            .ToListAsync(cancellationToken);

        return epaIds.Count == 0 ? null : epaIds.ToHashSet();
    }

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

        var key = scaleKey.Trim();
        _ = int.TryParse(key, out var scaleId);

        var resolvedScaleId = await _dbContext.Set<EntrustmentScale>()
            .AsNoTracking()
            .Where(scale => scale.Id == scaleId || scale.Name == key)
            .Select(scale => (int?)scale.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (resolvedScaleId is null)
        {
            return [];
        }

        return await _dbContext.Set<EntrustmentLevel>()
            .AsNoTracking()
            .Where(level => level.ScaleId == resolvedScaleId.Value)
            .OrderBy(level => level.Order)
            .Select(level => new ActivityCatalogueOption(
                level.Order.ToString(),
                level.Order + ". " + level.Label))
            .ToListAsync(cancellationToken);
    }

    private static string FormatUserLabel(UserIdentityDetails user)
    {
        var name = string.Join(" ", new[] { user.FirstName, user.LastName }
            .Where(part => !string.IsNullOrWhiteSpace(part)));
        return string.IsNullOrWhiteSpace(name) ? user.Email : $"{name} ({user.Email})";
    }
}
