using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;

namespace Wombat.Web.Components.Pages.Admin.Audit;

/// <summary>
/// The audit pages' addresses (T400, flow 18, lane C; T18, T277): the log with its filters, the window it was read in and
/// its page, so a way back returns to the same rows; and an entry, carrying that address as its way back.
/// </summary>
public static class AuditLinks
{
    /// <summary>The audit log: <c>/admin/audit</c>.</summary>
    public const string Log = "/admin/audit";

    /// <summary>An entry's <c>back</c>: the log's address it was opened from.</summary>
    public const string BackParameter = "back";

    /// <summary>
    /// The log's <c>entry</c>: the entry the reader came back from by the trail, whose Detail takes the focus when it is on
    /// the page (R3-L13, as an item's record returns to its row's View).
    /// </summary>
    public const string EntryParameter = "entry";

    /// <summary>
    /// Where an entry's trail returns (T277, R3-L13): the log's address it was opened from, allow-listed, with the entry
    /// named so its Detail takes the focus.
    /// </summary>
    public static string ReturnFrom(string? back, Guid entryId)
        => QueryHelpers.AddQueryString(SafeLogReturn(back), EntryParameter, entryId.ToString());

    /// <summary>An entry: <c>/admin/audit/{id}</c>, with <c>back=</c> the log's address it was opened from, when given.</summary>
    public static string Entry(Guid id, string? back = null)
    {
        var path = string.Create(CultureInfo.InvariantCulture, $"{Log}/{id}");
        return string.IsNullOrWhiteSpace(back) ? path : QueryHelpers.AddQueryString(path, BackParameter, back);
    }

    /// <summary>The log at <paramref name="address" />: <c>/admin/audit?result=failure&amp;shown=…&amp;page=2</c>.</summary>
    public static string LogAt(AuditLogAddress address)
    {
        var query = address.Query();
        return query.Count == 0 ? Log : QueryHelpers.AddQueryString(Log, query);
    }

    /// <summary>
    /// The log address an entry's trail may lead back to (an allow-listed return, as <c>PeopleLinks.SafeUsersReturn</c>):
    /// the log's own path with only its own parameters, rebuilt from their parsed values, so a crafted <c>back</c> can never
    /// send anyone to another page or host. Anything else, a missing value included, is plain <see cref="Log" />.
    /// </summary>
    public static string SafeLogReturn(string? back)
    {
        if (string.IsNullOrWhiteSpace(back))
        {
            return Log;
        }

        var queryStart = back.IndexOf('?', StringComparison.Ordinal);
        var path = queryStart < 0 ? back : back[..queryStart];
        if (!string.Equals(path, Log, StringComparison.Ordinal) || queryStart < 0)
        {
            return Log;
        }

        var parsed = QueryHelpers.ParseQuery(back[queryStart..]);
        if (parsed.Keys.Any(key => !AuditLogAddress.Parameters.Contains(key)))
        {
            return Log;
        }

        string? Text(string key) => parsed.TryGetValue(key, out var value) ? value.ToString() : null;

        return LogAt(AuditLogAddress.Parse(
            Text(AuditLogAddress.CategoryParameter),
            Text(AuditLogAddress.ActionParameter),
            Text(AuditLogAddress.PersonParameter),
            Text(AuditLogAddress.SubjectParameter),
            Text(AuditLogAddress.FromParameter),
            Text(AuditLogAddress.ToParameter),
            Text(AuditLogAddress.ResultParameter),
            Text(AuditLogAddress.InstitutionParameter),
            Text(AuditLogAddress.ShownParameter),
            Text(AuditLogAddress.OpenedParameter),
            Text(AuditLogAddress.PageParameter),
            Text(AuditLogAddress.SizeParameter)));
    }
}

/// <summary>How the log's window was fixed (T21): on opening the page, or by Show.</summary>
public enum AuditWindowFixed
{
    /// <summary>On opening the page: the last 24 hours to that moment, unless From or To says otherwise.</summary>
    Opened,

    /// <summary>By Show, at that moment: To, when empty, is it.</summary>
    Shown,
}

