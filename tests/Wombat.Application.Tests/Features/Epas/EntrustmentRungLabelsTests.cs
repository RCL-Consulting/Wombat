using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Epas;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Epas;

/// <summary>
/// The one seed-key, id or exact-name rule the WBA rating picker, the portfolio PDF and the rating trajectory
/// all resolve a schema <c>scale_key</c> through (T100, T253). It is <c>EntrustmentScaleBindings</c>, which
/// <c>CreditApplier</c> uses too, so a chart cannot label a rung the credit engine refused to compare.
/// </summary>
public sealed class EntrustmentRungLabelsTests
{
    [Fact]
    public async Task ResolvesAScaleKeyThatIsASeedKey_AndARenameOfTheScaleDoesNotUnbindIt()
    {
        // T253: every seed binds its ladder this way, so an administrator's rename cannot unbind it.
        await using var db = CreateDb();
        SeedLadders(db);
        (await db.Set<EntrustmentScale>().SingleAsync(scale => scale.Id == 3)).Name = "Paediatric ladder";
        await db.SaveChangesAsync();

        var lookup = await EntrustmentRungLabels.LoadForScaleKeysAsync(
            db, ["seed:cpsa:scale:v11.1", " seed:cpsa:scale:v11.1 ", "CPSA Paediatric Entrustment Scale v11.1"], CancellationToken.None);

        lookup.ResolveScaleKey("seed:cpsa:scale:v11.1").Should().Be(3);
        lookup.ResolveScaleKey(" seed:cpsa:scale:v11.1 ").Should().Be(3, "keys are trimmed before matching");
        lookup.FormatByScaleKey("seed:cpsa:scale:v11.1", 5).Should().Be("4");
        lookup.ResolveScaleKey("CPSA Paediatric Entrustment Scale v11.1").Should().BeNull("the old name binds nothing now");
    }

    [Fact]
    public async Task ASeedKeyBindsOnlyBySeedKey_AndDigitsOnlyById_NeverByAName()
    {
        // The three forms are told apart by the string alone (ScaleBinding). The validators refuse such names now, so
        // these two rows are what a database could hold only from before T253.
        await using var db = CreateDb();
        SeedLadders(db);
        db.Set<EntrustmentScale>().Add(new EntrustmentScale { Id = 8, Name = "seed:cpsa:scale:v9" });
        db.Set<EntrustmentScale>().Add(new EntrustmentScale { Id = 9, Name = "77" });
        await db.SaveChangesAsync();

        var lookup = await EntrustmentRungLabels.LoadForScaleKeysAsync(
            db, ["seed:cpsa:scale:v9", "77", "seed:demo:scale:o-r"], CancellationToken.None);

        lookup.ResolveScaleKey("seed:cpsa:scale:v9").Should().BeNull("no scale has that seed key");
        lookup.ResolveScaleKey("77").Should().BeNull("no scale has that id");
        lookup.ResolveScaleKey("seed:demo:scale:o-r").Should().Be(1);
    }

    [Fact]
    public async Task ResolvesAScaleKeyThatIsABareId()
    {
        // Every legacy *_paed activity type on dev declares scale_key "2" — a raw id, not a name.
        await using var db = CreateDb();
        SeedLadders(db);

        var lookup = await EntrustmentRungLabels.LoadForScaleKeysAsync(db, ["2"], CancellationToken.None);

        lookup.ResolveScaleKey("2").Should().Be(2);
        lookup.FormatByScaleKey("2", 4).Should().Be("Unsupervised");
    }

    [Fact]
    public async Task ResolvesAScaleKeyThatIsAnExactName()
    {
        await using var db = CreateDb();
        SeedLadders(db);

        var lookup = await EntrustmentRungLabels.LoadForScaleKeysAsync(
            db, ["CPSA Paediatric Entrustment Scale v11.1"], CancellationToken.None);

        // The point of the whole task: ordinal 5 is the College's rung "4".
        lookup.FormatByScaleKey("CPSA Paediatric Entrustment Scale v11.1", 5).Should().Be("4");
        lookup.FormatByScaleKey("CPSA Paediatric Entrustment Scale v11.1", 3).Should().Be("3a");
    }

