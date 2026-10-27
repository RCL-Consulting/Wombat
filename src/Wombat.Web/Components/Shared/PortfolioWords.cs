namespace Wombat.Web.Components.Shared;

/// <summary>
/// The words two or more of flow 13's lanes print (T379, build-contracts § 0), so the header action on three pages and the
/// profile's name in its h1 and in the export's trail cannot drift apart.
/// </summary>
public static class PortfolioWords
{
    /// <summary>
    /// The staff export's header action, "Export Lerato Molefe's portfolio" (Q4, F1): on the trainee profile (lane P), the
    /// committee review page and the registrar's programme page (lane E). Also the staff export page's h1 and tab (Q5).
    /// </summary>
    public static string ExportAction(string traineeName) => $"Export {Possessive(traineeName)} portfolio";

    /// <summary>
    /// A trainee profile's title, "Lerato Molefe's programme" (Q7, T2): the profile's h1, tab and crumb (lane P), and the
    /// middle crumb of the Institutional admin's staff export, "Home › Trainees › Lerato Molefe's programme › Export
    /// portfolio" (M4, lane E).
    /// </summary>
    public static string ProgrammeOf(string traineeName) => $"{Possessive(traineeName)} programme";

    /// <summary>"Pieter du Plessis's": the name and 's, whatever it ends in, as the boards write it (Spec 3).</summary>
    public static string Possessive(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return $"{name.Trim()}'s";
    }
}
