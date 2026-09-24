using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.MultiSourceFeedback;

namespace Wombat.Web.Components.Shared;

/// <summary>
/// The words for MSF coverage (T168), shared by the trainee's progress page and the committee's panel so the two say it
/// the same way. Every sentence is the College's D9: an EPA is covered when a released campaign COVERING it closed in the
/// semester, never "a campaign about" it. Nothing here is a target or a shortfall; that waits on Annexure B's cadence.
/// Built as whole strings, because Razor drops a space standing alone before an expression.
/// </summary>
public static class MsfCoverageText
{
    /// <summary>
    /// One EPA's line on a progress card, newest semester first: "MSF in Semester 2, 2026: no released campaign covering
    /// this EPA has closed yet. MSF in Semester 1, 2026: covered by a released campaign that closed on 10 March 2026."
    /// Null when the coverage does not list the EPA.
    /// </summary>
    public static string? CardLine(MsfCoverageDto coverage, int epaId)
    {
        ArgumentNullException.ThrowIfNull(coverage);

        var epa = coverage.Epas.FirstOrDefault(entry => entry.EpaId == epaId);
        if (epa is null)
        {
            return null;
        }

        return string.Join(" ", coverage.Periods
            .Reverse()
            .Select(period => $"MSF in {period.Name}: {PeriodPhrase(period, epa.For(period.Year, period.Semester))}."));
    }

    /// <summary>
    /// "covered by a released campaign that closed on 10 March 2026", "covered by 2 released campaigns, the latest closed
    /// on 10 March 2026", or, uncovered, "no released campaign covering this EPA has closed yet" while the semester runs.
    /// </summary>
    /// <remarks>
    /// An uncovered semester that has ended is never worded as final. A campaign is bucketed by the day it closed, and one
    /// that closed on 20 June can be released on 10 July: until then semester 1 has no released campaign, and after it
    /// semester 1 is covered. So the ended case says none "has been released", which the release can still change. The
    /// unreleased campaign itself is not named: before release the trainee has not seen it and it may be withdrawn.
    /// </remarks>
    public static string PeriodPhrase(MsfCoveragePeriodDto period, MsfEpaPeriodCoverageDto? coverage)
    {
        ArgumentNullException.ThrowIfNull(period);

        if (coverage?.Latest is not { } latest)
        {
            return period.HasEnded
                ? "no campaign covering this EPA that closed in the semester has been released"
                : "no released campaign covering this EPA has closed yet";
        }

        return coverage.Campaigns.Count == 1
            ? $"covered by a released campaign that closed on {QuotaText.LongDate(latest.ClosedOn)}"
            : $"covered by {coverage.Campaigns.Count} released campaigns, the latest closed on {QuotaText.LongDate(latest.ClosedOn)}";
    }

    /// <summary>
    /// The semester's count for the progress page's "This period" card: "3 of 15 EPAs covered by a released campaign that
    /// closed this semester". Of the newest semester, which is the one containing today.
    /// </summary>
    public static string ThisSemesterLine(MsfCoverageDto coverage)
    {
        ArgumentNullException.ThrowIfNull(coverage);

        var current = coverage.Periods[^1];
        return $"{current.EpasCovered} of {coverage.Epas.Count} EPA{(coverage.Epas.Count == 1 ? string.Empty : "s")} " +
               "covered by a released campaign that closed this semester. MSF is tracked on its own and counts towards no target.";
    }

    /// <summary>
    /// A cell of the committee's table: "Annual MSF #4, closed 10 Mar 2026", or with more than one, "2 campaigns; the
    /// latest Annual MSF #6, closed 10 Mar 2026".
    /// </summary>
    public static string CampaignNote(MsfEpaPeriodCoverageDto coverage)
    {
        ArgumentNullException.ThrowIfNull(coverage);

        var latest = coverage.Latest ?? throw new ArgumentException("The EPA was not covered in this semester.", nameof(coverage));
        var campaign = $"{latest.TemplateName} #{latest.CampaignId}, closed {QuotaText.ShortDate(latest.ClosedOn)}";
        return coverage.Campaigns.Count == 1 ? campaign : $"{coverage.Campaigns.Count} campaigns; the latest {campaign}";
    }
}
