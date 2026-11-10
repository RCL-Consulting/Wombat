using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Domain.Tests.Activities;

/// <summary>
/// The refusal's shape (T398, flow 17, build-contracts § 0, Q6, J2, S2): where a refusal of a draft belongs, and the
/// refusal that carries every place at once (P2). Lane A extends these with each parser's and cross-check's places.
/// </summary>
public sealed class RefusalPlaceTests
{
    [Fact]
    public void A_Place_NamesItsTabKindAndSentence_AndNothingElse_ByDefault()
    {
        var place = new RefusalPlace(RefusalTab.Form, RefusalKind.Other, "Field key 'topic' is used by 2 fields.");

        Assert.Equal(RefusalTab.Form, place.Tab);
        Assert.Equal(RefusalKind.Other, place.Kind);
        Assert.Equal("Field key 'topic' is used by 2 fields.", place.Sentence);
        Assert.Null(place.SectionKey);
        Assert.NotNull(place.FieldKeys);
        Assert.Empty(place.FieldKeys);
        Assert.Null(place.MoveKey);
        Assert.Null(place.DirectiveNumber);
        Assert.Null(place.NamedField);
        Assert.Null(place.ExpectedField);
        Assert.Null(place.JsonLine);
    }

    [Fact]
    public void A_Place_CarriesWhatItsKindSets()
    {
        var place = new RefusalPlace(RefusalTab.Credit, RefusalKind.CreditEpaFieldDisagrees, "Rule 1 reads its EPA from 'epa'.")
        {
            DirectiveNumber = 1,
            NamedField = "epa",
            ExpectedField = "epa_id"
        };

        Assert.Equal(1, place.DirectiveNumber);
        Assert.Equal("epa", place.NamedField);
        Assert.Equal("epa_id", place.ExpectedField);
        Assert.NotEqual(place, place with { DirectiveNumber = 2 });
    }

    [Fact]
    public void A_RefusedDraft_CarriesEveryPlace_AndSaysTheirSentencesInOrder()
    {
        RefusalPlace[] places =
        [
            new(RefusalTab.Form, RefusalKind.EmptySection, "Section 'more' has no fields.") { SectionKey = "more" },
            new(RefusalTab.Workflow, RefusalKind.FieldRuleNotUserField, "Transition 'cancel' names field 'topic'.")
            {
                MoveKey = "cancel",
                NamedField = "topic"
            }
        ];

        var refused = new ActivityTypeDraftRefusedException(places);

        Assert.Equal(places, refused.Places);
        Assert.Equal("Section 'more' has no fields. Transition 'cancel' names field 'topic'.", refused.Message);

        // An InvalidOperationException, so a caller reading a refusal's message as before still reads one.
        Assert.IsAssignableFrom<InvalidOperationException>(refused);
        Assert.IsAssignableFrom<IActivityTypeRefusal>(refused);
    }

    [Fact]
    public void A_RefusedDraft_NamesAtLeastOnePlace()
        => Assert.Throws<ArgumentException>(() => new ActivityTypeDraftRefusedException([]));

    // ---- the parse exceptions' places (lane A) ---------------------------------------------------

    /// <summary>
    /// A parse exception thrown with its message alone carries one place, its own tab's, the message its sentence: the
    /// summary line only. Its message is unchanged.
    /// </summary>
    [Fact]
    public void A_ParseException_WithItsMessageAlone_IsItsTabsAlone()
    {
        IActivityTypeRefusal[] refusals =
        [
            new SchemaParseException("Schema root must be an object."),
            new WorkflowParseException("Workflow root must be an object."),
            new CreditRulesParseException("Credit rules root must be an object.")
        ];

        Assert.Equal(
            [
                new RefusalPlace(RefusalTab.Form, RefusalKind.Other, "Schema root must be an object."),
                new RefusalPlace(RefusalTab.Workflow, RefusalKind.Other, "Workflow root must be an object."),
                new RefusalPlace(RefusalTab.Credit, RefusalKind.Other, "Credit rules root must be an object.")
            ],
            refusals.Select(refusal => Assert.Single(refusal.Places)));
        Assert.Equal("Schema root must be an object.", ((Exception)refusals[0]).Message);
    }

