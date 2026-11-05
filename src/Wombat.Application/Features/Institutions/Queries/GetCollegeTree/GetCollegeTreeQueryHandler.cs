using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions.Queries.GetCollegeTree;

/// <summary>
/// The tree in one query (T394, flow 16, Q1): until T394 the Specialities list read the College, then its specialities,
/// and each speciality's Sub-specialities list read its own rows, with only the default scale's id.
/// </summary>
public sealed class GetCollegeTreeQueryHandler : IRequestHandler<GetCollegeTreeQuery, CollegeTreeDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCollegeTreeQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CollegeTreeDto?> Handle(GetCollegeTreeQuery request, CancellationToken cancellationToken)
    {
        // Another College's tree reads as one that does not exist (Q7; 404, not 403), so nothing is read for it.
        if (!request.Principal.CanAccessCollege(request.CollegeId))
        {
            return null;
        }

        return await _dbContext.Set<College>()
            .AsNoTracking()
            .Where(college => college.Id == request.CollegeId)
            .Select(college => new CollegeTreeDto(
                college.Id,
                college.Name,
                college.ShortCode,
                college.Specialities
                    .OrderBy(speciality => speciality.Name)
                    .Select(speciality => new CollegeTreeSpecialityDto(
                        speciality.Id,
                        speciality.Name,
                        speciality.Description,
                        speciality.IsActive,
                        speciality.SubSpecialities
                            .OrderBy(subSpeciality => subSpeciality.Name)
                            .Select(subSpeciality => new CollegeTreeSubSpecialityDto(
                                subSpeciality.Id,
                                subSpeciality.SpecialityId,
                                subSpeciality.Name,
                                subSpeciality.Description,
                                subSpeciality.IsActive,
                                subSpeciality.DefaultEntrustmentScaleId,
                                subSpeciality.DefaultEntrustmentScale == null ? null : subSpeciality.DefaultEntrustmentScale.Name))
                            .ToList()))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
