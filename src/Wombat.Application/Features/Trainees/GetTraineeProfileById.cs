using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Trainees;

public sealed record GetTraineeProfileByIdQuery(int Id, ClaimsPrincipal Principal) : IRequest<TraineeProfileDto>;

public sealed class GetTraineeProfileByIdQueryHandler : IRequestHandler<GetTraineeProfileByIdQuery, TraineeProfileDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userAdministrationService;

    public GetTraineeProfileByIdQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService userAdministrationService)
    {
        _dbContext = dbContext;
        _userAdministrationService = userAdministrationService;
    }

    public async Task<TraineeProfileDto> Handle(GetTraineeProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var profile = await _dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .Include(entity => entity.Curriculum)
                .ThenInclude(entity => entity.SubSpeciality)
                    .ThenInclude(entity => entity.Speciality)
            .SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException("The trainee profile could not be found.");

        if (!request.Principal.CanAccessInstitution(profile.InstitutionId))
        {
            throw new InvalidOperationException("The trainee profile could not be found.");
        }

        var user = await _userAdministrationService.GetByIdAsync(profile.UserId, cancellationToken)
            ?? throw new InvalidOperationException("The trainee user could not be found.");

        return new TraineeProfileDto(
            profile.Id,
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            profile.CurriculumId,
            profile.Curriculum.Name,
            profile.Curriculum.Version,
            profile.Curriculum.SubSpeciality.SpecialityId,
            profile.Curriculum.SubSpeciality.Speciality.Name,
            profile.Curriculum.SubSpecialityId,
            profile.Curriculum.SubSpeciality.Name,
            profile.ProgrammeStartDate,
            profile.ExpectedCompletionDate,
            profile.IsActive,
            profile.CompletedOn);
    }
}
