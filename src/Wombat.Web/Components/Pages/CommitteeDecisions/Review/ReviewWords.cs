using System.Globalization;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.Epas;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Web.Components.Pages.CommitteeDecisions.Review;

/// <summary>
/// Every sentence the committee review page ships (T361, flow 07; round 3's boards, R2-Spec), singular and plural, built
/// here in C# because Razor drops a space standing alone before an expression and runs sentences together. No pronoun in
/// any of them (round-3-check 3): the person's name, "the chair", "the registrar". No deferral anywhere (D53): every line
/// left unstaged is "not decided at this sitting". Dates are ISO and times carry "SAST" (T325).
/// </summary>
public static class ReviewWords
{
    // ─── The header and the page's states (T329; round 3 items 19, E6) ─────────

    /// <summary>The h1 while the review is read, and when the read failed (board 8).</summary>
    public const string LoadingHeading = "Committee review";

    /// <summary>What the skeleton says while the review is read (board 8).</summary>
    public const string Loading = "Loading the review…";

    /// <summary>The page's load error: a fixed sentence, never an exception's text (T329).</summary>
    public const string LoadFailed = "The review could not be loaded. Nothing on it has changed.";

    /// <summary>Flow 01's not found, with this page's sentence (board 8): the one refusal the review's reads give (T194).</summary>
    public const string NotFoundHeading = "Page not found";

    /// <inheritdoc cref="NotFoundHeading" />
    public const string NotFound = "The committee review could not be found among the reviews you can view.";

    /// <summary>Not found's way back.</summary>
    public const string BackToReviews = "Back to Committee reviews";

    /// <summary>The h1 and the tab's stem: "Lerato Molefe · 2026 S2" (the title rule, round 3).</summary>
    public static string Heading(CommitteeReviewDetailDto review)
    {
        ArgumentNullException.ThrowIfNull(review);
        return $"{(string.IsNullOrWhiteSpace(review.TraineeName) ? LoadingHeading : review.TraineeName)} · {review.PeriodLabel}";
    }

    /// <summary>
    /// "Annual progression review · Paed Annual Review Panel · Training year 4": the type ("Formative check-in" for a
    /// formative review, T309 item 2), the panel, and the training year at the end of the review's period (E6).
    /// </summary>
    public static string Subtitle(CommitteeReviewDetailDto review)
    {
        ArgumentNullException.ThrowIfNull(review);
        var parts = new List<string> { review.ReviewTypeLabel, review.PanelName };
        if (review.TrainingYear is int year)
        {
            parts.Add($"Training year {year}");
        }

        return string.Join(" · ", parts);
    }

    /// <summary>"Lerato Molefe's progress": the registrar page, in PageHeader's SubtitleContent (round 3 item 19).</summary>
    public static string ProgressLink(string traineeName) => $"{traineeName}'s progress";

    // ─── The sitting bar (round 3 items 14, 21, 28; E2, E7, E8) ──────────────────

    /// <summary>The bar's section's name, a visually hidden h2.</summary>
    public const string SittingHeading = "The sitting";

    public const string StartLabel = "Start review…";
    public const string GoToDecisionLabel = "Go to the decision";
    public const string RatifyLabel = "Ratify…";
    public const string CloseLabel = "Close review…";
    public const string RefreshLabel = "Refresh";

    /// <summary>The mode of a formative review (round 3 item 28).</summary>
    public const string FormativeMode = "Formative: an interim check-in, no binding decision";

    /// <summary>
    /// Before Start, the mode and the evidence window (28): "Summative · evidence 2026-01-01 to 2026-12-31", or the
    /// formative mode with the window.
    /// </summary>
    public static string ModeBeforeStart(CommitteeReviewDetailDto review)
    {
        ArgumentNullException.ThrowIfNull(review);
        var window = $"evidence {Iso(review.ReviewPeriodFrom)} to {Iso(review.ReviewPeriodTo)}";
        return review.IsFormative ? $"{FormativeMode} · {window}" : $"Summative · {window}";
    }

    /// <summary>"15 EPAs on the agenda", before Start.</summary>
    public static string OnTheAgenda(int lines) => $"{Count(lines, "EPA", "EPAs")} on the agenda";

    /// <summary>"None staged", "1 staged", "3 staged": the bar's and the caption's count while the review is in progress.</summary>
    public static string Staged(int staged) => staged == 0 ? "None staged" : $"{staged} staged";

    /// <summary>After Record: "Satisfactory Progress · 3 decisions to issue".</summary>
    public static string ToIssue(string outcome, int staged)
        => $"{outcome} · {(staged == 0 ? "no decision to issue" : Count(staged, "decision", "decisions") + " to issue")}";

