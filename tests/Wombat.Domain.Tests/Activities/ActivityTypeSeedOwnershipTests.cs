using Wombat.Domain.Activities;

namespace Wombat.Domain.Tests.Activities;

/// <summary>
/// T398 (flow 17, N11, T18): the one predicate for whether the seed catalogue still owns a type, which the refresher
/// refreshes by and the builder tells its author by.
/// </summary>
public sealed class ActivityTypeSeedOwnershipTests
{
    [Fact]
    public void ATypeTheSeederOwnsAndLastPublished_IsSeedOwned()
        => Assert.True(ActivityTypeSeedOwnership.IsSeedOwned(Type(ActivityTypeSeedOwnership.SeederUserId, "seed-system", "seed-system")));

    /// <summary>The builder never reassigns the owner, so the newest publisher is what hands a seeded type over.</summary>
    [Fact]
    public void ASeededTypeSomeoneElseRepublished_IsNotSeedOwned()
    {
        var type = Type(ActivityTypeSeedOwnership.SeederUserId, "seed-system", "college-admin");

        Assert.True(ActivityTypeSeedOwnership.IsOwnedBySeeder(type));
        Assert.False(ActivityTypeSeedOwnership.NewestPublishedBySeeder(type));
        Assert.False(ActivityTypeSeedOwnership.IsSeedOwned(type));
    }

    [Fact]
    public void ATypeABuilderMade_IsNotSeedOwned()
        => Assert.False(ActivityTypeSeedOwnership.IsSeedOwned(Type("college-admin", "seed-system")));

    [Fact]
    public void ATypeNeverPublished_IsNotSeedOwned()
        => Assert.False(ActivityTypeSeedOwnership.IsSeedOwned(Type(ActivityTypeSeedOwnership.SeederUserId)));

    private static ActivityType Type(string owner, params string[] publishers)
    {
        var type = new ActivityType { Key = "probe", OwnerUserId = owner };
        for (var index = 0; index < publishers.Length; index++)
        {
            // Out of order on purpose: the newest is by number, not by position.
            type.Versions.Add(new ActivityTypeVersion { Version = index + 1, PublishedByUserId = publishers[index] });
        }

        type.Versions = type.Versions.Reverse().ToList();
        return type;
    }
}
