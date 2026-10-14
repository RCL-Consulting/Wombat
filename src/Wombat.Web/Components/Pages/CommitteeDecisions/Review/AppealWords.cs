using System.Globalization;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Web.Components.Pages.CommitteeDecisions.Review;

/// <summary>
/// Every sentence an appeal ships (T366, flow 09; boards R3-2 and R3-3): the appeal entry both review pages draw
/// (<c>AppealEntry</c>), and the trainee's lodge block and its dialog (<c>MyReview</c>). No pronoun names a person: the
/// trainee reads "your", as the boards do, and everyone else by name.
/// </summary>
public static class AppealWords
{
    // ─── The entry (AppealEntry) ─────────────────────────────────────────────────

    /// <summary>The trainee's own entry's heading.</summary>
    public const string YourAppeal = "Your appeal";

    /// <summary>The committee's entry's heading: "Appeal by Nomsa Mahlangu" (T308).</summary>
    public static string AppealBy(string? name) => string.IsNullOrWhiteSpace(name) ? "Appeal" : $"Appeal by {name}";

    /// <summary>An open appeal's badge word; a resolved one reads its outcome (<see cref="CommitteeDecisionWording.AppealOutcomeName" />).</summary>
    public const string Open = "Open";

    /// <summary>The badge's word for an appeal.</summary>
    public static string StateWord(CommitteeAppealOutcome? outcome)
        => outcome is { } resolved ? CommitteeDecisionWording.AppealOutcomeName(resolved) : Open;

    /// <summary>"Lodged 2026-10-13", then " · Resolved 2026-10-13 by Thandi Zulu" once resolved (C15, T308).</summary>
    public static string Meta(string lodgedOn, string? resolvedOn, string? resolvedBy)
    {
        var lodged = $"Lodged {lodgedOn}";
        if (resolvedOn is null)
        {
            return lodged;
        }

        return string.IsNullOrWhiteSpace(resolvedBy)
            ? $"{lodged} · Resolved {resolvedOn}"
            : $"{lodged} · Resolved {resolvedOn} by {resolvedBy}";
    }

    /// <summary>
    /// "Heard by Thandi Zulu (chair) and John van Rensburg (external).": an open appeal's appeal body, those who can act
    /// now; null when none can, and the entry then names nobody (board 3 c, C12a).
    /// </summary>
    public static string? HeardBy(IReadOnlyList<CommitteePersonDto> appealBody)
        => appealBody.Count == 0 ? null : $"Heard by {CommitteeAgendaText.Codes(appealBody.Select(person => person.Label))}.";

    /// <summary>A resolved appeal's outcome in a sentence: the trainee's ends "and it is final"; the committee's does not.</summary>
    public static string Outcome(CommitteeAppealOutcome outcome, bool forTrainee) => (outcome, forTrainee) switch
    {
        (CommitteeAppealOutcome.Dismissed, true) => "Dismissed: the decision stands, and it is final.",
        (CommitteeAppealOutcome.Dismissed, false) => "Dismissed: the decision stands.",
        (CommitteeAppealOutcome.Remitted, true) =>
            "Remitted: the appeal body replaced the decision. The decision above is theirs, and it is final.",
        (CommitteeAppealOutcome.Remitted, false) =>
            "Remitted: the appeal body replaced the decision. The replacement decision is above.",
        _ => CommitteeDecisionWording.AppealOutcomeName(outcome)
    };

    public const string ReasonLabel = "Reason";

    /// <summary>The clamped reason's control (S10), shown only when the reason overflows its six lines.</summary>
    public const string ShowWholeReason = "Show the whole reason";

    // ─── The trainee's Appeal section (MyReview) ─────────────────────────────────

    public const string AppealHeading = "Appeal";
    public const string WhatHappensNext = "What happens next";

    /// <summary>
    /// Who hears the appeal (C8): "Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van
    /// Rensburg (external)." Read for her Ratified review (round 2's data row). Said truly when nobody on it can act now.
    /// </summary>
    public static string HeardByTheAppealBody(IReadOnlyList<CommitteePersonDto> appealBody)
        => appealBody.Count == 0
            ? "Your appeal is heard by the panel's appeal body, its chair and its external members. None of them can act " +
              "today: a panel administrator must seat a chair who can before it is heard."
            : $"Your appeal is heard by the panel's appeal body: today, {CommitteeAgendaText.Codes(appealBody.Select(person => person.Label))}.";

    /// <summary>What the appeal body may do, and that either outcome is final (D51: Dismissed or Remitted, never Upheld).</summary>
    public const string Outcomes =
        "They dismiss it, and the decision stands, or remit it, and take a new decision in its place. Either outcome is " +
        "final: nothing reopens the review, and it cannot be appealed again.";

    /// <summary>
    /// Every reader of the reason, named truly (C8c): those who can open the review on the committee's side, the review
    /// ladder's rungs (<c>CommitteeDecisionAuthorization.MayReadReview</c>), the panel's institution by name.
    /// </summary>
    public static string Readers(string? institutionName)
        => "Who reads your reason: everyone who can open this review on the committee's side: the panel's members, the " +
           $"coordinators and institutional administrators of {(string.IsNullOrWhiteSpace(institutionName) ? "the panel's institution" : institutionName)}, " +
           "your programme's speciality and sub-speciality administrators, and Wombat's system administrators.";

    public const string ReasonFieldLabel = "Why you are appealing";

    /// <summary>The reason's help, its first <c>.field-help</c>.</summary>
    public static string ReasonHelp => $"Up to {Thousands(LodgeAppealCommand.MaxReasonLength)} characters.";

    /// <summary>The counter, the reason's second <c>.field-help</c> (S9): "153 of 4,000 characters".</summary>
    public static string Counter(int length) => $"{Thousands(length)} of {Thousands(LodgeAppealCommand.MaxReasonLength)} characters";

    public const string LodgeLabel = "Lodge appeal";

    /// <summary>Lodge appeal pressed with the reason blank: under the field, the field takes the focus.</summary>
    public const string GiveReason = LodgeAppealCommand.GiveReason;

    /// <summary>Lodge appeal pressed with a reason over the limit: the counter shows by how much.</summary>
    public const string ReasonTooLong = LodgeAppealCommand.ReasonTooLong;

    // ─── The lodge dialog (C14a; ConfirmDialog's action mode, as built) ──────────

    public const string DialogTitle = "Lodge your appeal?";
    public const string KeepEditing = "Keep editing";

    /// <summary>"Against the 2026 S2 annual progression review's decision, Inadequate Progress — Additional Training."</summary>
    public static string Against(string reviewHeading, CommitteeDecisionCategory? category)
        => $"Against the {reviewHeading}'s decision, {CommitteeDecisionWording.OutcomeLabel(category)}.";

    /// <summary>The dialog's appeal body sentence, shorter than the block's: "… Either outcome is final."</summary>
    public static string DialogHeardBy(IReadOnlyList<CommitteePersonDto> appealBody)
        => $"{HeardByTheAppealBody(appealBody)} Either outcome is final.";

    /// <summary>After the lodge; the result takes the focus.</summary>
    public const string Lodged = "Appeal lodged.";

    /// <summary>A lodge that failed for no reason Wombat gave: never the exception's text (T329, T338).</summary>
    public const string LodgeFailed = "Your appeal could not be lodged. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>A closed review, once its appeal is resolved (4.48).</summary>
    public const string ClosedNoMoreAppeals = "This review is closed and cannot be appealed again.";

    private static string Thousands(int value) => value.ToString("N0", CultureInfo.InvariantCulture);
}
