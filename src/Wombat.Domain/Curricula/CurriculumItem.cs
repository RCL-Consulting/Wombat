using System.Globalization;
using System.Text.Json;

namespace Wombat.Domain.Curricula;

public sealed class CurriculumItem
{
    public int Id { get; set; }
    public int CurriculumId { get; set; }
    public int EpaId { get; set; }
    public int RequiredCount { get; set; }
    public int MinimumLevelOrder { get; set; }
    public int WindowMonths { get; set; }
    public double? Weight { get; set; }
    public string? MinimumLevelByStageJson { get; set; }

    public Curriculum Curriculum { get; set; } = null!;
    public Wombat.Domain.Epas.Epa Epa { get; set; } = null!;

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
