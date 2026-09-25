using System.Text.RegularExpressions;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Services;
using DecisionsPage = Wombat.Web.Components.Pages.Admin.EntrustmentDecisions.Index;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T226: the entrustment decisions list is a list page (DESIGN.md § Table system). Its table was <c>class="data-table"</c>,
/// which app.css does not define, so it rendered unstyled, and at 390px it was 133px wider than the screen. Its filter
/// touched the table, its dates broke at a hyphen, and its status badges named classes app.css does not define (T226
/// review).
/// </summary>
public sealed partial class EntrustmentDecisionsListTableTests : TestContext
{
    public EntrustmentDecisionsListTableTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("instadmin@test");
        auth.SetRoles(WombatRoles.InstitutionalAdmin);
        Services.AddSingleton<IScopedSender>(new FakeSender(
        [
            Decision(1, "PAED-001", EntrustmentDecisionStatus.Active),
            Decision(2, "PAED-004", EntrustmentDecisionStatus.Revoked),
            Decision(3, "PAED-005", EntrustmentDecisionStatus.Expired),
            Decision(4, "PAED-006", EntrustmentDecisionStatus.Superseded)
        ]));
    }

    [Fact]
    public void TheDecisions_AreAClinicTable_InItsContainer()
    {
        var cut = RenderComponent<DecisionsPage>();
        cut.WaitForState(() => cut.Markup.Contains("PAED-004"));

        var table = cut.Find("table");
        table.ClassList.Should().Contain("clinic-table").And.NotContain("data-table");
        table.ParentElement!.ClassList.Should().Contain("table-container",
            "the container scrolls a table wider than the screen, rather than the page");
        table.Closest("section.detail-card").Should().BeNull("a list table sits in its own container, not in a card");
        table.QuerySelectorAll("tbody tr").Should().HaveCount(4);
    }

    [Fact]
    public void TheFilter_IsTheListPagesSearchContainer_WhoseMarginKeepsItOffTheTable()
    {
        // It was a detail-card, which has no margin: the table's top border met the filter's bottom one.
        var cut = RenderComponent<DecisionsPage>();
        cut.WaitForState(() => cut.Markup.Contains("PAED-004"));

        var filter = cut.Find("#filter-trainee").Closest("section")!;
        filter.ClassList.Should().Contain("search-container").And.NotContain("detail-card");
        filter.QuerySelector(".search-grid").Should().NotBeNull();
        Declarations(".search-container").Should().Contain(declaration => declaration.StartsWith("margin-bottom:", StringComparison.Ordinal));
    }

    [Fact]
    public void TheDates_AreColumnsAsNarrowAsTheirContent_OnOneLine()
    {
        // At 1280px "2026-07-02" broke at its hyphen, "2026-" over "07-02".
        var cut = RenderComponent<DecisionsPage>();
        cut.WaitForState(() => cut.Markup.Contains("PAED-004"));

        var headers = cut.FindAll("thead th").ToArray();
        foreach (var column in new[] { "Issued", "Expires" })
        {
            var index = Array.FindIndex(headers, header => header.TextContent.Trim() == column);
            headers[index].ClassList.Should().Contain("col-fit");
            cut.FindAll("tbody tr").Should().OnlyContain(row => row.Children[index].ClassList.Contains("col-fit"));
        }
    }

    [Fact]
    public void EveryStatus_WearsABadgeAppCssDefines()
    {
        // It was badge-{status}: badge-active, badge-revoked, badge-expired and badge-superseded, none of them in app.css,
        // so every status was an untinted pill. The word says the status; the tint repeats it (DESIGN.md § Badges).
        var cut = RenderComponent<DecisionsPage>();
        cut.WaitForState(() => cut.Markup.Contains("PAED-004"));

        var badges = cut.FindAll("tbody .badge")
            .ToDictionary(badge => badge.TextContent.Trim(), badge => badge.ClassList.Single(name => name != "badge"));
        badges.Should().Equal(new Dictionary<string, string>
        {
            ["Active"] = "badge-completed",
            ["Revoked"] = "badge-declined",
            ["Expired"] = "badge-accepted",
            ["Superseded"] = "badge-draft"
        });
        badges.Values.Should().OnlyContain(name => Declarations($".{name}").Any(), "a class app.css does not define styles nothing");
    }

    [Fact]
    public void ARowsButtons_AreOneCluster_UnderAHeaderThatNamesThem()
    {
        var cut = RenderComponent<DecisionsPage>();
        cut.WaitForState(() => cut.Markup.Contains("PAED-004"));

        // The row actions' flex-gap cluster (DESIGN.md § Button system), inside the cell: a <td> that is itself a flex box
        // is no longer a table cell.
        var rows = cut.FindAll("tbody tr").ToArray();
        rows[0].QuerySelectorAll("td > .actions-cell > button").Select(button => button.TextContent.Trim())
            .Should().Equal("Download", "Revoke");
        rows.Skip(1).Should().OnlyContain(row => row.QuerySelectorAll("td > .actions-cell > button")
            .Select(button => button.TextContent.Trim()).SequenceEqual(new[] { "Download" }));

        // An actions column's header is a visually hidden "Actions", never an empty <th> (§ Table system).
        var header = cut.FindAll("thead th").Last();
        header.QuerySelector(".visually-hidden")!.TextContent.Should().Be("Actions");
    }

    private static EntrustmentDecisionDto Decision(int id, string code, EntrustmentDecisionStatus status)
        => new(
            id, $"trainee-{id}", id, code, $"EPA {code}", true, 13, "3a", 3, new DateOnly(2026, 7, 2), null, 30, "chair-1",
            "Consistent across the window.", status, null, null, null, null, [])
        {
            TraineeName = $"Trainee {id}"
        };

    /// <summary>The declarations of app.css's top-level rule for exactly this selector, comments removed.</summary>
    private static IReadOnlyList<string> Declarations(string selector)
    {
        var css = Comment().Replace(File.ReadAllText(Path.Combine(SolutionRoot(), "src", "Wombat.Web", "wwwroot", "app.css")), string.Empty);
        return Rule().Matches(css)
            .Where(rule => rule.Groups["selector"].Value.Trim() == selector)
            .SelectMany(rule => rule.Groups["body"].Value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .ToList();
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

    [GeneratedRegex(@"(?<selector>[^{}]+)\{(?<body>[^{}]*)\}")]
    private static partial Regex Rule();

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex Comment();

    private sealed class FakeSender(IReadOnlyList<EntrustmentDecisionDto> decisions) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request is ListEntrustmentDecisionsForAdminQuery
                ? Task.FromResult((TResponse)(object)decisions)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