    /// <summary>Once ratified: "Satisfactory Progress · 3 STARs issued", "· no STAR issued".</summary>
    public static string Issued(string outcome, int issued)
        => $"{outcome} · {(issued == 0 ? "no STAR issued" : Count(issued, "STAR", "STARs") + " issued")}";

    /// <summary>A closed formative review's count.</summary>
    public const string FormativeClosed = "Formative check-in · no decision recorded";

    /// <summary>
    /// The chair's bar's Present, from the ticks (not live: the decision form's quorum line is): "Present: the chair · one
    /// more member needed", "4 present · quorum met", or, when the panel cannot sit a quorum, "Present: the chair · no
    /// other member may sit" (round 3 item 7).
    /// </summary>
    public static string Present(int present, bool panelCannotSit)
        => panelCannotSit ? "Present: the chair · no other member may sit"
            : present < CommitteeReview.Quorum ? "Present: the chair · one more member needed"
            : $"{present} present · quorum met";

    /// <summary>
    /// A reader's "As of 14:02 SAST" on the day it was read, "As of 2026-10-04 14:02 SAST" on another (E7); after
    /// Refresh, what changed: "As of 14:31 SAST. Now Decided." or "As of 14:31 SAST. No change." (21; round-3-check).
    /// </summary>
    public static string AsOf(DateTime readUtc, DateOnly today, string? change)
    {
        var local = DateTime.SpecifyKind(readUtc, DateTimeKind.Utc).AddHours(2);
        var day = DateOnly.FromDateTime(local);
        var time = local.ToString("HH:mm", CultureInfo.InvariantCulture);
        var stamp = day == today ? $"As of {time} SAST" : $"As of {Iso(day)} {time} SAST";
        return change is null ? stamp : $"{stamp}. {change}";
    }

    /// <summary>What a Refresh found: the review's new state, a new count of staged decisions, or nothing (21).</summary>
    public static string RefreshChange(string? newStateLabel, int? newStaged)
        => newStateLabel is not null ? $"Now {newStateLabel}."
            : newStaged is int staged ? $"Now {(staged == 0 ? "none" : staged.ToString(CultureInfo.InvariantCulture))} staged."
            : "No change.";

    /// <summary>A Refresh whose read failed: the page stands as last read.</summary>
    public const string RefreshFailed = "The review could not be read again; it is shown as last read.";

    /// <summary>
    /// Who can start a scheduled review, to a reader not offered Start (E2, T194): a member counts only while they may sit
    /// on the panel (T279).
    /// </summary>
    public const string WhoCanStart =
        "Only the coordinators of the panel's institution, and those of its members who are active committee members " +
        "there, can start this review.";

    /// <summary>A withdrawn review's line, where its state is read (T258).</summary>
    public static string Withdrawn(DateOnly withdrawnOn, string? reason)
        => string.IsNullOrWhiteSpace(reason)
            ? $"Withdrawn on {Iso(withdrawnOn)}."
            : $"Withdrawn on {Iso(withdrawnOn)}. {reason.Trim()}";

    // ─── Results (round 3 item 15: beside the control, taking the focus) ─────────

    public const string Started = "Review started.";
    public const string StagedResult = "Pending entrustment decision staged.";
    public const string RemovedResult = "Pending entrustment decision removed.";
    public const string Recorded = "Decision recorded.";
    public const string Ratified = "Decision ratified.";
    public const string Closed = "Formative review closed.";
    public const string AppealResolved = "Appeal resolved.";

    // ─── The dialogs (round 3 items 11, 22; E1) ──────────────────────────────────

    /// <summary>The safe button of every dialog but Remove's (22).</summary>
    public const string NotYet = "Not yet";

    /// <summary>Remove's safe button (22).</summary>
    public const string KeepIt = "Keep it";

    public const string StartConfirm = "Start review";
    public const string RecordConfirm = "Record decision";
    public const string CloseConfirm = "Close review";
    public const string RemoveConfirm = "Remove the decision";

    /// <summary>"Start Lerato Molefe's review for 2026 S2?"</summary>
    public static string StartTitle(CommitteeReviewDetailDto review)
        => $"Start {Possessive(review.TraineeName)} review for {review.PeriodLabel}?";