    [Fact]
    public void A_ParseException_WithPlaces_KeepsItsMessage_AndNamesAtLeastOne()
    {
        var place = new RefusalPlace(RefusalTab.Form, RefusalKind.EmptySection, "Section 'more' must contain at least one field.");

        var exception = new SchemaParseException("Its message.", [place]);

        Assert.Equal("Its message.", exception.Message);
        Assert.Equal([place], exception.Places);
        Assert.Throws<ArgumentException>(() => new WorkflowParseException("Its message.", []));
    }

    /// <summary>R3-C20: an empty section is at its head.</summary>
    [Fact]
    public void Schema_AnEmptySection_IsAtTheSection()
    {
        var exception = Assert.Throws<SchemaParseException>(() => FormSchemaParser.Parse(Schema(
            """{ "key": "more", "title": "More", "fields": [] }""")));

        var place = Assert.Single(exception.Places);
        Assert.Equal((RefusalTab.Form, RefusalKind.EmptySection, "more"), (place.Tab, place.Kind, place.SectionKey));
        Assert.Equal("Section 'more' must contain at least one field.", place.Sentence);
        Assert.Equal(place.Sentence, exception.Message);
    }

    /// <summary>A field's refusal from a helper that knows no place is at the field, in its section.</summary>
    [Fact]
    public void Schema_AFieldRefusal_IsAtTheFieldInItsSection()
    {
        var exception = Assert.Throws<SchemaParseException>(() => FormSchemaParser.Parse(Schema(
            """{ "key": "more", "title": "More", "fields": [ { "key": "topic", "type": "essay", "label": "Topic" } ] }""")));

        var place = Assert.Single(exception.Places);
        Assert.Equal((RefusalKind.Other, "more"), (place.Kind, place.SectionKey));
        Assert.Equal(["topic"], place.FieldKeys);
        Assert.Equal("Unknown field type 'essay'.", place.Sentence);
    }

    /// <summary>A field's actor rule, rethrown as the schema's, is at the field too.</summary>
    [Fact]
    public void Schema_AFieldsActorRule_IsAtTheField()
    {
        var exception = Assert.Throws<SchemaParseException>(() => FormSchemaParser.Parse(Schema(
            """{ "key": "more", "title": "More", "fields": [ { "key": "topic", "type": "text", "label": "Topic", "editable_by": "everyone" } ] }""")));

        Assert.Equal(["topic"], Assert.Single(exception.Places).FieldKeys);
        Assert.StartsWith("Property 'editable_by' is invalid: ", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Schema_ASectionRefusalBeforeItsFields_IsAtTheSection()
    {
        var exception = Assert.Throws<SchemaParseException>(() => FormSchemaParser.Parse(Schema(
            """{ "key": "more", "fields": [ { "key": "topic", "type": "text", "label": "Topic" } ] }""")));

        var place = Assert.Single(exception.Places);
        Assert.Equal("more", place.SectionKey);
        Assert.Empty(place.FieldKeys);
    }

    /// <summary>A form setting naming a field of the wrong type is at that field, naming it.</summary>
    [Fact]
    public void Schema_APointerAtTheWrongField_IsAtThatField()
    {
        var exception = Assert.Throws<SchemaParseException>(() => FormSchemaParser.Parse("""
            {
              "version": 1,
              "observation_date_field": "topic",
              "sections": [ { "key": "more", "title": "More", "fields": [ { "key": "topic", "type": "text", "label": "Topic" } ] } ]
            }
            """));

        var place = Assert.Single(exception.Places);
        Assert.Equal(("topic", "more"), (place.NamedField, place.SectionKey));
        Assert.Equal(["topic"], place.FieldKeys);
    }

    [Fact]
    public void Schema_MalformedJson_IsAtItsLine()
    {
        var exception = Assert.Throws<SchemaParseException>(() => FormSchemaParser.Parse("{\n  \"version\": 1,\n  \"sections\": [\n    ]]\n}"));

        var place = Assert.Single(exception.Places);
        Assert.Equal((RefusalTab.Form, RefusalKind.MalformedJson, 4L), (place.Tab, place.Kind, place.JsonLine));
        Assert.StartsWith("Schema JSON is malformed: ", exception.Message, StringComparison.Ordinal);
    }

    private static string Schema(string section) => $$"""
        {
          "version": 1,
          "sections": [
            { "key": "first", "title": "First", "fields": [ { "key": "title", "type": "text", "label": "Title" } ] },
            {{section}}
          ]
        }
        """;
}
