using System.Xml.Linq;
using FluentAssertions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Activities;
using Wombat.Infrastructure.Reporting;

namespace Wombat.Infrastructure.Tests.Reporting;

/// <summary>
/// T161, D28, T197: the portfolio PDF prints an undated activity's date as not recorded, with the day it was created, in
/// the wording the lists use, and names it as the encounter date, so it cannot be read as the activity not being recorded.
/// </summary>
/// <remarks>
/// The section is rendered through QuestPDF's SVG output, which keeps each run of text as a <c>&lt;text&gt;</c> element,
/// so these read what a reader of the page would read rather than what the component was asked to print. One element
/// is one line: the qualified date must not wrap, which the fixed 100pt the bare date sat in would have made it do.
/// </remarks>
[Collection(QuestPdfRenderingCollection.Name)]
public sealed class ActivitiesSectionEncounterDateTests
{
    private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";

    static ActivitiesSectionEncounterDateTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    [Fact]
    public void AnActivityWithNoStatedEncounterDate_IsPrintedAsNotRecorded_WithTheDayItWasCreated()
    {
        // A reflective note: its type declares no date field, so ObservationDateResolver fell back to the day it was created.
        var lines = RenderedLines(Reflection(ObservationDateSource.CreatedOn));

        lines.Should().Contain("Encounter date: not recorded (created 2026-03-20)");
    }

    [Fact]
    public void AnActivityWithAStatedEncounterDate_IsPrintedAsTheEncounterDate_Unqualified()
    {
        var lines = RenderedLines(Reflection(ObservationDateSource.Declared));

        lines.Should().Contain("Encounter date: 2026-03-20");
        lines.Should().NotContain(line => line.Contains("not recorded"));
    }

    private static Activity Reflection(ObservationDateSource source) => new()
    {
        Id = 5,
        ActivityTypeId = 1,
        SchemaVersion = 1,
        SubjectUserId = "trainee-1",
        CreatedByUserId = "trainee-1",
        CurrentState = "completed",
        DataJson = "{}",
        CreatedOn = new DateTime(2026, 3, 20, 8, 0, 0, DateTimeKind.Utc),
        UpdatedOn = new DateTime(2026, 3, 20, 8, 0, 0, DateTimeKind.Utc),
        ObservedOn = new DateOnly(2026, 3, 20),
        ObservedOnSource = source
    };

    /// <summary>Every line of text the activities section draws, in page order.</summary>
    private static IReadOnlyList<string> RenderedLines(Activity activity)
    {
        var pages = Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Content().Element(content => ActivitiesSectionComponent.Compose(
                content,
                new Dictionary<string, List<Activity>> { ["Reflective note"] = [activity] },
                [],
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
