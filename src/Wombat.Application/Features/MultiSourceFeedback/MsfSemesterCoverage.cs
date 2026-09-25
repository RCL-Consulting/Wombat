using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Curricula;
using Wombat.Domain.Curricula;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>
/// The coverage grid's rule (T168): which EPAs of a trainee's list a released multi-source feedback campaign about them
/// covered, and in which semester. The one implementation the trainee's grid (<see cref="GetMsfCoverageForTraineeQuery" />)
/// and the programme's counts (<see cref="GetMsfProgrammeCoverageQuery" />, T210) are both read from, so a programme's
/// "n of m trainees covered" is the trainees' own cards counted, and the two cannot disagree.
/// </summary>
/// <remarks>
/// <para>
/// <b>Which campaigns.</b> Multi-source feedback ones (<see cref="MsfTemplateKind.Msf" />): a learner-feedback campaign is
/// run on the same aggregate but is another instrument (T164, D35), and counting it would say MSF covered an EPA when
/// only learners answered. Released ones only, the cut [T138] makes for the committee snapshot: before release the trainee
/// has not seen the report and the coordinator may still withdraw it. And of a released campaign, only the EPAs its
/// evidence rows carry (<see cref="MsfCampaignCoverage" />, T186).
/// </para>
/// <para>
/// <b>Which semester.</b> The one containing the UTC day the campaign closed (<see cref="MsfCampaign.ClosedOn" />), the
/// day its evidence is dated and a committee review windows it on. The semesters are [T130]'s
/// <see cref="AcademicPeriod" />, read whole: from the first one's first instant to the instant the one after the last
/// begins, half open, as <c>MsfCampaignReviewWindow</c> draws them.
/// </para>
/// <para>
/// <b>Which EPAs.</b> The trainee's list (<see cref="OnListOf" />): the items of their curriculum in force (T158), the
/// College's and their own institution's, never another adopting institution's.
/// </para>
/// </remarks>
internal static class MsfSemesterCoverage
{
    /// <summary>
    /// Whether a trainee counts in a semester: their programme had started by its last day (<see cref="AcademicPeriod.End" />,
    /// 31 December for semester 2, D40), so a trainee who starts on 30 June counts in semester 1 and one who starts on
    /// 1 July does not. The rule by which a trainee's own card leaves out the earlier semester
    /// (<see cref="GetMsfCoverageForTraineeQuery" />) and by which the programme's counts leave a trainee out of a
    /// semester's "of m" (<see cref="GetMsfProgrammeCoverageQuery" />, T210), so the two cannot drift apart at the edge.
    /// </summary>
    public static bool CountsIn(DateOnly programmeStart, AcademicPeriod period) => programmeStart <= period.End;

    /// <summary>
    /// The items of a trainee's EPA list: in force (<see cref="CurriculumItemsInForce" />), on their curriculum, and
    /// either the College's or their own institution's. A curriculum row is shared by every institution that adopts it,
    /// so another institution's own items on it are not theirs.
    /// </summary>
    public static IQueryable<CurriculumItem> OnListOf(this IQueryable<CurriculumItem> items, int curriculumId, int institutionId)
    {
        ArgumentNullException.ThrowIfNull(items);

        return items
            .InForce()
            .Where(item => item.CurriculumId == curriculumId &&
                           (item.OwningInstitutionId == null || item.OwningInstitutionId == institutionId));
    }

    /// <summary>
    /// Every EPA a released multi-source feedback campaign about one of these trainees covered, each with the semester of
    /// <paramref name="periods" /> the campaign closed in and the campaign. One read of the campaigns and one of their
    /// evidence (<see cref="MsfCampaignCoverage.RecordedEpasAsync(IApplicationDbContext, IEnumerable{ValueTuple{string, int, MsfCampaignState}}, string, CancellationToken)" />),
    /// however many trainees; none when no campaign qualifies.
    /// </summary>
    /// <remarks>
    /// Whether an EPA is on a trainee's list is the caller's to judge, against <see cref="OnListOf" />: a campaign's
    /// evidence can carry an EPA the trainee's list no longer holds (the release checks the list as it was then).
    /// </remarks>
    /// <param name="periods">Consecutive semesters, oldest first.</param>
    public static async Task<IReadOnlyList<MsfSemesterCovering>> ReadAsync(
        IApplicationDbContext dbContext,
        IReadOnlyCollection<string> subjectUserIds,
        IReadOnlyList<AcademicPeriod> periods,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(subjectUserIds);
        ArgumentNullException.ThrowIfNull(periods);

        var subjects = subjectUserIds
            .Where(userId => !string.IsNullOrWhiteSpace(userId))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (subjects.Length == 0 || periods.Count == 0)
        {
            return [];
        }

        // Half-open UTC bounds, as MsfCampaignReviewWindow draws them: ClosedOn is an instant, and its UTC day is the day
        // the evidence is dated. The day after the last semester is the next semester's first; there is none only after
        // the last representable one.
        var closedFrom = periods[0].Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var closedBefore = periods[^1].Next() is { } after
            ? after.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
            : DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);

        // The campaigns first, windowed and cut to released MSF in SQL; then which EPAs each one's evidence rows carry,
        // from the one reader the committee snapshot shares (T186). No campaign, no second read.
        var releasedCampaigns = await dbContext.Set<MsfCampaign>()
            .AsNoTracking()
            .Where(campaign => subjects.Contains(campaign.SubjectUserId) &&
                               campaign.Template.Kind == MsfTemplateKind.Msf &&
                               campaign.State == MsfCampaignState.Released &&
                               campaign.ClosedOn >= closedFrom &&
                               campaign.ClosedOn < closedBefore)
            .Select(campaign => new
            {
                campaign.Id,
                campaign.SubjectUserId,
                campaign.State,
                TemplateName = campaign.Template.Name,
                ClosedOn = campaign.ClosedOn!.Value,
                campaign.ReleasedOn
            })
            .ToListAsync(cancellationToken);

        var recorded = await MsfCampaignCoverage.RecordedEpasAsync(
            dbContext,
            releasedCampaigns.Select(campaign => (campaign.SubjectUserId, campaign.Id, campaign.State)),
            MsfEvidenceKinds.MsfActivityTypeKey,
            cancellationToken);

        return releasedCampaigns
            .SelectMany(campaign => recorded[campaign.Id].Select(epa => new MsfSemesterCovering(
                campaign.SubjectUserId,
                epa.EpaId,
                AcademicPeriod.Containing(DateOnly.FromDateTime(campaign.ClosedOn)),
                new MsfCoveringCampaignDto(
                    campaign.Id,
                    campaign.TemplateName,
                    DateOnly.FromDateTime(campaign.ClosedOn),
                    campaign.ReleasedOn is DateTime released ? DateOnly.FromDateTime(released) : null))))
            .ToList();
    }
}

/// <summary>
/// One EPA a released multi-source feedback campaign about a trainee covered, in the semester the campaign closed in
/// (<see cref="MsfSemesterCoverage.ReadAsync" />).
/// </summary>
internal sealed record MsfSemesterCovering(
    string SubjectUserId,
    int EpaId,
    AcademicPeriod Period,
    MsfCoveringCampaignDto Campaign);
