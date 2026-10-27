using System.Globalization;
using Wombat.Application.Features.Reporting;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Components.Pages.Portfolio;

/// <summary>
/// Every sentence Export portfolio prints, own and staff (T379, flow 13, lane E; Spec 3's rows, the operator's Q17–Q19 and
/// the boards R3-E01–E20). "You" and "your" to the person exporting their own; the registrar's name, never a pronoun, on the
/// staff export (N10).
/// </summary>
public static class ExportWords
{
    /// <summary>The own export's h1, tab and menu item (Q17); the staff export's while it reads and when it failed (N12).</summary>
    public const string Title = "Export portfolio";

    /// <summary>The staff export's own crumb, under its owner: "Home › Trainees › Lerato Molefe's programme › Export portfolio" (M4).</summary>
    public const string Crumb = Title;

    /// <summary>The own export's subtitle (Spec 3's Notice row, for "Generate a PDF export of the trainee's portfolio.").</summary>
    public const string OwnSubtitle =
        "A PDF of your portfolio for the dates you choose: your progress, your STARs, your committee's decisions and your activities.";

    /// <summary>The Unavailable page's h1 and tab (Q19): one page for every refused cause.</summary>
    public const string UnavailableTitle = "Portfolio unavailable";

    /// <summary>The Unavailable page's one sentence, naming nobody (Q19, R3-E09, E10, E19).</summary>
    public const string UnavailableBody = "This portfolio does not exist, or you cannot export it.";

    /// <summary>The live region's words while the subject is read (R3-E14).</summary>
    public const string Loading = "Loading this export.";

    /// <summary>The subject read failed (R3-E15; T329: fixed words, the exception to the log).</summary>
    public const string LoadError = "Could not load this export. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>The live region's words while the PDF is made (R3-E02).</summary>
    public const string Generating = "Generating the PDF.";

    /// <summary>The button while the PDF is made (built).</summary>
    public const string GeneratingButton = "Generating...";

    /// <summary>The button (built).</summary>
    public const string ExportButton = "Export PDF";

    public const string FromLabel = "From date";

    public const string ToLabel = "To date";

    /// <summary>An emptied From (R3-E12, PROPOSED; T324: Wombat's words, never a property's name).</summary>
    public const string ChooseFrom = "Choose the from date.";

    /// <summary>An emptied To (R3-E12, PROPOSED).</summary>
    public const string ChooseTo = "Choose the to date.";

    /// <summary>The note under the dates (Spec 3's Hint row): the same dates give the same file, so Verify finds it.</summary>
    public const string SameFileNote = "Exporting the same dates again gives the same file, until the record changes.";

    /// <summary>To's hint while the programme runs: To opens on today on the South African calendar (T325).</summary>
    public const string ToToday = "Today, on the South African calendar.";

    /// <summary>The failure's first sentence, bold (R3-E13; S6: every exception but a ValidationException).</summary>
    public const string FailedFirst = "The portfolio could not be exported just now.";

    /// <summary>The failure's rest (R3-E13).</summary>
    public const string FailedRest = "Nothing was downloaded. Try again, or come back in a few minutes.";

    /// <summary>The result's second sentence and the start of its third, before the Verify link (R3-E03).</summary>
    public const string ResultRest = "The download should start automatically. Anyone holding the file can check it on";

    /// <summary>The result's Verify link, a plain href to the static page (S13).</summary>
    public const string VerifyLink = "Verify portfolio export";

    /// <summary>The staff export's h1 and tab, "Export Lerato Molefe's portfolio" (Q5): the header action's words.</summary>
    public static string StaffTitle(string name) => PortfolioWords.ExportAction(name);

    /// <summary>
    /// The staff export's subtitle, "Kgosi Kgari Teaching Hospital · Paediatric EPA Curriculum (11.1) · Training year 4"
    /// (R3-E04): where and on what the registrar trains, each part left out when it is not known; null when none is.
    /// </summary>
    public static string? StaffSubtitle(ExportSubjectDto subject)
    {
        ArgumentNullException.ThrowIfNull(subject);

        var parts = new List<string>(3);
        if (!string.IsNullOrWhiteSpace(subject.InstitutionName))
        {
            parts.Add(subject.InstitutionName);
        }

        if (!string.IsNullOrWhiteSpace(subject.CurriculumName))
        {
            parts.Add(string.IsNullOrWhiteSpace(subject.CurriculumVersion)
                ? subject.CurriculumName
                : $"{subject.CurriculumName} ({subject.CurriculumVersion})");
        }

        if (subject.TrainingYear is { } year)
        {
            parts.Add($"Training year {year.ToString(CultureInfo.InvariantCulture)}");
        }

        return parts.Count == 0 ? null : string.Join(" · ", parts);
    }

    /// <summary>From's hint: "Your programme started on 2023-01-15." or "Lerato Molefe's programme started on 2023-01-15."</summary>
    public static string? FromHint(bool own, string name, DateOnly? programmeStart)
        => programmeStart is { } start
            ? $"{(own ? "Your" : PortfolioWords.Possessive(name))} programme started on {Iso(start)}."
            : null;

    /// <summary>To's hint: today on the South African calendar, or the last day of an ended programme.</summary>
    public static string ToHint(bool own, string name, DateOnly? lastDay)
        => lastDay is null
            ? ToToday
            : own ? "Your last day in the programme." : $"{PortfolioWords.Possessive(name)} last day in the programme.";

    /// <summary>The result's first sentence, bold: "Your portfolio exported: portfolio-aba6ce7943c2.pdf." (Q18, R3-E03, E05).</summary>
    public static string Exported(bool own, string name, string fileName)
        => $"{(own ? "Your" : PortfolioWords.Possessive(name))} portfolio exported: {fileName}.";

    private static string Iso(DateOnly day) => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
