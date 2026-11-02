using System.ComponentModel.DataAnnotations;
using Wombat.Domain.Curricula;

namespace Wombat.Web.Components.Pages.Admin.Curricula;

/// <summary>
/// An item's form on its page of its own (T392, flow 15, Q14): what the add and the edit bind, moved out of the items page
/// with the form, its rules unchanged (T122's tool list, T125's minima by ladder, T131's decision).
/// </summary>
internal sealed class CurriculumItemFormModel
{
    [Range(1, int.MaxValue)]
    public int EpaId { get; set; }

    [Range(1, int.MaxValue)]
    public int RequiredCount { get; set; }

    /// <summary>The window RequiredCount is a target for (T130). Copied from the item on edit, so saving unchanged keeps it.</summary>
    public QuotaPeriod QuotaPeriod { get; set; }

    /// <summary>How often the EPA is decided, or null for no published cadence (T131). Nullable, so nothing defaults it.</summary>
    public QuotaPeriod? DecisionCadence { get; set; }

    /// <summary>The deciding body's key; null or blank for the general panel (T131).</summary>
    public string? DecisionBodyKey { get; set; }

    /// <summary>Whether the EPA is decided as opportunity allows (T131). Only with a cadence.</summary>
    public bool DecisionIsOpportunistic { get; set; }

    /// <summary>The flat minimum's ordinal; null until one is picked, and again after a scale change (T125).</summary>
    public int? MinimumLevelOrder { get; set; }

    [Range(1, int.MaxValue)]
    public int WindowMonths { get; set; }

    public double? Weight { get; set; }

    /// <summary>The minimum by training year, as rows (T125). Serialised to the JSON the command takes on save.</summary>
    public StageMinimaDraft StageMinima { get; set; } = StageMinimaDraft.Parse(null);

    /// <summary>Null means unpinned, which compares ordinals exactly as Wombat did before T109.</summary>
    public int? ScaleId { get; set; }

    /// <summary>Whether the scale differs from the one this edit (or this Add form) started on.</summary>
    public bool ScaleChanged => ScaleId != _startingScaleId;

    /// <summary>Whether the latest scale change emptied a minimum that had been picked, so the page can say why.</summary>
    public bool ClearedByScaleChange { get; private set; }

    /// <summary>The instruments ticked. Empty means any instrument (T122, D21), and is sent as such.</summary>
    public HashSet<string> PermittedToolKeys { get; set; } = new(StringComparer.Ordinal);

    private int? _startingScaleId;

    // The ladder the minima on screen were picked on. ScaleId has already moved when MoveToScale runs, so this is how
    // it knows which ladder it is leaving.
    private int? _ladder;

    // What was picked on each ladder left during this edit, keyed by scale id (0 for unpinned; ids start at 1).
    private readonly Dictionary<int, LadderPicks> _picksByLadder = [];

    /// <summary>Starts an edit, or a fresh Add form, on <paramref name="scaleId" />, with the minima already set.</summary>
    public void StartOn(int? scaleId)
    {
        ScaleId = scaleId;
        _startingScaleId = scaleId;
        _ladder = scaleId;
        _picksByLadder.Clear();
        ClearedByScaleChange = false;
    }

    /// <summary>
    /// Called after <see cref="ScaleId" /> changes. Every minimum, flat and by year, is emptied (T125): an ordinal is
    /// a different rung on each ladder, so carrying one across, or clamping it, would store a minimum nobody chose.
    /// </summary>
    /// <remarks>
    /// What was picked on the ladder being left is put aside and comes back if the operator returns to that ladder, so
    /// nothing ever crosses ladders but nothing is lost to a round trip either. On Windows each arrow key on a closed
    /// select is a change, so Down then Up would otherwise wipe an item's whole year curve while landing back on the
    /// ladder it was picked on.
    /// </remarks>
    public void MoveToScale()
    {
        if (ScaleId == _ladder)
        {
            return;
        }

        ClearedByScaleChange = MinimumLevelOrder is not null || StageMinima.Rows.Any(row => row.Level is not null);
        _picksByLadder[_ladder ?? 0] = new LadderPicks(
            MinimumLevelOrder, StageMinima.Rows.ToDictionary(row => row.Year, row => row.Level));

        MinimumLevelOrder = null;
        StageMinima.ClearLevels();
        if (_picksByLadder.TryGetValue(ScaleId ?? 0, out var picks))
        {
            MinimumLevelOrder = picks.Minimum;
            foreach (var row in StageMinima.Rows)
            {
                row.Level = picks.Years.GetValueOrDefault(row.Year);
            }
        }

        _ladder = ScaleId;
    }

    private sealed record LadderPicks(int? Minimum, IReadOnlyDictionary<int, int?> Years);
}