    [Fact]
    public async Task AnUnresolvableScaleKeyFallsBackToTheOrdinal()
    {
        // Until T110 the four generic WBA seeds declared "or_scale" while the seeded scale was named "O-R Scale", so
        // this was the live state of the product for months, not a hypothetical.
        await using var db = CreateDb();
        SeedLadders(db);

        var lookup = await EntrustmentRungLabels.LoadForScaleKeysAsync(
            db, ["or_scale"], CancellationToken.None);

        lookup.ResolveScaleKey("or_scale").Should().BeNull();
        lookup.FormatByScaleKey("or_scale", 4).Should().Be("4");
    }

    [Fact]
    public async Task NameMatchingIsExact_NotCaseInsensitiveAndNotTrimmedIntoAMatch()
    {
        await using var db = CreateDb();
        SeedLadders(db);

        var lookup = await EntrustmentRungLabels.LoadForScaleKeysAsync(
            db, ["o-r scale", "  O-R Scale  "], CancellationToken.None);

        lookup.ResolveScaleKey("o-r scale").Should().BeNull("case-insensitive matching would be a new rule");
        lookup.ResolveScaleKey("  O-R Scale  ").Should().Be(1, "keys are trimmed before matching");
    }

    [Fact]
    public async Task AnIdIsPreferredOverANameThatHappensToBeTheSameString()
    {
        // A scale literally named "2" alongside the scale whose id is 2. Digits bind by id alone (T253), and the
        // validators no longer let a scale be named like this.
        await using var db = CreateDb();
        SeedLadders(db);
        db.Set<EntrustmentScale>().Add(new EntrustmentScale { Id = 9, Name = "2" });
        db.Set<EntrustmentLevel>().Add(new EntrustmentLevel { Id = 900, ScaleId = 9, Order = 1, Label = "Only rung" });
        await db.SaveChangesAsync();

        var lookup = await EntrustmentRungLabels.LoadForScaleKeysAsync(db, ["2"], CancellationToken.None);

        lookup.ResolveScaleKey("2").Should().Be(2);
    }

    [Fact]
    public async Task LoadsNothingForAnEmptyOrAllBlankKeySet()
    {
        await using var db = CreateDb();
        SeedLadders(db);

        var lookup = await EntrustmentRungLabels.LoadForScaleKeysAsync(
            db, [null, "", "   "], CancellationToken.None);

        lookup.ResolveScaleKey("1").Should().BeNull();
        lookup.Format(null, 3).Should().Be("3");
    }

    [Fact]
    public async Task RungsOfReturnsTheLadderInOrder()
    {
        await using var db = CreateDb();
        SeedLadders(db);

        var lookup = await EntrustmentRungLabels.LoadAsync(db, [3], CancellationToken.None);

        lookup.RungsOf(3).Select(rung => rung.Label)
            .Should().Equal("1", "2", "3a", "3b", "4", "5");
        lookup.RungsOf(null).Should().BeEmpty();
        lookup.RungsOf(999).Should().BeEmpty();
    }

    [Fact]
    public async Task AnOrdinalThatIsNotARungOnItsScaleFallsBackToTheNumber()
    {
        await using var db = CreateDb();
        SeedLadders(db);

        var lookup = await EntrustmentRungLabels.LoadAsync(db, [1], CancellationToken.None);

        // The O-R Scale has five rungs; nothing sits at 6.
        lookup.Find(1, 6).Should().BeNull();
        lookup.Format(1, 6).Should().Be("6");
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static void SeedLadders(ApplicationDbContext db)
    {
        AddScale(db, 1, "O-R Scale",
            ["Observe only", "Direct supervision", "Indirect supervision", "Independent", "Supervises others"],
            "demo:scale:o-r");
        AddScale(db, 2, "Paed General Entrustment Scale",
            ["Observation only", "Direct supervision", "Indirect supervision", "Unsupervised", "Can supervise others"]);
        AddScale(db, 3, "CPSA Paediatric Entrustment Scale v11.1",
            ["1", "2", "3a", "3b", "4", "5"],
            "cpsa:scale:v11.1");
        db.SaveChanges();
    }

    private static void AddScale(ApplicationDbContext db, int scaleId, string name, string[] labels, string? seedKey = null)
    {
        db.Set<EntrustmentScale>().Add(new EntrustmentScale { Id = scaleId, Name = name, SeedKey = seedKey });
        for (var order = 1; order <= labels.Length; order++)
        {
            db.Set<EntrustmentLevel>().Add(new EntrustmentLevel
            {
                Id = (scaleId * 100) + order,
                ScaleId = scaleId,
                Order = order,
                Label = labels[order - 1]
            });
        }
    }
}
