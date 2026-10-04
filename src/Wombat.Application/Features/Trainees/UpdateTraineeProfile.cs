using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Commands.RebuildCurriculumProgress;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Trainees;

/// <summary>
/// Saves a running trainee profile's curriculum, programme start and expected completion.
/// </summary>
/// <remarks>
/// <para>
/// <b>Not an admission (T304).</b> An unchanged curriculum keeps the adoption the profile is pinned to, superseded or not:
/// re-adopting deactivates the old adoption, but its trainees stay on the old version (D24). Only a move is judged, and
/// only into the institution's active adoption for the trainee's discipline, which is what the page's picker offers
/// (<see cref="GetTraineeCurriculumChoicesQuery" />).
/// </para>
/// <para>
/// <b>A move, or a change of programme start, replays the trainee's credit (T304)</b>, in the same save: tallies are rows
/// per curriculum item, so a moved trainee's would otherwise stay on the old version's items while their progress reads the
/// new one's, and a completion's minimum is judged at its training year, which the start decides. The replay is the
/// rebuild's for one trainee (<see cref="CurriculumProgressReplay" />), handed the curriculum and start being saved
/// (<see cref="PendingProgrammeMove" />), as an end's is handed the end (T281). The save holds the profile against every
/// completion of the trainee's from before it reads it until its save commits (<see cref="ITraineeCreditLock.HoldForMoveAsync" />).
/// </para>
/// <para>
/// <b>An ended profile is archived (T305).</b> Mark complete says so: a completed or withdrawn trainee's profile is refused
/// before anything is written, and the page shows it read-only.
/// </para>
/// </remarks>
public sealed record UpdateTraineeProfileCommand(
    int Id,
    int CurriculumId,
    DateOnly ProgrammeStartDate,
    DateOnly? ExpectedCompletionDate,
    ClaimsPrincipal Principal) : IRequest<UpdateTraineeProfileResult>;

/// <summary>What a profile save did: the profile as saved, and the recount its replay made, if it replayed (T304).</summary>
public sealed record UpdateTraineeProfileResult(TraineeProfileDto Profile, TraineeCreditRecount? Recount)
{
    /// <summary>The sentence the page shows: the save, and what the replay counted again, against what.</summary>
    public string Message => Recount switch
    {
        null => "Trainee profile saved.",
        { CompletionsCounted: 0, CurriculumMoved: true } =>
            $"Trainee profile saved. They have no completions to count against {Profile.CurriculumVersion}.",
        { CompletionsCounted: 0 } => "Trainee profile saved. They have no completions to count again.",
        { CurriculumMoved: true } =>
            $"Trainee profile saved. {Completions(Recount.CompletionsCounted)} counted again against {Profile.CurriculumVersion}.",
        _ => $"Trainee profile saved. {Completions(Recount.CompletionsCounted)} counted again from the new programme start date."
    };

    private static string Completions(int count) => count == 1 ? "1 completion was" : $"{count} completions were";
}

/// <summary>
/// The replay a profile save ran (T304): whether it moved the trainee to another curriculum version (else it changed their
/// programme start), and how many of their completions credit was judged for again
/// (<see cref="RebuildCurriculumProgressResult.ActivitiesReplayed" />).
/// </summary>
public sealed record TraineeCreditRecount(bool CurriculumMoved, int CompletionsCounted);

public sealed class UpdateTraineeProfileCommandValidator : AbstractValidator<UpdateTraineeProfileCommand>
{
    public UpdateTraineeProfileCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
        RuleFor(command => command.CurriculumId).GreaterThan(0);
    }
}

public sealed class UpdateTraineeProfileCommandHandler : IRequestHandler<UpdateTraineeProfileCommand, UpdateTraineeProfileResult>
{
    /// <summary>The refusal of a move into anything but the institution's active adoption for the discipline (T304).</summary>
    public const string MoveNotAdopted =
        "A trainee can be moved only into the curriculum version this institution has adopted for their discipline.";

    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userAdministrationService;
    private readonly ICreditApplier _creditApplier;
    private readonly ITraineeCreditLock _traineeCreditLock;

    public UpdateTraineeProfileCommandHandler(
        IApplicationDbContext dbContext,
        IUserAdministrationService userAdministrationService,
        ICreditApplier creditApplier,
        ITraineeCreditLock traineeCreditLock)
    {
        _dbContext = dbContext;
        _userAdministrationService = userAdministrationService;
        _creditApplier = creditApplier;
        _traineeCreditLock = traineeCreditLock;
    }

