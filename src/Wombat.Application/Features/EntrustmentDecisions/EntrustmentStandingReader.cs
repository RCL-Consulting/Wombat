using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Queries;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Activities;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.EntrustmentDecisions;

/// <summary>
/// Where one trainee stands on each EPA of their curriculum (T166): the body of
/// <see cref="GetEntrustmentStandingForTraineeQueryHandler" /> after its scope check, moved here whole (T355, note 3) so the
/// Trainee's Home reads the same standing in summary mode inside its one read (<c>GetTraineeDashboardSummaryQuery</c>):
/// Home's card and My progress's panel cannot disagree. The rules (which EPAs, the year target, the exit rule, one ladder)
/// are the query's remarks.
/// </summary>
/// <remarks>
/// The reader checks no scope: its callers do. The query asks <see cref="TraineeScopeResolver.MayReadAsync" /> first; Home
/// reads the caller's own record. The latest ratings are read from the activities <c>principal</c> may read
/// (<c>WhereReadableBy</c>, T101), as the trajectory chart beside them is.
/// </remarks>
public static class EntrustmentStandingReader
{
    /// <param name="principal">Who is reading: the latest ratings are the ones they may read (T101).</param>
    /// <param name="asOf">The day the training year is read for; nothing else is read as of it.</param>
    /// <param name="withLatestRatings">
    /// False for the summary Home shows (note 3): every <see cref="EpaStandingDto.LatestRating" /> is null and no activity
    /// is read. Every other figure is the same.
    /// </param>
    /// <returns>Null when the trainee holds no profile.</returns>
    public static async Task<EntrustmentStandingDto?> ReadAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        string traineeUserId,
        DateOnly asOf,
        bool withLatestRatings,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(principal);

        var profile = await TraineeScopeResolver.PreferredProfiles(dbContext)
            .AsNoTracking()
            .Where(entity => entity.UserId == traineeUserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return null;
        }

        var stage = profile.GetStage(asOf);
        var targetYear = stage ?? 1;

        // T312: a running programme's items in force, or the list an ended one ended with (ProgrammeItems), so a pause or
        // a new item after a graduation does not change her standing.
        var items = (await ProgrammeItems.ReadAsync(dbContext, profile, cancellationToken))
            .OrderBy(item => item.EpaCode, StringComparer.Ordinal)
            .ToList();
        var epaIds = items.Select(item => item.EpaId).Distinct().ToArray();

        // Active is the status the expiry job maintains, as on the trainee's own authorisations page. Two active
        // decisions on one EPA should not exist (issuing supersedes); if they do, the latest issued is the one in force.
        // An ended programme's STARs are read as the programme left them (T312): one that expired, was revoked or was
        // superseded after the end still counts, and so does one the committee issued for this programme after it.
        var decisions = epaIds.Length == 0
            ? []
            : await DecisionsInForce(
                    dbContext,
                    traineeUserId,
                    epaIds,
                    profile.IsActive ? null : profile.EndedOn,
                    profile.ProgrammeStartDate)
                .Select(decision => new
                {
                    decision.Id,
                    decision.EpaId,
                    decision.IssuedOn,
                    decision.ExpiresOn,
                    LevelOrder = decision.AuthorisedLevel.Order,
                    LevelLabel = decision.AuthorisedLevel.Label,
                    LevelScaleId = decision.AuthorisedLevel.ScaleId
                })
                .ToListAsync(cancellationToken);

        var decisionByEpa = decisions
            .GroupBy(decision => decision.EpaId)
            .ToDictionary(
                group => group.Key,
                group => group.OrderByDescending(decision => decision.IssuedOn).ThenByDescending(decision => decision.Id).First());

        // The rows the trajectory chart reads, confined the way it confines them: the trainee's own activities, as far as
        // this caller may read them (T101). A trainee asking about themselves matches on SubjectUserId.
        var readableActivities = dbContext.Set<Activity>()
            .AsNoTracking()
            .Where(activity => activity.SubjectUserId == traineeUserId &&
                               activity.EpaId.HasValue &&
                               epaIds.Contains(activity.EpaId.Value))
            .WhereReadableBy(principal);

        // Summary mode (Home, note 3) reads no activity: it shows no rating, so it does not pay for reading them.
        var ratings = epaIds.Length == 0 || !withLatestRatings
            ? []
            : await AttributedRatings.ReadAsync(dbContext, readableActivities, cancellationToken);

