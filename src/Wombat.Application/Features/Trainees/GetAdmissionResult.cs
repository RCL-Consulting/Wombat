using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Trainees;

/// <summary>
/// What Trainees says after an admission (T373, flow 11, S8; T02–T04): Admit navigates to
/// <c>/admin/trainees?admitted=‹profileId›</c>, and the page reads that profile again here, in the caller's scope, to write
/// the sentence. The address carries the id alone, never a name, a curriculum or a date. Null for an id not the caller's, an
/// unknown one, or a profile that has ended: the page then says nothing.
/// </summary>
public sealed record GetAdmissionResultQuery(int TraineeProfileId, ClaimsPrincipal Principal) : IRequest<AdmissionResultDto?>;

/// <param name="TrainingYear">The training year today on the South African calendar; null when the programme has not started.</param>
/// <param name="CompletionWasDerived">
/// The expected completion is the one the curriculum window derives from the start: T04's " Expected completion …, derived
/// from the curriculum window: change it under Edit." Nothing stores whether the field was left empty (no migration, T373),
/// so a date typed equal to the derived one reads as derived too, which it is in value.
/// </param>
public sealed record AdmissionResultDto(
    int TraineeProfileId,
    string UserId,
    string FullName,
    string CurriculumName,
    string CurriculumVersion,
    int? TrainingYear,
    DateOnly ProgrammeStart,
    DateOnly ExpectedCompletion,
    bool CompletionWasDerived);

public sealed class GetAdmissionResultQueryHandler : IRequestHandler<GetAdmissionResultQuery, AdmissionResultDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;
    private readonly TimeProvider _clock;

    public GetAdmissionResultQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users, TimeProvider? clock = null)
    {
        _dbContext = dbContext;
        _users = users;
        _clock = clock ?? TimeProvider.System;
    }

    public async Task<AdmissionResultDto?> Handle(GetAdmissionResultQuery request, CancellationToken cancellationToken)
    {
        TraineeAdministrationRules.DemandTraineeAdministration(request.Principal);

        var profile = await _dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .Where(entity => entity.Id == request.TraineeProfileId && entity.IsActive)
            .Select(entity => new
            {
                entity.Id,
                entity.UserId,
                entity.InstitutionId,
                entity.ProgrammeStartDate,
                entity.ExpectedCompletionDate,
                entity.Curriculum.Name,
                entity.Curriculum.Version,
                Windows = entity.Curriculum.Items.Select(item => item.WindowMonths).ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        // Out of scope reads as not found (T056): nothing is said.
        if (profile is null || !request.Principal.CanAccessInstitution(profile.InstitutionId))
        {
            return null;
        }

        var names = await _users.GetDisplayNamesAsync([profile.UserId], cancellationToken);
        if (!names.TryGetValue(profile.UserId, out var name) || string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var derived = AdmissionDates.DerivedCompletion(profile.ProgrammeStartDate, AdmissionDates.CompletionMonths(profile.Windows));
        return new AdmissionResultDto(
            profile.Id,
            profile.UserId,
            name,
            profile.Name,
            profile.Version,
            TraineeProfile.StageOn(profile.ProgrammeStartDate, QuotaCalendar.Today(_clock)),
            profile.ProgrammeStartDate,
            profile.ExpectedCompletionDate,
            profile.ExpectedCompletionDate == derived);
    }
}
