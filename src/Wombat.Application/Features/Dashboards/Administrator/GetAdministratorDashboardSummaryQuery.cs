using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.Email;
using Wombat.Domain.Institutions;
using Wombat.Domain.Scheduling;

namespace Wombat.Application.Features.Dashboards.Administrator;

public sealed record GetAdministratorDashboardSummaryQuery(ClaimsPrincipal Principal)
    : IRequest<AdministratorDashboardSummaryDto>;

/// <summary>
/// The Administrator's Home (T400, flow 18; DESIGN.md § Dashboard page): System health's Database, Mail and Scheduled
/// jobs lines, and the Accounts card.
/// </summary>
/// <remarks>
/// <para>
/// Each part is read and caught on its own (K11, T9): a database that does not answer leaves Home what it can still say
/// (the Database line, and the Mail line while no mail server is set), with every part that needed the database read as
/// not read. A part's failure is logged here and never shown. Until T400 the summary was one read that either answered
/// whole or failed whole, and its Mail and jobs lines were stubs ("Email queue (T012)").
/// </para>
/// <para>
/// The accounts are counted by the directory in one query (<c>CountAccountsAsync</c>, K2): until T400 this looped over
/// every role's listing and counted distinct ids, which left out an account with no role and counted an erased one.
/// The jobs are judged by the host's own rule (<see cref="JobSchedule" />, K4), read here rather than through the
/// Scheduled jobs page's query. The mail is read from <c>EmailOutcomes</c>, which either host's worker writes (Q18, K20),
/// beside this host's <see cref="IEmailHealth" />. "Now" is <see cref="TimeProvider" />'s, so a test can pin it.
/// </para>
/// </remarks>
public sealed class GetAdministratorDashboardSummaryQueryHandler
    : IRequestHandler<GetAdministratorDashboardSummaryQuery, AdministratorDashboardSummaryDto>
{
    /// <summary>What the outcomes table holds by its retention, and so how far back "Nothing sent since …" can look (Q18).</summary>
    internal static readonly TimeSpan MailWindow = TimeSpan.FromDays(90);

    /// <summary>How far back a drop makes the Mail line Failing (Q13).</summary>
    internal static readonly TimeSpan DropWindow = TimeSpan.FromHours(24);

    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userService;
    private readonly IEmailHealth _emailHealth;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<GetAdministratorDashboardSummaryQueryHandler> _logger;

    public GetAdministratorDashboardSummaryQueryHandler(
        IApplicationDbContext dbContext,
        IUserAdministrationService userService,
        IEmailHealth emailHealth,
        TimeProvider timeProvider,
        ILogger<GetAdministratorDashboardSummaryQueryHandler> logger)
    {
        _dbContext = dbContext;
        _userService = userService;
        _emailHealth = emailHealth;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<AdministratorDashboardSummaryDto> Handle(
        GetAdministratorDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        // The page is the Administrator's alone; the handler is the second lock (the build review's N-3).
        if (!request.Principal.IsAdministrator())
        {
            throw new UnauthorizedAccessException("Only a Wombat administrator may read the system's health.");
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;

        // The probe: a light count. It names the moment the database did not answer (the Database line).
        DateTime? databaseFailedAt = null;
        try
        {
            await _dbContext.Set<Institution>().AsNoTracking().CountAsync(cancellationToken);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(exception, "Home's database probe failed.");
            databaseFailedAt = now;
        }

        // With the probe failed, the parts that need the database are not tried (the build review's S1): a database that
        // does not answer would make each wait out its connection timeout before Home could say so. They read Not read.
        var databaseAnswered = databaseFailedAt is null;
        var accounts = databaseAnswered
            ? await ReadPartAsync("accounts", () => ReadAccountsAsync(now, cancellationToken), cancellationToken)
            : null;
        var jobs = databaseAnswered
            ? await ReadPartAsync("scheduled jobs", () => ReadJobsAsync(now, cancellationToken), cancellationToken)
            : null;

        // With no mail server the line is configuration alone, and the table is not read (Q2): it reads Not configured
        // even while the database is down (R3-H08).
        var configured = _emailHealth.IsConfigured;
        var outcomes = configured && databaseAnswered
            ? await ReadPartAsync("mail outcomes", () => ReadMailAsync(now, cancellationToken), cancellationToken)
            : null;
        var mail = new AdministratorMailDto(configured, _emailHealth.QueueDepth, outcomes);

        return new AdministratorDashboardSummaryDto(now, databaseFailedAt, accounts, mail, jobs);
    }

    /// <summary>One part, read and caught on its own (K11): null when it could not be read, the failure logged.</summary>
    private async Task<T?> ReadPartAsync<T>(string part, Func<Task<T>> read, CancellationToken cancellationToken)
        where T : class
    {
        try
        {
            return await read();
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(exception, "Home's {Part} could not be read.", part);
            return null;
        }
    }

    private async Task<AdministratorAccountsDto> ReadAccountsAsync(DateTime now, CancellationToken cancellationToken)
    {
        var count = await _userService.CountAccountsAsync(now, cancellationToken);
        return new AdministratorAccountsDto(count.Accounts, count.Locked);
    }

    private async Task<AdministratorMailOutcomesDto> ReadMailAsync(DateTime now, CancellationToken cancellationToken)
    {
        var windowStart = now - MailWindow;
        var dropsFrom = now - DropWindow;
        var outcomes = _dbContext.Set<EmailOutcome>().AsNoTracking();

        var dropped = await outcomes
            .Where(outcome => outcome.Kind == EmailOutcomeKind.Dropped && outcome.OccurredAt > dropsFrom)
            .CountAsync(cancellationToken);

        var lastDrop = dropped == 0
            ? null
            : await outcomes
                .Where(outcome => outcome.Kind == EmailOutcomeKind.Dropped && outcome.OccurredAt > dropsFrom)
                .OrderByDescending(outcome => outcome.OccurredAt)
                .ThenByDescending(outcome => outcome.Id)
                .Select(outcome => new { outcome.OccurredAt, outcome.Refusal })
                .FirstOrDefaultAsync(cancellationToken);

        var lastSent = await outcomes
            .Where(outcome => outcome.Kind == EmailOutcomeKind.Sent && outcome.OccurredAt >= windowStart)
            .OrderByDescending(outcome => outcome.OccurredAt)
            .Select(outcome => (DateTime?)outcome.OccurredAt)
            .FirstOrDefaultAsync(cancellationToken);

        return new AdministratorMailOutcomesDto(windowStart, dropped, lastDrop?.OccurredAt, lastDrop?.Refusal, lastSent);
    }

    private async Task<AdministratorJobsDto> ReadJobsAsync(DateTime now, CancellationToken cancellationToken)
    {
        var definitions = await _dbContext.Set<ScheduledJobDefinition>()
            .AsNoTracking()
            .OrderBy(definition => definition.Key)
            .ToListAsync(cancellationToken);

        // The last run of each job, as the Scheduled jobs page reads it (GetScheduledJobStatusQueryHandler).
        var keys = definitions.Select(definition => definition.Key).ToList();
        var lastRuns = await _dbContext.Set<ScheduledJobRun>()
            .AsNoTracking()
            .Where(run => keys.Contains(run.Key))
            .GroupBy(run => run.Key)
            .Select(group => group.OrderByDescending(run => run.StartedAt).First())
            .ToListAsync(cancellationToken);
        var lastRunByKey = lastRuns.ToDictionary(run => run.Key, StringComparer.Ordinal);

        return new AdministratorJobsDto(definitions
            .Select(definition => Judge(definition, lastRunByKey.GetValueOrDefault(definition.Key), now))
            .ToList());
    }

    /// <summary>
    /// One job as the line says it (Q3, K4), by the host's rule: disabled; an unreadable cron; a run still running, which
    /// is neither failing nor overdue; a last run that failed; overdue by more than an hour; never run; else succeeded.
    /// </summary>
    internal static AdministratorJobHealthDto Judge(ScheduledJobDefinition definition, ScheduledJobRun? lastRun, DateTime now)
    {
        var readable = JobSchedule.TryParse(definition.CronExpression, out var cron);
        var lastStart = lastRun is null ? (DateTime?)null : DateTime.SpecifyKind(lastRun.StartedAt, DateTimeKind.Utc);
        var running = lastRun?.Status == ScheduledJobRunStatus.Running;
        var next = readable ? JobSchedule.NextAfter(cron!, lastStart, now) : null;

        var state = (definition.IsEnabled, readable, lastRun?.Status) switch
        {
            (false, _, _) => AdministratorJobState.Disabled,
            (_, false, _) => AdministratorJobState.Unreadable,
            (_, _, ScheduledJobRunStatus.Running) => AdministratorJobState.Running,
            (_, _, ScheduledJobRunStatus.Failed) => AdministratorJobState.Failed,
            _ when JobSchedule.IsOverdue(cron!, lastStart, running, now) => AdministratorJobState.Overdue,
            (_, _, null) => AdministratorJobState.NeverRun,
            _ => AdministratorJobState.Succeeded
        };

        return new AdministratorJobHealthDto(
            definition.Key,
            definition.CronExpression,
            definition.IsEnabled,
            lastRun?.Status.ToString(),
            lastStart,
            lastRun?.FinishedAt is { } finished ? DateTime.SpecifyKind(finished, DateTimeKind.Utc) : null,
            lastRun?.Status == ScheduledJobRunStatus.Failed ? FirstLine(lastRun.ErrorMessage) : null,
            next,
            state);
    }

    private static string? FirstLine(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            return null;
        }

        var line = error.Trim().Split('\n', 2)[0].Trim();
        return line.Length == 0 ? null : line;
    }
}