        var latestRatingByEpa = ratings
            .GroupBy(rating => rating.EpaId)
            .ToDictionary(
                group => group.Key,
                group => group.OrderByDescending(rating => rating.ObservedOn).ThenByDescending(rating => rating.ActivityId).First());

        var scaleIds = items.Select(item => item.ScaleId)
            .Concat(decisions.Select(decision => (int?)decision.LevelScaleId))
            .Concat(ratings.Select(rating => rating.RatedScaleId))
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToArray();

        var rungs = await EntrustmentRungLabels.LoadAsync(dbContext, scaleIds.Select(id => (int?)id), cancellationToken);
        var scaleNames = scaleIds.Length == 0
            ? new Dictionary<int, string>()
            : await dbContext.Set<EntrustmentScale>()
                .AsNoTracking()
                .Where(scale => scaleIds.Contains(scale.Id))
                .ToDictionaryAsync(scale => scale.Id, scale => scale.Name, cancellationToken);

        string? NameOf(int? scaleId)
            => scaleId is int id && scaleNames.TryGetValue(id, out var name) ? name : null;

        // Known to be on a ladder other than the item's pin. Unknown on either side is not "different".
        string? OtherLadder(int? ownScaleId, int? itemScaleId)
            => ownScaleId is int own && itemScaleId is int pinned && own != pinned ? NameOf(own) ?? $"scale {own}" : null;

        var epas = new List<EpaStandingDto>(items.Count);
        foreach (var item in items)
        {
            var yearTarget = new CurriculumItem
            {
                MinimumLevelOrder = item.MinimumLevelOrder,
                MinimumLevelByStageJson = item.MinimumLevelByStageJson
            }.GetMinimumLevelForStage(targetYear);

            // The reader above falls back to the exit level for a year its map does not name. Said, not hidden: a
            // "year 5 target", or a year-1 target nobody set on a local item, is not Annexure A's.
            var yearTargetIsExitLevel = !CurriculumItem.ParseStageOverrides(item.MinimumLevelByStageJson).ContainsKey(targetYear);

            StandingDecisionDto? decisionDto = null;
            int? decisionOrder = null;
            int? decisionScaleId = null;
            if (decisionByEpa.TryGetValue(item.EpaId, out var decision))
            {
                decisionOrder = decision.LevelOrder;
                decisionScaleId = decision.LevelScaleId;
                decisionDto = new StandingDecisionDto(
                    decision.Id,
                    decision.LevelOrder,
                    decision.LevelLabel,
                    OtherLadder(decision.LevelScaleId, item.ScaleId),
                    decision.IssuedOn,
                    decision.ExpiresOn);
            }

            StandingRatingDto? ratingDto = null;
            if (latestRatingByEpa.TryGetValue(item.EpaId, out var rating))
            {
                var otherLadder = OtherLadder(rating.RatedScaleId, item.ScaleId);
                ratingDto = new StandingRatingDto(
                    rating.ActivityId,
                    rating.ObservedOn,
                    rating.ObservedOnDeclared,
                    rating.Rating,
                    rungs.Format(otherLadder is null ? item.ScaleId : rating.RatedScaleId, rating.Rating),
                    otherLadder,
                    rating.Source,
                    EntrustmentStanding.Judge(rating.Rating, rating.RatedScaleId, yearTarget, item.ScaleId));
            }

            epas.Add(new EpaStandingDto(
                item.CurriculumItemId,
                item.EpaId,
                item.EpaCode,
                item.EpaTitle,
                NameOf(item.ScaleId),
                item.IsLocal,
                yearTarget,
                rungs.Format(item.ScaleId, yearTarget),
                yearTargetIsExitLevel,
                item.MinimumLevelOrder,
                rungs.Format(item.ScaleId, item.MinimumLevelOrder),
                decisionDto,
                EntrustmentStanding.Judge(decisionOrder, decisionScaleId, yearTarget, item.ScaleId),
                EntrustmentStanding.Judge(decisionOrder, decisionScaleId, item.MinimumLevelOrder, item.ScaleId),
                ratingDto)
            {
                EpaInForce = item.EpaInForce
            });
        }

