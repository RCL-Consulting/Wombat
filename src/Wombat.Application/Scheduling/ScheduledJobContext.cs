using Microsoft.Extensions.Logging;

namespace Wombat.Application.Scheduling;

public sealed record ScheduledJobContext(
    DateTime UtcNow,
    ILogger Logger);
