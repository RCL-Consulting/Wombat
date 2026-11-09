using Wombat.Application.Features.Epas;
using Wombat.Domain.Epas;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Components.Pages.Admin.Epas;

/// <summary>
/// The words of the EPA list and an EPA's page (T392, flow 15, lane C; boards R3-C08–C18b and the R2-C boards that stand,
/// R2-0-spec-2 § 8, R3-0-spec-3 § 8). One place, so the pages and their tests say the same, and no page prints an enum's
/// name (T324) or an exception's text (T329).
/// </summary>
public static class EpaWords
{
    // ---- The list ----

    public const string ListHeading = "EPAs";

    public const string CreateEpa = "Create EPA";

    /// <summary>S19: the list while it is read.</summary>
    public const string LoadingList = "Loading the EPAs.";

    /// <summary>R3-C17 (S7, T329): the list's read failed. Never the exception's text.</summary>
    public const string ListLoadFailed = "Could not load the EPAs. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>
    /// R3-E01 (T394, flow 16, T326, S15): the CollegeAdmin's subtitle, which tells them how far a change to one reaches.
    /// Until T394 it was the built "Maintain the entrustable professional activities available to curricula and forms."
    /// </summary>
    public static string CollegeSubtitle(string collegeName)
        => $"{collegeName}'s EPAs. A change to one is read at once wherever it is named, in every institution that has adopted a curriculum that holds it.";

    /// <summary>R3-C15: the Administrator's subtitle.</summary>
    public const string AdministratorSubtitle = "Every College's EPAs, and every institution's own.";

    /// <summary>R2-C01–C03, R3-C09: the InstitutionalAdmin's subtitle (T326's kind: a reader, not an editor).</summary>
    public static string InstitutionSubtitle(string? institutionName)
        => $"The EPAs of the curricula {institutionName ?? "your institution"} has adopted. The College's are read only; your institution's own are yours to change.";

    /// <summary>The list's subtitle by whose list it is; none for a College admin whose account names no College (R2-E19).</summary>
    public static string? ListSubtitle(EpaListDto list) => list.Reader switch
    {
        EpaListReader.Administrator => AdministratorSubtitle,
        EpaListReader.College => list.CollegeName is { } college ? CollegeSubtitle(college) : null,
        _ => InstitutionSubtitle(list.InstitutionName)
    };

    /// <summary>
    /// The table's caption, which does not repeat the heading: R3-C09's, R3-E04's, and R3-E01's for the College ("College of
    /// Paediatricians of South Africa's EPAs", T394; until then "Entrustable professional activities").
    /// </summary>
    public static string Caption(EpaListDto list) => list.Reader switch
    {
        EpaListReader.Administrator => "Every EPA",
        EpaListReader.College => list.CollegeName is { } college ? $"{college}'s EPAs" : "Entrustable professional activities",
        _ => "EPAs your institution reads"
    };

    /// <summary>
    /// T3 (T394, R3-E01): the College column, for a list that spans Colleges. The College admin's are all their College's,
    /// and the institution's (flow 15's) keep it.
    /// </summary>
    public static bool ShowsCollegeColumn(EpaListReader reader) => reader != EpaListReader.College;

    /// <summary>R2-E19 (T394, S12's words): a College admin whose account names no College, in place of the list.</summary>
    public const string NoCollegeTitle = "No College on your account";

    public const string NoCollegeBody = "Your account names no College. Ask the platform administrator to add it.";

    /// <summary>Whether the list is a College admin's whose account names no College (R2-E19): no list, no Create EPA.</summary>
    public static bool NamesNoCollege(EpaListDto list) => list is { Reader: EpaListReader.College, CollegeName: null };

    public const string EmptyTitle = "No EPAs yet";

    /// <summary>The built empty body, for a caller who may create the first EPA.</summary>
    public const string EmptyBodyMayCreate = "Create the first EPA to start building curricula and assessment forms.";

    /// <summary>R2-C01 (Q12): an institution with no adopted discipline has nothing to add an EPA to.</summary>
    public const string EmptyBodyNothingAdopted =
        "Your institution has not adopted a curriculum yet, so there is no discipline to add an EPA to. Adopt one under Curricula.";

    public const string GoToCurricula = "Go to Curricula";

    /// <summary>A national row's actions cell for a caller who may not change it (Q3, M1; DESIGN.md § Table system, T211).</summary>
    public const string SetByTheCollege = "Set by the College.";

    public const string Edit = "Edit";

    /// <summary>S14, P5: the pager's landmark and its size select's label.</summary>
    public const string PagerLabel = "Pages of EPAs";

