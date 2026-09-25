using Microsoft.AspNetCore.Components;

namespace Wombat.Web.Security;

/// <summary>
/// A circuit's way out once its sign-in has ended: a full page load of <see cref="SessionEnd" />, coming back to the page
/// the tab was on, made once however many components ask. (The T279 review.)
/// </summary>
/// <remarks>
/// <para>
/// Two components ask, both only in a circuit: <c>LeaveEndedSession</c>, in Routes.razor, the moment the circuit's sign-in
/// ends on any page, and <c>RedirectToLogin</c>, which the router renders on a page the circuit may no longer see. Both
/// hear the one change, so the second finds the tab already leaving.
/// </para>
/// <para>
/// A full load, never navigation inside the circuit: the sign-in page rendered in the old circuit posts the old user's
/// antiforgery token, which antiforgery refuses once the cookie is refused (<see cref="SessionEnd" />). Leaving also ends
/// the anonymous circuit, which App.razor never gives anyone.
/// </para>
/// </remarks>
public sealed class EndedSessionExit(NavigationManager navigation)
{
    private bool _leaving;

    /// <summary>Whether the tab has been sent to <see cref="SessionEnd" />.</summary>
    public bool Leaving => _leaving;

    /// <summary>Sends the tab to <see cref="SessionEnd" /> by a full page load, unless it has been sent already.</summary>
    public void Leave()
    {
        if (_leaving)
        {
            return;
        }

        _leaving = true;
        navigation.NavigateTo(SessionEnd.Url(CurrentPath(navigation)), forceLoad: true);
    }

    /// <summary>The page the tab is on, as a path on this site with its query: where signing in again comes back to.</summary>
    public static string CurrentPath(NavigationManager navigation)
    {
        ArgumentNullException.ThrowIfNull(navigation);

        var relative = navigation.ToBaseRelativePath(navigation.Uri);
        return string.IsNullOrWhiteSpace(relative) ? "/" : $"/{relative}";
    }
}
