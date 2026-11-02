using System.Globalization;
using Wombat.Application.Features.Adoptions;
using Wombat.Application.Features.Curricula;

namespace Wombat.Web.Components.Pages.Admin.Curricula;

/// <summary>
/// The words of Curricula (T392, flow 15, lane A; boards R3-A01–A15, R2-A12, R2-0-spec-2 § 8, R3-0-spec-3 § 8): the
/// Institutional admin's two sections, the College admin's list, and the Administrator's All curricula and "An
/// institution's curricula". Every count has its singular; every person or institution is named, never a pronoun.
/// </summary>
public static class CurriculaWords
{
    /// <summary>The College admin's subtitle, as built (R3-A12).</summary>
    public const string SubtitleCollege = "Manage curriculum versions and the sub-speciality they apply to.";

    /// <summary>The Administrator's subtitle (R2-A09, N7).</summary>
    public const string SubtitleAdministrator = "Every College's curricula; and, for one institution, what it trains against and may adopt.";

    /// <summary>The Institutional admin's subtitle, naming the institution (R3-A01).</summary>
    public static string SubtitleInstitution(string institutionName)
        => $"The College curricula {institutionName} trains against, and the versions open to it.";

    /// <summary>The same before the institution's name is read, or when it cannot be (NEW: no board draws it).</summary>
    public const string SubtitleInstitutionWithoutName = "The College curricula your institution trains against, and the versions open to it.";

    /// <summary>S19.</summary>
    public const string Loading = "Loading the curricula.";

    /// <summary>R3-A15, T329: never the exception's text.</summary>
    public const string LoadFailed = "Could not load the curricula. Nothing has changed. Try again, or come back in a few minutes.";

    // ---- The Institutional admin's sections ----

    public const string TrainAgainstHeading = "Curricula you train against";
    public const string TrainAgainstCaption = "The curricula your institution trains against, with the state of each";
    public const string StateColumn = "For your institution";
    public const string EmptyTitle = "No curricula yet";
    public const string EmptyBody = "Your institution has not adopted a curriculum yet. Adopt one below before admitting trainees.";

    public const string OpenHeading = "Open for adoption";
    public const string OpenCaption = "College curriculum versions open for adoption";

    public const string OpenNotice =
        "Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.";

    /// <summary>NEW: no board draws an empty "Open for adoption".</summary>
    public const string NothingOpen = "No other College curriculum version is open for adoption.";

    // ---- The College admin's and the Administrator's lists ----

    public const string AllHeading = "All curricula";
    public const string AllCaption = "Every College's curriculum versions, with the College's status. Items counts each institution's own items too.";

    /// <summary>The College admin's caption (R3-A12), naming the College.</summary>
    public static string CollegeCaption(string collegeName) => $"{collegeName}'s curriculum versions";

    /// <summary>The built empty state of a list with nothing to show the College or the Administrator.</summary>
    public const string CollegeEmptyBody = "Create a curriculum before admitting trainees or building progress rules.";

    /// <summary>T9: the College's flag is its release for adoption (T301).</summary>
    public static string Status(bool isOpen) => isOpen ? "Open" : "Held back";

    // ---- The Administrator's "An institution's curricula" (Q18, R3-A09–A11) ----

    public const string InstitutionHeading = "An institution's curricula";
    public const string InstitutionLabel = "Institution";
    public const string ChooseInstitution = "— Select an institution —";
    public const string Show = "Show";
    public const string ChooseHint = "Choose an institution, then Show, to see what it trains against and adopt for it.";

    /// <summary>NEW (build review P8): Show pressed with no institution chosen, under the select, focused.</summary>
    public const string ChooseInstitutionRefusal = "Choose an institution.";

    /// <summary>NEW (build review P1): an adoption made whose list could not be read again; said after the result.</summary>
    public const string ReadAgainFailed = "The list could not be read again: reload the page to see it.";
    public const string InstitutionStateColumn = "For this institution";

    public static string InstitutionTrainAgainstHeading(string institution) => $"Curricula {institution} trains against";

    public static string InstitutionTrainAgainstCaption(string institution)
        => $"The curricula {institution} trains against, with the state of each";