    /// <summary>The refusal of a save of an ended profile (T305).</summary>
    public static string Archived(DateOnly endedOn)
        => $"This programme ended on {endedOn:yyyy-MM-dd}; its record is archived and cannot be changed.";

    public async Task<UpdateTraineeProfileResult> Handle(UpdateTraineeProfileCommand request, CancellationToken cancellationToken)
    {
        // T304. Held before the profile is read, and until the save below commits, as an end holds it: no completion of the
        // trainee's credits against the curriculum or start this save replaces. Every save takes it, because whether this
        // one moves anything is read after the profile.
        await using var hold = await _traineeCreditLock.HoldForMoveAsync(request.Id, cancellationToken);

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

        // T305: Mark complete archives the profile, and a withdrawal ends it as surely. Refused before anything changes (the
        // audit trap). This also retires T209's start-after-end check: only an ended profile records an end.
        if (profile.IsEnded)
        {
            throw new InvalidOperationException(Archived(profile.EndedOn!.Value));
        }

        var curriculum = await _dbContext.Set<Curriculum>()
            .Include(entity => entity.SubSpeciality)
                .ThenInclude(entity => entity.Speciality)
            .Include(entity => entity.Items)
            .SingleOrDefaultAsync(entity => entity.Id == request.CurriculumId, cancellationToken)
            ?? throw new InvalidOperationException("The selected curriculum could not be found.");

        // T304: the curriculum is a national catalogue version (T091), and the profile stays pinned to the adoption it was
        // admitted or moved under. Only a move is judged, and only into the institution's active adoption for the trainee's
        // discipline. Read before anything changes.
        var moves = curriculum.Id != profile.CurriculumId;
        var adoptionId = profile.AdoptionId;
        if (moves)
        {
            adoptionId = await TraineeAdoptionResolver.ResolveMoveAdoptionIdAsync(
                    _dbContext, profile.InstitutionId, profile.Curriculum.SubSpecialityId, curriculum.Id, cancellationToken)
                ?? throw new InvalidOperationException(MoveNotAdopted);
        }

        var replays = moves || request.ProgrammeStartDate != profile.ProgrammeStartDate;

        profile.AdoptionId = adoptionId;
        profile.CurriculumId = curriculum.Id;
        profile.ProgrammeStartDate = request.ProgrammeStartDate;
        profile.ExpectedCompletionDate = request.ExpectedCompletionDate
            ?? request.ProgrammeStartDate.AddMonths(AdmitTraineeCommandHandler.GetDefaultCompletionMonths(curriculum));

        TraineeCreditRecount? recount = null;
        if (replays)
        {
            var replayed = await ReplayAsync(profile, cancellationToken);
            recount = new TraineeCreditRecount(moves, replayed.ActivitiesReplayed);
        }
        else
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // Committed before the scope is rewritten, which writes nothing the hold is for.
        await hold.CommitAsync(cancellationToken);

        await _userAdministrationService.UpdateScopeAsync(
            profile.UserId,
            profile.InstitutionId,
            [curriculum.SubSpeciality.SpecialityId],
            [curriculum.SubSpecialityId],
            cancellationToken);

        var user = await _userAdministrationService.GetByIdAsync(profile.UserId, cancellationToken)
            ?? throw new InvalidOperationException("The trainee user could not be found.");

        return new UpdateTraineeProfileResult(
            new TraineeProfileDto(
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
                profile.IsActive),
            recount);
    }

    /// <summary>
    /// Saves the change just made to <paramref name="profile" /> with a replay of the trainee's credit against it, in the
    /// replay's one save (T304), as <c>ProgrammeEndCredit</c> saves an end. When the replay throws, it has put back its own
    /// changes, and the profile is put back here, so the audit pipeline's save of this context commits neither.
    /// </summary>
    private async Task<RebuildCurriculumProgressResult> ReplayAsync(TraineeProfile profile, CancellationToken cancellationToken)
    {
        try
        {
            return await CurriculumProgressReplay.RunAsync(
                _dbContext,
                _creditApplier,
                _dbContext.Set<Activity>(),
                profile.UserId,
                pendingEnd: null,
                new PendingProgrammeMove(profile.Id, profile.CurriculumId, profile.ProgrammeStartDate),
                cancellationToken);
        }
        catch
        {
            var entry = _dbContext.Set<TraineeProfile>().Entry(profile);
            entry.CurrentValues.SetValues(entry.OriginalValues);
            entry.State = EntityState.Unchanged;
            throw;
        }
    }
}