        return new EntrustmentStandingDto(
            asOf,
            profile.ProgrammeStartDate,
            targetYear,
            stage is null,
            epas,
            ExitReadiness(epas, items.ToDictionary(item => item.CurriculumItemId, item => item.ScaleId)))
        {
            StarsAsEndedOn = profile.IsActive ? null : profile.EndedOn
        };
    }

    /// <summary>
    /// The trainee's STARs on <paramref name="epaIds" /> that are in force: those <see cref="EntrustmentDecisionStatus.Active" />
    /// now while the programme runs (or ended on a day Wombat did not record); those the programme ended with when it has
    /// ended (T312).
    /// </summary>
    /// <remarks>
    /// <para>
    /// A STAR is on an ended programme's record when it stood on the last day (issued by then), or when the committee
    /// issued it for this programme after the last day: at a review of this trainee whose period began by the last day and
    /// had not ended before the programme started (the operator's decision on T312's B5, 2026-10-27). A sitting can ratify
    /// after the end what it was convened for before it; a review of another programme of the same trainee, one that began
    /// after this one ended or ended before it began, is not this record's.
    /// </para>
    /// <para>
    /// Nothing after the last day takes a STAR off the record: it is kept unless it had expired before the last day (it
    /// expires after its <c>ExpiresOn</c>, as the expiry job judges), had been revoked by the end of it, or had been
    /// superseded by then by a STAR issued by then. A revocation, an expiry or a successor after the end changes nothing;
    /// where a successor is itself on the record, the reader takes the latest issued.
    /// </para>
    /// </remarks>
    private static IQueryable<EntrustmentDecision> DecisionsInForce(
        IApplicationDbContext dbContext,
        string traineeUserId,
        int[] epaIds,
        DateOnly? endedOn,
        DateOnly programmeStart)
    {
        var decisions = dbContext.Set<EntrustmentDecision>()
            .AsNoTracking()
            .Where(decision => decision.TraineeUserId == traineeUserId && epaIds.Contains(decision.EpaId));

        if (endedOn is not { } lastDay)
        {
            return decisions.Where(decision => decision.Status == EntrustmentDecisionStatus.Active);
        }

        var endOfLastDay = ProgrammeCalendar.EndOf(lastDay);
        var all = dbContext.Set<EntrustmentDecision>().AsNoTracking();
        var reviewsOfThisProgramme = dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .Where(review => review.ReviewPeriodFrom <= lastDay && review.ReviewPeriodTo >= programmeStart);
        return decisions.Where(decision =>
            (decision.IssuedOn <= lastDay ||
             reviewsOfThisProgramme.Any(review => review.Id == decision.IssuedByCommitteeReviewId)) &&
            (decision.ExpiresOn == null || decision.ExpiresOn >= lastDay) &&
            (decision.Status != EntrustmentDecisionStatus.Revoked || decision.RevokedOn >= endOfLastDay) &&
            (decision.Status != EntrustmentDecisionStatus.Superseded ||
             !all.Any(successor => successor.Id == decision.SupersededByDecisionId && successor.IssuedOn <= lastDay)));
    }

    /// <summary>
    /// The exit rule over the national core of <paramref name="all" />: every EPA's decision at or above its own exit
    /// level, grouped by exit level so the page can say it as the College does ("Level 5 in 9 EPAs and Level 4 in the
    /// remaining 6"). An institution's local items are not in the College's rule, so they are not counted in it.
    /// </summary>
    private static ExitRuleReadinessDto ExitReadiness(
        IReadOnlyList<EpaStandingDto> all,
        IReadOnlyDictionary<int, int?> scaleIdByItem)
    {
        var epas = all.Where(epa => !epa.IsLocal).ToArray();
        var groups = epas
            .GroupBy(epa => (ScaleId: scaleIdByItem[epa.CurriculumItemId], epa.ExitLevelOrder, epa.ExitLevelLabel))
            .OrderByDescending(group => group.Key.ExitLevelOrder)
            .ThenBy(group => group.Key.ExitLevelLabel, StringComparer.Ordinal)
            .Select(group => new ExitLevelGroupDto(
                group.Key.ExitLevelOrder,
                group.Key.ExitLevelLabel,
                group.Count(),
                group.Count(epa => epa.ExitStatus == EntrustmentStandingStatus.AtOrAbove)))
            .ToArray();

        return new ExitRuleReadinessDto(
            epas.Length,
            epas.Count(epa => epa.ExitStatus == EntrustmentStandingStatus.AtOrAbove),
            groups,
            epas
                .Where(epa => epa.ExitStatus != EntrustmentStandingStatus.AtOrAbove)
                .Select(epa => epa.EpaCode)
                .ToArray());
    }
}
