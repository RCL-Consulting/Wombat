using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Credit;

namespace Wombat.Domain.Tests.Activities;

public sealed class CreditRulesParserTests
{
    [Fact]
    public void Parse_ValidRules_ReturnsRules()
    {
        var rules = CreditRulesParser.Parse(ActivityTestData.ValidCreditRulesJson);

        Assert.Single(rules.CountsFor);
        Assert.Equal("epa_id", rules.CountsFor[0].CurriculumItemMatchRule.EpaField);
    }

    [Fact]
    public void Parse_MalformedDirective_Throws()
    {
        const string json = """
            {
              "counts_for": [
                {
                  "curriculum_item_match": {
                    "epa_field": "epa_id"
                  },
                  "amount": 1,
                  "unexpected": true
                }
              ]
            }
            """;

        var exception = Assert.Throws<CreditRulesParseException>(() => CreditRulesParser.Parse(json));

        Assert.Contains("unexpected", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Serialize_RoundTripsModuloWhitespace()
    {
        var parsed = CreditRulesParser.Parse(ActivityTestData.ValidCreditRulesJson);
        var serialized = CreditRulesParser.Serialize(parsed);

        Assert.Equal(
            ActivityTestData.NormalizeJson(ActivityTestData.ValidCreditRulesJson),
            ActivityTestData.NormalizeJson(serialized));
    }

    /// <summary>T398 (Q13, S11): laid out for the page, in the compact order, and parsing alike.</summary>
    [Fact]
    public void Serialize_Indented_IsOnePropertyALine_InTheCompactOrder_AndParsesAlike()
    {
        var parsed = CreditRulesParser.Parse(ActivityTestData.ValidCreditRulesJson);
        var compact = CreditRulesParser.Serialize(parsed);
        var indented = CreditRulesParser.Serialize(parsed, indented: true);

        Assert.DoesNotContain('\n', compact);
        Assert.StartsWith("{\n  \"counts_for\": [\n    {\n      \"curriculum_item_match\": {\n        \"epa_field\": ", indented);
        Assert.Equal(compact, CreditRulesParser.Serialize(CreditRulesParser.Parse(indented)));
    }

    /// <summary>The empty rules laid out, as a new type's Credit tab shows them (R2-D03).</summary>
    [Fact]
    public void Serialize_Indented_NoDirective()
        => Assert.Equal(
            "{\n  \"counts_for\": []\n}",
            CreditRulesParser.Serialize(CreditRulesParser.Parse("{\"counts_for\":[]}"), indented: true));

    /// <summary>T398 (Q6, S4): every refusal of a directive is at its number, counted from 1.</summary>
    [Fact]
    public void Parse_ARefusalOfADirective_IsAtItsNumber()
    {
        const string json = """
            {
              "counts_for": [
                { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1 },
                { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": "one" }
              ]
            }
            """;

        var exception = Assert.Throws<CreditRulesParseException>(() => CreditRulesParser.Parse(json));

        var place = Assert.Single(exception.Places);
        Assert.Equal((RefusalTab.Credit, 2), (place.Tab, place.DirectiveNumber));
        Assert.Equal("Property 'amount' must be an integer.", place.Sentence);
    }

    [Fact]
    public void Parse_MalformedJson_IsAtItsLine()
    {
        var exception = Assert.Throws<CreditRulesParseException>(
            () => CreditRulesParser.Parse("{\n  \"counts_for\": [\n    }\n"));

        var place = Assert.Single(exception.Places);
        Assert.Equal((RefusalKind.MalformedJson, 3L), (place.Kind, place.JsonLine));
    }
}
