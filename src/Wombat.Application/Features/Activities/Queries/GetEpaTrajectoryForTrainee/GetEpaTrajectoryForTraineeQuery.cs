using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Common.Users;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;

/// <summary>
/// One trainee's entrustment trajectory: extracted ratings, each carrying the activity id it came
/// from and the assessor who gave it.
/// </summary>
/// <remarks>
/// <see cref="Principal" /> sits second rather than last because the optional date bounds must keep
/// their defaults, and a parameter that decides what a caller may see is not one to leave optional.
/// Without it this query answered on a caller-supplied trainee id alone — ratings and deep-linkable
/// activity ids for any trainee named. (T101)
/// </remarks>
/// <param name="From">The window's first day, on the encounter date; also the first day of its stepped minimum.</param>
/// <param name="To">The window's last day.</param>
/// <param name="EpaId">Only this EPA's ratings: one EPA's page (T355, C10). Null reads every EPA, as before.</param>
public sealed record GetEpaTrajectoryForTraineeQuery(
    string TraineeUserId,
    ClaimsPrincipal Principal,
    DateOnly? From = null,
    DateOnly? To = null,
    int? EpaId = null) : IRequest<IReadOnlyList<EpaTrajectoryDto>>;

public sealed class GetEpaTrajectoryForTraineeQueryValidator
    : AbstractValidator<GetEpaTrajectoryForTraineeQuery>
{
    public GetEpaTrajectoryForTraineeQueryValidator()
    {
        RuleFor(query => query.TraineeUserId).NotEmpty();
        RuleFor(query => query)
            .Must(query => !query.From.HasValue || !query.To.HasValue || query.From.Value <= query.To.Value)
            .WithMessage("From must be on or before To.");
    }
}

public sealed record EpaTrajectoryDto(
    int EpaId,
    string EpaCode,
    string EpaTitle,
    /// <summary>
    /// Whether the EPA is in force now (T255, D48): <c>Epa.IsActive</c>, the flag its activities' EPA picker labels by
    /// (<see cref="Wombat.Application.Features.Epas.EpaOptionLabel" />) and <c>ActivitySummaryDto.EpaInForce</c> carries,
    /// so a trajectory heading marks "(no longer in use)" on exactly the EPA My activities marks. The chart still draws a
    /// deactivated EPA's ratings: they are evidence already recorded, and deactivating pauses credit, it erases nothing.
    /// No default, so a new producer cannot report every EPA as in force.
    /// </summary>
    bool EpaInForce,
    /// <summary>
    /// The ladder this EPA's ratings are read against — the scale the trainee's curriculum item for it
    /// is pinned to (T109). Null when the item is unpinned, which is a permanent and meaningful state.
    /// </summary>
    int? ScaleId,
    string? ScaleName,
    /// <summary>
    /// The rungs of the pinned scale, in order. Empty when nothing resolves, in which case the chart
    /// falls back to the numeric axis it drew before T123.
    /// </summary>
    IReadOnlyList<TrajectoryRungDto> Rungs,
    IReadOnlyList<TrajectoryPointDto> Points)
{
    /// <summary>
    /// The EPA's exit level on the pinned ladder: the trainee's curriculum item's flat minimum, which the entrustment
    /// standing's exit rule reads (T166; T355, C10). Null when the trainee's curriculum has no item for the EPA.
    /// </summary>
    public int? ExitLevelOrder { get; init; }

    /// <inheritdoc cref="ExitLevelOrder" />
    public string? ExitLevelLabel { get; init; }

    /// <summary>
    /// The item's minimum across the window, one step from each day the training year changes inside it
    /// (<see cref="TraineeProfile.StageOn" />, 365-day blocks: 2026-01-14 for the cast), the first from the window's first
    /// day, or the programme's start when the window opens before it: before the start no minimum applies, so none is
    /// drawn there (T355, build review R2). The chart's stepped edge (T355, R4). Empty when the trainee's curriculum has no
    /// item for the EPA, the trainee has no profile, or the window ends before the programme starts.
    /// </summary>
    public IReadOnlyList<TrajectoryMinimumStepDto> MinimumSteps { get; init; } = [];

    /// <summary>The window read: the request's From, or null when it named none.</summary>
    public DateOnly? WindowFrom { get; init; }

    /// <summary>The window read: the request's To, or null when it named none.</summary>
    public DateOnly? WindowTo { get; init; }
}

/// <summary>
/// The item's minimum level from <paramref name="From" /> on (T355, R4): the level each encounter from that day is judged
/// against, for <paramref name="TrainingYear" />. No step starts before the programme does (T355, build review R2), so the
/// year is always known; it stays nullable for the shape the chart reads.
/// </summary>
public sealed record TrajectoryMinimumStepDto(DateOnly From, int? TrainingYear, int MinimumOrder, string MinimumLabel);

