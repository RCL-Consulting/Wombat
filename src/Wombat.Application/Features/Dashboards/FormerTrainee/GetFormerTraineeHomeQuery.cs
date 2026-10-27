using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.Users;

namespace Wombat.Application.Features.Dashboards.FormerTrainee;

/// <summary>
/// The caller's own training record, as Home's no-role branch shows it (T379, flow 13, S8 and S9; R3-H01, R3-H02) and My
/// progress's "Your record:" line reads it: how and when the programme ended, the training year it ended in, and the
/// record's STARs. Null when the caller has no ended programme to show.
/// </summary>
/// <remarks>
/// <para>
/// The programme is the caller's preferred profile (<see cref="TraineeScopeResolver.PreferredProfiles" />), the one every
/// record reader picks: the active one, else the latest. <b>Null for a current Trainee</b>: the <c>trainee_record</c>
/// claim is issued for any profile, a running one included (S8), so holding it says nothing about an end. Null too for a
/// caller with no profile, and for a profile that ended before Wombat recorded the day (<see cref="EndedProgrammes.Of" />).
/// </para>
/// <para>
/// Answered whatever role the caller holds now: a graduate acting as Assessor has the same record (H03), though Home
/// shows them the Assessor's dashboard and reads none of it (Q25). Only the caller's own record is ever read: the user id is
/// the principal's, never a parameter.
/// </para>
/// </remarks>
public sealed record GetFormerTraineeHomeQuery(ClaimsPrincipal Principal) : IRequest<FormerTraineeHomeDto?>;

public sealed class GetFormerTraineeHomeQueryHandler : IRequestHandler<GetFormerTraineeHomeQuery, FormerTraineeHomeDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly TimeProvider _clock;

    /// <param name="clock">"Today" on the South African calendar, which the STARs are read as of (T325).</param>
    public GetFormerTraineeHomeQueryHandler(IApplicationDbContext dbContext, TimeProvider? clock = null)
    {
        _dbContext = dbContext;
        _clock = clock ?? TimeProvider.System;
    }

    public async Task<FormerTraineeHomeDto?> Handle(GetFormerTraineeHomeQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Principal);

        var userId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        var profile = await TraineeScopeResolver.PreferredProfiles(_dbContext)
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.UserId == userId, cancellationToken);
        if (profile is null || EndedProgrammes.Of(profile) is not { } ended)
        {
            return null;
        }

        // F6: any ratified STAR, in force or not, decides whether My authorisations is offered; the count in force is the
        // standing's, as the programme left them (T312).
        var stars = await RecordStars.ReadAsync(
            _dbContext, request.Principal, userId, QuotaCalendar.Today(_clock), cancellationToken);

        return new FormerTraineeHomeDto(ended.EndedOn, ended.Completed, ended.TrainingYearAtEnd, stars);
    }
}
