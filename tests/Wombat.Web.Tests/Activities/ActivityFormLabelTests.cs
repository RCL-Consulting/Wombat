using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities.Schema;
using Wombat.Web.Components.Shared.Activities;
using Wombat.Web.Tests.Accessibility;
using SchemaField = Wombat.Domain.Activities.Schema.FormField;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T177: every label on an activity form names a control that exists. A multi-choice field sat in a
/// <c>FormField</c> whose <c>&lt;label for="{key}"&gt;</c> named no element, each checkbox having an id of its
/// own, and the file placeholder's label named nothing at all. Both are now a fieldset named by its legend
/// (DESIGN.md § Form system). Every seeded form and every field type is held to the rule, so the next field
/// type or seed cannot bring the defect back unseen.
/// </summary>
public sealed class ActivityFormLabelTests : TestContext
{
    public ActivityFormLabelTests()
    {
        this.AddTestAuthorization().SetAuthorized("trainee@test");
        Services.AddSingleton<IActivityReferenceDataService, StubActivityReferenceDataService>();
    }

    // ---- the seed corpus ----

    public static TheoryData<string> SeedKeys => SeedSchemas.KeysAsTheoryData();

    [Fact]
    public void TheSeedCorpusReachesThisProject_WithAMultiChoiceAndAFileField_SoTheTheoryIsNotVacuous()
    {
        // If the output copy of the seed folders ever stops reaching this test project, the theory below would pass
        // with no rows. The two seeds named here carry the two field types the defect lived in.
        SeedSchemas.Keys().Should().HaveCountGreaterThanOrEqualTo(19)
            .And.Contain(["cca_cpsa", "research_output", "mini_cex_cpsa"]);

        SeedSchemas.Fields("cca_cpsa").Should().Contain(field => field.Type == FieldType.MultiChoice);
        SeedSchemas.Fields("research_output").Should().Contain(field => field.Type == FieldType.File);
    }

    [Theory]
    [MemberData(nameof(SeedKeys))]
    public void ASeededForm_EveryLabelAndDescriptionNamesARealElement(string seedKey)
    {
        var cut = RenderForm(SeedSchemas.EveryFieldVisible(SeedSchemas.Schema(seedKey)));

        IdReferences.Broken(cut).Should().BeEmpty("every label on the '{0}' form must name a real control", seedKey);
    }

    [Theory]
    [MemberData(nameof(SeedKeys))]
    public void ASeededForm_NamesEveryField(string seedKey)
    {
        // The check above passes on a form that renders no labels at all, so each field is also shown to be named:
        // by a label on its one control, or by the legend of its group.
        var cut = RenderForm(SeedSchemas.EveryFieldVisible(SeedSchemas.Schema(seedKey)));

        foreach (var field in SeedSchemas.Fields(seedKey))
        {
            NameOf(cut, field).Should().Contain(field.Label, "'{0}.{1}' must be named on the form", seedKey, field.Key);
        }
    }

    // ---- every field type ----

