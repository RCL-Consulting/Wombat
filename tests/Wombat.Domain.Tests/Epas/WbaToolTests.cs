using System.Globalization;
using Wombat.Domain.Epas;

namespace Wombat.Domain.Tests.Epas;

/// <summary>
/// T122. <see cref="WbaTool.NormalizeKey" /> is the one normaliser for a tool key.
/// </summary>
/// <remarks>
/// A curriculum item's allow-list is written by the seeder, the admin editor and a migration, and an activity type's
/// key by the seeder and the builder. The gate and the picker compare the two. If any writer or reader normalised
/// differently, "Mini-CEX may credit PAED-001" would hold on one path and not the other, so a trainee could be offered
/// an EPA the submit then refuses, or the reverse. These tests pin the three things every caller relies on: blank
/// means "no key", padding is dropped, and case folds the same way on every machine.
/// </remarks>
public sealed class WbaToolTests
{
    /// <summary>
    /// A blank key is "no instrument declared", which D21 reads as unrestricted. Returning an empty string instead
    /// would make a blank key look like a real one that no allow-list names, and refuse it everywhere.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    [InlineData(" \t \n ")]
    public void ABlankKeyIsNoKeyAtAll(string? key)
    {
        Assert.Null(WbaTool.NormalizeKey(key));
    }

    [Theory]
    [InlineData(" Mini_CEX ", "mini_cex")]
    [InlineData("mini_cex", "mini_cex")]
    [InlineData("MINI_CEX", "mini_cex")]
    [InlineData("\tDOPS\n", "dops")]
    [InlineData("Cbd", "cbd")]
    [InlineData("  Direct_Observation", "direct_observation")]
    [InlineData("Chart_Stimulated_Recall  ", "chart_stimulated_recall")]
    public void AKeyIsTrimmedAndLowerCased(string key, string expected)
    {
        Assert.Equal(expected, WbaTool.NormalizeKey(key));
    }

    /// <summary>
    /// Only the ends are trimmed. The underscore and anything else inside the key are part of its spelling, which
    /// T122 chose to match the <c>RatedActivityTypes</c> family names so that map can later be retired by a lookup.
    /// </summary>
    [Fact]
    public void TheInsideOfAKeyIsLeftAlone()
    {
        Assert.Equal("reflective_exercise", WbaTool.NormalizeKey("  Reflective_Exercise  "));
        Assert.Equal("mini cex", WbaTool.NormalizeKey(" Mini CEX "));
    }

    /// <summary>
    /// Invariant, not current-culture, lower-casing. Under a Turkish culture <c>"I".ToLower()</c> is the dotless
    /// <c>"ı"</c>, so a culture-sensitive normaliser would turn <c>MINI_CEX</c> into <c>mını_cex</c> on a server
    /// whose locale happened to be tr-TR, and that key would match no allow-list the seeder wrote.
    /// </summary>
    [Theory]
    [InlineData("MINI_CEX", "mini_cex")]
    [InlineData("CLINICAL_AUDIT", "clinical_audit")]
    [InlineData("DIRECT_OBSERVATION", "direct_observation")]
    [InlineData("PORTFOLIO_REVIEW", "portfolio_review")]
    public void CaseFoldsTheSameWayUnderATurkishCulture(string key, string expected)
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

            // Proves the culture is actually hostile on this machine, so the assertion below is not vacuous.
            Assert.NotEqual(expected, key.ToLower(CultureInfo.CurrentCulture));

            Assert.Equal(expected, WbaTool.NormalizeKey(key));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    /// <summary>
    /// Normalising a normalised key changes nothing. The allow-list writer normalises, the stored value is read back
    /// through the parser which normalises again, and the predicate normalises both sides once more; each pass must
    /// be a no-op on the previous one or the comparison drifts.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    [InlineData(" Mini_CEX ")]
    [InlineData("dops")]
    [InlineData("LEARNER_FEEDBACK")]
    public void NormalisingTwiceIsTheSameAsNormalisingOnce(string? key)
    {
        var once = WbaTool.NormalizeKey(key);

        Assert.Equal(once, WbaTool.NormalizeKey(once));
    }
}
