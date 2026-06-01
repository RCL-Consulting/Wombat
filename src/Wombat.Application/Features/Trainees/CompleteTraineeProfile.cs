using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Trainees;

/// <summary>
/// Marks a trainee's programme complete (graduation): records the completion date, deactivates the
/// profile, removes the Trainee role (there is no Alumnus role — the profile is archived), and emails
/// the graduate. (T080 / F-5-4)
/// </summary>
public sealed record CompleteTraineeProfileCommand(int Id, DateOnly CompletedOn, ClaimsPrincipal Principal) : IRequest;

public sealed class CompleteTraineeProfileCommandValidator : AbstractValidator<CompleteTraineeProfileCommand>
{
    public CompleteTraineeProfileCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class CompleteTraineeProfileCommandHandler : IRequestHandler<CompleteTraineeProfileCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userAdministrationService;
    private readonly IEmailSender _emailSender;

    public CompleteTraineeProfileCommandHandler(
        IApplicationDbContext dbContext,
        IUserAdministrationService userAdministrationService,
        IEmailSender emailSender)
    {
        _dbContext = dbContext;
        _userAdministrationService = userAdministrationService;
        _emailSender = emailSender;
    }

    public async Task Handle(CompleteTraineeProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _dbContext.Set<TraineeProfile>()
            .Include(entity => entity.Curriculum)
                .ThenInclude(entity => entity.SubSpeciality)
                    .ThenInclude(entity => entity.Speciality)
            .SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException("The trainee profile could not be found.");

        if (!request.Principal.CanAccessInstitution(profile.Curriculum.SubSpeciality.Speciality.InstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to complete this trainee profile.");
        }

        profile.Complete(request.CompletedOn);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Role transition: there is no Alumnus role, so the Trainee role is removed and the profile archived.
        await _userAdministrationService.RemoveRoleAsync(profile.UserId, WombatRoles.Trainee, cancellationToken);

        var user = await _userAdministrationService.GetByIdAsync(profile.UserId, cancellationToken);
        if (user is not null)
        {
            await _emailSender.SendAsync(
                GraduationEmail.Build(
                    user.Email,
                    $"{user.FirstName} {user.LastName}".Trim(),
                    profile.Curriculum.Name,
                    request.CompletedOn),
                cancellationToken);
        }
    }
}
