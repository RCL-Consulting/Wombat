using System.Globalization;
using Wombat.Domain.DataRights;

namespace Wombat.Application.Features.DataRights;

/// <summary>
/// A request's type and status as a person reads them (T387, T324): the words every data-rights page, row name, filter
/// and mail uses, never the enum's name printed. One place, so the pages and the mail cannot drift.
/// </summary>
/// <remarks>
/// Today each label is its member's name, so the map looks idle; it is written out so that a renamed member, or one whose
/// name is not a word (UnderReview was one, until T387), cannot reach a page.
/// </remarks>
public static class DataRightsLabels
{
    /// <summary>The four types a person may ask for, in the order My data rights offers them (Q8: not Objection).</summary>
    public static IReadOnlyList<DataRightsRequestType> OfferedTypes { get; } =
    [
        DataRightsRequestType.Access,
        DataRightsRequestType.Export,
        DataRightsRequestType.Rectification,
        DataRightsRequestType.Erasure
    ];

    /// <summary>Every status, in the order the queue's Status filter offers them (Q8: UnderReview is gone).</summary>
    public static IReadOnlyList<DataRightsRequestStatus> Statuses { get; } =
    [
        DataRightsRequestStatus.Submitted,
        DataRightsRequestStatus.Approved,
        DataRightsRequestStatus.Completed,
        DataRightsRequestStatus.Rejected,
        DataRightsRequestStatus.Withdrawn
    ];

    public static string Type(DataRightsRequestType type) => type switch
    {
        DataRightsRequestType.Access => "Access",
        DataRightsRequestType.Rectification => "Rectification",
        DataRightsRequestType.Export => "Export",
        DataRightsRequestType.Objection => "Objection",
        DataRightsRequestType.Erasure => "Erasure",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "A data-rights request type with no label.")
    };

    public static string Status(DataRightsRequestStatus status) => status switch
    {
        DataRightsRequestStatus.Submitted => "Submitted",
        DataRightsRequestStatus.Approved => "Approved",
        DataRightsRequestStatus.Rejected => "Rejected",
        DataRightsRequestStatus.Completed => "Completed",
        DataRightsRequestStatus.Withdrawn => "Withdrawn",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "A data-rights request status with no label.")
    };

    /// <summary>
    /// What an approved erasure leaves in the person's own words on every one of their requests: the reason they gave, and
    /// each correction's value before and after (T387, review S1, the operator's decision of 2026-10-30): "Erased with the
    /// person's data on 2026-10-30.", the erasure's South African day. A reason ("Please erase Sipho Ndlovu's data") or a
    /// corrected name ("Sipho" to "Siphiwe") would otherwise name, under the pseudonym, the person it replaces. The staff's
    /// decision note is theirs, and is kept.
    /// </summary>
    public static string ErasedText(DateOnly erasedOn)
        => $"Erased with the person's data on {erasedOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.";
}
