namespace Wombat.Web.Components.Pages.Admin.Assessors;

/// <summary>
/// The Assessors list's words (T376, flow 12, lane T; boards T01–T14, Spec 3). Her institution's name comes from
/// <c>GetMyInstitutionNameQuery</c>; while it loads, after a failed read, and for an institution gone, each sentence says
/// "your institution" (S9). The Administrator's are T14's.
/// </summary>
public static class AssessorsText
{
    /// <summary>The subtitle without the name: while loading, on failure (T12, T13).</summary>
    public const string SubtitleWithoutName =
        "Everyone at your institution who holds the Assessor role, with the assessor training recorded for each.";

    /// <summary>The Administrator's subtitle (T14).</summary>
    public const string SubtitleAdministrator =
        "Everyone who holds the Assessor role, at every institution, with the assessor training recorded for each.";

    public static string Subtitle(string institutionName)
        => $"Everyone at {institutionName} who holds the Assessor role, with the assessor training recorded for each.";

    public static string Heading(string? institutionName)
        => institutionName is null ? "Assessors at your institution" : $"Assessors at {institutionName}";

    public const string HeadingAdministrator = "Assessors at every institution";

    public static string Caption(string? institutionName)
        => $"At {institutionName ?? "your institution"}, by last name: each holder of the Assessor role, and the training recorded on their page.";

    public const string CaptionAdministrator = "Every institution, by last name: each holder of the Assessor role and their training.";

    public const string Loading = "Loading the assessors.";

    /// <summary>A failed read (T13, T329): a fixed sentence; the failure goes to the log.</summary>
    public const string LoadFailed = "Could not load the assessors. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>Nobody holds the role (T01, NEW; it replaces "No assessor profiles", T326).</summary>
    public const string EmptyTitle = "Nobody holds the Assessor role yet";

    public const string EmptyBody =
        "An assessor's training record holds their training status, the date they completed it and their qualifications. " +
        "Give someone the Assessor role from their page in Users; they are listed here, and their training is recorded on " +
        "their page.";

    /// <summary>The Training completed cell with no record at all (T02); a record with no date reads "Not recorded" (T08).</summary>
    public const string NoRecordDate = "—";

    public const string PagerLabel = "Pages of assessors";

    public const string PageSizeLabel = "Rows per page";
}
