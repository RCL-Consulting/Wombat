using Wombat.Domain.Curricula;

namespace Wombat.Domain.Tests.Curricula;

/// <summary>
/// T219: a progress row's last encounter, and whether its date was stated rather than being the day a form was created.
/// The rule depends on the set of encounters a row counts, never on the order they were credited in, so a rebuild
/// (which replays in filing order) writes what the live path wrote.
/// </summary>
public sealed class CurriculumItemProgressLastEncounterTests
{
    private static readonly DateOnly Earlier = new(2026, 8, 4);
    private static readonly DateOnly Later = new(2026, 8, 20);

    [Fact]
    public void ARowNothingHasCredited_HasNoLastEncounter_AndNothingStated()
    {
        var row = NewRow();

        Assert.Null(row.LastObservedOn);
        Assert.False(row.LastObservedOnDeclared);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TheFirstEncounter_SetsTheDateAndWhetherItWasStated(bool declared)
    {
        var row = NewRow();

        row.NoteEncounter(Earlier, declared);

        Assert.Equal(Earlier, row.LastObservedOn);
        Assert.Equal(declared, row.LastObservedOnDeclared);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void ALaterEncounter_ReplacesBoth(bool earlierDeclared, bool laterDeclared)
    {
        var row = NewRow();
        row.NoteEncounter(Earlier, earlierDeclared);

        row.NoteEncounter(Later, laterDeclared);

        Assert.Equal(Later, row.LastObservedOn);
        Assert.Equal(laterDeclared, row.LastObservedOnDeclared);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void AnEarlierEncounter_ChangesNeither(bool laterDeclared, bool earlierDeclared)
    {
        var row = NewRow();
        row.NoteEncounter(Later, laterDeclared);

        row.NoteEncounter(Earlier, earlierDeclared);

        Assert.Equal(Later, row.LastObservedOn);
        Assert.Equal(laterDeclared, row.LastObservedOnDeclared);
    }

    [Theory]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(false, false, false)]
    [InlineData(true, true, true)]
    public void TwoEncountersOnTheSameLastDate_AreStatedIfEitherIs(bool firstDeclared, bool secondDeclared, bool expected)
    {
        var row = NewRow();
        row.NoteEncounter(Later, firstDeclared);

        row.NoteEncounter(Later, secondDeclared);

        Assert.Equal(Later, row.LastObservedOn);
        Assert.Equal(expected, row.LastObservedOnDeclared);
    }

    private static CurriculumItemProgress NewRow() => new() { AcademicYear = 2026, Semester = 2 };
}
