using Wombat.Domain.Activities.Schema;

namespace Wombat.Domain.Tests.Activities;

/// <summary>
/// T191: a choice option may carry a label, so the form shows "Admission notes" and the activity stores
/// <c>admission_notes</c>. The label is words for a reader and nothing else; the value is what is stored and compared.
/// </summary>
public sealed class FormSchemaParserOptionLabelTests
{
    private const string Labelled = """
        {
          "version": 1,
          "sections": [
            {
              "key": "case",
              "title": "Case",
              "fields": [
                {
                  "key": "documents_reviewed", "type": "multichoice", "label": "Documentation reviewed",
                  "options": [
                    { "value": "admission_notes", "label": "Admission notes" },
                    "prescriptions"
                  ]
                }
              ]
            }
          ]
        }
        """;

    [Fact]
    public void AnObjectOption_IsReadAsItsValueAndItsLabel_AndABareStringIsItsOwnLabel()
    {
        var field = OnlyField(FormSchemaParser.Parse(Labelled));

        Assert.Equal(
            [new FieldOption("admission_notes", "Admission notes"), new FieldOption("prescriptions", "prescriptions")],
            field.Options);
        Assert.True(field.Options[0].IsLabelled);
        Assert.False(field.Options[1].IsLabelled);
    }

    [Fact]
    public void ALabel_SurvivesSerialize_AsAnObject_AndAnUnlabelledOptionStaysABareString()
    {
        // The Serialize half. Without it SaveDraft would drop every label at publish and the form would show keys
        // again, with nothing failing (CLAUDE.md, "Parse is not enough").
        var canonical = FormSchemaParser.Serialize(FormSchemaParser.Parse(Labelled));

        Assert.Contains("""{"value":"admission_notes","label":"Admission notes"},"prescriptions"]""", canonical, StringComparison.Ordinal);
        Assert.Equal(canonical, FormSchemaParser.Serialize(FormSchemaParser.Parse(canonical)));
        Assert.Equal(OnlyField(FormSchemaParser.Parse(Labelled)).Options, OnlyField(FormSchemaParser.Parse(canonical)).Options);
    }

    [Fact]
    public void AnOptionLabelledWithItsOwnValue_CanonicalisesToTheBareString()
    {
        // The bare string and the object that says the same thing are one option, so they canonicalise alike and the
        // refresher sees no difference between them.
        var spelledOut = FormSchemaParser.Serialize(FormSchemaParser.Parse(Schema("""{ "value": "ward", "label": "ward" }""")));
        var bare = FormSchemaParser.Serialize(FormSchemaParser.Parse(Schema("\"ward\"")));

        Assert.Equal(bare, spelledOut);
        Assert.Contains("\"options\":[\"ward\"]", bare, StringComparison.Ordinal);
    }

    [Fact]
    public void ALabelAndAValue_AreTrimmed()
    {
        var field = OnlyField(FormSchemaParser.Parse(Schema("""{ "value": " ward ", "label": " Ward " }""")));

        Assert.Equal([new FieldOption("ward", "Ward")], field.Options);
    }

