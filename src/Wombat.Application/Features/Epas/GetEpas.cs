using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Epas;

/// <summary>No validator: carries a single non-nullable int ID; EF lookup enforces existence.</summary>
[NoValidator]
public sealed record DeactivateEpaCommand(int Id, ClaimsPrincipal Principal) : IRequest;
public sealed record ListEpasForSubSpecialityQuery(int? SubSpecialityId, ClaimsPrincipal Principal) : IRequest<IReadOnlyList<EpaDto>>
{
    public ListEpasForSubSpecialityQuery(ClaimsPrincipal principal) : this(null, principal) { }
}

/// <summary>
/// One EPA by the list's rule (T392, Q2, Q19): null for an id that does not exist and for one outside the caller's list
/// alike, which the page draws as its Unavailable.
/// </summary>
public sealed record GetEpaByIdQuery(int Id, ClaimsPrincipal Principal) : IRequest<EpaDto?>;

/// <summary>The EPA list's page, read once (T392, flow 15, lane C): its rows, whose list it is, and whether Create EPA is offered.</summary>
public sealed record GetEpaListQuery(ClaimsPrincipal Principal) : IRequest<EpaListDto>;

/// <summary>
/// An EPA's page, read once (T392, flow 15, lane C): the EPA by the list's rule (none for a create, <paramref name="Id" />
/// null), the sub-speciality picker's options and the create's owner.
/// </summary>
public sealed record GetEpaPageQuery(int? Id, ClaimsPrincipal Principal) : IRequest<EpaPageDto>;

public sealed record GetEntrustmentScalesListQuery() : IRequest<IReadOnlyList<EntrustmentScaleDto>>;
public sealed record GetEntrustmentScaleByIdQuery(int Id) : IRequest<EntrustmentScaleDto?>;

/// <summary>
/// Takes an EPA out of force from now (T158). Its credit is paused, not cancelled (D48): progress it earned while in
/// force is kept, even by a rebuild, and what is completed against it from now counts once it is reactivated
/// (<see cref="UpdateEpaCommandHandler" />). Deactivating an EPA that is already inactive changes nothing, so the moment
/// its pause began stands (T196).
/// </summary>
public sealed class DeactivateEpaCommandHandler : IRequestHandler<DeactivateEpaCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IEpaCreditLock _epaCreditLock;
    private readonly TimeProvider _timeProvider;

    public DeactivateEpaCommandHandler(IApplicationDbContext dbContext, IEpaCreditLock epaCreditLock, TimeProvider? timeProvider = null)
    {
        _dbContext = dbContext;
        _epaCreditLock = epaCreditLock;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task Handle(DeactivateEpaCommand request, CancellationToken cancellationToken)
    {
        // T230. Held before the EPA is read and the moment is taken, until the save commits: a completion that already
        // judged the EPA in force commits first, so its moment falls before this pause begins, as a rebuild judges it;
        // one that comes after waits, and reads the pause. See UpdateEpaCommandHandler.
        await using var hold = await _epaCreditLock.HoldForChangeAsync(request.Id, cancellationToken);

        var epa = await _dbContext.Set<Epa>()
            .Include(entity => entity.SubSpeciality)
            .ThenInclude(subSpeciality => subSpeciality.Speciality)
            .SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);

        if (epa is null)
        {
            throw new InvalidOperationException("The requested EPA was not found.");
        }

        // National EPA -> CollegeAdmin; institution-local extra -> the owning InstitutionalAdmin (T091 phase 3).
        if (!EpaReads.MayChange(request.Principal, epa.SubSpeciality.Speciality.CollegeId, epa.OwningInstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to deactivate this EPA.");
        }

        epa.Deactivate(_timeProvider.GetUtcNow().UtcDateTime);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await hold.CommitAsync(cancellationToken);
    }
}

