using Wombat.Web.Components.Shared;
namespace Wombat.Web.Components.Pages.Admin.Institutions;

/// <summary>
/// The words of the College's tree, a speciality's and a sub-speciality's record, the Specialities redirect and the College
/// admin's Home card (T394, flow 16, lane A; boards R3-A02–A17, A22 and R2-A01, A10, A18a–c, A19a/b, A20a/b, A21, and
/// R2-0-spec-2 § 9, R3-0-spec-2 § 9, build-lanes.md § "Decision 3's words"). One place, so the pages and their tests say
/// the same, and no page prints an exception's text (T272, T393).
/// </summary>
public static class CollegeTreeWords
{
    // ---- The tree (Q1, S9) ----

    /// <summary>The tree's h1 and tab, for the College admin and the Administrator alike.</summary>
    public const string TreeHeading = "Specialities";

    /// <summary>S9, S15: "College of Paediatricians of South Africa (CPSA): its specialities, …".</summary>
    public static string TreeSubtitle(string collegeName, string shortCode)
        => $"{collegeName} ({shortCode}): its specialities, their sub-specialities and the ladder each uses.";

    public const string CreateSpeciality = "Create speciality";

    public const string CreateSubSpeciality = "Create sub-speciality";

    public const string Edit = "Edit";

    /// <summary>M8: the section's Edit, named for its speciality.</summary>
    public static string EditSpecialityName(string speciality) => $"Edit the speciality {speciality}";

    /// <summary>M5: the section's Create sub-speciality, named for its speciality.</summary>
    public static string CreateSubSpecialityName(string speciality) => $"Create sub-speciality of {speciality}";

    /// <summary>M8: a row's Edit, named for its row (N4: its speciality added when two share a name).</summary>
    public static string EditSubSpecialityName(string rowName) => $"Edit the sub-speciality {rowName}";

    /// <summary>
    /// N4: a row's name where two sub-specialities of the College share one, its speciality in brackets (NEW: no board
    /// draws the case). Within one speciality a name is unique.
    /// </summary>
    public static string SharedNameSuffix(string speciality) => $" ({speciality})";

    /// <summary>N6: the section's table caption, which does not repeat its heading's words alone.</summary>
    public static string SectionCaption(string speciality) => $"Sub-specialities of {speciality}";

    /// <summary>The speciality's line under its h2: "Specialist training in Paediatrics. · Active", or its status alone.</summary>
    public static string SpecialityLine(string? description, bool isActive)
        => string.IsNullOrWhiteSpace(description) ? Status(isActive) : $"{description} · {Status(isActive)}";

    /// <summary>R3-A12: a speciality with no sub-speciality yet, in place of its table.</summary>
    public static string NoSubSpecialities(string speciality)
        => $"No sub-specialities yet. Create the first sub-speciality of {speciality}.";

    public const string SubSpecialityColumn = "Sub-speciality";

    public const string DescriptionColumn = "Description";

    public const string DefaultScaleColumn = "Default scale";

    public const string StatusColumn = "Status";

    public const string ActionsColumn = "Actions";

    /// <summary>R3-A13b: a row whose sub-speciality has no default ladder.</summary>
    public const string NoDefault = "No default";

    public static string Status(bool isActive) => isActive ? "Active" : "Inactive";

    /// <summary>R2-A10: a College with no speciality yet.</summary>
    public const string NoSpecialitiesTitle = "No specialities yet";

    public static string NoSpecialitiesBody(string collegeName) => $"Create the first speciality for {collegeName}.";

    /// <summary>R2-A19a: the tree while it is read.</summary>
    public const string LoadingTree = "Loading the specialities.";

    /// <summary>R2-A20a: the tree's read failed. Never the exception's text.</summary>
    public const string TreeLoadFailed =
        "Could not load the specialities. Nothing has changed. Try again, or come back in a few minutes.";

    // ---- The tree's results, by token (S13) ----

    /// <summary>R3-A12: <c>?created=speciality:3</c>.</summary>
    public static TreeResult SpecialityCreated(string speciality) => new($"Speciality {speciality} created.");

    /// <summary>R3-A07, A13b: <c>?created=sub:4</c>.</summary>
    public static TreeResult SubSpecialityCreated(string subSpeciality) => new($"Sub-speciality {subSpeciality} created.");

    /// <summary>R3-A04: <c>?saved=speciality:2</c>.</summary>
    public static readonly TreeResult SpecialitySaved = new("Speciality saved.");

