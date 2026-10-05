using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Programme.Trainees;

/// <summary>
/// What Programme trainees is asked for (T358, flow 06; D3's <c>?short=&amp;year=&amp;filed=</c>): registrars short on one
/// EPA, in one training year, with nothing filed in 30 days. Every filter is a narrowing; none set is the whole roster.
/// </summary>
public sealed record ProgrammeTraineesFilter(int? ShortOnEpaId = null, int? TrainingYear = null, bool NothingFiled = false)
{
    /// <summary>Whether any filter is set: the page draws Clear filters, and the heading counts "n of m" (D9).</summary>
    public bool IsFiltered => ShortOnEpaId is not null || TrainingYear is not null || NothingFiled;

    /// <summary>How many filters are set: one gives its own heading, more than one "match these filters" (D9).</summary>
    public int Count => (ShortOnEpaId is null ? 0 : 1) + (TrainingYear is null ? 0 : 1) + (NothingFiled ? 1 : 0);
}

/// <summary>Why a current registrar holds no target this period, computed, never stored (review 9).</summary>
public enum ProgrammeExemption
{
    /// <summary>The programme started part-way through the period (D14, D42): "Started part-way through the period".</summary>
    StartedPartWay,

    /// <summary>The programme has not started yet: "Starts on 2027-01-15".</summary>
    NotStarted
}

/// <summary>
/// One EPA a registrar is short on in its current window: <paramref name="Count" /> of <paramref name="Target" />.
/// </summary>
public sealed record EpaShortfallDto(int EpaId, string EpaCode, QuotaPeriod QuotaPeriod, int Count, int Target)
{
    /// <summary>How many more the window asks for (D11's shortfall): never below nought.</summary>
    public int Short => Math.Max(0, Target - Count);

    /// <summary>The window's last day on the College's calendar, which "2 more by 2026-11-30" names.</summary>
    public DateOnly WindowEnd { get; init; }

    /// <summary>The academic year of the window, which "0 of 1 in 2026" names.</summary>
    public int AcademicYear { get; init; }

    public bool IsPerSemester => QuotaPeriod == QuotaPeriod.Semester;
}

/// <summary>An EPA the Short on filter offers: one of the scope's targets this period.</summary>
public sealed record EpaFilterOptionDto(int EpaId, string EpaCode, string EpaTitle, QuotaPeriod QuotaPeriod, int Target);

/// <summary>
/// One current registrar on Programme trainees and on Home's Registrars and Nothing filed cards (T358).
/// </summary>
/// <param name="ProfileId">The trainee profile, which the registrar page is addressed by.</param>
/// <param name="Name">The name as stored, "Nomsa Mahlangu"; the user id when no account names them.</param>
/// <param name="TrainingYear">The training year today (<c>TraineeProfile.GetStage</c>); null before the programme starts.</param>
/// <param name="SemesterMet">EPAs whose semester target is met (My progress's "0 of 10").</param>
/// <param name="SemesterApplying">EPAs whose semester target applies.</param>
/// <param name="YearMet">EPAs whose yearly target is met ("0 of 5").</param>
/// <param name="YearApplying">EPAs whose yearly target applies.</param>
/// <param name="Exemption">Why no target applies this period, for an exempt registrar; null otherwise.</param>
/// <param name="FurthestShort">The three EPAs furthest from this window's target (D11); empty when every target is met.</param>
/// <param name="ShortOn">The EPA the Short on filter asks about, when the registrar is short on it.</param>
/// <param name="LastFiledOn">The South African day of the latest filing (<see cref="Filing.FilingMoments" />); null: none.</param>
public sealed record ProgrammeTraineeRowDto(
    int ProfileId,
    string TraineeUserId,
    string Name,
    int? TrainingYear,
    int SemesterMet,
    int SemesterApplying,
    int YearMet,
    int YearApplying,
    ProgrammeExemption? Exemption,
    IReadOnlyList<EpaShortfallDto> FurthestShort,
    EpaShortfallDto? ShortOn,
    DateOnly? LastFiledOn)
{
    /// <summary>The programme start, which a not-started registrar's reason names ("Starts on 2027-01-15").</summary>
    public DateOnly ProgrammeStartDate { get; init; }

    /// <summary>The admission day (D1), from which Nothing filed's 30 days may start.</summary>
    public DateOnly AdmittedOn { get; init; }

    /// <summary>The academic year of the day read, which "EPAs met in 2026" names.</summary>
    public int AcademicYear { get; init; }

    /// <summary>Whether the registrar has filed nothing in the window (E5): the Nothing filed filter's and card's rule.</summary>
    public bool NothingFiled { get; init; }

    public bool IsExempt => Exemption is not null;
}

/// <summary>
/// The scope's current registrars, every match, unpaged (T358): Home takes the first five, Programme trainees a page.
/// </summary>
/// <param name="CurrentSemesterName">"Semester 2, 2026".</param>
/// <param name="CurrentSemesterEnd">The semester's last day on the College's calendar, "2026-11-30".</param>
/// <param name="CurrentCount">Every current registrar in the scope: the "of 5".</param>
/// <param name="ExemptCount">Of them, those exempt this period.</param>
/// <param name="Rows">The match, in its order.</param>
/// <param name="Epas">The Short on filter's options: the scope's EPAs with a target this period, by code.</param>
/// <param name="TrainingYears">The training year filter's options: the years the current registrars are in.</param>
/// <param name="ShortOn">The EPA asked about, when the filter names one the scope has.</param>
/// <param name="Coverage">The scope's Targets by EPA, read from the same rows.</param>
public sealed record ProgrammeRosterRead(
    ProgrammeScopeDto Scope,
    string CurrentSemesterName,
    DateOnly CurrentSemesterEnd,
    int CurrentCount,
    int ExemptCount,
    IReadOnlyList<ProgrammeTraineeRowDto> Rows,
    IReadOnlyList<EpaFilterOptionDto> Epas,
    IReadOnlyList<int> TrainingYears,
    EpaFilterOptionDto? ShortOn,
    CurriculumCoverage Coverage)
{
    /// <summary>The filter the rows answer.</summary>
    public ProgrammeTraineesFilter Filter { get; init; } = new();
}

/// <summary>Programme trainees: the roster read and one page of its match.</summary>
public sealed record ProgrammeTraineesDto(
    ProgrammeRosterRead Read,
    IReadOnlyList<ProgrammeTraineeRowDto> Page,
    int MatchCount,
    int PageNumber,
    int PageSize);

/// <summary>
/// One registrar, for the registrar page's header and its reads (T358, C2; review 6).
/// </summary>
/// <param name="Name">The name as stored.</param>
/// <param name="TrainingYear">The training year on the day read: today, or the last day of an ended programme.</param>
/// <param name="SubSpecialityName">The curriculum's sub-speciality, "Paediatrics".</param>
/// <param name="CurrentSemesterName">The semester of the day read: today's, or the one an ended programme ended in.</param>
/// <param name="Ended">How the programme ended, when it has (read-only, r6); null while it runs.</param>
/// <param name="Scope">The scope it was read in, which names the role read as.</param>
public sealed record ProgrammeTraineeDto(
    int ProfileId,
    string TraineeUserId,
    string Name,
    int? TrainingYear,
    string InstitutionName,
    string SubSpecialityName,
    string CurrentSemesterName,
    ProgrammeEndDto? Ended,
    ProgrammeScopeDto Scope);
