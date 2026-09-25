using Wombat.Domain.Epas;

namespace Wombat.Domain.Tests.Epas;

/// <summary>
/// T196, D48: an EPA's pause. Deactivating stamps when it began, reactivating ends it, and a completion credits the EPA
/// only at a moment it was in force.
/// </summary>
public sealed class EpaActivePeriodTests
{
    private static readonly DateTime PausedFrom = new(2026, 5, 1, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Deactivate_TakesTheEpaOutOfForceFromThatMoment()
    {
        var epa = new Epa();

        Assert.True(epa.Deactivate(PausedFrom));

        Assert.False(epa.IsActive);
        Assert.Equal(PausedFrom, epa.DeactivatedOn);
        Assert.True(epa.InForceAt(PausedFrom.AddTicks(-1)), "a completion before the pause began was in force");
        Assert.False(epa.InForceAt(PausedFrom), "the moment the pause begins is inside it");
        Assert.False(epa.InForceAt(PausedFrom.AddDays(30)));
    }

    [Fact]
    public void Deactivate_OnAnInactiveEpa_KeepsWhenThePauseBegan()
    {
        // Moving it later would put paused completions before the pause, and a rebuild would credit them.
        var epa = new Epa();
        epa.Deactivate(PausedFrom);

        Assert.False(epa.Deactivate(PausedFrom.AddDays(10)));

        Assert.Equal(PausedFrom, epa.DeactivatedOn);
    }

    [Fact]
    public void Reactivate_EndsThePause_AndPutsEveryMomentBackInForce()
    {
        // Every moment, because reactivating credits the paused completions: the rule no longer tells them apart.
        var epa = new Epa();
        epa.Deactivate(PausedFrom);

        Assert.True(epa.Reactivate());

        Assert.True(epa.IsActive);
        Assert.Null(epa.DeactivatedOn);
        Assert.True(epa.InForceAt(PausedFrom.AddDays(30)));
        Assert.False(epa.Reactivate(), "an active EPA has nothing to reactivate");
    }

    [Fact]
    public void ASecondDeactivation_HoldsBackOnlyWhatFollowsIt()
    {
        var epa = new Epa();
        epa.Deactivate(PausedFrom);
        epa.Reactivate();
        epa.Deactivate(PausedFrom.AddDays(60));

        Assert.True(epa.InForceAt(PausedFrom.AddDays(10)), "the first pause was closed by a reactivation, which credited it");
        Assert.False(epa.InForceAt(PausedFrom.AddDays(61)));
    }

    [Fact]
    public void IsActive_IsInitOnly_SoAStoredEpaChangesOnlyThroughDeactivateAndReactivate()
    {
        // T196 review. With a public setter, a handler could clear the flag without the moment, compile, and meet
        // CK_Epas_DeactivatedOn only at save. An init accessor carries the IsExternalInit modifier.
        var setter = typeof(Epa).GetProperty(nameof(Epa.IsActive))!.SetMethod!;

        Assert.Contains(typeof(System.Runtime.CompilerServices.IsExternalInit), setter.ReturnParameter.GetRequiredCustomModifiers());
    }

    [Fact]
    public void AnInactiveEpaWithNoRecordedPause_IsInForceAtNoMoment()
    {
        // Only a fixture that constructs one produces it; the database's CK_Epas_DeactivatedOn refuses it.
        var epa = new Epa { IsActive = false };

        Assert.False(epa.InForceAt(DateTime.MinValue));
        Assert.False(epa.InForceAt(PausedFrom));
    }
}
