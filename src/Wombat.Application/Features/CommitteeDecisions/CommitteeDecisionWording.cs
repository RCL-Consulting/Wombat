using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// How a committee review's type and its decision's outcome are said, wherever they are shown: the review pages, the
/// trainee's own reviews, and the portfolio PDF (T131 slice 5). One wording, so an entrustment-only review's decision reads
/// the same everywhere and never as a missing outcome.
/// </summary>
public static class CommitteeDecisionWording
{
    /// <summary>The outcome of an entrustment-only review's decision, which records no progression category.</summary>
    public const string EntrustmentOnlyOutcome = "Entrustment decisions only";

    /// <summary>The outcome of a review whose decision is not yet recorded.</summary>
    public const string PendingOutcome = "Pending";

    /// <summary>The outcome of a formative review, which records no binding decision.</summary>
    public const string FormativeOutcome = "No binding decision";

    /// <summary>What an entrustment-only review decides, said to the panel until it is ratified.</summary>
    public const string EntrustmentOnlyForThePanel =
        "This review decides entrustment only. Its decision is the entrustment decisions staged below, and it records no " +
        "progression category: the trainee's progression is decided at a progression review.";

    /// <summary>
    /// What an entrustment-only review decided, said to the panel once it is ratified, when the staged decisions have become
    /// the STARs its agenda names.
    /// </summary>
    public const string EntrustmentOnlyIssuedForThePanel =
        "This review decided entrustment only. Its decision is the entrustment decisions it issued when it was ratified, " +
        "each named on its agenda, and it records no progression category.";

    /// <summary>
    /// Said where a remitted appeal's replacement is taken on an entrustment-only review: the replacement records the appeal
    /// body's reasoning and no category, and changes no STAR the review issued (T131 slice 5).
    /// </summary>
    public const string EntrustmentOnlyRemitForThePanel =
        "This review decides entrustment only, so the replacement decision records no progression category, and it " +
        "changes no entrustment decision: the STARs this review issued stand. To change one, revoke it and re-decide the " +
        "EPA at a new review.";

    /// <summary>What an entrustment-only review decided, said to the trainee.</summary>
    public const string EntrustmentOnlyForTheTrainee =
        "This review decided entrustment only, so it records no progression outcome.";

    /// <summary>
    /// Said to the trainee after <see cref="EntrustmentOnlyForTheTrainee" /> when an appeal remitted the review's decision:
    /// the replacement is the appeal body's reasoning, and no STAR changed with it (T131 slice 5).
    /// </summary>
    public const string EntrustmentOnlyRemittedForTheTrainee =
        "The appeal was remitted: this decision is the appeal body's, and the entrustment decisions this review " +
        "issued stand unless one is revoked.";

    /// <summary>Said to the trainee after <see cref="EntrustmentOnlyForTheTrainee" /> when the review's agenda is shown.</summary>
    public const string EntrustmentOnlySeeTheAgenda = "What it decided on each EPA is on its agenda below.";

    /// <summary>
    /// What an entrustment-only review decides, said to the panel in <paramref name="state" />: the decisions staged below
    /// until it is ratified, the decisions it issued after.
    /// </summary>
    public static string EntrustmentOnlyPanelNote(CommitteeReviewState state)
        => state is CommitteeReviewState.Ratified or CommitteeReviewState.UnderAppeal or CommitteeReviewState.Final
            ? EntrustmentOnlyIssuedForThePanel
            : EntrustmentOnlyForThePanel;

    /// <summary>The review's type as a heading or a detail line says it: "Entrustment-only review".</summary>
    public static string ReviewTypeLabel(CommitteeReviewType type) => type switch
    {
        CommitteeReviewType.AnnualProgression => "Annual progression review",
        CommitteeReviewType.PreGraduation => "Pre-graduation review",
        CommitteeReviewType.EntrustmentOnly => "Entrustment-only review",
        _ => type.ToString()
    };

    /// <summary>The review's type as a list column says it: "Entrustment only".</summary>
    public static string ReviewTypeShortLabel(CommitteeReviewType type) => type switch
    {
        CommitteeReviewType.AnnualProgression => "Annual progression",
        CommitteeReviewType.PreGraduation => "Pre-graduation",
        CommitteeReviewType.EntrustmentOnly => "Entrustment only",
        _ => type.ToString()
    };

    /// <summary>A progression category in words: "Satisfactory with Observations".</summary>
    public static string CategoryLabel(CommitteeDecisionCategory category) => category switch
    {
        CommitteeDecisionCategory.SatisfactoryProgress => "Satisfactory Progress",
        CommitteeDecisionCategory.SatisfactoryWithObservations => "Satisfactory with Observations",
        CommitteeDecisionCategory.InadequateProgressAdditionalTraining => "Inadequate Progress — Additional Training",
        CommitteeDecisionCategory.InadequateProgressRepeat => "Inadequate Progress — Repeat",
        CommitteeDecisionCategory.ReleaseFromTraining => "Release from Training",
        CommitteeDecisionCategory.OutcomeDeferred => "Outcome Deferred",
        CommitteeDecisionCategory.Graduate => "Graduate (programme complete)",
        _ => category.ToString()
    };

    /// <summary>
    /// A recorded decision's outcome: its progression category, or, on an entrustment-only review's decision, which
    /// records none, <see cref="EntrustmentOnlyOutcome" />.
    /// </summary>
    public static string OutcomeLabel(CommitteeDecisionCategory? category)
        => category is { } value ? CategoryLabel(value) : EntrustmentOnlyOutcome;

    /// <summary>
    /// A listed review's outcome: no binding decision for a formative review, pending until the decision is recorded, then
    /// the decision's outcome (<see cref="OutcomeLabel" />).
    /// </summary>
    public static string OutcomeLabel(CommitteeReviewListItemDto review)
    {
        ArgumentNullException.ThrowIfNull(review);

        if (review.IsFormative)
        {
            return FormativeOutcome;
        }

        if (!review.HasDecision)
        {
            return PendingOutcome;
        }

        return review.DecidesProgression
            ? review.CurrentDecisionCategory is { } category ? CategoryLabel(category) : PendingOutcome
            : EntrustmentOnlyOutcome;
    }
}
