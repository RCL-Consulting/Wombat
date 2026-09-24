using System.Data.Common;
using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// Puts a trainee before a panel for one academic period (<paramref name="AcademicYear" />, <paramref name="Semester" />),
/// judging the evidence between <paramref name="ReviewPeriodFrom" /> and <paramref name="ReviewPeriodTo" />.
/// </summary>
/// <remarks>
/// <para>
/// The period is chosen, never derived from the window (T131, Decision 4): the page fills the window from the period and
/// leaves it editable. A binding review is given its agenda here (<see cref="AgendaPlanner" />), and Start adds whatever
/// has come due since.
/// </para>
/// <para>
/// <b>One open binding review per trainee, period and seat</b> (T131 slice 4 review). A seat is what a panel sits as at
/// its institution: the College committee it carries (<see cref="DecisionPanel.DecisionBodyKey" />), or none, which is
/// the trainee's general committee. Routing gives an EPA with no body to every eligible general panel, so an
/// institution-wide panel and a speciality panel would otherwise each hold the same closing lines for the same period,
/// and each ratify a STAR on them, the second superseding the first. A neonatal CCC and a general panel are different
/// seats, and a panel at another institution is never in the way: a trainee who moved has stranded that review (T182).
/// The check runs before the first mutation and names the review in the way. The database's partial unique index holds
/// the same panel against a race; two different panels of one seat scheduled at the same instant are not held by it.
/// </para>
/// </remarks>
public sealed record ScheduleCommitteeReviewCommand(
    string TraineeUserId,
    int PanelId,
    int AcademicYear,
    int Semester,
    DateOnly ReviewPeriodFrom,
    DateOnly ReviewPeriodTo,
    DateOnly ScheduledOn,
    ClaimsPrincipal Principal,
    bool IsFormative = false,
    CommitteeReviewType ReviewType = CommitteeReviewType.AnnualProgression) : IRequest<CommitteeReviewListItemDto>;

public sealed class ScheduleCommitteeReviewCommandValidator : AbstractValidator<ScheduleCommitteeReviewCommand>
{
    public ScheduleCommitteeReviewCommandValidator()
    {
        RuleFor(command => command.TraineeUserId).NotEmpty();
        RuleFor(command => command.PanelId).GreaterThan(0);
        RuleFor(command => command.AcademicYear).InclusiveBetween(ReviewPeriodRules.FirstYear, ReviewPeriodRules.LastYear);
        RuleFor(command => command.Semester).InclusiveBetween(1, 2).WithMessage(ReviewPeriodRules.SemesterMessage);
        RuleFor(command => command.Principal).NotNull();
        RuleFor(command => command.ReviewPeriodTo).GreaterThanOrEqualTo(command => command.ReviewPeriodFrom);
    }
}

public sealed class ScheduleCommitteeReviewCommandHandler : IRequestHandler<ScheduleCommitteeReviewCommand, CommitteeReviewListItemDto>
{
    /// <summary>PostgreSQL's unique_violation, which the one-open-binding-review index raises.</summary>
    private const string UniqueViolation = "23505";

    private readonly IApplicationDbContext _dbContext;

    public ScheduleCommitteeReviewCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CommitteeReviewListItemDto> Handle(ScheduleCommitteeReviewCommand request, CancellationToken cancellationToken)
    {
        CommitteeDecisionAuthorization.DemandReviewScheduling(request.Principal);

        var traineeUserId = request.TraineeUserId.Trim();

        var panel = await _dbContext.Set<DecisionPanel>()
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == request.PanelId, cancellationToken);

        if (panel is null)
        {
            // Anyone but an Administrator is refused exactly as for another institution's panel, so that the refusal
            // does not say which panel ids exist. (T182)
            throw request.Principal.IsAdministrator()
                ? new InvalidOperationException("The decision panel could not be found.")
                : new UnauthorizedAccessException(CommitteeTraineeScope.NotSchedulable);
        }

        // The trainee must train at the panel's institution, and anyone but an Administrator must oversee them; before
        // T182 only an InstitutionalAdmin's panel was checked, and the trainee not at all. Nothing is written until this
        // passes.
        await CommitteeTraineeScope.DemandSchedulableAsync(
            _dbContext, request.Principal, panel, traineeUserId, cancellationToken);

        var period = new AcademicPeriod(request.AcademicYear, request.Semester);

