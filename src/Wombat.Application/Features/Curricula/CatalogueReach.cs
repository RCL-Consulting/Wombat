using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Curricula;

/// <summary>
/// How far a College's change reaches (T394, flow 16, Q2, S16, T8): which institutions train against a curriculum version,
/// and how many registrars in training it measures. Told in numbers and institution names, never a registrar's name.
/// </summary>
/// <param name="AdoptedBy">
/// The institutions whose ACTIVE adoption of its discipline is this version, by name (ordinal): "Adopted by 1 institution,
/// Kgosi Kgari Teaching Hospital".
/// </param>
/// <param name="RegistrarCount">
/// Their ACTIVE trainee profiles on this version: <c>TraineeProfile.InstitutionId</c> and <c>CurriculumId</c>, never the
/// nullable <c>AdoptionId</c> (flow 15's <c>CurriculumInstitutionStateDto.RegistrarCount</c> rule, S1, S16).
/// </param>
/// <param name="SupersededBy">
/// The institutions that adopted this version and have since adopted another of the discipline, but still have registrars
/// in training on it (T8: "Still used by 1 institution that has superseded it, …"), by name. An institution with none left
/// on it is not reached, and is not named.
/// </param>
/// <param name="SupersededRegistrarCount">Those institutions' active profiles still on this version.</param>
public sealed record CurriculumReachDto(
    IReadOnlyList<string> AdoptedBy,
    int RegistrarCount,
    IReadOnlyList<string> SupersededBy,
    int SupersededRegistrarCount)
{
    /// <summary>No institution reaches it: "No institution has adopted this version yet." (R3-D05).</summary>
    public static CurriculumReachDto None { get; } = new([], 0, [], 0);

    /// <summary>Whether any institution trains against it, adopted or superseded.</summary>
    public bool Reaches => AdoptedBy.Count > 0 || SupersededBy.Count > 0;
}

/// <summary>
/// How far a change to one EPA reaches (T394, flow 16, Q2, Q21's EV2, S16): the curricula an institution trains against
/// that hold it, the institutions, and the registrars in training measured against it.
/// </summary>
/// <param name="CurriculumCount">
/// The versions holding it that some institution trains against (<see cref="CurriculumReachDto.Reaches" />): for a national
/// EPA its national items; for an institution's own EPA that institution's own items.
/// </param>
/// <param name="Institutions">The institutions, each once, by name (ordinal).</param>
/// <param name="RegistrarCount">The registrars in training on those versions at those institutions, summed (S16).</param>
public sealed record EpaReachDto(int CurriculumCount, IReadOnlyList<string> Institutions, int RegistrarCount)
{
    /// <summary>On no curriculum an institution trains against (R3-E11: "PAED-016 is on no curriculum …").</summary>
    public static EpaReachDto None { get; } = new(0, [], 0);
}

/// <summary>
/// The reach reads (T394, flow 16, build-contracts § 0): one rule for the curriculum's page, the items page, an item's page
/// and an EPA's page, so the counts cannot disagree between them. Three reads whatever the count of curricula: adoptions,
/// active profiles, and (for an EPA) its items.
/// </summary>
/// <remarks>
/// <para>
/// A version reaches an institution while that institution's adoption of it is active (<see cref="CurriculumReachDto.AdoptedBy" />),
/// or while it has superseded the version but still has active registrars pinned to it
/// (<see cref="CurriculumReachDto.SupersededBy" />): a target the College changes on 11.1 is read live for those registrars
/// too (T8, "including periods that have already closed"). A registrar is an ACTIVE <see cref="TraineeProfile" /> whose
/// <c>InstitutionId</c> and <c>CurriculumId</c> are the institution's and the version's: a graduate's or a withdrawn
/// registrar's profile is inactive and is not counted.
/// </para>
/// <para>
/// Only a caller who may change the record is told (the College and the Administrator for a national one; the owning
/// institution's administrator and the Administrator for an institution's own EPA): the reach names institutions, which an
/// institution's administrator does not read of another's. The reads below do not check: their callers fill the DTOs only
/// where <c>CanEditCurriculum</c> or <c>MayChange</c> holds.
/// </para>
/// </remarks>
public static class CatalogueReach
{
    /// <summary>The reach of each of <paramref name="curriculumIds" />; a version no institution trains against reads <see cref="CurriculumReachDto.None" />.</summary>
    public static async Task<IReadOnlyDictionary<int, CurriculumReachDto>> ForCurriculaAsync(
        IApplicationDbContext dbContext, IReadOnlyCollection<int> curriculumIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(curriculumIds);
        if (curriculumIds.Count == 0)
        {
            return new Dictionary<int, CurriculumReachDto>();
        }

        var ids = curriculumIds.Distinct().ToList();
        var adoptions = await dbContext.Set<InstitutionCurriculumAdoption>()
            .AsNoTracking()
            .Where(adoption => ids.Contains(adoption.CurriculumId))
            .Select(adoption => new { adoption.CurriculumId, adoption.InstitutionId, adoption.IsActive, InstitutionName = adoption.Institution.Name })
            .ToListAsync(cancellationToken);

        var registrars = await dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .Where(profile => profile.IsActive && ids.Contains(profile.CurriculumId))
            .GroupBy(profile => new { profile.CurriculumId, profile.InstitutionId })
            .Select(group => new { group.Key.CurriculumId, group.Key.InstitutionId, Count = group.Count() })
            .ToListAsync(cancellationToken);
        var countOf = registrars.ToDictionary(row => (row.CurriculumId, row.InstitutionId), row => row.Count);

        var result = new Dictionary<int, CurriculumReachDto>();
        foreach (var curriculumId in ids)
        {
            // An institution adopts a version once per going-back (flow 15's Q21), so it may hold several rows of it: it
            // is reached by its active one, or else as a superseder.
            var byInstitution = adoptions
                .Where(adoption => adoption.CurriculumId == curriculumId)
                .GroupBy(adoption => adoption.InstitutionId)
                .Select(group => new
                {
                    InstitutionId = group.Key,
                    Name = group.First().InstitutionName,
                    Active = group.Any(adoption => adoption.IsActive),
                    Registrars = countOf.GetValueOrDefault((curriculumId, group.Key))
                })
                .ToList();

            var adopted = byInstitution.Where(row => row.Active).ToList();
            var superseded = byInstitution.Where(row => !row.Active && row.Registrars > 0).ToList();
            result[curriculumId] = new CurriculumReachDto(
                adopted.Select(row => row.Name).Order(StringComparer.Ordinal).ToList(),
                adopted.Sum(row => row.Registrars),
                superseded.Select(row => row.Name).Order(StringComparer.Ordinal).ToList(),
                superseded.Sum(row => row.Registrars));
        }

        return result;
    }

