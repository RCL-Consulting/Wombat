using System.Globalization;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Curricula;

namespace Wombat.Web.Components.Pages.Admin.Curricula;

/// <summary>
/// The words of a curriculum's items page and an item's page of its own (T392, flow 15, lane B; boards R3-B01–B24 and the
/// round-2 B04, B05, B09, B12, B15, B16 that stand). The built sentences are kept word for word (build-contracts § 5); the
/// em dash is <see cref="EpaOptionLabel" />'s (S10). Both pages print only what is here, so their tests and the runbook read
/// one source.
/// </summary>
internal static class CurriculumItemsWords
{
    // ---- the list (CurriculumItemsEdit) ----

    /// <summary>The h1 and the tab while the curriculum is read, or when it could not be (T7, R3-B18, B19).</summary>
    public const string ListPageName = "Curriculum items";

    /// <summary>The loading line (S19, R3-B18).</summary>
    public const string ListLoading = "Loading this curriculum's items.";

    /// <summary>The load failure (S7, R3-B19), the exception logged, never printed (T329).</summary>
    public const string ListLoadFailed =
        "Could not load this curriculum's items. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>The h1, the tab and the crumb: "Items of Paediatric EPA Curriculum (11.1)" (T10).</summary>
    public static string ListTitle(CurriculumDto curriculum) => $"Items of {VersionName(curriculum)}";

    /// <summary>"Paediatric EPA Curriculum (11.2)".</summary>
    public static string VersionName(CurriculumDto curriculum) => $"{curriculum.Name} ({curriculum.Version})";

    /// <summary>The table's caption, which does not repeat the heading (T10).</summary>
    public static string Caption(CurriculumDto curriculum) => $"{VersionName(curriculum)}'s items, in EPA order";

    /// <summary>The section's h2: "Items (15)" (T10).</summary>
    public static string ItemsHeading(int count) => string.Create(CultureInfo.InvariantCulture, $"Items ({count})");

    /// <summary>
    /// The subtitle (R3-B01, B17, B21): the College, the discipline and the effective day, then, for an institution, its
    /// adoption ("adopted by your institution on 2026-10-30") or F6's supersession ("superseded on 2026-10-30 by Paediatric
    /// EPA Curriculum (11.2); 2 registrars still on it"). The College and the Administrator read no institution's state.
    /// </summary>
    public static string Subtitle(CurriculumDto curriculum)
    {
        var head = $"{curriculum.CollegeName} · {curriculum.SpecialityName} / {curriculum.SubSpecialityName} · effective {QuotaText.Iso(curriculum.EffectiveFrom)}";
        return curriculum.Institution switch
        {
            { IsActiveAdoption: true } state => $"{head} · adopted by your institution on {QuotaText.Iso(state.AdoptedOn)}",
            { SupersededOn: { } on } state => $"{head} · superseded on {QuotaText.Iso(on)}{SupersededBy(state)}{StillOnIt(state.RegistrarCount)}",
            _ => head
        };
    }

    private static string SupersededBy(CurriculumInstitutionStateDto state)
        => state.SupersededByName is { } name ? $" by {name} ({state.SupersededByVersion})" : string.Empty;

    // NEW for none (no board draws a superseded version with no registrar on it): the clause is left out, as the
    // adoption dialog leaves out its registrars' line for none (R3-0-spec-2 § 6).
    private static string StillOnIt(int registrars) => registrars switch
    {
        0 => string.Empty,
        1 => "; 1 registrar still on it",
        _ => string.Create(CultureInfo.InvariantCulture, $"; {registrars} registrars still on it")
    };

    /// <summary>The College's and the Administrator's standing notice (built; R3-B17, B23).</summary>
    public const string CollegeNotice =
        "A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.";

    /// <summary>An institution's standing notice (built, T211; R3-B01).</summary>
    public const string InstitutionNotice =
        "The College sets this curriculum's national items, so they are read only here. Your institution's own items apply only to its trainees, and you can add, edit and remove them. A target and its period are read live: changing one changes what every trainee of your institution on this curriculum is measured against, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.";

    /// <summary>The paused notice (built, T158; R3-B02).</summary>
    public static string PausedNotice(IReadOnlyList<string> codes)
        => (codes.Count == 1
               ? $"{codes[0]} is inactive, so its item is not in force: no trainee can choose it for a new activity, its credit is paused, and no running programme's progress page or dashboard shows it (a programme that has ended keeps it on its record)."
               : $"{string.Join(", ", codes)} are inactive, so their items are not in force: no trainee can choose them for a new activity, their credit is paused, and no running programme's progress page or dashboard shows them (a programme that has ended keeps them on its record).")
           + " Reactivating an EPA on its own page brings its item back, and credits what was completed against it meanwhile.";