/// <summary>
/// A rating against the minimum that applied when it was observed (T355, E2), computed live with
/// <see cref="EntrustmentLevelComparer" />, the comparer credit uses.
/// </summary>
public enum TrajectoryAgainstMinimum
{
    /// <summary>At or above the training year's minimum, on the item's ladder.</summary>
    AtOrAbove,

    /// <summary>Below it.</summary>
    Below,

    /// <summary>Rated on another ladder (<see cref="LevelComparisonBasis.ScaleMismatch" />): counts towards the number, not the level.</summary>
    NotComparable,

    /// <summary>
    /// No minimum judges this rating: the trainee's curriculum holds none for this EPA, the encounter is before the
    /// programme's start, or no directive of the activity's pinned credit rules that credits the item names a minimum
    /// level, so credit counts it with no level judgement (T355, build review R2, R3).
    /// </summary>
    NotGated
}

/// <summary>
/// One rung of the ladder a trajectory is plotted against. Order is the stored ordinal and the position
/// on the axis; Label is the rung as the College prints it. On the CPSA v11.1 ladder they are different
/// numbers — Order 5 is rung "4" — which is why the chart cannot label its own axis from ordinals. (T100)
/// </summary>
public sealed record TrajectoryRungDto(int Order, string Label);

public sealed record TrajectoryPointDto(
    int ActivityId,
    DateOnly ObservedOn,
    /// <summary>
    /// False when nobody stated when the encounter happened and <see cref="ObservedOn" /> is only the day the activity
    /// was created (<c>ObservedOnSource == CreatedOn</c>). The point still sits there, the same date the window selects
    /// on, but the chart must not present it as a clinical date (T161, D28; <c>EncounterDate.Label</c>). No default,
    /// so a new call site cannot silently report every point as dated.
    /// </summary>
    bool ObservedOnDeclared,
    int Rating,
    /// <summary>
    /// Rating rendered as a rung on the EPA's pinned scale, or the bare ordinal when it resolves to
    /// nothing — including when the ordinal is not a rung on that ladder at all, which is how a rating
    /// recorded against a different scale shows up here.
    /// </summary>
    string RatingLabel,
    string Source,
    string AssessorUserId,
    /// <summary>
    /// The rating was recorded against a DIFFERENT ladder from the one this EPA's axis is drawn from,
    /// so its ordinal does not mean on this chart what it meant on the form. Drawn hollow and left out
    /// of the polyline. (T123 D30, made computable by T126)
    /// </summary>
    /// <remarks>
    /// False is not "on the ladder" — it is "no disagreement established". It covers a point whose own
    /// ladder is unknown, which is the common case while four generic seeds still declare the
    /// unresolvable <c>or_scale</c> (T110) and while types published before T126 carry no
    /// <c>rated_level_field</c>. Marking those would assert a conflict nothing has shown.
    /// </remarks>
    bool OffLadder = false)
{
    /// <summary>The assessor's name, as the user store holds it; the id where there is none (T355, C10).</summary>
    public string AssessorName { get; init; } = string.Empty;

    /// <summary>
    /// The activity's name as My activities' row reads it, "Mini-CEX (Paediatrics) · PAED-001 · 2026-09-21", with its
    /// nominee added where another of the subject's activities shares the rest (E7, counted over the subject's whole list,
    /// not the window): the chart's table links the activity by it (T355, C10).
    /// </summary>
    public string ActivityName { get; init; } = string.Empty;

    /// <summary>The training year at the encounter (<see cref="TraineeProfile.StageOn" />); null before the programme starts or with no profile.</summary>
    public int? TrainingYear { get; init; }

    /// <summary>The minimum the encounter was judged against, on the item's ladder ("5"); null when <see cref="AgainstMinimum" /> is NotGated.</summary>
    public string? MinimumLabel { get; init; }

    /// <summary>
    /// The rating against <see cref="MinimumLabel" />, computed live with the comparer credit uses (T355, E2):
    /// <c>EntrustmentLevelComparer.Compare(rating, its own ladder, the item's minimum for <see cref="TrainingYear" />, the
    /// item's ladder)</c>.
    /// </summary>
    public TrajectoryAgainstMinimum AgainstMinimum { get; init; } = TrajectoryAgainstMinimum.NotGated;

    /// <summary>The name of the ladder the rating was recorded on, when it is another (<see cref="OffLadder" />): C11's "[scale]".</summary>
    public string? OtherScaleName { get; init; }

    /// <summary>
    /// The rating as a rung of its own ladder, when that is another (<see cref="OffLadder" />): C11's "[rating]"
    /// ("Independent"). <see cref="RatingLabel" /> stays the ordinal read on the axis's ladder.
    /// </summary>
    public string? OtherScaleRatingLabel { get; init; }
}

