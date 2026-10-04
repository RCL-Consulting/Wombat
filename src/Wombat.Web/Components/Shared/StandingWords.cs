using Wombat.Application.Features.EntrustmentDecisions;

namespace Wombat.Web.Components.Shared;

/// <summary>
/// The standing against Annexure A in words, said once for Home's My authorisations card and the standing panel ("the
/// standing summary in the panel's words", T355, R1).
/// </summary>
public static class StandingWords
{
    /// <summary>
    /// "2 at or above · 1 below · 12 with no decision, of 15 EPAs", with "· 1 not comparable" when a decision is on another
    /// ladder: moved word for word out of <c>EntrustmentStandingPanel</c>'s <c>YearLine</c> (T355, R1).
    /// </summary>
    public static string YearLine(EntrustmentStandingDto standing)
    {
        ArgumentNullException.ThrowIfNull(standing);

        var parts = new List<string>
        {
            $"{standing.AtYearTarget} at or above",
            $"{standing.BelowYearTarget} below",
            $"{standing.WithoutDecision} with no decision"
        };

        if (standing.NotComparable > 0)
        {
            parts.Add($"{standing.NotComparable} not comparable");
        }

        return $"{string.Join(" · ", parts)}, of {standing.Epas.Count} EPAs";
    }
}
