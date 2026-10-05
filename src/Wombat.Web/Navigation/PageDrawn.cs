using RouteData = Microsoft.AspNetCore.Components.RouteData;

namespace Wombat.Web.Navigation;

/// <summary>
/// The page a routed page draws in its own place, as <c>Routes</c> cascades it to the shell (T358, build review D1): a
/// registrar, a roster or an EPA that is not the caller's to read draws flow 01's Page not found inline, at its own address,
/// and the shell lights what that page lights, which is nothing, not the routed page's owner. One per render of Routes, so
/// one for the page's prerender and one for its circuit.
/// </summary>
/// <remarks>
/// <see cref="Components.Shared.PageHeader" /> tells it: its <c>Page</c> is the page drawn when it is not the routed one
/// (Page not found, Access denied). The word is held against the route it was said on, so a navigation to another page
/// (a new <see cref="RouteData" />) forgets it without anyone clearing it. NavMenu reads it before the route.
/// </remarks>
public sealed class PageDrawn
{
    private RouteData? _route;
    private Type? _page;

    /// <summary>Raised when the page drawn for the route shown changes, so the menu lights again.</summary>
    public event Action? Changed;

    /// <summary>
    /// The page drawn on <paramref name="route" />: <paramref name="page" /> when the page draws another in its place, or
    /// null when it draws itself.
    /// </summary>
    public void Draw(RouteData? route, Type? page)
    {
        if (route is null || (ReferenceEquals(route, _route) && page == _page) || (page is null && !ReferenceEquals(route, _route)))
        {
            return;
        }

        _route = route;
        _page = page;
        Changed?.Invoke();
    }

    /// <summary>The page shown on <paramref name="route" />: the one drawn in its place, if one is, else the routed page.</summary>
    public Type? On(RouteData? route)
        => route is not null && ReferenceEquals(route, _route) && _page is not null ? _page : route?.PageType;
}
