using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.Design;

public sealed class PageShapeSmokeTests : TestContext
{
    [Fact]
    public void ListShape_RendersHeaderAndTable()
    {
        var cut = Render(builder =>
        {
            builder.OpenComponent<PageHeader>(0);
            builder.AddAttribute(1, "Title", "List");
            builder.CloseComponent();

            builder.OpenComponent<DataTable<string>>(2);
            builder.AddAttribute(3, "Items", new[] { "One" });
            builder.AddAttribute(4, "HeaderRow", (RenderFragment)(header =>
            {
                header.OpenElement(0, "tr");
                header.OpenElement(1, "th");
                header.AddContent(2, "Name");
                header.CloseElement();
                header.CloseElement();
            }));
            builder.AddAttribute(5, "Row", (RenderFragment<string>)(item => row =>
            {
                row.OpenElement(0, "tr");
                row.OpenElement(1, "td");
                row.AddContent(2, item);
                row.CloseElement();
                row.CloseElement();
            }));
            builder.CloseComponent();
        });

        cut.Find(".header-container").Should().NotBeNull();
        cut.Find(".clinic-table").Should().NotBeNull();
    }

    [Fact]
    public void DetailShape_RendersDetailsGrid()
    {
        var cut = Render(builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "details-grid");
            builder.OpenElement(2, "aside");
            builder.AddAttribute(3, "class", "detail-card");
            builder.CloseElement();
            builder.OpenElement(4, "section");
            builder.AddAttribute(5, "class", "detail-card");
            builder.CloseElement();
            builder.CloseElement();
        });

        cut.Find(".details-grid").Children.Length.Should().Be(2);
    }

    [Fact]
    public void FormShape_RendersFormContainer()
    {
        var cut = Render(builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "form-container");
            builder.OpenElement(2, "div");
            builder.AddAttribute(3, "class", "form-grid");
            builder.CloseElement();
            builder.OpenElement(4, "div");
            builder.AddAttribute(5, "class", "form-actions");
            builder.CloseElement();
            builder.CloseElement();
        });

        cut.Find(".form-container").Should().NotBeNull();
        cut.Find(".form-actions").Should().NotBeNull();
    }

    [Fact]
    public void DashboardShape_RendersDashboardGrid()
    {
        var cut = Render(builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "dashboard-grid");
            builder.OpenElement(2, "div");
            builder.AddAttribute(3, "class", "detail-card");
            builder.CloseElement();
            builder.CloseElement();
        });

        cut.Find(".dashboard-grid .detail-card").Should().NotBeNull();
    }

    [Fact]
    public void NoPage_SaysLoadingAsText()
    {
        // "No more 'Loading…' plain text — that pattern is dead" (DESIGN.md § Alerts, validation, empty states): a list or
        // detail page's loading state is StatePanel's skeletons. The entrustment decisions page and an audit entry said
        // "Loading…" until T266, and a failed read left it there under the error.
        var web = Path.Combine(SolutionRoot(), "src", "Wombat.Web");
        var pages = Directory.EnumerateFiles(web, "*.razor", SearchOption.AllDirectories).ToList();

        pages.Should().HaveCountGreaterThanOrEqualTo(100, "guard: the scan finds the pages");
        pages.Where(path => System.Text.RegularExpressions.Regex.IsMatch(File.ReadAllText(path), @">\s*Loading(…|\.\.\.)\s*<"))
            .Select(path => Path.GetRelativePath(web, path))
            .Should().BeEmpty("a page's loading state is StatePanel's");
    }

    private static string SolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && directory.GetFiles("Wombat.sln").Length == 0)
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not find Wombat.sln.");
    }
}
