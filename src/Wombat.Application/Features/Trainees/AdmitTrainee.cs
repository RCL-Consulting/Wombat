using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Curricula.Quota;
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
    /// <summary>
    /// The refusal to admit a registrar who already holds the Trainee role beside PendingTrainee. (T303 review)
    /// </summary>
    /// <remarks>
    /// Admission is what grants Trainee, and <c>PromotePendingTraineeAsync</c> fails on a role already held. It runs last,
    /// after the profile and the scope are saved, so until the T303 review such an admission was committed part-way under
    /// a refusal. The Users page no longer adds Trainee (T303); an SSO group mapping to Trainee still can.
    /// </remarks>
    public const string AlreadyHoldsTrainee =
        "This registrar already holds the Trainee role, which only admission should grant, so they cannot be admitted " +
        "until it is taken away. An SSO group mapping to Trainee is the one way the role is given outside admission.";

    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userAdministrationService;
    private readonly TimeProvider _clock;

    /// <param name="clock">What "today" is read from, for the admission day (T358, D1); the system clock unless a test pins it.</param>
    public AdmitTraineeCommandHandler(
        IApplicationDbContext dbContext,
        IUserAdministrationService userAdministrationService,
        TimeProvider? clock = null)
    {
        _dbContext = dbContext;
        _userAdministrationService = userAdministrationService;
        _clock = clock ?? TimeProvider.System;
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

        // Before anything is written: the role promotion below would fail on it after the profile and the scope are saved.
        if (user.Roles.Contains(WombatRoles.Trainee, StringComparer.Ordinal))
        {
            throw new InvalidOperationException(AlreadyHoldsTrainee);
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

        // The trainee is pinned to the national curriculum version their institution has adopted for
        // the discipline; admission into a non-adopted version (or discipline) is rejected (T091 phase 4).
        var adoptionId = await TraineeAdoptionResolver.ResolveAdoptionIdAsync(_dbContext, institutionId, curriculum, cancellationToken);

        var expectedCompletionDate = request.ExpectedCompletionDate
            ?? request.ProgrammeStartDate.AddMonths(GetDefaultCompletionMonths(curriculum));

        var profile = new TraineeProfile
        {
            UserId = request.UserId,
            InstitutionId = institutionId,
            CurriculumId = curriculum.Id,
            AdoptionId = adoptionId,
            ProgrammeStartDate = request.ProgrammeStartDate,

            // The day of the admission itself, on the South African calendar (T358, D1): what "Nothing filed in 30 days"
            // counts from when it is later than today − 30 (E5), never the programme's start.
            AdmittedOn = QuotaCalendar.Today(_clock),
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
