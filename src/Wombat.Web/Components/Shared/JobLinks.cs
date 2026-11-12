using Microsoft.AspNetCore.WebUtilities;

namespace Wombat.Web.Components.Shared;

/// <summary>
/// The scheduled jobs' addresses (T400, flow 18, T18, R3-H05, R3-R09), so no page writes one by hand: Scheduled jobs, Run
/// history, and Run history opened on one job's runs of one status, as Home's "Its runs" opens it.
/// </summary>
/// <remarks>
/// Run history reads its query by <see cref="JobParameter" /> and <see cref="StatusParameter" />
/// (<c>[SupplyParameterFromQuery(Name = …)]</c>) and may add its own (<c>from</c>, <c>to</c>, <c>page</c>). A status is the
/// run status's name: <c>Succeeded</c>, <c>Failed</c> or <c>Running</c>.
/// </remarks>
public static class JobLinks
{
    /// <summary>Scheduled jobs: <c>/admin/jobs</c>.</summary>
    public const string Jobs = "/admin/jobs";

    /// <summary>Run history: <c>/admin/jobs/runs</c>.</summary>
    public const string Runs = "/admin/jobs/runs";

    /// <summary>Run history's job filter: <c>job</c>, a job's whole key.</summary>
    public const string JobParameter = "job";

    /// <summary>Run history's status filter: <c>status</c>, a run status's name.</summary>
    public const string StatusParameter = "status";

    /// <summary>
    /// Run history filtered to <paramref name="jobKey" />'s runs of <paramref name="status" />:
    /// <c>/admin/jobs/runs?job=entrustment-decision-expiry&amp;status=Failed</c>, each value escaped. A null or empty part is
    /// left out; with neither, Run history itself.
    /// </summary>
    public static string RunsOf(string? jobKey, string? status)
    {
        var address = Runs;
        if (!string.IsNullOrEmpty(jobKey))
        {
            address = QueryHelpers.AddQueryString(address, JobParameter, jobKey);
        }

        if (!string.IsNullOrEmpty(status))
        {
            address = QueryHelpers.AddQueryString(address, StatusParameter, status);
        }

        return address;
    }
}
