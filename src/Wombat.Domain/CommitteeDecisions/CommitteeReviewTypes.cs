namespace Wombat.Domain.CommitteeDecisions;

/// <summary>
/// Which types a committee review may take, and what each decides (T131 slice 5). The scheduling handler refuses by it and
/// the scheduling form offers by it, so the form cannot offer a type the handler refuses.
/// </summary>
/// <remarks>
/// <para>
/// A panel sitting as a College decision body (<see cref="DecisionPanel.DecisionBodyKey" />: the neonatal CCC) decides
/// only the entrustment decisions routed to it, so every review before it is <see cref="CommitteeReviewType.EntrustmentOnly" />
/// (O4). The College gives such a body no progression outcome to write.
/// </para>
/// <para>
/// A general panel's semester-2 sitting is the year's last, where the annual EPAs close (T131, Decision 5), and it decides
/// the trainee's progression for the year, so it is an annual progression or a pre-graduation review and never
/// entrustment-only. Its semester-1 sitting may decide progression as well, or entrustment only: the adopted design's
/// "a general semester-1 sitting may also be" is read as the whole list of where a general panel sits entrustment-only.
/// That reading, the refusal at semester 2, awaits the operator's confirmation.
/// </para>
/// <para>
/// Judged when the review is scheduled, from the panel as it is then, and the review keeps its type: recording, ratifying
/// and remitting branch on the review's own type, never on its panel's tag. So the tag cannot change under a review that
/// is still open (<c>SetDecisionPanelBodyCommand</c> refuses it while one is scheduled, in progress or awaiting
/// ratification): a general panel tagged as the neonatal CCC would otherwise hold a progression review whose agenda the
/// next Start fills with the committee's EPAs, and a neonatal panel made general an entrustment-only review at a
/// semester-2 sitting.
/// </para>
/// </remarks>
public static class CommitteeReviewTypes
{
    private static readonly CommitteeReviewType[] BeforeADecisionBody = [CommitteeReviewType.EntrustmentOnly];

    private static readonly CommitteeReviewType[] GeneralSemester1 =
        [CommitteeReviewType.AnnualProgression, CommitteeReviewType.PreGraduation, CommitteeReviewType.EntrustmentOnly];

    private static readonly CommitteeReviewType[] GeneralSemester2 =
        [CommitteeReviewType.AnnualProgression, CommitteeReviewType.PreGraduation];

    /// <summary>The refusal for a type that is none of the enum's values.</summary>
    public const string UnknownType = "Choose a review type from the list.";

    /// <summary>The refusal for an entrustment-only review of a general panel's semester-2 sitting.</summary>
    public const string EntrustmentOnlyNotAtSemester2 =
        "A general panel's semester-2 sitting decides the trainee's progression for the year, so it is an annual " +
        "progression or a pre-graduation review. An entrustment-only review sits in semester 1, or before a College " +
        "committee such as the neonatal CCC.";

    /// <summary>
    /// Whether a review of this type records a progression category with its decision: every type but
    /// <see cref="CommitteeReviewType.EntrustmentOnly" />, whose decision is the STARs staged at it.
    /// </summary>
    public static bool DecidesProgression(CommitteeReviewType type) => type != CommitteeReviewType.EntrustmentOnly;

    /// <summary>
    /// The types a review may take before this panel for this period, in the order the scheduling form offers them: the
    /// first is the one a review takes when none is asked for.
    /// </summary>
    /// <param name="sitsAsDecisionBody">Whether the panel sits as a College decision body.</param>
    /// <param name="semester">The semester, 1 or 2, of the period the review sits for.</param>
    public static IReadOnlyList<CommitteeReviewType> Allowed(bool sitsAsDecisionBody, int semester)
        => sitsAsDecisionBody
            ? BeforeADecisionBody
            : semester == 1 ? GeneralSemester1 : GeneralSemester2;

    /// <summary>The type a review takes before this panel for this period when none is asked for.</summary>
    public static CommitteeReviewType DefaultFor(bool sitsAsDecisionBody, int semester) => Allowed(sitsAsDecisionBody, semester)[0];

    /// <summary>
    /// Why a review of <paramref name="type" /> cannot sit before this panel for this period, or null when it can.
    /// </summary>
    /// <param name="type">The type asked for.</param>
    /// <param name="decisionBodyName">
    /// The name of the College committee the panel sits as, or null for a general panel.
    /// </param>
    /// <param name="semester">The semester, 1 or 2, of the period the review sits for.</param>
    public static string? Refusal(CommitteeReviewType type, string? decisionBodyName, int semester)
    {
        if (!Enum.IsDefined(type))
        {
            return UnknownType;
        }

        var sitsAsDecisionBody = decisionBodyName is not null;
        if (Allowed(sitsAsDecisionBody, semester).Contains(type))
        {
            return null;
        }

        return sitsAsDecisionBody
            ? $"The {decisionBodyName} decides entrustment only: a review before it is an entrustment-only review, which " +
              "records no progression category. The trainee's progression is decided by their general committee."
            : EntrustmentOnlyNotAtSemester2;
    }
}
