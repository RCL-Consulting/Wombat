using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities.Schema;
using Wombat.Web.Components.Shared;
using Wombat.Web.Components.Shared.Activities;
using Wombat.Web.Tests.Accessibility;
using SchemaField = Wombat.Domain.Activities.Schema.FormField;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T193: a field's help text is read with the field. <c>FormField</c> rendered the help under the input, and nothing
/// pointed the input at it, so a screen reader announced "Date observed, edit, date" and never "When the encounter
/// happened, not when this form is completed." Each control now names its help with <c>aria-describedby</c>, first, and
/// then the filing-date notice's live region while the field has one (T160; T192 renamed it, since it may instead hold
/// the pre-programme hint). Every seeded form is held to it.
/// </summary>
public sealed class ActivityFormHelpTextTests : TestContext
{
    private static readonly DateOnly FiledOn = new(2026, 9, 24);

    public ActivityFormHelpTextTests()
    {
        this.AddTestAuthorization().SetAuthorized("trainee@test");
        Services.AddSingleton<IActivityReferenceDataService, StubActivityReferenceDataService>();
    }

    // ---- the seed corpus ----

    public static TheoryData<string> SeedKeys => SeedSchemas.KeysAsTheoryData();

    [Fact]
    public void TheSeedCorpus_CarriesHelpText_OnSingleControlsAndOnGroups_SoTheTheoryIsNotVacuous()
    {
        var withHelp = SeedSchemas.Keys()
            .SelectMany(seedKey => SeedSchemas.Fields(seedKey))
            .Where(field => !string.IsNullOrWhiteSpace(field.HelpText))
            .ToList();

        withHelp.Should().HaveCountGreaterThanOrEqualTo(40);
        withHelp.Should().Contain(field => field.Type == FieldType.Date, "the encounter date's help is the task's example");
        withHelp.Should().Contain(field => field.Type == FieldType.User, "the Assessor hint is the task's other example");
        withHelp.Should().Contain(field => field.Type == FieldType.MultiChoice, "a group's help is linked by its fieldset");
    }

    [Theory]
    [MemberData(nameof(SeedKeys))]
    public void ASeededForm_EveryControlNamesItsHelpText_ThenAnyWarning(string seedKey)
    {
        // Rendered as the create page renders it: writable, filed today, under the seed's own credit rules, so the
        // encounter date of a type that can credit carries its filing-date notice region too.
        var cut = RenderForm(
            SeedSchemas.EveryFieldVisible(SeedSchemas.Schema(seedKey)),
            SeedSchemas.CreditRules(seedKey),
            FiledOn);

        foreach (var field in SeedSchemas.Fields(seedKey))
        {
            var described = DescribedElementOf(cut, field);
            var expected = new List<string>();
            if (!string.IsNullOrWhiteSpace(field.HelpText))
            {
                expected.Add(FieldHelp.Id(field.Key));
                cut.Find($"[id='{FieldHelp.Id(field.Key)}']").TextContent.Trim().Should().Be(
                    field.HelpText, "'{0}.{1}' renders its help under the id its control names", seedKey, field.Key);
            }
            else
            {
                cut.FindAll($"[id='{FieldHelp.Id(field.Key)}']").Should().BeEmpty(
                    "'{0}.{1}' has no help text to render", seedKey, field.Key);
            }

            if (cut.FindAll($"[id='{field.Key}-filing-notice']").Count > 0)
            {
                expected.Add($"{field.Key}-filing-notice");
            }

            DescribedBy(described).Should().Equal(
                expected,
                "'{0}.{1}' must name its help text, then any warning region, and nothing else", seedKey, field.Key);
        }

        IdReferences.Broken(cut).Should().BeEmpty("every description on the '{0}' form must name a real element", seedKey);
    }

    [Fact]
    public void TheEncounterDateOfAMiniCex_NamesItsHelp_ThenTheLateFilingWarning()
    {
        // The case the task was filed on: T160 gave the date field aria-describedby for its warning alone.
        var cut = RenderForm(
            SeedSchemas.EveryFieldVisible(SeedSchemas.Schema("mini_cex_cpsa")),
            SeedSchemas.CreditRules("mini_cex_cpsa"),
            FiledOn);

        var date = cut.Find("#observed_on-in");
        date.GetAttribute("aria-describedby").Should().Be("observed_on-help observed_on-filing-notice");
        cut.Find("#observed_on-help").TextContent.Should().Be("When the encounter happened, not when this form is completed.");

        cut.Find("#assessor_user_id-in").GetAttribute("aria-describedby").Should().Be("assessor_user_id-help");
        cut.Find("#assessor_user_id-help").TextContent.Should().Be("The supervisor who observed you. They complete this assessment.");

        cut.Find("#epa_id-in").HasAttribute("aria-describedby").Should().BeFalse("the EPA field has no help text and no warning");
    }

