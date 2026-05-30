using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Assessors;

public sealed record GetAssessorProfileByIdQuery(int Id, ClaimsPrincipal Principal) : IRequest<AssessorProfileDto>;

public sealed class GetAssessorProfileByIdQueryHandler : IRequestHandler<GetAssessorProfileByIdQuery, AssessorProfileDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userAdministrationService;

    public GetAssessorProfileByIdQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService userAdministrationService)
    {
        _dbContext = dbContext;
        _userAdministrationService = userAdministrationService;
    }

    public async Task<AssessorProfileDto> Handle(GetAssessorProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var profile = await _dbContext.Set<AssessorProfile>()
            .AsNoTracking()
            .Include(entity => entity.Institution)
            .Include(entity => entity.Speciality)
            .Include(entity => entity.SubSpeciality)
            .SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException("The assessor profile could not be found.");

        if (!request.Principal.CanAccessInstitution(profile.InstitutionId))
        {
            throw new InvalidOperationException("The assessor profile could not be found.");
        }

        var user = await _userAdministrationService.GetByIdAsync(profile.UserId, cancellationToken)
            ?? throw new InvalidOperationException("The assessor user could not be found.");

        return new AssessorProfileDto(
            profile.Id,
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            profile.Qualifications,
            profile.InstitutionId,
            profile.Institution.Name,
            profile.SpecialityId,
            profile.Speciality?.Name,
            profile.SubSpecialityId,
            profile.SubSpeciality?.Name,
            profile.TrainingStatus,
            profile.TrainingCompletedOn);
    }
}
