using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Queries;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// Assessor- and source-concentration warnings for one review's evidence.
/// </summary>
/// <remarks>
/// <para>
/// Carries the caller because the report is computed from the trainee's activity rows, and named a
/// review id only — so any caller who could reach it could pull an out-of-institution trainee's
/// rating counts and their assessors' user ids out of it. Two gates answer that, and they answer
/// different questions. The review ladder decides whether this caller may have a report about this
/// review at all; reaching a review you have no business in is now a refusal rather than a report
/// that happens to be empty. The row filter then decides which evidence rows go into the
/// arithmetic, and it can still withhold rows from someone the ladder admitted. (T101)
/// </para>
/// <para>
/// That second case is the dangerous one, because the output is a statistic. An External panel
/// member sitting on a review outside their own institution, or a trainee who transferred mid-year,
/// leaves rows the caller may not read — and a mean computed on what is left is not a weaker
/// version of the right answer, it is a different answer. Dropping the dominant assessor's rows
/// clears a concentration warning; dropping anyone's rows can invent a
/// fewer-than-three-assessors one. So the report reports its own completeness, and a caller who was
/// shown less than the whole is told so rather than being handed a clean-looking number.
/// </para>
/// </remarks>
public sealed record GetSamplingConcentrationWarningsQuery(int ReviewId, ClaimsPrincipal Principal)
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
    IReadOnlyList<EpaSamplingConcentrationDto> PerEpa,
    int WithheldRatedActivities)
{
    /// <summary>
    /// Whether every rated observation in the review window went into the numbers above. When this
    /// is false the report is arithmetic on a subset and its silence means nothing — the absence of
    /// a warning is then "we could not look", not "we looked and it is clean". A panel deciding
    /// whether a trainee progresses has to be able to tell those two apart, so the page renders the
    /// incomplete case as its own statement rather than as an empty warning list.
    /// </summary>
    public bool EvidenceComplete => WithheldRatedActivities == 0;
}

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

public sealed class GetSamplingConcentrationWarningsQueryHandler
    : IRequestHandler<GetSamplingConcentrationWarningsQuery, SamplingConcentrationReportDto>
{
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
            .Include(entity => entity.Panel)
                .ThenInclude(panel => panel.Members)
            .SingleOrDefaultAsync(entity => entity.Id == request.ReviewId, cancellationToken)
            ?? throw new InvalidOperationException("The committee review could not be found.");

        CommitteeDecisionAuthorization.DemandReviewAccess(request.Principal, review);

        // Bunching is a question about clinical practice — was this trainee only ever watched by one
        // assessor, in one narrow stretch of the period? — so the window selects on the encounter date
        // rather than the filing date, and matches the evidence snapshot StartCommitteeReview builds for
        // the same review. Two windows over the same period that disagreed about which activities are in
        // it would make the warnings describe a different sample from the one the panel is reading. (T119)
        //
        // DateOnly bounds against a DateOnly column, inclusive at both ends as the AddDays(1)-exclusive
        // instant was; it also keeps the index on ObservedOn usable.
        var fromDate = review.ReviewPeriodFrom;
        var toDate = review.ReviewPeriodTo;

        // The denominator is the rated evidence in the window, whether or not this caller may read
        // it. Counting it first is what lets the report distinguish a clean sample from a sample it
        // was only shown part of; narrowing it to the rating-bearing types keeps a withheld
        // reflective note from being reported as missing evidence, since nothing unrated would have
        // entered the arithmetic anyway.
        // Which types are rated is resolved BEFORE the window query, to a set of ids (T134). The
        // rated test itself is not SQL-translatable — it reads T126's declared pointer out of the
        // schema — but it does not need to be. What must stay SQL is the predicate that produces the
        // denominator, and an int-set Contains is exactly that, still evaluated before the
        // readability filter below so WithheldRatedActivities keeps its meaning.
        var inWindow = _dbContext.Set<Activity>()
            .AsNoTracking()
            .Where(activity =>
                activity.SubjectUserId == review.TraineeUserId &&
                activity.ObservedOn >= fromDate &&
                activity.ObservedOn <= toDate);

        var typeIdsInWindow = await inWindow
            .Select(activity => activity.ActivityTypeId)
            .Distinct()
            .ToArrayAsync(cancellationToken);

        var verdicts = await RatedActivityTypes.LoadAsync(_dbContext, typeIdsInWindow, cancellationToken);
        var ratedTypeIds = verdicts
            .Where(entry => entry.Value.IsRated)
            .Select(entry => entry.Key)
            .ToArray();

        var ratedInWindow = inWindow.Where(activity => ratedTypeIds.Contains(activity.ActivityTypeId));

        var ratedInWindowCount = await ratedInWindow.CountAsync(cancellationToken);

        // No Include: the source bucket is looked up by ActivityTypeId, so the navigation is a join
        // nothing reads any more.
        var activities = await ratedInWindow
            .WhereReadableBy(request.Principal)
            .ToListAsync(cancellationToken);

        var withheldRatedActivities = ratedInWindowCount - activities.Count;

        var ratings = new List<(int EpaId, string AssessorUserId, string Source)>();
        foreach (var activity in activities)
        {
            // Every row here is already rated — the gate above said so — so an unknown family must
            // NOT drop it. It is in the denominator; dropping it from the numerator would under-report
            // TotalRatedActivities while EvidenceComplete still read true, which is the same lie this
            // task exists to remove, moved somewhere harder to see. Its own key becomes its source,
            // so two activities of one unfamiliar type count as one source rather than none.
            var source = verdicts.TryGetValue(activity.ActivityTypeId, out var verdict)
                ? verdict.SourceBucket
                : activity.ActivityTypeId.ToString(CultureInfo.InvariantCulture);

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
                PerEpa: Array.Empty<EpaSamplingConcentrationDto>(),
                withheldRatedActivities);
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
                .Distinct(StringComparer.Ordinal)
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
            perEpa,
            withheldRatedActivities);
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
