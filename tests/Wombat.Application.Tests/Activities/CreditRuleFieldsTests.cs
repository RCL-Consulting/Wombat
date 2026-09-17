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
}
