using System.Security.Claims;
using MediatR;

namespace Wombat.Application.Features.Epas.Commands.UpdateEntrustmentScale;

public sealed record EntrustmentLevelUpdate(int? Id, int Order, string Label, string? Description);

public sealed record UpdateEntrustmentScaleCommand(
    int Id,
    string Name,
    string? Description,
    IReadOnlyList<EntrustmentLevelUpdate> Levels,
    ClaimsPrincipal Principal) : IRequest<UpdateEntrustmentScaleResult>;

/// <summary>The saved scale, and what the save did that the administrator should hear about.</summary>
/// <param name="RenameWarning">
/// Set when the save renamed the scale and a published activity-type schema bound it by its old name, so that schema's
/// fields no longer find it (T253). It names each such type. Null otherwise, which is every rename of a scale the seeds
/// bind (by seed key) or the builder binds (by id).
/// </param>
public sealed record UpdateEntrustmentScaleResult(EntrustmentScaleDto Scale, string? RenameWarning);
