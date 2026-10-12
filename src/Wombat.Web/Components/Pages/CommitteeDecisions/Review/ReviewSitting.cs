using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Components.Pages.CommitteeDecisions.Review;

/// <summary>The one act a sitting bar offers (T361, R2-Spec "SittingBar").</summary>
public enum SittingAct
{
    None,
    Start,
    GoToDecision,
    Ratify,
    Close
}

/// <summary>
/// What the sitting bar says and offers (T361, flow 07; R2-Spec "SittingBar"; round 3 items 14, 21, 28; E2, E7, E8).
/// </summary>
/// <param name="Chairs">
/// The chair's bar: an act is the chair's (the chair may act and the review is scheduled, in progress or decided). Sticky
/// from 641 px, its stripe the action blue. Every other bar is a reader's (<c>.sitting-bar--reader</c>): static under the
/// title, its stripe the input border, or the action blue where the reader may start the review (E2).
/// </param>
/// <param name="Count">"3 staged", "15 EPAs on the agenda", "Satisfactory Progress · 3 decisions to issue".</param>
/// <param name="Mode">Before Start, the mode and the evidence window (28); a formative review's mode while in progress.</param>
/// <param name="Present">The chair's bar's Present, from the ticks, while the review is in progress (not live).</param>
/// <param name="Sentence">A reader's who-may-act sentence, or why nobody can act.</param>
/// <param name="ActReason">Why the act is unavailable (Ratify short of a quorum), or null.</param>
/// <param name="Refresh">A reader's "As of … SAST" and Refresh, while the review can still change (round-3-check).</param>
public sealed record SittingBarView(
    bool Chairs,
    string StateLabel,
    string BadgeClass,
    string? Count,
    string? Mode,
    string? Present,
    string? Sentence,
    SittingAct Act,
    string? ActReason,
    bool Refresh)
{
    /// <summary>A reader who may start the review: the bar's stripe is the action blue (E2).</summary>
    public bool StartsAsReader => !Chairs && Act == SittingAct.Start;
}

/// <summary>
/// The review page's reading of a review, apart from its markup so each sentence and choice can be held by a test (T361).
/// </summary>
public static class ReviewSitting
{
    /// <summary>
    /// Whether the reader may take the chair's actions (T213, T256): they hold the Chair seat and may sit, and the trainee
    /// check those actions demand passes for them.
    /// </summary>
    public static bool ChairMayAct(CommitteeReviewDetailDto review)
        => review is { CallerChairs: true, TraineeElsewhere: null };

    /// <summary>Whether Start is offered: scheduled, the handler lets the reader start it, and the chair can act (E2, Q11).</summary>
    public static bool OffersStart(CommitteeReviewDetailDto review)
        => review is { State: CommitteeReviewState.Scheduled, CallerMayStart: true, TraineeElsewhere: null, ChairCannotAct: null };

    /// <summary>The review's state badge: one of the five (DESIGN.md § Badges), named in one place (T266).</summary>
    public static string StateBadge(CommitteeReviewState state) => BadgeFor.State(state switch
    {
        CommitteeReviewState.InProgress => BadgeState.Submitted,
        CommitteeReviewState.Decided or CommitteeReviewState.UnderAppeal => BadgeState.Accepted,
        CommitteeReviewState.Ratified or CommitteeReviewState.Final => BadgeState.Completed,
        CommitteeReviewState.Withdrawn => BadgeState.Declined,
        _ => BadgeState.Draft
    });

    /// <summary>How many decisions are staged: the staged list, else the agenda's staged lines.</summary>
    public static int StagedCount(CommitteeAgendaDto? agenda, IReadOnlyList<PendingEntrustmentDecisionDto> pending)
        => pending.Count > 0 ? pending.Count : agenda?.Lines.Count(line => line.IsStaged) ?? 0;

    /// <summary>
    /// The codes of every line Record leaves unstaged and records as not decided (E1), due and optional alike, in agenda
    /// order: never a staged line, nor one another sitting decided, nor one already settled.
    /// </summary>
    public static IReadOnlyList<string> NotStagedCodes(
        CommitteeAgendaDto? agenda, IReadOnlyCollection<PendingEntrustmentDecisionDto>? pending = null)
        => agenda is null
            ? []
            : agenda.Lines
                .Where(line => !line.IsStaged &&
                               pending?.Any(staged => staged.EpaId == line.EpaId) != true &&
                               line.State == CommitteeAgendaLineState.Due &&
                               line.Status is not (CommitteeAgendaLineStatus.DecidedElsewhere or CommitteeAgendaLineStatus.Staged))
                .Select(line => line.EpaCode)
                .ToArray();

    /// <summary>The codes of the lines a decided review's agenda reads "Not decided at this sitting", which Ratify names (E1).</summary>
    public static IReadOnlyList<string> NotDecidedCodes(CommitteeAgendaDto? agenda)
        => agenda is null ? [] : agenda.Lines.Where(line => line.ReadsNotDecided).Select(line => line.EpaCode).ToArray();

