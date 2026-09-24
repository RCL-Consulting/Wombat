using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Curricula;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>
/// Per EPA of a trainee's curriculum and per semester, whether a released multi-source feedback campaign covering the
/// EPA closed in that semester, and when. For the trainee's own progress page and the committee review page. (T168)
/// </summary>
/// <remarks>
/// <para>
/// <b>What "covered" means is the College's D9.</b> A campaign is run once per period across many EPAs, so "MSF completed
/// for EPA 7" means "a campaign covering EPA 7 was released this period", never "a campaign about EPA 7". Which EPAs a
/// released campaign covered is read by <see cref="MsfCampaignCoverage" />, not from <c>counts_for</c> and not from
/// <c>CurriculumItemProgress</c>: by D8 MSF credits nothing (<c>msf_cpsa</c> ships <c>counts_for: []</c>), so it moves
/// no progress row and consumes none of Annexure A's encounters. Nothing here is a target or a shortfall. Whether the
/// College wants one campaign per semester, or some other cadence, is not confirmed from Annexure B, so the reader shows
/// coverage and says nothing about what is missing.
/// </para>
/// <para>
/// <b>Which campaigns.</b> Multi-source feedback ones (<see cref="MsfTemplateKind.Msf" />): a learner-feedback campaign
/// is run on the same aggregate but is another instrument, and covers nothing here (T164). Released ones only, the cut [T138] makes for the committee snapshot and
/// <see cref="ListMsfCampaignsForTraineeQuery" /> makes for the trainee: before release the trainee has not seen the
/// report and the coordinator may still withdraw it, and a withdrawn campaign was retracted. And of a released campaign,
/// only the EPAs its evidence rows carry: the <c>msf_cpsa</c> activities its release wrote, one per EPA it could honour,
/// each naming the campaign and stamped with its EPA (<c>Activity.EpaId</c>, T137). A declared EPA with no such row was
/// not covered, whatever the reason. Not the per-EPA stamp <see cref="MsfCampaignEpa.RecordedOn" />, which a campaign
/// released before it existed does not carry although its evidence does (T186). So a covered EPA is exactly an
/// <c>msf_cpsa</c> evidence activity with that EPA, the rule the committee snapshot's campaign line is written by, and
/// the two cannot disagree.
/// </para>
/// <para>
/// <b>Which semester.</b> The one containing the UTC day the campaign actually closed (<see cref="MsfCampaign.ClosedOn" />),
/// which is the <c>ObservedOn</c> the release stamps on each evidence activity and the day a committee review windows
/// the campaign on (T138, <c>MsfCampaignReviewWindow</c>). Not <see cref="MsfCampaign.ClosesOn" />, the scheduled day,
/// and not <see cref="MsfCampaign.ReleasedOn" />: a campaign that closed on 28 June and was released on 5 July is
/// semester 1's evidence. The semesters are [T130]'s <see cref="AcademicPeriod" />, the national calendar every quota
/// is bucketed on.
/// </para>
/// <para>
/// <b>Who may ask:</b> the trainee themselves, a global Administrator, or someone who oversees the trainee's programme
/// (<see cref="TraineeScopeResolver.MayReadAsync" />, T113), the rule <see cref="ListMsfCampaignsForTraineeQuery" /> and
/// the progress reader apply. Anyone else gets null, which is also the answer for a trainee with no profile, so the
/// answer never confirms that the id names somebody. Nothing a respondent wrote is read: a campaign's id, template name
/// and dates only.
/// </para>
/// <para>
/// <b>Which EPAs:</b> the curriculum on the trainee's preferred profile (<see cref="TraineeScopeResolver.PreferredProfiles" />),
/// national core items plus the trainee's own institution's local items (a curriculum row is shared by every adopting
/// institution), in force only (<see cref="CurriculumItemsInForce" />, T158). The same items the standing beside it lists.
/// </para>
/// </remarks>
/// <param name="From">
/// The first day of the span, whose semester is the first one read. The committee review page passes its review period.
/// When null, the span is the semester before <paramref name="To" />'s and <paramref name="To" />'s own, leaving out the
/// earlier one when the trainee's programme started after it ended: what the progress page's quota cards show. When
/// given, <paramref name="To" /> must be too.
/// </param>
/// <param name="To">The last day of the span. Defaults to <paramref name="AsOf" />.</param>
/// <param name="AsOf">
/// The day read as today: <paramref name="To" />'s default, and the day a semester has ended before when its last day,
/// 31 December for semester 2 (D40's December fold), is behind it. Defaults to today in South Africa. The pages leave it
/// null; it pins the calendar's edges in a test, as it does for the standing and the progress reader.
/// </param>
public sealed record GetMsfCoverageForTraineeQuery(
    string TraineeUserId,
    ClaimsPrincipal Principal,
    DateOnly? From = null,
    DateOnly? To = null,
    DateOnly? AsOf = null)
    : IRequest<MsfCoverageDto?>;

