namespace Wombat.Web.Components.Shared;

/// <summary>
/// A button whose own action is running (DESIGN.md § Button system; T234 and its review).
/// </summary>
/// <remarks>
/// The button is not disabled while its action runs: it has the focus, and a browser drops the focus of a button it
/// disables, to the page body. The page's in-flight flag makes a second press send nothing, and
/// <see cref="AriaDisabled" /> says so to a screen reader. A changed label ("Saving...") on the focused button is often
/// not read out; a change of its <c>aria-disabled</c> state is ("unavailable", "dimmed").
/// </remarks>
public static class InFlight
{
    /// <summary>
    /// The button's <c>aria-disabled</c>: <c>"true"</c> while its own action runs, else nothing, so the attribute is not
    /// rendered at rest.
    /// </summary>
    public static string? AriaDisabled(bool running) => running ? "true" : null;
}
