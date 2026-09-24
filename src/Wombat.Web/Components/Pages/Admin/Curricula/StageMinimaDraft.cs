using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Wombat.Web.Components.Pages.Admin.Curricula;

/// <summary>
/// A curriculum item's minimum-by-training-year map while it is being edited: one row per year, each with a rung
/// (T125).
/// </summary>
/// <remarks>
/// <para>
/// The rows are the keys already in the map, not a fixed set of years. A curriculum has no programme length and
/// <c>TraineeProfile.GetStage</c> is uncapped, so an editor that rendered years 1 to 4 would silently drop a year-5
/// key on the first save.
/// </para>
/// <para>
/// <b>An entry the editor cannot render as a year and a rung is carried, untouched, never dropped.</b> A key that is
/// not a positive whole number, a value that is not a level from 1 to 20, an earlier spelling of a year ("1" before
/// "01"), and a stored value that is not a JSON object at all are all kept verbatim, and the editor lists them so they
/// can be seen and removed. Dropping them would change the item without being asked. The command validator refuses
/// every one of them, so the page does not offer to save while any is left (<see cref="HasUnrenderedEntries" />): the
/// administrator removes each, knowingly, first. This is the tool list's rule for a key its vocabulary no longer holds
/// (T122), with the refusal made before the click rather than after it.
/// </para>
/// <para>
/// A year's rung is read exactly as <c>CurriculumItem.ParseStageOverrides</c> reads it, the parser credit uses,
/// including that <b>of two spellings of one year, the last counts</b>: the row shows that one, and the earlier is a
/// kept entry. <see cref="ToJson" /> writes the shape <c>NormalizeStageOverridesJson</c> and the command validator
/// already accept.
/// </para>
/// </remarks>
public sealed class StageMinimaDraft
{
    private readonly List<StageMinimumRow> _rows = [];
    private readonly List<KeptStageEntry> _kept = [];

    private StageMinimaDraft()
    {
    }

    /// <summary>The training years, in order, each with its rung (null until one is picked).</summary>
    public IReadOnlyList<StageMinimumRow> Rows => _rows;

    /// <summary>Entries of the stored object the editor cannot render as a year and a rung, kept as stored.</summary>
    public IReadOnlyList<KeptStageEntry> Kept => _kept;

    /// <summary>The whole stored value, verbatim, when it is not a JSON object; null otherwise.</summary>
    public string? Unreadable { get; private set; }

    /// <summary>Years can be added only to an object: an unreadable value has to be removed first.</summary>
    public bool CanAddYear => Unreadable is null;

    /// <summary>
    /// Whether anything stored is not a year and a rung. The command validator refuses all of it, so the page does
    /// not save until each is removed.
    /// </summary>
    public bool HasUnrenderedEntries => Unreadable is not null || _kept.Count > 0;

    /// <summary>The year <see cref="AddYear" /> adds: the lowest year not yet shown, so a gap is filled first.</summary>
    public int NextYear
    {
        get
        {
            var year = 1;
            while (_rows.Any(row => row.Year == year))
            {
                year++;
            }

            return year;
        }
    }

