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
/// What a binding review of this trainee before this panel, for this period, would put on its agenda: the scheduling
/// form's preview, planned by the planner scheduling itself runs (<see cref="AgendaPlanner" />). (T131 slice 4)
/// </summary>
/// <remarks>
/// Authorised as scheduling is (<see cref="CommitteeTraineeScope.DemandSchedulableAsync" />): whoever may not put this
/// trainee before this panel gets the scheduling refusal, which says nothing about either id. A caller who holds Trainee
/// previews no agenda, whatever other role they hold, and is told so before either id is looked up: the preview names the
/// trainee's standing on every EPA due (T216).
/// </remarks>
/// <param name="Today">The day "missed" is judged on; the programme's today when null.</param>
public sealed record PreviewCommitteeAgendaQuery(
    string TraineeUserId,
    int PanelId,
    int AcademicYear,
    int Semester,
    ClaimsPrincipal Principal,
    DateOnly? Today = null) : IRequest<CommitteeAgendaPreviewDto>;

public sealed class PreviewCommitteeAgendaQueryValidator : AbstractValidator<PreviewCommitteeAgendaQuery>
{
    public PreviewCommitteeAgendaQueryValidator()
    {
        RuleFor(query => query.TraineeUserId).NotEmpty();
        RuleFor(query => query.PanelId).GreaterThan(0);
        RuleFor(query => query.AcademicYear).InclusiveBetween(ReviewPeriodRules.FirstYear, ReviewPeriodRules.LastYear);
        RuleFor(query => query.Semester).InclusiveBetween(1, 2).WithMessage(ReviewPeriodRules.SemesterMessage);
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class PreviewCommitteeAgendaQueryHandler : IRequestHandler<PreviewCommitteeAgendaQuery, CommitteeAgendaPreviewDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public PreviewCommitteeAgendaQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<CommitteeAgendaPreviewDto> Handle(PreviewCommitteeAgendaQuery request, CancellationToken cancellationToken)
    {
        CommitteeDecisionAuthorization.DemandReviewScheduling(request.Principal);

        var traineeUserId = request.TraineeUserId.Trim();
        var panel = await _dbContext.Set<DecisionPanel>()
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == request.PanelId, cancellationToken)
            ?? throw (request.Principal.IsAdministrator()
                ? new InvalidOperationException("The decision panel could not be found.")
                : new UnauthorizedAccessException(CommitteeTraineeScope.NotSchedulable));

        await CommitteeTraineeScope.DemandSchedulableAsync(
            _dbContext, _users, request.Principal, panel, traineeUserId, cancellationToken);

        var sitting = new AcademicPeriod(request.AcademicYear, request.Semester);
        var plan = await AgendaPlanner.PlanAsync(
            _dbContext,
            traineeUserId,
            panel,
            sitting,
            request.Today ?? ProgrammeCalendar.DateOf(DateTime.UtcNow),
            cancellationToken);

        return new CommitteeAgendaPreviewDto(
            sitting.Year,
            sitting.Semester,
            sitting.ToString(),
            plan.TraineeHasCurriculum,
            plan.Lines.Select(line => CommitteeAgendaReader.ToDto(line, sitting, staged: false, decidedElsewhere: false, evidenceCount: 0)).ToArray(),
            plan.RoutedElsewhere,
            plan.DecidedInWindow.Select(epa => epa.Code).ToArray())
        {
            PanelDecidesAnything = plan.PanelDecidesAnything
        };
    }
}

/// <summary>What a review's period may be: one semester of a representable academic year. (T131, Decision 4)</summary>
internal static class ReviewPeriodRules
{
    /// <summary>The first year a period may name: the year after the first representable one, so its preceding semester exists.</summary>
    public const int FirstYear = 2;

    /// <summary>The last year a period may name: the year before the last representable one, so its following semester exists.</summary>
    public const int LastYear = 9998;

    public const string SemesterMessage = "A review sits for semester 1 or semester 2 of an academic year.";
}
