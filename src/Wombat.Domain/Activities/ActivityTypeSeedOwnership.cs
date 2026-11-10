namespace Wombat.Domain.Activities;

/// <summary>
/// Whether Wombat's seed catalogue still owns an activity type (T103; T398, flow 17, N11, T18): the one predicate the seed
/// refresher refreshes by and the builder tells its author by.
/// </summary>
/// <remarks>
/// A type is seed-owned while its <see cref="ActivityType.OwnerUserId" /> and its newest published version's publisher are
/// both the seeder. The owner alone is not enough: the builder never reassigns it, so a type an administrator has
/// republished still names the seeder as its owner, and the newest publisher is the honest signal. Once anyone else
/// publishes it, the refresher no longer carries the seed folder's edits into it.
/// </remarks>
public static class ActivityTypeSeedOwnership
{
    /// <summary>
    /// The actor recorded as owner, draft author and publisher for everything the seeders write. Moved here from
    /// <c>ActivityTypeSeedCatalogue.SeedActorUserId</c>, which now reads it (T398, N11).
    /// </summary>
    public const string SeederUserId = "seed-system";

    /// <summary>Whether the seeder is the type's owner.</summary>
    public static bool IsOwnedBySeeder(ActivityType activityType)
    {
        ArgumentNullException.ThrowIfNull(activityType);
        return string.Equals(activityType.OwnerUserId, SeederUserId, StringComparison.Ordinal);
    }

    /// <summary>Whether the type's newest published version was published by the seeder; false with none published.</summary>
    public static bool NewestPublishedBySeeder(ActivityType activityType)
    {
        ArgumentNullException.ThrowIfNull(activityType);
        var newest = activityType.Versions.MaxBy(version => version.Version);
        return newest is not null && string.Equals(newest.PublishedByUserId, SeederUserId, StringComparison.Ordinal);
    }

    /// <summary>
    /// Whether the seed catalogue still owns the type: <see cref="IsOwnedBySeeder" /> and
    /// <see cref="NewestPublishedBySeeder" /> both. Reads <see cref="ActivityType.Versions" />, so load them first.
    /// </summary>
    public static bool IsSeedOwned(ActivityType activityType)
        => IsOwnedBySeeder(activityType) && NewestPublishedBySeeder(activityType);
}
