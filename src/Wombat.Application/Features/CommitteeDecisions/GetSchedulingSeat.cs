using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Common.Users;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// The open binding review that already holds this trainee's seat for the period, before a panel in this panel's seat: what
/// the schedule page says as "Already held" before Create (T364, Q3; T309 item 1).
/// </summary>
/// <param name="ReviewId">The review in the way, which the page offers to open instead ("Open review #1").</param>
/// <param name="PanelName">The panel it sits before, which may be another panel of the same seat.</param>
/// <param name="PeriodLabel">"2026 S2".</param>
/// <param name="ScheduledOn">The day it was scheduled on.</param>
/// <param name="TraineeName">The trainee by name, as the sentence names them.</param>
/// <param name="Sentence">
/// The scheduling handler's own refusal for a second review, word for word
/// (<see cref="ScheduleCommitteeReviewCommandHandler.AlreadyScheduled" />), so the warning and the refusal never differ.
/// </param>
public sealed record SchedulingSeatDto(
    int ReviewId,
    string PanelName,
    string PeriodLabel,
    DateOnly ScheduledOn,
    string TraineeName,
    string Sentence);

/// <summary>
/// Whether a binding review of this trainee before this panel, for this period, would be refused because the seat is held,
/// read by the create's own check (<see cref="CommitteeReviewSeats.HolderAsync" />), never a second copy (T364, Q3).
/// </summary>
/// <remarks>
/// <para>
/// A query of its own, independent of the agenda preview: the preview is advice that may fail (C6), while the seat decides
/// whether the page offers Create at all.
/// </para>
/// <para>
/// Null when nothing holds the seat, and equally for anything the caller may not schedule: a caller who schedules nobody
/// (someone who holds Trainee among them, T216), a panel that is not there, a trainee who is not current or not theirs.
/// Out of scope reads as nothing, so the answer confirms no review, panel or trainee the caller cannot reach (CLAUDE.md,
/// T056's rule); the create handler still refuses such a caller in its own words. A formative check-in holds no seat.
/// </para>
/// </remarks>
public sealed record GetSchedulingSeatQuery(
    string TraineeUserId,
    int PanelId,
    int AcademicYear,
    int Semester,
    ClaimsPrincipal Principal) : IRequest<SchedulingSeatDto?>;

public sealed class GetSchedulingSeatQueryValidator : AbstractValidator<GetSchedulingSeatQuery>
{
    public GetSchedulingSeatQueryValidator()
    {
        // No rule on either id (T364, build review R2): a panel or trainee that is not there is answered with null, as the
        // remarks promise, never refused. The period is the validator's, since no seat can be read without one.
        RuleFor(query => query.AcademicYear).InclusiveBetween(ReviewPeriodRules.FirstYear, ReviewPeriodRules.LastYear);
        RuleFor(query => query.Semester).InclusiveBetween(1, 2).WithMessage(ReviewPeriodRules.SemesterMessage);
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class GetSchedulingSeatQueryHandler : IRequestHandler<GetSchedulingSeatQuery, SchedulingSeatDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public GetSchedulingSeatQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<SchedulingSeatDto?> Handle(GetSchedulingSeatQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Principal);

        // Who schedules at all first, as the create asks it, before either id is looked up (T216).
        if (!CommitteeDecisionAuthorization.MayScheduleReviews(request.Principal))
        {
            return null;
        }

        var traineeUserId = (request.TraineeUserId ?? string.Empty).Trim();
        if (traineeUserId.Length == 0)
        {
            return null;
        }

        var panel = await _dbContext.Set<DecisionPanel>()
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == request.PanelId, cancellationToken);
        if (panel is null)
        {
            return null;
        }

        // The create's own predicate over the create's own resolver (CommitteeTraineeScope.DemandSchedulableAsync), asked
        // without its refusal: the answer for a trainee the caller may not schedule is nothing.
        var trainee = await TraineeScopeResolver.ResolveCurrentAsync(_dbContext, _users, traineeUserId, cancellationToken);
        if (!CommitteeTraineeScope.MayScheduleFor(request.Principal, panel, trainee))
        {
            return null;
        }

        var period = new AcademicPeriod(request.AcademicYear, request.Semester);
        var holder = await CommitteeReviewSeats.HolderAsync(_dbContext, traineeUserId, panel, period, cancellationToken);
        if (holder is null)
        {
            return null;
        }

        var traineeName = (await UserDisplayNames.ResolveAsync(_users, [traineeUserId], cancellationToken)).NameOf(traineeUserId);
        return new SchedulingSeatDto(
            holder.Id,
            holder.PanelName,
            period.ToString(),
            holder.ScheduledOn,
            traineeName,
            ScheduleCommitteeReviewCommandHandler.AlreadyScheduled(holder, traineeName, period));
    }
}
