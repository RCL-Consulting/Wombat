using MediatR;

namespace Wombat.Application.Features.Epas.Commands.UpdateEntrustmentScale;

public sealed record EntrustmentLevelUpdate(int? Id, int Order, string Label, string? Description);

public sealed record UpdateEntrustmentScaleCommand(
    int Id,
    string Name,
    string? Description,
    IReadOnlyList<EntrustmentLevelUpdate> Levels) : IRequest<EntrustmentScaleDto>;
