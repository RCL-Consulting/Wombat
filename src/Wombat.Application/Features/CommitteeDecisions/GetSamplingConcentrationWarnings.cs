using System.Globalization;
using System.Security.Claims;
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

/// <summary>
/// The committee's sampling report for one review.
/// </summary>
/// <remarks>
/// <b>The partition (T135).</b> Every activity of a rated type in the review window whose state is a terminal state of
/// its pinned workflow (D44) is in exactly one of four counts:
/// <c>TotalRatedActivities + WithheldRatedActivities + UnreadableRatedActivities + UnattributedRatedActivities</c>.
/// Nothing else is in any of them: a draft, a request, a declined or a cancelled activity is not evidence.
/// </remarks>
/// <param name="TotalRatedActivities">Assessor-attributed ratings the arithmetic ran on.</param>
/// <param name="WithheldRatedActivities">Evidence rows this caller may not read, of a version that names an
/// assessor, so they could have entered the figures.</param>
/// <param name="UnreadableRatedActivities">Evidence rows this caller may read whose declared EPA, rating or assessor is
/// missing or malformed, or empty where every move into the row's state required it.</param>
/// <param name="UnattributedRatedActivities">Evidence rows with no named assessor's rating in them, allowed to be so:
/// the pinned version names nobody who writes the rating (<c>msf_cpsa</c>), whether or not this caller may read the
/// row, or the rating or assessor was left empty where the form allows it. Nothing in them could enter the figures,
/// so they do not make the report incomplete.</param>
public sealed record SamplingConcentrationReportDto(
    int ReviewId,
    int TotalRatedActivities,
    int DistinctAssessorCount,
    bool AnyWarning,
    IReadOnlyList<EpaSamplingConcentrationDto> PerEpa,
    int WithheldRatedActivities,
    int UnreadableRatedActivities,
    int UnattributedRatedActivities)
{
    /// <summary>
    /// Whether every assessor's rating in the review window went into the numbers above. When this
    /// is false the report is arithmetic on a subset and its silence means nothing — the absence of
    /// a warning is then "we could not look", not "we looked and it is clean". A panel deciding
    /// whether a trainee progresses has to be able to tell those two apart, so the page renders the
    /// incomplete case as its own statement rather than as an empty warning list.
    /// </summary>
    /// <remarks>
    /// Two things make it false, and the page names each: rows the caller may not read, and rows nobody can read.
    /// An unattributed row does not: there was never an assessor's rating in it to leave out.
    /// </remarks>
    public bool EvidenceComplete => WithheldRatedActivities == 0 && UnreadableRatedActivities == 0;
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
    private readonly IUserAdministrationService _users;

    public GetSamplingConcentrationWarningsQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<SamplingConcentrationReportDto> Handle(
        GetSamplingConcentrationWarningsQuery request,
        CancellationToken cancellationToken)
    {
        var review = await _dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .Include(entity => entity.Panel)
                .ThenInclude(panel => panel.Members)
            .SingleOrDefaultAsync(entity => entity.Id == request.ReviewId, cancellationToken);

        // One refusal for an unknown review and one out of reach, before anything about it is said (T194 item 1).
        review = await CommitteeDecisionAuthorization.DemandReviewAccessAsync(
            _dbContext, _users, request.Principal, review, cancellationToken);

        // Bunching is a question about clinical practice — was this trainee only ever watched by one
        // assessor, in one narrow stretch of the period? — so the window selects on the encounter date
        // rather than the filing date. (T119)
        //
        // The DATES are the ones the evidence snapshot StartCommitteeReview builds for the same review; the
        // ROWS are not. The snapshot lists every activity in the window in whatever state, each labelled
        // with it, because a run of declines is something a panel should see, and it is frozen at Start.
        // This report is computed live on every load and samples only rated evidence in a terminal state
        // of its pinned workflow (D44). So it leaves out rows the snapshot lists (every other state), and
        // it can count rows the snapshot never listed: a WBA observed in the window and completed after
        // Start, or any row of a review not yet started, which has no snapshot at all. (T135)
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
        // schema — but it does not need to be.
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

        // Which of those rows are evidence depends on the state each is in and on its PINNED workflow (D44):
        // a terminal state, where credit fires. That is decided per (type, version) pin, once, from a
        // projection of every rated row in the window — readable or not, so the denominator still counts
        // what this caller may not see — and then applied to the rows by id. A draft, a request, a declined
        // and a cancelled activity never enter any count below. (T135 defect 1, T150)
        var ratedRows = await inWindow
            .Where(activity => ratedTypeIds.Contains(activity.ActivityTypeId))
            .Select(activity => new { activity.Id, activity.ActivityTypeId, activity.SchemaVersion, activity.CurrentState })
            .ToListAsync(cancellationToken);

        var profiles = await RatedEvidenceProfiles.LoadAsync(
            _dbContext,
            ratedRows.Select(row => (row.ActivityTypeId, row.SchemaVersion)),
            cancellationToken);

        var evidenceRows = ratedRows
            .Where(row => profiles[(row.ActivityTypeId, row.SchemaVersion)].IsEvidence(row.CurrentState))
            .ToArray();
        var evidenceIds = evidenceRows.Select(row => row.Id).ToArray();

        // No Include: the source bucket is looked up by ActivityTypeId, so the navigation is a join
        // nothing reads any more.
        var activities = await _dbContext.Set<Activity>()
            .AsNoTracking()
            .Where(activity => evidenceIds.Contains(activity.Id))
            .WhereReadableBy(request.Principal)
            .ToListAsync(cancellationToken);

        // A row this caller may not read is withheld only if it could have entered the figures. One whose pinned
        // version names nobody who writes the rating (an MSF, D36) never could, so the figures are the same without
        // it, and calling it withheld would make the page say they are not. Which it is follows from the version
        // alone, without reading the row; the caller learns only what kind of record it is, beside a count the
        // withheld figure already disclosed. (T135 review)
        var readableIds = activities.Select(activity => activity.Id).ToHashSet();
        var withheldRatedActivities = 0;
        var unreadableRatedActivities = 0;
        var unattributedRatedActivities = 0;
        foreach (var row in evidenceRows.Where(row => !readableIds.Contains(row.Id)))
        {
            if (profiles[(row.ActivityTypeId, row.SchemaVersion)].NamesNoAssessor)
            {
                unattributedRatedActivities++;
            }
            else
            {
                withheldRatedActivities++;
            }
        }

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

            // Every readable evidence row lands in exactly one column (T135 defect 2). An unreadable row is
            // evidence the report could not use, so it makes the report incomplete; an unattributed one (an
            // MSF, or a rating or assessor left empty where the form allows it) never had a named assessor's
            // rating in it, so it does not.
            var reading = profiles[(activity.ActivityTypeId, activity.SchemaVersion)]
                .Read(activity.CurrentState, activity.EpaId, activity.DataJson);
            switch (reading.Outcome)
            {
                case RatedEvidenceOutcome.Attributed:
                    ratings.Add((reading.EpaId, reading.AssessorUserId, source));
                    break;
                case RatedEvidenceOutcome.Unattributed:
                    unattributedRatedActivities++;
                    break;
                default:
                    unreadableRatedActivities++;
                    break;
            }
        }

        if (ratings.Count == 0)
        {
            return new SamplingConcentrationReportDto(
                review.Id,
                TotalRatedActivities: 0,
                DistinctAssessorCount: 0,
                AnyWarning: false,
                PerEpa: Array.Empty<EpaSamplingConcentrationDto>(),
                withheldRatedActivities,
                unreadableRatedActivities,
                unattributedRatedActivities);
        }

        var namedEpaIds = ratings.Select(rating => rating.EpaId).Distinct().ToArray();
        var epas = await _dbContext.Set<Epa>()
            .AsNoTracking()
            .Where(epa => namedEpaIds.Contains(epa.Id))
            .ToDictionaryAsync(epa => epa.Id, cancellationToken);

        // The EPA is the activity's stamped EpaId (T137), which named an existing EPA when it was stamped
        // (EvidenceEpaResolver). The column carries no foreign key, though (ActivityConfiguration), so an id whose EPA
        // is gone since is unreadable: not a rating of nothing, and not a KeyNotFoundException below.
        unreadableRatedActivities += ratings.RemoveAll(rating => !epas.ContainsKey(rating.EpaId));

        var totalRated = ratings.Count;
        var distinctAssessorsOverall = ratings
            .Select(rating => rating.AssessorUserId)
            .Distinct(StringComparer.Ordinal)
            .Count();

        var epaIds = ratings.Select(rating => rating.EpaId).Distinct().ToArray();
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
            withheldRatedActivities,
            unreadableRatedActivities,
            unattributedRatedActivities);
    }
}
