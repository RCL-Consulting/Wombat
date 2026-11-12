using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using Wombat.Application.Features.Audit;
using Wombat.Domain.Audit;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Components.Pages.Admin.Audit;

/// <summary>
/// The audit log's and an entry's words (T400, flow 18, lane C; R3-0-spec-2 § 9), so the pages print no enum's or
/// property's name and every time is <see cref="SastTime" />'s.
/// </summary>
public static class AuditWords
{
    // ── Titles and subtitles ────────────────────────────────────────────────────────────────────────────────────────
    public const string EntryTitle = "Audit entry";
    public const string EntryUnavailableTitle = "Audit entry unavailable";

    /// <summary>The Administrator's subtitle (T12).</summary>
    public const string EveryInstitutionSubtitle =
        "Every consequential action across the institutions, and those with no institution, newest first.";

    /// <summary>An institution's administrator's subtitle, by their institution's name (T12).</summary>
    public static string InstitutionSubtitle(string institutionName)
        => $"Every consequential action at {institutionName}, newest first.";

    // ── The filters (T1, T3; R3-0-spec-2 § 9) ─────────────────────────────────────────────────────────────────────────
    public const string FilterName = "Filter the audit log";
    public const string CategoryLabel = "Category";
    public const string AllCategories = "All categories";
    public const string ActionLabel = "Action";
    public const string ActionHelp = "Part of a name: \"Msf\" finds the MSF commands.";
    public const string PersonLabel = "Person's user id";
    public const string SubjectLabel = "Subject type";
    public const string FromLabel = "From (SAST)";
    public const string ToLabel = "To (SAST)";
    public const string ToHelp = "Empty: up to when you press Show.";
    public const string ResultLabel = "Result";
    public const string AllResults = "All results";
    public const string SuccessOnly = "Success only";
    public const string FailuresOnly = "Failures only";
    public const string InstitutionLabel = "Institution";
    public const string AllInstitutions = "All institutions";
    public const string NoInstitution = "No institution";
    public const string Show = "Show";
    public const string ClearFilters = "Clear filters";

    /// <summary>The categories in the boards' order, each by its label (K15: a map; the query keeps the enum).</summary>
    public static IReadOnlyList<(string Value, string Label)> Categories { get; } =
        Enum.GetValues<AuditCategory>().Select(category => (category.ToString(), Category(category))).ToList();

    /// <summary>A category by its label: "MSF", "Data rights", "Activity type" (K15, T324).</summary>
    public static string Category(AuditCategory category) => category switch
    {
        AuditCategory.Command => "Command",
        AuditCategory.Authentication => "Authentication",
        AuditCategory.Permission => "Permission",
        AuditCategory.Committee => "Committee",
        AuditCategory.Msf => "MSF",
        AuditCategory.Export => "Export",
        AuditCategory.DataRights => "Data rights",
        AuditCategory.Job => "Job",
        AuditCategory.ActivityType => "Activity type",
        _ => "Other",
    };

    /// <summary>Whether <paramref name="value" /> names a category, as the address carries it.</summary>
    public static bool IsCategory(string? value)
        => value is not null && Enum.TryParse<AuditCategory>(value, ignoreCase: false, out var category) &&
           Enum.IsDefined(category) && !int.TryParse(value, out _);

    /// <summary>The address's category as the query takes it; null for none.</summary>
    public static AuditCategory? CategoryOf(string? value)
        => IsCategory(value) ? Enum.Parse<AuditCategory>(value!) : null;

    /// <summary>The address's Result as the query takes it: true for success only, false for failures only.</summary>
    public static bool? SuccessOf(string? value) => value switch
    {
        AuditLogAddress.SuccessValue => true,
        AuditLogAddress.FailureValue => false,
        _ => null,
    };

