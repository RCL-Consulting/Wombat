using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.DataRights;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Domain.Scheduling;

namespace Wombat.Web.Components.Shared;

/// <summary>
/// The five state badges app.css defines (DESIGN.md § Badges), named after the activity states they were drawn for. Every
/// state a page badges is one of these five; the badge's words say which state it is, and the tint repeats it.
/// </summary>
public enum BadgeState
{
    /// <summary>Grey (<c>badge-draft</c>): not started, nothing wrong yet, or no longer in force.</summary>
    Draft,

    /// <summary>Blue (<c>badge-submitted</c>): handed on, and waiting on someone.</summary>
    Submitted,

    /// <summary>Amber (<c>badge-accepted</c>): taken on and in hand, or wanting attention again.</summary>
    Accepted,

    /// <summary>Green (<c>badge-completed</c>): done, or succeeded.</summary>
    Completed,

    /// <summary>Red (<c>badge-declined</c>): refused, failed, missed or withdrawn.</summary>
    Declined
}

/// <summary>
/// The one place a badge's class is named (T266). A page writes <c>class="badge @BadgeFor.…(…)"</c> and never a
/// <c>badge-</c> class of its own, so no page can name one app.css does not define: until T266 eight pages wore
/// <c>badge-success</c>, <c>-danger</c>, <c>-warning</c>, <c>-info</c> and <c>-primary</c>, and the dashboards
/// <c>badge-{state key}</c>, which is tinted for five keys of the fifteen the seeded workflows use. Each status is mapped
/// here onto a <see cref="BadgeState" />, as DESIGN.md's tables give them. <c>Design/DefinedClassTests</c> holds every
/// value each method can return to app.css, and fails on a <c>badge-</c> class named anywhere else.
/// </summary>
public static class BadgeFor
{
    /// <summary>The class of one of the five state badges.</summary>
    public static string State(BadgeState state) => state switch
    {
        BadgeState.Submitted => "badge-submitted",
        BadgeState.Accepted => "badge-accepted",
        BadgeState.Completed => "badge-completed",
        BadgeState.Declined => "badge-declined",
        _ => "badge-draft"
    };

    /// <summary>
    /// An activity's workflow state (the dashboards' badges, T220). Done is green, and done is a terminal state of the
    /// activity's pinned workflow (<paramref name="isFinished" />, <c>ActivityCompletion</c>, D44), never a key's name:
    /// teaching_session finishes in <c>accepted</c>, which on a Mini-CEX is a supervisor's work in hand, and an
    /// institution's own workflow may finish anywhere (T266 review). A refusal or a cancellation is red, even where a
    /// version makes it terminal. Of the rest, waiting on a supervisor is blue and a supervisor's work in hand amber; any
    /// other key, such as a state an institution's own workflow names, is grey: the badge's words, the state's label, say
    /// what it is, and grey claims nothing.
    /// </summary>
    public static string ActivityState(string? stateKey, bool isFinished) => State(stateKey switch
    {
        "declined" or "rejected" or "cancelled" => BadgeState.Declined,
        _ when isFinished => BadgeState.Completed,
        "submitted" or "requested" => BadgeState.Submitted,
        "accepted" => BadgeState.Accepted,
        _ => BadgeState.Draft
    });

    /// <summary>
    /// A line of a review's agenda (T131 slice 4; DESIGN.md § Badges, "Committee agenda"): the shades of "still due" share
    /// the grey badge and are told apart by their label, and so do the two of "decided".
    /// </summary>
    public static string AgendaLine(CommitteeAgendaLineStatus status) => State(status switch
    {
        CommitteeAgendaLineStatus.Staged => BadgeState.Submitted,
        CommitteeAgendaLineStatus.Decided or CommitteeAgendaLineStatus.DecidedElsewhere => BadgeState.Completed,
        CommitteeAgendaLineStatus.Deferred => BadgeState.Accepted,
        CommitteeAgendaLineStatus.NotDecided => BadgeState.Declined,
        _ => BadgeState.Draft
    });

    /// <summary>Another panel's decision on the agenda; "Missed" is computed, never stored, and is red.</summary>
    public static string AgendaElsewhere(CommitteeAgendaElsewhereStatus status) => State(status switch
    {
        CommitteeAgendaElsewhereStatus.Decided => BadgeState.Completed,
        CommitteeAgendaElsewhereStatus.OnAgenda => BadgeState.Submitted,
        CommitteeAgendaElsewhereStatus.Deferred => BadgeState.Accepted,
        CommitteeAgendaElsewhereStatus.Missed => BadgeState.Declined,
        _ => BadgeState.Draft
    });