public sealed class GetMsfCoverageForTraineeQueryValidator : AbstractValidator<GetMsfCoverageForTraineeQuery>
{
    /// <summary>A pre-graduation review spans a programme; ten years is well past any, and bounds the columns.</summary>
    internal const int MaximumSpanDays = 3660;

    public GetMsfCoverageForTraineeQueryValidator()
    {
        RuleFor(query => query.TraineeUserId).NotEmpty();
        RuleFor(query => query.Principal).NotNull();

        RuleFor(query => query.To)
            .NotNull()
            .When(query => query.From.HasValue)
            .WithMessage("A span with a first day needs a last day.");

        RuleFor(query => query)
            .Must(query => query.From!.Value <= query.To!.Value)
            .When(query => query.From.HasValue && query.To.HasValue)
            .WithMessage("The span must not end before it starts.");

        RuleFor(query => query)
            .Must(query => query.To!.Value.DayNumber - query.From!.Value.DayNumber <= MaximumSpanDays)
            .When(query => query.From.HasValue && query.To.HasValue)
            .WithMessage("The span may be at most ten years.");
    }
}

public sealed class GetMsfCoverageForTraineeQueryHandler
    : IRequestHandler<GetMsfCoverageForTraineeQuery, MsfCoverageDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetMsfCoverageForTraineeQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MsfCoverageDto?> Handle(GetMsfCoverageForTraineeQuery request, CancellationToken cancellationToken)
    {
        // Trimmed once, so the id that is authorised is the id that is read.
        var traineeUserId = request.TraineeUserId.Trim();

        if (!await TraineeScopeResolver.MayReadAsync(_dbContext, request.Principal, traineeUserId, cancellationToken))
        {
            return null;
        }

        var profile = await TraineeScopeResolver.PreferredProfiles(_dbContext)
            .AsNoTracking()
            .Where(entity => entity.UserId == traineeUserId)
            .Select(entity => new { entity.CurriculumId, entity.InstitutionId, entity.ProgrammeStartDate })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return null;
        }

        var today = request.AsOf ?? QuotaCalendar.Today();
        var to = request.To ?? today;
        var periods = request.From is { } from
            ? SemestersBetween(from, to)
            : CurrentAndPrevious(to, profile.ProgrammeStartDate);

        var items = await _dbContext.Set<CurriculumItem>()
            .AsNoTracking()
            .InForce()
            .Where(item => item.CurriculumId == profile.CurriculumId &&
                           (item.OwningInstitutionId == null || item.OwningInstitutionId == profile.InstitutionId))
            .Select(item => new
            {
                item.Id,
                item.EpaId,
                EpaCode = item.Epa.Code,
                EpaTitle = item.Epa.Title,
                IsLocal = item.OwningInstitutionId != null
            })
            .ToListAsync(cancellationToken);

        items = items.OrderBy(item => item.EpaCode, StringComparer.Ordinal).ThenBy(item => item.Id).ToList();
        var epaIds = items.Select(item => item.EpaId).Distinct().ToArray();

        // Half-open UTC bounds, as MsfCampaignReviewWindow draws them: ClosedOn is an instant, and its UTC day is the
        // day the evidence is dated. The day after the last semester is the next semester's first; there is none only
        // after the last representable one.
        var closedFrom = periods[0].Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var closedBefore = periods[^1].Next() is { } after
            ? after.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
            : DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);

        // The campaigns first, windowed and cut to released in SQL; then which EPAs each one's evidence rows carry, from
        // the one reader the committee snapshot shares (T186). No campaign, no second read.
        var releasedCampaigns = epaIds.Length == 0
            ? []
            : await _dbContext.Set<MsfCampaign>()
                .AsNoTracking()
                .Where(campaign => campaign.SubjectUserId == traineeUserId &&
                                   // Multi-source feedback only. A learner-feedback campaign is run on the same aggregate
                                   // but is another instrument (T164, D35): counting it here would tell a committee that
                                   // MSF covered PAED-015 when only learners answered. Its evidence rows are another type
                                   // too, so the reader below would find none; this says so rather than relying on it.
                                   campaign.Template.Kind == MsfTemplateKind.Msf &&
                                   campaign.State == MsfCampaignState.Released &&
                                   campaign.ClosedOn >= closedFrom &&
                                   campaign.ClosedOn < closedBefore)
                .Select(campaign => new
                {
                    campaign.Id,
                    campaign.State,
                    TemplateName = campaign.Template.Name,
                    ClosedOn = campaign.ClosedOn!.Value,
                    campaign.ReleasedOn
                })
                .ToListAsync(cancellationToken);

        var recorded = await MsfCampaignCoverage.RecordedEpasAsync(
            _dbContext,
            traineeUserId,
            releasedCampaigns.Select(campaign => (campaign.Id, campaign.State)),
            MsfEvidenceKinds.MsfActivityTypeKey,
            cancellationToken);

        var onCurriculum = epaIds.ToHashSet();
        var covering = releasedCampaigns
            .SelectMany(campaign => recorded[campaign.Id]
                .Where(epa => onCurriculum.Contains(epa.EpaId))
                .Select(epa => new
                {
                    epa.EpaId,
                    CampaignId = campaign.Id,
                    campaign.TemplateName,
                    campaign.ClosedOn,
                    campaign.ReleasedOn
                }))
            .ToList();

        var campaignsByEpaAndPeriod = covering
            .Select(entry => new
            {
                entry.EpaId,
                Period = AcademicPeriod.Containing(DateOnly.FromDateTime(entry.ClosedOn)),
                Campaign = new MsfCoveringCampaignDto(
                    entry.CampaignId,
                    entry.TemplateName,
                    DateOnly.FromDateTime(entry.ClosedOn),
                    entry.ReleasedOn is DateTime released ? DateOnly.FromDateTime(released) : null)
            })
            .GroupBy(entry => (entry.EpaId, entry.Period))
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<MsfCoveringCampaignDto>)group
                    .Select(entry => entry.Campaign)
                    .DistinctBy(campaign => campaign.CampaignId)
                    .OrderByDescending(campaign => campaign.ClosedOn)
                    .ThenByDescending(campaign => campaign.CampaignId)
                    .ToArray());

        var epas = items
            .Select(item => new MsfEpaCoverageDto(
                item.Id,
                item.EpaId,
                item.EpaCode,
                item.EpaTitle,
                item.IsLocal,
                periods
                    .Select(period => new MsfEpaPeriodCoverageDto(
                        period.Year,
                        period.Semester,
                        campaignsByEpaAndPeriod.TryGetValue((item.EpaId, period), out var campaigns) ? campaigns : []))
                    .ToArray()))
            .ToArray();

        var periodDtos = periods
            .Select(period => new MsfCoveragePeriodDto(
                period.Year,
                period.Semester,
                QuotaText.SemesterName(period),
                QuotaText.Months(period),
                period.Start,
                period.End,
                period.NominalEnd,
                HasEnded: period.End < today,
                EpasCovered: epas.Count(epa => epa.For(period.Year, period.Semester)?.IsCovered == true)))
            .ToArray();

        return new MsfCoverageDto(to, periodDtos, epas);
    }

    /// <summary>
    /// Every semester from the one containing <paramref name="from" /> to the one containing <paramref name="to" />, or
    /// <paramref name="to" />'s alone when the two are the wrong way round (the validator refuses that before here).
    /// </summary>
    private static IReadOnlyList<AcademicPeriod> SemestersBetween(DateOnly from, DateOnly to)
    {
        var last = AcademicPeriod.Containing(to);
        var current = AcademicPeriod.Containing(from);
        if (from > to)
        {
            return [last];
        }

        var periods = new List<AcademicPeriod> { current };
        while (current != last && current.Next() is { } next)
        {
            current = next;
            periods.Add(current);
        }

        return periods;
    }

    /// <summary>
    /// The semester before <paramref name="to" />'s and its own, as the progress page's cards show a current and a
    /// previous window; the earlier one only if the programme had started by its last day.
    /// </summary>
    private static IReadOnlyList<AcademicPeriod> CurrentAndPrevious(DateOnly to, DateOnly programmeStart)
    {
        var current = AcademicPeriod.Containing(to);
        return current.Previous() is { } previous && programmeStart <= previous.End
            ? [previous, current]
            : [current];
    }
}