    public static TheoryData<FieldType> FieldTypes
    {
        get
        {
            var data = new TheoryData<FieldType>();
            foreach (var type in Enum.GetValues<FieldType>())
            {
                data.Add(type);
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(FieldTypes))]
    public void AFieldOfEveryType_IsNamed_AndNamesOnlyRealElements(FieldType type)
    {
        // The seeds use ten of the seventeen types; a builder-made type may use any. A type ActivityForm renders
        // through FormField without a control whose id is the key would leave a label naming nothing.
        var field = OneField(type);

        var cut = RenderForm(OneFieldSchema(field));

        IdReferences.Broken(cut).Should().BeEmpty("a {0} field must name only real elements", type);
        NameOf(cut, field).Should().Contain("The field", "a {0} field must be named", type);
    }

    /// <summary>
    /// The types ActivityForm renders as a group named by its legend: several controls (multi-choice), none yet (the file
    /// placeholder, T154), or none at all. Every other type is one control whose id is the key. Pinned, as the seed lists
    /// are: giving a type a control of its own means moving it out of here on purpose.
    /// </summary>
    private static readonly FieldType[] GroupTypes =
        [FieldType.MultiChoice, FieldType.File, FieldType.DateTime, FieldType.Checkbox, FieldType.Markdown];

    /// <summary>The types ActivityForm has no control for at all, which say so on the form.</summary>
    private static readonly FieldType[] UnsupportedTypes = [FieldType.DateTime, FieldType.Checkbox, FieldType.Markdown];

    [Theory]
    [MemberData(nameof(FieldTypes))]
    public void AFieldOfEveryType_IsOneKeyedControl_UnlessItIsAGroup(FieldType type)
    {
        // ActivityForm chooses FormField or a fieldset with one list (IsSingleControl) and renders the control with
        // another (its switch). The check above catches only a type in the list with no case in the switch. The other
        // drift, a type with a case that the list leaves out, renders "Unsupported field type" in a named group and
        // breaks no reference, so each type's shape is held here.
        var cut = RenderForm(OneFieldSchema(OneField(type)));

        var control = cut.FindAll("#f").SingleOrDefault();
        if (GroupTypes.Contains(type))
        {
            control.Should().BeNull("a {0} field is a group, not one control keyed by the field", type);
            cut.FindAll("fieldset.form-group > legend").Should().ContainSingle()
                .Which.TextContent.Should().Contain("The field");
        }
        else
        {
            control.Should().NotBeNull("a {0} field is one control whose id is the field's key", type);
            control!.LocalName.Should().BeOneOf("input", "select", "textarea");
            cut.FindAll("label[for=f]").Should().ContainSingle()
                .Which.TextContent.Should().Contain("The field");
        }

        cut.Markup.Contains("Unsupported field type", StringComparison.Ordinal)
            .Should().Be(UnsupportedTypes.Contains(type), "only a type ActivityForm has no control for says so");
    }

    // ---- the two shapes the defect lived in ----

    [Fact]
    public void AMultiChoiceField_IsAFieldsetNamedByItsLegend_EachCheckboxLabelledByItsOption()
    {
        var cut = RenderForm("""
            {
              "version": 1,
              "sections": [
                {
                  "key": "case",
                  "title": "Case",
                  "fields": [
                    { "key": "reference", "type": "text", "label": "Case" },
                    {
                      "key": "documents_reviewed", "type": "multichoice", "label": "Documentation reviewed", "required": true,
                      "help_text": "Tick every document you discussed.",
                      "options": ["admission_notes", "progress_notes", "prescriptions"]
                    }
                  ]
                }
              ]
            }
            """);

        // The section's own fieldset holds the field's.
        var group = cut.Find("fieldset fieldset");

        // The legend names the group and says it is required the way FormField says it of a single field.
        var legend = group.QuerySelector("legend")!;
        legend.TextContent.Should().Contain("Documentation reviewed");
        legend.QuerySelector(".visually-hidden")!.TextContent.Trim().Should().Be("required");

        // It reads as a field among fields: a form group spanning the row, not a new section.
        group.ClassList.Should().Contain(["form-group", "full-width"]);

        // Every checkbox is in the group's grid, each with a label naming it and only it.
        var checks = group.QuerySelectorAll(".check-grid > .form-check").ToList();
        checks.Select(check => check.QuerySelector("input[type=checkbox]")!.Id)
            .Should().Equal("documents_reviewed-admission_notes", "documents_reviewed-progress_notes", "documents_reviewed-prescriptions");
        checks.Select(check => check.QuerySelector("label")!.GetAttribute("for"))
            .Should().Equal(checks.Select(check => check.QuerySelector("input[type=checkbox]")!.Id));
        checks.Select(check => check.QuerySelector("label")!.TextContent.Trim())
            .Should().Equal("admission_notes", "progress_notes", "prescriptions");

        // The help text is announced with the group, not left as text a screen reader may skip.
        var help = cut.Find($"#{group.GetAttribute("aria-describedby")}");
        help.TextContent.Should().Be("Tick every document you discussed.");
        group.Contains(help).Should().BeTrue();

        // And no label claims to name the group: the old one pointed at an id nothing carried.
        cut.FindAll("label").Should().NotContain(label => label.TextContent.Contains("Documentation reviewed"));
        cut.FindAll("#documents_reviewed").Should().BeEmpty();
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void AMultiChoiceFieldWithNoHelpText_DescribesNothing()
    {
        var cut = RenderForm("""
            { "version": 1, "sections": [ { "key": "s", "title": "S", "fields": [
              { "key": "settings", "type": "multichoice", "label": "Settings", "options": ["ward", "clinic"] }
            ] } ] }
            """);

        var group = cut.Find("fieldset fieldset");
        group.HasAttribute("aria-describedby").Should().BeFalse();
        group.QuerySelector("legend .visually-hidden").Should().BeNull("an optional field is not marked required");
    }

    [Fact]
    public void AFileField_IsNamedByALegend_AndHasNoLabelPointingAtItsPlaceholder()
    {
        var cut = RenderForm("""
            { "version": 1, "sections": [ { "key": "s", "title": "S", "fields": [
              { "key": "title", "type": "text", "label": "Title" },
              { "key": "supporting_file", "type": "file", "label": "PDF upload" }
            ] } ] }
            """);

        var group = cut.Find("fieldset fieldset");
        group.QuerySelector("legend")!.TextContent.Trim().Should().Be("PDF upload");
        group.TextContent.Should().Contain("File uploads are represented in the schema");
        group.ClassList.Should().Contain("form-group").And.NotContain("full-width");

        cut.FindAll("label").Select(label => label.TextContent.Trim()).Should().Equal("Title");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    // ---- helpers ----

    private static SchemaField OneField(FieldType type)
    {
        IReadOnlyList<FieldOption> options = type is FieldType.Choice or FieldType.MultiChoice
            ? [FieldOption.Unlabelled("a"), FieldOption.Unlabelled("b")]
            : [];
        return new SchemaField("f", type, "The field", "Help for the field", true, options, null, null, null, null, null, null);
    }

    private static string OneFieldSchema(SchemaField field)
        => FormSchemaParser.Serialize(new FormSchema(1, [new FormSection("s", "S", null, [field], null)]));

    private IRenderedComponent<ActivityForm> RenderForm(string schemaJson)
        => RenderComponent<ActivityForm>(parameters => parameters
            .Add(component => component.SchemaJson, schemaJson)
            .Add(component => component.DataJson, "{}"));

    /// <summary>
    /// What names a field on the rendered form: the label whose <c>for</c> is its key, or else the legend of the
    /// fieldset holding its controls. Empty when neither exists.
    /// </summary>
    private static string NameOf(IRenderedFragment cut, SchemaField field)
    {
        var label = cut.FindAll("label[for]")
            .FirstOrDefault(candidate => string.Equals(candidate.GetAttribute("for"), field.Key, StringComparison.Ordinal));
        if (label is not null)
        {
            return label.TextContent;
        }

        var legend = cut.FindAll("fieldset fieldset > legend")
            .FirstOrDefault(candidate => candidate.TextContent.Contains(field.Label, StringComparison.Ordinal));
        return legend?.TextContent ?? string.Empty;
    }
}