    /// <summary>
    /// What Start freezes, counted by the code Start runs (GetStartPreviewQuery, round 3 item 11): "Starting freezes the
    /// evidence: the 9 items observed from 2026-01-01 to 2026-12-31 (8 activities and the MSF campaign report) become this
    /// review's snapshot, each in the state it is in now. Anything finished later is not added." Without a count (the
    /// preview could not be read), the window alone.
    /// </summary>
    public static string StartBody(StartPreviewDto? preview, DateOnly from, DateOnly to)
    {
        const string After = "Anything finished later is not added.";
        if (preview is null)
        {
            return $"Starting freezes the evidence observed from {Iso(from)} to {Iso(to)} as this review's snapshot, each item " +
                   $"in the state it is in now. {After}";
        }

        if (preview.ItemCount == 0)
        {
            return $"Starting freezes the evidence: nothing was observed from {Iso(preview.WindowFrom)} to " +
                   $"{Iso(preview.WindowTo)}, so this review's snapshot will be empty. {After}";
        }

        var parts = new List<string>();
        if (preview.ActivityCount > 0)
        {
            parts.Add(Count(preview.ActivityCount, "activity", "activities"));
        }

        if (preview.CampaignReportCount > 0)
        {
            parts.Add(preview.CampaignReportCount == 1
                ? "the MSF campaign report"
                : $"{preview.CampaignReportCount} MSF campaign reports");
        }

        var what = preview.ItemCount == 1 ? "the 1 item" : $"the {preview.ItemCount} items";
        return $"Starting freezes the evidence: {what} observed from {Iso(preview.WindowFrom)} to {Iso(preview.WindowTo)} " +
               $"({CommitteeAgendaText.Codes(parts)}) {(preview.ItemCount == 1 ? "becomes" : "become")} this review's " +
               $"snapshot, each in the state it is in now. {After}";
    }

    /// <summary>T173's sentence, in the Start dialog only when it applies.</summary>
    public static string? CampaignsLeftOut(int awaiting)
        => awaiting <= 0
            ? null
            : $"Starting now leaves out {Count(awaiting, "feedback campaign", "feedback campaigns")} that closed in this " +
              $"window and {(awaiting == 1 ? "is" : "are")} not yet released.";

    /// <summary>"Remove the staged decision on PAED-012?" (and the row action's name, without the question mark).</summary>
    public static string RemoveName(string epaCode) => $"Remove the staged decision on {epaCode}";

    /// <summary>
    /// What Remove discards: "Its level 5, its rationale and the 2 snapshot items it rests on are discarded. PAED-012 is
    /// due at this sitting again."; the second sentence only for a line the agenda planned.
    /// </summary>
    public static string RemoveBody(PendingEntrustmentDecisionDto pending, bool dueAgain)
    {
        ArgumentNullException.ThrowIfNull(pending);
        var items = pending.EvidenceItemIds.Count == 0
            ? "its rationale are discarded."
            : $"its rationale and the {Count(pending.EvidenceItemIds.Count, "snapshot item", "snapshot items")} it rests on are discarded.";
        var level = $"Its level {pending.AuthorisedLevelLabel}, {items}";
        return dueAgain ? $"{level} {pending.EpaCode} is due at this sitting again." : level;
    }

    /// <summary>"Record Satisfactory Progress for Lerato Molefe?"; an entrustment-only review records its staged decisions.</summary>
    public static string RecordTitle(CommitteeDecisionCategory? category, string? traineeName)
        => category is { } value
            ? $"Record {CommitteeDecisionWording.CategoryLabel(value)} for {traineeName}?"
            : $"Record the entrustment decisions for {traineeName}?";

    /// <summary>"Fixes 3 staged decisions: PAED-001 at 5, PAED-010 at 4 (expires 2026-10-25), PAED-012 at 5." (E1)</summary>
    public static string Fixes(IReadOnlyList<PendingEntrustmentDecisionDto> staged)
    {
        ArgumentNullException.ThrowIfNull(staged);
        if (staged.Count == 0)
        {
            return "Fixes no staged decision.";
        }

        var each = staged.Select(pending => pending.ExpiresOn is DateOnly expires
            ? $"{pending.EpaCode} at {pending.AuthorisedLevelLabel} (expires {Iso(expires)})"
            : $"{pending.EpaCode} at {pending.AuthorisedLevelLabel}");
        return $"Fixes {Count(staged.Count, "staged decision", "staged decisions")}: {string.Join(", ", each)}.";
    }

    /// <summary>
    /// "Records 12 EPAs as not decided: PAED-002, PAED-003, … and PAED-015." (E1): every line left unstaged, due and
    /// optional alike. Null when every line is staged.
    /// </summary>
    public static string? RecordsNotDecided(IReadOnlyList<string> codes)
    {
        ArgumentNullException.ThrowIfNull(codes);
        return codes.Count == 0
            ? null
            : $"Records {Count(codes.Count, "EPA", "EPAs")} as not decided: {CommitteeAgendaText.Codes(codes)}.";
    }

    /// <summary>"Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)."</summary>
    public static string PresentList(IEnumerable<string> labels) => $"Present: {string.Join(", ", labels)}.";

    /// <summary>The last line of the Record dialog.</summary>
    public const string RecordAfterwards = "Afterwards only a decision that no longer fits the trainee's curriculum can be removed.";