public sealed class ListEpasForSubSpecialityQueryHandler : IRequestHandler<ListEpasForSubSpecialityQuery, IReadOnlyList<EpaDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListEpasForSubSpecialityQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EpaDto>> Handle(ListEpasForSubSpecialityQuery request, CancellationToken cancellationToken)
    {
        var scope = await EpaReadScope.ForAsync(_dbContext, request.Principal, cancellationToken);
        var query = scope.Apply(_dbContext.Set<Epa>());

        if (request.SubSpecialityId.HasValue)
        {
            query = query.Where(entity => entity.SubSpecialityId == request.SubSpecialityId.Value);
        }

        return await EpaReads.ListAsync(_dbContext, query, request.Principal, cancellationToken);
    }
}

public sealed class GetEpaByIdQueryHandler : IRequestHandler<GetEpaByIdQuery, EpaDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetEpaByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<EpaDto?> Handle(GetEpaByIdQuery request, CancellationToken cancellationToken)
        => EpaReads.ByIdAsync(_dbContext, request.Id, request.Principal, cancellationToken);
}

public sealed class GetEpaListQueryHandler : IRequestHandler<GetEpaListQuery, EpaListDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetEpaListQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EpaListDto> Handle(GetEpaListQuery request, CancellationToken cancellationToken)
    {
        var principal = request.Principal;
        var scope = await EpaReadScope.ForAsync(_dbContext, principal, cancellationToken);
        var epas = await EpaReads.ListAsync(_dbContext, scope.Apply(_dbContext.Set<Epa>()), principal, cancellationToken);

        var reader = principal.IsAdministrator() ? EpaListReader.Administrator
            : principal.IsCollegeAdmin() ? EpaListReader.College
            : EpaListReader.Institution;
        var institutionName = reader == EpaListReader.Institution
            ? await EpaReads.InstitutionNameAsync(_dbContext, principal.GetInstitutionId(), cancellationToken)
            : null;
        var mayCreate = await EpaReads.MayCreateAsync(_dbContext, principal, cancellationToken);

        return new EpaListDto(epas, reader, mayCreate, institutionName);
    }
}

public sealed class GetEpaPageQueryHandler : IRequestHandler<GetEpaPageQuery, EpaPageDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetEpaPageQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EpaPageDto> Handle(GetEpaPageQuery request, CancellationToken cancellationToken)
    {
        var principal = request.Principal;
        EpaDto? epa = null;
        if (request.Id is { } id)
        {
            epa = await EpaReads.ByIdAsync(_dbContext, id, principal, cancellationToken);
            if (epa is null)
            {
                // Unavailable (Q19): nothing else is read for an id the caller cannot open.
                return new EpaPageDto(null, [], false, null, false);
            }
        }

        var disciplines = await EpaReads.DisciplinesAsync(_dbContext, principal, epa?.SubSpecialityId, cancellationToken);
        var createsOwn = EpaReads.CreatesOwn(principal);
        var institutionName = createsOwn
            ? await EpaReads.InstitutionNameAsync(_dbContext, principal.GetInstitutionId(), cancellationToken)
            : null;
        var mayCreate = await EpaReads.MayCreateAsync(_dbContext, principal, cancellationToken);

        return new EpaPageDto(epa, disciplines, createsOwn, institutionName, mayCreate);
    }
}

/// <summary>
/// The EPAs a caller reads (T091; T392, Q2, Q19), one rule for the list, the by-id read and so the EPA's page: an
/// Administrator every EPA; a CollegeAdmin the College's national EPAs; an InstitutionalAdmin the national EPAs of the
/// disciplines the institution has actively adopted, and its own. Until T392 the by-id read admitted any InstitutionalAdmin
/// to every national EPA (T291's sweep), so the Demo College's EPA-001 opened for Kgosi Kgari as a form.
/// </summary>
internal sealed class EpaReadScope
{
    private readonly bool _everything;
    private readonly int? _collegeId;
    private readonly int? _institutionId;
    private readonly List<int> _adoptedSubSpecialityIds;

