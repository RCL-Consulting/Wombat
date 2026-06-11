using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
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

    public async Task<IReadOnlyList<ActivityCatalogueOption>> GetEpaOptionsAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<Epa>().AsNoTracking().Where(epa => epa.IsActive);

        if (!principal.IsAdministrator())
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
                return [];
            }

            query = query.Where(epa => subSpecialityIds.Contains(epa.SubSpecialityId));
        }

        return await query
            .OrderBy(epa => epa.Code)
            .Select(epa => new ActivityCatalogueOption(
                epa.Id.ToString(),
                epa.Code + " — " + epa.Title))
            .ToListAsync(cancellationToken);
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
