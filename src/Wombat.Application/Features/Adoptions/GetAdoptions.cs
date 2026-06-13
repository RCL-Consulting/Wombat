using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Adoptions;

/// <summary>
/// Lists an institution's curriculum adoptions. An Administrator may scope to any institution (or all);
/// an InstitutionalAdmin only ever sees their own institution's adoptions. (T091 phase 4.)
/// </summary>
public sealed record ListAdoptionsQuery(int? InstitutionId, ClaimsPrincipal Principal) : IRequest<IReadOnlyList<AdoptionDto>>
{
    public ListAdoptionsQuery(ClaimsPrincipal principal) : this(null, principal) { }
}

public sealed class ListAdoptionsQueryHandler : IRequestHandler<ListAdoptionsQuery, IReadOnlyList<AdoptionDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListAdoptionsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AdoptionDto>> Handle(ListAdoptionsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Set<InstitutionCurriculumAdoption>().AsQueryable();

        if (request.Principal.IsAdministrator())
        {
            if (request.InstitutionId.HasValue)
            {
                query = query.Where(entity => entity.InstitutionId == request.InstitutionId.Value);
            }
        }
        else
        {
            var scopedInstitutionId = request.Principal.GetInstitutionId();
            if (!scopedInstitutionId.HasValue)
            {
                return Array.Empty<AdoptionDto>();
            }

            query = query.Where(entity => entity.InstitutionId == scopedInstitutionId.Value);
        }

        return await query
            .OrderBy(entity => entity.Curriculum.SubSpeciality.Speciality.Name)
            .ThenBy(entity => entity.Curriculum.SubSpeciality.Name)
            .ThenByDescending(entity => entity.IsActive)
            .ThenByDescending(entity => entity.AdoptedOn)
            .Select(entity => new AdoptionDto(
                entity.Id,
                entity.InstitutionId,
                entity.CurriculumId,
                entity.Curriculum.Name,
                entity.Curriculum.Version,
                entity.SubSpecialityId,
                entity.Curriculum.SubSpeciality.Name,
                entity.Curriculum.SubSpeciality.Speciality.Name,
                entity.Curriculum.SubSpeciality.Speciality.College.Name,
                entity.AdoptedOn,
                entity.IsActive))
            .ToListAsync(cancellationToken);
    }
}