    /// <summary>"Ratify and issue 3 STARs to Lerato Molefe?", or with nothing staged "Ratify Lerato Molefe's review?".</summary>
    public static string RatifyTitle(int staged, string? traineeName)
        => staged == 0
            ? $"Ratify {Possessive(traineeName)} review?"
            : $"Ratify and issue {Count(staged, "STAR", "STARs")} to {traineeName}?";

    /// <summary>The Ratify dialog's confirm: "Ratify and issue 3 STARs", or "Ratify the review".</summary>
    public static string RatifyConfirm(int staged)
        => staged == 0 ? "Ratify the review" : $"Ratify and issue {Count(staged, "STAR", "STARs")}";

    /// <summary>One STAR Ratify issues: "PAED-010 at 4, issued 2026-10-05, expires 2026-10-25".</summary>
    public static string Issues(PendingEntrustmentDecisionDto pending)
    {
        ArgumentNullException.ThrowIfNull(pending);
        var line = $"{pending.EpaCode} at {pending.AuthorisedLevelLabel}, issued {Iso(pending.IssuedOn)}";
        return pending.ExpiresOn is DateOnly expires ? $"{line}, expires {Iso(expires)}" : line;
    }

    /// <summary>"PAED-002 and 11 others were not decided at this sitting." (E1); null when every line was staged.</summary>
    public static string? NotDecidedAtRatify(IReadOnlyList<string> codes)
    {
        ArgumentNullException.ThrowIfNull(codes);
        return codes.Count switch
        {
            0 => null,
            1 => $"{codes[0]} was not decided at this sitting.",
            2 => $"{codes[0]} and 1 other were not decided at this sitting.",
            _ => $"{codes[0]} and {codes.Count - 1} others were not decided at this sitting."
        };
    }

    /// <summary>The Ratify dialog's warning.</summary>
    public const string RatifyFinal = "Ratifying cannot be undone: nothing on this review can change afterwards.";

    /// <summary>"Close Sipho Ndlovu's formative check-in?"</summary>
    public static string CloseTitle(string? traineeName) => $"Close {Possessive(traineeName)} formative check-in?";

    /// <summary>The Close dialog's body.</summary>
    public const string CloseBody = "It records no decision and issues no STAR. Nothing on it can change once it is closed.";

    // ─── The notes card (round 3 items 13, 31) ───────────────────────────────────

    public const string NotesHeading = "Before the committee decides";
    public const string SamplingHeading = "Sampling concentration warnings";

    /// <summary>
    /// One EPA's sampling line: "PAED-010 · 1 rating · 1 assessor · 1 source. Fewer than three distinct assessors across
    /// this EPA's evidence.", "(no longer in use)" after a code not in force (13, T273).
    /// </summary>
    public static string SamplingLine(EpaSamplingConcentrationDto epa)
    {
        ArgumentNullException.ThrowIfNull(epa);
        var code = epa.EpaInForce ? epa.EpaCode : $"{epa.EpaCode} {EpaOptionLabel.NoLongerInUse}";
        var flags = new List<string>();
        if (epa.OneAssessorOverHalf)
        {
            flags.Add($"One assessor contributed {epa.DominantAssessorCount} of {epa.RatingCount} ratings (more than half).");
        }

        if (epa.SingleSource)
        {
            flags.Add("All ratings came from a single source of evidence.");
        }

        if (epa.FewerThanThreeAssessors)
        {
            flags.Add("Fewer than three distinct assessors across this EPA's evidence.");
        }

        var counts = $"{code} · {Count(epa.RatingCount, "rating", "ratings")} · " +
                     $"{Count(epa.DistinctAssessorCount, "assessor", "assessors")} · " +
                     $"{Count(epa.DistinctSourceCount, "source", "sources")}.";
        return flags.Count == 0 ? counts : $"{counts} {string.Join(" ", flags)}";
    }

    /// <summary>The records the figures leave out, said under them.</summary>
    public static string NotCounted(int unattributed)
        => $"Not counted: {Count(unattributed, "record", "records")} in the review window with no rating by a named " +
           "assessor (the form names nobody for the rating, multi-source feedback for one, or the rating or the assessor " +
           "was left blank where the form allows it). The figures above count only ratings a named assessor gave.";

    // ─── The agenda (round 3 items 17, 24, 25, 27, 30; E1, E9) ───────────────────

    public const string AgendaHeading = "Agenda";

    /// <summary>"15 EPAs for 2026 S2 · 3 staged" while the review is in progress, else "15 EPAs for 2026 S2".</summary>
    public static string AgendaCaption(int lines, string periodLabel, int? staged)
    {
        var caption = $"{Count(lines, "EPA", "EPAs")} for {periodLabel}";
        return staged is int count ? $"{caption} · {(count == 0 ? "none staged" : $"{count} staged")}" : caption;
    }

