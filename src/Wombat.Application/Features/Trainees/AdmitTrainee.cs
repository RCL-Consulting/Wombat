using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Trainees;

public sealed record AdmitTraineeCommand(
    string UserId,
    int CurriculumId,
    DateOnly ProgrammeStartDate,
    DateOnly? ExpectedCompletionDate,
    ClaimsPrincipal Principal) : IRequest<TraineeProfileDto>;

public sealed class AdmitTraineeCommandValidator : AbstractValidator<AdmitTraineeCommand>
{
    public AdmitTraineeCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.CurriculumId).GreaterThan(0);
    }
}

public sealed class AdmitTraineeCommandHandler : IRequestHandler<AdmitTraineeCommand, TraineeProfileDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userAdministrationService;

    public AdmitTraineeCommandHandler(IApplicationDbContext dbContext, IUserAdministrationService userAdministrationService)
    {
        _dbContext = dbContext;
        _userAdministrationService = userAdministrationService;
    }

    public async Task<TraineeProfileDto> Handle(AdmitTraineeCommand request, CancellationToken cancellationToken)
    {
        var user = await _userAdministrationService.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new InvalidOperationException("The trainee user could not be found.");

        if (!user.Roles.Contains(WombatRoles.PendingTrainee, StringComparer.Ordinal))
        {
            throw new InvalidOperationException("Only users in the PendingTrainee role can be admitted.");
        }

        // Pending trainee must be in caller's institution.
        if (user.InstitutionId.HasValue && !request.Principal.CanAccessInstitution(user.InstitutionId.Value))
        {
            throw new UnauthorizedAccessException("You do not have permission to admit this trainee.");
        }

        var existingActiveProfile = await _dbContext.Set<TraineeProfile>()
            .AnyAsync(profile => profile.UserId == request.UserId && profile.IsActive, cancellationToken);

        if (existingActiveProfile)
        {
            throw new InvalidOperationException("This user already has an active trainee profile.");
        }

        var curriculum = await _dbContext.Set<Curriculum>()
            .Include(entity => entity.SubSpeciality)
                .ThenInclude(entity => entity.Speciality)
            .Include(entity => entity.Items)
            .SingleOrDefaultAsync(entity => entity.Id == request.CurriculumId, cancellationToken)
            ?? throw new InvalidOperationException("The selected curriculum could not be found.");

        // The curriculum is now a national (College-owned) catalogue version, so the trainee's institution
        // is the institution they belong to, not one derived from the curriculum (T091). Adoption-based
        // gating of which national curricula an institution may admit into arrives in phase 4.
        var institutionId = user.InstitutionId
            ?? request.Principal.GetInstitutionId()
            ?? throw new InvalidOperationException("The trainee's institution could not be determined.");

        var expectedCompletionDate = request.ExpectedCompletionDate
            ?? request.ProgrammeStartDate.AddMonths(GetDefaultCompletionMonths(curriculum));

        var profile = new TraineeProfile
        {
            UserId = request.UserId,
            InstitutionId = institutionId,
            CurriculumId = curriculum.Id,
            ProgrammeStartDate = request.ProgrammeStartDate,
            ExpectedCompletionDate = expectedCompletionDate,
            IsActive = true
        };

        _dbContext.Set<TraineeProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _userAdministrationService.UpdateScopeAsync(
            request.UserId,
            institutionId,
            [curriculum.SubSpeciality.SpecialityId],
            [curriculum.SubSpecialityId],
            cancellationToken);

        await _userAdministrationService.PromotePendingTraineeAsync(request.UserId, cancellationToken);

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

    internal static int GetDefaultCompletionMonths(Curriculum curriculum)
        => Math.Max(1, curriculum.Items.Select(item => item.WindowMonths).DefaultIfEmpty(12).Max());
}
