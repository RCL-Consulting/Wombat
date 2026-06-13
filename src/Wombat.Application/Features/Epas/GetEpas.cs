using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Epas;

/// <summary>No validator: carries a single non-nullable int ID; EF lookup enforces existence.</summary>
[NoValidator]
public sealed record DeactivateEpaCommand(int Id, ClaimsPrincipal Principal) : IRequest;
public sealed record ListEpasForSubSpecialityQuery(int? SubSpecialityId, ClaimsPrincipal Principal) : IRequest<IReadOnlyList<EpaDto>>
{
    public ListEpasForSubSpecialityQuery(ClaimsPrincipal principal) : this(null, principal) { }
}
public sealed record GetEpaByIdQuery(int Id, ClaimsPrincipal Principal) : IRequest<EpaDto?>;
public sealed record GetEntrustmentScalesListQuery() : IRequest<IReadOnlyList<EntrustmentScaleDto>>;
public sealed record GetEntrustmentScaleByIdQuery(int Id) : IRequest<EntrustmentScaleDto?>;

public sealed class DeactivateEpaCommandHandler : IRequestHandler<DeactivateEpaCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public DeactivateEpaCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeactivateEpaCommand request, CancellationToken cancellationToken)
    {
        var epa = await _dbContext.Set<Epa>()
            .Include(entity => entity.SubSpeciality)
            .ThenInclude(subSpeciality => subSpeciality.Speciality)
            .SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);

        if (epa is null)
        {
            throw new InvalidOperationException("The requested EPA was not found.");
        }

        // National EPA -> CollegeAdmin; institution-local extra -> the owning InstitutionalAdmin (T091 phase 3).
        var authorized = epa.OwningInstitutionId is null
            ? request.Principal.CanAccessCollege(epa.SubSpeciality.Speciality.CollegeId)
            : request.Principal.CanAccessInstitution(epa.OwningInstitutionId.Value);
        if (!authorized)
        {
            throw new UnauthorizedAccessException("You do not have permission to deactivate this EPA.");
        }

        epa.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
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
        var query = _dbContext.Set<Epa>().AsQueryable();

        if (request.SubSpecialityId.HasValue)
        {
            query = query.Where(entity => entity.SubSpecialityId == request.SubSpecialityId.Value);
        }

        if (!request.Principal.IsAdministrator())
        {
            // National EPAs are visible to the owning College's CollegeAdmin and to any InstitutionalAdmin
            // (they adopt the national catalogue; adoption narrowing arrives in phase 4). An InstitutionalAdmin
            // also sees their own institution-local extras. (T091 phase 3.)
            var scopedCollegeId = request.Principal.GetCollegeId();
            var scopedInstitutionId = request.Principal.GetInstitutionId();
            if (!scopedCollegeId.HasValue && !scopedInstitutionId.HasValue)
            {
                return Array.Empty<EpaDto>();
            }

            query = query.Where(entity =>
                (entity.OwningInstitutionId == null &&
                    (scopedCollegeId == null || entity.SubSpeciality.Speciality.CollegeId == scopedCollegeId.Value)) ||
                (scopedInstitutionId != null && entity.OwningInstitutionId == scopedInstitutionId.Value));
        }

        return await query
            .OrderBy(entity => entity.SubSpeciality.Speciality.College.Name)
            .ThenBy(entity => entity.SubSpeciality.Speciality.Name)
            .ThenBy(entity => entity.SubSpeciality.Name)
            .ThenBy(entity => entity.Code)
            .Select(entity => new EpaDto(
                entity.Id,
                entity.SubSpecialityId,
                entity.SubSpeciality.Name,
                entity.Code,
                entity.Title,
                entity.Description,
                entity.RequiredKnowledgeSkills,
                entity.Category,
                entity.IsActive,
                entity.CreatedOn))
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetEpaByIdQueryHandler : IRequestHandler<GetEpaByIdQuery, EpaDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetEpaByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EpaDto?> Handle(GetEpaByIdQuery request, CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Set<Epa>()
            .Where(entity => entity.Id == request.Id)
            .Select(entity => new
            {
                Dto = new EpaDto(
                    entity.Id,
                    entity.SubSpecialityId,
                    entity.SubSpeciality.Name,
                    entity.Code,
                    entity.Title,
                    entity.Description,
                    entity.RequiredKnowledgeSkills,
                    entity.Category,
                    entity.IsActive,
                    entity.CreatedOn),
                CollegeId = entity.SubSpeciality.Speciality.CollegeId,
                entity.OwningInstitutionId
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (projection is null)
        {
            return null;
        }

        // National EPA: viewable by the owning College's CollegeAdmin, or any InstitutionalAdmin (they adopt
        // the national catalogue). Local extra: viewable by the owning InstitutionalAdmin. (T091 phase 3.)
        var authorized = projection.OwningInstitutionId is null
            ? request.Principal.CanAccessCollege(projection.CollegeId) || request.Principal.IsInstitutionalAdmin()
            : request.Principal.CanAccessInstitution(projection.OwningInstitutionId.Value);
        if (!authorized)
        {
            return null;
        }

        return projection.Dto;
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
