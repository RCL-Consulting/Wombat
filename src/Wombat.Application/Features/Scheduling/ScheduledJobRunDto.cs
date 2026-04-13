namespace Wombat.Application.Features.Scheduling;

public sealed record ScheduledJobRunDto(
    int Id,
    string Key,
    DateTime StartedAt,
    DateTime? FinishedAt,
    string Status,
    string? ErrorMessage,
    string? TriggeredBy);
