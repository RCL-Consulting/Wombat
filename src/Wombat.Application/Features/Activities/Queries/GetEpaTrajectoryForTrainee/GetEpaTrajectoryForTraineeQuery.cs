using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Activities;
using Wombat.Domain.Epas;

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
    IReadOnlyList<TrajectoryPointDto> Points);

public sealed record TrajectoryPointDto(
    int ActivityId,
    DateOnly ObservedOn,
    int Rating,
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

        if (request.From.HasValue)
        {
            var fromUtc = request.From.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(activity => activity.CreatedOn >= fromUtc);
        }

        if (request.To.HasValue)
        {
            var toUtcExclusive = request.To.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(activity => activity.CreatedOn < toUtcExclusive);
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

            var observedOn = DateOnly.FromDateTime(activity.CreatedOn);
            rawPoints.Add((epaId, new TrajectoryPointDto(activity.Id, observedOn, rating, source, assessorUserId)));
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

        return rawPoints
            .GroupBy(entry => entry.EpaId)
            .Where(group => epas.ContainsKey(group.Key))
            .Select(group =>
            {
                var epa = epas[group.Key];
                var points = group
                    .Select(entry => entry.Point)
                    .OrderBy(point => point.ObservedOn)
                    .ThenBy(point => point.ActivityId)
                    .ToArray();
                return new EpaTrajectoryDto(epa.Id, epa.Code, epa.Title, points);
            })
            .OrderBy(dto => dto.EpaCode, StringComparer.Ordinal)
            .ToArray();
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