    private EpaReadScope(bool everything, int? collegeId, int? institutionId, List<int> adoptedSubSpecialityIds)
    {
        _everything = everything;
        _collegeId = collegeId;
        _institutionId = institutionId;
        _adoptedSubSpecialityIds = adoptedSubSpecialityIds;
    }

    public static async Task<EpaReadScope> ForAsync(IApplicationDbContext dbContext, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        if (principal.IsAdministrator())
        {
            return new EpaReadScope(true, null, null, []);
        }

        // Each claim only with its role, as CurriculumAdminScope.Openable, CanAccessCollege and CanAccessInstitution read them
        // (T392 build review C1): every signed-in user carries an institution claim (T113), and it makes nobody an
        // InstitutionalAdmin, so an Assessor or a CollegeAdmin who carries one reads none of that institution's EPAs here.
        var collegeId = principal.IsCollegeAdmin() ? principal.GetCollegeId() : null;
        var institutionId = principal.IsInstitutionalAdmin() ? principal.GetInstitutionId() : null;
        var adopted = collegeId is null && institutionId is { } institution
            ? await EpaReads.AdoptedDisciplinesAsync(dbContext, institution, cancellationToken)
            : [];
        return new EpaReadScope(false, collegeId, institutionId, adopted);
    }

    public IQueryable<Epa> Apply(IQueryable<Epa> query)
    {
        if (_everything)
        {
            return query;
        }

        if (_collegeId is null && _institutionId is null)
        {
            return query.Where(entity => false);
        }

        var collegeId = _collegeId;
        var institutionId = _institutionId;
        var adopted = _adoptedSubSpecialityIds;
        return query.Where(entity =>
            (entity.OwningInstitutionId == null &&
                (collegeId != null
                    ? entity.SubSpeciality.Speciality.CollegeId == collegeId.Value
                    : adopted.Contains(entity.SubSpecialityId))) ||
            (institutionId != null && entity.OwningInstitutionId == institutionId.Value));
    }
}

/// <summary>The EPA reads the list, the by-id read, the EPA's page and the EPA commands share (T392, flow 15, lane C).</summary>
internal static class EpaReads
{
    /// <summary>
    /// Whether the caller may change an EPA: a national EPA its College's CollegeAdmin or an Administrator, an institution's
    /// own EPA that institution's InstitutionalAdmin or an Administrator (T091 phase 3). <see cref="UpdateEpaCommandHandler" />
    /// and <see cref="DeactivateEpaCommandHandler" /> refuse by it, and the pages read it as <see cref="EpaDto.MayChange" />
    /// (S4: never the caller's roles).
    /// </summary>
    public static bool MayChange(ClaimsPrincipal principal, int collegeId, int? owningInstitutionId)
        => owningInstitutionId is { } owner
            ? principal.CanAccessInstitution(owner)
            : principal.CanAccessCollege(collegeId);

    /// <summary>
    /// Whether an EPA this caller creates is the institution's own (<see cref="CreateEpaCommandHandler" />): an
    /// InstitutionalAdmin who is neither an Administrator nor a CollegeAdmin. The others create national EPAs.
    /// </summary>
    public static bool CreatesOwn(ClaimsPrincipal principal)
        => !principal.IsAdministrator() && !principal.IsCollegeAdmin() && principal.IsInstitutionalAdmin();

    /// <summary>
    /// Whether Create EPA can be completed (T392, Q12, S4): an Administrator; a CollegeAdmin with a College; an
    /// InstitutionalAdmin only while the institution has actively adopted a discipline (the create's refusal, F9).
    /// </summary>
    public static async Task<bool> MayCreateAsync(IApplicationDbContext dbContext, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        if (principal.IsAdministrator())
        {
            return true;
        }

        if (principal.IsCollegeAdmin())
        {
            return principal.GetCollegeId() is not null;
        }

        return principal.IsInstitutionalAdmin()
            && principal.GetInstitutionId() is { } institutionId
            && (await AdoptedDisciplinesAsync(dbContext, institutionId, cancellationToken)).Count > 0;
    }

