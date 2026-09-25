using System.Xml.Linq;
using FluentAssertions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;
using Wombat.Infrastructure.Reporting;

namespace Wombat.Infrastructure.Tests.Reporting;

/// <summary>
/// T220: the portfolio PDF's activities section names each activity's state by its pinned workflow's label, as the
/// activity's page does, and by its key only where that workflow cannot name it.
/// </summary>
[Collection(QuestPdfRenderingCollection.Name)]
public sealed class ActivitiesSectionStateLabelTests
{
    private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";

    private static readonly Workflow ClinicalAudit = WorkflowParser.Parse(File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", "clinical_audit_cpsa", "workflow.json")));

    static ActivitiesSectionStateLabelTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    [Fact]
    public void AnActivitysState_IsPrintedByItsPinnedWorkflowsLabel()
    {
        var lines = RenderedLines(Audit("submitted"), new() { [(1, 1)] = ClinicalAudit });

        lines.Should().Contain(line => line.Contains("State: Awaiting supervisor"));
        lines.Should().NotContain(line => line.Contains("State: submitted"));
    }

    [Fact]
    public void AStateThePinnedWorkflowDoesNotDeclare_IsPrintedByItsKey()
    {
        var lines = RenderedLines(Audit("archived"), new() { [(1, 1)] = ClinicalAudit });

        lines.Should().Contain(line => line.Contains("State: archived"));
    }

    [Fact]
    public void AnActivityWithNoWorkflowThatParses_IsPrintedByItsKey()
    {
        var lines = RenderedLines(Audit("submitted"), new() { [(1, 1)] = null });

        lines.Should().Contain(line => line.Contains("State: submitted"));
    }

    private static Activity Audit(string state) => new()
    {
        Id = 5,
        ActivityTypeId = 1,
        SchemaVersion = 1,
        SubjectUserId = "trainee-1",
        CreatedByUserId = "trainee-1",
        CurrentState = state,
        DataJson = "{}",
        CreatedOn = new DateTime(2026, 3, 20, 8, 0, 0, DateTimeKind.Utc),
        UpdatedOn = new DateTime(2026, 3, 20, 8, 0, 0, DateTimeKind.Utc),
        ObservedOn = new DateOnly(2026, 3, 20),
        ObservedOnSource = ObservationDateSource.Declared
    };

    /// <summary>Every line of text the activities section draws, in page order.</summary>
    private static IReadOnlyList<string> RenderedLines(
        Activity activity,
        Dictionary<(int ActivityTypeId, int Version), Workflow?> workflows)
    {
        var pages = Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Content().Element(content => ActivitiesSectionComponent.Compose(
                content,
                new Dictionary<string, List<Activity>> { ["Clinical audit"] = [activity] },
                [],
                workflows,
                EntrustmentRungLookup.Empty,
                PortfolioFieldReferences.Empty));
        })).GenerateSvg();

        return pages
            .SelectMany(page => XDocument.Parse(page).Descendants(Svg + "text"))
            .Select(text => text.Value.Trim())
            .ToList();
    }
}