    /// <summary>Said above a decided review's agenda: the staged decisions are fixed with the decision (T165).</summary>
    public const string FixedWhenRecorded = "Fixed when the committee's decision was recorded: ratifying issues exactly these.";

    /// <summary>Said beside it when a staged decision no longer fits the trainee's curriculum (T165).</summary>
    public const string NoLongerFitsCanGo = "Only a decision that no longer fits the trainee's curriculum can still be removed.";

    public const string IssuedWhatWasStaged =
        "Nothing is pending: ratifying the review issued what was staged as STARs, and the agenda names each one.";

    public const string NothingWasStaged =
        "Nothing was staged when the review was ratified, so ratifying it issued no STAR.";

    public const string WithdrawnNothingPending =
        "Nothing is pending: the review was withdrawn, so it issues no STAR, and anything staged at it was removed.";

    /// <summary>"3 items", "1 item": the Evidence column.</summary>
    public static string Items(int count) => Count(count, "item", "items");

    /// <summary>The Action column's Stage, named by its line (T239): "Stage a decision on PAED-001", "(draft kept)" when one is.</summary>
    public static string StageName(string epaCode, bool draftKept)
        => draftKept ? $"Stage a decision on {epaCode} (draft kept)" : $"Stage a decision on {epaCode}";

    public const string StageLabel = "Stage";
    public const string RemoveLabel = "Remove…";

    /// <summary>"Draft kept" beside a closed line's Stage (E9).</summary>
    public const string DraftKept = "Draft kept";

    /// <summary>A chair's line (25): staged on an EPA the agenda did not plan.</summary>
    public const string AddedByTheChair = "Added by the chair.";

    /// <summary>
    /// A staged line's decision in round 3 item 30's words: "Level 5 · issued 2026-10-05 · no expiry." or "… · expires
    /// 2026-10-25."
    /// </summary>
    public static string StagedDecision(PendingEntrustmentDecisionDto pending)
    {
        ArgumentNullException.ThrowIfNull(pending);
        var expiry = pending.ExpiresOn is DateOnly expires ? $"expires {Iso(expires)}" : "no expiry";
        return $"Level {pending.AuthorisedLevelLabel} · issued {Iso(pending.IssuedOn)} · {expiry}.";
    }

    /// <summary>"Rests on 2 items of the snapshot: Mini-CEX (Paediatrics) #12; Case-Based Discussion (Paediatrics) #13."</summary>
    public static string RestsOn(IReadOnlyList<string> labels)
    {
        ArgumentNullException.ThrowIfNull(labels);
        return $"Rests on {Count(labels.Count, "item", "items")} of the snapshot: {string.Join("; ", labels)}.";
    }

    /// <summary>
    /// A staged decision that names no evidence (T213): only a row written some other way can. In progress the chair can
    /// remove it and stage it again; once recorded it is fixed (D46).
    /// </summary>
    public static string NamesNoEvidence(bool inProgress, bool chairMayAct)
        => "Names no evidence, so the review cannot be ratified with it. " + (inProgress
            ? chairMayAct
                ? "Remove it and stage it again, naming the evidence."
                : "The chair can remove it and stage it again, naming the evidence."
            : StagedEvidence.UngroundedOnceRecorded);

    /// <summary>"No longer fits: …" (T167).</summary>
    public static string NoLongerFits(string reason) => $"No longer fits: {reason}";

    /// <summary>The foot form's heading (round 3, "Stage a decision on an EPA not on this agenda").</summary>
    public const string FootHeading = "Stage a decision on an EPA not on this agenda";

    /// <summary>An agenda with no line.</summary>
    public static string AgendaEmpty(CommitteeReviewState state) => state switch
    {
        CommitteeReviewState.Scheduled =>
            "Nothing is on the agenda yet. Starting the review adds every EPA routed to this panel that is due in its period " +
            "and not already decided in its window.",
        CommitteeReviewState.Withdrawn => "Nothing was on this review's agenda when it was withdrawn.",
        _ => "Nothing is on this review's agenda. A decision can still be staged on any EPA of the trainee's curriculum that " +
             "routes to this panel, and staging it adds it."
    };

    public const string AgendaNotRead = "The agenda could not be read. The rest of the review stands.";
    public const string StagedNotRead = "The staged decisions could not be read. The agenda shows the ones last read.";

    // ─── The line's form (round 3 items 8, 11, 16, 26, 30; C2, C3) ───────────────

    /// <summary>"Stage a decision on PAED-001".</summary>
    public static string StageHeading(string epaCode) => $"Stage a decision on {epaCode}";

