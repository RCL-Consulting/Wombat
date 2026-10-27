using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Trainees;

/// <summary>
/// Marks a trainee's programme complete (graduation): records the completion date, deactivates the
/// profile, removes the Trainee role (there is no Alumnus role — the profile is archived), and emails
/// the graduate. (T080 / F-5-4) An encounter observed after the completion day credits nothing on the profile, so
/// recording the day takes back any credit such an encounter already earned, in the same save (T281,
/// <see cref="ProgrammeEndCredit" />), under a hold on the profile that keeps every completion of the trainee's out until
/// it commits (<see cref="ITraineeCreditLock" />). The same save keeps the list the programme ended with (T312,
/// <see cref="ProgrammeItems.ReadEndedWithAsync" />), which the graduate's record and portfolio read from then on. It
/// answers with how many activities' credit it took back (T379, flow 13, F2: <see cref="ProgrammeEndResult" />).
/// </summary>
public sealed record CompleteTraineeProfileCommand(int Id, DateOnly CompletedOn, ClaimsPrincipal Principal) : IRequest<ProgrammeEndResult>;

public sealed class CompleteTraineeProfileCommandValidator : AbstractValidator<CompleteTraineeProfileCommand>
{
    public CompleteTraineeProfileCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class CompleteTraineeProfileCommandHandler : IRequestHandler<CompleteTraineeProfileCommand, ProgrammeEndResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userAdministrationService;
    private readonly IEmailSender _emailSender;
    private readonly ICreditApplier _creditApplier;
    private readonly ITraineeCreditLock _traineeCreditLock;
    private readonly TimeProvider _timeProvider;
    private readonly string? _baseUrl;

    /// <param name="options">Wombat's options, whose base URL the graduation email's link to My progress is under (T379, Q12).</param>
    public CompleteTraineeProfileCommandHandler(
        IApplicationDbContext dbContext,
        IUserAdministrationService userAdministrationService,
        IEmailSender emailSender,
        ICreditApplier creditApplier,
        ITraineeCreditLock traineeCreditLock,
        TimeProvider? timeProvider = null,
        IOptions<WombatOptions>? options = null)
    {
        _dbContext = dbContext;
        _userAdministrationService = userAdministrationService;
        _emailSender = emailSender;
        _creditApplier = creditApplier;
        _traineeCreditLock = traineeCreditLock;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _baseUrl = options?.Value.BaseUrl;
    }

    /// <summary>My progress, where the graduate reads the record (T379, Q12): the web app's <c>PortfolioLinks.MyProgress</c>.</summary>
    private const string MyProgressPath = "/portfolio/progress";

    public async Task<ProgrammeEndResult> Handle(CompleteTraineeProfileCommand request, CancellationToken cancellationToken)
    {
        TraineeAdministrationRules.DemandTraineeAdministration(request.Principal);

        // T281. Held before the profile is read, and until the save below commits, as a withdrawal holds it
        // (DeactivateTraineeProfileCommandHandler): no completion of the trainee's credits in between.
        await using var hold = await _traineeCreditLock.HoldForEndAsync(request.Id, cancellationToken);

        var profile = await _dbContext.Set<TraineeProfile>()
            .Include(entity => entity.Curriculum)
                .ThenInclude(entity => entity.SubSpeciality)
                    .ThenInclude(entity => entity.Speciality)
            .SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException("The trainee profile could not be found.");

        if (!request.Principal.CanAccessInstitution(profile.InstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to complete this trainee profile.");
        }

        // T281: how many activities the completion day takes back credit from, given for encounters after it. Read before
        // the end is recorded, like every other read that can fail; the answer is what the command returns (T379, F2).
        var takenBack = await ProgrammeEndCredit.CountTakenBackAsync(
            _dbContext.Set<Activity>(), profile.UserId, request.CompletedOn, cancellationToken);

        // T312: the list the programme ends with, each item in force for the trainee on the last day with its target then,
        // which the record reads from now on whatever the catalogue does. Read here, before the end is recorded, like
        // every other read that can fail; kept with the end below, and so saved with it and put back with it.
        var endedWith = await ProgrammeItems.ReadEndedWithAsync(
            _dbContext, profile, request.CompletedOn, _timeProvider.GetUtcNow().UtcDateTime, cancellationToken);

        // Complete checks that the profile is active and that the day lies between the programme start and today (T209
        // review: D49 reads it) before it changes anything, so a refusal leaves nothing for the audit pipeline to commit.
        profile.Complete(request.CompletedOn, QuotaCalendar.Today(_timeProvider));
        profile.RecordEndedCurriculum(endedWith);

        // Saved with a replay of the trainee's credit when the end takes some back, and put back if that replay fails.
        await ProgrammeEndCredit.SaveAsync(
            _dbContext, _creditApplier, _dbContext.Set<Activity>(), profile, takenBack, cancellationToken);

        // Committed before the role change and the email, which write and send nothing the hold is for.
        await hold.CommitAsync(cancellationToken);

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
                    request.CompletedOn,
                    $"{(_baseUrl ?? string.Empty).TrimEnd('/')}{MyProgressPath}"),
                cancellationToken);
        }

        return new ProgrammeEndResult(takenBack);
    }
}