public sealed class GetEpaTrajectoryForTraineeQueryHandler
    : IRequestHandler<GetEpaTrajectoryForTraineeQuery, IReadOnlyList<EpaTrajectoryDto>>
{
    // Which types are rated, and what evidence they are, now has ONE answer for the whole
    // repository: RatedActivityTypes (T134). This file used to keep its own family map and the
    // committee sampling report kept a second, EXACT-key one — which is why every seeded CPSA tool
    // charted here and counted as no evidence at all there. Both maps are gone; the surviving one
    // moved into the shared classifier unchanged.
    //
    // The KNOWN LIMITATION this carried for months — "an institution that builds its own rated tool
    // under an unfamiliar key will not chart" — is CLOSED. The gate is now the type's own declared
    // rated field (T126), so any rated tool charts whatever it is called. Its evidence SOURCE is the
    // category of the instrument it declares (WbaToolKey, T144), so a builder-made Mini-CEX under an
    // unfamiliar key reads as Direct observation. A type reads as its raw key only when its instrument
    // has no category, or when it declares none and its key matches no known family.

    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService? _users;

    /// <param name="users">
    /// The user store, for the assessors' names and the activities' names (T355, C10). Null reads neither: the portfolio
    /// export calls this handler directly for its counts and prints no name from it, so each point's
    /// <see cref="TrajectoryPointDto.AssessorName" /> is then the assessor's id and its <see cref="TrajectoryPointDto.ActivityName" />
    /// empty. Every request sent through MediatR has the store.
    /// </param>
    public GetEpaTrajectoryForTraineeQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService? users = null)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<IReadOnlyList<EpaTrajectoryDto>> Handle(
        GetEpaTrajectoryForTraineeQuery request,
        CancellationToken cancellationToken)
    {
        var traineeUserId = request.TraineeUserId.Trim();

        var readable = _dbContext.Set<Activity>()
            .AsNoTracking()
            .Where(activity => activity.SubjectUserId == traineeUserId)
            // The trainee id is whatever the caller asked about, so the rows are cut down to what
            // this caller may read: a trainee asking about themselves matches on SubjectUserId,
            // anyone else must oversee the programme each activity is stamped to. Out of scope
            // draws an empty chart rather than raising — a refusal would confirm the trainee. (T101)
            .WhereReadableBy(request.Principal);

        // The window selects on the ENCOUNTER date, not the filing date, so an assessment of a March
        // encounter filed in September is inside a March window and outside a September one. (T119)
        //
        // The bounds stay DateOnly and compare against a DateOnly column rather than being widened to
        // UTC instants: it keeps the index on ObservedOn usable, and it removes the boundary defect the
        // instants had — an activity created 00:30 SAST fell on the previous UTC day and dropped out of
        // a window that began that morning.
        var query = readable;
        if (request.From.HasValue)
        {
            var from = request.From.Value;
            query = query.Where(activity => activity.ObservedOn >= from);
        }

        if (request.To.HasValue)
        {
            var to = request.To.Value;
            query = query.Where(activity => activity.ObservedOn <= to);
        }

        // One EPA's page reads that EPA's ratings alone, by the stamped column the chart groups on (T137; T355, C10).
        if (request.EpaId is { } epaFilter)
        {
            query = query.Where(activity => activity.EpaId == epaFilter);
        }

        // What a rating is, and which rows hold one, is AttributedRatings' answer, shared with the entrustment
        // standing (T166): a terminal state of the pinned workflow (D44), the pinned schema's declared rated field, the
        // stamped EPA (T137), and the assessor RatedEvidenceProfile names (T135, T150). The rows it reads are the ones
        // this handler confined above, so the read rule is applied here, where the boundary test can see it.
        var ratings = await AttributedRatings.ReadAsync(_dbContext, query, cancellationToken);

        // The x-axis is the encounter date the clinician stated, which is what this chart has always claimed to plot
        // and never did: it plotted CreatedOn, the audit clock. (T119) Where nobody stated one (ObservedOnSource ==
        // CreatedOn) the point still sits on the day it was created, and says so: the chart's tooltip and table mark it
        // as undated evidence (T161, D28, T119 D4). RatingLabel is filled in below, once the EPA's pinned ladder is known.
        var rawPoints = ratings
            .Select(rating => (rating.EpaId, Point: new TrajectoryPointDto(
                rating.ActivityId,
                rating.ObservedOn,
                rating.ObservedOnDeclared,
                rating.Rating,
                rating.Rating.ToString(),
                rating.Source,
                rating.AssessorUserId)))
            .ToList();
        var ratedScaleIdByActivity = ratings
            .Where(rating => rating.RatedScaleId.HasValue)
            .ToDictionary(rating => rating.ActivityId, rating => rating.RatedScaleId!.Value);

        if (rawPoints.Count == 0)
        {
            return Array.Empty<EpaTrajectoryDto>();
        }

        var epaIds = rawPoints.Select(entry => entry.EpaId).Distinct().ToArray();
        var epas = await _dbContext.Set<Epa>()
            .AsNoTracking()
            .Where(epa => epaIds.Contains(epa.Id))
            .ToDictionaryAsync(epa => epa.Id, cancellationToken);

        var ladders = await ResolvePinnedLaddersAsync(
            traineeUserId, epaIds, ratedScaleIdByActivity.Values, cancellationToken);

        // What credit judges each rating's level by: its pinned credit rules' directives (T355, build review R3).
        var gates = await ReadCreditGatesAsync(
            readable, ratings.Select(rating => rating.ActivityId).ToArray(), cancellationToken);

        // T355 (C10, E7): each rating's activity named as My activities names its row, and each assessor by name, in one
        // lookup. The names are taken over everything of the subject's this caller may read, not the window, so a name
        // here is the name the row has on My activities.
        var (activityNames, people) = _users is null
            ? (new Dictionary<int, string>(), null)
            : await NameRowsAsync(
                _users, readable, ratings.Select(rating => rating.ActivityId).ToArray(),
                ratings.Select(rating => rating.AssessorUserId), request.Principal, cancellationToken);

        return rawPoints
            .GroupBy(entry => entry.EpaId)
            .Where(group => epas.ContainsKey(group.Key))
            .Select(group =>
            {
                var epa = epas[group.Key];
                ladders.ItemByEpa.TryGetValue(epa.Id, out var item);
                var scaleId = item?.ScaleId;
                var points = group
                    .Select(entry =>
                    {
                        // Off-ladder is asserted only when BOTH sides are known and they disagree. An
                        // unresolvable point key (or_scale, T110) or a type predating T126 leaves it
                        // alone: a hollow dot says "this was measured on something else", and saying so
                        // without evidence is worse than the numeric axis this chart already drew.
                        int? ownScaleId = ratedScaleIdByActivity.TryGetValue(entry.Point.ActivityId, out var own) ? own : null;
                        var offLadder = scaleId is not null && ownScaleId is not null && ownScaleId != scaleId.Value;

                        var trainingYear = ladders.ProgrammeStart is { } start
                            ? TraineeProfile.StageOn(start, entry.Point.ObservedOn)
                            : null;
                        var (against, minimumLabel) = AgainstMinimumThen(
                            item,
                            gates.TryGetValue(entry.Point.ActivityId, out var directives) ? directives : [],
                            trainingYear,
                            ladders.Rungs);

                        return entry.Point with
                        {
                            RatingLabel = ladders.Rungs.Format(scaleId, entry.Point.Rating),
                            OffLadder = offLadder,
                            AssessorName = people?.NameOf(entry.Point.AssessorUserId) ?? entry.Point.AssessorUserId,
                            ActivityName = activityNames.TryGetValue(entry.Point.ActivityId, out var name)
                                ? name
                                : string.Empty,
                            TrainingYear = trainingYear,
                            MinimumLabel = minimumLabel,
                            AgainstMinimum = against,
                            OtherScaleName = offLadder
                                ? ladders.ScaleNameById.TryGetValue(ownScaleId!.Value, out var otherName)
                                    ? otherName
                                    : $"scale {ownScaleId.Value}"
                                : null,
                            OtherScaleRatingLabel = offLadder ? ladders.Rungs.Format(ownScaleId, entry.Point.Rating) : null
                        };
                    })
                    .OrderBy(point => point.ObservedOn)
                    .ThenBy(point => point.ActivityId)
                    .ToArray();

                var rungs = ladders.Rungs.RungsOf(scaleId)
                    .Select(rung => new TrajectoryRungDto(rung.Order, rung.Label))
                    .ToArray();

                return new EpaTrajectoryDto(
                    epa.Id,
                    epa.Code,
                    epa.Title,
                    // T255. In force now, by the rule the activity's own picker labels by (EpaOptionLabel).
                    epa.IsActive,
                    scaleId,
                    scaleId.HasValue && ladders.ScaleNameById.TryGetValue(scaleId.Value, out var scaleName)
                        ? scaleName
                        : null,
                    rungs,
                    points)
                {
                    ExitLevelOrder = item?.MinimumLevelOrder,
                    ExitLevelLabel = item is null ? null : ladders.Rungs.Format(scaleId, item.MinimumLevelOrder),
                    MinimumSteps = item is not null && ladders.ProgrammeStart is { } programmeStart
                        ? MinimumSteps(
                            item,
                            programmeStart,
                            request.From ?? points[0].ObservedOn,
                            request.To ?? points[^1].ObservedOn,
                            ladders.Rungs)
                        : [],
                    WindowFrom = request.From,
                    WindowTo = request.To
                };
            })
            .OrderBy(dto => dto.EpaCode, StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>
    /// A rating against the minimum that applied when it was observed (T355, E2), as credit judged it: the item's minimum
    /// for the training year at the encounter (<see cref="CurriculumItem.GetMinimumLevelForStage" />), compared by
    /// <see cref="EntrustmentLevelComparer.Compare" /> on the value and ladder the crediting directive names, exactly as
    /// <c>CreditApplier.CompareMinimumLevel</c> compares it. Computed live, not read from the stored tally: no outcome per
    /// activity is stored, and a rebuild already clears any divergence (E2).
    /// </summary>
    /// <remarks>
    /// NotGated where credit makes no level judgement: no item, or no directive crediting the item names a
    /// <c>minimum_level_field</c> or a <c>minimum_level_fixed</c>, so credit counts it by volume alone (T355, build review
    /// R3); and before the programme starts, where no training year's minimum applies and the chart draws none (R2).
    /// </remarks>
    private static (TrajectoryAgainstMinimum Against, string? MinimumLabel) AgainstMinimumThen(
        CurriculumItem? item, IReadOnlyList<CreditGate> directives, int? trainingYear, EntrustmentRungLookup rungs)
    {
        if (item is null || trainingYear is null)
        {
            return (TrajectoryAgainstMinimum.NotGated, null);
        }

        // The first directive that credits this item and gates on a level: the seeded types each have one.
        var gate = directives.FirstOrDefault(directive => directive.Gates && directive.Targets(item));
        if (gate is null)
        {
            return (TrajectoryAgainstMinimum.NotGated, null);
        }

        var minimum = item.GetMinimumLevelForStage(trainingYear);
        if (minimum <= 0)
        {
            return (TrajectoryAgainstMinimum.NotGated, null);
        }

        // A gating directive whose field holds no level: credit counts it as not reaching the minimum (ValueMissing).
        var comparison = gate.Level is { } level
            ? EntrustmentLevelComparer.Compare(level, gate.LevelScaleId, minimum, item.ScaleId)
            : new LevelComparison(false, LevelComparisonBasis.ValueMissing);
        var against = comparison.Basis == LevelComparisonBasis.ScaleMismatch
            ? TrajectoryAgainstMinimum.NotComparable
            : comparison.MinimumMet
                ? TrajectoryAgainstMinimum.AtOrAbove
                : TrajectoryAgainstMinimum.Below;

        return (against, rungs.Format(item.ScaleId, minimum));
    }

    /// <summary>
    /// The item's minimum across <paramref name="from" /> to <paramref name="to" />: a step from the first day (or the
    /// programme's start, when the window opens before it: no minimum applies before then, R2), then one on each day the
    /// training year changes (<see cref="TraineeProfile.StageOn" />, whole 365-day blocks from the programme start), so
    /// the chart's dashed edge steps where the minimum did (T355, R4).
    /// </summary>
    private static IReadOnlyList<TrajectoryMinimumStepDto> MinimumSteps(
        CurriculumItem item, DateOnly programmeStart, DateOnly from, DateOnly to, EntrustmentRungLookup rungs)
    {
        var steps = new List<TrajectoryMinimumStepDto>();
        for (var day = from > programmeStart ? from : programmeStart; day <= to;)
        {
            var stage = TraineeProfile.StageOn(programmeStart, day)!.Value;
            var minimum = item.GetMinimumLevelForStage(stage);
            steps.Add(new TrajectoryMinimumStepDto(day, stage, minimum, rungs.Format(item.ScaleId, minimum)));

            day = programmeStart.AddDays(365 * stage);
        }

        return steps;
    }

    /// <summary>
    /// One <c>counts_for</c> directive of a rating's pinned credit rules, read against the activity's data as credit reads
    /// it (T355, build review R3): what it targets (<c>CreditTargetResolver.DescribeTarget</c>'s precedence: a literal
    /// item, else an item field, else an EPA field), whether it gates on a level, and the level and ladder it compares.
    /// </summary>
    private sealed record CreditGate(int? ItemId, int? EpaId, bool Gates, int? Level, int? LevelScaleId)
    {
        public bool Targets(CurriculumItem item)
            => ItemId is { } itemId ? item.Id == itemId : EpaId is { } epaId && item.EpaId == epaId;
    }

    /// <summary>
    /// Each rated activity's credit directives, from its PINNED version's credit rules and schema (T355, build review R3),
    /// read as <c>CreditApplier.PlanAsync</c> reads them: the level is the <c>minimum_level_field</c>'s value (or the
    /// literal <c>minimum_level_fixed</c>, which has no ladder), on the ladder that field's <c>scale_key</c> binds
    /// (<see cref="EntrustmentScaleBindings.ResolveAsync" />, the resolver credit uses). A version whose rules or schema do
    /// not parse credits nothing, so its ratings are judged by no minimum.
    /// </summary>
    private async Task<IReadOnlyDictionary<int, IReadOnlyList<CreditGate>>> ReadCreditGatesAsync(
        IQueryable<Activity> readable, int[] activityIds, CancellationToken cancellationToken)
    {
        var rows = await readable
            .Where(activity => activityIds.Contains(activity.Id))
            .Select(activity => new { activity.Id, activity.ActivityTypeId, activity.SchemaVersion, activity.DataJson })
            .ToListAsync(cancellationToken);

        var typeIds = rows.Select(row => row.ActivityTypeId).Distinct().ToArray();
        var versions = await _dbContext.Set<ActivityTypeVersion>()
            .AsNoTracking()
            .Where(version => typeIds.Contains(version.ActivityTypeId))
            .Select(version => new { version.ActivityTypeId, version.Version, version.SchemaJson, version.CreditRulesJson })
            .ToListAsync(cancellationToken);

        var rulesByPin = new Dictionary<(int, int), (CreditRules Rules, Dictionary<string, string> ScaleKeys)>();
        foreach (var version in versions)
        {
            try
            {
                var rules = CreditRulesParser.Parse(version.CreditRulesJson);
                var gatedFields = rules.CountsFor
                    .Select(directive => directive.MinimumLevelField)
                    .Where(key => !string.IsNullOrWhiteSpace(key))
                    .ToHashSet(StringComparer.Ordinal);
                var scaleKeys = gatedFields.Count == 0
                    ? new Dictionary<string, string>(StringComparer.Ordinal)
                    : FormSchemaParser.Parse(version.SchemaJson).Sections
                        .SelectMany(section => section.Fields)
                        .Where(field => gatedFields.Contains(field.Key) && !string.IsNullOrWhiteSpace(field.ScaleKey))
                        .GroupBy(field => field.Key, StringComparer.Ordinal)
                        .ToDictionary(group => group.Key, group => group.First().ScaleKey!, StringComparer.Ordinal);
                rulesByPin[(version.ActivityTypeId, version.Version)] = (rules, scaleKeys);
            }
            catch (Exception exception) when (exception is CreditRulesParseException or SchemaParseException)
            {
                // Credit cannot read these rules either: the rating is judged by no minimum.
            }
        }

        var scaleIdByKey = await EntrustmentScaleBindings.ResolveAsync(
            _dbContext, rulesByPin.Values.SelectMany(entry => entry.ScaleKeys.Values), cancellationToken);

        var gates = new Dictionary<int, IReadOnlyList<CreditGate>>(rows.Count);
        foreach (var row in rows)
        {
            if (!rulesByPin.TryGetValue((row.ActivityTypeId, row.SchemaVersion), out var pinned))
            {
                continue;
            }

            using var document = JsonDocument.Parse(row.DataJson);
            var data = document.RootElement;
            var directives = new List<CreditGate>(pinned.Rules.CountsFor.Count);
            foreach (var directive in pinned.Rules.CountsFor)
            {
                var match = directive.CurriculumItemMatchRule;
                int? itemId = match.CurriculumItemId
                    ?? (TryReadInt(data, match.CurriculumItemField, out var fromItemField) ? fromItemField : null);
                int? epaId = itemId is null && TryReadInt(data, match.EpaField, out var fromEpaField) ? fromEpaField : null;

                var gatesOnField = !string.IsNullOrWhiteSpace(directive.MinimumLevelField);
                var gatesOnFixed = !string.IsNullOrWhiteSpace(directive.MinimumLevelFixed);
                int? level = null;
                int? levelScaleId = null;
                if (gatesOnField && TryReadInt(data, directive.MinimumLevelField, out var provided))
                {
                    level = provided;
                    levelScaleId = pinned.ScaleKeys.TryGetValue(directive.MinimumLevelField!, out var scaleKey)
                        && scaleIdByKey.TryGetValue(scaleKey.Trim(), out var scaleId)
                            ? scaleId
                            : null;
                }
                else if (gatesOnFixed
                    && int.TryParse(directive.MinimumLevelFixed, CultureInfo.InvariantCulture, out var fixedLevel))
                {
                    level = fixedLevel;
                }

                directives.Add(new CreditGate(itemId, epaId, gatesOnField || gatesOnFixed, level, levelScaleId));
            }

            gates[row.Id] = directives;
        }

        return gates;
    }

    /// <summary>A whole-number field of the data, a number or a numeric string, as <c>CreditTargetResolver.TryGetInt32</c> reads it.</summary>
    private static bool TryReadInt(JsonElement data, string? fieldKey, out int value)
    {
        value = default;
        if (data.ValueKind != JsonValueKind.Object || string.IsNullOrWhiteSpace(fieldKey)
            || !data.TryGetProperty(fieldKey, out var property))
        {
            return false;
        }

        return property.ValueKind == JsonValueKind.Number
            ? property.TryGetInt32(out value)
            : property.ValueKind == JsonValueKind.String
                && int.TryParse(property.GetString(), CultureInfo.InvariantCulture, out value);
    }

    /// <summary>
    /// Each rated activity's name, as <c>ListActivitiesBySubjectQuery</c> names My activities' row, and the people named,
    /// in one lookup (T355, C10, E7): <see cref="ActivityRowDetails.ResolveAsync" /> over the rows, with the nominee added
    /// where another of the subject's activities in <paramref name="readable" /> shares the rest.
    /// </summary>
    private async Task<(IReadOnlyDictionary<int, string> Names, UserDisplayNames? People)> NameRowsAsync(
        IUserAdministrationService users,
        IQueryable<Activity> readable,
        int[] activityIds,
        IEnumerable<string> assessorUserIds,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        var rows = await (
                from activity in readable
                where activityIds.Contains(activity.Id)
                join epa in _dbContext.Set<Epa>() on activity.EpaId equals (int?)epa.Id into stamped
                from epa in stamped.DefaultIfEmpty()
                select new
                {
                    activity.Id,
                    activity.ActivityTypeId,
                    activity.SchemaVersion,
                    TypeName = activity.ActivityType.Name,
                    activity.SubjectUserId,
                    activity.CreatedByUserId,
                    activity.CurrentState,
                    activity.EpaId,
                    EpaCode = epa == null ? null : epa.Code,
                    activity.ObservedOn,
                    ObservedOnDeclared = activity.ObservedOnSource == ObservationDateSource.Declared,
                    activity.DataJson,
                    LastMove = activity.Transitions
                        .OrderByDescending(transition => transition.OccurredOn)
                        .ThenByDescending(transition => transition.Id)
                        .Select(transition => new
                        {
                            transition.FromState,
                            transition.ToState,
                            transition.ActorUserId,
                            transition.OccurredOn,
                            transition.Note,
                            transition.TransitionKey
                        })
                        .FirstOrDefault()
                })
            .ToListAsync(cancellationToken);

        var facts = rows
            .Select(row => new ActivityRowFacts(
                row.Id,
                row.ActivityTypeId,
                row.SchemaVersion,
                row.TypeName,
                row.SubjectUserId,
                row.CreatedByUserId,
                row.CurrentState,
                row.EpaId,
                row.EpaCode,
                row.ObservedOn,
                row.ObservedOnDeclared,
                row.DataJson,
                row.LastMove is { } last
                    ? new ActivityLastMove(
                        last.FromState, last.ToState, last.ActorUserId, last.OccurredOn, last.Note, last.TransitionKey)
                    : null))
            .ToList();

        var shared = await ActivityRowDetails.SharedKeysAsync(readable, facts, cancellationToken);
        var forms = await PinnedForms.LoadAsync(
            _dbContext, facts.Select(fact => (fact.ActivityTypeId, fact.SchemaVersion)), cancellationToken);
        var (details, people) = await ActivityRowDetails.ResolveAsync(
            facts,
            fact => forms[(fact.ActivityTypeId, fact.SchemaVersion)],
            fact => shared.Contains(fact.CollisionKey),
            principal,
            users,
            cancellationToken,
            alsoNamed: assessorUserIds);

        return (details.ToDictionary(entry => entry.Key, entry => entry.Value.DisplayName), people);
    }

    private sealed record PinnedLadders(
        Dictionary<int, CurriculumItem> ItemByEpa,
        DateOnly? ProgrammeStart,
        Dictionary<int, string> ScaleNameById,
        EntrustmentRungLookup Rungs);

    /// <summary>
    /// The entrustment ladder each EPA's ratings should be read against, for this trainee.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The source is <c>CurriculumItem.ScaleId</c> — T109's per-item pin, which is literally "the scale
    /// this EPA's minima are expressed on" for this trainee's curriculum. It is deliberately NOT
    /// <c>Curriculum.SubSpeciality.DefaultEntrustmentScaleId</c>, which two neighbouring queries do read
    /// and which <c>CurriculumItem</c>'s own remarks reject as a source of pins, because it is a
    /// programme-wide committee-picker default an administrator may change at any time (T187), not the
    /// ladder any one EPA's minima were written on.
    /// </para>
    /// <para>
    /// The profile is the trainee's preferred one (<see cref="TraineeScopeResolver.PreferredProfiles" />:
    /// the active one, else the highest id), NOT filtered on <c>IsActive</c> — the one pick the credit
    /// engine, the EPA picker and the export make (T185), so the chart and credit cannot disagree about
    /// which profile row is in force, and a graduated trainee still has a trajectory worth reading. Until
    /// T185 this broke ties by the latest <c>ProgrammeStartDate</c> with no final tie-break at all.
    /// <c>GetCurriculumProgressForTrainee</c>, the other query on the same page, reads the same profile
    /// but only while it is active: it reads today's targets, and a completed programme has none.
    /// </para>
    /// <para>
    /// Every step is permissive: no profile, no curriculum item for the EPA, or an unpinned item all
    /// yield no ladder, and the chart then renders exactly as it did before T123.
    /// </para>
    /// <para>
    /// What this does NOT do is resolve the ladder each plotted ACTIVITY was recorded against, which is
    /// what lets a point on a different scale be drawn as such. That is
    /// <see cref="AttributedRating.RatedScaleId" /> (T126), from the pinned version's declared rated field, the
    /// same field the rating itself is read from (<see cref="RatedEvidenceProfile" />, T135).
    /// </para>
    /// </remarks>
    private async Task<PinnedLadders> ResolvePinnedLaddersAsync(
        string traineeUserId,
        int[] epaIds,
        IEnumerable<int> ratedScaleIds,
        CancellationToken cancellationToken)
    {
        var none = new PinnedLadders([], null, [], EntrustmentRungLookup.Empty);

        var profile = await TraineeScopeResolver.PreferredProfiles(_dbContext)
            .AsNoTracking()
            .Where(entity => entity.UserId == traineeUserId)
            .Select(entity => new { entity.CurriculumId, entity.InstitutionId, entity.ProgrammeStartDate })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return none;
        }

        // The owner predicate is not optional. A national curriculum row is SHARED by every adopting
        // institution (AdoptCurriculumCommandHandler never clones it), and a national item never shares
        // an EPA with an institution's own (T223), so an institution-local item added by institution B
        // has no national row beside it for that EPA. Without this, a trainee at institution A would take B's ladder as
        // their chart's axis — and then have their own correctly-rated observations marked "not a rung
        // on this scale" against a ladder that was never theirs.
        //
        // T355 (E2): with the item's minima, so each rating is judged against the minimum of its own training year, and
        // the chart steps where that minimum did. Not filtered on the EPA being in force: a paused EPA still charts what
        // was recorded against it (T255), against the minimum that applied.
        var pins = await _dbContext.Set<CurriculumItem>()
            .AsNoTracking()
            .Where(item => item.CurriculumId == profile.CurriculumId
                && (item.OwningInstitutionId == null || item.OwningInstitutionId == profile.InstitutionId)
                && epaIds.Contains(item.EpaId))
            .OrderBy(item => item.Id)
            .Select(item => new { item.Id, item.EpaId, item.ScaleId, item.MinimumLevelOrder, item.MinimumLevelByStageJson })
            .ToListAsync(cancellationToken);

        if (pins.Count == 0)
        {
            return none with { ProgrammeStart = profile.ProgrammeStartDate };
        }

        var itemByEpa = new Dictionary<int, CurriculumItem>(pins.Count);
        foreach (var pin in pins)
        {
            itemByEpa.TryAdd(pin.EpaId, new CurriculumItem
            {
                Id = pin.Id,
                EpaId = pin.EpaId,
                ScaleId = pin.ScaleId,
                MinimumLevelOrder = pin.MinimumLevelOrder,
                MinimumLevelByStageJson = pin.MinimumLevelByStageJson
            });
        }

        // The pinned ladders, and the ladders the ratings were recorded on: a rating on another ladder is named on its
        // own rung and scale (C11's "[rating] on [scale]").
        var scaleIds = itemByEpa.Values
            .Select(item => item.ScaleId)
            .Concat(ratedScaleIds.Select(id => (int?)id))
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToArray();

        var rungs = await EntrustmentRungLabels.LoadAsync(
            _dbContext, scaleIds.Select(id => (int?)id), cancellationToken);

        var scaleNameById = scaleIds.Length == 0
            ? []
            : await _dbContext.Set<EntrustmentScale>()
                .AsNoTracking()
                .Where(scale => scaleIds.Contains(scale.Id))
                .ToDictionaryAsync(scale => scale.Id, scale => scale.Name, cancellationToken);

        return new PinnedLadders(itemByEpa, profile.ProgrammeStartDate, scaleNameById, rungs);
    }
}
