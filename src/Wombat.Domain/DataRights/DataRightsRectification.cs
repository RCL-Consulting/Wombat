namespace Wombat.Domain.DataRights;

public sealed class DataRightsRectification
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public Guid TargetId { get; set; }
    public string FromValueJson { get; set; } = "{}";
    public string ToValueJson { get; set; } = "{}";
    public DateTime? AppliedOn { get; set; }

    public DataRightsRequest Request { get; set; } = null!;

    public static DataRightsRectification Create(
        Guid requestId,
        string targetType,
        Guid targetId,
        string fromValueJson,
        string toValueJson)
    {
        return new DataRightsRectification
        {
            Id = Guid.CreateVersion7(),
            RequestId = requestId,
            TargetType = targetType.Trim(),
            TargetId = targetId,
            FromValueJson = fromValueJson,
            ToValueJson = toValueJson
        };
    }

    public void MarkApplied(DateTime utcNow)
    {
        AppliedOn = utcNow;
    }
}
