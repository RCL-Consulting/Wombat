using System.Data.Common;
using System.Globalization;
using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Users;
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
/// <para>
/// <b>What the review decides</b> (T131 slice 5): <paramref name="ReviewType" /> must be one
/// <see cref="CommitteeReviewTypes.Allowed" /> gives for the panel and the semester. Before a panel sitting as a College
/// committee that is entrustment-only and nothing else; a general panel's semester-2 sitting decides progression. Null
/// asks for the panel's default (<see cref="CommitteeReviewTypes.DefaultFor" />): entrustment-only before a College
/// committee, annual progression before a general panel. The scheduling form offers exactly the allowed types. A binding
/// entrustment-only review is refused before a panel that decides no EPA on the trainee's curriculum, which is nothing it
/// could put on its agenda (<see cref="AgendaPlan.PanelDecidesAnything" />).
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
    CommitteeReviewType? ReviewType = null) : IRequest<CommitteeReviewListItemDto>;

public sealed class ScheduleCommitteeReviewCommandValidator : AbstractValidator<ScheduleCommitteeReviewCommand>
{
    public ScheduleCommitteeReviewCommandValidator()
    {
        RuleFor(command => command.TraineeUserId).NotEmpty();
        RuleFor(command => command.PanelId).GreaterThan(0);
        RuleFor(command => command.AcademicYear).InclusiveBetween(ReviewPeriodRules.FirstYear, ReviewPeriodRules.LastYear);
        RuleFor(command => command.Semester).InclusiveBetween(1, 2).WithMessage(ReviewPeriodRules.SemesterMessage);
        RuleFor(command => command.Principal).NotNull();
        // In the schedule page's words (T364, S4; T324), which say the same beside the field before anything is sent.
        RuleFor(command => command.ReviewPeriodTo)
            .GreaterThanOrEqualTo(command => command.ReviewPeriodFrom)
            .WithMessage(command => ScheduleCommitteeReviewCommandHandler.WindowEndsBeforeItStarts(command.ReviewPeriodFrom));
        RuleFor(command => command.ReviewType).IsInEnum().WithMessage(CommitteeReviewTypes.UnknownType);
    }
}

