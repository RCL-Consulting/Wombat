using System.Text.Json;
using System.Text.Json.Serialization;
using Wombat.Domain.Curricula;

namespace Wombat.Domain.Identity;

/// <summary>
/// One item of the list a programme ended with (T312): a curriculum item that was in force for the trainee on their last
/// day, national or their own institution's, with the target it held them to then. Kept on the profile
/// (<see cref="TraineeProfile.EndedCurriculumJson" />), so an ended record reads the targets it ended with, whatever the
/// catalogue does after.
/// </summary>
/// <remarks>
/// Only the item's identity and its target are kept. The EPA's code and title stay live (read from the EPA), so a
/// correction to a title still shows on the record, and so does a pause: an EPA paused after the end is still on the list,
/// headed "(no longer in use)" (T255), not dropped.
/// </remarks>
/// <param name="CurriculumItemId">The item: the key its progress rows carry.</param>
/// <param name="EpaId">The item's EPA, whose code and title the record reads live.</param>
/// <param name="IsLocal">The trainee's own institution's item rather than the College's.</param>
/// <param name="RequiredCount">The target per window (<see cref="CurriculumItem.RequiredCount" />) on the last day.</param>
/// <param name="QuotaPeriod">The window the target is counted over, on the last day.</param>
/// <param name="MinimumLevelOrder">The flat minimum and exit level (<see cref="CurriculumItem.MinimumLevelOrder" />).</param>
/// <param name="MinimumLevelByStageJson">Annexure A's per-year map (<see cref="CurriculumItem.MinimumLevelByStageJson" />).</param>
/// <param name="ScaleId">The ladder the levels are on (<see cref="CurriculumItem.ScaleId" />), or null when unpinned.</param>
/// <param name="DecisionCadence">How often a committee decided the EPA (<see cref="CurriculumItem.DecisionCadence" />).</param>
/// <param name="DecisionIsOpportunistic">Decided as opportunity allowed (<see cref="CurriculumItem.DecisionIsOpportunistic" />).</param>
public sealed record EndedCurriculumItem(
    int CurriculumItemId,
    int EpaId,
    bool IsLocal,
    int RequiredCount,
    QuotaPeriod QuotaPeriod,
    int MinimumLevelOrder,
    string? MinimumLevelByStageJson,
    int? ScaleId,
    QuotaPeriod? DecisionCadence,
    bool DecisionIsOpportunistic)
{
    /// <summary>
    /// The stored form: a JSON array, camelCase, enums as their integers (as the catalogue stores them). The T312 migration
    /// writes the same shape in SQL for the profiles that had already ended, so the two must stay in step.
    /// </summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    /// <summary>The item as the catalogue holds it now: what <see cref="TraineeProfile.RecordEndedCurriculum" /> keeps.</summary>
    public static EndedCurriculumItem Of(CurriculumItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new EndedCurriculumItem(
            item.Id,
            item.EpaId,
            item.OwningInstitutionId is not null,
            item.RequiredCount,
            item.QuotaPeriod,
            item.MinimumLevelOrder,
            item.MinimumLevelByStageJson,
            item.ScaleId,
            item.DecisionCadence,
            item.DecisionIsOpportunistic);
    }

    /// <summary>The list as stored, in item order so the same list always reads the same.</summary>
    public static string Serialize(IEnumerable<EndedCurriculumItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return JsonSerializer.Serialize(items.OrderBy(item => item.CurriculumItemId).ToArray(), Options);
    }

    /// <summary>
    /// The stored list, or null when none is stored. Postgres re-renders a <c>jsonb</c> value (keys reordered, spaces
    /// added), so the text is parsed, never compared.
    /// </summary>
    public static IReadOnlyList<EndedCurriculumItem>? Parse(string? json)
        => string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<EndedCurriculumItem[]>(json, Options)
              ?? throw new InvalidOperationException("The list a programme ended with could not be read.");
}
