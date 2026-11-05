namespace Wombat.Web.Components.Pages.Admin.EntrustmentScales;

/// <summary>
/// The words of the entrustment scales list (T392, flow 15, boards R2-E01–E05, R3-E03, E04), in one place, so the page and
/// its tests read the same sentences.
/// </summary>
public static class EntrustmentScalesWords
{
    /// <summary>Q13 (T326): a reader is told what the ladders are and who sets them, not invited to define them.</summary>
    public const string ReaderSubtitle =
        "The rating ladders that assessment forms and committee reviews use. A Wombat administrator sets them; they are " +
        "shown here for reference.";

    /// <summary>The Administrator's, as built: the scales are the Administrator's to change.</summary>
    public const string AdministratorSubtitle =
        "Define and maintain the rating ladders used by assessment forms and committee reviews.";

    /// <summary>The table's caption (R2-E01), which does not repeat the heading's words alone.</summary>
    public const string Caption = "Entrustment scales, by name";

    /// <summary>S19 (R3-E03).</summary>
    public const string Loading = "Loading the entrustment scales.";

    /// <summary>S7 (R3-E04).</summary>
    public const string LoadFailed =
        "Could not load the entrustment scales. Nothing has changed. Try again, or come back in a few minutes.";

    public const string EmptyTitle = "No scales yet";

    /// <summary>R2-E05, the reader's (the seed makes it unreachable).</summary>
    public const string ReaderEmptyBody = "A Wombat administrator has not set up an entrustment scale yet.";

    /// <summary>The Administrator's, as built.</summary>
    public const string AdministratorEmptyBody =
        "Create the first entrustment scale to back assessment ratings and entrustment decisions.";

    /// <summary>A delete that failed other than by the handler's refusal (T329). NEW: no board draws it.</summary>
    public const string DeleteFailed =
        "The scale could not be deleted. Nothing has changed. Try again, or come back in a few minutes.";

    // T394 (flow 16, lane B; boards R2-C01, C06, C07, R3-C08–C10): the Administrator's Delete… and its dialog (Q11), the
    // refusal's holder links (S6), and the result.

    public const string CreateScale = "Create scale";

    /// <summary>R2-C07 (Q11).</summary>
    public static string DeleteTitle(string scale) => $"Delete {scale}?";

    public const string DeleteBody = "It is removed from every list. This cannot be undone.";

    /// <summary>R2-C07: round 1's second paragraph, kept on the board.</summary>
    public const string DeleteInUseLine =
        "A scale that anything uses is not deleted: you will be told what uses it, and nothing changes.";

    public const string DeleteConfirm = "Delete scale";

    /// <summary>R3-C09.</summary>
    public const string Deleted = "Entrustment scale deleted.";

    /// <summary>S6 (R3-C08): a holder's link, by its name, and its accessible name (M5).</summary>
    public static string OpenHolder(string subSpeciality) => $"Open {subSpeciality}";

    public static string OpenHolderName(string subSpeciality) => $"Open {subSpeciality}, the sub-speciality";

    /// <summary>S6: past three holders, the rest counted.</summary>
    public static string MoreHolders(int count) => $"and {count} more.";
}
