using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Options;
using Wombat.Domain.Activities;
using Wombat.Domain.Audit;

namespace Wombat.Infrastructure.Persistence;

/// <summary>What the refresher did with one seeded activity type.</summary>
public enum ActivityTypeSeedRefreshOutcome
{
    /// <summary>No stored type with this key — creation belongs to the seeders, not here.</summary>
    NotSeededYet,

    /// <summary>The published JSON already matches the seed folder.</summary>
    Unchanged,

    /// <summary>A new version was published from the seed folder.</summary>
    Republished,

    /// <summary>The seed folder differs, but the refresh is switched off; reported only.</summary>
    ReportedOnly,

    /// <summary><c>OwnerUserId</c> is not the seed identity — an operator owns this type.</summary>
    SkippedOperatorOwned,

    /// <summary>The newest version was published by someone other than the seeder.</summary>
    SkippedOperatorPublished,

    /// <summary>The refresh of this one type threw and was rolled back; it is unchanged.</summary>
    SkippedRefreshFailed,

    /// <summary>A saved draft is staged; publishing over it would destroy the operator's work.</summary>
    SkippedDraftInFlight,

    /// <summary>The current <c>Version</c> has no <c>ActivityTypeVersion</c> row — already broken.</summary>
    SkippedMissingVersionRow,

    /// <summary>The seed folder is missing or does not parse.</summary>
    SkippedSeedUnreadable,

    /// <summary>The stored JSON does not parse, so no honest comparison is possible.</summary>
    SkippedStoredJsonUnreadable
}

/// <summary>One line of the refresh report.</summary>
public sealed record ActivityTypeSeedRefreshResult(
    string Key,
    ActivityTypeSeedRefreshOutcome Outcome,
    int Version,
    string Detail);

/// <summary>
/// Carries edits to the seed folders under <c>Activities/Seeds/</c> into activity types that already
/// exist in the database, by publishing a new version (T103).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="DataSeeder"/> and <see cref="PaediatricCatalogueSeeder"/> skip any key that already
/// exists, which is right for idempotency and wrong for evolution: once a type exists, editing its
/// <c>schema.json</c> changes nothing, with no error and no log line. This runs after both of them
/// and closes that gap for the keys the seeders own.
/// </para>
/// <para><b>Three things make it safe to run on a production boot:</b></para>
/// <list type="number">
/// <item><b>Canonical-to-canonical comparison.</b> The four JSON columns are <c>jsonb</c>, so the
/// bytes PostgreSQL returns are its own rendering and are never byte-equal to
/// <c>FormSchemaParser.Serialize</c> output. Both sides are parsed and re-serialised before they are
/// compared, which is a fixed point — so an unchanged seed folder produces no write at all, on this
/// boot or any later one.</item>
/// <item><b>Operator work is never reverted.</b> Three guards, all evaluated before
/// <see cref="ActivityType.SaveDraft"/> is called: seed ownership, no draft in flight, and — the one
/// that actually catches customisation — the newest published version having been published by the
/// seeder. The builder's edit path never reassigns <c>OwnerUserId</c>, so an admin who republishes a
/// seeded type still reads as seed-owned; <c>PublishedByUserId</c> is the honest signal.</item>
/// <item><b>It cannot take the host down.</b> Every failure is logged and swallowed. A seeder that
/// throws at startup is a systemd crash loop, not a degraded feature.</item>
/// </list>
/// <para>
/// <b>In-flight activities stay pinned and are not unblocked by a republish.</b> An activity created
/// against version 1 keeps validating against version 1 (<c>ActivityService.GetPinnedVersion</c>), so
/// a schema fix reaches new activities only. That is deliberate — it is also why the refresher
/// refuses to bump a type whose current version has no <c>ActivityTypeVersion</c> row: bumping would
/// add vN+1 and leave those activities exactly as broken.
/// </para>
/// </remarks>
public sealed class ActivityTypeSeedRefresher
{
    /// <summary>
    /// 1412498227 is 0x54313033 — "T103" as ASCII. Two hosts booting together would otherwise both
    /// diff, both bump to the same version, and collide on the unique
    /// <c>(ActivityTypeId, Version)</c> index — which at startup means a crash loop.
    /// </summary>
    /// <remarks>
    /// <c>pg_try_advisory_lock</c>, not the blocking <c>pg_advisory_lock</c>. This runs inside the
    /// startup block before the host listens, so a lock nobody releases — an orphaned backend, or a DBA
    /// session that took the same key while investigating — would hang the boot indefinitely rather than
    /// degrade it. Failing to take the lock means another host is already refreshing, which is a reason
    /// to skip, not to wait.
    /// </remarks>
    private const string TryAcquireAdvisoryLockSql = "SELECT pg_try_advisory_lock(1412498227)";

