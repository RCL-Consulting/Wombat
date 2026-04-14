namespace Wombat.Domain.DataRights;

/// <summary>
/// Records the outcome of an approved erasure request — what was pseudonymised,
/// what was retained, and why. One record per erased user.
/// </summary>
public sealed class DataRightsErasureRecord
{
    private DataRightsErasureRecord() { }

    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Pseudonym { get; set; } = string.Empty;
    public DateTime ErasedOn { get; set; }

    /// <summary>
    /// JSON array of retention reason codes explaining what was kept and why,
    /// e.g. ["committee_decision","audit_log","ratified_assessment_record"].
    /// Stored as jsonb.
    /// </summary>
    public string RetentionReasonsJson { get; set; } = "[]";

    public DataRightsRequest Request { get; set; } = null!;

    public static DataRightsErasureRecord Create(
        Guid requestId,
        string userId,
        string pseudonym,
        DateTime utcNow,
        string retentionReasonsJson)
    {
        return new DataRightsErasureRecord
        {
            Id = Guid.CreateVersion7(utcNow),
            RequestId = requestId,
            UserId = userId.Trim(),
            Pseudonym = pseudonym.Trim(),
            ErasedOn = utcNow,
            RetentionReasonsJson = retentionReasonsJson
        };
    }
}