    public static string InstitutionOpenHeading(string institution) => $"Open to {institution}";

    public const string InstitutionOpenNotice =
        "Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline the institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and the institution's own items are not carried over.";

    /// <summary>NEW: the Administrator's chosen institution has adopted nothing (no board draws it).</summary>
    public static string InstitutionEmptyBody(string institution)
        => $"{institution} has not adopted a curriculum yet. Adopt one below before admitting trainees.";

    // ---- A row ----

    /// <summary>"Paediatric EPA Curriculum (11.1)": the name every row, link and dialog uses.</summary>
    public static string Name(string name, string version) => $"{name} ({version})";

    /// <summary>"Paediatrics / Paediatrics".</summary>
    public static string Discipline(string speciality, string subSpeciality) => $"{speciality} / {subSpeciality}";

    public static string Day(DateOnly day) => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>"15 national · 1 your own", or to the Administrator "15 national · 1 Kgosi Kgari Teaching Hospital's own".</summary>
    public static string Items(CurriculumInstitutionStateDto state, bool named)
        => $"{state.NationalItemCount} national · {state.OwnItemCount} {(named ? $"{state.InstitutionName}'s own" : "your own")}";

    /// <summary>Q10: "Adopted on ‹date›" or "Superseded on ‹date›".</summary>
    public static string State(CurriculumInstitutionStateDto state)
        => state.IsActiveAdoption || state.SupersededOn is not { } superseded
            ? $"Adopted on {Day(state.AdoptedOn)}"
            : $"Superseded on {Day(superseded)}";

    /// <summary>The line under a superseded version's state (S2), or null when no registrar is still on it.</summary>
    public static string? StillOnIt(CurriculumInstitutionStateDto state) => state.IsActiveAdoption
        ? null
        : state.RegistrarCount switch
        {
            0 => null,
            1 => "1 registrar still on it",
            var count => $"{count} registrars still on it"
        };

    /// <summary>A2's line under an open version's name (R3-A01, A03, A05; for the Administrator, A10, A11).</summary>
    public static string Relation(AdoptableCurriculumDto row, string? institution)
    {
        var who = institution ?? "your institution";
        return row.Relation switch
        {
            AdoptionRelation.Newer => $"Newer than the version {who} has adopted ({row.AdoptedVersion}).",
            AdoptionRelation.Older => $"Older than the version {who} has adopted ({row.AdoptedVersion}).",
            _ => $"{Capitalised(who)} has adopted no {Discipline(row.SpecialityName, row.SubSpecialityName)} version yet."
        };
    }

    public const string Adopt = "Adopt";
    public const string AdoptAsking = "Adopt…";
    public const string Edit = "Edit";
    public const string ItemsLink = "Items";

    /// <summary>The trigger's words: "Adopt…" where it opens the superseding dialog (Q6, N6), else "Adopt".</summary>
    public static string AdoptWord(bool asks) => asks ? AdoptAsking : Adopt;

    /// <summary>The trigger's name: "Adopt Paediatric EPA Curriculum (11.2)…", "… for Kgosi Kgari Teaching Hospital…".</summary>

    /// <param name="action">The trigger's visible words (<see cref="AdoptWord" />), its "…" kept at the name's end.</param>
    /// <param name="rowName">The row's name as <c>RowNames.Distinct</c> gives it.</param>
    public static string AdoptName(string action, string rowName, string? institution)
        => $"{action.TrimEnd('…')} {rowName}{(institution is null ? null : $" for {institution}")}{(action.EndsWith('…') ? "…" : null)}";

    /// <summary>S15: the Items link of the Administrator's institution section names the institution.</summary>
    /// <param name="action">The link's visible words (<see cref="ItemsLink" />).</param>
    public static string ItemsName(string action, string rowName, string? institution)
        => institution is null ? $"{action} of {rowName}" : $"{action} of {rowName} for {institution}";

    // ---- The superseding dialog (Q6; R3-A03, A08, A11) ----

    public static string DialogTitle(AdoptableCurriculumDto row, string institution)
        => $"Adopt {Name(row.Name, row.Version)} for {institution}?";