    /// <summary>
    /// A decision due (T131 slice 6; DESIGN.md § Badges, "Decisions due"): the agenda's badges, with every shade of "still
    /// to be decided, nothing wrong yet" grey, and every one that needs a decision taken again red.
    /// </summary>
    public static string DecisionDue(EntrustmentDecisionDueStatus status) => State(status switch
    {
        EntrustmentDecisionDueStatus.Decided => BadgeState.Completed,
        EntrustmentDecisionDueStatus.Scheduled => BadgeState.Submitted,
        EntrustmentDecisionDueStatus.Deferred => BadgeState.Accepted,
        EntrustmentDecisionDueStatus.Missed or EntrustmentDecisionDueStatus.Revoked => BadgeState.Declined,
        _ => BadgeState.Draft
    });

    /// <summary>
    /// An MSF campaign's state (T217): a draft is grey, an open campaign blue, one closed and under review amber, a
    /// released one green, and a withdrawn one red.
    /// </summary>
    public static string MsfCampaign(MsfCampaignState state) => State(state switch
    {
        MsfCampaignState.Open => BadgeState.Submitted,
        MsfCampaignState.Closed or MsfCampaignState.UnderReview => BadgeState.Accepted,
        MsfCampaignState.Released => BadgeState.Completed,
        MsfCampaignState.Withdrawn => BadgeState.Declined,
        _ => BadgeState.Draft
    });

    /// <summary>
    /// A STAR's status (T226 review): in force is green; lapsed amber, since the EPA wants deciding again; revoked red;
    /// superseded by a later STAR grey.
    /// </summary>
    public static string EntrustmentDecision(EntrustmentDecisionStatus status) => State(status switch
    {
        EntrustmentDecisionStatus.Active => BadgeState.Completed,
        EntrustmentDecisionStatus.Expired => BadgeState.Accepted,
        EntrustmentDecisionStatus.Revoked => BadgeState.Declined,
        _ => BadgeState.Draft
    });

    /// <summary>
    /// A data rights request's status: waiting on an administrator is blue, under review amber, approved or completed
    /// green, rejected red, and withdrawn by its requester grey.
    /// </summary>
    public static string DataRightsRequest(DataRightsRequestStatus status) => State(status switch
    {
        DataRightsRequestStatus.Submitted => BadgeState.Submitted,
        DataRightsRequestStatus.UnderReview => BadgeState.Accepted,
        DataRightsRequestStatus.Approved or DataRightsRequestStatus.Completed => BadgeState.Completed,
        DataRightsRequestStatus.Rejected => BadgeState.Declined,
        _ => BadgeState.Draft
    });

    /// <summary>
    /// A scheduled job's run, by the status its DTO carries as text (<see cref="ScheduledJobRunStatus" />'s names): running
    /// is amber, succeeded green, failed red. Anything else is grey.
    /// </summary>
    public static string JobRun(string? status) => State(Enum.TryParse<ScheduledJobRunStatus>(status, out var parsed)
        ? parsed switch
        {
            ScheduledJobRunStatus.Running => BadgeState.Accepted,
            ScheduledJobRunStatus.Succeeded => BadgeState.Completed,
            ScheduledJobRunStatus.Failed => BadgeState.Declined,
            _ => BadgeState.Draft
        }
        : BadgeState.Draft);

    /// <summary>An audited action's result: done is green, failed red.</summary>
    public static string AuditResult(bool success) => State(success ? BadgeState.Completed : BadgeState.Declined);

    /// <summary>
    /// A level against a target (T166, <c>EntrustmentStandingPanel</c>): the one badge that is a comparison, not a state.
    /// Body text on a tinted ground, since the semantic colours as text on their own tints fall short of 4.5:1 at 0.75rem.
    /// </summary>
    public static string Standing(EntrustmentStandingStatus status) => status switch
    {
        EntrustmentStandingStatus.AtOrAbove => "badge-standing-met",
        EntrustmentStandingStatus.Below => "badge-standing-below",
        _ => "badge-standing-none"
    };
}
