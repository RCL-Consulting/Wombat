using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Trainees;

public sealed record UpdateTraineeProfileCommand(
    int Id,
    int CurriculumId,
    DateOnly ProgrammeStartDate,
    DateOnly? ExpectedCompletionDate,
    ClaimsPrincipal Principal) : IRequest<TraineeProfileDto>;

public sealed class UpdateTraineeProfileCommandValidator : AbstractValidator<UpdateTraineeProfileCommand>
{
    public UpdateTraineeProfileCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
        RuleFor(command => command.CurriculumId).GreaterThan(0);
    }
}

public sealed class UpdateTraineeProfileCommandHandler : IRequestHandler<UpdateTraineeProfileCommand, TraineeProfileDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userAdministrationService;

    public UpdateTraineeProfileCommandHandler(IApplicationDbContext dbContext, IUserAdministrationService userAdministrationService)
    {
        _dbContext = dbContext;
        _userAdministrationService = userAdministrationService;
    }

    public async Task<TraineeProfileDto> Handle(UpdateTraineeProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _dbContext.Set<TraineeProfile>()
            .Include(entity => entity.Curriculum)
                .ThenInclude(entity => entity.SubSpeciality)
                    .ThenInclude(entity => entity.Speciality)
            .SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException("The trainee profile could not be found.");

        if (!request.Principal.CanAccessInstitution(profile.InstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to update this trainee profile.");
        }

        var curriculum = await _dbContext.Set<Curriculum>()
            .Include(entity => entity.SubSpeciality)
                .ThenInclude(entity => entity.Speciality)
            .Include(entity => entity.Items)
            .SingleOrDefaultAsync(entity => entity.Id == request.CurriculumId, cancellationToken)
            ?? throw new InvalidOperationException("The selected curriculum could not be found.");

        // The curriculum is a national catalogue version (T091); the trainee stays in their own institution.
        // Re-pin to the institution's active adoption for the (possibly changed) curriculum's discipline,
        // rejecting a version the institution has not adopted (T091 phase 4).
        profile.AdoptionId = await TraineeAdoptionResolver.ResolveAdoptionIdAsync(_dbContext, profile.InstitutionId, curriculum, cancellationToken);
        profile.CurriculumId = request.CurriculumId;
        profile.ProgrammeStartDate = request.ProgrammeStartDate;
        profile.ExpectedCompletionDate = request.ExpectedCompletionDate
            ?? request.ProgrammeStartDate.AddMonths(AdmitTraineeCommandHandler.GetDefaultCompletionMonths(curriculum));

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _userAdministrationService.UpdateScopeAsync(
            profile.UserId,
            profile.InstitutionId,
            [curriculum.SubSpeciality.SpecialityId],
            [curriculum.SubSpecialityId],
            cancellationToken);

        var user = await _userAdministrationService.GetByIdAsync(profile.UserId, cancellationToken)
            ?? throw new InvalidOperationException("The trainee user could not be found.");

        return new TraineeProfileDto(
            profile.Id,
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            curriculum.Id,
            curriculum.Name,
            curriculum.Version,
            curriculum.SubSpeciality.SpecialityId,
            curriculum.SubSpeciality.Speciality.Name,
            curriculum.SubSpecialityId,
            curriculum.SubSpeciality.Name,
            profile.ProgrammeStartDate,
            profile.ExpectedCompletionDate,
            profile.IsActive);
    }
}
