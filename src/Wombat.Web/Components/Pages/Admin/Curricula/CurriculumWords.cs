using System.Globalization;
using Wombat.Application.Features.Curricula;
using Wombat.Domain.Curricula;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Components.Pages.Admin.Curricula;

/// <summary>
/// The words of a curriculum's page (T394, flow 16, lane C; boards R3-D01–D14, D18–D20, R2-D15–D17, R2-0-spec-2 § 9, and
/// build-lanes.md § Decision 3's words and decision 5). Every count has its singular; an institution is named, never a
/// registrar. Words no board draws are marked NEW.
/// </summary>
public static class CurriculumWords
{
    // ---- Titles (T2, T7) ----

    public const string CreateHeading = "Create curriculum";

    /// <summary>While the version is read, or when its read failed (R2-D16, D17).</summary>
    public const string HeadingWhileUnread = "Curriculum";

    /// <summary>The h1 and the tab of a version: "Paediatric EPA Curriculum (11.1)".</summary>
    public static string Heading(CurriculumDto curriculum) => CurriculaWords.Name(curriculum.Name, curriculum.Version);

    public const string CreateSubtitle = "A new version starts held back: no institution can adopt it until you open it.";

    /// <summary>"College of Paediatricians of South Africa · Paediatrics / Paediatrics · effective 2026-01-01" (R3-D06).</summary>
    public static string Subtitle(CurriculumDto curriculum)
        => $"{curriculum.CollegeName} · {CurriculaWords.Discipline(curriculum.SpecialityName, curriculum.SubSpecialityName)} · effective {CurriculaWords.Day(curriculum.EffectiveFrom)}";

    // ---- Loading, failure, Unavailable (R2-D15–D17) ----

    public const string Loading = "Loading this curriculum.";

    public const string LoadFailed = "Could not load this curriculum. Nothing has changed. Try again, or come back in a few minutes.";

    public const string UnavailableHeading = "Curriculum unavailable";

    public const string UnavailableBody = "This curriculum does not exist, or you cannot open it.";

    public const string GoToCurricula = "Go to Curricula";

    // ---- The header's link (T4, M5) ----

    public static string ItemsLink(int count) => Invariant($"Items ({count})");

    /// <summary>"Items (15) of Paediatric EPA Curriculum (11.1)".</summary>
    public static string ItemsName(int count, string name) => $"{ItemsLink(count)} of {name}";

    // ---- Details ----

    /// <summary>M4, as flows 11 and 14 write it (InvitationText, AdmissionText).</summary>
    public const string RequiredMarkLine = RequiredMark.Line;

    /// <summary>N6: a new version's Effective from, the South African day of <paramref name="clock" />, never the server's.</summary>
    public static DateOnly Today(TimeProvider clock) => ProgrammeCalendar.DateOf(clock.GetUtcNow().UtcDateTime);

    public const string DetailsHeading = "Details";
    public const string SubSpecialityLabel = "Sub-speciality";
    public const string NameLabel = "Name";
    public const string VersionLabel = "Version";
    public const string EffectiveFromLabel = "Effective from";
    public const string EffectiveToLabel = "Effective to";

    /// <summary>Q19's read-out help (R3-D06).</summary>
    public const string SubSpecialityFixedHelp =
        "Fixed once a version holds an item or has been adopted. To use another sub-speciality, create a curriculum there.";

    // ---- Open for adoption (T301, N14: the h2 names the box) ----

    public const string OpenHeading = "Open for adoption";
    public const string OpenLabel = "Open for adoption";

    public const string OpenHelp =
        "An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.";

    // ---- The form's actions ----

    public const string Cancel = "Cancel";
    public const string Save = "Save";
    public const string Saving = "Saving…";
    public const string Create = "Create curriculum";
    public const string Creating = "Creating…";
    public const string HoldBackTrigger = "Hold back…";

    // ---- A new version ----

    public const string NewVersionHeading = "A new version";
    public const string NewVersionLabel = "New version";
    public const string Clone = "Clone";
    public const string Cloning = "Cloning…";

    /// <summary>"Clone Paediatric EPA Curriculum (11.1) as a new version".</summary>
    public static string CloneName(string name) => $"Clone {name} as a new version";

    /// <summary>The clone's intro by the version's national items (R3-D06; R3-D05's none; the singular NEW).</summary>
    public static string CloneIntro(int nationalItems) => nationalItems switch
    {
        0 => "This version has no national items yet, so a new version starts with none. It starts held back.",
        1 => "A new version copies this version's 1 national item, with its target, minimum level and tools. An institution's own items are never copied. It starts held back.",
        _ => Invariant($"A new version copies this version's {nationalItems} national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.")
    };

