using System.Globalization;
using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Activities;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;

public sealed record GetEpaTrajectoryForTraineeQuery(
    string TraineeUserId,
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
    private static readonly IReadOnlyDictionary<string, string> SourceByActivityKey =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["mini_cex"] = "Direct observation",
            ["dops"] = "Direct observation",
            ["cbd"] = "Conversation",
            ["acat"] = "Conversation"
        };

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
            .Where(activity => activity.SubjectUserId == traineeUserId);

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
            if (!SourceByActivityKey.TryGetValue(activity.ActivityType.Key, out var source))
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

            if (!TryGetInt32(document.RootElement, "overall", out rating) || rating < 1 || rating > 5)
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