    public static string DialogSupersedes(AdoptableCurriculumDto row, string institution)
        => $"It supersedes {Name(row.AdoptedName ?? row.Name, row.AdoptedVersion ?? string.Empty)} for every new admission to {Discipline(row.SpecialityName, row.SubSpecialityName)} at {institution}.";

    /// <summary>S1: the singular for one registrar; the line left out for none.</summary>
    public static string? DialogRegistrars(AdoptableCurriculumDto row) => row.AdoptedRegistrarCount switch
    {
        0 => null,
        1 => $"The registrar on {row.AdoptedVersion} stays on it until moved on their profile.",
        var count => $"The {count} registrars on {row.AdoptedVersion} stay on it until each is moved on their profile."
    };

    /// <summary>The own items not carried over, or null for none; to the Administrator, named for the institution.</summary>
    public static string? DialogOwnItems(AdoptableCurriculumDto row, string? institutionNamed)
    {
        var codes = row.OwnItemsNotCarried;
        if (codes.Count == 0)
        {
            return null;
        }

        var whose = institutionNamed is null ? "Your institution's" : $"{institutionNamed}'s";
        return codes.Count == 1
            ? $"{whose} own item on {row.AdoptedVersion}, {codes[0]}, is not carried over: add it again on {row.Version}."
            : $"{whose} own items on {row.AdoptedVersion}, {List(codes)}, are not carried over: add them again on {row.Version}.";
    }

    public static string DialogConfirm(AdoptableCurriculumDto row) => $"Adopt {row.Version}";

    // ---- Results ----

    /// <summary>
    /// R3-A02, A05: "Paediatric EPA Curriculum (11.2) adopted." in bold, then the rest. To the Administrator the admissions
    /// are named for the institution (NEW). The superseded sentence's singular and its absence for no registrar are NEW.
    /// </summary>
    public static (string Lead, string Tail) Adopted(AdoptionDto adoption, string? institutionNamed)
    {
        var lead = $"{Name(adoption.CurriculumName, adoption.CurriculumVersion)} adopted.";
        var at = institutionNamed is null ? null : $" at {institutionNamed}";
        var rest = $"New admissions to {Discipline(adoption.SpecialityName, adoption.SubSpecialityName)}{at} join it.";
        if (adoption.SupersededName is { } name && adoption.SupersededVersion is { } version)
        {
            var superseded = Name(name, version);
            rest += adoption.SupersededRegistrarCount switch
            {
                0 => $" {superseded} is superseded.",
                1 => $" {superseded} is superseded; its registrar stays on it until moved.",
                var count => $" {superseded} is superseded; its {count} registrars stay on it until each is moved."
            };
        }

        return (lead, rest);
    }

    /// <summary>The typed refusals (T301's R3-A07, and the two built stale-page guards), never the exception's message.</summary>
    public static (string Lead, string? Tail) Refused(AdoptionRefusal reason) => reason switch
    {
        AdoptionRefusal.HeldBack => ("This curriculum version is not open for adoption.", "Its College has held it back, so nothing was adopted."),
        AdoptionRefusal.AlreadyAdopted => ("This institution has already adopted this curriculum version.", null),
        // NEW (build review C4): what adopting would supersede changed after the page was read.
        AdoptionRefusal.Stale => ("Another adoption was made since this page was read, so nothing was adopted.", "The list below has been read again."),
        _ => ("The selected curriculum could not be found.", null)
    };

    /// <summary>NEW (the board's fixed failure sentence, A6, T329): any other failure of an adoption, logged.</summary>
    public const string AdoptFailedLead = "The curriculum could not be adopted.";

    public const string AdoptFailedRest = "Nothing has changed. Try again, or come back in a few minutes.";

    private static string List(IReadOnlyList<string> items)
        => items.Count switch
        {
            1 => items[0],
            2 => $"{items[0]} and {items[1]}",
            _ => $"{string.Join(", ", items.Take(items.Count - 1))} and {items[^1]}"
        };

    private static string Capitalised(string words) => words.Length == 0 ? words : char.ToUpperInvariant(words[0]) + words[1..];
}