    /// <summary>The disciplines an institution has actively adopted a curriculum version of (T091 phase 4).</summary>
    public static Task<List<int>> AdoptedDisciplinesAsync(IApplicationDbContext dbContext, int institutionId, CancellationToken cancellationToken)
        => dbContext.Set<InstitutionCurriculumAdoption>()
            .Where(adoption => adoption.IsActive && adoption.InstitutionId == institutionId)
            .Select(adoption => adoption.SubSpecialityId)
            .Distinct()
            .ToListAsync(cancellationToken);

    public static async Task<string?> InstitutionNameAsync(IApplicationDbContext dbContext, int? institutionId, CancellationToken cancellationToken)
        => institutionId is { } id
            ? await dbContext.Set<Institution>().Where(entity => entity.Id == id).Select(entity => entity.Name).SingleOrDefaultAsync(cancellationToken)
            : null;

    /// <summary>"Paediatrics / Paediatrics": a discipline as every catalogue page writes it.</summary>
    public static string DisciplineName(string specialityName, string subSpecialityName) => $"{specialityName} / {subSpecialityName}";

    public static async Task<EpaDto?> ByIdAsync(IApplicationDbContext dbContext, int id, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var scope = await EpaReadScope.ForAsync(dbContext, principal, cancellationToken);
        var epas = await ListAsync(dbContext, scope.Apply(dbContext.Set<Epa>()).Where(entity => entity.Id == id), principal, cancellationToken);
        return epas.SingleOrDefault();
    }

