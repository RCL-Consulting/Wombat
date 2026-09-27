namespace Wombat.Web.Navigation;

/// <summary>
/// A switch's one-time word as <c>Routes</c> cascades it: what the switch did, and whether the page it landed on is still
/// the page shown. One per render of Routes, so one for the page's prerender and one for its circuit (T335, flow 01).
/// </summary>
/// <remarks>
/// The word is said on the page it landed on, and once: <c>App</c> took the cookie, so a reload says nothing, and Routes
/// marks the page left at the circuit's first navigation, so the next page says nothing either. Only the first alert to
/// show it takes the focus, so a header drawn again (a page that reloads what it shows) does not pull the focus back.
/// </remarks>
public sealed class ActingRoleSwitchNotice
{
    private bool _focusTaken;

    public ActingRoleSwitchNotice(ActingRoleSwitchResult? result)
    {
        Result = result;
    }

    /// <summary>What the switch did; null when this page is not the one a switch landed on.</summary>
    public ActingRoleSwitchResult? Result { get; }

    /// <summary>Whether the page the switch landed on has been left.</summary>
    public bool PageLeft { get; private set; }

    /// <summary>Marks the page the switch landed on as left.</summary>
    public void LeavePage() => PageLeft = true;

    /// <summary>True the first time it is asked, and never again.</summary>
    public bool TakeFocus()
    {
        if (_focusTaken)
        {
            return false;
        }

        _focusTaken = true;
        return true;
    }
}
