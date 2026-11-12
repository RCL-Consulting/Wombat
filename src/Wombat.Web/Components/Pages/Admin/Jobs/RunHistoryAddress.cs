using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Components.Pages.Admin.Jobs;

/// <summary>
/// What Run history shows, as its address carries it (T400, flow 18, T18): <c>?job=&amp;status=&amp;from=&amp;to=&amp;page=</c>,
/// with <c>size</c> where the rows a page are not 50. Home's and a failed run's "Its runs" build the job and status part
/// with <see cref="JobLinks.RunsOf" />; this reads it, and writes the rest on Show and on a page turn, so the way back to
/// the page returns to the same rows.
/// </summary>
/// <param name="Job">One job's whole key, or null for every job.</param>
/// <param name="Status">A run status's name (Running, Succeeded, Failed), or null for every status.</param>
/// <param name="FromDay">The first South African day, or null.</param>
/// <param name="ToDay">The last South African day, or null.</param>
/// <param name="Page">The page, from 1.</param>
/// <param name="PageSize">Runs a page: 10, 20, 50 or 100.</param>
public sealed record RunHistoryAddress(
    string? Job,
    string? Status,
    DateOnly? FromDay,
    DateOnly? ToDay,
    int Page,
    int PageSize)
{
    public const string FromParameter = "from";

    public const string ToParameter = "to";

    public const string PageParameter = "page";

    public const string SizeParameter = "size";

    /// <summary>Fifty runs a page (R3-R01).</summary>
    public const int DefaultPageSize = 50;

    private const string DayFormat = "yyyy-MM-dd";

    private static readonly int[] PageSizes = [10, 20, 50, 100];

    /// <summary>Every run, from the first page, fifty a page.</summary>
    public static readonly RunHistoryAddress None = new(null, null, null, null, 1, DefaultPageSize);

    /// <summary>A filter is applied: Clear filters is offered (R3-R02), and nothing matching says what was asked.</summary>
    public bool IsFiltered => Job is not null || Status is not null || FromDay is not null || ToDay is not null;

    /// <summary>
    /// What the address's values ask for, each one that cannot be read left out: a status that is no run status's name, a
    /// day that is not <c>yyyy-MM-dd</c>, a page below 1, a size the pager does not offer.
    /// </summary>
    public static RunHistoryAddress From(string? job, string? status, string? from, string? to, int? page, int? size)
        => new(
            string.IsNullOrWhiteSpace(job) ? null : job.Trim(),
            status is not null && JobWords.Statuses.Contains(status, StringComparer.Ordinal) ? status : null,
            Day(from),
            Day(to),
            page is > 1 ? page.Value : 1,
            size is { } chosen && PageSizes.Contains(chosen) ? chosen : DefaultPageSize);

    /// <summary>The page's own address for these values: <c>/admin/jobs/runs?job=…&amp;status=…&amp;from=…&amp;to=…&amp;page=2</c>.</summary>
    public string Address()
    {
        var query = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            [JobLinks.JobParameter] = Job,
            [JobLinks.StatusParameter] = Status,
            [FromParameter] = FromDay?.ToString(DayFormat, CultureInfo.InvariantCulture),
            [ToParameter] = ToDay?.ToString(DayFormat, CultureInfo.InvariantCulture),
            [PageParameter] = Page > 1 ? Page.ToString(CultureInfo.InvariantCulture) : null,
            [SizeParameter] = PageSize != DefaultPageSize ? PageSize.ToString(CultureInfo.InvariantCulture) : null
        };

        var present = query.Where(pair => !string.IsNullOrEmpty(pair.Value)).ToDictionary(pair => pair.Key, pair => pair.Value);
        return present.Count == 0 ? JobLinks.Runs : QueryHelpers.AddQueryString(JobLinks.Runs, present);
    }

    private static DateOnly? Day(string? value)
        => DateOnly.TryParseExact(value, DayFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var day) ? day : null;
}