    /// <summary>
    /// The EPAs <paramref name="query" /> selects, ordered by College, speciality, sub-speciality and code, each with its owner,
    /// its discipline and whether the caller may change it. One query, whatever the count: no read per row.
    /// </summary>
    public static async Task<IReadOnlyList<EpaDto>> ListAsync(
        IApplicationDbContext dbContext, IQueryable<Epa> query, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var institutions = dbContext.Set<Institution>();
        var rows = await query
            .OrderBy(entity => entity.SubSpeciality.Speciality.College.Name)
            .ThenBy(entity => entity.SubSpeciality.Speciality.Name)
            .ThenBy(entity => entity.SubSpeciality.Name)
            .ThenBy(entity => entity.Code)
            .ThenBy(entity => entity.Id)
            .Select(entity => new
            {
                Dto = new EpaDto(
                    entity.Id,
                    entity.SubSpecialityId,
                    entity.SubSpeciality.Name,
                    entity.SubSpeciality.Speciality.College.Name,
                    entity.Code,
                    entity.Title,
                    entity.Description,
                    entity.RequiredKnowledgeSkills,
                    entity.Category,
                    entity.IsActive,
                    entity.CreatedOn),
                SpecialityName = entity.SubSpeciality.Speciality.Name,
                CollegeId = entity.SubSpeciality.Speciality.CollegeId,
                entity.OwningInstitutionId,
                OwnerName = institutions
                    .Where(institution => institution.Id == entity.OwningInstitutionId)
                    .Select(institution => institution.Name)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var namesOwner = NamesOwners(principal);
        return rows
            .Select(row => row.Dto with
            {
                OwningInstitutionId = row.OwningInstitutionId,
                OwningInstitutionName = row.OwningInstitutionId is null ? null : row.OwnerName,
                NamesOwner = namesOwner,
                MayChange = MayChange(principal, row.CollegeId, row.OwningInstitutionId),
                DisciplineName = DisciplineName(row.SpecialityName, row.Dto.SubSpecialityName)
            })
            .ToList();
    }

    /// <summary>
    /// Whether an own EPA's mark names its owner: to a caller whose reads span institutions, as an own item's does
    /// (<c>CurriculumAdminScope.NamesItemOwners</c>, T222).
    /// </summary>
    public static bool NamesOwners(ClaimsPrincipal principal)
        => principal.IsAdministrator() || !principal.IsInstitutionalAdmin();

    /// <summary>
    /// The sub-speciality picker's options (T392, F9): the disciplines the caller's create or move is accepted in, by
    /// <c>GetSubSpecialitiesListQuery</c>'s rule (every one for an Administrator, the College's for a CollegeAdmin, the
    /// actively adopted ones for an InstitutionalAdmin), and <paramref name="currentSubSpecialityId" /> where it is not among
    /// them: an own EPA left in a discipline no longer adopted keeps it, and keeps its Save.
    /// </summary>
    public static async Task<IReadOnlyList<EpaDisciplineDto>> DisciplinesAsync(
        IApplicationDbContext dbContext, ClaimsPrincipal principal, int? currentSubSpecialityId, CancellationToken cancellationToken)
    {
        var query = dbContext.Set<SubSpeciality>().AsQueryable();
        if (!principal.IsAdministrator())
        {
            // Each claim only with its role (C1), as the read scope above.
            var collegeId = principal.IsCollegeAdmin() ? principal.GetCollegeId() : null;
            var institutionId = principal.IsInstitutionalAdmin() ? principal.GetInstitutionId() : null;
            if (collegeId is { } college)
            {
                query = query.Where(entity => entity.Speciality.CollegeId == college || entity.Id == currentSubSpecialityId);
            }
            else if (institutionId is { } institution)
            {
                var adopted = await AdoptedDisciplinesAsync(dbContext, institution, cancellationToken);
                query = query.Where(entity => adopted.Contains(entity.Id) || entity.Id == currentSubSpecialityId);
            }
            else
            {
                query = query.Where(entity => entity.Id == currentSubSpecialityId);
            }
        }

        var rows = await query
            .OrderBy(entity => entity.Speciality.College.Name)
            .ThenBy(entity => entity.Speciality.Name)
            .ThenBy(entity => entity.Name)
            .Select(entity => new { entity.Id, SpecialityName = entity.Speciality.Name, entity.Name })
            .ToListAsync(cancellationToken);

        return rows.Select(row => new EpaDisciplineDto(row.Id, DisciplineName(row.SpecialityName, row.Name))).ToList();
    }
}

public sealed class GetEntrustmentScalesListQueryHandler : IRequestHandler<GetEntrustmentScalesListQuery, IReadOnlyList<EntrustmentScaleDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetEntrustmentScalesListQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EntrustmentScaleDto>> Handle(GetEntrustmentScalesListQuery request, CancellationToken cancellationToken)
        => await _dbContext.Set<EntrustmentScale>()
            .OrderBy(entity => entity.Name)
            .Select(entity => new EntrustmentScaleDto(
                entity.Id,
                entity.Name,
                entity.Description,
                entity.Levels
                    .OrderBy(level => level.Order)
                    .Select(level => new EntrustmentLevelDto(level.Id, level.Order, level.Label, level.Description))
                    .ToList()))
            .ToListAsync(cancellationToken);
}

public sealed class GetEntrustmentScaleByIdQueryHandler : IRequestHandler<GetEntrustmentScaleByIdQuery, EntrustmentScaleDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetEntrustmentScaleByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EntrustmentScaleDto?> Handle(GetEntrustmentScaleByIdQuery request, CancellationToken cancellationToken)
        => await _dbContext.Set<EntrustmentScale>()
            .Where(entity => entity.Id == request.Id)
            .Select(entity => new EntrustmentScaleDto(
                entity.Id,
                entity.Name,
                entity.Description,
                entity.Levels
                    .OrderBy(level => level.Order)
                    .Select(level => new EntrustmentLevelDto(level.Id, level.Order, level.Label, level.Description))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);
}