    /// <summary>"(inactive: not in force)" beside a paused EPA's title (built, T158).</summary>
    public const string NotInForce = "(inactive: not in force)";

    /// <summary>The add link beside "Items (n)", and the add page's h1 and crumb (F3).</summary>
    public static string AddAction(bool canEditCurriculum)
        => canEditCurriculum ? "Add item" : "Add your institution's own item";

    /// <summary>
    /// The line where the add link would be when no EPA is left to add (built, T222): the College's words where the caller
    /// writes the College's items, else the institution's.
    /// </summary>
    public static string NothingToAdd(CurriculumDto curriculum)
        => curriculum.CanEditCurriculum
            ? $"Every national EPA of {curriculum.SubSpecialityName} is already on this curriculum, as a national item or as an institution's own item, so there is none left to add. To add an item, first add a national EPA of {curriculum.SubSpecialityName}, or remove a national item to free its EPA. An EPA an institution has as its own item is freed only when that institution removes it."
            : $"Every EPA your institution could add is already on this curriculum: each national EPA of {curriculum.SubSpecialityName}, and each of your institution's own EPAs of {curriculum.SubSpecialityName}. To add an item of your own, first create a local EPA of {curriculum.SubSpecialityName}.";

    /// <summary>F6's line in place of the add link on a version the institution has superseded (S12, R3-B21).</summary>
    public const string Superseded =
        "Your institution has superseded this version, so no item is added to it. Its own items stay for the registrars still on it.";

    /// <summary>
    /// B07's line on a version that holds none of the institution's own items while another version of it does (R3-B07,
    /// B14): "A new version carries no institution's own items. Your item on 11.1 stays there." Null when there is none.
    /// </summary>
    public static string? NewVersionLine(CurriculumInstitutionStateDto? state)
    {
        if (state is not { OwnItemCount: 0, OwnItemsElsewhere.Count: > 0 })
        {
            return null;
        }

        // NEW beyond one item on one version (B07 draws one): the plural, and one clause a version.
        var where = string.Join(" ", state.OwnItemsElsewhere.Select(other => other.Count == 1
            ? $"Your item on {other.Version} stays there."
            : string.Create(CultureInfo.InvariantCulture, $"Your {other.Count} items on {other.Version} stay there.")));
        return $"A new version carries no institution's own items. {where}";
    }

    /// <summary>The result after an add, by <c>?added</c> (built; R3-B06, B23).</summary>
    public const string Added = "Curriculum item added.";

    /// <summary>The result after a save, by <c>?saved</c> (built; R3-B10).</summary>
    public const string Saved = "Curriculum item saved.";

    /// <summary>The result after a Remove (built, T11; R3-B14).</summary>
    public static string Removed(string itemName) => $"{itemName} removed from this curriculum.";

    /// <summary>A Remove that failed for no reason the command names (NEW): fixed words, the exception logged (T329).</summary>
    public static string RemoveFailed(string itemName)
        => $"{itemName} could not be removed. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>The Remove dialog's title, naming its item (T3, R3-B13).</summary>
    public static string RemoveTitle(string itemName, CurriculumDto curriculum)
        => $"Remove {itemName} from {VersionName(curriculum)}?";

    /// <summary>The Remove dialog's body (built, T222; its dash the em dash, S10; R3-B13).</summary>
    public static string RemoveBody(CurriculumItemDto item, CurriculumDto curriculum)
        => $"Remove {Label(item)} from {VersionName(curriculum)}? Its target, minima, decision and tool list are deleted, and so is each trainee's tally toward it. {NoLongerMeasured(item)} This cannot be undone.";

    private static string NoLongerMeasured(CurriculumItemDto item)
        => !item.IsLocal
            ? "Trainees on this curriculum, in every institution that has adopted it, are no longer measured against this EPA."
            : item.OwningInstitutionName is { Length: > 0 } owner
                ? $"{owner}'s trainees on this curriculum are no longer measured against this EPA."
                : "Your institution's trainees on this curriculum are no longer measured against this EPA.";

    /// <summary>The Remove dialog's confirm (built).</summary>
    public const string RemoveConfirm = "Remove item";

    /// <summary>A national row's actions cell beside View (M1; P2).</summary>
    public const string SetByTheCollege = "Set by the College.";

    /// <summary>Another institution's item that the caller reads and may not change (built, T211).</summary>
    public const string AnotherInstitutionsItem = "Another institution's item.";