    /// <summary>A refused stage, at the top of its form: "Not staged. An expiry date must be after the issue date."</summary>
    public static string NotStaged(string refusal) => $"Not staged. {refusal}";

    /// <summary>The foot form's refusal with no EPA chosen, marked on EPA.</summary>
    public const string ChooseAnEpa = "Choose the EPA the decision is on.";

    /// <summary>A stage with no rung chosen, marked on Authorised level.</summary>
    public const string ChooseALevel = "Choose the authorised level.";

    /// <summary>A stage with no rationale, marked on Rationale.</summary>
    public const string GiveDecisionRationale = "Give the decision's rationale.";

    /// <summary>The refusal a too-early expiry gets, marked on Expires on (4.18).</summary>
    public const string ExpiryRefusal = "An expiry date must be after the issue date.";

    /// <summary>A part of the standing that failed, said once in its place (round 3 item 11).</summary>
    public const string StandingFailed = "Standing could not be loaded.";

    /// <summary>
    /// What follows "The multi-source feedback coverage could not be loaded:" in the MSF card when its read failed: fixed
    /// words, never the exception's (T329).
    /// </summary>
    public const string MsfCoverageFailed = "the read failed. The rest of the review stands.";

    /// <summary>The Action column's words on a line that offers nothing beside one that does (T211).</summary>
    public const string NothingToDo = "Nothing to do at this sitting";

    /// <summary>The same, once the decision is recorded and the staged decisions are fixed with it (T165).</summary>
    public const string FixedWithTheDecision = "Fixed with the recorded decision";

    /// <summary>The standing's first part: "Year 4 target 5 · no STAR yet", "Year 4 target 5 · STAR at 4".</summary>
    public static string TargetAndStar(int targetYear, string targetLabel, string? starLevel)
        => $"Year {targetYear} target {targetLabel} · {(starLevel is null ? "no STAR yet" : $"STAR at {starLevel}")}";

    /// <summary>"3 ratings in the review window".</summary>
    public static string RatingsInWindow(int count) => $"{Count(count, "rating", "ratings")} in the review window";

    /// <summary>"MSF: Covered" or "MSF: None released yet" (26).</summary>
    public static string Msf(bool covered) => covered ? "MSF: Covered" : "MSF: None released yet";

    /// <summary>"Rating chart for PAED-001" (C2): moves the focus to the chart's heading.</summary>
    public static string ChartJump(string epaCode) => $"Rating chart for {epaCode}";

    /// <summary>"Back to PAED-001's line" beside the chart, while that line is open (C2).</summary>
    public static string ChartBack(string epaCode) => $"Back to {epaCode}'s line";

    public const string EvidenceLegend = "Evidence it rests on";

    /// <summary>"Show the other 6 snapshot lines" (20): a button with aria-expanded.</summary>
    public static string ShowOthers(int count) => $"Show the other {Count(count, "snapshot line", "snapshot lines")}";

    /// <summary>The picker's line: "Mini-CEX (Paediatrics) #12 · Thandi Zulu".</summary>
    public static string PickLabel(CommitteeEvidenceDto line)
    {
        ArgumentNullException.ThrowIfNull(line);
        return string.IsNullOrWhiteSpace(line.AssessorName) ? line.SourceLabel : $"{line.SourceLabel} · {line.AssessorName}";
    }

    /// <summary>The picker's detail: "Rated 5 · 2026-09-23 · Completed", "Not recorded" for a rating nobody gave.</summary>
    public static string PickDetail(CommitteeEvidenceDto line)
    {
        ArgumentNullException.ThrowIfNull(line);

        // A campaign's report rates nobody: its state alone, as the snapshot lists it.
        if (line.SourceType != CommitteeEvidenceSourceType.Activity)
        {
            return line.SourceStateLabel ?? string.Empty;
        }

        var parts = new List<string>
        {
            string.IsNullOrWhiteSpace(line.RatingLabel) ? EvidenceSnapshotGroups.RatingText(line) : $"Rated {line.RatingLabel}"
        };
        if (line.ObservedOn is DateOnly observedOn)
        {
            parts.Add(line.ObservedOnDeclared == false ? $"{Iso(observedOn)} (filed)" : Iso(observedOn));
        }

        if (!string.IsNullOrWhiteSpace(line.SourceStateLabel))
        {
            parts.Add(line.SourceStateLabel);
        }

        return string.Join(" · ", parts);
    }

    /// <summary>Read's name: "Read Mini-CEX (Paediatrics) #12" (label in name).</summary>
    public static string ReadName(CommitteeEvidenceDto line) => $"Read {line.SourceLabel}";

