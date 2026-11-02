using System.Linq.Expressions;
using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Curricula;

/// <summary>
/// The curricula the caller may open (<see cref="CurriculumAdminScope.Openable" />), each cut to what they read, with its
/// state for one institution where there is one (T392, flow 15, S3).
/// </summary>
/// <param name="InstitutionId">
/// Null: the caller's own institution for an InstitutionalAdmin (<see cref="CurriculumDto.Institution" /> filled), and no
/// institution for the College admin and the Administrator's All curricula. Given, the Administrator's "An institution's
/// curricula" (Q18): the curricula that institution trains against (<see cref="CurriculumAdminScope.OpenToInstitution" />),
/// each with its state there. Any other caller who names an institution not their own reads nothing, never another
/// institution's (T056).
/// </param>
public sealed record GetCurriculaListQuery(ClaimsPrincipal Principal, int? InstitutionId = null) : IRequest<IReadOnlyList<CurriculumDto>>;

/// <summary>
/// One curriculum, cut to what the caller reads, or null when they may not open it (T056: not found, not forbidden). For an
/// InstitutionalAdmin it carries the version's state for her institution (<see cref="CurriculumDto.Institution" />), and for
/// everyone whether the items page offers an add (<see cref="CurriculumDto.MayAddItem" />; T392, F6).
/// </summary>
public sealed record GetCurriculumByIdQuery(int Id, ClaimsPrincipal Principal) : IRequest<CurriculumDto?>;

public sealed class GetCurriculaListQueryHandler : IRequestHandler<GetCurriculaListQuery, IReadOnlyList<CurriculumDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCurriculaListQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CurriculumDto>> Handle(GetCurriculaListQuery request, CancellationToken cancellationToken)
    {
        var principal = request.Principal;
        IQueryable<Curriculum> curricula;
        int? stateFor;
        if (principal.IsAdministrator())
        {
            // S3: the Administrator's All curricula has no institution; her chosen one reads what it trains against.
            stateFor = request.InstitutionId;
            curricula = request.InstitutionId is int chosen
                ? CurriculumAdminScope.OpenToInstitution(_dbContext, chosen)
                : CurriculumAdminScope.Openable(_dbContext, principal);
        }
        else
        {
            stateFor = CurriculumInstitutionStates.OwnInstitution(principal);
            if (request.InstitutionId is int asked && asked != stateFor)
            {
                return [];
            }

            // The same rule as GetCurriculumByIdQuery, so every row the list shows is one the caller can open (T211).
            curricula = CurriculumAdminScope.Openable(_dbContext, principal);
        }

        var rows = await curricula
            .OrderBy(entity => entity.SubSpeciality.Speciality.College.Name)
            .ThenBy(entity => entity.SubSpeciality.Speciality.Name)
            .ThenBy(entity => entity.SubSpeciality.Name)
            .ThenBy(entity => entity.Name)
            .ThenByDescending(entity => entity.EffectiveFrom)
            .Select(CurriculumRow.Projection(_dbContext.Set<Institution>()))
            .ToListAsync(cancellationToken);

        var states = stateFor is int institutionId
            ? await CurriculumInstitutionStates.ReadAsync(_dbContext, institutionId, cancellationToken)
            : null;
        return rows.Select(row => CurriculumInstitutionStates.Cut(row, principal, states)).ToList();
    }
}

public sealed class GetCurriculumByIdQueryHandler : IRequestHandler<GetCurriculumByIdQuery, CurriculumDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCurriculumByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CurriculumDto?> Handle(GetCurriculumByIdQuery request, CancellationToken cancellationToken)
    {
        var row = await CurriculumAdminScope.Openable(_dbContext, request.Principal)
            .Where(entity => entity.Id == request.Id)
            .Select(CurriculumRow.Projection(_dbContext.Set<Institution>()))
            .SingleOrDefaultAsync(cancellationToken);
        if (row is null)
        {
            return null;
        }

        // P1: the Administrator's items page is every institution's, so it carries no institution's state.
        var states = CurriculumInstitutionStates.OwnInstitution(request.Principal) is int institutionId
            ? await CurriculumInstitutionStates.ReadAsync(_dbContext, institutionId, cancellationToken)
            : null;
        return CurriculumInstitutionStates.Cut(row, request.Principal, states);
    }
}

