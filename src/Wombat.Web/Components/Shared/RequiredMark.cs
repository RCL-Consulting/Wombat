namespace Wombat.Web.Components.Shared;

/// <summary>
/// The line that opens every form with a starred field (the system's M4 rule, flow 11 onwards): one sentence for every
/// page, so a form does not word it again (T394 fix pass, pages N8). A page's words class may name it as its own constant,
/// always as this one.
/// </summary>
public static class RequiredMark
{
    public const string Line = "* marks a field you must fill in.";
}
