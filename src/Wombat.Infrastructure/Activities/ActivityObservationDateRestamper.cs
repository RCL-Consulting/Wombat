using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;

namespace Wombat.Infrastructure.Activities;

/// <summary>
/// Gives an already-stored activity the encounter date its clinician actually typed, once the schema
/// version it is pinned to has gained an <c>observation_date_field</c> pointer. (T119)
/// </summary>
/// <remarks>
/// <para>
/// T119's migration backfills every row from <c>CreatedOn</c> and deliberately reads no
/// <c>DataJson</c>, because of a startup-ordering fact: <c>Program.cs</c> migrates <b>before</b>
/// <see cref="ActivityTypeSeedRefresher" /> runs, so at migration time no published version declares a
/// pointer and there is nothing to resolve against. Resolution has to happen after the refresher, where
/// the pointer exists — and in C#, so it can reuse <see cref="ObservationDateResolver" /> rather than
/// becoming a second implementation of the same question in SQL.
/// </para>
/// <para>
/// Only rows whose source is still <see cref="ObservationDateSource.CreatedOn" /> are considered, so a
/// date a clinician has since corrected is never overwritten and the pass is idempotent. An activity
/// whose pinned version declares no pointer — or which never carried a date — keeps the fallback and
/// stays honestly labelled as such. **It does not manufacture clinical history**: a missing date stays
/// missing.
/// </para>
/// <para>
/// Like the refresher, this belongs to the Web host only. <c>Wombat.Api</c> migrates but runs neither
/// the seeders nor the refresher, so a pointer would not exist there yet.
/// </para>
/// </remarks>
public sealed class ActivityObservationDateRestamper
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<ActivityObservationDateRestamper> _logger;
    private readonly IOptions<WombatOptions> _options;

    public ActivityObservationDateRestamper(
        IApplicationDbContext dbContext,
        ILogger<ActivityObservationDateRestamper> logger,
        IOptions<WombatOptions> options)
    {
        _dbContext = dbContext;
        _logger = logger;
        _options = options;
    }

    public async Task RestampAsync(CancellationToken cancellationToken = default)
    {
        var enabled = _options.Value.RestampActivityObservationDates;

        var candidates = await _dbContext.Set<Activity>()
            .Include(entity => entity.ActivityType)
                .ThenInclude(activityType => activityType.Versions)
            .Where(entity => entity.ObservedOnSource == ObservationDateSource.CreatedOn)
            .ToListAsync(cancellationToken);

        if (candidates.Count == 0)
        {
            return;
        }

        var restamped = 0;

        foreach (var activity in candidates)
        {
            var version = activity.ActivityType?.Versions
                .SingleOrDefault(entity => entity.Version == activity.SchemaVersion);

            if (version is null)
            {
                // A stranded activity (T107). Not this pass's problem, and not worth a warning each boot.
                continue;
            }

            FormSchema schema;
            try
            {
                schema = FormSchemaParser.Parse(version.SchemaJson);
            }
            catch (Exception exception) when (exception is SchemaParseException or ArgumentException)
            {
                _logger.LogWarning(
                    exception,
                    "Could not parse the pinned schema for activity {ActivityId}; its encounter date stays on the creation timestamp.",
                    activity.Id);
                continue;
            }

            var (observedOn, source) = ObservationDateResolver.Resolve(activity, schema, activity.DataJson);
            if (source != ObservationDateSource.Declared)
            {
                continue;
            }

            if (enabled)
            {
                activity.ObservedOn = observedOn;
                activity.ObservedOnSource = source;
            }

            restamped++;
        }

        if (restamped == 0)
        {
            return;
        }

        if (!enabled)
        {
            // Mirrors the refresher's kill switch: off still reports, so a stale corpus is a visible
            // no-op rather than a silent one.
            _logger.LogInformation(
                "Wombat__RestampActivityObservationDates is off: {Count} activities would have taken their declared encounter date.",
                restamped);
            return;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "Restamped {Count} activities onto the encounter date their pinned schema declares.",
            restamped);
    }
}