    [Theory]
    [InlineData("""{ "value": "ward" }""", "label")]
    [InlineData("""{ "label": "Ward" }""", "value")]
    [InlineData("""{ "value": "ward", "label": "  " }""", "label")]
    [InlineData("""{ "value": "", "label": "Ward" }""", "value")]
    [InlineData("""{ "value": "ward", "label": "Ward", "colour": "red" }""", "colour")]
    [InlineData("""{ "value": 1, "label": "One" }""", "value")]
    [InlineData("1", "options")]
    [InlineData("\"  \"", "options")]
    public void AMalformedOption_IsRefused_NamingWhatIsWrong(string option, string named)
    {
        var exception = Assert.Throws<SchemaParseException>(() => FormSchemaParser.Parse(Schema(option)));

        Assert.Contains(named, exception.Message, StringComparison.Ordinal);

        // An option has no key of its own, so the field's is the only way to find it in a long schema.
        Assert.Contains("'setting'", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("\"high|low\"", "'|'")]
    [InlineData("""{ "value": "high|low", "label": "High or low" }""", "'|'")]
    [InlineData("\"high\\nlow\"", "one line")]
    [InlineData("""{ "value": "high\r\nlow", "label": "High" }""", "one line")]
    [InlineData("""{ "value": "high", "label": "High\nor low" }""", "one line")]
    public void AnOptionTheBuildersBoxCouldNotCarry_IsRefused(string option, string named)
    {
        // The builder's Options box is one option per line, the value ending at the first bar. A value holding a bar or
        // a line break, or a label holding a line break, would come back from an open-and-save as other options, and the
        // activities of the next version would store different values, with nothing said.
        var exception = Assert.Throws<SchemaParseException>(() => FormSchemaParser.Parse(Schema(option)));

        Assert.Contains(named, exception.Message, StringComparison.Ordinal);
        Assert.Contains("'setting'", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ALabelMayHoldABarAndAComma_AndAValueACommaAndSpaces()
    {
        var field = OnlyField(FormSchemaParser.Parse(Schema(
            """{ "value": "other", "label": "Other | not listed, or unsure" }, "Notes, letters", "high care" """)));

        Assert.Equal(
            [
                new FieldOption("other", "Other | not listed, or unsure"),
                FieldOption.Unlabelled("Notes, letters"),
                FieldOption.Unlabelled("high care")
            ],
            field.Options);
    }

    [Theory]
    [InlineData("\"ward\", \"ward\"")]
    [InlineData("""{ "value": "ward", "label": "Ward" }, "ward" """)]
    [InlineData("""{ "value": "ward", "label": "Ward" }, { "value": " ward ", "label": "General ward" }""")]
    public void AValueOfferedTwice_IsRefused_NamingTheFieldAndTheValue(string options)
    {
        // An activity stores the value, so two options sharing one are two choices nobody can tell apart once chosen,
        // read back under whichever label comes first.
        var exception = Assert.Throws<SchemaParseException>(() => FormSchemaParser.Parse(Schema(options)));

        Assert.Contains("'setting'", exception.Message, StringComparison.Ordinal);
        Assert.Contains("'ward' twice", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValuesThatDifferOnlyInCase_AreTwoOptions()
    {
        // Values compare ordinally everywhere they are checked (Offers, the validator), so these are distinct keys.
        var field = OnlyField(FormSchemaParser.Parse(Schema("\"ward\", \"Ward\"")));

        Assert.Equal([FieldOption.Unlabelled("ward"), FieldOption.Unlabelled("Ward")], field.Options);
    }

    [Fact]
    public void LabelFor_ReadsAStoredValueAsItsLabel_AndAValueNoOptionDeclaresAsItself()
    {
        var field = OnlyField(FormSchemaParser.Parse(Labelled));

        Assert.Equal("Admission notes", field.LabelFor("admission_notes"));
        Assert.Equal("prescriptions", field.LabelFor("prescriptions"));

        // A value the pinned version never offered prints as stored.
        Assert.Equal("discharge_summary", field.LabelFor("discharge_summary"));
    }

    [Fact]
    public void Offers_MatchesTheStoredValue_NeverTheLabel()
    {
        var field = OnlyField(FormSchemaParser.Parse(Labelled));

        Assert.True(field.Offers("admission_notes"));

        // The label is words for a reader, not something an activity stores; values compare ordinally.
        Assert.False(field.Offers("Admission notes"));
        Assert.False(field.Offers("ADMISSION_NOTES"));
    }

    private static string Schema(string option) => $$"""
        {
          "version": 1,
          "sections": [
            { "key": "s", "title": "S", "fields": [ { "key": "setting", "type": "choice", "label": "Setting", "options": [{{option}}] } ] }
          ]
        }
        """;

    private static FormField OnlyField(FormSchema schema) => schema.Sections.Single().Fields.Single();
}
