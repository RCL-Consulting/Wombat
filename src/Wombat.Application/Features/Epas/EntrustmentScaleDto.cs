namespace Wombat.Application.Features.Epas;

public sealed record EntrustmentScaleDto(int Id, string Name, string? Description, IReadOnlyList<EntrustmentLevelDto> Levels);

public sealed record EntrustmentLevelDto(int Id, int Order, string Label, string? Description);
