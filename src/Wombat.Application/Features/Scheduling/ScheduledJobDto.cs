namespace Wombat.Application.Features.Scheduling;

public sealed record ScheduledJobDto(
    int Id,
    string Key,
    string CronExpression,
    bool IsEnabled,
    string Description,
    DateTime? LastRunAt,
    string? LastRunStatus,
    DateTime? NextRunAt);
