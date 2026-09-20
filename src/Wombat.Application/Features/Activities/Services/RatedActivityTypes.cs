using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;

namespace Wombat.Application.Features.Activities.Services;

/// <summary>
/// The category of evidence a workplace-based assessment tool produces. (T134)
/// </summary>
/// <remarks>
/// Three members, because three is what the repository actually distinguishes. This replaces
/// <c>WbaSourceCategory</c>, which carried <c>LongitudinalObservation</c> and <c>ProductEvaluation</c>
/// — both declared in June and referenced by nothing, in <c>src/</c> or <c>tests/</c>, ever. They
/// belonged to a different taxonomy from the one the trajectory chart prints, and keeping them would
/// have made a shared type carry two vocabularies, which is the drift this file exists to stop. When
/// MSF ([T121]) or the fourteen annexure names ([T122]) need a longitudinal or product category, it
/// gets added against the College's vocabulary rather than guessed ahead of it.
/// </remarks>
public enum WbaEvidenceSource
{
    DirectObservation = 1,
    Conversation = 2,
    CaseAnalysis = 3
}

/// <summary>
/// What is known about one activity type: whether it produces an entrustment rating, and what kind of
/// evidence it is. (T134)
/// </summary>
/// <param name="IsRated">
/// Whether this type produces an entrustment rating at all. This is the gate two separate handlers had
/// their own private answer to before T134.
/// </param>
/// <param name="Category">
/// The evidence category, or null when the type is rated but its key matches no known family — a tool
/// an institution built under a name this list has never heard of.
/// </param>
/// <param name="SourceBucket">
/// What to count and print as "the source". The category's label when there is one, otherwise the
/// type's own key, so two activities of the same unfamiliar type count as ONE source rather than as
/// none. Never empty.
/// </param>
public readonly record struct RatedTypeVerdict(bool IsRated, WbaEvidenceSource? Category, string SourceBucket);

/// <summary>
/// The one answer to "does this activity type produce an entrustment rating, and what kind of evidence
/// is it?" (T134)
/// </summary>
/// <remarks>
/// <para>
/// Before T134 there were three disagreeing answers. `GetSamplingConcentrationWarnings` matched EXACT
/// keys over <c>mini_cex</c>/<c>dops</c>/<c>cbd</c>/<c>acat</c> — so every seeded CPSA v11.1 tool fell
/// outside it and a v11.1 trainee's committee sampling report said there was no rated evidence at all.
/// `GetEpaTrajectoryForTraineeQuery` asked the same question by family prefix and got a different
/// answer. The credit rules asked it a third way. Each read correctly on its own, which is why the
/// disagreement was invisible.
/// </para>
/// <para>
/// <b>The gate is the declaration, and only the declaration.</b> A type is rated when it declares
/// <c>rated_level_field</c> (T126) — the authoritative answer, and the one a type states about itself.
/// </para>
/// <para>
/// This was briefly a disjunction, rated when the pointer was declared <b>or</b> the key matched a
/// known family, because four operator-built <c>*_paed</c> types existed only as scenario rows, could
/// never be given a pointer, and would otherwise have stopped counting. The dev database was emptied
/// and rebuilt on 2026-09-20 and those rows are gone: every activity type now comes from a seed
/// folder and every rated one declares its pointer. The second arm was preserving data that no longer
/// exists, so it is gone too — CLAUDE.md is explicit that backward compatibility is not a design
/// constraint here.
/// </para>
/// <para>
/// The family map survives for <b>labelling only</b>. It answers "what kind of evidence is this",
/// which feeds <c>DistinctSourceCount</c> on the committee sampling report and the trajectory's source
/// label. It no longer decides what is rated, and it retires entirely with [T122]'s
/// <c>WbaToolKey</c>.
/// </para>
/// </remarks>
public static class RatedActivityTypes
{
    /// <summary>
    /// Known tool families and the evidence each produces. Matched exactly or as a <c>"&lt;family&gt;_"</c>
    /// prefix, because schema-driven types carry institution-specific keys like <c>mini_cex_paed</c>.
    /// </summary>
    /// <remarks>
    /// Moved here from <c>GetEpaTrajectoryForTraineeQuery</c> rather than copied — there was already a
    /// second copy in the sampling handler, and a third would have been how the next defect got made.
    /// It classifies; it does not gate.
    /// <para>
    /// Two entries were removed on 2026-09-20 because the College's reply retired the instruments
    /// themselves, not merely their names: <c>case_note_review</c> is an alias of CCA (D4) and
    /// <c>observed_clinical_exam</c> is the same instrument as Mini-CEX (D12). Keeping either would
    /// have been a category for a tool that will never be seeded.
    /// </para>
    /// <para>
    /// <c>cca</c>, <c>rca</c> and <c>chart_stimulated_recall</c> stay: they are real instruments
    /// [T120] is queued to author, and this map now only decides what their evidence is CALLED, not
    /// whether it counts. <c>chart_stimulated_recall</c> is filed under Conversation and is arguably
    /// case analysis; re-filing it is [T122]'s DECISION 2 and belongs to the College.
    /// </para>
    /// </remarks>
    private static readonly IReadOnlyDictionary<string, WbaEvidenceSource> SourceByActivityFamily =
        new Dictionary<string, WbaEvidenceSource>(StringComparer.Ordinal)
        {
            ["mini_cex"] = WbaEvidenceSource.DirectObservation,
            ["dops"] = WbaEvidenceSource.DirectObservation,
            ["direct_observation"] = WbaEvidenceSource.DirectObservation,
            ["cbd"] = WbaEvidenceSource.Conversation,
            ["acat"] = WbaEvidenceSource.Conversation,
            ["chart_stimulated_recall"] = WbaEvidenceSource.Conversation,
            ["cca"] = WbaEvidenceSource.CaseAnalysis,
            ["rca"] = WbaEvidenceSource.CaseAnalysis
        };

