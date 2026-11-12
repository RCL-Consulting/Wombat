namespace Wombat.Application.Features.Scheduling;

/// <summary>
/// One run on Run history (T400, flow 18; R3-R01–R09). Every moment is UTC; the page says it in South African time.
/// </summary>
/// <param name="Status">The run's status by name: Running, Succeeded or Failed.</param>
/// <param name="ErrorMessage">A failed run's error as stored, whole (up to its 2,000 characters; T1); null otherwise.</param>
/// <param name="RanBy">
/// Who ran it by hand, by name (an erased person by their pseudonym, M3, through <c>DataRightsPeople.NamesOfAsync</c>);
/// null for the scheduler.
/// </param>
public sealed record ScheduledJobRunDto(
    int Id,
    string Key,
    DateTime StartedAt,
    DateTime? FinishedAt,
    string Status,
    string? ErrorMessage,
    string? RanBy);

/// <summary>
/// A page of Run history (T400, T1, R3-R06): the runs that match, newest first, with how many match in all, how many runs
/// are held at all (the no-match state's "Clear them to see all 21.", and a new install's "No job has run yet"), and the
/// jobs' keys for the Job select, by key.
/// </summary>
public sealed record ScheduledJobRunsPageDto(
    IReadOnlyList<ScheduledJobRunDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int AllRunsCount,
    IReadOnlyList<string> JobKeys);
