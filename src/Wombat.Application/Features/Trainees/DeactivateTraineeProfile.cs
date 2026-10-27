using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Trainees;

/// <summary>
/// Ends a trainee's programme without completing it (a withdrawal): records the trainee's last day and deactivates the
/// profile. The day is what the quota reads for D49 (T209): the period it falls in holds no target unless it falls in that
/// period's last month, and the periods after it are outside the programme. It is the day the trainee left, which may be
/// before the day an administrator records it, as a completion date is, but never after today on the South African
/// calendar: the profile ends now, and a later day would hold the trainee to every period up to it (T209 review).
/// An encounter observed after the day credits nothing on the profile, so recording it takes back any credit such an
/// encounter already earned, in the same save (T281, <see cref="ProgrammeEndCredit" />), under a hold on the profile that
/// keeps every completion of the trainee's out until it commits (<see cref="ITraineeCreditLock" />). It takes the Trainee
/// role away, as a completion does (T290): a withdrawn registrar files nothing more, since nothing after their last day
/// could credit, and reads their own record through the profile they ended on (T252's <c>trainee_record</c> claim). The
/// same save keeps the list the programme ended with (T312, <see cref="ProgrammeItems.ReadEndedWithAsync" />), which that
/// record reads from then on. It answers with how many activities' credit it took back (T379, flow 13, F2:
/// <see cref="ProgrammeEndResult" />).
/// </summary>
public sealed record DeactivateTraineeProfileCommand(int Id, DateOnly DeactivatedOn, ClaimsPrincipal Principal) : IRequest<ProgrammeEndResult>;

public sealed class DeactivateTraineeProfileCommandValidator : AbstractValidator<DeactivateTraineeProfileCommand>
{
    public DeactivateTraineeProfileCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class DeactivateTraineeProfileCommandHandler : IRequestHandler<DeactivateTraineeProfileCommand, ProgrammeEndResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userAdministrationService;
    private readonly ICreditApplier _creditApplier;
    private readonly ITraineeCreditLock _traineeCreditLock;
    private readonly TimeProvider _timeProvider;

    public DeactivateTraineeProfileCommandHandler(
        IApplicationDbContext dbContext,
        IUserAdministrationService userAdministrationService,
        ICreditApplier creditApplier,
        ITraineeCreditLock traineeCreditLock,
        TimeProvider? timeProvider = null)
    {
        _dbContext = dbContext;
        _userAdministrationService = userAdministrationService;
        _creditApplier = creditApplier;
        _traineeCreditLock = traineeCreditLock;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<ProgrammeEndResult> Handle(DeactivateTraineeProfileCommand request, CancellationToken cancellationToken)
    {
        TraineeAdministrationRules.DemandTraineeAdministration(request.Principal);

        // T281. Held before the profile is read, and until the save below commits, so no completion of the trainee's can
        // credit in between: one in flight is waited for, and its credit is then read below and taken back if it is after
        // the last day; one that comes after waits for this save and reads the end. A second end of the same profile waits
        // too, and then finds it no longer active. Before the scope check, which needs the profile read; a refused caller
        // holds it only until the throw disposes the hold (ProgrammeEndCredit).
        await using var hold = await _traineeCreditLock.HoldForEndAsync(request.Id, cancellationToken);

        var profile = await _dbContext.Set<TraineeProfile>()
            .Include(entity => entity.Curriculum)
                .ThenInclude(entity => entity.SubSpeciality)
                    .ThenInclude(entity => entity.Speciality)
            .SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException("The trainee profile could not be found.");

        if (!request.Principal.CanAccessInstitution(profile.InstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to deactivate this trainee profile.");
        }

        // T281: how many activities the last day takes back credit from, given for encounters after it. Read before the end
        // is recorded, like every other read that can fail; the answer is what the command returns (T379, F2).
        var takenBack = await ProgrammeEndCredit.CountTakenBackAsync(
            _dbContext.Set<Activity>(), profile.UserId, request.DeactivatedOn, cancellationToken);

        // T312: the list the programme ends with, each item in force for the trainee on the last day with its target then,
        // which the record reads from now on whatever the catalogue does. Read here, before the end is recorded, like
        // every other read that can fail; kept with the end below, and so saved with it and put back with it.
        var endedWith = await ProgrammeItems.ReadEndedWithAsync(
            _dbContext, profile, request.DeactivatedOn, _timeProvider.GetUtcNow().UtcDateTime, cancellationToken);

        // Deactivate checks that the profile is active and that the day lies between the programme start and today before
        // it changes anything, so a refusal leaves nothing for the audit pipeline to commit.
        profile.Deactivate(request.DeactivatedOn, QuotaCalendar.Today(_timeProvider));
        profile.RecordEndedCurriculum(endedWith);

        // Saved with a replay of the trainee's credit when the end takes some back, and put back if that replay fails.
        await ProgrammeEndCredit.SaveAsync(
            _dbContext, _creditApplier, _dbContext.Set<Activity>(), profile, takenBack, cancellationToken);
        await hold.CommitAsync(cancellationToken);

        // As a completion (T290): the role goes with the programme, after the save, which writes nothing the hold is for.
        await _userAdministrationService.RemoveRoleAsync(profile.UserId, WombatRoles.Trainee, cancellationToken);

        return new ProgrammeEndResult(takenBack);
    }
}
