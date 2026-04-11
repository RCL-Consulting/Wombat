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
}
