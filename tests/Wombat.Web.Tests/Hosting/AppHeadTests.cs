using System.Text.Json;
using AngleSharp.Dom;
using FluentAssertions;

namespace Wombat.Web.Tests.Hosting;

/// <summary>
/// The tab title follows the page, on a full load and after in-app navigation (T180).
/// </summary>
/// <remarks>
/// Every page declares a <c>&lt;PageTitle&gt;</c>, which fills the <c>&lt;HeadOutlet&gt;</c> in <c>App.razor</c>. The
/// router runs in the interactive server circuit, so after the first load every page renders there. Before T180 the
/// head outlet was static: it was rendered once, on the full load, and nothing in the circuit could reach it, so
/// <c>document.title</c> stayed on the first page's title ("Dashboard — Wombat") however far the user moved. As an
/// interactive root of its own, in the same circuit as the router, the outlet re-renders when the page's title
/// changes.
/// </remarks>
public sealed class AppHeadTests
{
    [Fact]
    public async Task AFullLoad_PrerendersThePagesTitleInTheHead()
    {
        await using var host = await AppTestHost.StartAsync();
        var (_, _, document) = await host.LoadAsync(AppTestHost.AnonymousPage);

        document.Head!.QuerySelectorAll("title").Should().ContainSingle()
            .Which.TextContent.Should().Be("Reset password · Wombat", "the page's PageTitle reaches the head before any script runs");
    }

    /// <summary>
    /// A browser test would click between pages; this asserts the wiring that makes that work. The outlet's prerendered
    /// title sits inside an interactive server component's markers, which is how the circuit takes the outlet over and
    /// re-renders it when a page reached in the circuit sets its title.
    /// </summary>
    /// <remarks>
    /// Signed in: only a signed-in user has a circuit, so only their pages are interactive. A visitor who has not signed
    /// in gets static pages, whose head is rendered whole on every load (T181; <see cref="BlazorEndpointAccessTests" />).
    /// </remarks>
    [Fact]
    public async Task TheHeadOutlet_IsAnInteractiveServerRoot_SoTheTitleCanFollowInAppNavigation()
    {
        await using var host = await AppTestHost.StartAsync(SignedInVisitor.Register);
        var (_, _, document) = await host.LoadAsync(AppTestHost.AnonymousPage);

        var head = document.Head!.ChildNodes.ToList();
        var start = head.FindIndex(node => node is IComment comment && ServerComponentStart(comment.Data) is not null);
        start.Should().BeGreaterThanOrEqualTo(0, "the head outlet is rendered as an interactive server component");

        var prerenderId = ServerComponentStart(((IComment)head[start]).Data)!;
        var end = head.FindIndex(start + 1, node => node is IComment comment && comment.Data == $"Blazor:{{\"prerenderId\":\"{prerenderId}\"}}");
        var title = head.FindIndex(node => node is IElement { LocalName: "title" });

        end.Should().BeGreaterThan(start, "the component's prerendered output is closed by its end marker");
        title.Should().BeInRange(start + 1, end - 1, "the title is that component's output, so the circuit owns it");
    }

    /// <summary>The prerender id of an interactive server component's start marker, or null for any other comment.</summary>
    private static string? ServerComponentStart(string data)
    {
        const string prefix = "Blazor:";
        if (!data.StartsWith(prefix, StringComparison.Ordinal))
        {
            return null;
        }

        using var marker = JsonDocument.Parse(data[prefix.Length..]);
        return marker.RootElement.TryGetProperty("type", out var type) && type.GetString() == "server"
            && marker.RootElement.TryGetProperty("prerenderId", out var id)
                ? id.GetString()
                : null;
    }
}