/// <summary>
/// A trainee's MSF coverage: per EPA of their curriculum, per semester of a span, which released campaigns covering the
/// EPA closed in it (<see cref="GetMsfCoverageForTraineeQuery" />, T168).
/// </summary>
/// <param name="To">The last day of the span.</param>
/// <param name="Periods">The span's semesters, oldest first.</param>
/// <param name="Epas">One row per curriculum item, in EPA code order, each with one entry per semester in <see cref="Periods" />.</param>
public sealed record MsfCoverageDto(
    DateOnly To,
    IReadOnlyList<MsfCoveragePeriodDto> Periods,
    IReadOnlyList<MsfEpaCoverageDto> Epas);

/// <summary>One semester of the span, as a reader names it.</summary>
/// <param name="Name">"Semester 1, 2026".</param>
/// <param name="Months">"January to June", on the College's calendar.</param>
/// <param name="End">The last day a campaign's close is bucketed in: 31 December for semester 2 (D40's December fold).</param>
/// <param name="NominalEnd">The semester's last day on the College's calendar: 30 November for semester 2.</param>
/// <param name="HasEnded">
/// <see cref="End" /> is behind the day read as today, so no campaign can still close in it. One that closed in it can
/// still be released, so an ended semester's coverage can still grow, and nothing may word it as final.
/// </param>
/// <param name="EpasCovered">How many of the rows a released campaign covered in this semester.</param>
public sealed record MsfCoveragePeriodDto(
    int Year,
    int Semester,
    string Name,
    string Months,
    DateOnly Start,
    DateOnly End,
    DateOnly NominalEnd,
    bool HasEnded,
    int EpasCovered);

