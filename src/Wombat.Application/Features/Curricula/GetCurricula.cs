using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;

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
            var scopedInstitutionId = request.Principal.GetInstitutionId();
            if (!scopedInstitutionId.HasValue)
            {
                return Array.Empty<CurriculumDto>();
            }

            query = query.Where(entity => entity.SubSpeciality.Speciality.InstitutionId == scopedInstitutionId.Value);
        }

        return await query
            .OrderBy(entity => entity.SubSpeciality.Speciality.Institution.Name)
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
                InstitutionId = entity.SubSpeciality.Speciality.InstitutionId
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (projection is null)
        {
            return null;
        }

        if (!request.Principal.CanAccessInstitution(projection.InstitutionId))
        {
            return null;
        }

        return projection.Dto;
    }
}
