using FluentAssertions;
using Wombat.Domain.Activities.Schema;
using Wombat.Web.Components.Pages.Admin.ActivityTypes;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T133. <c>BuilderSchemaModel</c> is the only schema-authoring path the product has — the Form tab is
/// a structured builder, and the raw textareas are for the workflow and credit DSLs only. It had no
/// test of any kind, which is how T119's <c>observation_date_field</c> and T126's
/// <c>rated_level_field</c> came to be erased on every operator draft save with nothing going red.
/// </summary>
public sealed class BuilderSchemaModelTests
{
    private const string BothPointers = """
        {
          "version": 1,
          "observation_date_field": "observed_on",
          "rated_level_field": "overall",
          "sections": [
            {
              "key": "encounter",
              "title": "Encounter",
              "fields": [
                { "key": "observed_on", "type": "date", "label": "Date observed", "required": true },
                { "key": "overall", "type": "scale", "label": "Overall", "required": false, "options": ["1", "2"], "scale_key": "O-R Scale" },
                { "key": "feedback", "type": "longtext", "label": "Feedback", "required": false }
              ]
            }
          ]
        }
        """;

    [Fact]
    public void BothRootPointersSurviveTheBuilderRoundTrip()
    {
        // The defect: ToJson called the two-argument FormSchema constructor and Parse never read them,
        // so an operator who opened a type and saved a draft silently un-declared both.
        var model = BuilderSchemaModel.Parse(BothPointers);

        model.ObservationDateField.Should().Be("observed_on");
        model.RatedLevelField.Should().Be("overall");

        var reparsed = FormSchemaParser.Parse(model.ToJson());

        reparsed.ObservationDateField.Should().Be("observed_on");
        reparsed.RatedLevelField.Should().Be("overall");
    }

    [Fact]
    public void ASchemaWithNoPointersStaysWithoutThem()
    {
        const string none = """
            {
              "version": 1,
              "sections": [
                {
                  "key": "reflection",
                  "title": "Reflection",
                  "fields": [
                    { "key": "what_i_learned", "type": "longtext", "label": "What I learned" }
                  ]
                }
              ]
            }
            """;

        var json = BuilderSchemaModel.Parse(none).ToJson();

        json.Should().NotContain("observation_date_field");
        json.Should().NotContain("rated_level_field");
    }

    /// <summary>
    /// The dead end this task had to avoid. Once the builder carries a pointer through, deleting the
    /// field it names produces a schema <c>FormSchemaParser</c> REFUSES — and <c>ActivityType.SaveDraft</c>
    /// runs the parser, so the type would become permanently unsaveable through the only door there is.
    /// </summary>
    [Fact]
    public void DeletingThePointedAtFieldDoesNotMakeTheDraftUnsaveable()
    {
        var model = BuilderSchemaModel.Parse(BothPointers);
        var section = model.Sections.Single();
        section.Fields.RemoveAll(field => field.Key is "observed_on" or "overall");

        var json = model.ToJson();

        // The pointer is dropped rather than emitted orphaned...
        json.Should().NotContain("observation_date_field");
        json.Should().NotContain("rated_level_field");
        // ...and the result is something SaveDraft can actually accept.
        var act = () => FormSchemaParser.Parse(json);
        act.Should().NotThrow();
    }

    /// <summary>
    /// Dropping an orphaned pointer is only defensible because it is visible. This is the half that
    /// makes it so — and it is the warning nobody would otherwise get, since the field-removal warning
    /// says nothing about what went with it.
    /// </summary>
    [Fact]
    public void RemovingAPointerIsWarnedAboutAtPublish()
    {
        var model = BuilderSchemaModel.Parse(BothPointers);
        model.Sections.Single().Fields.RemoveAll(field => field.Key == "overall");

        var warnings = BuilderSchemaModel.GetPublishWarnings(BothPointers, model.ToJson());

        warnings.Should().Contain(warning => warning.Contains("stop recording which field carries the entrustment rating"));
    }

    [Fact]
    public void RepointingIsWarnedAboutAtPublish()
    {
        var model = BuilderSchemaModel.Parse(BothPointers);
        model.Sections.Single().Fields.Add(new BuilderFieldModel
        {
            Key = "global_rating",
            Type = FieldType.Scale,
            Label = "Global rating",
            OptionsText = "1"
        });
        model.RatedLevelField = "global_rating";

        var warnings = BuilderSchemaModel.GetPublishWarnings(BothPointers, model.ToJson());

        warnings.Should().Contain(warning => warning.Contains("moves from field 'overall' to 'global_rating'"));
    }

    [Fact]
    public void DeclaringAPointerForTheFirstTimeIsWarnedAboutAtPublish()
    {
        const string published = """
            {
              "version": 1,
              "sections": [
                {
                  "key": "encounter",
                  "title": "Encounter",
                  "fields": [
                    { "key": "overall", "type": "scale", "label": "Overall", "options": ["1", "2"], "scale_key": "O-R Scale" }
                  ]
                }
              ]
            }
            """;

        var model = BuilderSchemaModel.Parse(published);
        model.RatedLevelField = "overall";

        var warnings = BuilderSchemaModel.GetPublishWarnings(published, model.ToJson());

        warnings.Should().Contain(warning => warning.Contains("'overall' will become the entrustment rating"));
    }

    /// <summary>
    /// A pointer at a field of the wrong type is refused by the parser, so the builder must not emit
    /// one — changing a rated field's type to text would otherwise brick the draft the same way.
    /// </summary>
    [Fact]
    public void APointerAtAFieldWhoseTypeChangedIsDropped()
    {
        var model = BuilderSchemaModel.Parse(BothPointers);
        model.Sections.Single().Fields.Single(field => field.Key == "overall").Type = FieldType.Text;

        var json = model.ToJson();

        json.Should().NotContain("rated_level_field");
        json.Should().Contain("observation_date_field", "only the invalid pointer is dropped");
        FormSchemaParser.Parse(json).ObservationDateField.Should().Be("observed_on");
    }

    /// <summary>
    /// The neighbouring settings this model was already carrying correctly. Pinned so the next root
    /// property added to the DSL fails here rather than silently joining the pattern this task fixed.
    /// </summary>
    [Fact]
    public void FieldLevelSettingsStillSurviveToo()
    {
        var reparsed = FormSchemaParser.Parse(BuilderSchemaModel.Parse(BothPointers).ToJson());
        var fields = reparsed.Sections.Single().Fields;

        fields.Single(field => field.Key == "overall").ScaleKey.Should().Be("O-R Scale");
        fields.Single(field => field.Key == "observed_on").Required.Should().BeTrue();
    }
}
