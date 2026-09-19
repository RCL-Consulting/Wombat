using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Activities;
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
public sealed record GetEpaTrajectoryForTraineeQuery(
    string TraineeUserId,
    ClaimsPrincipal Principal,
    DateOnly? From = null,
    DateOnly? To = null) : IRequest<IReadOnlyList<EpaTrajectoryDto>>;

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
    IReadOnlyList<TrajectoryPointDto> Points);

/// <summary>
/// One rung of the ladder a trajectory is plotted against. Order is the stored ordinal and the position
/// on the axis; Label is the rung as the College prints it. On the CPSA v11.1 ladder they are different
/// numbers — Order 5 is rung "4" — which is why the chart cannot label its own axis from ordinals. (T100)
/// </summary>
public sealed record TrajectoryRungDto(int Order, string Label);

public sealed record TrajectoryPointDto(
    int ActivityId,
    DateOnly ObservedOn,
    int Rating,
    /// <summary>
    /// Rating rendered as a rung on the EPA's pinned scale, or the bare ordinal when it resolves to
    /// nothing — including when the ordinal is not a rung on that ladder at all, which is how a rating
    /// recorded against a different scale shows up here.
    /// </summary>
    string RatingLabel,
    string Source,
    string AssessorUserId);

public sealed class GetEpaTrajectoryForTraineeQueryHandler
    : IRequestHandler<GetEpaTrajectoryForTraineeQuery, IReadOnlyList<EpaTrajectoryDto>>
{
    // An entrustment scale is data, so the rung count is not knowable here. This bound exists
    // only to reject a value that cannot be a rung at all (a mis-mapped field, a year, a score
    // out of 100) without re-introducing a hard-coded ceiling that silently hides observations.
    private const int MaxPlausibleRung = 20;

    // Schema-driven activity types carry institution-specific keys (e.g. "mini_cex_paed"),
    // so we match a known assessment family either exactly or as a "<base>_..." prefix rather
    // than against a fixed set of literal keys.
    //
    // Membership is deliberate: this is an ENTRUSTMENT trajectory, so only assessor-rated
    // assessment tools belong on it. Trainee-authored types (reflective notes, logbook entries)
    // are excluded even when they happen to carry a numeric field.
    //
    // Extended for the paediatric v11.1 tool set (T098). Tools named by v11.1 but not yet seeded
    // as activity types are listed here so they chart the moment they are created.
    //
    // KNOWN LIMITATION: this is still a hard-coded list, so an institution that builds its own
    // rated tool through the Activity builder under an unfamiliar key will not chart. The real
    // fix is a flag on ActivityType marking it as producing an entrustment rating — recorded as
    // a gap in Tasks/T098-epa-v11-adoption.md rather than guessed at here.
    private static readonly IReadOnlyDictionary<string, string> SourceByActivityFamily =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["mini_cex"] = "Direct observation",
            ["dops"] = "Direct observation",
            ["direct_observation"] = "Direct observation",
            ["observed_clinical_exam"] = "Direct observation",
            ["cbd"] = "Conversation",
            ["acat"] = "Conversation",
            ["cca"] = "Case analysis",
            ["rca"] = "Case analysis",
            ["case_note_review"] = "Case analysis",
            ["chart_stimulated_recall"] = "Conversation"
        };

    private static bool TryResolveSource(string activityTypeKey, out string source)
    {
        foreach (var (family, label) in SourceByActivityFamily)
        {
            if (activityTypeKey == family ||
                activityTypeKey.StartsWith(family + "_", StringComparison.Ordinal))
            {
                source = label;
                return true;
            }
        }

        source = string.Empty;
        return false;
    }

    private readonly IApplicationDbContext _dbContext;

    public GetEpaTrajectoryForTraineeQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EpaTrajectoryDto>> Handle(
        GetEpaTrajectoryForTraineeQuery request,
        CancellationToken cancellationToken)
    {
        var traineeUserId = request.TraineeUserId.Trim();

        var query = _dbContext.Set<Activity>()
            .AsNoTracking()
            .Include(activity => activity.ActivityType)
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

        var activities = await query.ToListAsync(cancellationToken);

        var rawPoints = new List<(int EpaId, TrajectoryPointDto Point)>();
        foreach (var activity in activities)
        {
            if (!TryResolveSource(activity.ActivityType.Key, out var source))
            {
                continue;
            }

            if (!TryParseObservation(activity.DataJson, out var epaId, out var rating, out var assessorUserId))
            {
                continue;
            }

            // The x-axis is the encounter date the clinician stated, which is what this chart has always
            // claimed to plot and never did — it plotted CreatedOn, the audit clock. (T119)
            //
            // NOT exposed here: activity.ObservedOnSource. When it is CreatedOn, nobody stated a date and
            // this point is sitting on the filing date, which the chart presents as though it were a
            // clinical fact. Marking those as undated evidence is T119 decision D4, deliberately left to a
            // follow-up so it lands with T100's neighbouring label fixes rather than ahead of them.
            var observedOn = activity.ObservedOn;
            // RatingLabel is filled in below, once the EPA's pinned ladder is known.
            rawPoints.Add((epaId, new TrajectoryPointDto(
                activity.Id, observedOn, rating, rating.ToString(), source, assessorUserId)));
        }

        if (rawPoints.Count == 0)
        {
            return Array.Empty<EpaTrajectoryDto>();
        }

        var epaIds = rawPoints.Select(entry => entry.EpaId).Distinct().ToArray();
        var epas = await _dbContext.Set<Epa>()
            .AsNoTracking()
            .Where(epa => epaIds.Contains(epa.Id))
            .ToDictionaryAsync(epa => epa.Id, cancellationToken);

        var ladders = await ResolvePinnedLaddersAsync(traineeUserId, epaIds, cancellationToken);

        return rawPoints
            .GroupBy(entry => entry.EpaId)
            .Where(group => epas.ContainsKey(group.Key))
            .Select(group =>
            {
                var epa = epas[group.Key];
                ladders.ScaleIdByEpa.TryGetValue(epa.Id, out var scaleId);
                var points = group
                    .Select(entry => entry.Point with
                    {
                        RatingLabel = ladders.Rungs.Format(scaleId, entry.Point.Rating)
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
                    scaleId,
                    scaleId.HasValue && ladders.ScaleNameById.TryGetValue(scaleId.Value, out var name)
                        ? name
                        : null,
                    rungs,
                    points);
            })
            .OrderBy(dto => dto.EpaCode, StringComparer.Ordinal)
            .ToArray();
    }

    private sealed record PinnedLadders(
        Dictionary<int, int?> ScaleIdByEpa,
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
    /// and which <c>CurriculumItem</c>'s own remarks reject as a source of pins, because
    /// <c>PaediatricCatalogueSeeder</c> force-overwrites it on every boot.
    /// </para>
    /// <para>
    /// The profile is resolved active-first, then by latest <c>ProgrammeStartDate</c>, and is NOT
    /// filtered on <c>IsActive</c> — the same rule as
    /// <c>ActivityReferenceDataService.ResolveCreditableEpaIdsAsync</c>, so the chart and the credit
    /// engine cannot disagree about which profile row is in force, and a graduated trainee still has a
    /// trajectory worth reading. This differs from <c>GetCurriculumProgressForTrainee</c>, the other
    /// query on the same page, which takes the active profile only; if those two are ever reconciled
    /// they should be reconciled towards this one.
    /// </para>
    /// <para>
    /// Every step is permissive: no profile, no curriculum item for the EPA, or an unpinned item all
    /// yield no ladder, and the chart then renders exactly as it did before T123.
    /// </para>
    /// <para>
    /// What this does NOT do is resolve the ladder each plotted ACTIVITY was recorded against, which is
    /// what would let a point on a different scale be drawn as such. That needs two things Wombat does
    /// not have: a way to know which schema field carried the rating (<c>TryParseObservation</c> reads
    /// the literal keys, it does not consult the schema) and a navigation from an activity to its pinned
    /// <c>ActivityTypeVersion</c>. Both are real work; see T126.
    /// </para>
    /// </remarks>
    private async Task<PinnedLadders> ResolvePinnedLaddersAsync(
        string traineeUserId,
        int[] epaIds,
        CancellationToken cancellationToken)
    {
        var none = new PinnedLadders([], [], EntrustmentRungLookup.Empty);

        var curriculumId = await _dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .Where(entity => entity.UserId == traineeUserId)
            .OrderByDescending(entity => entity.IsActive)
            .ThenByDescending(entity => entity.ProgrammeStartDate)
            .Select(entity => (int?)entity.CurriculumId)
            .FirstOrDefaultAsync(cancellationToken);

        if (curriculumId is null)
        {
            return none;
        }

        var pins = await _dbContext.Set<CurriculumItem>()
            .AsNoTracking()
            .Where(item => item.CurriculumId == curriculumId.Value && epaIds.Contains(item.EpaId))
            .Select(item => new { item.EpaId, item.ScaleId })
            .ToListAsync(cancellationToken);

        if (pins.Count == 0)
        {
            return none;
        }

        var scaleIdByEpa = new Dictionary<int, int?>(pins.Count);
        foreach (var pin in pins)
        {
            scaleIdByEpa[pin.EpaId] = pin.ScaleId;
        }

        var rungs = await EntrustmentRungLabels.LoadAsync(
            _dbContext, scaleIdByEpa.Values, cancellationToken);

        var pinnedScaleIds = scaleIdByEpa.Values
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToArray();

        var scaleNameById = pinnedScaleIds.Length == 0
            ? []
            : await _dbContext.Set<EntrustmentScale>()
                .AsNoTracking()
                .Where(scale => pinnedScaleIds.Contains(scale.Id))
                .ToDictionaryAsync(scale => scale.Id, scale => scale.Name, cancellationToken);

        return new PinnedLadders(scaleIdByEpa, scaleNameById, rungs);
    }

    private static bool TryParseObservation(string dataJson, out int epaId, out int rating, out string assessorUserId)
    {
        epaId = 0;
        rating = 0;
        assessorUserId = string.Empty;

        if (string.IsNullOrWhiteSpace(dataJson))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(dataJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (!TryGetInt32(document.RootElement, "epa_id", out epaId) || epaId <= 0)
            {
                return false;
            }

            // "overall" is the legacy seeded field name; schema-driven types built via the
            // visual builder name the overall rating "overall_level".
            //
            // The upper bound is deliberately NOT the old hard-coded 5. An entrustment scale is
            // data — the paediatric v11.1 ladder has six rungs (1, 2, 3a, 3b, 4, 5) — and a scale
            // with more rungs than the chart happened to assume must not make observations
            // disappear. Callers derive the axis maximum from the data; this guard only rejects
            // values that cannot be a rung at all.
            if ((!TryGetInt32(document.RootElement, "overall", out rating) &&
                 !TryGetInt32(document.RootElement, "overall_level", out rating)) ||
                rating < 1 || rating > MaxPlausibleRung)
            {
                return false;
            }

            if (!TryGetTrimmedString(document.RootElement, "assessor_user_id", out assessorUserId))
            {
                return false;
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryGetInt32(JsonElement root, string propertyName, out int value)
    {
        if (root.TryGetProperty(propertyName, out var property))
        {
            if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out value))
            {
                return true;
            }

            if (property.ValueKind == JsonValueKind.String &&
                int.TryParse(property.GetString(), CultureInfo.InvariantCulture, out value))
            {
                return true;
            }
        }

        value = 0;
        return false;
    }

    private static bool TryGetTrimmedString(JsonElement root, string propertyName, out string value)
    {
        if (root.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
        {
            var raw = property.GetString();
            if (!string.IsNullOrWhiteSpace(raw))
            {
                value = raw.Trim();
                return true;
            }
        }

        value = string.Empty;
        return false;
    }
}
