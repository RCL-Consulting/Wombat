using System.Globalization;
using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Activities;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record GetSamplingConcentrationWarningsQuery(int ReviewId)
    : IRequest<SamplingConcentrationReportDto>;

public sealed class GetSamplingConcentrationWarningsQueryValidator
    : AbstractValidator<GetSamplingConcentrationWarningsQuery>
{
    public GetSamplingConcentrationWarningsQueryValidator()
    {
        RuleFor(query => query.ReviewId).GreaterThan(0);
    }
}

public sealed record SamplingConcentrationReportDto(
    int ReviewId,
    int TotalRatedActivities,
    int DistinctAssessorCount,
    bool AnyWarning,
    IReadOnlyList<EpaSamplingConcentrationDto> PerEpa);

public sealed record EpaSamplingConcentrationDto(
    int EpaId,
    string EpaCode,
    string EpaTitle,
    int RatingCount,
    int DistinctAssessorCount,
    int DistinctSourceCount,
    string? DominantAssessorUserId,
    int DominantAssessorCount,
    bool OneAssessorOverHalf,
    bool SingleSource,
    bool FewerThanThreeAssessors);

public enum WbaSourceCategory
{
    DirectObservation = 1,
    Conversation = 2,
    LongitudinalObservation = 3,
    ProductEvaluation = 4
}

public sealed class GetSamplingConcentrationWarningsQueryHandler
    : IRequestHandler<GetSamplingConcentrationWarningsQuery, SamplingConcentrationReportDto>
{
    private static readonly IReadOnlyDictionary<string, WbaSourceCategory> SourceByActivityKey =
        new Dictionary<string, WbaSourceCategory>(StringComparer.Ordinal)
        {
            ["mini_cex"] = WbaSourceCategory.DirectObservation,
            ["dops"] = WbaSourceCategory.DirectObservation,
            ["cbd"] = WbaSourceCategory.Conversation,
            ["acat"] = WbaSourceCategory.Conversation
        };

    private readonly IApplicationDbContext _dbContext;

    public GetSamplingConcentrationWarningsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SamplingConcentrationReportDto> Handle(
        GetSamplingConcentrationWarningsQuery request,
        CancellationToken cancellationToken)
    {
        var review = await _dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == request.ReviewId, cancellationToken)
            ?? throw new InvalidOperationException("The committee review could not be found.");

        var fromUtc = review.ReviewPeriodFrom.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtcExclusive = review.ReviewPeriodTo.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var activities = await _dbContext.Set<Activity>()
            .AsNoTracking()
            .Include(activity => activity.ActivityType)
            .Where(activity =>
                activity.SubjectUserId == review.TraineeUserId &&
                activity.CreatedOn >= fromUtc &&
                activity.CreatedOn < toUtcExclusive)
            .ToListAsync(cancellationToken);

        var ratings = new List<(int EpaId, string AssessorUserId, WbaSourceCategory Source)>();
        foreach (var activity in activities)
        {
            if (!SourceByActivityKey.TryGetValue(activity.ActivityType.Key, out var source))
            {
                continue;
            }

            if (!TryParseRating(activity.DataJson, out var epaId, out var assessorUserId))
            {
                continue;
            }

            ratings.Add((epaId, assessorUserId, source));
        }

        var totalRated = ratings.Count;
        var distinctAssessorsOverall = ratings
            .Select(rating => rating.AssessorUserId)
            .Distinct(StringComparer.Ordinal)
            .Count();

        if (totalRated == 0)
        {
            return new SamplingConcentrationReportDto(
                review.Id,
                TotalRatedActivities: 0,
                DistinctAssessorCount: 0,
                AnyWarning: false,
                PerEpa: Array.Empty<EpaSamplingConcentrationDto>());
        }

        var epaIds = ratings.Select(rating => rating.EpaId).Distinct().ToArray();
        var epas = await _dbContext.Set<Epa>()
            .AsNoTracking()
            .Where(epa => epaIds.Contains(epa.Id))
            .ToDictionaryAsync(epa => epa.Id, cancellationToken);

        var perEpa = new List<EpaSamplingConcentrationDto>();
        foreach (var epaId in epaIds.OrderBy(id => id))
        {
            var epaRatings = ratings.Where(rating => rating.EpaId == epaId).ToArray();
            var ratingCount = epaRatings.Length;

            var assessorGroups = epaRatings
                .GroupBy(rating => rating.AssessorUserId, StringComparer.Ordinal)
                .Select(group => new { AssessorUserId = group.Key, Count = group.Count() })
                .OrderByDescending(entry => entry.Count)
                .ThenBy(entry => entry.AssessorUserId, StringComparer.Ordinal)
                .ToArray();

            var distinctAssessors = assessorGroups.Length;
            var distinctSources = epaRatings
                .Select(rating => rating.Source)
                .Distinct()
                .Count();

            var dominant = assessorGroups.FirstOrDefault();
            var dominantCount = dominant?.Count ?? 0;

            var oneAssessorOverHalf = ratingCount >= 2 && dominantCount * 2 > ratingCount;
            var singleSource = ratingCount >= 2 && distinctSources == 1;
            var fewerThanThree = distinctAssessors < 3;

            if (!oneAssessorOverHalf && !singleSource && !fewerThanThree)
            {
                continue;
            }

            var epa = epas[epaId];
            perEpa.Add(new EpaSamplingConcentrationDto(
                epa.Id,
                epa.Code,
                epa.Title,
                ratingCount,
                distinctAssessors,
                distinctSources,
                dominant?.AssessorUserId,
                dominantCount,
                oneAssessorOverHalf,
                singleSource,
                fewerThanThree));
        }

        return new SamplingConcentrationReportDto(
            review.Id,
            totalRated,
            distinctAssessorsOverall,
            AnyWarning: perEpa.Count > 0,
            perEpa);
    }

    private static bool TryParseRating(string dataJson, out int epaId, out string assessorUserId)
    {
        epaId = 0;
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
