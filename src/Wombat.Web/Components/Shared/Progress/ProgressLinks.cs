namespace Wombat.Web.Components.Shared.Progress;

/// <summary>
/// The EPA page's address and the names every way in links it by (T355, round 1 correction 2; C5, C7, E5). One place, so
/// Home's rows, My progress's index, the completed card and My activities' Credit cell cannot address or name it apart.
/// </summary>
/// <remarks>
/// Addressed by the EPA's id, never its code: a code is unique only within its namespace (national, or one institution's
/// local extras). No query and no fragment: the page's h1 takes the focus on arrival (Spec § 5).
/// </remarks>
public static class ProgressLinks
{
    /// <summary>"/portfolio/progress/2": one EPA's page under My progress.</summary>
    public static string Epa(int epaId) => $"/portfolio/progress/{epaId}";

    /// <summary>"Open My progress at PAED-001": the completed card's link when it lands on the EPA's page (E5).</summary>
    public static string OpenAt(string epaCode) => $"Open My progress at {epaCode}";

    /// <summary>"1 item to PAED-001, in My progress": My activities' Credit link, by the words it shows (C7).</summary>
    public static string CreditTo(string label, string epaCode) => $"{label} to {epaCode}, in My progress";
}