    public const string ReadLabel = "Read";
    public const string LevelLabel = "Authorised level";
    public const string ChooseRung = "Choose a rung…";
    public const string ChooseEpaFirst = "Choose an EPA first…";
    public const string IssuedOnLabel = "Issued on";
    public const string ExpiresOnLabel = "Expires on (optional)";
    public const string RationaleLabel = "Rationale";
    public const string EpaLabel = "EPA";
    public const string ChooseEpa = "Choose an EPA…";
    public const string EpaHelp = "The EPAs on the trainee's curriculum.";
    public const string StageSubmit = "Stage pending decision";
    public const string Cancel = "Cancel";

    /// <summary>The level's help: the ladder the EPA is assessed on.</summary>
    public static string LevelHelp(string? scaleName, bool epaChosen)
        => !epaChosen ? "The rungs of the chosen EPA's ladder."
            : scaleName is null ? "This EPA is on no ladder, so any scale's rung may be chosen."
            : $"Rungs of {scaleName}, the ladder this EPA is assessed on.";

    /// <summary>A staged decision's hints, said beside the ticks and in its line, never enforced (D38, O1).</summary>
    public const string OnlyOneItem =
        "Only one item is named. The College's rule is that an entrustment decision is never taken from a single form.";

    /// <summary>"None of the named items is about PAED-002."</summary>
    public static string NoneAbout(string epaCode) => $"None of the named items is about {epaCode}.";

    public const string NoFinishedItem =
        "None of the named items was finished when the review started (each shows the state it was in), so the decision " +
        "would rest on no completed assessment.";

    /// <summary>The hints for a set of named lines on <paramref name="epaId" />.</summary>
    public static IReadOnlyList<string> Hints(IReadOnlyList<CommitteeEvidenceDto> named, int epaId, string? epaCode)
    {
        ArgumentNullException.ThrowIfNull(named);
        if (named.Count == 0)
        {
            return [];
        }

        var hints = new List<string>();
        if (named.Count == 1)
        {
            hints.Add(OnlyOneItem);
        }

        if (epaId > 0 && epaCode is not null && named.All(line => line.EpaId != epaId))
        {
            hints.Add(NoneAbout(epaCode));
        }

        if (named.All(line => line.SourceFinished == false))
        {
            hints.Add(NoFinishedItem);
        }

        return hints;
    }

    public const string NoLineCanGround =
        "The evidence snapshot holds no line a decision can rest on, so no entrustment decision can be staged at this review.";

    public const string NoCurriculumEpa = "This trainee's curriculum lists no EPA, so no entrustment decision can be staged.";

    // ─── The reader (round 3 item 8; C3) ─────────────────────────────────────────

    /// <summary>"Mini-CEX (Paediatrics) #12 · PAED-001".</summary>
    public static string ReaderHeading(CommitteeEvidenceDto line)
        => string.IsNullOrWhiteSpace(line.EpaCode) ? line.SourceLabel : $"{line.SourceLabel} · {line.EpaCode}";

    /// <summary>"Back to PAED-001": restores the form as typed.</summary>
    public static string ReaderBack(string epaCode) => $"Back to {epaCode}";

    /// <summary>"Now Completed. Frozen at Start as Awaiting review." (8)</summary>
    public static string MovedSinceStart(string now, string frozen) => $"Now {now}. Frozen at Start as {frozen}.";

    public const string ReaderLoading = "Reading the activity…";
    public const string ReaderFailed = "The activity could not be read. Nothing on the review has changed.";
    public const string ReaderUnavailable = "This activity does not exist, or it cannot be opened from this account.";

    // ─── The decision (round 3 items 7, 16, 21, 22; E1, E4; T324) ────────────────

    public const string DecisionHeading = "Decision";
    public const string CategoryLabel = "Category";
    public const string SelectCategory = "Select a category…";
    public const string UndecidedLabel = "About the EPAs not decided (optional)";
    public const string UndecidedHelp = "The registrar sees this once the review is ratified.";
    public const string ConditionsLabel = "Conditions";
    public const string PresentLegend = "Present";
    public const string RecordLabel = "Record decision…";

    /// <summary>The Present fieldset's help.</summary>
    public static string PresentHelp => $"Tick every panel member at this sitting. {CommitteeReview.QuorumRule}";

    /// <summary>
    /// The quorum line, from the ticks (7): "Only the chair is ticked. …", "4 present: the chair and 3 others. The quorum is
    /// met."
    /// </summary>
    public static string Quorum(int present)
        => present < CommitteeReview.Quorum
            ? $"Only the chair is ticked. {CommitteeReview.QuorumRule}"
            : $"{present} present: the chair and {Count(present - 1, "other", "others")}. The quorum is met.";

