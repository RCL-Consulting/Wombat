using Wombat.Application.Features.Activities.Dtos;

namespace Wombat.Application.Features.Dashboards.Assessor;

/// <summary>
/// The Assessor's Home (T350, R1): "Waiting for you" and "Recent decisions", each the Activity inbox's own read, so the
/// cards and the sections they link to cannot disagree (T297).
/// </summary>
/// <param name="Waiting">
/// Every activity waiting on the caller, oldest first (<c>WaitingForYou</c>): the card lists the first five, badges the
/// count and the overdue count in words, and says how many more wait in the inbox (E5).
/// </param>
/// <param name="Decisions">
/// The first page of "Decided by you" at <c>GetAssessorDashboardSummaryQueryHandler.DecisionsListed</c> rows
/// (<c>DecidedByYou</c>): the newest decisions, with the total for "All your decisions".
/// </param>
public sealed record AssessorDashboardSummaryDto(WaitingForYouDto Waiting, ActivityListPageDto Decisions);