    public const string PageSizeLabel = "EPAs per page";

    public static string Status(bool isActive) => isActive ? "Active" : "Inactive";

    /// <summary>
    /// An own EPA's mark under its title (Q3; DESIGN.md § Table system, T211): "Your institution's own EPA", or, to a caller
    /// whose reads span institutions, "Kgosi Kgari Teaching Hospital's own EPA" (the query decides, <see cref="EpaDto.NamesOwner" />).
    /// Null for a national EPA.
    /// </summary>
    public static string? OwnerMark(EpaDto epa)
        => epa.OwningInstitutionId is null ? null
            : epa.NamesOwner && epa.OwningInstitutionName is { Length: > 0 } owner ? $"{owner}'s own EPA"
            : "Your institution's own EPA";

    /// <summary>"PAED-001 — Providing paediatric emergency care to children": every EPA a page names, with the em dash (S10).</summary>
    public static string Label(EpaDto epa) => EpaOptionLabel.For(epa.Code, epa.Title, inForce: true);

    // ---- An EPA's page ----

    /// <summary>T7: the page's title while it is read, and when its read failed.</summary>
    public const string PageHeadingWhileUnread = "EPA";

    public const string CreateHeading = CreateEpa;

    /// <summary>
    /// The built subtitle: the form of an institution's own EPA (flow 15's), and the Administrator's create, which names no
    /// College because its picker is every College's.
    /// </summary>
    public const string FormSubtitle = "Define the EPA code, scope, and descriptive guidance.";

    /// <summary>
    /// R3-E05, E20 (T394, S15): a national EPA's form names its College and its discipline, "College of Paediatricians of
    /// South Africa · Paediatrics / Paediatrics", for the College admin and the Administrator alike.
    /// </summary>
    public static string NationalFormSubtitle(EpaDto epa) => $"{epa.CollegeName} · {epa.DisciplineName}";

    /// <summary>R3-E10 (T394, S15): the College admin's create.</summary>
    public static string CreateNationalSubtitle(string collegeName)
        => $"A national EPA of {collegeName}. It reaches a registrar once a curriculum an institution has adopted holds it.";

    /// <summary>R3-E05, E10 (T394, S15): the College admin's picker help.</summary>
    public static string SubSpecialityHelpCollege(string collegeName) => $"{collegeName}'s sub-specialities.";

    /// <summary>R3-E20 (T394, N16): the Administrator's picker help.</summary>
    public const string SubSpecialityHelpEveryCollege = "Every College's sub-specialities.";

    /// <summary>
    /// R3-E05, E06, E09, E11 (T394, Q21's EV2): the standing reach line on a national EPA's page for a caller who may change
    /// it, the reach (<see cref="CatalogueReachWords.ForNationalEpa" />) then what a change to it reaches.
    /// </summary>
    public static string ReachLine(EpaDto epa, Wombat.Application.Features.Curricula.EpaReachDto reach)
        => $"{CatalogueReachWords.ForNationalEpa(epa.Code, reach)} Its code, title and description are read at once wherever it is named.";

    /// <summary>R3-E07 (T394, Q2): the pause dialog's first paragraph, the reach alone.</summary>
    public static string DeactivateReach(EpaDto epa, Wombat.Application.Features.Curricula.EpaReachDto reach)
        => CatalogueReachWords.ForNationalEpa(epa.Code, reach);

    /// <summary>M4 (DESIGN.md § Form system): the line above a form with a starred field, as flows 11 and 14 write it.</summary>
    public const string RequiredMarkLine = RequiredMark.Line;

    /// <summary>R2-C04: the create of an institution's own EPA.</summary>
    public static string CreateOwnSubtitle(string? institutionName)
        => $"An EPA of your institution's own. It applies only to {institutionName ?? "your institution"}'s trainees.";

    /// <summary>R2-C10: the national EPA's record, for a reader who may not change it (Q2).</summary>
    public static string RecordSubtitle(EpaDto epa) => $"{epa.CollegeName} · {epa.DisciplineName} · Set by the College, so read only here";

    /// <summary>S19: an EPA's page while it is read.</summary>
    public const string LoadingEpa = "Loading this EPA.";

    /// <summary>R3-C18b (T329): the page's own read failed, which is not "unavailable".</summary>
    public const string EpaLoadFailed = "Could not load this EPA. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>R2-C11 (Q2, Q19; DESIGN.md § Page-level patterns, Unavailable): no id shown, nothing said of which.</summary>
    public const string UnavailableHeading = "EPA unavailable";

    public const string UnavailableBody = "This EPA does not exist, or you cannot open it.";

    public const string GoToEpas = "Go to EPAs";

