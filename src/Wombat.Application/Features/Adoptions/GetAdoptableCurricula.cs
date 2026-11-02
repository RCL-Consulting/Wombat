using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Adoptions;

/// <summary>How an open version stands to the version an institution has actively adopted for its discipline (T392, F2).</summary>
public enum AdoptionRelation
{
    /// <summary>The institution has adopted no version of the discipline: adopting it supersedes nothing.</summary>
    None,

    /// <summary>Later than the adopted version by (EffectiveFrom, Id).</summary>
    Newer,

    /// <summary>Earlier than the adopted version by (EffectiveFrom, Id): adopting it goes back (Q7, Q21).</summary>
    Older
}

/// <summary>A national curriculum version an institution can adopt. (T091 phase 5.)</summary>
public sealed record AdoptableCurriculumDto(
    int Id,
    int SubSpecialityId,
    string CollegeName,
    string SpecialityName,
    string SubSpecialityName,
    string Name,
    string Version)
{
    /// <summary>The day the version is effective from.</summary>
    public DateOnly EffectiveFrom { get; init; }

    /// <summary>The version's national items: what every institution that adopts it trains against (T392, Q10).</summary>
    public int NationalItemCount { get; init; }

    /// <summary>
    /// How the version stands to the institution's active adoption of its discipline (F2): newer or older by
    /// (<see cref="EffectiveFrom" />, Id), so a later clone with the same date is newer; <see cref="AdoptionRelation.None" />
    /// when the institution has adopted none, and adopting it asks nothing (Q6).
    /// </summary>
    public AdoptionRelation Relation { get; init; }

    /// <summary>The version the institution has actively adopted for the discipline, which adopting this one supersedes; else null.</summary>
    public int? AdoptedCurriculumId { get; init; }

    /// <summary>That version's name; else null.</summary>
    public string? AdoptedName { get; init; }

    /// <summary>That version's version ("11.1"); else null.</summary>
    public string? AdoptedVersion { get; init; }

    /// <summary>
    /// The institution's ACTIVE trainee profiles on the adopted version (S1), who stay on it until each is moved: the
    /// dialog's "The 3 registrars on 11.1 stay on it …"; 0 when none, and the line is left out.
    /// </summary>
    public int AdoptedRegistrarCount { get; init; }

    /// <summary>
    /// The codes of the institution's own items on the adopted version whose EPA this version does not hold as the
    /// institution's own (Q6): "Your institution's own item on 11.1, KGK-001, is not carried over"; empty when none.
    /// </summary>
    public IReadOnlyList<string> OwnItemsNotCarried { get; init; } = [];
}

/// <summary>
/// The open national curriculum versions an institution may adopt: every version its College has not held back (T301)
/// that the institution has not ACTIVELY adopted (T392, Q7: an adopted version is a row with its state on Curricula, never
/// an offer; an older one stays, going back, Q21), each measured against the institution's active adoption of its
/// discipline, with what the superseding dialog says (Q6). Administrators and InstitutionalAdmins only.
/// </summary>
/// <param name="InstitutionId">
/// The institution: required for the Administrator (her "An institution's curricula", Q18; null reads nothing), and the
/// InstitutionalAdmin's own when null. A scoped caller who names another institution reads nothing (T056).
/// </param>
public sealed record GetAdoptableCurriculaQuery(ClaimsPrincipal Principal, int? InstitutionId = null) : IRequest<IReadOnlyList<AdoptableCurriculumDto>>;

