using System.Security.Claims;
using MediatR;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Queries.ListNeedsYou;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Dashboards.Trainee;

/// <param name="AsOf">The day to read curriculum progress for. Defaults to today in South Africa; tests pin it.</param>
public sealed record GetTraineeDashboardSummaryQuery(ClaimsPrincipal Principal, DateOnly? AsOf = null) : IRequest<TraineeDashboardSummaryDto>;

/// <summary>
/// The Trainee's Home (T355, R1; Q2; Q3): four parts in one read, so a failure is Home's one load error (note 3). Each is
/// read by the code the page it links to reads with, so a card and its page cannot disagree (T297).
/// </summary>
/// <remarks>
/// Every read is the caller's own: the targets and the standing by their user id, Needs you and Recent decisions through
/// the shared readers (<see cref="NeedsYou" />, <see cref="DecidedOnYours" />), each of which puts her rows through the read
/// rule. Until T355 the handler also read Recent activities (the five newest by creation) and Upcoming deadlines (a
/// <c>due_date</c> field scanned out of the data) itself; both are retired (Q3; T298).
/// </remarks>
public sealed class GetTraineeDashboardSummaryQueryHandler
    : IRequestHandler<GetTraineeDashboardSummaryQuery, TraineeDashboardSummaryDto>
{
    /// <summary>
    /// How many of the Needs you rows Home's card lists: its first, the most recently updated. The summary carries them
    /// all, so the card's count is the whole of it (T342, lane D).
    /// </summary>
    public const int NeedsYouListed = 5;

    /// <summary>How many recent decisions Home lists, newest first, with no footer (T355, R1).</summary>
    public const int RecentDecisionsListed = 5;

    private readonly IApplicationDbContext _dbContext;
    private readonly IWorkflowEvaluator _workflowEvaluator;
    private readonly IUserAdministrationService _users;
    private readonly TimeProvider _clock;

    /// <param name="clock">"Today" on the South African calendar (T325); the system clock when none is given.</param>
    public GetTraineeDashboardSummaryQueryHandler(
        IApplicationDbContext dbContext,
        IWorkflowEvaluator workflowEvaluator,
        IUserAdministrationService users,
        TimeProvider? clock = null)
    {
        _dbContext = dbContext;
        _workflowEvaluator = workflowEvaluator;
        _users = users;
        _clock = clock ?? TimeProvider.System;
    }

    public async Task<TraineeDashboardSummaryDto> Handle(
        GetTraineeDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var userId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var isPending = request.Principal.IsInRole("PendingTrainee") &&
                        !request.Principal.IsInRole("Trainee");

        if (isPending)
        {
            return new TraineeDashboardSummaryDto(null, [], [], null, IsPendingTrainee: true);
        }

        var today = QuotaCalendar.Today(_clock);

        // The same read model as the progress page (T130), so the card and the page cannot disagree. It is
        // item-driven: a period that has only just begun reads "0 of 3" rather than showing nothing.
        var curriculumTargets = await TraineeQuotaProgressReader.ReadAsync(
            _dbContext, userId, request.AsOf ?? today, cancellationToken);

        // T297, restated by flow 03 (T342, E8; lane D): Home's card is Needs you, and it lists what My activities' Needs
        // you section lists, read by the same code (NeedsYou.ReadAsync, ListNeedsYouQuery's own read): the caller's
        // drafts and the work returned to them.
        var needsYou = await NeedsYou.ReadAsync(_dbContext, _workflowEvaluator, _users, request.Principal, cancellationToken);

        // T355 (B1; E6; note 1): what someone else decided on her requests, in place of Recent activities. A declined
        // request is here with its badge and "File it again, to someone else".
        var recentDecisions = await DecidedOnYours.ReadAsync(
            _dbContext, _users, request.Principal, RecentDecisionsListed, today, cancellationToken);

        // Note 3: the standing in summary mode, inside this one read, as on the day the targets are read for: the last day
        // of an ended programme (T252), so a graduate's card reads the training year they ended in. No latest rating is
        // read: Home shows none.
        var standing = curriculumTargets is null
            ? null
            : await EntrustmentStandingReader.ReadAsync(
                _dbContext, request.Principal, userId, curriculumTargets.AsOf, withLatestRatings: false, cancellationToken);

        return new TraineeDashboardSummaryDto(
            curriculumTargets,
            needsYou,
            recentDecisions,
            standing,
            IsPendingTrainee: false);
    }

    public static int? ComputeTraineeStage(DateOnly programmeStart, DateOnly today)
        => new TraineeProfile { ProgrammeStartDate = programmeStart }.GetStage(today);
}
