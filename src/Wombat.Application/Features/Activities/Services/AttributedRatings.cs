using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;

namespace Wombat.Application.Features.Activities.Services;

/// <summary>
/// One named assessor's rating of one EPA: an activity of a rated type, in a state that is evidence, that
/// <see cref="RatedEvidenceProfile" /> read as <see cref="RatedEvidenceOutcome.Attributed" />. (T135, T166)
/// </summary>
/// <param name="ActivityId">The activity the rating is in.</param>
/// <param name="EpaId">The EPA stamped on the activity (<c>Activity.EpaId</c>, T137).</param>
/// <param name="Rating">The ordinal the assessor recorded.</param>
/// <param name="AssessorUserId">Who rated it.</param>
/// <param name="ObservedOn">The encounter date (T119); the day the activity was created when nobody stated one.</param>
/// <param name="ObservedOnDeclared">Whether a clinician stated <paramref name="ObservedOn" /> (T161, D28).</param>
/// <param name="Source">The evidence source the type's instrument belongs to (<see cref="RatedTypeVerdict.SourceBucket" />).</param>
/// <param name="RatedScaleId">
/// The ladder the rating was recorded against: the <c>scale_key</c> of the pinned version's rated field, resolved as
/// the credit engine resolves it. Null means "not knowable" (no such key, or one naming no scale), never "the same as
/// the curriculum's ladder". (T126)
/// </param>
public sealed record AttributedRating(
    int ActivityId,
    int EpaId,
    int Rating,
    string AssessorUserId,
    DateOnly ObservedOn,
    bool ObservedOnDeclared,
    string Source,
    int? RatedScaleId);

/// <summary>
/// Reads the named assessors' ratings among a set of activities: the one reading the trajectory chart plots and the
/// entrustment standing takes its latest rating from, so the two cannot disagree about what a rating is. (T135, T166)
/// </summary>
/// <remarks>
/// <para>
/// Lifted out of <c>GetEpaTrajectoryForTraineeQueryHandler</c> unchanged when T166 needed the same answer. Which rows
/// are evidence, where the rating and the assessor are read from, and why the EPA is the stamped one, are
/// <see cref="RatedEvidenceProfile" />'s rules; this only applies them to rows.
/// </para>
/// <para>
/// <b>It reads what it is handed.</b> The caller confines the rows first, with <c>ActivityReadScope.WhereReadableBy</c>
/// and whatever else it filters on. Both callers start from <c>Set&lt;Activity&gt;()</c> in their own handler, where
/// <c>ActivityReadBoundaryTests</c> can see the read rule applied. A reader that fetched its own rows would be a way
/// round that test.
/// </para>
/// </remarks>
public static class AttributedRatings
{
    /// <summary>
    /// The attributed ratings among <paramref name="activities" />, in no particular order.
    /// </summary>
    /// <param name="dbContext">The context the rows' types and pinned versions are read from.</param>
    /// <param name="activities">The rows to read, already confined to what the caller may see.</param>
    /// <param name="cancellationToken">Cancels the reads.</param>
    public static async Task<IReadOnlyList<AttributedRating>> ReadAsync(
        IApplicationDbContext dbContext,
        IQueryable<Activity> activities,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(activities);

        // Gated on what the type DECLARES, not on whether its key is one a list has heard of: an institution's own
        // rated tool is read the moment it declares `rated_level_field` (T126). Its source is classified by the
        // instrument it declares (WbaToolKey, T144). Classified once per type, not once per row.
        var typeIds = await activities
            .Select(activity => activity.ActivityTypeId)
            .Distinct()
            .ToArrayAsync(cancellationToken);

        var verdicts = await RatedActivityTypes.LoadAsync(dbContext, typeIds, cancellationToken);
        var ratedTypeIds = verdicts
            .Where(entry => entry.Value.IsRated)
            .Select(entry => entry.Key)
            .ToArray();

        var rows = await activities
            .Where(activity => ratedTypeIds.Contains(activity.ActivityTypeId))
            .ToListAsync(cancellationToken);

        // Only a terminal state of the pinned workflow is an observation (D44), so a draft, a request, and a rating an
        // assessor wrote and then declined are not read; the rating is the pinned schema's declared field; the EPA is
        // the one stamped on the activity (T137); the assessor is the one RatedEvidenceProfile names. A row that names
        // no assessor (an MSF, D36, or a rating the trainee wrote themselves) or whose rating is empty is not an
        // assessor's rating; one stamped with no EPA is about no EPA.
        var profiles = await RatedEvidenceProfiles.LoadAsync(
            dbContext,
            rows.Select(activity => (activity.ActivityTypeId, activity.SchemaVersion)),
            cancellationToken);

        var ratedScaleIdByActivity = await ResolveRatedScaleIdsAsync(dbContext, rows, cancellationToken);

        var ratings = new List<AttributedRating>();
        foreach (var activity in rows)
        {
            var profile = profiles[(activity.ActivityTypeId, activity.SchemaVersion)];
            if (!profile.IsEvidence(activity.CurrentState))
            {
                continue;
            }

            var reading = profile.Read(activity.CurrentState, activity.EpaId, activity.DataJson);
            if (reading.Outcome != RatedEvidenceOutcome.Attributed)
            {
                continue;
            }

            ratings.Add(new AttributedRating(
                activity.Id,
                reading.EpaId,
                reading.Rating,
                reading.AssessorUserId,
                activity.ObservedOn,
                activity.ObservedOnSource == ObservationDateSource.Declared,
                verdicts[activity.ActivityTypeId].SourceBucket,
                ratedScaleIdByActivity.TryGetValue(activity.Id, out var scaleId) ? scaleId : null));
        }

        return ratings;
    }