    /// <summary>
    /// Whose own item a local item is (built, T222): its institution's to a caller whose reads span institutions (the
    /// Administrator, R3-B17), else "Your institution's own item".
    /// </summary>
    public static string OwnerText(CurriculumItemDto item)
        => item.OwningInstitutionName is { Length: > 0 } owner ? $"{owner}'s own item" : "Your institution's own item";

    /// <summary>"PAED-001 — Providing paediatric emergency care to children" (S10).</summary>
    public static string Label(CurriculumItemDto item) => EpaOptionLabel.For(item.EpaCode, item.EpaTitle, inForce: true);

    // ---- out of scope (Q19) ----

    public const string CurriculumUnavailable = "Curriculum unavailable";

    public const string CurriculumUnavailableBody = "This curriculum does not exist, or you cannot open it.";

    public const string GoToCurricula = "Go to Curricula";

    public const string ItemUnavailable = "Item unavailable";

    public const string ItemUnavailableBody = "This curriculum item does not exist, or you cannot change it.";

    /// <summary>NEW (build review P3): the add page's h1 and tab on a version the institution has superseded; its body is <see cref="Superseded" />.</summary>
    public const string CannotAddItem = "Cannot add an item";

    /// <summary>"Go to Items of Paediatric EPA Curriculum (11.2)" (R2-B15).</summary>
    public static string GoToItems(CurriculumDto curriculum) => $"Go to {ListTitle(curriculum)}";

    // ---- an item's page (CurriculumItemPage) ----

    /// <summary>The h1 and the tab while the item's page reads, or when it could not (NEW: T7 named only the list's).</summary>
    public const string ItemPageName = "Curriculum item";

    /// <summary>The loading line (NEW, S19's form).</summary>
    public const string ItemLoading = "Loading this curriculum item.";

    /// <summary>The load failure (NEW, S7's form).</summary>
    public const string ItemLoadFailed =
        "Could not load this curriculum item. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>The add page's subtitle: a national item (F3, R3-B22), or the institution's own (R2-B04).</summary>
    public static string AddSubtitle(CurriculumDto curriculum)
        => curriculum.CanEditCurriculum
            ? NationalSubtitle(curriculum)
            : curriculum.Institution?.InstitutionName is { Length: > 0 } institution
                ? $"On {VersionName(curriculum)}. It applies only to {institution}'s trainees."
                : $"On {VersionName(curriculum)}. It applies only to your institution's trainees.";

    /// <summary>A national item's subtitle on its add and edit pages (F3, R3-B22, B24).</summary>
    public static string NationalSubtitle(CurriculumDto curriculum)
        => $"On {VersionName(curriculum)}. It applies to every institution that has adopted it.";

    /// <summary>The edit page's h1: "Edit KGK-001 — Running a paediatric outreach clinic at a district hospital" (R2-B09, R3-B24).</summary>
    public static string EditTitle(CurriculumItemDto item) => $"Edit {Label(item)}";

    /// <summary>The edit page's subtitle: a national item's (R3-B24), or "Your institution's own item on …" (R2-B09).</summary>
    public static string EditSubtitle(CurriculumItemDto item, CurriculumDto curriculum)
        => item.IsLocal ? $"{OwnerText(item)} on {VersionName(curriculum)}" : NationalSubtitle(curriculum);

    /// <summary>A national item read by a caller who may not change it (P2, NEW): its record's subtitle.</summary>
    public static string RecordSubtitle(CurriculumDto curriculum)
        => $"A national item on {VersionName(curriculum)}. The College sets it, so it is read only here.";

    /// <summary>P2's link back to the list, which carries <c>?back=</c> (NEW).</summary>
    public static string BackToItems(CurriculumDto curriculum) => $"Back to {ListTitle(curriculum)}";

    /// <summary>A national row's link to its record (P2).</summary>
    public const string View = "View";

    /// <summary>The add page's submit (built).</summary>
    public const string AddSubmit = "Add item";

    /// <summary>The edit page's submit (F3, R2-B09).</summary>
    public const string SaveSubmit = "Save item";

    /// <summary>An add that failed for no reason a field names (NEW): fixed words, the exception logged (T329).</summary>
    public const string AddFailed = "The item could not be added. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>A save that failed for no reason a field names (NEW): fixed words, the exception logged (T329).</summary>
    public const string SaveFailed = "The item could not be saved. Nothing has changed. Try again, or come back in a few minutes.";

    // ---- the item's values, as the list's cells and the record print them ----

    /// <summary>"3 per semester" (T130).</summary>
    public static string TargetText(CurriculumItemDto item)
        => string.Create(CultureInfo.InvariantCulture,
            $"{item.RequiredCount} per {(item.QuotaPeriod == QuotaPeriod.Semester ? "semester" : "academic year")}");

