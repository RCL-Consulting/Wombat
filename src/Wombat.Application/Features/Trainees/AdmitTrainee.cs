using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Email.Templates;
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
    /// a refusal. Neither the Users page (T303) nor an SSO group mapping (T333) gives Trainee, so only a row left from before
    /// them can hold it here.
    /// </remarks>
    public const string AlreadyHoldsTrainee =
        "This registrar already holds the Trainee role, which only admission grants, so they cannot be admitted until it " +
        "is taken away.";

    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userAdministrationService;
    private readonly AccountNotices _notices;
    private readonly TimeProvider _clock;

    /// <param name="notices">Sends the admission's mail after the save (T373, flow 11, Q7).</param>
    /// <param name="clock">What "today" is read from, for the admission day (T358, D1); the system clock unless a test pins it.</param>
    public AdmitTraineeCommandHandler(
        IApplicationDbContext dbContext,
        IUserAdministrationService userAdministrationService,
        AccountNotices notices,
        TimeProvider? clock = null)
    {
        _dbContext = dbContext;
        _userAdministrationService = userAdministrationService;
        _notices = notices;
        _clock = clock ?? TimeProvider.System;
    }

    /// <remarks>
    /// Every refusal is a <see cref="TraineeAdmissionRefusedException" /> (T373, flow 11, S9) thrown before anything is
    /// staged, so the audit pipeline, which saves what a failed handler left staged (T201), commits nothing. In order: no
    /// registrar waiting in the caller's scope (one reason for an unknown id, one not waiting and one out of scope, so the
    /// audit row names nobody of another institution); an active profile (a second Admit from a stale tab, M09); Trainee
    /// held already; a deactivated account (S6); no institution (M04b); nothing adopted (M04); a curriculum not offered
    /// (F1); an empty completion whose derived date has passed (F4).
    /// </remarks>
    public async Task<TraineeProfileDto> Handle(AdmitTraineeCommand request, CancellationToken cancellationToken)
    {
        TraineeAdministrationRules.DemandTraineeAdministration(request.Principal);

        var user = await _userAdministrationService.GetByIdAsync(request.UserId, cancellationToken);

        // Out of scope reads as not waiting (T056, M08): the refusal says nothing about another institution's account.
        if (user is null || !AdmissionScope.IsInScope(user, request.Principal))
        {
            throw new TraineeAdmissionRefusedException(TraineeAdmissionRefusal.NotPending);
        }

        // Before the role check: once admitted, the registrar no longer holds PendingTrainee, and a stale tab's second
        // Admit is told what happened, not that nobody is waiting (M09).
        var existingActiveProfile = await _dbContext.Set<TraineeProfile>()
            .AnyAsync(profile => profile.UserId == request.UserId && profile.IsActive, cancellationToken);
        if (existingActiveProfile)
        {
            throw new TraineeAdmissionRefusedException(TraineeAdmissionRefusal.AlreadyAdmitted);
        }

        if (!user.Roles.Contains(WombatRoles.PendingTrainee, StringComparer.Ordinal))
        {
            throw new TraineeAdmissionRefusedException(TraineeAdmissionRefusal.NotPending);
        }

        // Before anything is written: the role promotion below would fail on it after the profile and the scope are saved.
        if (user.Roles.Contains(WombatRoles.Trainee, StringComparer.Ordinal))
        {
            throw new TraineeAdmissionRefusedException(TraineeAdmissionRefusal.HoldsTrainee);
        }

        // S6: a deactivated account is left off Waiting, and refused here, as an account no one can sign in to.
        if (user.IsDeactivated)
        {
            throw new TraineeAdmissionRefusedException(TraineeAdmissionRefusal.Deactivated);
        }

        // The trainee's institution is the one they belong to (T091). Until T373 a registrar of none was admitted into the
        // caller's; now only an Administrator reaches one, and is told no curriculum can be offered (M04b).
        var institutionId = user.InstitutionId
            ?? throw new TraineeAdmissionRefusedException(TraineeAdmissionRefusal.NoInstitution);

        // F1: only what the page offered. Invited to a sub-speciality, the one curriculum adopted for it; invited to none,
        // one of the institution's adopted curricula, from which the scope is then set.
        var offered = await AdmissionCurricula.Offered(_dbContext, institutionId, user)
            .Select(curriculum => curriculum.Id)
            .ToListAsync(cancellationToken);
        if (offered.Count == 0)
        {
            throw new TraineeAdmissionRefusedException(TraineeAdmissionRefusal.NothingAdopted);
        }

        if (!offered.Contains(request.CurriculumId))
        {
            throw new TraineeAdmissionRefusedException(TraineeAdmissionRefusal.CurriculumNotOffered);
        }

        var curriculum = await _dbContext.Set<Curriculum>()
            .Include(entity => entity.SubSpeciality)
                .ThenInclude(entity => entity.Speciality)
            .Include(entity => entity.Items)
            .SingleAsync(entity => entity.Id == request.CurriculumId, cancellationToken);

        // The trainee is pinned to the national curriculum version their institution has adopted for the discipline
        // (T091 phase 4); F1 has already narrowed the choice to it.
        var adoptionId = await TraineeAdoptionResolver.ResolveAdoptionIdAsync(_dbContext, institutionId, curriculum, cancellationToken);

        var today = QuotaCalendar.Today(_clock);

        // F4: an empty completion is derived from the curriculum window; one that would already have passed is refused
        // beside the field, in the words the form's hint uses (M15, check rule 7).
        var expectedCompletionDate = request.ExpectedCompletionDate
            ?? AdmissionDates.DerivedCompletion(request.ProgrammeStartDate, GetDefaultCompletionMonths(curriculum));
        if (request.ExpectedCompletionDate is null && AdmissionDates.HasPassed(expectedCompletionDate, today))
        {
            throw new TraineeAdmissionRefusedException(TraineeAdmissionRefusal.CompletionPassed, expectedCompletionDate);
        }

        var profile = new TraineeProfile
        {
            UserId = request.UserId,
            InstitutionId = institutionId,
            CurriculumId = curriculum.Id,
            AdoptionId = adoptionId,
            ProgrammeStartDate = request.ProgrammeStartDate,

            // The day of the admission itself, on the South African calendar (T358, D1): what "Nothing filed in 30 days"
            // counts from when it is later than today - 30 (E5), never the programme's start.
            AdmittedOn = today,
            ExpectedCompletionDate = expectedCompletionDate,
            IsActive = true
        };

        // One transaction (T373 fix pass, B2), as the accept's (F10): the profile, the scope and the role swap commit
        // together or not at all. Until the fix each committed on its own, and a swap that failed after PendingTrainee was
        // taken away left a registrar with a profile and no role, whom a retry refused as already admitted.
        await _userAdministrationService.InOneTransactionAsync(async ct =>
        {
            _dbContext.Set<TraineeProfile>().Add(profile);
            await _dbContext.SaveChangesAsync(ct);

            // F1: the invited scope is kept; a registrar invited to none (M14) takes the scope of the curriculum chosen.
            // Until T373 every admission rewrote the scope to the chosen curriculum's discipline.
            if (!AdmissionCurricula.HasInvitedScope(user))
            {
                await _userAdministrationService.UpdateScopeAsync(
                    request.UserId,
                    institutionId,
                    [curriculum.SubSpeciality.SpecialityId],
                    [curriculum.SubSpecialityId],
                    ct);
            }

            await _userAdministrationService.PromotePendingTraineeAsync(request.UserId, ct);
            return true;
        }, cancellationToken);

        // Q7 (decided, T367): the registrar is told, after the commit, so a refused admission mails nobody;
        // a failed send is logged and the admission stands (AccountNotices). The page claims no delivery.
        var trainingYear = TraineeProfile.StageOn(profile.ProgrammeStartDate, today);
        await _notices.SendAsync(request.UserId, "admission", registrar => Task.FromResult(AdmissionEmail.Build(
            registrar.Email,
            curriculum.Name,
            curriculum.Version,
            trainingYear,
            profile.ProgrammeStartDate,
            _notices.LinkTo("/account/login"))));

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
            profile.IsActive)
        {
            TrainingYear = trainingYear
        };
    }

    internal static int GetDefaultCompletionMonths(Curriculum curriculum)
        => AdmissionDates.CompletionMonths(curriculum.Items.Select(item => item.WindowMonths));
}