    /// <summary>The address's Institution as the query takes it (K15); null for every row.</summary>
    public static AuditInstitutionFilter? InstitutionOf(string? value)
        => value == AuditLogAddress.NoInstitutionValue
            ? AuditInstitutionFilter.NoInstitution
            : int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var id) && id > 0
                ? AuditInstitutionFilter.One(id)
                : null;

    // ── The list ─────────────────────────────────────────────────────────────────────────────────────────────────────
    public const string ListHeading = "Actions";
    public const string Caption = "Actions, newest first";
    public const string Loading = "Loading the audit log.";
    public const string LoadFailed =
        "Could not load the audit log. Nothing has changed. Try again, or come back in a few minutes.";
    public const string Detail = "Detail";
    public const string NoMatchTitle = "No actions match";
    public const string NoMatchBody =
        "Nothing matches these filters between these times. Widen From and To, or clear the filters.";
    public const string ClearTheFilters = "Clear the filters";

    /// <summary>The count line (T17, K19): "166 actions.", "1 action.", or that none match.</summary>
    public static string CountLine(int total) => total switch
    {
        0 => "No actions match these filters.",
        1 => "1 action.",
        _ => string.Create(CultureInfo.InvariantCulture, $"{total} actions."),
    };

    /// <summary>
    /// The window line (T21): the window the rows were read in, and that a page turn keeps it. On arrival, the last 24
    /// hours to when the page was opened; after Show, From to when Show was pressed; a To typed is said whole.
    /// </summary>
    public static string WindowLine(DateTime from, DateTime to, AuditWindowFixed fixedBy, bool fromTyped, bool toTyped)
    {
        const string Keeps = " Turning a page keeps this window.";
        if (toTyped)
        {
            return $"Read from {SastTime.Minute(from)} to {SastTime.Minute(to)}.{Keeps}";
        }

        return fixedBy == AuditWindowFixed.Shown
            ? $"Read from {SastTime.Minute(from)} to {SastTime.Clock(to)} SAST, when you pressed Show.{Keeps}"
            : fromTyped
                ? $"Read from {SastTime.Minute(from)} to {SastTime.Clock(to)} SAST, when you opened the page.{Keeps}"
                : $"Read from {SastTime.Minute(from)} to {SastTime.Clock(to)} SAST: the last 24 hours, when you opened the page.{Keeps}";
    }

    /// <summary>The When cell's day, "2026-11-10": a time breaks only between its day and its hour.</summary>
    public static string WhenDay(DateTime utc) => SastTime.Day(utc);

    /// <summary>The When cell's hour, "20:24:10 SAST".</summary>
    public static string WhenHour(DateTime utc) => SastTime.Second(utc)[(SastTime.Day(utc).Length + 1)..];

    /// <summary>A Detail's name (C7): "Login at 2026-11-10 20:24:10 SAST", made distinct by <c>RowNames</c>.</summary>
    public static IReadOnlyDictionary<Guid, string> DetailNames(IEnumerable<AuditLogRowDto> rows)
        => RowNames.Distinct(
            rows,
            row => row.Id,
            row => $"{row.Action} at {SastTime.Second(row.OccurredAt)}",
            row => $" by {Person(row.ActorKind, row.PersonName)}");

    // ── Who acted (K8, T20) ──────────────────────────────────────────────────────────────────────────────────────────
    public const string MsfRespondent = "MSF respondent (no account)";
    public const string NoSignedInPerson = "No signed-in person";
    public const string MsfRespondentWho = "No person: an MSF respondent answers by link, with no account.";

    /// <summary>The Person cell and the subtitle's person: the name read now, or what kind of actor there was.</summary>
    public static string Person(AuditActorKind kind, string? name) => kind switch
    {
        AuditActorKind.Person => name ?? NoSignedInPerson,
        AuditActorKind.MsfRespondent => MsfRespondent,
        _ => name ?? NoSignedInPerson,
    };

    // ── Results ──────────────────────────────────────────────────────────────────────────────────────────────────────
    public static string Result(bool success) => success ? "Success" : "Failed";

    // ── An entry (T4) ────────────────────────────────────────────────────────────────────────────────────────────────
    public const string EntryLoading = "Loading this audit entry.";
    public const string EntryLoadFailed =
        "Could not load this audit entry. Nothing has changed. Try again, or come back in a few minutes.";
    public const string EntryUnavailableBody = "This audit entry does not exist, or you cannot open it.";
    public const string GoToAuditLog = "Go to Audit log";
    public const string WhatHappened = "What happened";
    public const string Who = "Who";
    public const string WhatItWasDoneTo = "What it was done to";
    public const string Payload = "Payload";
    public const string NoSubject = "No subject";
    public const string None = "None";
    public const string PayloadNote =
        "As stored, laid out one property a line: secrets read \"[REDACTED]\" and the signed-in person \"[PRINCIPAL]\".";

    /// <summary>The entry's subtitle: "TransitionActivityCommand by David Naidoo, 2026-11-10 19:10:20 SAST · Success".</summary>
    public static string EntrySubtitle(AuditEntryDetailDto entry)
        => $"{entry.Action} by {Person(entry.ActorKind, entry.PersonName)}, {SastTime.Second(entry.OccurredAt)} · {Result(entry.Success)}";

    /// <summary>The entry's time, to the millisecond on the South African clock: "2026-11-10 19:10:20.376 SAST" (T4).</summary>
    public static string Millisecond(DateTime utc) => SastTime.Millisecond(utc);

    /// <summary>The line under it: "Stored as 2026-11-10 17:10:20.376 UTC".</summary>
    public static string StoredAs(DateTime utc) => $"Stored as {SastTime.StoredUtc(utc)}";

    /// <summary>
    /// The sentence beside a stored address (K9), by its prefix: an IPv6 address is kept to its /48, an IPv4 one to its /24
    /// (<c>HttpAuditContextProvider</c>). Null for an address stored whole.
    /// </summary>
    public static string? AddressCut(string? address)
        => address is null ? null
            : address.EndsWith("/48", StringComparison.Ordinal) ? "Cut to its first 48 bits when stored."
            : address.EndsWith("/24", StringComparison.Ordinal) ? "Cut to its first 24 bits when stored."
            : null;

    private static readonly JsonWriterOptions PayloadLayout = new()
    {
        Indented = true,
        // Read as text on the page (Blazor escapes it), so it is written as stored: no ' for an apostrophe.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>
    /// The payload laid out one property a line, two spaces an indent, in its stored key order (T114 item 1, Q9): parsed
    /// and written again, never compared as text (CLAUDE.md: PostgreSQL re-renders a stored jsonb). A payload that does not
    /// parse is shown as stored.
    /// </summary>
    public static string LaidOut(string summaryJson)
    {
        try
        {
            using var document = JsonDocument.Parse(summaryJson);
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, PayloadLayout))
            {
                document.WriteTo(writer);
            }

            return System.Text.Encoding.UTF8.GetString(stream.ToArray());
        }
        catch (JsonException)
        {
            return summaryJson;
        }
    }
}
