using System.Security.Claims;
using MediatR;

namespace Wombat.Application.Features.Epas.Commands.CreateEntrustmentScale;

public sealed record EntrustmentLevelInput(int Order, string Label, string? Description);

public sealed record CreateEntrustmentScaleCommand(
    string Name,
    string? Description,
    IReadOnlyList<EntrustmentLevelInput> Levels,
    ClaimsPrincipal Principal) : IRequest<EntrustmentScaleDto>;
