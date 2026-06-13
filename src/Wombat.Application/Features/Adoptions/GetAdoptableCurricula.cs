using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Adoptions;

/// <summary>A national curriculum version an institution can adopt. (T091 phase 5.)</summary>
public sealed record AdoptableCurriculumDto(
    int Id,
    int SubSpecialityId,
    string CollegeName,
    string SpecialityName,
    string SubSpecialityName,
    string Name,
    string Version);

/// <summary>
/// Lists the active national curriculum versions an institution may adopt — the whole shared catalogue,
/// since institutions choose what to adopt (distinct from <see cref="GetAdoptions"/>, which lists what they
/// already adopted, and from the InstitutionalAdmin-narrowed curriculum list). Available to Administrators
/// and InstitutionalAdmins. (T091 phase 5.)
/// </summary>
public sealed record GetAdoptableCurriculaQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<AdoptableCurriculumDto>>;

public sealed class GetAdoptableCurriculaQueryHandler : IRequestHandler<GetAdoptableCurriculaQuery, IReadOnlyList<AdoptableCurriculumDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAdoptableCurriculaQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AdoptableCurriculumDto>> Handle(GetAdoptableCurriculaQuery request, CancellationToken cancellationToken)
    {
        if (!request.Principal.IsAdministrator() && !request.Principal.IsInstitutionalAdmin())
        {
            return Array.Empty<AdoptableCurriculumDto>();
        }

        return await _dbContext.Set<Curriculum>()
            .Where(entity => entity.IsActive)
            .OrderBy(entity => entity.SubSpeciality.Speciality.College.Name)
            .ThenBy(entity => entity.SubSpeciality.Speciality.Name)
            .ThenBy(entity => entity.SubSpeciality.Name)
            .ThenBy(entity => entity.Name)
            .ThenByDescending(entity => entity.EffectiveFrom)
            .Select(entity => new AdoptableCurriculumDto(
                entity.Id,
                entity.SubSpecialityId,
                entity.SubSpeciality.Speciality.College.Name,
                entity.SubSpeciality.Speciality.Name,
                entity.SubSpeciality.Name,
                entity.Name,
                entity.Version))
            .ToListAsync(cancellationToken);
    }
}