/// <summary>
/// What the log's address says (T400, flow 18, T18, T21, T277): the filters as typed, the moment the window was fixed and
/// how, the page and its size. Each value is checked as it is read, so a hand-made address cannot reach the query with a
/// value it would refuse; a value that does not read is no filter.
/// </summary>
/// <param name="Category">A category's name, as the query takes it.</param>
/// <param name="From">From as typed, on the South African clock ("2026-11-10T18:58").</param>
/// <param name="To">To as typed; empty is "up to when you press Show".</param>
/// <param name="Result">"success" or "failure".</param>
/// <param name="Institution">"none", or an institution's id (the Administrator's alone).</param>
/// <param name="FixedAt">When the window was fixed, UTC; null for none yet.</param>
public sealed record AuditLogAddress(
    string? Category,
    string? Action,
    string? Person,
    string? Subject,
    string? From,
    string? To,
    string? Result,
    string? Institution,
    DateTime? FixedAt,
    AuditWindowFixed Fixed,
    int Page,
    int PageSize)
{
    public const string CategoryParameter = "category";
    public const string ActionParameter = "action";
    public const string PersonParameter = "person";
    public const string SubjectParameter = "subject";
    public const string FromParameter = "from";
    public const string ToParameter = "to";
    public const string ResultParameter = "result";
    public const string InstitutionParameter = "institution";
    public const string ShownParameter = "shown";
    public const string OpenedParameter = "opened";
    public const string PageParameter = "page";
    public const string SizeParameter = "size";

    /// <summary>The Result filter's values.</summary>
    public const string SuccessValue = "success";
    public const string FailureValue = "failure";

    /// <summary>The Institution filter's "No institution" value.</summary>
    public const string NoInstitutionValue = "none";

    /// <summary>The list's page size on arrival (T17: 50 a page).</summary>
    public const int DefaultPageSize = 50;

    /// <summary>The <c>datetime-local</c> value's form, to the minute, as the boards draw it.</summary>
    public const string InputFormat = "yyyy-MM-ddTHH:mm";

    private static readonly string[] InputFormats = [InputFormat, "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.fff"];

    private static readonly int[] PageSizes = [10, 20, 50, 100];

    /// <summary>Every parameter the log reads.</summary>
    public static readonly IReadOnlySet<string> Parameters = new HashSet<string>(StringComparer.Ordinal)
    {
        CategoryParameter, ActionParameter, PersonParameter, SubjectParameter, FromParameter, ToParameter, ResultParameter,
        InstitutionParameter, ShownParameter, OpenedParameter, PageParameter, SizeParameter,
    };

    /// <summary>The log with nothing asked: the last 24 hours, fixed on arrival.</summary>
    public static AuditLogAddress None { get; } =
        new(null, null, null, null, null, null, null, null, null, AuditWindowFixed.Opened, 1, DefaultPageSize);

    /// <summary>Whether any filter is set (Clear filters is offered, T328).</summary>
    public bool IsFiltered
        => Category is not null || Action is not null || Person is not null || Subject is not null || From is not null ||
           To is not null || Result is not null || Institution is not null;

    /// <summary>
    /// Reads the address's values, keeping only those that read: a category the query knows, a typed time, a result, an
    /// institution's id or "none", a UTC moment, a page from 1 and one of PagerControls' sizes.
    /// </summary>
    public static AuditLogAddress Parse(
        string? category,
        string? action,
        string? person,
        string? subject,
        string? from,
        string? to,
        string? result,
        string? institution,
        string? shown,
        string? opened,
        string? page,
        string? size)
    {
        var shownAt = Moment(shown);
        var openedAt = shownAt is null ? Moment(opened) : null;
        return new AuditLogAddress(
            AuditWords.IsCategory(category) ? category : null,
            Text(action),
            Text(person),
            Text(subject),
            TypedTime(from) is not null ? Text(from) : null,
            TypedTime(to) is not null ? Text(to) : null,
            result is SuccessValue or FailureValue ? result : null,
            institution == NoInstitutionValue || PositiveNumber(institution) is not null ? institution : null,
            shownAt ?? openedAt,
            shownAt is not null ? AuditWindowFixed.Shown : AuditWindowFixed.Opened,
            PositiveNumber(page) ?? 1,
            PositiveNumber(size) is int chosen && PageSizes.Contains(chosen) ? chosen : DefaultPageSize);
    }

    /// <summary>A typed From or To as the South African clock reads it; null for none or one that does not read.</summary>
    public static DateTime? TypedTime(string? typed)
        => !string.IsNullOrWhiteSpace(typed) &&
           DateTime.TryParseExact(typed.Trim(), InputFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)
            ? value
            : null;

    /// <summary>The query string's values: only those set, the defaults left out.</summary>
    public Dictionary<string, string?> Query()
    {
        var query = new Dictionary<string, string?>(StringComparer.Ordinal);
        Add(query, CategoryParameter, Category);
        Add(query, ActionParameter, Action);
        Add(query, PersonParameter, Person);
        Add(query, SubjectParameter, Subject);
        Add(query, FromParameter, From);
        Add(query, ToParameter, To);
        Add(query, ResultParameter, Result);
        Add(query, InstitutionParameter, Institution);
        if (FixedAt is { } fixedAt)
        {
            Add(
                query,
                Fixed == AuditWindowFixed.Shown ? ShownParameter : OpenedParameter,
                DateTime.SpecifyKind(fixedAt, DateTimeKind.Utc).ToString("O", CultureInfo.InvariantCulture));
        }

        if (Page > 1)
        {
            Add(query, PageParameter, Page.ToString(CultureInfo.InvariantCulture));
        }

        if (PageSize != DefaultPageSize)
        {
            Add(query, SizeParameter, PageSize.ToString(CultureInfo.InvariantCulture));
        }

        return query;
    }

    private static DateTime? Moment(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var moment)
            ? DateTime.SpecifyKind(moment, DateTimeKind.Utc)
            : null;

    private static int? PositiveNumber(string? value)
        => int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var number) && number > 0 ? number : null;

    private static string? Text(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void Add(Dictionary<string, string?> query, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            query[key] = value;
        }
    }
}
