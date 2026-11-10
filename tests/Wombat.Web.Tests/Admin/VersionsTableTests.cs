using AngleSharp.Dom;
using Bunit;
using FluentAssertions;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Web.Components.Pages.Admin.ActivityTypes;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T398 (flow 17, lane B, Q3, T16): "Published versions" as R2-B03, R2-E03, R2-E06 and R3-E01 draw it, from version DTOs
/// built by hand: the heading at the level the builder asks, each version by number, its South African day and its
/// publisher by name ("Wombat's catalogue" for the seeder), "Not published yet." for none, and Q3's line.
/// </summary>
public sealed class VersionsTableTests : TestContext
{
    [Fact]
    public void OneVersion_ReadsItsNumber_ItsDay_AndItsPublisher()
    {
        var cut = Render([Version(1, new DateTime(2026, 11, 4, 22, 30, 0, DateTimeKind.Utc), "Wombat's catalogue")], headingLevel: 2);

        cut.Find("div.versions-readout").Should().NotBeNull();
        Text(cut.Find("div.versions-readout > h2")).Should().Be("Versions");
        Text(cut.Find("caption")).Should().Be("Published versions");
        cut.FindAll("thead th").Select(Text).Should().Equal("Version", "Published", "By");
        cut.Find("table").ClassList.Should().Contain("clinic-table--stack-wide");
        Text(cut.Find("tbody th[scope=row]")).Should().Be("1");
        Text(cut.Find("tbody td[data-label=Published]")).Should().Be("2026-11-05", "22:30 UTC is already the 5th in South Africa");
        Text(cut.Find("tbody td[data-label=By]")).Should().Be("Wombat's catalogue");
        var line = cut.Find("div.versions-readout > p.muted");
        Text(line).Should().Be("Activities filed under a version stay on it.");
        cut.Markup.Should().NotContain("seed-system");
    }

    [Fact]
    public void TwoVersions_ReadInOrder_UnderAnH3_OnThePublishTab()
    {
        var cut = Render(
        [
            Version(2, new DateTime(2026, 11, 9, 8, 0, 0, DateTimeKind.Utc), "Nolwazi Mbatha"),
            Version(1, new DateTime(2026, 11, 5, 8, 0, 0, DateTimeKind.Utc), "Nolwazi Mbatha")
        ], headingLevel: 3);

        Text(cut.Find("div.versions-readout > h3")).Should().Be("Versions");
        cut.FindAll("tbody th[scope=row]").Select(Text).Should().Equal("1", "2");
    }

    [Fact]
    public void NoVersion_ReadsNotPublishedYet()
    {
        var cut = Render([], headingLevel: 3);

        cut.FindAll("table").Should().BeEmpty();
        cut.FindAll("div.versions-readout > p").Select(Text).Should().Equal(
            "Not published yet.", "Activities filed under a version stay on it.");
    }

    [Fact]
    public void APublisherNoOneNamed_IsNeverShownById()
    {
        var cut = Render([new ActivityTypeVersionDto(1, new DateTime(2026, 11, 5, 8, 0, 0, DateTimeKind.Utc), "user-guid-123")], headingLevel: 3);

        cut.Markup.Should().NotContain("user-guid-123");
        cut.FindAll("thead th").Select(Text).Should().Equal("Version", "Published");
    }

    private IRenderedComponent<VersionsTable> Render(IReadOnlyList<ActivityTypeVersionDto> versions, int headingLevel)
        => RenderComponent<VersionsTable>(parameters => parameters
            .Add(component => component.Versions, versions)
            .Add(component => component.HeadingLevel, headingLevel));

    private static ActivityTypeVersionDto Version(int number, DateTime publishedOnUtc, string publishedBy)
        => new(number, publishedOnUtc, "user-" + number) { PublishedByName = publishedBy };

    private static string Text(IElement element) => element.TextContent.Trim();
}
