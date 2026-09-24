using FluentAssertions;
using Wombat.Application.Features.Activities.Services;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T108. Which schema fields does the credit engine actually read an EPA out of?
/// </summary>
/// <remarks>
/// The picker narrows on the answer, so a field named here that the engine never reads would hide
/// choices that cannot affect credit — the one way this fix could over-hide.
/// </remarks>
public sealed class CreditRuleFieldsTests
{
    [Fact]
    public void ResolveCreditedEpaFieldKeys_WithAnEpaFieldAlone_ReturnsIt()
    {
        var keys = CreditRuleFields.ResolveCreditedEpaFieldKeys("""
            { "counts_for": [ { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1 } ] }
            """);

        keys.Should().BeEquivalentTo(["epa_id"]);
    }

    /// <summary>
    /// CreditApplier tests curriculum_item_id FIRST and returns before it ever reads epa_field, and
    /// CreditRulesParser does not make the two mutually exclusive — so a rule block can name an
    /// epa_field the engine provably never reads.
    /// </summary>
    [Fact]
    public void ResolveCreditedEpaFieldKeys_WhenAFixedCurriculumItemWins_IgnoresTheEpaField()
    {
        var keys = CreditRuleFields.ResolveCreditedEpaFieldKeys("""
            {
              "counts_for": [
                { "curriculum_item_match": { "epa_field": "epa_id", "curriculum_item_id": 14 }, "amount": 1 }
              ]
            }
            """);

        keys.Should().BeEmpty("the EPA choice cannot affect credit, so narrowing the picker on it would hide choices for no reason");
    }

    [Fact]
    public void ResolveCreditedEpaFieldKeys_WhenACurriculumItemFieldWins_IgnoresTheEpaField()
    {
        var keys = CreditRuleFields.ResolveCreditedEpaFieldKeys("""
            {
              "counts_for": [
                { "curriculum_item_match": { "epa_field": "epa_id", "curriculum_item_field": "item_id" }, "amount": 1 }
              ]
            }
            """);

        keys.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("{ not json")]
    [InlineData("""{ "counts_for": [] }""")]
    [InlineData("""{ "counts_for": [ { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": "one" } ] }""")]
    public void ResolveCreditedEpaFieldKeys_WhenTheRulesAreAbsentOrUnreadable_NarrowsNothing(string? creditRulesJson)
    {
        // Failing open keeps a required field submittable. A malformed credit block is the builder's
        // error to surface, not the runtime form's. The last case is an `amount` of the wrong JSON
        // primitive, which surfaces as InvalidOperationException rather than CreditRulesParseException.
        CreditRuleFields.ResolveCreditedEpaFieldKeys(creditRulesJson).Should().BeEmpty();
    }

    // ---- T154: what the picker narrows, which is what the write path's tool gate judges ----

    private const string CreditsNothing = """{ "counts_for": [] }""";

    [Fact]
    public void ResolveNarrowedEpaFieldKeys_ForAnInstrumentThatCreditsNothing_IsItsEvidenceEpa()
    {
        // The clinical audit, the portfolio review, the reflective exercise: evidence for one EPA, held to its list.
        CreditRuleFields.ResolveNarrowedEpaFieldKeys(CreditsNothing, "epa_id", "clinical_audit")
            .Should().BeEquivalentTo(["epa_id"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ResolveNarrowedEpaFieldKeys_ForATypeThatIsNoInstrument_NarrowsNothing(string? wbaToolKey)
    {
        // D21, and T108's reason a reflective note was never narrowed: its EPA choice changes nothing, and no list binds it.
        CreditRuleFields.ResolveNarrowedEpaFieldKeys(CreditsNothing, "epa_id", wbaToolKey).Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ResolveNarrowedEpaFieldKeys_ForAnInstrumentThatNamesNoEvidenceEpa_NarrowsNothing(string? evidenceEpaField)
    {
        CreditRuleFields.ResolveNarrowedEpaFieldKeys(CreditsNothing, evidenceEpaField, "clinical_audit").Should().BeEmpty();
    }

    [Fact]
    public void ResolveNarrowedEpaFieldKeys_ForATypeThatCredits_IsExactlyTheCreditedFields()
    {
        // A pinned version from before T137 can credit one field and point at another; only what credit reads narrows,
        // as the gate judges only the directives when there are any.
        CreditRuleFields.ResolveNarrowedEpaFieldKeys(
                """{ "counts_for": [ { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1 } ] }""",
                "other_epa_id",
                "mini_cex")
            .Should().BeEquivalentTo(["epa_id"]);

        CreditRuleFields.ResolveNarrowedEpaFieldKeys(
                """{ "counts_for": [ { "curriculum_item_match": { "curriculum_item_id": 14 }, "amount": 1 } ] }""",
                "epa_id",
                "mini_cex")
            .Should().BeEmpty("a type that credits a fixed item is judged on the item, never on its EPA field");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("{ not json")]
    public void ResolveNarrowedEpaFieldKeys_WhenTheRulesAreAbsentOrUnreadable_NarrowsNothing(string? creditRulesJson)
    {
        // The gate fails open on the same input, so the picker must too, or it would hide EPAs submitting accepts.
        CreditRuleFields.ResolveNarrowedEpaFieldKeys(creditRulesJson, "epa_id", "clinical_audit").Should().BeEmpty();
    }
}
