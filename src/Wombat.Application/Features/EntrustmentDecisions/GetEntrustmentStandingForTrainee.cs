using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Queries;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.EntrustmentDecisions;

/// <summary>
/// Where a trainee stands on each EPA of their curriculum: the active STAR decision against Annexure A's target for their
/// training year and against the exit rule, beside the latest rating a named assessor gave. For the committee's review
/// page and the trainee's own progress page. (T166)
/// </summary>
/// <remarks>
/// <para>
/// <b>Read-only, and gates nothing.</b> Whether recording a Graduate decision, or completing a programme, should be
/// refused or need a recorded reason when the exit rule is unmet is an operator decision T166 left open. The
/// recommendation is to keep this informational until the College confirms the exit rule; nothing consults it.
/// </para>
/// <para>
/// <b>Who may ask:</b> the trainee themselves, a global Administrator, or someone who oversees the trainee's programme
/// (<see cref="TraineeScopeResolver.MayReadAsync" />, T113). Anyone else gets null, which is also the answer for a
/// trainee with no profile, so the answer never confirms that the id names somebody. The latest rating is read from the
/// activities the caller may read (<c>WhereReadableBy</c>, T101), exactly as the trajectory chart beside it is: an
/// overseer at the trainee's current institution is not thereby shown evidence stamped to another.
/// </para>
/// <para>
/// <b>Which EPAs:</b> the curriculum on the trainee's preferred profile (<see cref="TraineeScopeResolver.PreferredProfiles" />:
/// the active one, else the most recent, so a graduate's standing still reads), national core items plus the trainee's
/// own institution's local items only (a curriculum row is shared by every adopting institution), and only items in
/// force (<see cref="CurriculumItemsInForce" />, T158).
/// </para>
/// <para>
/// <b>The year target</b> is the item's per-stage map (<c>MinimumLevelByStageJson</c>, Annexure A's <c>y1</c> to
/// <c>y4</c>), read by <see cref="CurriculumItem.GetMinimumLevelForStage" />, the same reader the credit engine uses for
/// its per-encounter minimum. There is deliberately no second copy of Annexure A: two would drift. Where the map names
/// no level for the year (an item with no map, or a trainee past the years it names), that reader gives the exit level,
/// and the row says so (<see cref="EpaStandingDto.YearTargetIsExitLevel" />) rather than passing it off as Annexure A's.
/// </para>
/// <para>
/// <b>The exit rule</b> is the College's, so it is counted over the national core items only. An institution's own local
/// items are rows of the table with their own exit level, but they are not part of "Level 5 in 9 EPAs and Level 4 in the
/// remaining 6".
/// </para>
/// <para>
/// <b>Levels are compared only on the item's pinned ladder</b> (<see cref="EntrustmentStanding.Judge" />, T109).
/// </para>
/// </remarks>
/// <param name="AsOf">
/// The day the training year is read for; nothing else is read as of it. Defaults to today in South Africa. The committee
/// review page passes the last day of the review period once that has passed, so a panel judging a year sees that year's
/// targets; decisions and ratings are still read as they stand, because the decisions a review issues postdate its
/// period.
/// </param>
public sealed record GetEntrustmentStandingForTraineeQuery(
    string TraineeUserId,
    ClaimsPrincipal Principal,
    DateOnly? AsOf = null)
    : IRequest<EntrustmentStandingDto?>;

public sealed class GetEntrustmentStandingForTraineeQueryValidator
    : AbstractValidator<GetEntrustmentStandingForTraineeQuery>
{
    public GetEntrustmentStandingForTraineeQueryValidator()
    {
        RuleFor(query => query.TraineeUserId).NotEmpty();
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class GetEntrustmentStandingForTraineeQueryHandler
    : IRequestHandler<GetEntrustmentStandingForTraineeQuery, EntrustmentStandingDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetEntrustmentStandingForTraineeQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EntrustmentStandingDto?> Handle(
        GetEntrustmentStandingForTraineeQuery request,
        CancellationToken cancellationToken)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return null;
        }

        var asOf = request.AsOf ?? QuotaCalendar.Today();
        var stage = profile.GetStage(asOf);
        var targetYear = stage ?? 1;

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
                item.MinimumLevelOrder,
                item.MinimumLevelByStageJson,
                item.ScaleId,
                IsLocal = item.OwningInstitutionId != null
            })
            .ToListAsync(cancellationToken);

        items = items.OrderBy(item => item.EpaCode, StringComparer.Ordinal).ToList();
        var epaIds = items.Select(item => item.EpaId).Distinct().ToArray();

        // Active is the status the expiry job maintains, as on the trainee's own authorisations page. Two active
        // decisions on one EPA should not exist (issuing supersedes); if they do, the latest issued is the one in force.
        var decisions = epaIds.Length == 0
            ? []
            : await _dbContext.Set<EntrustmentDecision>()
                .AsNoTracking()
                .Where(decision => decision.TraineeUserId == traineeUserId &&
                                   decision.Status == EntrustmentDecisionStatus.Active &&
                                   epaIds.Contains(decision.EpaId))
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
        var readableActivities = _dbContext.Set<Activity>()
            .AsNoTracking()
            .Where(activity => activity.SubjectUserId == traineeUserId &&
                               activity.EpaId.HasValue &&
                               epaIds.Contains(activity.EpaId.Value))
            .WhereReadableBy(request.Principal);

        var ratings = epaIds.Length == 0
            ? []
            : await AttributedRatings.ReadAsync(_dbContext, readableActivities, cancellationToken);

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

        var rungs = await EntrustmentRungLabels.LoadAsync(_dbContext, scaleIds.Select(id => (int?)id), cancellationToken);
        var scaleNames = scaleIds.Length == 0
            ? new Dictionary<int, string>()
            : await _dbContext.Set<EntrustmentScale>()
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
                item.Id,
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
                ratingDto));
        }

        return new EntrustmentStandingDto(
            asOf,
            profile.ProgrammeStartDate,
            targetYear,
            stage is null,
            epas,
            ExitReadiness(epas, items.ToDictionary(item => item.Id, item => item.ScaleId)));
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