    /// <summary>R3-A14b: <c>?saved=sub:3</c>.</summary>
    public static readonly TreeResult SubSpecialitySaved = new("Sub-speciality saved.");

    // ---- Unavailable (Q7; R2-A18a–c, N2) ----

    public const string CollegeUnavailableHeading = "College unavailable";

    public const string CollegeUnavailableBody = "This College does not exist, or you cannot open it.";

    public const string SpecialityUnavailableHeading = "Speciality unavailable";

    public const string SpecialityUnavailableBody = "This speciality does not exist, or you cannot open it.";

    public const string SubSpecialityUnavailableHeading = "Sub-speciality unavailable";

    public const string SubSpecialityUnavailableBody = "This sub-speciality does not exist, or you cannot open it.";

    /// <summary>The College admin's way out of an Unavailable page: his own Specialities.</summary>
    public const string GoToSpecialities = "Go to Specialities";

    /// <summary>The Administrator's (N2, as R2-B13): Colleges.</summary>
    public const string GoToColleges = "Go to Colleges";

    // ---- The records (S11, M8) ----

    /// <summary>A speciality's record while it is read, or when its read failed (flow 15's T7: the kind's word).</summary>
    public const string SpecialityHeadingWhileUnread = "Speciality";

    /// <summary>A sub-speciality's record while it is read, or when its read failed (R2-A19b).</summary>
    public const string SubSpecialityHeadingWhileUnread = "Sub-speciality";

    /// <summary>R3-A03: a speciality's subtitle.</summary>
    public static string SpecialitySubtitle(string collegeName) => $"Speciality of {collegeName}";

    /// <summary>R3-A11: Create speciality's subtitle.</summary>
    public static string CreateSpecialitySubtitle(string collegeName) => $"A speciality of {collegeName}.";

    /// <summary>R3-A05: a sub-speciality's subtitle.</summary>
    public static string SubSpecialitySubtitle(string specialityName) => $"Sub-speciality of {specialityName}";

    /// <summary>R3-A06: Create sub-speciality's subtitle.</summary>
    public static string CreateSubSpecialitySubtitle(string specialityName, string collegeName)
        => $"A sub-speciality of {specialityName}, in {collegeName}.";

    /// <summary>M4: above the first field of a form with a starred one (as flows 11 and 14 write it).</summary>
    public const string RequiredMarkLine = RequiredMark.Line;

    public const string NameLabel = "Name";

    public const string DescriptionLabel = "Description";

    public const string DefaultScaleLabel = "Default entrustment scale";

    /// <summary>The built option for no default.</summary>
    public const string NoDefaultOption = "No default — offer every scale";

    /// <summary>The built help under the default scale.</summary>
    public const string DefaultScaleHelp = "Committee STARs for this programme's trainees are limited to this scale's levels.";

    public const string StatusLabel = "Status";

    public const string ActiveLabel = "Active";

    /// <summary>R3-A03 (D56, S15): a speciality's Status help while it is stored active.</summary>
    public const string SpecialityActiveHelp =
        "An inactive speciality cannot be named by a new invitation, and an institution can no longer give anyone a role for it. Nothing else reads it: its EPAs and curricula, and the registrars on them, carry on.";

    /// <summary>R3-A05 (D56, S15): a sub-speciality's Status help while it is stored active.</summary>
    public const string SubSpecialityActiveHelp =
        "An inactive sub-speciality cannot be named by a new invitation, and an institution can no longer give anyone a role for it. Nothing else reads it: its EPAs and curricula, and the registrars on them, carry on.";

    /// <summary>
    /// NEW (build-lanes § "Decision 3's words"): a speciality's Status help while it is stored inactive. The sub-speciality's
    /// adds nothing of sub-specialities (R3-A16a).
    /// </summary>
    public const string SpecialityInactiveLine =
        "Inactive. A new invitation cannot name it, and no institution can give anyone a role for it or any of its sub-specialities. Tick Active and save to reactivate it.";

    /// <summary>R3-A16a: a sub-speciality's Status help while it is stored inactive.</summary>
    public const string SubSpecialityInactiveLine =
        "Inactive. A new invitation cannot name it, and no institution can give anyone a role for it. Tick Active and save to reactivate it.";

    public const string Cancel = "Cancel";

    public const string Save = "Save";

    public const string Saving = "Saving…";

    /// <summary>The destructive trigger, which opens its dialog (DESIGN.md § Button system).</summary>
    public const string DeactivateTrigger = "Deactivate…";