    /// <summary>The item's entrustment decision, as the target's second line (T131): "Decided each semester, by the …".</summary>
    public static string DecisionText(CurriculumItemDto item)
    {
        var cadence = item.DecisionCadence switch
        {
            QuotaPeriod.Semester => "Decided each semester",
            QuotaPeriod.AcademicYear => "Decided each academic year",
            _ => "No decision cadence"
        };
        var opportunity = item.DecisionIsOpportunistic ? ", as opportunity allows" : string.Empty;
        var body = item.DecisionBodyName is { } name ? $", by the {name}" : string.Empty;
        return $"{cadence}{opportunity}{body}";
    }

    /// <summary>"Not pinned", the scale of an item on no ladder (built).</summary>
    public const string NotPinned = "Not pinned";

    /// <summary>"12 months" (P2's record; decision 1 took it off the table).</summary>
    public static string WindowText(CurriculumItemDto item) => string.Create(CultureInfo.InvariantCulture, $"{item.WindowMonths} months");

    /// <summary>The weight, or the em dash for none.</summary>
    public static string WeightText(CurriculumItemDto item) => item.Weight?.ToString("0.##", CultureInfo.InvariantCulture) ?? "—";

    /// <summary>"No year has its own minimum." (StageMinimaEditor's own words), on the record.</summary>
    public const string NoYearMinimum = "No year has its own minimum.";

    /// <summary>
    /// The rung as the College prints it, resolved from the loaded ladders (T125, T100): the ordinal when the item is
    /// unpinned; the ordinal, marked, when it is not a rung on its pinned ladder (built).
    /// </summary>
    public static string RungText(IReadOnlyList<EntrustmentScaleDto> scales, int? scaleId, int order)
    {
        var ordinal = order.ToString(CultureInfo.InvariantCulture);
        var scale = scaleId is int id ? scales.FirstOrDefault(candidate => candidate.Id == id) : null;
        return scale is null
            ? ordinal
            : scale.Levels.FirstOrDefault(level => level.Order == order)?.Label ?? $"{ordinal} (not a rung on {scale.Name})";
    }

    /// <summary>
    /// The minimum by training year folded into one cell (T10, R3-B01): "3a / 3b / 4 / 5" when the years run 1, 2, 3 … with
    /// none missing, each named ("Year 3: 3b / Year 4: 4") when they do not (NEW: no board draws a gap), the em dash for
    /// none. An entry the editor cannot read as a year and a rung is shown as stored (built, T125).
    /// </summary>
    public static string YearsText(IReadOnlyList<EntrustmentScaleDto> scales, CurriculumItemDto item)
    {
        var draft = StageMinimaDraft.Parse(item.MinimumLevelByStageJson);
        if (draft.Unreadable is { } unreadable)
        {
            return unreadable;
        }

        var rows = draft.Rows.ToList();
        var consecutive = rows.Select((row, index) => row.Year == index + 1).All(match => match);
        var parts = rows
            .Select(row => (row.Year, Rung: row.Level is int level ? RungText(scales, item.ScaleId, level) : "—"))
            .Select(row => consecutive ? row.Rung : string.Create(CultureInfo.InvariantCulture, $"Year {row.Year}: {row.Rung}"))
            .Concat(draft.Kept.Select(entry => $"{entry.Name}: {entry.RawValue}"))
            .ToList();
        return parts.Count == 0 ? "—" : string.Join(" / ", parts);
    }

    /// <summary>The year rows, each named, for the record (P2): "Year 1: 2", or <see cref="NoYearMinimum" />.</summary>
    public static IReadOnlyList<string> YearLines(IReadOnlyList<EntrustmentScaleDto> scales, CurriculumItemDto item)
    {
        var draft = StageMinimaDraft.Parse(item.MinimumLevelByStageJson);
        if (draft.Unreadable is { } unreadable)
        {
            return [unreadable];
        }

        return draft.Rows
            .Select(row => string.Create(CultureInfo.InvariantCulture,
                $"Year {row.Year}: {(row.Level is int level ? RungText(scales, item.ScaleId, level) : "—")}"))
            .Concat(draft.Kept.Select(entry => $"{entry.Name}: {entry.RawValue}"))
            .ToList();
    }

    /// <summary>The tools by name, a key the vocabulary no longer holds by its key (T122), or "Any instrument" (built).</summary>
    public static string ToolsText(IReadOnlyList<WbaToolDto> tools, IEnumerable<string> toolKeys)
    {
        var names = toolKeys
            .Select(key => tools.FirstOrDefault(tool => tool.Key == key)?.Name ?? key)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();
        return names.Count == 0 ? "Any instrument" : string.Join(", ", names);
    }
}