    [Fact]
    public void ALockedForm_HasNoControlsToDescribe_AndNamesNothingThatIsNotThere()
    {
        // T342 (flow 03): a reader who may write nothing is shown no controls. A section that holds values is read out,
        // each value under its label; one that holds nothing is a locked section. No help is left to describe a control,
        // and no description names an element that is not there.
        var cut = RenderComponent<ActivityForm>(parameters => parameters
            .Add(component => component.SchemaJson, SeedSchemas.Schema("mini_cex_cpsa"))
            .Add(component => component.DataJson, """{ "observed_on": "2026-09-09" }""")
            .Add(component => component.ReadOnly, true));

        cut.FindAll("input, select, textarea").Should().BeEmpty();
        cut.Find("#observed_on-in dt").TextContent.Trim().Should().Be("Date observed");
        cut.Find("#observed_on-in dd").TextContent.Trim().Should().Be("2026-09-09");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void AKeyWithASpace_StillNamesItsHelp_AndItsWarning()
    {
        // A builder's field key is free text, and aria-describedby is a space-separated list of ids: "seen on-help" would
        // be read as two ids that name nothing. The ids it names are built from the key with the space replaced.
        const string schema = """
            {
              "version": 1,
              "observation_date_field": "seen on",
              "sections": [
                {
                  "key": "s",
                  "title": "S",
                  "fields": [
                    { "key": "seen on", "type": "date", "label": "Seen on", "help_text": "When you saw the patient." },
                    { "key": "patient age", "type": "text", "label": "Patient age", "help_text": "In whole years." }
                  ]
                }
              ]
            }
            """;
        const string credit = """{ "counts_for": [ { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1 } ] }""";

        var cut = RenderForm(schema, credit, FiledOn);

        cut.Find("#seen-on-in").GetAttribute("aria-describedby").Should().Be("seen-on-help seen-on-filing-notice");
        cut.Find("#seen-on-help").TextContent.Trim().Should().Be("When you saw the patient.");
        cut.Find("#patient-age-in").GetAttribute("aria-describedby").Should().Be("patient-age-help");
        cut.Find("#patient-age-help").TextContent.Trim().Should().Be("In whole years.");
        IdReferences.Broken(cut).Should().BeEmpty();
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
    public void AFieldOfEveryType_WithHelpText_NamesIt(FieldType type)
    {
        // The seeds carry help on a handful of types; a builder-made form may put it on any.
        var field = OneField(type, "Help for the field");

        var cut = RenderForm(OneFieldSchema(field));

        DescribedBy(DescribedElementOf(cut, field)).Should().Equal(["f-help"], "a {0} field names its help text", type);
        cut.Find("#f-help").TextContent.Trim().Should().Be("Help for the field");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(FieldTypes))]
    public void AFieldOfEveryType_WithNoHelpText_DescribesNothing(FieldType type)
    {
        var field = OneField(type, helpText: null);

        var cut = RenderForm(OneFieldSchema(field));

        DescribedElementOf(cut, field).HasAttribute("aria-describedby").Should().BeFalse(
            "a {0} field with no help text must not name an element that is not there", type);
        cut.FindAll("#f-help").Should().BeEmpty();
    }

    // ---- helpers ----

    private IRenderedComponent<ActivityForm> RenderForm(string schemaJson, string? creditRulesJson = null, DateOnly? filedOn = null)
        => RenderComponent<ActivityForm>(parameters => parameters
            .Add(component => component.SchemaJson, schemaJson)
            .Add(component => component.DataJson, "{}")
            .Add(component => component.CreditRulesJson, creditRulesJson)
            .Add(component => component.FiledOn, filedOn));

    /// <summary>
    /// The element a field's description belongs on: its one control, whose id is the field's key, or else the fieldset
    /// of a field rendered as a group (T177), found by its legend.
    /// </summary>
    private static IElement DescribedElementOf(IRenderedFragment cut, SchemaField field)
    {
        var control = cut.FindAll($"[id='{ActivityFieldIds.Input(field.Key)}']").SingleOrDefault();
        if (control is not null)
        {
            return control;
        }

        var groups = cut.FindAll("section fieldset.form-group")
            .Where(group => group.QuerySelector("legend")!.TextContent.Contains(field.Label, StringComparison.Ordinal))
            .ToList();
        groups.Should().ContainSingle("field '{0}' is one control keyed by its key or one group named by its label", field.Key);
        return groups[0];
    }

    private static IReadOnlyList<string> DescribedBy(IElement element)
        => (element.GetAttribute("aria-describedby") ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

    private static SchemaField OneField(FieldType type, string? helpText)
    {
        IReadOnlyList<FieldOption> options = type is FieldType.Choice or FieldType.MultiChoice
            ? [FieldOption.Unlabelled("a"), FieldOption.Unlabelled("b")]
            : [];
        return new SchemaField("f", type, "The field", helpText, false, options, null, null, null, null, null, null);
    }

    private static string OneFieldSchema(SchemaField field)
        => FormSchemaParser.Serialize(new FormSchema(1, [new FormSection("s", "S", null, [field], null)]));
}
