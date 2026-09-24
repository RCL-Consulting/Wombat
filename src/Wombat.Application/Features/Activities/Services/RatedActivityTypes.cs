using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Epas;

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
/// MSF ([T121]) or the College's instrument vocabulary ([T122]'s <c>WbaTools</c>) need a longitudinal or product category, it
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
/// The evidence category, or null when there is none to give: the instrument the type declares is no
/// category of rated evidence (MSF, a reflective exercise), or the type declares no instrument and its key
/// matches no known family. See <see cref="RatedActivityTypes.Classify" />.
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
/// The category answers "what kind of evidence is this", which feeds <c>DistinctSourceCount</c> on the
/// committee sampling report and the trajectory's source label. It never decides what is rated. It is read
/// from the instrument the type DECLARES, <c>ActivityType.WbaToolKey</c> ([T122]), and guessed from the
/// type's key only when the type declares no instrument (T144).
/// </para>
/// </remarks>
public static class RatedActivityTypes
{
    /// <summary>
    /// The evidence each instrument of the College's vocabulary produces, keyed on <c>WbaTool.Key</c>. (T144)
    /// </summary>
    /// <remarks>
    /// <para>
    /// A static map rather than a <c>Category</c> column on <c>WbaTools</c>. The category is a vocabulary
    /// this file owns (three members, see <see cref="WbaEvidenceSource" />), not something the College's
    /// catalogue states, and the seeder reconciles that table from the catalogue file on every boot. A column
    /// would have needed a migration and a catalogue field the College never wrote.
    /// </para>
    /// <para>
    /// Every key of the vocabulary is stated, including the five that are no category of rated evidence
    /// (MSF, reflective exercise, clinical audit, portfolio review, learner feedback), so a missing key is a
    /// decision nobody made rather than one made as "none". <c>SeedScaleKeyTests</c> holds this list against
    /// the catalogue's vocabulary.
    /// </para>
    /// <para>
    /// <c>chart_stimulated_recall</c> is filed under Conversation, which is what it has always charted as, and
    /// is arguably case analysis; re-filing it belongs to the College. "Case note review" and "Directly
    /// observed clinical examination" are not keys: the College made them CCA (D4) and Mini-CEX (D12).
    /// </para>
    /// </remarks>
    private static readonly IReadOnlyDictionary<string, WbaEvidenceSource?> CategoryByWbaToolKey =
        new Dictionary<string, WbaEvidenceSource?>(StringComparer.Ordinal)
        {
            ["mini_cex"] = WbaEvidenceSource.DirectObservation,
            ["dops"] = WbaEvidenceSource.DirectObservation,
            ["direct_observation"] = WbaEvidenceSource.DirectObservation,
            ["cbd"] = WbaEvidenceSource.Conversation,
            ["chart_stimulated_recall"] = WbaEvidenceSource.Conversation,
            ["cca"] = WbaEvidenceSource.CaseAnalysis,
            ["rca"] = WbaEvidenceSource.CaseAnalysis,
            ["msf"] = null,
            ["reflective_exercise"] = null,
            ["clinical_audit"] = null,
            ["portfolio_review"] = null,
            ["learner_feedback"] = null
        };

    /// <summary>
    /// The fallback for a type that declares no instrument: its key matched exactly or as a
    /// <c>"&lt;family&gt;_"</c> prefix, so an unkeyed <c>mini_cex_paed</c> still reads as Direct observation.
    /// </summary>
    /// <remarks>
    /// Not a second hand-kept list. [T122] spelled the instrument keys like the families, so the families are
    /// the categorised instruments above plus the one family that is no College instrument: <c>acat</c>,
    /// seeded unkeyed (D21), which keeps its Conversation label this way.
    /// </remarks>
    private static readonly IReadOnlyDictionary<string, WbaEvidenceSource> CategoryByUnkeyedFamily =
        CategoryByWbaToolKey
            .Where(entry => entry.Value is not null)
            .Select(entry => KeyValuePair.Create(entry.Key, entry.Value!.Value))
            .Append(KeyValuePair.Create("acat", WbaEvidenceSource.Conversation))
            .ToDictionary(StringComparer.Ordinal);

    /// <summary>
    /// The instrument keys this classifier has a stated answer for, "no category" included.
    /// </summary>
    public static IReadOnlyCollection<string> ClassifiedWbaToolKeys { get; } = CategoryByWbaToolKey.Keys.ToArray();

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
    /// Classify one activity type from its key, the instrument it declares, and its CURRENT published schema.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The category comes from <paramref name="wbaToolKey" /> whenever the type declares one, so a
    /// builder-made Mini-CEX under a key nobody has heard of reads as Direct observation, and a type keyed
    /// <c>msf</c> has no category whatever it is called. The type's own key is consulted only when the type
    /// declares no instrument. (T144)
    /// </para>
    /// <para>
    /// <b>The instrument is read live, so the category is too.</b> <c>WbaToolKey</c> is unversioned and the
    /// builder writes it when a draft is SAVED, not when it is published, and discarding the draft does not undo
    /// it ([T122]). Changing a type's instrument therefore relabels every activity of that type on the trajectory,
    /// completed ones included, and can move a committee sampling report's <c>DistinctSourceCount</c> and
    /// <c>SingleSource</c>, from the save onward. Before T144 the category came from the type's key, which cannot
    /// change after publish. What is RATED still follows the published schema alone. Staging the instrument with
    /// the draft would make the two move together; that is the tool gate's design as much as this one's.
    /// </para>
    /// <para>
    /// The current schema, not the activity's pinned version, and that is deliberate: "is this
    /// instrument an entrustment instrument" is a property of the TYPE, not a per-activity semantic.
    /// A pinned read would be strictly worse here, because the seed refresher republishes seeded types
    /// at boot and leaves in-flight activities pinned to pre-T126 versions that carry no pointer —
    /// which would reproduce the very defect this fixes for exactly those rows. Contrast
    /// <c>GetEpaTrajectoryForTraineeQuery.ResolveRatedScaleIdsAsync</c>, which MUST read the pinned
    /// version, because WHICH LADDER a rating sits on is a fact about that rating.
    /// </para>
    /// </remarks>
    public static RatedTypeVerdict Classify(string? activityTypeKey, string? wbaToolKey, string? schemaJson)
    {
        var key = activityTypeKey?.Trim() ?? string.Empty;
        var toolKey = WbaTool.NormalizeKey(wbaToolKey);
        var category = toolKey is null
            ? ResolveFamily(key)
            : CategoryByWbaToolKey.GetValueOrDefault(toolKey);
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
            .Select(type => new { type.Id, type.Key, type.WbaToolKey, type.SchemaJson })
            .ToListAsync(cancellationToken);

        return types.ToDictionary(
            type => type.Id,
            type => Classify(type.Key, type.WbaToolKey, type.SchemaJson));
    }

    private static WbaEvidenceSource? ResolveFamily(string activityTypeKey)
    {
        if (string.IsNullOrEmpty(activityTypeKey))
        {
            return null;
        }

        foreach (var (family, category) in CategoryByUnkeyedFamily)
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
    /// version that no longer parses is a real defect, but it is not a committee report's to raise.
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