    /// <summary>How many STARs ratifying issued: the lines it decided (staging always leaves the EPA a line).</summary>
    public static int IssuedCount(CommitteeAgendaDto? agenda)
        => agenda?.Lines.Count(line => line.Status == CommitteeAgendaLineStatus.Decided) ?? 0;

    /// <summary>The sitting bar for this reader.</summary>
    /// <param name="present">How many the decision form's ticks name as present, the chair included.</param>
    public static SittingBarView Bar(
        CommitteeReviewDetailDto review,
        CommitteeAgendaDto? agenda,
        IReadOnlyList<PendingEntrustmentDecisionDto> pending,
        int present)
    {
        ArgumentNullException.ThrowIfNull(review);
        ArgumentNullException.ThrowIfNull(pending);

        var chairMayAct = ChairMayAct(review);
        var open = review.State is CommitteeReviewState.Scheduled or CommitteeReviewState.InProgress or CommitteeReviewState.Decided;
        var act = review.State switch
        {
            CommitteeReviewState.Scheduled when OffersStart(review) => SittingAct.Start,
            CommitteeReviewState.InProgress when chairMayAct => review.IsFormative ? SittingAct.Close : SittingAct.GoToDecision,
            CommitteeReviewState.Decided when chairMayAct && !review.IsFormative => SittingAct.Ratify,
            _ => SittingAct.None
        };
        var chairs = chairMayAct && open && act != SittingAct.None;

        return new SittingBarView(
            chairs,
            review.StateLabel,
            StateBadge(review.State),
            Count(review, agenda, pending),
            review.State == CommitteeReviewState.Scheduled ? ReviewWords.ModeBeforeStart(review)
                : review is { State: CommitteeReviewState.InProgress, IsFormative: true } ? ReviewWords.FormativeMode
                : null,
            chairs && review is { State: CommitteeReviewState.InProgress, IsFormative: false }
                ? ReviewWords.Present(present, review.PanelShortfall is not null)
                : null,
            chairs ? null : Sentence(review, pending),
            act,
            act == SittingAct.Ratify ? review.QuorumShortfall : null,
            !chairs && open);
    }

    private static string? Count(
        CommitteeReviewDetailDto review, CommitteeAgendaDto? agenda, IReadOnlyList<PendingEntrustmentDecisionDto> pending)
    {
        var outcome = review.Decisions.FirstOrDefault() is { } decision
            ? CommitteeDecisionWording.OutcomeLabel(decision.Category)
            : null;

        return review.State switch
        {
            CommitteeReviewState.Scheduled when !review.IsFormative && agenda is not null => ReviewWords.OnTheAgenda(agenda.Lines.Count),
            CommitteeReviewState.InProgress when !review.IsFormative => ReviewWords.Staged(StagedCount(agenda, pending)),
            CommitteeReviewState.Decided when outcome is not null => ReviewWords.ToIssue(outcome, StagedCount(agenda, pending)),
            CommitteeReviewState.Ratified or CommitteeReviewState.UnderAppeal or CommitteeReviewState.Final
                when outcome is not null && !review.IsFormative => ReviewWords.Issued(outcome, IssuedCount(agenda)),
            CommitteeReviewState.Final when review.IsFormative => ReviewWords.FormativeClosed,
            _ => null
        };
    }

    /// <summary>
    /// Who can act now, to a reader who is not offered the chair's controls (T213): why nobody can (the trainee elsewhere,
    /// the chair unable to act), who can start a scheduled review (E2), or the chair by name and what only the chair can do.
    /// </summary>
    private static string? Sentence(CommitteeReviewDetailDto review, IReadOnlyList<PendingEntrustmentDecisionDto> pending)
    {
        if (review.WithdrawnOn is { } withdrawnOn)
        {
            return ReviewWords.Withdrawn(withdrawnOn, review.WithdrawalReason);
        }

        if (review.TraineeElsewhere is { } elsewhere)
        {
            return elsewhere;
        }

        if (review.ChairCannotAct is { } cannotAct)
        {
            return cannotAct;
        }

        if (review is { State: CommitteeReviewState.Scheduled, CallerMayStart: false })
        {
            return ReviewWords.WhoCanStart;
        }

        if (review.CallerChairs)
        {
            return null;
        }

        var chair = review.PanelMembers.FirstOrDefault(member => member.Role == DecisionPanelMemberRole.Chair);
        var only = CommitteeDecisionWording.OnlyTheChair(chair?.DisplayName);
        return review switch
        {
            { IsFormative: true, State: CommitteeReviewState.InProgress } => $"{only} can close this review.",
            { IsFormative: false, State: CommitteeReviewState.InProgress } => CommitteeDecisionWording.OnlyTheChairSits(chair?.DisplayName),
            { IsFormative: false, State: CommitteeReviewState.Decided } =>
                pending.Any(staged => staged.NoLongerFits is not null)
                    ? $"{only} can remove a staged decision that no longer fits the trainee's curriculum and ratify the committee's decision."
                    : $"{only} can ratify the committee's decision.",
            _ => null
        };
    }
}