    public const string DeactivateConfirm = "Deactivate";

    // ---- The deactivation dialogs (D56, Q15: no count; N10) ----

    /// <summary>R3-A17.</summary>
    public static string DeactivateSpecialityTitle(string? speciality) => $"Deactivate the speciality {speciality}?";

    /// <summary>R3-A15.</summary>
    public static string DeactivateSubSpecialityTitle(string? subSpeciality) => $"Deactivate the sub-speciality {subSpeciality}?";

    /// <summary>R3-A17 (N10): the speciality's first paragraph.</summary>
    public const string DeactivateSpecialityBody =
        "A new invitation will no longer be able to name it, and an institution can no longer give anyone a role for it or any of its sub-specialities. That is all it changes: its EPAs and curricula, and the registrars already on them, carry on.";

    /// <summary>R3-A15: the sub-speciality's first paragraph.</summary>
    public const string DeactivateSubSpecialityBody =
        "A new invitation will no longer be able to name it, and an institution can no longer give anyone a role for it. That is all it changes: its EPAs and curricula, and the registrars already on them, carry on.";

    /// <summary>R3-A15, A17: both dialogs' second paragraph.</summary>
    public const string DeactivateReactivateLine = "You can reactivate it on this page.";

    // ---- The records' results and failures (T272, T393) ----

    /// <summary>NEW (build-lanes § "Decision 3's words"): a speciality deactivated, on its record.</summary>
    public static TreeResult SpecialityDeactivated(string speciality)
        => new($"Speciality {speciality} deactivated.", "A new invitation can no longer name it.");

    /// <summary>R3-A16a: a sub-speciality deactivated, on its record.</summary>
    public static TreeResult SubSpecialityDeactivated(string subSpeciality)
        => new($"Sub-speciality {subSpeciality} deactivated.", "A new invitation can no longer name it.");

    private const string NothingChanged = "Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>R3-A09: a save that failed for no field's reason, the cause to the log.</summary>
    public static readonly TreeResult SpecialitySaveFailed = new("The speciality could not be saved.", NothingChanged);

    public static readonly TreeResult SpecialityCreateFailed = new("The speciality could not be created.", NothingChanged);

    public static readonly TreeResult SubSpecialitySaveFailed = new("The sub-speciality could not be saved.", NothingChanged);

    public static readonly TreeResult SubSpecialityCreateFailed = new("The sub-speciality could not be created.", NothingChanged);

    /// <summary>NEW (no board draws a failed deactivation): the speciality's, in the save's shape.</summary>
    public static readonly TreeResult SpecialityDeactivateFailed = new("The speciality could not be deactivated.", NothingChanged);

    /// <summary>NEW (no board draws a failed deactivation): the sub-speciality's, in the save's shape.</summary>
    public static readonly TreeResult SubSpecialityDeactivateFailed = new("The sub-speciality could not be deactivated.", NothingChanged);

    // ---- The records' loading and load failures (R2-A19b, A20b, R2-0-spec-2 § 9) ----

    public const string LoadingSpeciality = "Loading this speciality.";

    public const string LoadingSubSpeciality = "Loading this sub-speciality.";

    /// <summary>NEW: Create speciality reads its College first.</summary>
    public const string LoadingCollege = "Loading this College.";

    public const string SpecialityLoadFailed = "Could not load this speciality. " + NothingChanged;

    public const string SubSpecialityLoadFailed = "Could not load this sub-speciality. " + NothingChanged;

    /// <summary>Create speciality's read of its College failed (R2-0-spec-2 § 9's "this College").</summary>
    public const string CollegeLoadFailed = "Could not load this College. " + NothingChanged;

    // ---- The Specialities redirect (S12; R2-A21) ----

    public const string NoCollegeTitle = "No College on your account";

    public const string NoCollegeBody = "Your account names no College. Ask the platform administrator to add it.";

    // ---- The College admin's Home card (Q12, Q17; R2-A01) ----

    public const string HomeCardTitle = "National catalogue";

    public const string HomeCardBody =
        "Your College's specialities, EPAs, curricula and activity types. An institution can adopt a curriculum version once you open it.";
}

/// <summary>A result or a failure: its first sentence, which the page bolds (BRIEF § 11, flow 11), and the rest.</summary>
public sealed record TreeResult(string Lead, string? Rest = null)
{
    /// <summary>The whole, as read.</summary>
    public override string ToString() => Rest is null ? Lead : $"{Lead} {Rest}";
}
