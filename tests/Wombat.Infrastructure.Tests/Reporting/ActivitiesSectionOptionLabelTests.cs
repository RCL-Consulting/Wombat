using System.Text.Json;
using System.Xml.Linq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Epas;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Reporting;

namespace Wombat.Infrastructure.Tests.Reporting;

/// <summary>
/// T191: the portfolio export prints a chosen option by its label, as the form shows it. Printing the stored key would
/// leave the export the one place a committee reads <c>admission_notes</c>.
/// </summary>
/// <remarks>Rendered through QuestPDF's SVG output, as <see cref="ActivitiesSectionEncounterDateTests" /> reads it.</remarks>
[Collection(QuestPdfRenderingCollection.Name)]
public sealed class ActivitiesSectionOptionLabelTests
{
    private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";

    static ActivitiesSectionOptionLabelTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    [Fact]
    public void ACca_IsPrintedWithItsDocumentsAndSettingInWords()
    {
        var text = string.Join(" ", RenderedLines("""
            { "case_reference": "Bed 4, 12 March", "setting": "picu",
              "documents_reviewed": ["admission_notes", "discharge_summary"] }
            """));

        text.Should().Contain("Admission notes, Discharge summary");
        text.Should().Contain("PICU");
        text.Should().NotContain("admission_notes").And.NotContain("discharge_summary").And.NotContain("picu");
    }

    [Fact]
    public void AStoredValue_IsPrintedAsItsLabel_OrAsStoredWhenNoOptionDeclaresIt()
    {
        var field = FormSchemaParser.Parse(CcaSchema).Sections
            .SelectMany(section => section.Fields)
            .Single(candidate => candidate.Key == "documents_reviewed");

        Text(field, """{ "documents_reviewed": ["admission_notes", "retired_key"] }""")
            .Should().Be("Admission notes, retired_key", "a value the pinned version never offered prints as stored");
        Text(field, """{ "documents_reviewed": "prescriptions" }""").Should().Be("Prescriptions");
        Text(field, "{}").Should().BeNull();
    }

    [Fact]
    public void AFieldWithNoOptions_IsPrintedAsStored()
    {
        var field = FormSchemaParser.Parse(CcaSchema).Sections
            .SelectMany(section => section.Fields)
            .Single(candidate => candidate.Key == "case_reference");

        Text(field, """{ "case_reference": "ward_round" }""").Should().Be("ward_round", "free text is the author's words, never looked up");
    }

    [Fact]
    public async Task AScaleOption_PrintsTheRung_WhetherStoredAsTextOrAsANumber()
    {
        // T100 before T191: a scale's ordinal prints as the rung the College prints, even where the option carries a
        // label of its own. The form stores "1" and other writers store 1; both are the same rating and print alike.
        var field = LabelledScaleField();
        var rungs = await OrScaleRungsAsync();

        Text(field, """{ "overall": "1" }""", rungs).Should().Be("Observe only");
        Text(field, """{ "overall": 1 }""", rungs).Should().Be("Observe only");
        Text(field, """{ "overall": "2" }""", rungs).Should().Be("Direct supervision");
    }

    [Fact]
    public void AScaleOption_PrintsItsLabel_OnlyWhenNoRungResolves_ThenTheOrdinal()
    {
        var field = LabelledScaleField();

        Text(field, """{ "overall": "1" }""", EntrustmentRungLookup.Empty).Should().Be("First point");
        Text(field, """{ "overall": 1 }""", EntrustmentRungLookup.Empty).Should().Be("First point");
        Text(field, """{ "overall": 2 }""", EntrustmentRungLookup.Empty).Should().Be("2", "an unlabelled point is its own words");
        Text(field, """{ "overall": "7" }""", EntrustmentRungLookup.Empty).Should().Be("7", "a point no option declares prints as stored");
    }

    private static FormField LabelledScaleField()
        => FormSchemaParser.Parse("""
            {
              "version": 1,
              "sections": [
                {
                  "key": "rating", "title": "Rating",
                  "fields": [
                    {
                      "key": "overall", "type": "scale", "label": "Overall", "scale_key": "O-R Scale",
                      "options": [ { "value": "1", "label": "First point" }, "2", "3", "4", "5" ]
                    }
                  ]
                }
              ]
            }
            """).Sections.Single().Fields.Single();

    /// <summary>The seeded O-R Scale's five rungs, resolved by name as a schema's <c>scale_key</c> names them.</summary>
    private static async Task<EntrustmentRungLookup> OrScaleRungsAsync()
    {
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        db.Set<EntrustmentScale>().Add(new EntrustmentScale { Id = 1, Name = "O-R Scale" });
        string[] labels = ["Observe only", "Direct supervision", "Indirect supervision", "Independent", "Supervises others"];
        for (var order = 1; order <= labels.Length; order++)
        {
            db.Set<EntrustmentLevel>().Add(new EntrustmentLevel { Id = 100 + order, ScaleId = 1, Order = order, Label = labels[order - 1] });
        }

        await db.SaveChangesAsync();
        return await EntrustmentRungLabels.LoadForScaleKeysAsync(db, ["O-R Scale"], CancellationToken.None);
    }

    private static string? Text(FormField field, string dataJson, EntrustmentRungLookup? rungs = null)
    {
        using var document = JsonDocument.Parse(dataJson);
        return ActivitiesSectionComponent.FieldValueText(
            field, document.RootElement, rungs ?? EntrustmentRungLookup.Empty, PortfolioFieldReferences.Empty, activityTypeKey: null);
    }

    private static string CcaSchema
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", "cca_cpsa", "schema.json"));

    /// <summary>Every line of text the activities section draws for one CCA pinned to the seeded schema.</summary>
    private static IReadOnlyList<string> RenderedLines(string dataJson)
    {
        var activity = new Activity
        {
            Id = 7,
            ActivityTypeId = 1,
            SchemaVersion = 1,
            SubjectUserId = "trainee-1",
            CreatedByUserId = "trainee-1",
            CurrentState = "completed",
            DataJson = dataJson,
            CreatedOn = new DateTime(2026, 3, 20, 8, 0, 0, DateTimeKind.Utc),
            UpdatedOn = new DateTime(2026, 3, 20, 8, 0, 0, DateTimeKind.Utc),
            ObservedOn = new DateOnly(2026, 3, 20),
            ObservedOnSource = ObservationDateSource.Declared
        };
        var version = new ActivityTypeVersion { ActivityTypeId = 1, Version = 1, SchemaJson = CcaSchema };

        var pages = Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Content().Element(content => ActivitiesSectionComponent.Compose(
                content,
                new Dictionary<string, List<Activity>> { ["Case analysis"] = [activity] },
                new Dictionary<(int ActivityTypeId, int Version), ActivityTypeVersion> { [(1, 1)] = version },
                new Dictionary<(int ActivityTypeId, int Version), Wombat.Domain.Activities.Workflow.Workflow?>(),
                EntrustmentRungLookup.Empty,
                PortfolioFieldReferences.Empty));
        })).GenerateSvg();

        return pages
            .SelectMany(page => XDocument.Parse(page).Descendants(Svg + "text"))
            .Select(text => text.Value.Trim())
            .ToList();
    }
}
