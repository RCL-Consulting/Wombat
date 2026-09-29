using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities.Schema;
using Wombat.Web.Components.Shared.Activities;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T191: an activity form lists every option by its label and never by the key it stores. The CCA listed
/// "admission_notes" under "Documentation reviewed" because the form printed the option itself; the option now carries
/// words of its own, and the form posts the key and shows the words. Every seeded form is rendered and read.
/// </summary>
public sealed partial class ActivityFormOptionLabelTests : TestContext
{
    [GeneratedRegex("^[a-z][a-z0-9]*(?:_[a-z0-9]+)*$")]
    private static partial Regex SnakeCaseKey();

    public ActivityFormOptionLabelTests()
    {
        this.AddTestAuthorization().SetAuthorized("trainee@test");
        Services.AddSingleton<IActivityReferenceDataService, StubActivityReferenceDataService>();
    }

    public static TheoryData<string> SeedKeys
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var seedKey in SeedKeysWithASchema())
            {
                data.Add(seedKey);
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(SeedKeys))]
    public void ASeededForm_ShowsNoOptionAsASnakeCaseKey(string seedKey)
    {
        var cut = RenderForm(EveryFieldVisible(ReadSeedSchema(seedKey)));

        var shown = ShownOptions(cut);

        // Not vacuous: the form lists exactly the options the seed declares on the fields it renders them for (the
        // reference lists are stubbed empty, so an EPA, person or catalogue picker lists none).
        var declared = FormSchemaParser.Parse(ReadSeedSchema(seedKey)).Sections
            .SelectMany(section => section.Fields)
            .Where(field => field.Type is FieldType.Choice or FieldType.MultiChoice or FieldType.Scale)
            .Sum(field => field.Options.Count);
        shown.Should().HaveCount(declared, "'{0}' must list every option it declares", seedKey);

        shown.Where(text => SnakeCaseKey().IsMatch(text))
            .Should().BeEmpty("'{0}' must list its options in words, never as the keys they store", seedKey);
    }

    [Fact]
    public void TheCca_ListsItsDocumentsAndSettingsInWords_AndStillPostsTheKeys()
    {
        // The form the defect was found on (the T177 browser check): what a reader sees, and what the controls send.
        var cut = RenderForm(ReadSeedSchema("cca_cpsa"));

        var documents = cut.FindAll("section fieldset .check-grid .form-check");
        documents.Select(check => check.QuerySelector("label")!.TextContent.Trim())
            .Should().Equal("Admission notes", "Progress notes", "Referral letters", "Discharge summary", "Prescriptions");
        documents.Select(check => check.QuerySelector("input")!.Id)
            .Should().Equal(
                "documents_reviewed-admission_notes", "documents_reviewed-progress_notes",
                "documents_reviewed-referral_letters", "documents_reviewed-discharge_summary",
                "documents_reviewed-prescriptions");

        var settings = cut.FindAll("#setting-in option").Where(option => option.GetAttribute("value") != string.Empty).ToList();
        settings.Select(option => option.GetAttribute("value"))
            .Should().Equal("ward", "outpatient_clinic", "emergency_unit", "neonatal_unit", "picu", "community");
        settings.Select(option => option.TextContent.Trim())
            .Should().Equal("Ward", "Outpatient clinic", "Emergency unit", "Neonatal unit", "PICU", "Community");
    }

    [Fact]
    public void AStoredCca_ReadsItsStoredKeysAsTheirLabels()
    {
        // An activity filed before the labels, or after: it stores keys either way, and the detail page shows words.
        var cut = RenderComponent<ActivityForm>(parameters => parameters
            .Add(component => component.SchemaJson, ReadSeedSchema("cca_cpsa"))
            .Add(component => component.DataJson, """{ "setting": "picu", "documents_reviewed": ["admission_notes", "prescriptions"] }""")
            .Add(component => component.ReadOnly, true));

        // Read out since T342 (flow 03): a reader who may write nothing is shown the values as text, in words.
        cut.Find("#setting-in dd").TextContent.Trim().Should().Be("PICU");
        cut.Find("#documents_reviewed-in dd").TextContent.Trim().Should().Be("Admission notes, Prescriptions");
    }

    /// <summary>
    /// The words the form lists as options: every <c>&lt;option&gt;</c> but the empty "Select…" prompt, and every checkbox
    /// label of a multi-choice group.
    /// </summary>
    private static IReadOnlyList<string> ShownOptions(IRenderedFragment cut)
        => cut.FindAll("option")
            .Where(option => !string.IsNullOrEmpty(option.GetAttribute("value")))
            .Select(option => option.TextContent.Trim())
            .Concat(cut.FindAll(".check-grid .form-check label").Select(label => label.TextContent.Trim()))
            .ToList();

    private IRenderedComponent<ActivityForm> RenderForm(string schemaJson)
        => RenderComponent<ActivityForm>(parameters => parameters
            .Add(component => component.SchemaJson, schemaJson)
            .Add(component => component.DataJson, "{}"));

    /// <summary>The schema with every <c>show_if</c> removed, so a field a condition hides is read too.</summary>
    private static string EveryFieldVisible(string schemaJson)
    {
        var root = JsonNode.Parse(schemaJson)!.AsObject();
        foreach (var section in root["sections"]!.AsArray().Select(node => node!.AsObject()))
        {
            section.Remove("show_if");
            foreach (var field in section["fields"]!.AsArray().Select(node => node!.AsObject()))
            {
                field.Remove("show_if");
            }
        }

        return root.ToJsonString();
    }

    private static IEnumerable<string> SeedKeysWithASchema()
        => Directory.EnumerateDirectories(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds"))
            .Select(directory => Path.GetFileName(directory)!)
            .Where(seedKey => File.Exists(SeedSchemaPath(seedKey)))
            .Order(StringComparer.Ordinal)
            .ToList();

    private static string ReadSeedSchema(string seedKey)
        => File.ReadAllText(SeedSchemaPath(seedKey));

    private static string SeedSchemaPath(string seedKey)
        => Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", seedKey, "schema.json");
}