    /// <inheritdoc cref="TryAcquireAdvisoryLockSql"/>
    private const string ReleaseAdvisoryLockSql = "SELECT pg_advisory_unlock(1412498227)";

    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ActivityTypeSeedRefresher> _logger;
    private readonly IOptions<WombatOptions> _options;

    public ActivityTypeSeedRefresher(
        ApplicationDbContext dbContext,
        ILogger<ActivityTypeSeedRefresher> logger,
        IOptions<WombatOptions> options)
    {
        _dbContext = dbContext;
        _logger = logger;
        _options = options;
    }

    /// <summary>
    /// Compares every seed folder against its stored activity type and republishes the ones that
    /// differ. Never throws: a failure is logged and startup continues.
    /// </summary>
    public async Task<IReadOnlyList<ActivityTypeSeedRefreshResult>> RefreshAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await RefreshCoreAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Activity type seed refresh failed before it could complete. Each type commits separately, "
                + "so some may already have been republished — check the per-type log lines above rather "
                + "than assuming nothing changed. Startup continues.");

            return [];
        }
    }

    private async Task<IReadOnlyList<ActivityTypeSeedRefreshResult>> RefreshCoreAsync(CancellationToken cancellationToken)
    {
        var refreshEnabled = _options.Value.RefreshSeededActivityTypes;
        var usesPostgres = _dbContext.Database.IsNpgsql();
        var results = new List<ActivityTypeSeedRefreshResult>();
        var connectionOpened = false;
        var lockHeld = false;

        try
        {
            if (usesPostgres)
            {
                // Held for the whole refresh. The connection is opened explicitly because a
                // session-level advisory lock is lost the moment the connection returns to the pool.
                await _dbContext.Database.OpenConnectionAsync(cancellationToken);
                connectionOpened = true;

                lockHeld = await TryAcquireAdvisoryLockAsync(cancellationToken);
                if (!lockHeld)
                {
                    _logger.LogInformation(
                        "Activity type seed refresh skipped: another host holds the refresh lock and is "
                        + "doing it. Published activity types are unchanged by this host.");

                    return [];
                }
            }

            var keys = ActivityTypeSeedCatalogue.Entries.Select(entry => entry.Key).ToArray();
            var stored = await _dbContext.ActivityTypes
                .Include(activityType => activityType.Versions)
                .Where(activityType => keys.Contains(activityType.Key))
                .ToDictionaryAsync(activityType => activityType.Key, StringComparer.Ordinal, cancellationToken);

            foreach (var entry in ActivityTypeSeedCatalogue.Entries)
            {
                stored.TryGetValue(entry.Key, out var activityType);

                // Per-type isolation. Each type is its own unit of work, so one bad seed folder or one
                // failed SaveChanges must not abandon the remaining types — and must not leave its own
                // half-applied mutation behind. SaveDraft/PublishDraft mutate the tracked entity in
                // place, so a failed save leaves Version already incremented and a new version row
                // attached to a DbContext that Program.cs goes on to reuse: the next unrelated
                // SaveChangesAsync would commit a refresh that was logged as failed. Rolling the entry
                // back to Unchanged is what stops that.
                try
                {
                    results.Add(await RefreshOneAsync(entry, activityType, refreshEnabled, cancellationToken));
                }
                catch (Exception exception)
                {
                    RollbackTrackedChanges(activityType);

                    _logger.LogError(
                        exception,
                        "Activity type '{Key}': seed refresh failed and was rolled back. This type is unchanged; the other seeded types are unaffected.",
                        entry.Key);

                    results.Add(Skipped(
                        entry.Key,
                        activityType?.Version ?? 0,
                        ActivityTypeSeedRefreshOutcome.SkippedRefreshFailed,
                        "the refresh threw and was rolled back"));
                }
            }
        }
        finally
        {
            try
            {
                if (lockHeld)
                {
                    await _dbContext.Database.ExecuteSqlRawAsync(ReleaseAdvisoryLockSql, CancellationToken.None);
                }
            }
            finally
            {
                if (connectionOpened)
                {
                    await _dbContext.Database.CloseConnectionAsync();
                }
            }
        }

        LogSummary(results, refreshEnabled);
        return results;
    }

    private async Task<ActivityTypeSeedRefreshResult> RefreshOneAsync(
        ActivityTypeSeedEntry entry,
        ActivityType? activityType,
        bool refreshEnabled,
        CancellationToken cancellationToken)
    {
        if (activityType is null)
        {
            // The seeders create missing types; nothing to evolve.
            return new ActivityTypeSeedRefreshResult(entry.Key, ActivityTypeSeedRefreshOutcome.NotSeededYet, 0, "not present in this database");
        }

        ActivityTypeSeedPayload desired;
        try
        {
            desired = await ActivityTypeSeedCatalogue.ReadCanonicalAsync(entry, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // One unreadable seed folder — an interrupted deploy, a malformed edit — must not stop
            // the other thirteen, and must not stop the host from booting.
            _logger.LogError(
                exception,
                "Activity type '{Key}': seed files could not be read or parsed. Skipped; the published version is unchanged.",
                entry.Key);

            return Skipped(entry.Key, activityType.Version, ActivityTypeSeedRefreshOutcome.SkippedSeedUnreadable, exception.Message);
        }

        ActivityTypeSeedPayload current;
        try
        {
            current = ActivityTypeSeedCatalogue.Canonicalise(
                activityType.SchemaJson ?? string.Empty,
                activityType.WorkflowJson ?? string.Empty,
                activityType.CreditRulesJson ?? string.Empty,
                activityType.DisplayFieldsJson);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(
                exception,
                "Activity type '{Key}': the stored JSON does not parse, so it cannot be compared with the seed. Skipped.",
                entry.Key);

            return Skipped(entry.Key, activityType.Version, ActivityTypeSeedRefreshOutcome.SkippedStoredJsonUnreadable, exception.Message);
        }

        var changed = DescribeChanges(desired, current);
        if (changed.Count == 0)
        {
            _logger.LogDebug(
                "Activity type '{Key}': published v{Version} already matches the seed folder.",
                entry.Key,
                activityType.Version);

            return new ActivityTypeSeedRefreshResult(entry.Key, ActivityTypeSeedRefreshOutcome.Unchanged, activityType.Version, "in sync");
        }

        var changedList = string.Join(", ", changed);

        // Every guard below runs BEFORE SaveDraft. SaveDraft overwrites all six staging columns
        // unconditionally, so checking afterwards would already have destroyed the operator's draft.
        if (!string.Equals(activityType.OwnerUserId, ActivityTypeSeedCatalogue.SeedActorUserId, StringComparison.Ordinal))
        {
            _logger.LogInformation(
                "Activity type '{Key}': seed {Changed} differ from published v{Version}, but the type is owned by '{OwnerUserId}'. Skipped — operator-owned types are never reverted.",
                entry.Key,
                changedList,
                activityType.Version,
                activityType.OwnerUserId);

            return Skipped(entry.Key, activityType.Version, ActivityTypeSeedRefreshOutcome.SkippedOperatorOwned, $"owned by '{activityType.OwnerUserId}'");
        }

        if (activityType.HasDraft)
        {
            _logger.LogWarning(
                "Activity type '{Key}': seed {Changed} differ from published v{Version}, but a draft saved by '{DraftUserId}' on {DraftUpdatedOn:u} is in flight. Skipped — publish or discard the draft, then restart.",
                entry.Key,
                changedList,
                activityType.Version,
                activityType.StagingUpdatedByUserId,
                activityType.StagingUpdatedOn);

            return Skipped(entry.Key, activityType.Version, ActivityTypeSeedRefreshOutcome.SkippedDraftInFlight, $"draft in flight from '{activityType.StagingUpdatedByUserId}'");
        }

        var pinnedVersion = activityType.Versions.SingleOrDefault(version => version.Version == activityType.Version);
        if (pinnedVersion is null)
        {
            // Bumping here would append vN+1 and leave every activity pinned to vN just as broken:
            // ActivityService.GetPinnedVersion throws on every read and write for them.
            _logger.LogWarning(
                "Activity type '{Key}': current Version {Version} has no ActivityTypeVersion row, so activities pinned to it already fail to load. Skipped — backfill the version row before the seed can be refreshed.",
                entry.Key,
                activityType.Version);

            return Skipped(entry.Key, activityType.Version, ActivityTypeSeedRefreshOutcome.SkippedMissingVersionRow, "no ActivityTypeVersion row for the current version");
        }

        var newestVersion = activityType.Versions.OrderByDescending(version => version.Version).First();
        if (!string.Equals(newestVersion.PublishedByUserId, ActivityTypeSeedCatalogue.SeedActorUserId, StringComparison.Ordinal))
        {
            // OwnerUserId alone is not enough: the builder's edit path never reassigns it, so a
            // College or Institutional admin who customises a seeded type still reads as seed-owned.
            _logger.LogInformation(
                "Activity type '{Key}': seed {Changed} differ from published v{Version}, but v{PublishedVersion} was published by '{PublishedByUserId}'. Skipped — this type has been customised and will not be reverted.",
                entry.Key,
                changedList,
                activityType.Version,
                newestVersion.Version,
                newestVersion.PublishedByUserId);

            return Skipped(entry.Key, activityType.Version, ActivityTypeSeedRefreshOutcome.SkippedOperatorPublished, $"v{newestVersion.Version} published by '{newestVersion.PublishedByUserId}'");
        }

        if (!refreshEnabled)
        {
            _logger.LogInformation(
                "Activity type '{Key}': seed {Changed} differ from published v{Version}. NOT republished — the seed refresh is disabled (set Wombat__RefreshSeededActivityTypes=true to apply it).",
                entry.Key,
                changedList,
                activityType.Version);

            return new ActivityTypeSeedRefreshResult(entry.Key, ActivityTypeSeedRefreshOutcome.ReportedOnly, activityType.Version, changedList);
        }

        var previousVersion = activityType.Version;

        activityType.SaveDraft(
            desired.SchemaJson,
            desired.WorkflowJson,
            desired.CreditRulesJson,
            desired.DisplayFieldsJson,
            ActivityTypeSeedCatalogue.SeedActorUserId);

        var published = activityType.PublishDraft(ActivityTypeSeedCatalogue.SeedActorUserId);

        _dbContext.AuditEntries.Add(AuditEntry.Create(
            DateTime.UtcNow,
            AuditCategory.ActivityType,
            "ActivityTypeSeedRefresh",
            success: true,
            actorUserId: ActivityTypeSeedCatalogue.SeedActorUserId,
            actorDisplay: "Seed refresher (startup)",
            subjectType: nameof(ActivityType),
            summaryJson: JsonSerializer.Serialize(new
            {
                activityTypeId = activityType.Id,
                key = entry.Key,
                fromVersion = previousVersion,
                toVersion = published.Version,
                changed
            })));

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Activity type '{Key}': republished from the seed folder, v{PreviousVersion} -> v{NewVersion} ({Changed}). The previous version is retained; activities already created against it stay pinned to it and are NOT changed by this publish.",
            entry.Key,
            previousVersion,
            published.Version,
            changedList);

        return new ActivityTypeSeedRefreshResult(entry.Key, ActivityTypeSeedRefreshOutcome.Republished, published.Version, changedList);
    }

    private void LogSummary(IReadOnlyList<ActivityTypeSeedRefreshResult> results, bool refreshEnabled)
    {
        var republished = results.Count(result => result.Outcome == ActivityTypeSeedRefreshOutcome.Republished);
        var unchanged = results.Count(result => result.Outcome == ActivityTypeSeedRefreshOutcome.Unchanged);
        var pending = results.Count(result => result.Outcome == ActivityTypeSeedRefreshOutcome.ReportedOnly);
        var skipped = results.Count(result =>
            result.Outcome is ActivityTypeSeedRefreshOutcome.SkippedOperatorOwned
                or ActivityTypeSeedRefreshOutcome.SkippedOperatorPublished
                or ActivityTypeSeedRefreshOutcome.SkippedDraftInFlight
                or ActivityTypeSeedRefreshOutcome.SkippedMissingVersionRow
                or ActivityTypeSeedRefreshOutcome.SkippedSeedUnreadable
                or ActivityTypeSeedRefreshOutcome.SkippedStoredJsonUnreadable
                or ActivityTypeSeedRefreshOutcome.SkippedRefreshFailed);
        var failed = results.Count(result => result.Outcome == ActivityTypeSeedRefreshOutcome.SkippedRefreshFailed);
        // NotSeededYet is silent per type (creation is the seeders' job), so the summary is the only
        // place it can surface. Without it the counters do not add up to Total and a database where a
        // seeder never completed looks identical to a healthy one.
        var notSeededYet = results.Count(result => result.Outcome == ActivityTypeSeedRefreshOutcome.NotSeededYet);

        var log = failed > 0 ? LogLevel.Error : LogLevel.Information;
        _logger.Log(
            log,
            "Activity type seed refresh ({Mode}): {Republished} republished, {Unchanged} unchanged, {Skipped} skipped ({Failed} of them failed), {Pending} pending, {NotSeededYet} not yet seeded, of {Total} seeded types.",
            refreshEnabled ? "enabled" : "report only",
            republished,
            unchanged,
            skipped,
            failed,
            pending,
            notSeededYet,
            results.Count);
    }

    /// <summary>
    /// Takes the refresh lock if it is free. Returns false when another host holds it.
    /// </summary>
    private async Task<bool> TryAcquireAdvisoryLockAsync(CancellationToken cancellationToken)
    {
        var connection = _dbContext.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = TryAcquireAdvisoryLockSql;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is true;
    }

    /// <summary>
    /// Undoes the in-memory mutations of a failed republish.
    /// </summary>
    /// <remarks>
    /// <see cref="ActivityType.SaveDraft"/> and <see cref="ActivityType.PublishDraft"/> mutate the
    /// tracked entity in place — <c>Version++</c>, staging columns cleared, a new
    /// <see cref="ActivityTypeVersion"/> added to the navigation. If the save then fails, those changes
    /// are still sitting in the change tracker of a DbContext that startup goes on to reuse, so the next
    /// unrelated <c>SaveChangesAsync</c> would silently commit a refresh that was reported as failed.
    /// Reloading from the database is the cheapest honest undo; detaching the new version row first
    /// stops it being re-inserted, since a freshly added entity has nothing to reload from.
    /// </remarks>
    private void RollbackTrackedChanges(ActivityType? activityType)
    {
        try
        {
            foreach (var entry in _dbContext.ChangeTracker.Entries()
                         .Where(entry => entry.State == EntityState.Added)
                         .ToArray())
            {
                entry.State = EntityState.Detached;
            }

            if (activityType is not null)
            {
                _dbContext.Entry(activityType).Reload();
            }
        }
        catch (Exception exception)
        {
            // Best effort. A failed rollback is worth knowing about but must not itself abort the
            // remaining types, which is the whole point of the per-type isolation this serves.
            _logger.LogWarning(
                exception,
                "Could not roll back the in-memory state after a failed seed refresh. Subsequent startup "
                + "saves may be affected; restart the host if the next log lines look wrong.");
        }
    }

    private static ActivityTypeSeedRefreshResult Skipped(
        string key,
        int version,
        ActivityTypeSeedRefreshOutcome outcome,
        string detail)
        => new(key, outcome, version, detail);

    private static IReadOnlyList<string> DescribeChanges(ActivityTypeSeedPayload desired, ActivityTypeSeedPayload current)
    {
        var changes = new List<string>(4);

        if (!string.Equals(desired.SchemaJson, current.SchemaJson, StringComparison.Ordinal))
        {
            changes.Add("schema");
        }

        if (!string.Equals(desired.WorkflowJson, current.WorkflowJson, StringComparison.Ordinal))
        {
            changes.Add("workflow");
        }

        if (!string.Equals(desired.CreditRulesJson, current.CreditRulesJson, StringComparison.Ordinal))
        {
            changes.Add("credit rules");
        }

        if (!string.Equals(desired.DisplayFieldsJson, current.DisplayFieldsJson, StringComparison.Ordinal))
        {
            changes.Add("display fields");
        }

        return changes;
    }
}