        // One open binding review per trainee, period and seat (see the remarks): named, so the scheduler can open it
        // instead. The index holds the same panel against a race.
        IReadOnlyList<CommitteeAgendaLine> agenda = [];
        if (!request.IsFormative)
        {
            var open = await OpenBindingReviewAsync(traineeUserId, panel, period, cancellationToken);
            if (open is not null)
            {
                throw new InvalidOperationException(AlreadyScheduled(open, period));
            }

            agenda = (await AgendaPlanner.PlanAsync(
                    _dbContext, traineeUserId, panel, period, ProgrammeCalendar.DateOf(DateTime.UtcNow), cancellationToken))
                .Lines;
        }

        // The first mutation.
        var review = new CommitteeReview
        {
            TraineeUserId = traineeUserId,
            PanelId = request.PanelId,
            AcademicYear = period.Year,
            Semester = period.Semester,
            ReviewPeriodFrom = request.ReviewPeriodFrom,
            ReviewPeriodTo = request.ReviewPeriodTo,
            ScheduledOn = request.ScheduledOn,
            IsFormative = request.IsFormative,
            ReviewType = request.ReviewType
        };
        review.AddCadenceLines(agenda);

        _dbContext.Set<CommitteeReview>().Add(review);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is DbException { SqlState: UniqueViolation })
        {
            // The race: another binding review of this trainee before this panel for this period was scheduled after the
            // check above read the table. Named, with the database's refusal kept underneath so the audit pipeline writes
            // its row without re-sending the refused insert (T201).
            var open = await OpenBindingReviewAsync(traineeUserId, panel, period, cancellationToken);
            if (open is null)
            {
                throw;
            }

            throw new InvalidOperationException(AlreadyScheduled(open, period), exception);
        }

        return new CommitteeReviewListItemDto(
            review.Id,
            review.TraineeUserId,
            review.PanelId,
            panel.Name,
            review.ReviewPeriodFrom,
            review.ReviewPeriodTo,
            review.ScheduledOn,
            review.State,
            null,
            null,
            review.IsFormative,
            review.ReviewType)
        {
            AcademicYear = review.AcademicYear,
            Semester = review.Semester
        };
    }

    /// <summary>
    /// The open binding review of this trainee for this period before a panel in <paramref name="panel" />'s seat, if any:
    /// a panel at the same institution sitting as the same College committee, or as none. Includes the panel itself, which
    /// is what the index allows one of.
    /// </summary>
    private async Task<OpenReview?> OpenBindingReviewAsync(
        string traineeUserId, DecisionPanel panel, AcademicPeriod period, CancellationToken cancellationToken)
    {
        var institutionId = panel.InstitutionId;
        var bodyKey = DecisionBody.NormalizeKey(panel.DecisionBodyKey);

        return await _dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .Where(entity => entity.TraineeUserId == traineeUserId &&
                             entity.Panel.InstitutionId == institutionId &&
                             entity.Panel.DecisionBodyKey == bodyKey &&
                             entity.AcademicYear == period.Year &&
                             entity.Semester == period.Semester &&
                             !entity.IsFormative &&
                             (entity.State == CommitteeReviewState.Scheduled ||
                              entity.State == CommitteeReviewState.InProgress ||
                              entity.State == CommitteeReviewState.Decided))
            .OrderBy(entity => entity.Id)
            .Select(entity => new OpenReview(entity.Id, entity.Panel.Name, entity.State, entity.ScheduledOn))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>The refusal for a second open binding review of one trainee, in one seat, for one period.</summary>
    internal static string AlreadyScheduled(OpenReview open, AcademicPeriod period)
        => $"Review #{open.Id} already puts this trainee before {open.PanelName} for {period} " +
           $"({StateLabel(open.State)}, scheduled {open.ScheduledOn:yyyy-MM-dd}). A trainee has one binding review for " +
           "each period before the panels that decide the same EPAs, until it is ratified: open that review, or ratify " +
           "it before scheduling another.";

    private static string StateLabel(CommitteeReviewState state) => state switch
    {
        CommitteeReviewState.Scheduled => "scheduled",
        CommitteeReviewState.InProgress => "in progress",
        CommitteeReviewState.Decided => "decided, not yet ratified",
        _ => state.ToString()
    };

    internal sealed record OpenReview(int Id, string PanelName, CommitteeReviewState State, DateOnly ScheduledOn);
}
