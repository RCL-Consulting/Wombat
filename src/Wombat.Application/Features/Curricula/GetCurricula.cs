using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Curricula;

public sealed record GetCurriculaListQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<CurriculumDto>>;
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
        var query = _dbContext.Set<Curriculum>().AsQueryable();

        if (!request.Principal.IsAdministrator())
        {
            var scopedCollegeId = request.Principal.GetCollegeId();
            var scopedInstitutionId = request.Principal.GetInstitutionId();

            if (scopedCollegeId.HasValue)
            {
                // CollegeAdmin: the national catalogue they govern.
                query = query.Where(entity => entity.SubSpeciality.Speciality.CollegeId == scopedCollegeId.Value);
            }
            else if (scopedInstitutionId.HasValue)
            {
                // InstitutionalAdmin: only the national versions their institution has actively adopted —
                // the versions they may admit trainees into (T091 phase 4).
                var adoptedCurriculumIds = await _dbContext.Set<InstitutionCurriculumAdoption>()
                    .Where(adoption => adoption.IsActive && adoption.InstitutionId == scopedInstitutionId.Value)
                    .Select(adoption => adoption.CurriculumId)
                    .ToListAsync(cancellationToken);

                query = query.Where(entity => adoptedCurriculumIds.Contains(entity.Id));
            }
            else
            {
                return Array.Empty<CurriculumDto>();
            }
        }

        return await query
            .OrderBy(entity => entity.SubSpeciality.Speciality.College.Name)
            .ThenBy(entity => entity.SubSpeciality.Speciality.Name)
            .ThenBy(entity => entity.SubSpeciality.Name)
            .ThenBy(entity => entity.Name)
            .ThenByDescending(entity => entity.EffectiveFrom)
            .Select(entity => new CurriculumDto(
                entity.Id,
                entity.SubSpeciality.SpecialityId,
                entity.SubSpecialityId,
                entity.SubSpeciality.Speciality.Name,
                entity.SubSpeciality.Name,
                entity.Name,
                entity.Version,
                entity.EffectiveFrom,
                entity.EffectiveTo,
                entity.IsActive,
                true,
                entity.Items
                    .OrderBy(item => item.Epa.Code)
                    .Select(item => new CurriculumItemDto(item.Id, item.EpaId, item.Epa.Code, item.Epa.Title, item.RequiredCount, item.MinimumLevelOrder, item.WindowMonths, item.Weight, item.MinimumLevelByStageJson))
                    .ToList()))
            .ToListAsync(cancellationToken);
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
        var projection = await _dbContext.Set<Curriculum>()
            .Where(entity => entity.Id == request.Id)
            .Select(entity => new
            {
                Dto = new CurriculumDto(
                    entity.Id,
                    entity.SubSpeciality.SpecialityId,
                    entity.SubSpecialityId,
                    entity.SubSpeciality.Speciality.Name,
                    entity.SubSpeciality.Name,
                    entity.Name,
                    entity.Version,
                    entity.EffectiveFrom,
                    entity.EffectiveTo,
                    entity.IsActive,
                    true,
                    entity.Items
                        .OrderBy(item => item.Epa.Code)
                        .Select(item => new CurriculumItemDto(item.Id, item.EpaId, item.Epa.Code, item.Epa.Title, item.RequiredCount, item.MinimumLevelOrder, item.WindowMonths, item.Weight, item.MinimumLevelByStageJson))
                        .ToList()),
                CollegeId = entity.SubSpeciality.Speciality.CollegeId
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (projection is null)
        {
            return null;
        }

        if (!request.Principal.CanAccessCollege(projection.CollegeId))
        {
            return null;
        }

        return projection.Dto;
    }
}