/// <summary>One EPA of the trainee's curriculum, with its coverage in each semester of the span.</summary>
/// <param name="IsLocal">The item is the trainee's institution's own, not one of the College's.</param>
/// <param name="Periods">One entry per semester of the span, oldest first, in the order of <see cref="MsfCoverageDto.Periods" />.</param>
public sealed record MsfEpaCoverageDto(
    int CurriculumItemId,
    int EpaId,
    string EpaCode,
    string EpaTitle,
    bool IsLocal,
    IReadOnlyList<MsfEpaPeriodCoverageDto> Periods)
{
    /// <summary>This EPA's coverage in one semester, or null when the semester is outside the span.</summary>
    public MsfEpaPeriodCoverageDto? For(int year, int semester)
        => Periods.FirstOrDefault(period => period.Year == year && period.Semester == semester);
}

/// <summary>One EPA in one semester: the released campaigns covering it that closed in the semester, newest first.</summary>
public sealed record MsfEpaPeriodCoverageDto(int Year, int Semester, IReadOnlyList<MsfCoveringCampaignDto> Campaigns)
{
    public bool IsCovered => Campaigns.Count > 0;

    /// <summary>The campaign that closed last, or null when none covered the EPA.</summary>
    public MsfCoveringCampaignDto? Latest => Campaigns.Count > 0 ? Campaigns[0] : null;
}

/// <summary>A released campaign that covered an EPA.</summary>
/// <param name="ClosedOn">The UTC day the response window shut: the day its evidence is dated, and the semester's key.</param>
/// <param name="ReleasedOn">The UTC day the report was released to the trainee.</param>
public sealed record MsfCoveringCampaignDto(int CampaignId, string TemplateName, DateOnly ClosedOn, DateOnly? ReleasedOn);
