using System.Globalization;
using System.Text.Json;

namespace Wombat.Domain.Curricula;

public sealed class CurriculumItem
{
    public int Id { get; set; }
    public int CurriculumId { get; set; }
    public int EpaId { get; set; }

    /// <summary>
    /// Null for a national core item (part of the College's published curriculum). Set to an institution id
    /// for an institution-local addition: an institution may add local items to a national curriculum it has
    /// adopted, but never edit the national core (T091, phase 3).
    /// </summary>
    public int? OwningInstitutionId { get; set; }

    /// <summary>
    /// The number of credited encounters the trainee must reach in each <see cref="QuotaPeriod" /> window
    /// (T130, D18). It is a per-period target, not a programme total. For the CPSA paediatric catalogue it is
    /// Annexure B's per-semester figure for a <see cref="Curricula.QuotaPeriod.Semester" /> item and its
    /// per-annum figure for an <see cref="Curricula.QuotaPeriod.AcademicYear" /> item.
    /// </summary>
    /// <remarks>
    /// Before T130 the catalogue seeder stored the annual quota multiplied by four programme years, which made
    /// the progress page read "1 / 24" against a number the College never published. A lifetime figure, if one
    /// is ever wanted, is a multiplication in a read model, not a stored column.
    /// </remarks>
    public int RequiredCount { get; set; }

    /// <summary>
    /// Which window <see cref="RequiredCount" /> is a target for. Progress is stored per semester whatever this
    /// says, so changing it re-reads the same stored buckets and never needs a rebuild.
    /// </summary>
    public QuotaPeriod QuotaPeriod { get; set; }

    public int MinimumLevelOrder { get; set; }

    /// <summary>
    /// Months, as authored. Its only reader is <c>AdmitTrainee</c>, which uses the largest value in the
    /// curriculum to default a trainee's expected completion date.
    /// </summary>
    /// <remarks>
    /// <b>This is not the quota period, and credit does not enforce it (D19).</b> It looks like a period, and
    /// the College's "expiry period if not practised" is the concept it was probably meant to hold. But nothing
    /// in the credit path reads it. <see cref="QuotaPeriod" /> is the window a target is counted over.
    /// </remarks>
    public int WindowMonths { get; set; }
    public double? Weight { get; set; }
    public string? MinimumLevelByStageJson { get; set; }

    /// <summary>
    /// The entrustment scale <see cref="MinimumLevelOrder" /> — and every value inside
    /// <see cref="MinimumLevelByStageJson" /> — is expressed on. Null means unpinned (T109).
    /// </summary>
    /// <remarks>
    /// Null is a permanent, meaningful state, not a migration artefact awaiting cleanup. An unpinned item
    /// compares ordinals exactly as Wombat did before T109, so leaving it null changes nothing; pinning it
    /// to the WRONG ladder silently refuses credit the trainee legitimately earned. That asymmetry is why
    /// nothing infers this value: the only automatic pins come from the seeders that author the minima and
    /// therefore know which ladder they were written against. In particular it is NOT inferred from
    /// <c>Curriculum.SubSpeciality.DefaultEntrustmentScaleId</c>, which
    /// <c>PaediatricCatalogueSeeder.EnsureDefaultScaleAsync</c> force-overwrites on every boot and which
    /// cannot differ between two versions of one curriculum anyway, <c>CloneAsNewVersion</c> copying
    /// <c>SubSpecialityId</c>.
    /// </remarks>
    public int? ScaleId { get; set; }

    public Curriculum Curriculum { get; set; } = null!;
    public Wombat.Domain.Epas.Epa Epa { get; set; } = null!;
    public Wombat.Domain.Epas.EntrustmentScale? Scale { get; set; }

    public int GetMinimumLevelForStage(int? traineeStage)
    {
        if (!traineeStage.HasValue)
        {
            return MinimumLevelOrder;
        }

        var overrides = ParseStageOverrides(MinimumLevelByStageJson);
        return overrides.TryGetValue(traineeStage.Value, out var stageLevel)
            ? stageLevel
            : MinimumLevelOrder;
    }

    public static IReadOnlyDictionary<int, int> ParseStageOverrides(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return EmptyOverrides;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return EmptyOverrides;
            }

            var result = new Dictionary<int, int>();
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (!int.TryParse(property.Name, NumberStyles.Integer, CultureInfo.InvariantCulture, out var stage) || stage <= 0)
                {
                    continue;
                }

                int level;
                if (property.Value.ValueKind == JsonValueKind.Number && property.Value.TryGetInt32(out level))
                {
                    // accepted
                }
                else if (property.Value.ValueKind == JsonValueKind.String &&
                         int.TryParse(property.Value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out level))
                {
                    // accepted
                }
                else
                {
                    continue;
                }

                if (level < 1 || level > 20)
                {
                    continue;
                }

                result[stage] = level;
            }

            return result;
        }
        catch (JsonException)
        {
            return EmptyOverrides;
        }
    }

    public static string? NormalizeStageOverridesJson(string? json)
    {
        var overrides = ParseStageOverrides(json);
        if (overrides.Count == 0)
        {
            return null;
        }

        var ordered = overrides
            .OrderBy(entry => entry.Key)
            .ToDictionary(
                entry => entry.Key.ToString(CultureInfo.InvariantCulture),
                entry => entry.Value);
        return JsonSerializer.Serialize(ordered);
    }

    private static readonly IReadOnlyDictionary<int, int> EmptyOverrides = new Dictionary<int, int>();
}