public sealed class GetAdoptableCurriculaQueryHandler : IRequestHandler<GetAdoptableCurriculaQuery, IReadOnlyList<AdoptableCurriculumDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAdoptableCurriculaQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AdoptableCurriculumDto>> Handle(GetAdoptableCurriculaQuery request, CancellationToken cancellationToken)
    {
        var principal = request.Principal;
        int institutionId;
        if (principal.IsAdministrator())
        {
            // An id naming no institution reads nothing, as a scoped caller's other id does (N-6).
            if (request.InstitutionId is not int chosen
                || !await _dbContext.Set<Institution>().AnyAsync(institution => institution.Id == chosen, cancellationToken))
            {
                return [];
            }

            institutionId = chosen;
        }
        else if (principal.IsInstitutionalAdmin() && principal.GetInstitutionId() is int own
                 && (request.InstitutionId is null || request.InstitutionId == own))
        {
            institutionId = own;
        }
        else
        {
            return [];
        }

        var active = await _dbContext.Set<InstitutionCurriculumAdoption>()
            .AsNoTracking()
            .Where(adoption => adoption.InstitutionId == institutionId && adoption.IsActive)
            .Select(adoption => new
            {
                adoption.SubSpecialityId,
                adoption.CurriculumId,
                adoption.Curriculum.Name,
                adoption.Curriculum.Version,
                adoption.Curriculum.EffectiveFrom
            })
            .ToListAsync(cancellationToken);
        var activeIds = active.Select(adoption => adoption.CurriculumId).ToList();

        var open = await _dbContext.Set<Curriculum>()
            .AsNoTracking()
            .Where(entity => entity.IsActive && !activeIds.Contains(entity.Id))
            .OrderBy(entity => entity.SubSpeciality.Speciality.College.Name)
            .ThenBy(entity => entity.SubSpeciality.Speciality.Name)
            .ThenBy(entity => entity.SubSpeciality.Name)
            .ThenBy(entity => entity.Name)
            .ThenByDescending(entity => entity.EffectiveFrom)
            .Select(entity => new
            {
                Row = new AdoptableCurriculumDto(
                    entity.Id,
                    entity.SubSpecialityId,
                    entity.SubSpeciality.Speciality.College.Name,
                    entity.SubSpeciality.Speciality.Name,
                    entity.SubSpeciality.Name,
                    entity.Name,
                    entity.Version),
                entity.EffectiveFrom,
                NationalItemCount = entity.Items.Count(item => item.OwningInstitutionId == null)
            })
            .ToListAsync(cancellationToken);

        var registrars = await _dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .Where(profile => profile.InstitutionId == institutionId && profile.IsActive && activeIds.Contains(profile.CurriculumId))
            .GroupBy(profile => profile.CurriculumId)
            .Select(group => new { CurriculumId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.CurriculumId, row => row.Count, cancellationToken);

        var ownItems = await _dbContext.Set<CurriculumItem>()
            .AsNoTracking()
            .Where(item => item.OwningInstitutionId == institutionId)
            .Select(item => new { item.CurriculumId, item.EpaId, item.Epa.Code })
            .ToListAsync(cancellationToken);

        return open.Select(candidate =>
        {
            var adopted = active.FirstOrDefault(adoption => adoption.SubSpecialityId == candidate.Row.SubSpecialityId);
            var row = candidate.Row with
            {
                EffectiveFrom = candidate.EffectiveFrom,
                NationalItemCount = candidate.NationalItemCount
            };
            if (adopted is null)
            {
                return row;
            }

            // F2: (EffectiveFrom, Id), so a clone dated the same day as its source is the newer.
            var newer = candidate.EffectiveFrom > adopted.EffectiveFrom
                || (candidate.EffectiveFrom == adopted.EffectiveFrom && candidate.Row.Id > adopted.CurriculumId);
            var heldHere = ownItems
                .Where(item => item.CurriculumId == candidate.Row.Id)
                .Select(item => item.EpaId)
                .ToHashSet();
            return row with
            {
                Relation = newer ? AdoptionRelation.Newer : AdoptionRelation.Older,
                AdoptedCurriculumId = adopted.CurriculumId,
                AdoptedName = adopted.Name,
                AdoptedVersion = adopted.Version,
                AdoptedRegistrarCount = registrars.GetValueOrDefault(adopted.CurriculumId),
                OwnItemsNotCarried = ownItems
                    .Where(item => item.CurriculumId == adopted.CurriculumId && !heldHere.Contains(item.EpaId))
                    .Select(item => item.Code)
                    .OrderBy(code => code, StringComparer.Ordinal)
                    .ToList()
            };
        }).ToList();
    }
}
