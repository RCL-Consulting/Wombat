using System.Globalization;

namespace Wombat.Web.Components.Shared.Programme;

/// <summary>
/// The addresses of flow 06's three pages and the names of the filters their addresses carry (T358, D3): one place, so
/// Home's rows, the menus and the pages' Show build the same links.
/// </summary>
/// <remarks>
/// Programme trainees reads <c>?short=&lt;EpaId&gt;&amp;year=&lt;n&gt;&amp;filed=true</c>; Waiting for assessors
/// <c>?show=overdue&amp;with=&lt;userId&gt;</c>. The page number is not in the address (D3).
/// </remarks>
public static class ProgrammeLinks
{
    /// <summary>Programme trainees.</summary>
    public const string Trainees = "/programme/trainees";

    /// <summary>Waiting for assessors.</summary>
    public const string Waiting = "/programme/waiting";

    /// <summary>The Short on filter's query name.</summary>
    public const string ShortKey = "short";

    /// <summary>The training year filter's query name.</summary>
    public const string YearKey = "year";

    /// <summary>The Nothing filed filter's query name.</summary>
    public const string FiledKey = "filed";

    /// <summary>Waiting for assessors' Waiting filter's query name.</summary>
    public const string ShowKey = "show";

    /// <summary>The Waiting filter's one value besides All: Overdue only.</summary>
    public const string ShowOverdueValue = "overdue";

    /// <summary>Waiting for assessors' With filter's query name.</summary>
    public const string WithKey = "with";

    /// <summary>Programme trainees filtered to Nothing filed in 30 days: the Coordinator's card's foot.</summary>
    public const string NothingFiled = Trainees + "?" + FiledKey + "=true";

    /// <summary>The registrar page: "/programme/trainees/7", by trainee-profile id.</summary>
    public static string Trainee(int profileId) => $"{Trainees}/{profileId.ToString(CultureInfo.InvariantCulture)}";

    /// <summary>Programme trainees filtered Short on one EPA: "/programme/trainees?short=2", an EPA's row on Home.</summary>
    public static string ShortOn(int epaId) => $"{Trainees}?{ShortKey}={epaId.ToString(CultureInfo.InvariantCulture)}";

    /// <summary>
    /// Programme trainees with these filters, as Show navigates (D3): the bare route when none is set, so Clear filters and
    /// an empty Show agree.
    /// </summary>
    public static string TraineesFiltered(int? shortOnEpaId, int? trainingYear, bool nothingFiled)
    {
        var query = new List<string>(3);
        if (shortOnEpaId is int epaId)
        {
            query.Add($"{ShortKey}={epaId.ToString(CultureInfo.InvariantCulture)}");
        }

        if (trainingYear is int year)
        {
            query.Add($"{YearKey}={year.ToString(CultureInfo.InvariantCulture)}");
        }

        if (nothingFiled)
        {
            query.Add($"{FiledKey}=true");
        }

        return query.Count == 0 ? Trainees : $"{Trainees}?{string.Join('&', query)}";
    }

    /// <summary>
    /// Waiting for assessors with these filters (D3): the bare route when none is set. The user id is escaped, as an
    /// address must carry it.
    /// </summary>
    public static string WaitingFiltered(bool overdueOnly, string? withUserId)
    {
        var query = new List<string>(2);
        if (overdueOnly)
        {
            query.Add($"{ShowKey}={ShowOverdueValue}");
        }

        if (!string.IsNullOrEmpty(withUserId))
        {
            query.Add($"{WithKey}={Uri.EscapeDataString(withUserId)}");
        }

        return query.Count == 0 ? Waiting : $"{Waiting}?{string.Join('&', query)}";
    }
}
