using System.Globalization;
using FluentAssertions;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Tests.Features.Activities;

/// <summary>
/// T122. <see cref="ToolPermission.Evaluate" /> is the one predicate behind the EPA→tool allow-list: the EPA picker
/// narrows with it and the write path refuses with it (D20), so the two cannot disagree.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ToolPermissionVerdict.NotRestricted" /> is the load-bearing default (D21). An item with no list, and an
/// activity type that declares no instrument, behave exactly as before T122. A predicate that answered NotPermitted
/// where the answer is unknown would empty the picker or refuse a submit nobody can act on, and T123 narrows the same
/// picker from the other direction, so between them they must never empty it.
/// </para>
/// <para>
/// Normalisation is on both sides. An allow-list written by the admin editor and a key stamped by a migration must
/// compare equal whatever casing or padding reached either, or a keyed tool is offered by the picker and then refused at
/// submit, or the reverse.
/// </para>
/// </remarks>
public sealed class ToolPermissionTests
{
    private static readonly string[] CbdDopsMsf = ["cbd", "dops", "msf"];

    // ----- NotRestricted: no list -----

    [Theory]
    [InlineData("mini_cex")]
    [InlineData("cbd")]
    [InlineData(null)]
    [InlineData("")]
    public void ANullListIsNotRestrictedForAnyKey(string? toolKey)
    {
        ToolPermission.Evaluate(null, toolKey).Should().Be(ToolPermissionVerdict.NotRestricted);
    }

    /// <summary>
    /// An empty list means the same as no list. It is never stored (the writer turns it into null), but the parser
    /// returns it for every unusable value, so it is what the predicate actually sees for a null column.
    /// </summary>
    [Theory]
    [InlineData("mini_cex")]
    [InlineData("cbd")]
    [InlineData(null)]
    public void AnEmptyListIsNotRestrictedForAnyKey(string? toolKey)
    {
        ToolPermission.Evaluate(Array.Empty<string>(), toolKey).Should().Be(ToolPermissionVerdict.NotRestricted);
    }

    // ----- NotRestricted: no key -----

    /// <summary>
    /// A type that declares no instrument is unrestricted even against an item that has a list (D21). That is every
    /// builder-made type until an administrator picks an instrument, and every generic seed that is not one.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\n")]
    public void ABlankToolKeyIsNotRestrictedEvenAgainstAList(string? toolKey)
    {
        ToolPermission.Evaluate(CbdDopsMsf, toolKey).Should().Be(ToolPermissionVerdict.NotRestricted);
    }

    // ----- Permitted and NotPermitted -----

    [Theory]
    [InlineData("cbd")]
    [InlineData("dops")]
    [InlineData("msf")]
    public void AListedKeyIsPermitted(string toolKey)
    {
        ToolPermission.Evaluate(CbdDopsMsf, toolKey).Should().Be(ToolPermissionVerdict.Permitted);
    }

    /// <summary>
    /// PAED-005's shape: CBD, DOPS and MSF only. A Mini-CEX filed against it is the refusal T122 exists to make.
    /// </summary>
    [Theory]
    [InlineData("mini_cex")]
    [InlineData("direct_observation")]
    [InlineData("cca")]
    [InlineData("not_a_tool")]
    public void AnUnlistedKeyIsNotPermitted(string toolKey)
    {
        ToolPermission.Evaluate(CbdDopsMsf, toolKey).Should().Be(ToolPermissionVerdict.NotPermitted);
    }

    /// <summary>
    /// A match is on the whole key, never a prefix or a substring: <c>cbd</c> on the list does not admit a
    /// hypothetical <c>cbd_checklist</c>, and <c>mini_cex</c> does not admit <c>mini</c>.
    /// </summary>
    [Theory]
    [InlineData("cb")]
    [InlineData("cbd_checklist")]
    [InlineData("mini")]
    public void AMatchIsOnTheWholeKey(string toolKey)
    {
        ToolPermission.Evaluate(new[] { "cbd", "mini_cex" }, toolKey).Should().Be(ToolPermissionVerdict.NotPermitted);
    }

    // ----- Normalisation on both sides -----

    [Theory]
    [InlineData("CBD")]
    [InlineData(" cbd ")]
    [InlineData("\tCbd\n")]
    [InlineData("DOPS")]
    [InlineData(" Msf")]
    public void TheToolKeyIsNormalisedBeforeItIsCompared(string toolKey)
    {
        ToolPermission.Evaluate(CbdDopsMsf, toolKey).Should().Be(ToolPermissionVerdict.Permitted);
    }

    [Theory]
    [InlineData("cbd")]
    [InlineData("CBD")]
    [InlineData(" dops")]
    public void TheListEntriesAreNormalisedBeforeTheyAreCompared(string toolKey)
    {
        var unnormalisedList = new[] { " CBD ", "Dops\t", "MSF" };

        ToolPermission.Evaluate(unnormalisedList, toolKey).Should().Be(ToolPermissionVerdict.Permitted);
    }

    [Fact]
    public void NormalisationDoesNotTurnAnUnlistedKeyIntoAListedOne()
    {
        ToolPermission.Evaluate(new[] { " CBD ", "Dops\t" }, " Mini_CEX ").Should().Be(ToolPermissionVerdict.NotPermitted);
    }