    /// <summary>
    /// The hint beside an available Record (round-3-check): "Recording fixes the 3 staged decisions, and records the 12
    /// EPAs not staged as not decided."
    /// </summary>
    public static string RecordHint(int staged, int notStaged)
    {
        var fixes = staged == 1 ? "the 1 staged decision" : $"the {staged} staged decisions";
        var records = notStaged == 1 ? "the 1 EPA not staged" : $"the {notStaged} EPAs not staged";
        return (staged, notStaged) switch
        {
            (0, 0) => "Recording records the committee's decision.",
            (0, _) => $"Recording records {records} as not decided.",
            (_, 0) => $"Recording fixes {fixes}.",
            _ => $"Recording fixes {fixes}, and records {records} as not decided."
        };
    }

    /// <summary>A refusal before any dialog (T324): "Not recorded. Give the committee's rationale."</summary>
    public static string NotRecorded(IEnumerable<string> refusals) => $"Not recorded. {string.Join(" ", refusals)}";

    /// <summary>A server refusal of Record, at the form's top.</summary>
    public static string NotRecorded(string refusal) => $"Not recorded. {refusal}";

    public const string ChooseCategory = "Choose a progression category.";
    /// <summary>Record with no rationale: the Record validator's own sentence (T361, review R4).</summary>
    public const string GiveRationale = CommitteeDecisionWording.GiveCommitteeRationale;

    /// <summary>"About the EPAs not decided: To be decided at the final review." on the recorded decision.</summary>
    public static string AboutNotDecided(string note) => $"About the EPAs not decided: {note.Trim()}";

    /// <summary>"Conditions: At least two Mini-CEX before the next sitting."</summary>
    public static string Conditions(string conditions) => $"Conditions: {conditions.Trim()}";

    public const string NobodyPresent = "Nobody was recorded as present for this decision.";
    public const string ReplacedOnAppeal = "Replaced on appeal by the decision above.";
    public const string NoDecisionYet = "No decision has been recorded yet.";
    public const string NoDecisionWithdrawn = "No decision was recorded before the review was withdrawn.";

    /// <summary>
    /// What an entrustment-only review decides, said to the panel (§ Entrustment-only reviews): its staged decisions are
    /// "staged above" while the decision form sits under the agenda, "on the agenda below" once the recorded decision rises
    /// above it; then as lane A words it.
    /// </summary>
    public static string EntrustmentOnlyNote(CommitteeReviewState state) => state switch
    {
        CommitteeReviewState.InProgress or CommitteeReviewState.Scheduled =>
            "This review decides entrustment only. Its decision is the entrustment decisions staged above, and it records no " +
            "progression category: the trainee's progression is decided at a progression review.",
        CommitteeReviewState.Decided =>
            "This review decides entrustment only. Its decision is the entrustment decisions staged on the agenda below, and " +
            "it records no progression category: the trainee's progression is decided at a progression review.",
        _ => CommitteeDecisionWording.EntrustmentOnlyPanelNote(state)
    };

    // ─── The snapshot (round 3 items 23, 30; Q6) ─────────────────────────────────

    public const string SnapshotHeading = "Evidence snapshot";

    /// <summary>"Frozen when the review started, 2026-10-05 · 9 items".</summary>
    public static string FrozenOn(string startedOn, int items) => $"Frozen when the review started, {startedOn} · {Items(items)}";

    public const string NothingFrozenYet = "No evidence has been frozen yet. Starting the review captures the current bundle.";
    public const string NothingFrozenWithdrawn = "No evidence was frozen: the review was withdrawn before it started.";
    public const string WindowHeldNothing = "The review window held no evidence when this review started.";
    public const string AcrossEpasHeading = "Not about a single EPA";
    public const string FrozenBeforeHeading = "Frozen before lines named their EPA";

    // ─── The other sections ──────────────────────────────────────────────────────

    public const string StandingHeading = "Entrustment against Annexure A";
    public const string MsfHeading = "Multi-source feedback by EPA";
    public const string TrajectoryHeading = "Rating trajectory by EPA";
    public const string NoTrajectory = "No rating by a named assessor to chart in this review's window.";
    public const string TrajectoryFailed = "The rating trajectories could not be loaded. The rest of the review stands.";
    public const string AppealsHeading = "Appeals";
    public const string NoAppeal = "No appeal has been lodged against this review.";

    // ─── Helpers ─────────────────────────────────────────────────────────────────

    /// <summary>"1 item", "3 items".</summary>
    public static string Count(int count, string one, string many)
        => $"{count.ToString(CultureInfo.InvariantCulture)} {(count == 1 ? one : many)}";

    /// <summary>ISO, never the server's culture (T325).</summary>
    public static string Iso(DateOnly day) => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string Possessive(string? name)
        => string.IsNullOrWhiteSpace(name) ? "the trainee's" : $"{name}'s";
}
