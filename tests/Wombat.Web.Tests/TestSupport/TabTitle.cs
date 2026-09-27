using Bunit;
using Microsoft.AspNetCore.Components.Web;

namespace Wombat.Web.Tests.TestSupport;

/// <summary>
/// What a rendered page's <c>&lt;PageTitle&gt;</c> puts in the browser's tab, as it reads now (T190).
/// </summary>
/// <remarks>
/// A <c>PageTitle</c> hands its content to the <c>HeadOutlet</c> in <c>App.razor</c>, which a bUnit render does not have,
/// so its words are never in <c>cut.Markup</c>. This renders that content on its own. It reads the page's fields as they
/// are at the call, so a test can read the tab before and after a load: the builder's tab was empty once its editor had
/// loaded (T190's note), which a check of the first render would have passed.
/// </remarks>
public static class TabTitle
{
    public static string Of(TestContext context, IRenderedFragment page)
    {
        var content = page.FindComponent<PageTitle>().Instance.ChildContent
            ?? throw new InvalidOperationException("The page's PageTitle has no content.");

        return string.Concat(context.Render(content).Nodes.Select(node => node.TextContent));
    }
}
