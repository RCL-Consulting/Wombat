using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Trainees;

/// <summary>
/// What the Admit form says about the programme start it holds (T373, flow 11, S18, M03, M06, M07; T306's admission half):
/// the training year it gives today, and the expected completion an empty field would be derived as. Asked again each time
/// the start or the curriculum changes, so the page calls neither <see cref="TraineeProfile.StageOn" /> nor the
/// curriculum's month count itself.
/// </summary>
/// <param name="UserId">The registrar being admitted: the completion is derived only from a curriculum they are offered (B12).</param>
/// <param name="CurriculumId">The curriculum the registrar is admitted to; null before one is chosen (M14), and nothing is derived.</param>
public sealed record GetAdmissionDatesQuery(string UserId, int? CurriculumId, DateOnly ProgrammeStart, ClaimsPrincipal Principal)
    : IRequest<AdmissionDatesDto>;

/// <param name="Today">Today on the South African calendar: what the training year is counted to.</param>
/// <param name="TrainingYear">The training year on <paramref name="Today" />; null when the programme has not started (M06).</param>
/// <param name="DerivedCompletion">The expected completion an empty field is derived as; null with no curriculum.</param>
/// <param name="DerivedHasPassed">The derived completion is before today, so an empty field is refused (F4).</param>
/// <param name="StartsMoreThanFourYearsAgo">The start is more than four years before today (M07): a warning, never a refusal.</param>
public sealed record AdmissionDatesDto(
    DateOnly Today,
    int? TrainingYear,
    DateOnly? DerivedCompletion,
    bool DerivedHasPassed,
    bool StartsMoreThanFourYearsAgo);

public sealed class GetAdmissionDatesQueryHandler : IRequestHandler<GetAdmissionDatesQuery, AdmissionDatesDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;
    private readonly TimeProvider _clock;

    public GetAdmissionDatesQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users, TimeProvider? clock = null)
    {
        _dbContext = dbContext;
        _users = users;
        _clock = clock ?? TimeProvider.System;
    }

    public async Task<AdmissionDatesDto> Handle(GetAdmissionDatesQuery request, CancellationToken cancellationToken)
    {
        TraineeAdministrationRules.DemandTraineeAdministration(request.Principal);

        var today = QuotaCalendar.Today(_clock);
        int? months = null;
        if (request.CurriculumId is { } curriculumId
            && await OfferedAsync(request.UserId, curriculumId, request.Principal, cancellationToken) is { } offered)
        {
            var curriculum = await offered
                .AsNoTracking()
                .Where(entity => entity.Id == curriculumId)
                .Select(entity => new { Windows = entity.Items.Select(item => item.WindowMonths).ToList() })
                .SingleOrDefaultAsync(cancellationToken);
            months = curriculum is null ? null : AdmissionDates.CompletionMonths(curriculum.Windows);
        }

        return AdmissionDates.For(request.ProgrammeStart, months, today);
    }

    /// <summary>
    /// The curricula the registrar is offered (<see cref="AdmissionCurricula.Offered" />, F1), as the command judges them;
    /// null for a registrar not waiting in the caller's scope (T373 fix pass, B12). Until the fix any curriculum id was
    /// answered, one the registrar is not offered included.
    /// </summary>
    private async Task<IQueryable<Curriculum>?> OfferedAsync(
        string userId,
        int curriculumId,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        var user = string.IsNullOrWhiteSpace(userId) ? null : await _users.GetByIdAsync(userId, cancellationToken);
        return AdmissionScope.IsWaitingInScope(user, principal) && user!.InstitutionId is { } institutionId
            ? AdmissionCurricula.Offered(_dbContext, institutionId, user)
            : null;
    }
}

/// <summary>The admission's date rules, which the form's read and the command share (T373, flow 11).</summary>
internal static class AdmissionDates
{
    /// <summary>
    /// The derived completion's months: the curriculum's longest item window, at least one, twelve with no items
    /// (<see cref="AdmitTraineeCommandHandler.GetDefaultCompletionMonths" />'s rule, read here from the windows alone).
    /// </summary>
    public static int CompletionMonths(IEnumerable<int> windowMonths)
        => Math.Max(1, windowMonths.DefaultIfEmpty(12).Max());

    public static DateOnly DerivedCompletion(DateOnly programmeStart, int months) => programmeStart.AddMonths(months);

    /// <summary>F4: a derived completion before today has passed.</summary>
    public static bool HasPassed(DateOnly completion, DateOnly today) => completion < today;

    public static AdmissionDatesDto For(DateOnly programmeStart, int? months, DateOnly today)
    {
        DateOnly? derived = months is { } value ? DerivedCompletion(programmeStart, value) : null;
        return new AdmissionDatesDto(
            today,
            TraineeProfile.StageOn(programmeStart, today),
            derived,
            derived is { } date && HasPassed(date, today),
            programmeStart < today.AddYears(-4));
    }
}