    /// <summary>The reach of one curriculum version.</summary>
    public static async Task<CurriculumReachDto> ForCurriculumAsync(
        IApplicationDbContext dbContext, int curriculumId, CancellationToken cancellationToken)
        => (await ForCurriculaAsync(dbContext, [curriculumId], cancellationToken))[curriculumId];

    /// <summary>
    /// The reach of one EPA: through its national items for a national EPA, through its owner's own items for an
    /// institution's own EPA (<paramref name="owningInstitutionId" />), counting only the owner there.
    /// </summary>
    public static async Task<EpaReachDto> ForEpaAsync(
        IApplicationDbContext dbContext, int epaId, int? owningInstitutionId, CancellationToken cancellationToken)
    {
        var curriculumIds = await dbContext.Set<CurriculumItem>()
            .AsNoTracking()
            .Where(item => item.EpaId == epaId && item.OwningInstitutionId == owningInstitutionId)
            .Select(item => item.CurriculumId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (owningInstitutionId is int owner)
        {
            // An own item reaches its owner alone (T211), so only the owner's adoptions and registrars count: another
            // institution adopting the same version reads no item of the owner's. Reached as the curricula are: the owner's
            // adoption active, or superseded with registrars still on it (T8).
            var ownerAdoptions = await dbContext.Set<InstitutionCurriculumAdoption>()
                .AsNoTracking()
                .Where(adoption => adoption.InstitutionId == owner && curriculumIds.Contains(adoption.CurriculumId))
                .Select(adoption => new { adoption.CurriculumId, adoption.IsActive, InstitutionName = adoption.Institution.Name })
                .ToListAsync(cancellationToken);
            var ownerCounts = await OwnerRegistrarsAsync(dbContext, owner, curriculumIds, cancellationToken);
            var reached = ownerAdoptions
                .GroupBy(adoption => adoption.CurriculumId)
                .Where(group => group.Any(adoption => adoption.IsActive) || ownerCounts.GetValueOrDefault(group.Key) > 0)
                .Select(group => group.Key)
                .ToList();
            return reached.Count == 0
                ? EpaReachDto.None
                : new EpaReachDto(reached.Count, [ownerAdoptions[0].InstitutionName], reached.Sum(id => ownerCounts.GetValueOrDefault(id)));
        }

        var reaches = await ForCurriculaAsync(dbContext, curriculumIds, cancellationToken);
        var reaching = reaches.Values.Where(reach => reach.Reaches).ToList();
        return reaching.Count == 0
            ? EpaReachDto.None
            : new EpaReachDto(
                reaching.Count,
                reaching.SelectMany(reach => reach.AdoptedBy.Concat(reach.SupersededBy)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
                reaching.Sum(reach => reach.RegistrarCount + reach.SupersededRegistrarCount));
    }

    private static async Task<IReadOnlyDictionary<int, int>> OwnerRegistrarsAsync(
        IApplicationDbContext dbContext, int institutionId, IReadOnlyCollection<int> curriculumIds, CancellationToken cancellationToken)
        => await dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .Where(profile => profile.IsActive
                && profile.InstitutionId == institutionId
                && curriculumIds.Contains(profile.CurriculumId))
            .GroupBy(profile => profile.CurriculumId)
            .Select(group => new { CurriculumId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.CurriculumId, row => row.Count, cancellationToken);
}
