using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.Users;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Trainees;

/// <summary>
/// One trainee profile, as its page reads it (T379, flow 13, S4): "‹name›'s programme", its details, and either the end
/// section of a running programme (the last ratified decision) or the ended record (how and when it ended, and the account
/// it belongs to now). Null for an unknown id and for another institution's profile alike, so the page says "Profile
/// unavailable" for both and never confirms that an id exists elsewhere (until T379 it threw, and the page printed the
/// exception's text, T329).
/// </summary>
public sealed record GetTraineeProfileByIdQuery(int Id, ClaimsPrincipal Principal) : IRequest<TraineeProfileDto?>;

public sealed class GetTraineeProfileByIdQueryHandler : IRequestHandler<GetTraineeProfileByIdQuery, TraineeProfileDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userAdministrationService;
    private readonly TimeProvider _clock;

    /// <param name="clock">"Today" on the South African calendar, which the record's STARs are read as of; the system clock unless a test pins it.</param>
    public GetTraineeProfileByIdQueryHandler(
        IApplicationDbContext dbContext,
        IUserAdministrationService userAdministrationService,
        TimeProvider? clock = null)
    {
        _dbContext = dbContext;
        _userAdministrationService = userAdministrationService;
        _clock = clock ?? TimeProvider.System;
    }

    public async Task<TraineeProfileDto?> Handle(GetTraineeProfileByIdQuery request, CancellationToken cancellationToken)
    {
        // Asked before anything is read (T278, T290): the page shows its note instead.
        TraineeAdministrationRules.DemandTraineeAdministration(request.Principal);

        var profile = await _dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .Include(entity => entity.Curriculum)
                .ThenInclude(entity => entity.SubSpeciality)
                    .ThenInclude(entity => entity.Speciality)
            .SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);

        if (profile is null || !request.Principal.CanAccessInstitution(profile.InstitutionId))
        {
            return null;
        }

        // An erased person's profile belongs to a pseudonym no account holds (T258): the record is read under it, and says
        // nothing about an account (F7, S5).
        var user = await _userAdministrationService.GetByIdAsync(profile.UserId, cancellationToken);
        var erased = user is null;

        var ended = EndedProgrammes.Of(profile);
        var dto = new TraineeProfileDto(
            profile.Id,
            profile.UserId,
            user?.Email ?? string.Empty,
            user?.FirstName ?? profile.UserId,
            user?.LastName ?? string.Empty,
            profile.CurriculumId,
            profile.Curriculum.Name,
            profile.Curriculum.Version,
            profile.Curriculum.SubSpeciality.SpecialityId,
            profile.Curriculum.SubSpeciality.Speciality.Name,
            profile.Curriculum.SubSpecialityId,
            profile.Curriculum.SubSpeciality.Name,
            profile.ProgrammeStartDate,
            profile.ExpectedCompletionDate,
            profile.IsActive,
            profile.CompletedOn,
            profile.DeactivatedOn)
        {
            // F1: the export command's own check, so the header offers no export the command would refuse.
            MayExport = await TraineeScopeResolver.MayReadAsync(_dbContext, request.Principal, profile.UserId, cancellationToken),
            Ended = ended,
            IsErased = erased,
        };

        if (profile.IsActive)
        {
            // Q13: named by End this programme, gating nothing.
            return dto with
            {
                LastRatifiedDecision = await LastRatifiedDecision.ReadAsync(_dbContext, profile.UserId, cancellationToken)
            };
        }

        if (user is null)
        {
            return dto;
        }

        // F5, S5: the account sentence names the STAR certificates only when a STAR was ever issued; it is read only for an
        // account that holds no role and is not locked, the one sentence that has the clause.
        var locked = user.IsDeactivated;
        var anyStar = user.Roles.Count == 0 && !locked &&
                      (await RecordStars.ReadAsync(
                          _dbContext, request.Principal, profile.UserId, QuotaCalendar.Today(_clock), cancellationToken)).AnyIssued;

        return dto with { Account = new TraineeAccountDto(user.Roles.ToList(), locked, anyStar) };
    }
}
