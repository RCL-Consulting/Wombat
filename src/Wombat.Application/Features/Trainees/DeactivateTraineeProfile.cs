using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Services;
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
/// keeps every completion of the trainee's out until it commits (<see cref="ITraineeCreditLock" />).
/// </summary>
public sealed record DeactivateTraineeProfileCommand(int Id, DateOnly DeactivatedOn, ClaimsPrincipal Principal) : IRequest;

public sealed class DeactivateTraineeProfileCommandValidator : AbstractValidator<DeactivateTraineeProfileCommand>
{
    public DeactivateTraineeProfileCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class DeactivateTraineeProfileCommandHandler : IRequestHandler<DeactivateTraineeProfileCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICreditApplier _creditApplier;
    private readonly ITraineeCreditLock _traineeCreditLock;
    private readonly TimeProvider _timeProvider;

    public DeactivateTraineeProfileCommandHandler(
        IApplicationDbContext dbContext,
        ICreditApplier creditApplier,
        ITraineeCreditLock traineeCreditLock,
        TimeProvider? timeProvider = null)
    {
        _dbContext = dbContext;
        _creditApplier = creditApplier;
        _traineeCreditLock = traineeCreditLock;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task Handle(DeactivateTraineeProfileCommand request, CancellationToken cancellationToken)
    {
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

        // T281: whether the last day takes back credit already given for encounters after it. Read before the end is
        // recorded, like every other read that can fail.
        var takesCreditBack = await ProgrammeEndCredit.TakesCreditBackAsync(
            _dbContext.Set<Activity>(), profile.UserId, request.DeactivatedOn, cancellationToken);

        // Deactivate checks that the profile is active and that the day lies between the programme start and today before
        // it changes anything, so a refusal leaves nothing for the audit pipeline to commit.
        profile.Deactivate(request.DeactivatedOn, QuotaCalendar.Today(_timeProvider));

        // Saved with a replay of the trainee's credit when the end takes some back, and put back if that replay fails.
        await ProgrammeEndCredit.SaveAsync(
            _dbContext, _creditApplier, _dbContext.Set<Activity>(), profile, takesCreditBack, cancellationToken);
        await hold.CommitAsync(cancellationToken);
    }
}
