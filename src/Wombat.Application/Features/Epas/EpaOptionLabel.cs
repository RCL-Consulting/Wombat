namespace Wombat.Application.Features.Epas;

/// <summary>
/// How an activity's EPA option reads (T196, D48).
/// </summary>
/// <remarks>
/// The pickers offer only EPAs in force, so an EPA that is not in force reaches an option list only as the value an
/// activity already stores. It stays on the record, and says why it cannot be chosen again: a deactivated EPA pauses
/// credit, so an activity filed against it counts once the EPA is reactivated, and until then the reader should know.
/// My activities and the Inbox print <see cref="NoLongerInUse" /> after the same EPA, by the same flag
/// (<c>ActivitySummaryDto.EpaInForce</c>, T231), and so do the rating trajectory headings and the entrustment decision
/// lists (<c>EpaTrajectoryDto.EpaInForce</c>, <c>EntrustmentDecisionDto.EpaInForce</c>, T255).
/// </remarks>
public static class EpaOptionLabel
{
    public const string NoLongerInUse = "(no longer in use)";

    /// <summary>"PAED-003 — Resuscitating a child", followed by <see cref="NoLongerInUse" /> when the EPA is not in force.</summary>
    public static string For(string code, string title, bool inForce)
        => inForce ? $"{code} — {title}" : $"{code} — {title} {NoLongerInUse}";
}