    public static StageMinimaDraft Parse(string? json)
    {
        var draft = new StageMinimaDraft();
        if (string.IsNullOrWhiteSpace(json))
        {
            return draft;
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException)
        {
            draft.Unreadable = json;
            return draft;
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                draft.Unreadable = json;
                return draft;
            }

            var entries = document.RootElement.EnumerateObject()
                .Select(property => new
                {
                    property.Name,
                    RawValue = property.Value.GetRawText(),
                    Year = TryReadYear(property.Name, out var year) && TryReadLevel(property.Value, out var level)
                        ? (year, level)
                        : ((int Year, int Level)?)null
                })
                .ToList();

            // Credit reads a year spelled twice ("1", then "01") as its LAST spelling, so that is the row.
            var counted = new Dictionary<int, int>();
            for (var index = 0; index < entries.Count; index++)
            {
                if (entries[index].Year is { } read)
                {
                    counted[read.Year] = index;
                }
            }

            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                if (entry.Year is not { } read)
                {
                    draft._kept.Add(new KeptStageEntry(entry.Name, entry.RawValue));
                }
                else if (counted[read.Year] == index)
                {
                    draft._rows.Add(new StageMinimumRow(read.Year) { Level = read.Level });
                }
                else
                {
                    draft._kept.Add(new KeptStageEntry(entry.Name, entry.RawValue, EarlierSpellingOf: read.Year));
                }
            }
        }

        draft._rows.Sort((left, right) => left.Year.CompareTo(right.Year));
        return draft;
    }

    /// <summary>Adds <see cref="NextYear" /> with no rung picked.</summary>
    public void AddYear()
    {
        if (!CanAddYear)
        {
            return;
        }

        _rows.Add(new StageMinimumRow(NextYear));
        _rows.Sort((left, right) => left.Year.CompareTo(right.Year));
    }

    public void RemoveYear(int year) => _rows.RemoveAll(row => row.Year == year);

    public void RemoveKept(KeptStageEntry entry) => _kept.Remove(entry);

    public void RemoveUnreadable() => Unreadable = null;

    /// <summary>
    /// Empties every year's rung, keeping the years. A scale change does this: an ordinal means a different rung on
    /// each ladder, so none is carried across (T125). Kept entries are not rungs and are left alone.
    /// </summary>
    public void ClearLevels()
    {
        foreach (var row in _rows)
        {
            row.Level = null;
        }
    }

    /// <summary>
    /// The JSON the command takes: <c>{"1":3,"2":4}</c>, with any kept entries before the years; the unreadable value
    /// verbatim; or null when there is nothing at all.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Kept entries go first so that an earlier spelling of a year stays earlier. Credit reads the last spelling, and
    /// writing the kept one after the row would quietly make it the one that counts.
    /// </para>
    /// <para>
    /// A year with no rung is written as <c>null</c> rather than left out. The page does not save until every year
    /// has a rung, so this is never reached from it, and if it were the server would refuse it instead of the year
    /// quietly vanishing.
    /// </para>
    /// </remarks>
    public string? ToJson()
    {
        if (Unreadable is not null)
        {
            return Unreadable;
        }

        if (_rows.Count == 0 && _kept.Count == 0)
        {
            return null;
        }

        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            foreach (var entry in _kept)
            {
                writer.WritePropertyName(entry.Name);
                writer.WriteRawValue(entry.RawValue, skipInputValidation: true);
            }

            foreach (var row in _rows)
            {
                var name = row.Year.ToString(CultureInfo.InvariantCulture);
                if (row.Level is int level)
                {
                    writer.WriteNumber(name, level);
                }
                else
                {
                    writer.WriteNull(name);
                }
            }

            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(buffer.ToArray());
    }

    // The same reading as CurriculumItem.ParseStageOverrides: a positive whole-number key.
    private static bool TryReadYear(string name, out int year)
        => int.TryParse(name, NumberStyles.Integer, CultureInfo.InvariantCulture, out year) && year > 0;

    // The same reading as CurriculumItem.ParseStageOverrides: a number, or a string holding one, from 1 to 20.
    private static bool TryReadLevel(JsonElement value, out int level)
    {
        level = 0;
        var read = value.ValueKind switch
        {
            JsonValueKind.Number => value.TryGetInt32(out level),
            JsonValueKind.String => int.TryParse(value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out level),
            _ => false
        };

        return read && level is >= 1 and <= 20;
    }
}

/// <summary>One training year's minimum. <see cref="Level" /> is the rung's ordinal, or null until one is picked.</summary>
public sealed class StageMinimumRow(int year)
{
    public int Year { get; } = year;

    public int? Level { get; set; }
}

/// <summary>A stored entry the editor cannot render as a year and a rung: its key and its JSON value, verbatim.</summary>
/// <param name="EarlierSpellingOf">
/// The year, when the entry is a year and a rung but a later spelling of the same year is the one credit reads.
/// </param>
public sealed record KeptStageEntry(string Name, string RawValue, int? EarlierSpellingOf = null);
