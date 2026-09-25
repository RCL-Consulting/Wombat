using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.MultiSourceFeedback;

namespace Wombat.Web.Components.Pages.MultiSourceFeedback;

/// <summary>
/// The words of the programme's MSF coverage page (<c>/msf/coverage</c>, T210), built as whole strings because Razor drops
/// a space standing alone before an expression. Every sentence is the College's D9, as <c>MsfCoverageText</c> words the
/// trainee's own card: an EPA is covered when a released campaign COVERING it closed in the semester, never a campaign
/// "about" it. Nothing here is a target or a shortfall.
/// </summary>
public static class MsfProgrammeCoverageText
{
    /// <summary>
    /// What the page says, as standing content, to someone who holds Trainee: they read no other trainee's record,
    /// whatever other role brought them here (T185's rung), and their own coverage is on their progress page.
    /// </summary>
    public const string TraineeSeesNoProgramme =
        "You hold the Trainee role, so you see no other trainee's multi-source feedback coverage, whatever other role " +
        "you hold. Your own is on My progress.";

    /// <summary>The empty card's title and body: nobody the caller may read about is on a programme now.</summary>
    public const string EmptyTitle = "No trainees to show";

    public const string EmptyBody =
        "No trainee whose record you may read is on a programme now. A trainee who has completed or left a programme is " +
        "not counted.";

    /// <summary>What the counts are and are not.</summary>
    public const string Opening =
        "For each programme, EPA and semester, how many of the programme's trainees were covered: a released " +
        "multi-source feedback campaign covering the EPA closed in the semester. Each trainee is counted from their " +
        "own coverage, the lines on their progress page. A campaign covers several EPAs and is not about any one of " +
        "them. Only released campaigns count, and MSF counts towards no target, so an uncovered trainee is not a " +
        "shortfall. A trainee counts in a semester once their programme has started by its last day. A semester " +
        "that has ended can still gain a campaign that closed in it and is released later.";

    /// <summary>A programme's heading: "Paediatric EPA Curriculum 11.1 at Demo Institution".</summary>
    public static string ProgrammeName(MsfProgrammeDto programme)
    {
        ArgumentNullException.ThrowIfNull(programme);

        var curriculum = string.IsNullOrWhiteSpace(programme.CurriculumVersion)
            ? programme.CurriculumName
            : $"{programme.CurriculumName} {programme.CurriculumVersion}";
        return $"{curriculum} at {programme.InstitutionName}";
    }

    /// <summary>
    /// A semester's line in a programme's summary: "3 trainees, whose programme had started by 31 December 2026", or
    /// that none had.
    /// </summary>
    public static string TraineesLine(MsfProgrammeCoveragePeriodDto period, MsfProgrammePeriodDto? counted)
    {
        ArgumentNullException.ThrowIfNull(period);

        var trainees = counted?.Trainees ?? 0;
        var by = QuotaText.LongDate(period.End);
        return trainees == 0
            ? $"No trainee on the programme had started by {by}"
            : $"{Trainees(trainees)}, whose programme had started by {by}";
    }

    /// <summary>
    /// An EPA's cell: "2 of 3 trainees covered", or that no trainee counts in the semester. Null cell reads as none.
    /// </summary>
    public static string EpaCell(MsfProgrammeEpaPeriodDto? cell)
    {
        if (cell is null || cell.Trainees == 0)
        {
            return "No trainee had started";
        }

        return $"{cell.TraineesCovered} of {Trainees(cell.Trainees)} covered";
    }

    /// <summary>A trainee's cell: "6 of 15 EPAs covered", as their own card counts it, or "Not started".</summary>
    public static string TraineeCell(MsfProgrammeTraineePeriodDto? cell, int epas)
    {
        if (cell is not { HadStarted: true })
        {
            return "Not started";
        }

        return $"{cell.EpasCovered} of {epas} EPA{(epas == 1 ? string.Empty : "s")} covered";
    }

    /// <summary>The caption of a programme's EPA table.</summary>
    public const string EpaCaption =
        "Each EPA of the programme, by semester: of the trainees who count in the semester, how many had a released " +
        "MSF campaign covering the EPA close in it.";

    /// <summary>The caption of a programme's trainee table.</summary>
    public const string TraineeCaption =
        "Each trainee on the programme, by semester: how many of its EPAs a released MSF campaign covered for them, the " +
        "count their own progress page gives.";

    private static string Trainees(int count) => count == 1 ? "1 trainee" : $"{count} trainees";
}