    /// <summary>
    /// The category as a clinician reads it. These three strings are what the trajectory chart has
    /// always printed; they are reproduced verbatim so no tooltip, axis or accessibility table changes.
    /// </summary>
    public static string Label(this WbaEvidenceSource source) => source switch
    {
        WbaEvidenceSource.DirectObservation => "Direct observation",
        WbaEvidenceSource.Conversation => "Conversation",
        WbaEvidenceSource.CaseAnalysis => "Case analysis",
        _ => "Other"
    };

    /// <summary>
    /// Classify one activity type from its key and its CURRENT published schema.
    /// </summary>
    /// <remarks>
    /// The current schema, not the activity's pinned version, and that is deliberate: "is this
    /// instrument an entrustment instrument" is a property of the TYPE, not a per-activity semantic.
    /// A pinned read would be strictly worse here, because the seed refresher republishes seeded types
    /// at boot and leaves in-flight activities pinned to pre-T126 versions that carry no pointer —
    /// which would reproduce the very defect this fixes for exactly those rows. Contrast
    /// <c>GetEpaTrajectoryForTraineeQuery.ResolveRatedScaleIdsAsync</c>, which MUST read the pinned
    /// version, because WHICH LADDER a rating sits on is a fact about that rating.
    /// </remarks>
    public static RatedTypeVerdict Classify(string? activityTypeKey, string? schemaJson)
    {
        var key = activityTypeKey?.Trim() ?? string.Empty;
        var category = ResolveFamily(key);
        var declaresRating = DeclaresRating(schemaJson);

        return new RatedTypeVerdict(
            IsRated: declaresRating,
            Category: category,
            SourceBucket: category?.Label() ?? key);
    }

    /// <summary>
    /// Classify every activity type named, keyed by id, for a handler that has activity rows and needs
    /// to know which of their types are rated.
    /// </summary>
    /// <remarks>
    /// A static loader rather than an injected service, following
    /// <c>EntrustmentRungLabels.LoadAsync</c>: no handler constructor changes, so no call site moves.
    /// </remarks>
    public static async Task<IReadOnlyDictionary<int, RatedTypeVerdict>> LoadAsync(
        IApplicationDbContext dbContext,
        IEnumerable<int> activityTypeIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        var ids = activityTypeIds.Distinct().ToArray();
        if (ids.Length == 0)
        {
            return new Dictionary<int, RatedTypeVerdict>();
        }

        var types = await dbContext.Set<ActivityType>()
            .AsNoTracking()
            .Where(type => ids.Contains(type.Id))
            .Select(type => new { type.Id, type.Key, type.SchemaJson })
            .ToListAsync(cancellationToken);

        return types.ToDictionary(
            type => type.Id,
            type => Classify(type.Key, type.SchemaJson));
    }

    private static WbaEvidenceSource? ResolveFamily(string activityTypeKey)
    {
        if (string.IsNullOrEmpty(activityTypeKey))
        {
            return null;
        }

        foreach (var (family, category) in SourceByActivityFamily)
        {
            if (activityTypeKey == family ||
                activityTypeKey.StartsWith(family + "_", StringComparison.Ordinal))
            {
                return category;
            }
        }

        return null;
    }

    /// <summary>
    /// Whether the schema declares which field carries its entrustment rating (T126).
    /// </summary>
    /// <remarks>
    /// A schema that does not parse is treated as declaring nothing rather than as an error. A stored
    /// version that no longer parses is a real defect, but it is not a committee report's to raise —
    /// and the family arm still covers every tool the product ships.
    /// </remarks>
    private static bool DeclaresRating(string? schemaJson)
    {
        if (string.IsNullOrWhiteSpace(schemaJson))
        {
            return false;
        }

        try
        {
            return FormSchemaParser.Parse(schemaJson).RatedLevelField is not null;
        }
        catch (SchemaParseException)
        {
            return false;
        }
    }
}