    // ---- Hold back's question (R3-D12, D14; the no-adopter and several-adopter bodies NEW) ----

    public static string HoldBackTitle(string name) => $"Hold back {name}?";

    public const string HoldBackFirst = "No institution can adopt it while it is held back.";

    /// <summary>
    /// The question's middle paragraph: the reach, then whom it changes nothing for ("Kgosi Kgari Teaching Hospital keeps it,
    /// and its registrars stay on it."); for a version nobody has adopted, <see cref="CatalogueReachWords.NoAdoption" />.
    /// </summary>
    /// <remarks>
    /// "Them" is every institution the reach names, its active adopters and then those that superseded it with registrars
    /// still on it (T8). NEW: with no registrar on it at all the registrars' clause is left out, and with one it reads "its
    /// registrar stays on it".
    /// </remarks>
    public static string HoldBackReach(CurriculumReachDto? reach)
    {
        if (reach is null || !reach.Reaches)
        {
            return CatalogueReachWords.NoAdoption;
        }

        var names = reach.AdoptedBy.Concat(reach.SupersededBy).Distinct(StringComparer.Ordinal).ToList();
        var one = names.Count == 1;
        var keeps = $"{CatalogueReachWords.JoinAnd(names)} {(one ? "keeps" : "keep")} it";
        // T394 fix pass (pages N5): the verb follows the registrars' count, as flow 15's "its registrar stays on it" does.
        var registrars = (reach.RegistrarCount + reach.SupersededRegistrarCount) switch
        {
            0 => null,
            1 => $", and {(one ? "its" : "their")} registrar stays on it",
            _ => $", and {(one ? "its" : "their")} registrars stay on it"
        };
        return $"{CatalogueReachWords.ForCurriculum(reach)} Holding it back changes nothing for them: {keeps}{registrars}.";
    }

    public const string HoldBackLast = "You can open it again on this page.";

    public const string HoldBackConfirm = "Hold back";

    // ---- Results (Q5, T7) ----

    /// <summary>R3-D05.</summary>
    public static string Created(CurriculumDto curriculum)
        => $"{Heading(curriculum)} created. It is held back: no institution can adopt it until you open it.";

    /// <summary>R3-D08's plural; the singular and the none NEW.</summary>
    public static string Cloned(CurriculumDto clone, string fromVersion) => clone.NationalItemCount switch
    {
        0 => $"{Heading(clone)} cloned from {fromVersion}, with no national items. It is held back.",
        1 => $"{Heading(clone)} cloned from {fromVersion}, with its 1 national item. It is held back.",
        var count => Invariant($"{Heading(clone)} cloned from {fromVersion}, with its {count} national items. It is held back.")
    };

    /// <summary>T7: the save's result by the box as stored.</summary>
    public static string Saved(bool isOpen) => isOpen
        ? "Curriculum saved. It is open for adoption: an institution can adopt it now."
        : "Curriculum saved. It is held back: no institution can adopt it until you open it again.";

    // ---- Refusals and failures ----

    /// <summary>
    /// Q19's refusal by its cause (decision 5, NEW): "Not saved. This version now holds items, …", or "Not saved. Kgosi Kgari
    /// Teaching Hospital has adopted this version, …" (two by name; three or more, the first two and "‹N› other
    /// institutions").
    /// </summary>
    public static string SubSpecialityFixed(CurriculumSubSpecialityFixedException refusal)
    {
        ArgumentNullException.ThrowIfNull(refusal);
        const string Tail = "so its sub-speciality can no longer change. The page has been read again.";
        var adopters = refusal.AdoptedBy;
        if (refusal.Cause == CurriculumSubSpecialityFixedCause.HoldsItems || adopters.Count == 0)
        {
            return $"Not saved. This version now holds items, {Tail}";
        }

        IReadOnlyList<string> named = adopters.Count <= 2
            ? adopters
            : [adopters[0], adopters[1], adopters.Count == 3 ? "1 other institution" : Invariant($"{adopters.Count - 2} other institutions")];
        return $"Not saved. {CatalogueReachWords.JoinAnd(named)} {(adopters.Count == 1 ? "has" : "have")} adopted this version, {Tail}";
    }

    public const string CreateFailed = "The curriculum could not be created. Nothing has changed. Try again, or come back in a few minutes.";

    public const string SaveFailed = "The curriculum could not be saved. Nothing has changed. Try again, or come back in a few minutes.";

    public const string CloneFailed = "The new version could not be made. Nothing has changed. Try again, or come back in a few minutes.";

    private static string Invariant(FormattableString value) => value.ToString(CultureInfo.InvariantCulture);
}
