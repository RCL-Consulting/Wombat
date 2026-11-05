using Wombat.Application.Features.Colleges;

namespace Wombat.Web.Components.Pages.Admin.Colleges;

/// <summary>
/// The words of Colleges and a College's record (T394, flow 16, lane B; boards R3-B01–B12, B16, B17a/b, R2-B13–B15b), in
/// one place, so the pages and their tests read the same sentences.
/// </summary>
public static class CollegeWords
{
    // ---- Colleges ----

    /// <summary>R3-B01.</summary>
    public const string ListSubtitle =
        "The national Colleges. Each College's admin keeps its specialities, EPAs and curricula; you keep the Colleges " +
        "themselves.";

    /// <summary>R3-B01: the caption, which does not repeat the heading alone.</summary>
    public const string Caption = "Every College";

    public const string CreateCollege = "Create college";

    /// <summary>R2-B14a.</summary>
    public const string LoadingColleges = "Loading the Colleges.";

    /// <summary>R2-B15a.</summary>
    public const string CollegesLoadFailed =
        "Could not load the Colleges. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>As built: the seed makes it unreachable.</summary>
    public const string EmptyTitle = "No colleges yet";

    public const string EmptyBody = "Create the first college to start defining national specialities, EPAs and curricula.";

    public static string Status(bool isActive) => isActive ? "Active" : "Inactive";

    // ---- a College's record ----

    /// <summary>R2-B14b, B15b: the h1 while the record is read, or when its read failed (T7).</summary>
    public const string RecordHeadingWhileUnread = "College";

    public const string LoadingCollege = "Loading this College.";

    public const string CollegeLoadFailed =
        "Could not load this College. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>R2-B13 (Q7): an unknown id, no id shown.</summary>
    public const string UnavailableHeading = "College unavailable";

    public const string UnavailableBody = "This College does not exist, or you cannot open it.";

    public const string GoToColleges = "Go to Colleges";

    /// <summary>R3-B03.</summary>
    public const string CreateSubtitle = "A national College keeps its own specialities, EPAs and curricula. It starts active.";

    /// <summary>R3-B06, B07: "CNSA · Active" (S15).</summary>
    public static string RecordSubtitle(string shortCode, bool isActive) => $"{shortCode} · {Status(isActive)}";

    public const string NameLabel = "Name";
    public const string ShortCodeLabel = "Short code";
    public const string DescriptionLabel = "Description";
    public const string StatusLabel = "Status";
    public const string ActiveLabel = "Active";

    /// <summary>R3-B06, B07: Active's help while the College is active as stored (D56).</summary>
    public const string ActiveHelp =
        "An inactive College cannot be named by a new invitation. Nothing else reads it: its specialities, EPAs and " +
        "curricula, and every institution that has adopted one of its curricula, carry on as they are.";

    /// <summary>R3-B11a: in the help's place while it is inactive as stored. No Reactivate button: the box reactivates.</summary>
    public const string InactiveLine = "Inactive. A new invitation cannot name this College. Tick Active and save to reactivate it.";

    public const string Cancel = "Cancel";
    public const string Save = "Save";
    public const string Saving = "Saving…";
    public const string Creating = "Creating…";
    public const string DeactivateTrigger = "Deactivate…";

    // ---- results and failures ----

    /// <summary>R3-B06, by the ?created token.</summary>
    public const string Created = "College created.";

    /// <summary>R3-B08.</summary>
    public const string Saved = "College saved.";

    /// <summary>R3-B11a (R1): through Deactivate… or a Save with Active unticked.</summary>
    public static string Deactivated(string college) => $"{college} deactivated. A new invitation can no longer name it.";

    /// <summary>R2-0-spec-2 § 9 (R1): a Save with Active ticked on a College stored inactive.</summary>
    public static string Reactivated(string college) => $"{college} reactivated. A new invitation can name it again.";

    /// <summary>R3-B12 (T272): a failure that is no field's refusal.</summary>
    public const string SaveFailed =
        "The College could not be saved. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>R2-0-spec-2 § 9's "could not be created".</summary>
    public const string CreateFailed =
        "The College could not be created. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>NEW (T394, lane B): no board draws a failed deactivation; it reads as the save's.</summary>
    public const string DeactivateFailed =
        "The College could not be deactivated. Nothing has changed. Try again, or come back in a few minutes.";

    // ---- the field refusals the page asks before it sends (the commands' words, CollegeHolders) ----

    public const string NameRequired = "Enter a name.";
    public const string ShortCodeRequired = "Enter a short code.";
    public const string NameTooLong = "A name is at most 200 characters.";

    /// <summary>NEW (T394, lane B), as the command's.</summary>
    public const string ShortCodeTooLong = "A short code is at most 32 characters.";

    public const string DescriptionTooLong = "A description is at most 1,000 characters.";

    // ---- the dialog (D56, Q15: no count) ----

    /// <summary>R3-B09.</summary>
    public static string DeactivateTitle(string college) => $"Deactivate {college}?";

    public const string DeactivateBody =
        "A new invitation will no longer be able to name it. That is all it changes: its specialities, EPAs and curricula " +
        "stay as they are, every institution that has adopted one of its curricula carries on, and everyone already " +
        "invited keeps their invitation.";

    public const string DeactivateReactivateLine = "You can reactivate it on this page.";

    public const string DeactivateConfirm = "Deactivate";

    /// <summary>The page's own heading words for a College it read.</summary>
    public static string Heading(CollegeDto college) => college.Name;
}
