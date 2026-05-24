using MediatR;
using Wombat.Application.Common;

namespace Wombat.Application.Features.Epas.Commands.DeleteEntrustmentScale;

/// <summary>No validator: single id; handler enforces existence and referential integrity.</summary>
[NoValidator]
public sealed record DeleteEntrustmentScaleCommand(int Id) : IRequest;
