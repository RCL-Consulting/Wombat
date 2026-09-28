using System.Text.RegularExpressions;

namespace Wombat.Web.Tests.Hosting;

/// <summary>
/// The comment Blazor writes where an interactive server root starts (<c>&lt;!--Blazor:{… "type":"server" …</c>): its
/// presence in a full load's HTML is what makes <c>blazor.web.js</c> open a circuit. A static page has none. One pattern,
/// shared by the hosting tests in Web.Tests that ask whether a page is static (T339, flow 02; four files had a copy each).
/// The integration suite cannot see this internal type, so <c>SessionRevalidationFlowTests</c> there keeps its own copy.
/// </summary>
internal static partial class ServerComponentMarker
{
    /// <summary>Whether <paramref name="html" /> holds an interactive server root.</summary>
    public static bool IsIn(string html) => Pattern().IsMatch(html);

    /// <summary>How many interactive server roots <paramref name="html" /> holds.</summary>
    public static int CountIn(string html) => Pattern().Matches(html).Count;

    [GeneratedRegex("""<!--Blazor:\{[^>]*"type":"server""")]
    private static partial Regex Pattern();
}
