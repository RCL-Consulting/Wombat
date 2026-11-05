namespace Wombat.Application.Features.Epas.Commands.UpdateEntrustmentScale;

/// <summary>Why a scale's save was refused (T394, flow 16, Q18): the page words each from the refusal's data.</summary>
public enum EntrustmentScaleUpdateRefusal
{
    /// <summary>Q6: a pinned curriculum item needs a rung the new levels leave out (R3-C11). "Put … back" is offered.</summary>
    RungNeeded,

    /// <summary>The ended record's (T312, R3-C13): a programme's closed record keeps a removed rung as its minimum.</summary>
    RungOnAnEndedRecord,

    /// <summary>Built (T109): an entrustment decision is on a removed rung.</summary>
    RungDecided,

    /// <summary>Q16, T395 (R3-C12): a reorder, or a removal no item needs, on a scale something uses. Nothing to put back.</summary>
    InUse
}

/// <summary>
/// One curriculum item that needs a rung of this scale, by its EPA's code, its curriculum's name and version, and the
/// rung by its id and label (Q6: never by rank or by the item's id).
/// </summary>
public sealed record EntrustmentScaleRungNeed(
    string EpaCode,
    string CurriculumName,
    string CurriculumVersion,
    int LevelId,
    string RungLabel);

/// <summary>
/// The update's own refusal of a scale's levels (T394, flow 16, lane B; Q6, Q16, Q18, T395). Typed, so the scale's page
/// words it from <see cref="Reason" />, <see cref="Needs" /> and <see cref="MoreCount" />, and shows no exception's text
/// (T329, T393). Thrown before anything is assigned (T201): the audit pipeline commits whatever a refused handler staged.
/// </summary>
/// <remarks>
/// <see cref="Exception.Message" /> is the same sentence in plain text, for the caller's audit row; the Administrator alone
/// can save a scale, and every curriculum and EPA is the Administrator's to read, so it names no one out of scope.
/// </remarks>
public sealed class EntrustmentScaleUpdateRefusedException : InvalidOperationException
{
    public EntrustmentScaleUpdateRefusedException(
        EntrustmentScaleUpdateRefusal reason,
        IReadOnlyList<EntrustmentScaleRungNeed> needs,
        int moreCount,
        IReadOnlyList<int> rungsToPutBack,
        string message)
        : base(message)
    {
        Reason = reason;
        Needs = needs;
        MoreCount = moreCount;
        RungsToPutBack = rungsToPutBack;
    }

    public EntrustmentScaleUpdateRefusal Reason { get; }

    /// <summary>
    /// For <see cref="EntrustmentScaleUpdateRefusal.RungNeeded" />, the first three needs, by EPA code, curriculum and rung
    /// order; for <see cref="EntrustmentScaleUpdateRefusal.RungOnAnEndedRecord" />, the one need named. Empty otherwise.
    /// </summary>
    public IReadOnlyList<EntrustmentScaleRungNeed> Needs { get; }

    /// <summary>How many needs past <see cref="Needs" /> ("and 37 more."): 0 when all are named.</summary>
    public int MoreCount { get; }

    /// <summary>
    /// The removed rungs (by level id) a curriculum item needs, each a "Put rung … back" the page offers (S14). Only for
    /// <see cref="EntrustmentScaleUpdateRefusal.RungNeeded" />: after Q16's refusal the page is read again instead.
    /// </summary>
    public IReadOnlyList<int> RungsToPutBack { get; }
}

/// <summary>
/// The update's refusals in words (T394, flow 16, Q6, Q18, Spec-2 § 9; R3-C11, C12, C13): one source for the audit row's
/// message and the scale's page, which composes them from the refusal's data. A rung is named by its label in curly quotes,
/// as the boards write it, never by its rank.
/// </summary>
public static class EntrustmentScaleRefusalWords
{
    /// <summary>The lead of every refusal the page shows in its result region.</summary>
    public const string NotSaved = "Not saved.";

    /// <summary>Q16 (R3-C12): the page is read again, the moves gone.</summary>
    public const string InUse =
        "Not saved. Rungs can be reordered or removed only while nothing uses this scale, and this scale is now in use. " +
        "The page has been read again.";

    /// <summary>Built (T109): a removed rung an entrustment decision is on.</summary>
    public const string RungDecided = "One or more levels are referenced by entrustment decisions and cannot be removed.";

    /// <summary>One item's need (Q6, R3-C11).</summary>
    public static string Need(EntrustmentScaleRungNeed need)
        => $"{need.EpaCode} on {need.CurriculumName} ({need.CurriculumVersion}) needs the rung “{need.RungLabel}” of this " +
           "scale, which the new levels leave out. Change that item's minimum first.";

    /// <summary>The tail joined to the third need (N5): " and 37 more.".</summary>
    public static string More(int moreCount) => $" and {moreCount} more.";

    /// <summary>The ended record's (R3-C13), whole.</summary>
    public static string OnAnEndedRecord(EntrustmentScaleRungNeed need)
        => $"Not saved. {need.EpaCode} on {need.CurriculumName} ({need.CurriculumVersion}) was on the record of a programme " +
           $"that has ended, which keeps the rung “{need.RungLabel}” of this scale as its minimum. A closed record keeps the " +
           $"targets it ended with, so the rung “{need.RungLabel}” cannot be removed.";

    /// <summary>S14: the button that re-inserts a removed rung in its place.</summary>
    public static string PutBack(string rungLabel) => $"Put rung “{rungLabel}” back";

    /// <summary>The needs as the page lists them, each a sentence, the third carrying the tail.</summary>
    public static IReadOnlyList<string> NeedLines(IReadOnlyList<EntrustmentScaleRungNeed> needs, int moreCount)
        => needs.Select((need, index) => index == needs.Count - 1 && moreCount > 0 ? Need(need) + More(moreCount) : Need(need))
            .ToList();
}