public sealed class ScheduleCommitteeReviewCommandHandler : IRequestHandler<ScheduleCommitteeReviewCommand, CommitteeReviewListItemDto>
{
    /// <summary>PostgreSQL's unique_violation, which the one-open-binding-review index raises.</summary>
    private const string UniqueViolation = "23505";

    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public ScheduleCommitteeReviewCommandHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
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
                ? new CommitteeRefusedException("The decision panel could not be found.")
                : new UnauthorizedAccessException(CommitteeTraineeScope.NotSchedulable);
        }

        // The trainee must be a current trainee (T238) who trains at the panel's institution, and anyone but an
        // Administrator must oversee them; before T182 only an InstitutionalAdmin's panel was checked, and the trainee not
        // at all, and before T238 an erased trainee's pseudonym passed. Nothing is written until this passes.
        await CommitteeTraineeScope.DemandSchedulableAsync(
            _dbContext, _users, request.Principal, panel, traineeUserId, cancellationToken);

        var period = new AcademicPeriod(request.AcademicYear, request.Semester);

        // T131 slice 5: what the review decides. Before a College committee, entrustment only; a general panel's
        // semester-2 sitting decides progression. Judged from the panel as it is now; the review keeps its type after.
        var bodyName = panel.DecisionBodyKey is { } bodyKey
            ? await DecisionPanelBodies.NameOfAsync(_dbContext, bodyKey, cancellationToken) ?? bodyKey
            : null;
        var reviewType = request.ReviewType ?? CommitteeReviewTypes.DefaultFor(bodyName is not null, period.Semester);
        if (CommitteeReviewTypes.Refusal(reviewType, bodyName, period.Semester) is { } typeRefusal)
        {
            throw new CommitteeRefusedException(typeRefusal);
        }

        // One open binding review per trainee, period and seat (see the remarks): named, so the scheduler can open it
        // instead. The index holds the same panel against a race.
        IReadOnlyList<CommitteeAgendaLine> agenda = [];
        if (!request.IsFormative)
        {
            var open = await CommitteeReviewSeats.HolderAsync(_dbContext, traineeUserId, panel, period, cancellationToken);
            if (open is not null)
            {
                throw new CommitteeRefusedException(AlreadyScheduled(open, await TraineeNameAsync(traineeUserId, cancellationToken), period));
            }

            var plan = await AgendaPlanner.PlanAsync(
                _dbContext, traineeUserId, panel, period, ProgrammeCalendar.DateOf(DateTime.UtcNow), cancellationToken);

            // T131 slice 5: an entrustment-only review decides what its agenda holds, and cannot be recorded with nothing on
            // it (CommitteeReview.EmptyAgendaRefusal). Before a panel that decides no EPA on the trainee's curriculum, due or
            // not, nothing could ever be put there, and the review could never be recorded.
            if (reviewType == CommitteeReviewType.EntrustmentOnly && !plan.PanelDecidesAnything)
            {
                throw new CommitteeRefusedException(NothingToDecide(panel.Name));
            }

            agenda = plan.Lines;
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
            ReviewType = reviewType
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
            var open = await CommitteeReviewSeats.HolderAsync(_dbContext, traineeUserId, panel, period, cancellationToken);
            if (open is null)
            {
                throw;
            }

            throw new CommitteeRefusedException(
                AlreadyScheduled(open, await TraineeNameAsync(traineeUserId, cancellationToken), period), exception);
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

    /// <summary>The trainee by name, as the refusal names them (T364, C7); their id when no name is on record.</summary>
    private async Task<string> TraineeNameAsync(string traineeUserId, CancellationToken cancellationToken)
        => (await UserDisplayNames.ResolveAsync(_users, [traineeUserId], cancellationToken)).NameOf(traineeUserId);

    /// <summary>
    /// The refusal for an entrustment-only review before a panel that decides no EPA on the trainee's curriculum (T131
    /// slice 5). The scheduling preview says the same first (<see cref="CommitteeAgendaPreviewDto.PanelDecidesAnything" />).
    /// </summary>
    internal static string NothingToDecide(string panelName)
        => $"{panelName} decides no EPA on this trainee's curriculum, so an entrustment-only review before it would have " +
           "nothing to decide.";

    /// <summary>
    /// The refusal for a second open binding review of one trainee, in one seat, for one period, and the schedule page's
    /// Already held, which says it before Create (<see cref="GetSchedulingSeatQuery" />): "Review #1 already puts Lerato
    /// Molefe before Paed Annual Review Panel for 2026 S2 (scheduled on 2026-10-12). …". The trainee by name, and
    /// "scheduled" said once (T364, C7): a review under way says its state first, "(in progress, scheduled on …)".
    /// </summary>
    internal static string AlreadyScheduled(SeatHolder open, string traineeName, AcademicPeriod period)
    {
        ArgumentNullException.ThrowIfNull(open);

        var state = open.State == CommitteeReviewState.Scheduled
            ? string.Empty
            : $"{CommitteeDecisionWording.StateInSentence(open.State)}, ";
        return $"Review #{open.Id} already puts {traineeName} before {open.PanelName} for {period} " +
               string.Create(CultureInfo.InvariantCulture, $"({state}scheduled on {open.ScheduledOn:yyyy-MM-dd}). ") +
               "A trainee has one binding review for each period before the panels that decide the same EPAs, until it " +
               "is ratified: open that review, or ratify it before scheduling another.";
    }

    /// <summary>
    /// An evidence window that ends before it starts, refused (T364, S4; T324): "The window ends before it starts. Choose a
    /// day on or after 2026-01-01.", beside To on the schedule page and from the command's validator alike.
    /// </summary>
    public static string WindowEndsBeforeItStarts(DateOnly from)
        => string.Create(CultureInfo.InvariantCulture, $"The window ends before it starts. Choose a day on or after {from:yyyy-MM-dd}.");
}