    /// <summary>
    /// The entrustment ladder each activity's rating was actually recorded against, by activity id. Absent means "not
    /// knowable", never "the same as the curriculum's ladder". (T126)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Resolved from the activity's PINNED <c>ActivityTypeVersion</c>, not the live one: an activity rated on version 1
    /// keeps version 1's meaning even after the type is republished, which is the whole point of pinning. There is
    /// deliberately no foreign key to navigate (<c>ActivityConfiguration</c>), so the pair is matched in memory over a
    /// set already bounded by the caller's rows.
    /// </para>
    /// <para>
    /// The pinned schema, and only the pinned schema, is the source. The credit rules' <c>minimum_level_field</c> names
    /// the field a PARTICULAR directive gates on, which is a different question that merely had the same answer in the
    /// seed corpus; treating it as a fallback here would resolve confidently to a component scale for any type crediting
    /// on one of the five or six that <c>dops</c>, <c>acat</c>, <c>mini_cex</c> and <c>cbd</c> each declare.
    /// </para>
    /// </remarks>
    private static async Task<IReadOnlyDictionary<int, int>> ResolveRatedScaleIdsAsync(
        IApplicationDbContext dbContext,
        IReadOnlyCollection<Activity> activities,
        CancellationToken cancellationToken)
    {
        var empty = (IReadOnlyDictionary<int, int>)new Dictionary<int, int>();

        var typeIds = activities.Select(activity => activity.ActivityTypeId).Distinct().ToArray();
        if (typeIds.Length == 0)
        {
            return empty;
        }

        var versions = await dbContext.Set<ActivityTypeVersion>()
            .AsNoTracking()
            .Where(version => typeIds.Contains(version.ActivityTypeId))
            .Select(version => new { version.ActivityTypeId, version.Version, version.SchemaJson })
            .ToListAsync(cancellationToken);

        var scaleKeyByPin = new Dictionary<(int TypeId, int Version), string?>(versions.Count);
        foreach (var version in versions)
        {
            scaleKeyByPin[(version.ActivityTypeId, version.Version)] = TryReadRatedScaleKey(version.SchemaJson);
        }

        var lookup = await EntrustmentRungLabels.LoadForScaleKeysAsync(
            dbContext, scaleKeyByPin.Values, cancellationToken);

        var resolved = new Dictionary<int, int>();
        foreach (var activity in activities)
        {
            if (!scaleKeyByPin.TryGetValue((activity.ActivityTypeId, activity.SchemaVersion), out var scaleKey))
            {
                continue;
            }

            if (lookup.ResolveScaleKey(scaleKey) is { } scaleId)
            {
                resolved[activity.Id] = scaleId;
            }
        }

        return resolved;
    }

    /// <summary>
    /// The <c>scale_key</c> of the field a schema declares as its entrustment rating, or null when it declares none,
    /// names a field that carries no ladder, or does not parse. (T126)
    /// </summary>
    private static string? TryReadRatedScaleKey(string schemaJson)
    {
        try
        {
            var schema = FormSchemaParser.Parse(schemaJson);
            if (schema.RatedLevelField is null)
            {
                return null;
            }

            return schema.Sections
                .SelectMany(section => section.Fields)
                .FirstOrDefault(field => string.Equals(field.Key, schema.RatedLevelField, StringComparison.Ordinal))
                ?.ScaleKey;
        }
        catch (SchemaParseException)
        {
            // A stored version that no longer parses is a defect, but it is not a reader's to raise: the rating's
            // ladder is then unknown, which every caller already handles.
            return null;
        }
    }
}
