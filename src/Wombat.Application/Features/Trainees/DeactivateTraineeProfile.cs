using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Trainees;

/// <summary>
/// Ends a trainee's programme without completing it (a withdrawal): records the trainee's last day and deactivates the
/// profile. The day is what the quota reads for D49 (T209): the period it falls in holds no target unless it falls in that
/// period's last month, and the periods after it are outside the programme. It is the day the trainee left, which may be
/// before the day an administrator records it, as a completion date is, but never after today on the South African
/// calendar: the profile ends now, and a later day would hold the trainee to every period up to it (T209 review).
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
    private readonly TimeProvider _timeProvider;

    public DeactivateTraineeProfileCommandHandler(IApplicationDbContext dbContext, TimeProvider? timeProvider = null)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task Handle(DeactivateTraineeProfileCommand request, CancellationToken cancellationToken)
    {
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

        // Deactivate checks that the profile is active and that the day lies between the programme start and today before
        // it changes anything, so a refusal leaves nothing for the audit pipeline to commit.
        profile.Deactivate(request.DeactivatedOn, QuotaCalendar.Today(_timeProvider));
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