/// <summary>
/// A curriculum version's state for one institution (T392, flow 15, Q10, F6, S1; <see cref="CurriculumInstitutionStateDto" />),
/// computed for every row of a page from three reads of that institution's own rows, never a read per row.
/// </summary>
internal sealed class CurriculumInstitutionStates
{
    private readonly int _institutionId;
    private readonly IReadOnlyList<AdoptionRow> _adoptions;
    private readonly IReadOnlyDictionary<int, int> _activeRegistrars;
    private readonly IReadOnlyList<OwnItemRow> _ownItems;

    private CurriculumInstitutionStates(
        int institutionId,
        IReadOnlyList<AdoptionRow> adoptions,
        IReadOnlyDictionary<int, int> activeRegistrars,
        IReadOnlyList<OwnItemRow> ownItems)
    {
        _institutionId = institutionId;
        _adoptions = adoptions;
        _activeRegistrars = activeRegistrars;
        _ownItems = ownItems;
    }

    /// <summary>The institution an InstitutionalAdmin acts for, or null for anyone else (an Administrator included).</summary>
    public static int? OwnInstitution(ClaimsPrincipal principal)
        => principal.IsInstitutionalAdmin() && !principal.IsAdministrator() ? principal.GetInstitutionId() : null;

    /// <summary>
    /// The institution's adoptions (each with its version's name and the institution's), its ACTIVE trainee profiles counted
    /// by curriculum (by <c>CurriculumId</c>, never the nullable <c>AdoptionId</c>, S1), and its own items by curriculum.
    /// </summary>
    public static async Task<CurriculumInstitutionStates> ReadAsync(
        IApplicationDbContext dbContext, int institutionId, CancellationToken cancellationToken)
    {
        var adoptions = await dbContext.Set<InstitutionCurriculumAdoption>()
            .AsNoTracking()
            .Where(adoption => adoption.InstitutionId == institutionId)
            .Select(adoption => new AdoptionRow(
                adoption.Id,
                adoption.CurriculumId,
                adoption.SubSpecialityId,
                adoption.AdoptedOn,
                adoption.IsActive,
                adoption.Curriculum.Name,
                adoption.Curriculum.Version,
                adoption.Institution.Name))
            .ToListAsync(cancellationToken);

        var registrars = await dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .Where(profile => profile.InstitutionId == institutionId && profile.IsActive)
            .GroupBy(profile => profile.CurriculumId)
            .Select(group => new { CurriculumId = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        var ownItems = await dbContext.Set<CurriculumItem>()
            .AsNoTracking()
            .Where(item => item.OwningInstitutionId == institutionId)
            .Select(item => new OwnItemRow(
                item.CurriculumId,
                item.Curriculum.SubSpecialityId,
                item.Curriculum.Version,
                item.Curriculum.EffectiveFrom))
            .ToListAsync(cancellationToken);

        return new CurriculumInstitutionStates(
            institutionId,
            adoptions,
            registrars.ToDictionary(row => row.CurriculumId, row => row.Count),
            ownItems);
    }

    /// <summary>
    /// The row cut to the caller (<see cref="CurriculumAdminScope.ForCaller" />), with the institution's state where it has
    /// adopted the version (<paramref name="states" /> null: none), and <see cref="CurriculumDto.MayAddItem" />: the College's
    /// own may add (a national item), and an InstitutionalAdmin where her institution's adoption of it is the active one (F6).
    /// </summary>
    public static CurriculumDto Cut(CurriculumRow row, ClaimsPrincipal principal, CurriculumInstitutionStates? states)
    {
        // The counts read the row before it is cut: the Administrator's chosen institution's own items are its to count
        // whatever the caller herself reads.
        var state = states?.For(row.Curriculum);
        var cut = CurriculumAdminScope.ForCaller(row.Curriculum, row.CollegeId, principal);
        var ownActive = state is { IsActiveAdoption: true } && state.InstitutionId == OwnInstitution(principal);
        return cut with
        {
            Institution = state,
            MayAddItem = cut.CanEditCurriculum || ownActive
        };
    }

    /// <summary>The version's state for the institution, or null when the institution has never adopted it.</summary>
    private CurriculumInstitutionStateDto? For(CurriculumDto curriculum)
    {
        var mine = _adoptions
            .Where(adoption => adoption.CurriculumId == curriculum.Id)
            .OrderBy(adoption => adoption.AdoptedOn)
            .ThenBy(adoption => adoption.Id)
            .ToList();
        if (mine.Count == 0)
        {
            return null;
        }

        var active = mine.FirstOrDefault(adoption => adoption.IsActive);
        var latest = active ?? mine[^1];

        // S1: the adoption that followed the version's latest, in the discipline, ordered by (AdoptedOn, Id): both may be
        // dated the same day, and going back adds a second row of the older version.
        var next = active is not null
            ? null
            : _adoptions
                .Where(adoption => adoption.SubSpecialityId == latest.SubSpecialityId)
                .Where(adoption => adoption.AdoptedOn > latest.AdoptedOn
                    || (adoption.AdoptedOn == latest.AdoptedOn && adoption.Id > latest.Id))
                .OrderBy(adoption => adoption.AdoptedOn)
                .ThenBy(adoption => adoption.Id)
                .FirstOrDefault();

        var elsewhere = _ownItems
            .Where(item => item.SubSpecialityId == curriculum.SubSpecialityId && item.CurriculumId != curriculum.Id)
            .GroupBy(item => (item.CurriculumId, item.Version, item.EffectiveFrom))
            .OrderBy(group => group.Key.EffectiveFrom)
            .ThenBy(group => group.Key.CurriculumId)
            .Select(group => new OwnItemsOnVersionDto(group.Key.CurriculumId, group.Key.Version, group.Count()))
            .ToList();

        return new CurriculumInstitutionStateDto(
            _institutionId,
            latest.InstitutionName,
            active is not null,
            latest.AdoptedOn,
            next?.AdoptedOn,
            next?.CurriculumName,
            next?.CurriculumVersion,
            _activeRegistrars.GetValueOrDefault(curriculum.Id),
            curriculum.Items.Count(item => item.OwningInstitutionId is null),
            curriculum.Items.Count(item => item.OwningInstitutionId == _institutionId),
            elsewhere);
    }

    private sealed record AdoptionRow(
        int Id,
        int CurriculumId,
        int SubSpecialityId,
        DateOnly AdoptedOn,
        bool IsActive,
        string CurriculumName,
        string CurriculumVersion,
        string InstitutionName);

    private sealed record OwnItemRow(int CurriculumId, int SubSpecialityId, string Version, DateOnly EffectiveFrom);
}

/// <summary>
/// A curriculum as read for the admin pages, with every item and the College it belongs to, before
/// <see cref="CurriculumAdminScope.ForCaller" /> cuts it to the caller. The list and the one-curriculum read share it.
/// </summary>
internal sealed record CurriculumRow(CurriculumDto Curriculum, int CollegeId)
{
    /// <summary>The projection, with each local item's owner looked up by name in <paramref name="institutions" /> (T222).</summary>
    /// <param name="institutions">
    /// The institutions set. A curriculum item has no navigation to its institution, so its name is a correlated scalar
    /// subquery inside the item projection: the curriculum and its items are still read in one statement.
    /// </param>
    public static Expression<Func<Curriculum, CurriculumRow>> Projection(IQueryable<Institution> institutions) => entity => new CurriculumRow(
        new CurriculumDto(
            entity.Id,
            entity.SubSpeciality.SpecialityId,
            entity.SubSpecialityId,
            entity.SubSpeciality.Speciality.Name,
            entity.SubSpeciality.Name,
            entity.SubSpeciality.Speciality.College.Name,
            entity.Name,
            entity.Version,
            entity.EffectiveFrom,
            entity.EffectiveTo,
            entity.IsActive,
            // T392 (flow 15, round-3-check decision 3): the national items in EPA order, then each institution's own (by
            // institution, then EPA), so KGK-001 follows PAED-015 rather than heading the table as its code would put it.
            entity.Items
                .OrderBy(item => item.OwningInstitutionId == null ? 0 : 1)
                .ThenBy(item => item.OwningInstitutionId)
                .ThenBy(item => item.Epa.Code)
                .Select(item => new CurriculumItemDto(item.Id, item.EpaId, item.Epa.Code, item.Epa.Title, item.RequiredCount, item.QuotaPeriod, item.MinimumLevelOrder, item.WindowMonths, item.Weight, item.MinimumLevelByStageJson, item.PermittedToolsJson, item.Epa.IsActive, item.OwningInstitutionId,
                    institutions.Where(institution => institution.Id == item.OwningInstitutionId).Select(institution => (string?)institution.Name).FirstOrDefault(),
                    item.DecisionCadence, item.DecisionBodyKey, item.DecisionBody == null ? null : item.DecisionBody.Name, item.DecisionIsOpportunistic, item.ScaleId, item.Scale == null ? null : item.Scale.Name))
                .ToList(),
            entity.SubSpeciality.DefaultEntrustmentScaleId),
        entity.SubSpeciality.Speciality.CollegeId);
}