    public const string SubSpecialityLabel = "Sub-speciality";

    /// <summary>R2-C04: the picker's help for an institution's own EPA.</summary>
    public const string SubSpecialityHelpOwn = "The disciplines your institution has adopted.";

    public const string CodeLabel = "Code";
    public const string TitleLabel = "Title";
    public const string DescriptionLabel = "Description";
    public const string RequiredKnowledgeSkillsLabel = "Required knowledge and skills";
    public const string CategoryLabel = "Category";
    public const string StatusLabel = "Status";
    public const string ActiveLabel = "Active";
    public const string Cancel = "Cancel";
    public const string Save = "Save";
    public const string Saving = "Saving…";

    /// <summary>S11: the trigger opens its dialog, so it ends in "…" and carries <c>aria-haspopup</c>.</summary>
    public const string DeactivateTrigger = "Deactivate…";

    public const string DeactivateConfirm = "Deactivate";

    public static string Category(EpaCategory category) => category switch
    {
        EpaCategory.Elective => "Elective",
        _ => "Core"
    };

    public static IReadOnlyList<EpaCategory> Categories { get; } = [EpaCategory.Core, EpaCategory.Elective];

    /// <summary>The built Active help, for a national EPA (T158, T196, T312).</summary>
    public const string ActiveHelpNational =
        "An inactive EPA cannot be chosen for a new activity and is not a target on any running programme's progress page " +
        "or dashboard; a programme that has ended keeps it on its record. " +
        "Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity " +
        "completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating " +
        "it credits those activities.";

    /// <summary>R3-C08 (Q11): an institution's own EPA measures only its own trainees, so the help says whose.</summary>
    public static string ActiveHelpOwn(string? institutionName)
        => $"An inactive EPA cannot be chosen by {institutionName ?? "your institution"}'s trainees for a new activity and is not " +
           "a target on their running programmes' progress pages or dashboards; a programme that has ended keeps it on its record. " +
           "Evidence already recorded against it is kept, and so is the progress it earned while it was active. An activity " +
           "completed against it while it is inactive counts towards nothing until the EPA is reactivated, and reactivating " +
           "it credits those activities.";

    public static string ActiveHelp(EpaDto? epa)
        => epa?.OwningInstitutionId is not null ? ActiveHelpOwn(epa.OwningInstitutionName) : ActiveHelpNational;

    /// <summary>R3-C12 (Q11, S8): the dialog names the EPA.</summary>
    public static string DeactivateTitle(EpaDto? epa) => epa is null ? "Deactivate this EPA?" : $"Deactivate {epa.Code}?";

    /// <summary>
    /// R3-C12 (Q11): the built body, naming whose trainees it stops measuring: the owning institution's for its own EPA,
    /// "every institution that has adopted it" for a national one (until T392, "every institution that uses it" for both).
    /// </summary>
    public static string DeactivateBody(EpaDto? epa)
    {
        var whose = epa?.OwningInstitutionId is not null
            ? $"for {epa.OwningInstitutionName ?? "your institution"}'s trainees"
            : "for every institution that has adopted it";
        return "It can no longer be chosen for a new activity, and it stops being a target on running programmes' progress " +
               $"pages and dashboards, {whose}. A programme that has ended keeps it on its record, marked no longer in use. " +
               "The progress it has earned is kept. An activity completed against it while it is inactive counts towards " +
               "nothing until it is reactivated. You can reactivate it on this page.";
    }

    public const string Saved = "EPA saved.";
    public const string SavedAndDeactivated = "EPA saved and deactivated.";
    public const string Deactivated = "EPA deactivated.";

    /// <summary>What reactivating credited: the activities completed against the EPA while it was inactive (T196, D48).</summary>
    public static string Reactivated(int completionsCredited) => completionsCredited switch
    {
        0 => "EPA reactivated. No activity was completed against it while it was inactive, so no progress changed.",
        1 => "EPA reactivated. 1 activity completed against it while it was inactive now counts towards progress.",
        _ => $"EPA reactivated. {completionsCredited} activities completed against it while it was inactive now count towards progress."
    };

    /// <summary>R2-C06 (T272, T329): a save that failed for any reason but a field's refusal.</summary>
    public const string SaveFailed = "The EPA could not be saved. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>A deactivation that failed (T329): on no board; the save's sentence, for the deactivation.</summary>
    public const string DeactivateFailed = "The EPA could not be deactivated. Nothing has changed. Try again, or come back in a few minutes.";

    // ---- The national EPA's record (R2-C10) ----

    public const string AboutHeading = "About this EPA";
    public const string CollegeLabel = "College";
}