    /// <summary>
    /// Invariant case folding on both sides. Under tr-TR a culture-sensitive lower-casing would turn <c>MINI_CEX</c>
    /// into <c>mını_cex</c>, and a server with a Turkish locale would refuse the instrument the College listed.
    /// </summary>
    [Fact]
    public void ComparisonIsCultureInvariant()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

            ToolPermission.Evaluate(new[] { "MINI_CEX", "CLINICAL_AUDIT" }, "mini_cex").Should().Be(ToolPermissionVerdict.Permitted);
            ToolPermission.Evaluate(new[] { "mini_cex" }, "MINI_CEX").Should().Be(ToolPermissionVerdict.Permitted);
            ToolPermission.Evaluate(new[] { "clinical_audit" }, "DIRECT_OBSERVATION").Should().Be(ToolPermissionVerdict.NotPermitted);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    // ----- Agreement with the parser -----

    /// <summary>
    /// A list whose every entry is blank is, after the normalisation this predicate applies to its entries, a list of no
    /// keys, and the parser reads the same value as empty. It must therefore be NotRestricted like the empty list, not
    /// NotPermitted: refusing every instrument is the unfileable-EPA state the writer never stores <c>[]</c> to avoid.
    /// </summary>
    /// <remarks>
    /// Every production caller passes <see cref="CurriculumItem.ParsePermittedTools" /> output today, which never
    /// contains a blank, so this is latent. It is pinned because the predicate is public and normalises its list
    /// entries itself, so it claims to accept an un-normalised list.
    /// </remarks>
    [Theory]
    [InlineData("mini_cex")]
    [InlineData("cbd")]
    public void AListOfOnlyBlankEntriesIsNotRestricted(string toolKey)
    {
        ToolPermission.Evaluate(new[] { "", " ", "\t" }, toolKey).Should().Be(ToolPermissionVerdict.NotRestricted);
    }

    public static TheoryData<string[], string?> ParserAgreementCases() => new()
    {
        { Array.Empty<string>(), "mini_cex" },
        { new[] { "cbd", "dops", "msf" }, "mini_cex" },
        { new[] { "cbd", "dops", "msf" }, "cbd" },
        { new[] { " CBD ", "Dops" }, "dops" },
        { new[] { " CBD ", "Dops" }, "MINI_CEX" },
        { new[] { "cbd", "cbd", "CBD" }, " cbd " },
        { new[] { "mini_cex" }, null },
        { new[] { "mini_cex" }, "  " },
        { new[] { "", " " }, "cbd" },
        { new[] { "", "cbd" }, "cbd" },
        { new[] { "", "cbd" }, "dops" }
    };

    /// <summary>
    /// The predicate gives the same verdict on a raw list as on that list written and read back through the stored
    /// form. The picker and the gate read the column through the parser; the admin editor and any future caller may
    /// hand the predicate a list straight from a form. The two must never disagree about what an instrument may credit.
    /// </summary>
    [Theory]
    [MemberData(nameof(ParserAgreementCases))]
    public void TheVerdictOnARawListEqualsTheVerdictOnItsStoredForm(string[] rawList, string? toolKey)
    {
        var storedThenParsed = CurriculumItem.ParsePermittedTools(CurriculumItem.NormalizePermittedToolsJson(rawList));

        ToolPermission.Evaluate(rawList, toolKey).Should().Be(ToolPermission.Evaluate(storedThenParsed, toolKey));
    }

    /// <summary>
    /// The whole truth table in one place, over the stored form the production callers actually pass: the parsed value of
    /// a nullable jsonb column.
    /// </summary>
    [Theory]
    [InlineData(null, "mini_cex", ToolPermissionVerdict.NotRestricted)]
    [InlineData("[]", "mini_cex", ToolPermissionVerdict.NotRestricted)]
    [InlineData("not json", "mini_cex", ToolPermissionVerdict.NotRestricted)]
    [InlineData("{\"cbd\":true}", "mini_cex", ToolPermissionVerdict.NotRestricted)]
    [InlineData("[\"cbd\",\"dops\",\"msf\"]", null, ToolPermissionVerdict.NotRestricted)]
    [InlineData("[\"cbd\",\"dops\",\"msf\"]", "", ToolPermissionVerdict.NotRestricted)]
    [InlineData("[\"cbd\",\"dops\",\"msf\"]", "cbd", ToolPermissionVerdict.Permitted)]
    [InlineData("[\"cbd\", \"dops\", \"msf\"]", " DOPS ", ToolPermissionVerdict.Permitted)]
    [InlineData("[\"cbd\",\"dops\",\"msf\"]", "mini_cex", ToolPermissionVerdict.NotPermitted)]
    [InlineData("[\"CBD\"]", "cbd", ToolPermissionVerdict.Permitted)]
    public void TheTruthTableOverTheStoredColumn(string? storedJson, string? toolKey, ToolPermissionVerdict expected)
    {
        ToolPermission.Evaluate(CurriculumItem.ParsePermittedTools(storedJson), toolKey).Should().Be(expected);
    }
}
