namespace Wombat.Domain.CommitteeDecisions;

/// <summary>
/// Why a committee review is held. Independent of <see cref="CommitteeReview.IsFormative" />, which says whether the
/// review issues a binding decision at all.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="AnnualProgression" /> and <see cref="PreGraduation" /> are the same decision to the engine, a progression
/// outcome with its category (F-4B-1 d): the type records why the review is held, so reports and the committee can tell a
/// routine annual review from the final one.
/// </para>
/// <para>
/// <see cref="EntrustmentOnly" /> is the one the engine branches on (T131 slice 5, O4). Its decision is the entrustment
/// decisions (STARs) staged at it: the decision it records carries no progression category, the review ratifies without
/// one, and an appeal it remits replaces it with another that carries none. A progression review's decision always
/// carries one. Which types a review may take is <see cref="CommitteeReviewTypes" />'s rule.
/// </para>
/// </remarks>
public enum CommitteeReviewType
{
    AnnualProgression = 1,
    PreGraduation = 2,

    /// <summary>
    /// Decides entrustment only, with no progression category (T131 slice 5). Always the type of a review before a panel
    /// sitting as a College decision body (the neonatal CCC decides EPAs 4 and 5, and nobody's progression); a general
    /// panel's semester-1 sitting may be one too, when it decides no progression. The semester-2 sitting always does.
    /// </summary>
    EntrustmentOnly = 3
}
