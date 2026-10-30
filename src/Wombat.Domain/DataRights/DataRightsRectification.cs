namespace Wombat.Domain.DataRights;

/// <summary>
/// One correction recorded on an approved Rectification: which record of the requester was changed, its value before and
/// after, who recorded it and when (T316; T387, Q2, Q26).
/// </summary>
/// <remarks>
/// <para>
/// The data itself is changed on the page that owns it (My account for a name, the trainee profile for a programme
/// start); the correction is the request's record that it was. <see cref="TargetType" /> is one of
/// <c>DataRightsRecords</c>' keys ("Account.LastName", "Other:‹name›"), and the record is always the requester's, so no
/// target id is kept: until T387 a <c>Guid TargetId</c> could hold neither a trainee profile's id (an int) nor say
/// anything the request's own requester does not.
/// </para>
/// <para>
/// The values are stored as JSON strings (<c>jsonb</c>), so compare them canonical-to-canonical, never as raw text
/// (CLAUDE.md: Postgres re-renders what it stores).
/// </para>
/// </remarks>
public sealed class DataRightsRectification
{
    private DataRightsRectification() { }

    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public string FromValueJson { get; set; } = "\"\"";
    public string ToValueJson { get; set; } = "\"\"";

    /// <summary>Who recorded the correction (T387, S11): the Coordinator or Administrator who carries the request through.</summary>
    public string AppliedByUserId { get; set; } = string.Empty;

    public DateTime AppliedOn { get; set; }

    public DataRightsRequest Request { get; set; } = null!;

    public static DataRightsRectification Create(
        Guid requestId,
        string targetType,
        string fromValueJson,
        string toValueJson,
        string appliedByUserId,
        DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetType);
        ArgumentException.ThrowIfNullOrWhiteSpace(appliedByUserId);

        return new DataRightsRectification
        {
            Id = Guid.CreateVersion7(utcNow),
            RequestId = requestId,
            TargetType = targetType.Trim(),
            FromValueJson = fromValueJson,
            ToValueJson = toValueJson,
            AppliedByUserId = appliedByUserId.Trim(),
            AppliedOn = utcNow
        };
    }
}
