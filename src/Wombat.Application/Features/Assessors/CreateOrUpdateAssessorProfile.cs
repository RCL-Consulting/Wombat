using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Users;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Assessors;

public sealed record CreateOrUpdateAssessorProfileCommand(
    string UserId,
    string Qualifications,
    int InstitutionId,
    int? SpecialityId,
    int? SubSpecialityId,
    AssessorTrainingStatus TrainingStatus,
    DateOnly? TrainingCompletedOn,
    ClaimsPrincipal Principal) : IRequest<AssessorProfileDto>;

public sealed class CreateOrUpdateAssessorProfileCommandValidator : AbstractValidator<CreateOrUpdateAssessorProfileCommand>
{
    public CreateOrUpdateAssessorProfileCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Qualifications).NotEmpty().MaximumLength(4000);
        RuleFor(command => command.InstitutionId).GreaterThan(0);
        RuleFor(command => command.TrainingStatus).IsInEnum();
        RuleFor(command => command.SubSpecialityId)
            .Must((command, subSpecialityId) => !subSpecialityId.HasValue || command.SpecialityId.HasValue)
            .WithMessage("A sub-speciality requires a speciality.");
        // Note: a completion date is only meaningful for Provisional/Trained; rather than
        // rejecting a stale value (the edit form hides the field for other statuses), the
        // handler normalizes it away. See CreateOrUpdateAssessorProfileCommandHandler.
    }
}

public sealed class CreateOrUpdateAssessorProfileCommandHandler : IRequestHandler<CreateOrUpdateAssessorProfileCommand, AssessorProfileDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userAdministrationService;

    public CreateOrUpdateAssessorProfileCommandHandler(IApplicationDbContext dbContext, IUserAdministrationService userAdministrationService)
    {
        _dbContext = dbContext;
        _userAdministrationService = userAdministrationService;
    }

    /// <summary>
    /// The refusal to record a profile at an institution the assessor does not belong to. (T289)
    /// </summary>
    public const string AssessorAtAnotherInstitution =
        "An assessor's profile is recorded at the assessor's own institution. Move the user to another institution from " +
        "Users first.";

    /// <remarks>
    /// <para>
    /// <b>A trainee first</b> (<see cref="UserAdministrationRules.DemandUserAdministration" />, T278), asked before
    /// anything is looked up: the save writes the user's scope, and until T289 a registrar who also administered could
    /// move their own panel's chair to another discipline and stall their own review.
    /// </para>
    /// <para>
    /// <b>The assessor's own institution</b>, for every caller, the Administrator included (T289). The scope write sets
    /// the user's institution, and a move between institutions drops their external logins, so a form value naming
    /// another institution must not move anyone: an id outside the caller's institution is refused as one that names
    /// nobody (<see cref="UserAdministrationRules.DemandInScope" />), and an in-scope user is refused a profile anywhere
    /// but where they are. A move is Users' to make.
    /// </para>
    /// </remarks>
    public async Task<AssessorProfileDto> Handle(CreateOrUpdateAssessorProfileCommand request, CancellationToken cancellationToken)
    {
        UserAdministrationRules.DemandUserAdministration(request.Principal);

        if (!request.Principal.CanAccessInstitution(request.InstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to manage assessors in that institution.");
        }

        var user = UserAdministrationRules.DemandInScope(
            request.Principal,
            await _userAdministrationService.GetByIdAsync(request.UserId, cancellationToken),
            "You do not have permission to manage this assessor.");

        if (user.InstitutionId != request.InstitutionId)
        {
            throw new InvalidOperationException(AssessorAtAnotherInstitution);
        }

        if (!user.Roles.Contains(WombatRoles.Assessor, StringComparer.Ordinal))
        {
            throw new InvalidOperationException("Only users in the Assessor role can have an assessor profile.");
        }

        var institution = await _dbContext.Set<Institution>()
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == request.InstitutionId, cancellationToken)
            ?? throw new InvalidOperationException("The selected institution could not be found.");

        Speciality? speciality = null;
        if (request.SpecialityId.HasValue)
        {
            speciality = await _dbContext.Set<Speciality>()
                .AsNoTracking()
                .SingleOrDefaultAsync(entity => entity.Id == request.SpecialityId.Value, cancellationToken)
                ?? throw new InvalidOperationException("The selected speciality could not be found.");

            // Specialities are national now (T091); the assessor's speciality denotes a discipline, not an
            // institution tie, so no institution-consistency check is required.
        }

        SubSpeciality? subSpeciality = null;
        if (request.SubSpecialityId.HasValue)
        {
            subSpeciality = await _dbContext.Set<SubSpeciality>()
                .AsNoTracking()
                .SingleOrDefaultAsync(entity => entity.Id == request.SubSpecialityId.Value, cancellationToken)
                ?? throw new InvalidOperationException("The selected sub-speciality could not be found.");

            if (!request.SpecialityId.HasValue || subSpeciality.SpecialityId != request.SpecialityId.Value)
            {
                throw new InvalidOperationException("The selected sub-speciality does not belong to the selected speciality.");
            }
        }

        var profile = await _dbContext.Set<AssessorProfile>()
            .SingleOrDefaultAsync(entity => entity.UserId == request.UserId, cancellationToken);

        if (profile is null)
        {
            profile = new AssessorProfile
            {
                UserId = request.UserId
            };

            _dbContext.Set<AssessorProfile>().Add(profile);
        }
        else if (!request.Principal.CanAccessInstitution(profile.InstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to update this assessor profile.");
        }

        profile.Qualifications = request.Qualifications.Trim();
        profile.InstitutionId = request.InstitutionId;
        profile.SpecialityId = request.SpecialityId;
        profile.SubSpecialityId = request.SubSpecialityId;
        profile.TrainingStatus = request.TrainingStatus;
        profile.TrainingCompletedOn =
            request.TrainingStatus is AssessorTrainingStatus.Provisional or AssessorTrainingStatus.Trained
                ? request.TrainingCompletedOn
                : null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _userAdministrationService.UpdateScopeAsync(
            request.UserId,
            request.InstitutionId,
            request.SpecialityId.HasValue ? [request.SpecialityId.Value] : [],
            request.SubSpecialityId.HasValue ? [request.SubSpecialityId.Value] : [],
            cancellationToken);

        return new AssessorProfileDto(
            profile.Id,
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            profile.Qualifications,
            institution.Id,
            institution.Name,
            speciality?.Id,
            speciality?.Name,
            subSpeciality?.Id,
            subSpeciality?.Name,
            profile.TrainingStatus,
            profile.TrainingCompletedOn);
    }
}
