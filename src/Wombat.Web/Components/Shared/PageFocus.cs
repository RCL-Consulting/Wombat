using Microsoft.JSInterop;

namespace Wombat.Web.Components.Shared;

/// <summary>
/// Moves the focus to an element the page has no <see cref="Microsoft.AspNetCore.Components.ElementReference" /> to:
/// wombat.js's <c>wombat.focusById</c> and <c>wombat.focusHeading</c> (T342, flow 03; the build review's A1, A3, A4). Each
/// makes an element that cannot take the focus of itself programmatically focusable first, as FocusOnNavigate does.
/// </summary>
public static class PageFocus
{
    /// <summary>The script function that focuses an element by its id.</summary>
    public const string FocusByIdIdentifier = "wombat.focusById";

    /// <summary>The script function that focuses the page's h1.</summary>
    public const string FocusHeadingIdentifier = "wombat.focusHeading";

    /// <summary>
    /// The element with this id takes the focus: a refusal summary's link names its field's input by fragment, which
    /// <c>&lt;base href="/"&gt;</c> would turn into a navigation to Home (A1).
    /// </summary>
    public static ValueTask FocusByIdAsync(IJSRuntime js, string id)
    {
        ArgumentNullException.ThrowIfNull(js);
        return js.InvokeVoidAsync(FocusByIdIdentifier, id);
    }

    /// <summary>
    /// The page's h1 takes the focus, after a load that did not change the page: FocusOnNavigate moves it only when the
    /// page itself changes (A3, A4).
    /// </summary>
    public static ValueTask FocusHeadingAsync(IJSRuntime js)
    {
        ArgumentNullException.ThrowIfNull(js);
        return js.InvokeVoidAsync(FocusHeadingIdentifier);
    }
}
