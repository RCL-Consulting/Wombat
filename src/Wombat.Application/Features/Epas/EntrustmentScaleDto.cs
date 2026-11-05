namespace Wombat.Application.Features.Epas;

public sealed record EntrustmentScaleDto(int Id, string Name, string? Description, IReadOnlyList<EntrustmentLevelDto> Levels)
{
    /// <summary>
    /// Whether anything uses the scale (<see cref="EntrustmentScaleUses" />, T394, flow 16, Q16, T395): its page then offers
    /// no Up, Down or Remove, and the update refuses a reorder or a removal. Filled by <c>GetEntrustmentScaleByIdQuery</c>
    /// alone; false on the list and from the commands (build-contracts § 0).
    /// </summary>
    public bool InUse { get; init; }
}

public sealed record EntrustmentLevelDto(int Id, int Order, string Label, string? Description);
