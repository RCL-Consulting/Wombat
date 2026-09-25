using FluentAssertions;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.DataRights;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Domain.Scheduling;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// DESIGN.md § Badges, one row per status (T266 review). <see cref="DefinedClassTests" /> holds every class
/// <see cref="BadgeFor" /> can return to app.css, which a swapped tint passes: a rejected data rights request shown green,
/// or a failed job run, is still a class app.css defines. So each status's tint is pinned here as the design gives it, and
/// every member of each enum must have its row, so a status added later is a failing test until someone says its tint.
/// </summary>
public sealed class BadgeForStatusTableTests
{
    private const string Grey = "badge-draft";
    private const string Blue = "badge-submitted";
    private const string Amber = "badge-accepted";
    private const string Green = "badge-completed";
    private const string Red = "badge-declined";

    [Fact]
    public void TheFiveStates_AreTheFiveTints()
    {
        Rows<BadgeState>(BadgeFor.State,
            (BadgeState.Draft, Grey),
            (BadgeState.Submitted, Blue),
            (BadgeState.Accepted, Amber),
            (BadgeState.Completed, Green),
            (BadgeState.Declined, Red));
    }

    [Fact]
    public void ADataRightsRequest_IsTintedAsDesignMdGivesIt()
    {
        Rows<DataRightsRequestStatus>(BadgeFor.DataRightsRequest,
            (DataRightsRequestStatus.Submitted, Blue),
            (DataRightsRequestStatus.UnderReview, Amber),
            (DataRightsRequestStatus.Approved, Green),
            (DataRightsRequestStatus.Completed, Green),
            (DataRightsRequestStatus.Rejected, Red),
            (DataRightsRequestStatus.Withdrawn, Grey));
    }

    [Fact]
    public void AJobRun_IsTintedAsDesignMdGivesIt_AndAStatusItDoesNotKnowIsGrey()
    {
        Rows<ScheduledJobRunStatus>(status => BadgeFor.JobRun(status.ToString()),
            (ScheduledJobRunStatus.Running, Amber),
            (ScheduledJobRunStatus.Succeeded, Green),
            (ScheduledJobRunStatus.Failed, Red));

        BadgeFor.JobRun("Skipped").Should().Be(Grey);
        BadgeFor.JobRun(null).Should().Be(Grey, "a job that has never run");
    }

    [Fact]
    public void AnAuditedResult_IsGreenWhenDone_AndRedWhenFailed()
    {
        BadgeFor.AuditResult(true).Should().Be(Green);
        BadgeFor.AuditResult(false).Should().Be(Red);
    }

    [Fact]
    public void AnMsfCampaign_IsTintedAsDesignMdGivesIt()
    {
        Rows<MsfCampaignState>(BadgeFor.MsfCampaign,
            (MsfCampaignState.Draft, Grey),
            (MsfCampaignState.Open, Blue),
            (MsfCampaignState.Closed, Amber),
            (MsfCampaignState.UnderReview, Amber),
            (MsfCampaignState.Released, Green),
            (MsfCampaignState.Withdrawn, Red));
    }

    [Fact]
    public void AStar_IsTintedAsDesignMdGivesIt()
    {
        Rows<EntrustmentDecisionStatus>(BadgeFor.EntrustmentDecision,
            (EntrustmentDecisionStatus.Active, Green),
            (EntrustmentDecisionStatus.Expired, Amber),
            (EntrustmentDecisionStatus.Revoked, Red),
            (EntrustmentDecisionStatus.Superseded, Grey));
    }

    [Fact]
    public void AnAgendaLine_IsTintedAsDesignMdGivesIt()
    {
        Rows<CommitteeAgendaLineStatus>(BadgeFor.AgendaLine,
            (CommitteeAgendaLineStatus.Due, Grey),
            (CommitteeAgendaLineStatus.DueByYearEnd, Grey),
            (CommitteeAgendaLineStatus.PartialPeriod, Grey),
            (CommitteeAgendaLineStatus.AsOpportunityAllows, Grey),
            (CommitteeAgendaLineStatus.Staged, Blue),
            (CommitteeAgendaLineStatus.Decided, Green),
            (CommitteeAgendaLineStatus.DecidedElsewhere, Green),
            (CommitteeAgendaLineStatus.Deferred, Amber),
            (CommitteeAgendaLineStatus.NotDecided, Red));

        Rows<CommitteeAgendaElsewhereStatus>(BadgeFor.AgendaElsewhere,
            (CommitteeAgendaElsewhereStatus.NotYetDecided, Grey),
            (CommitteeAgendaElsewhereStatus.OnAgenda, Blue),
            (CommitteeAgendaElsewhereStatus.Deferred, Amber),
            (CommitteeAgendaElsewhereStatus.Decided, Green),
            (CommitteeAgendaElsewhereStatus.Missed, Red));
    }

    [Fact]
    public void ADecisionDue_IsTintedAsDesignMdGivesIt()
    {
        Rows<EntrustmentDecisionDueStatus>(BadgeFor.DecisionDue,
            (EntrustmentDecisionDueStatus.Decided, Green),
            (EntrustmentDecisionDueStatus.Scheduled, Blue),
            (EntrustmentDecisionDueStatus.Deferred, Amber),
            (EntrustmentDecisionDueStatus.NotScheduled, Grey),
            (EntrustmentDecisionDueStatus.DueByYearEnd, Grey),
            (EntrustmentDecisionDueStatus.PartialPeriod, Grey),
            (EntrustmentDecisionDueStatus.AsOpportunityAllows, Grey),
            (EntrustmentDecisionDueStatus.Missed, Red),
            (EntrustmentDecisionDueStatus.Revoked, Red));
    }

    [Fact]
    public void AStanding_IsItsVerdictsBadge()
    {
        Rows<EntrustmentStandingStatus>(BadgeFor.Standing,
            (EntrustmentStandingStatus.AtOrAbove, "badge-standing-met"),
            (EntrustmentStandingStatus.Below, "badge-standing-below"),
            (EntrustmentStandingStatus.NoDecision, "badge-standing-none"),
            (EntrustmentStandingStatus.NotComparable, "badge-standing-none"));
    }

    /// <summary>One row per member of <typeparamref name="TStatus" />, each checked, and none left out.</summary>
    private static void Rows<TStatus>(Func<TStatus, string> badge, params (TStatus Status, string Class)[] rows)
        where TStatus : struct, Enum
    {
        rows.Select(row => row.Status).Should().BeEquivalentTo(Enum.GetValues<TStatus>(),
            $"every {typeof(TStatus).Name} has its row in DESIGN.md § Badges, and here");
        rows.Select(row => (row.Status, Class: badge(row.Status))).Should().Equal(rows);
    }
}
