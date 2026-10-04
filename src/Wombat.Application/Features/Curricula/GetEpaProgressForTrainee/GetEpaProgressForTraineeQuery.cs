using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Curricula.GetEpaProgressForTrainee;

/// <summary>
/// One EPA of the caller's own curriculum, for its page under My progress (T355, round 1 correction 2): the item's windows,
/// the training year and its level, the cadence and the exit level.
/// </summary>
/// <remarks>
/// <para>
/// <b>By the EPA's id, never its code</b>: a code is unique only within its namespace (national, or one institution's
/// local extras). The caller's own record only: the profile is their preferred one
/// (<see cref="TraineeScopeResolver.PreferredProfiles" />), the one My progress, credit and the export read. There is no
/// trainee id to pass, so no other trainee can be named.
/// </para>
/// <para>
/// The item is read by <see cref="TraineeQuotaProgressReader.ReadItemAsync" />, the code My progress's rows are read by,
/// so the page and the row cannot disagree. It is read whether or not its EPA is in force: a paused EPA's page says why it
/// is no target (D48). A programme that has ended is read as My progress reads it (T252, R5): as on its last day, with
/// every period back to its start in <see cref="TraineeCurriculumProgressDto.Periods" />, read-only.
/// </para>
/// <para>
/// Null when the caller has no trainee profile, when the EPA is no item of their profile's curriculum (another
/// institution's local item included), or when the id names no EPA: the page says "Page not found", so an id never
/// confirms anything.
/// </para>
/// </remarks>
/// <param name="AsOf">The day to read for; defaults to today on the South African calendar (T325). Tests pin it.</param>
public sealed record GetEpaProgressForTraineeQuery(ClaimsPrincipal Principal, int EpaId, DateOnly? AsOf = null)
    : IRequest<EpaProgressDto?>;

/// <summary>One EPA of a trainee's curriculum, as its page reads it (T355).</summary>
/// <param name="Item">
/// The item for the EPA, read by the same code as My progress's row; <see cref="TraineeCurriculumProgressDto.EpaInForce" />
/// false when the EPA is paused.
/// </param>
/// <param name="AsOf">The day the figures are for: today, or an ended programme's last day.</param>
/// <param name="TraineeStage">The training year on <paramref name="AsOf" />; null before the programme starts.</param>
/// <param name="IsAfterTeachingYear">
/// <paramref name="AsOf" /> is in December, after the College's January to November year (D40).
/// </param>
/// <param name="Ended">How the programme ended, when it has (T252); null while it runs.</param>
public sealed record EpaProgressDto(
    TraineeCurriculumProgressDto Item,
    DateOnly AsOf,
    DateOnly ProgrammeStartDate,
    int? TraineeStage,
    bool IsAfterTeachingYear,
    ProgrammeEndDto? Ended);

public sealed class GetEpaProgressForTraineeQueryValidator : AbstractValidator<GetEpaProgressForTraineeQuery>
{
    public GetEpaProgressForTraineeQueryValidator()
    {
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class GetEpaProgressForTraineeQueryHandler : IRequestHandler<GetEpaProgressForTraineeQuery, EpaProgressDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly TimeProvider _clock;

    public GetEpaProgressForTraineeQueryHandler(IApplicationDbContext dbContext, TimeProvider clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<EpaProgressDto?> Handle(GetEpaProgressForTraineeQuery request, CancellationToken cancellationToken)
    {
        var userId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        var profile = await TraineeScopeResolver.PreferredProfiles(_dbContext)
            .AsNoTracking()
            .Where(entity => entity.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return null;
        }

        var today = request.AsOf ?? QuotaCalendar.Today(_clock);

        // As TraineeQuotaProgressReader.ReadAsync reads My progress (T252): a running programme on today, with its current
        // and previous windows; an ended one as on its last day, with every period back to its start (R5).
        ProgrammeEndDto? ended = null;
        var asOf = today;
        DateOnly? periodsFrom = null;
        if (!profile.IsActive)
        {
            asOf = profile.EndedOn is { } endedOn && endedOn < today ? endedOn : today;
            periodsFrom = DateOnly.MinValue;
            ended = new ProgrammeEndDto(profile.CompletedOn is not null, profile.EndedOn, today);
        }

        var item = await TraineeQuotaProgressReader.ReadItemAsync(
            _dbContext, profile, request.EpaId, asOf, periodsFrom, cancellationToken);

        if (item is null)
        {
            return null;
        }

        return new EpaProgressDto(
            item,
            asOf,
            profile.ProgrammeStartDate,
            profile.GetStage(asOf),
            asOf > AcademicPeriod.Containing(asOf).NominalEnd,
            ended);
    }
}
