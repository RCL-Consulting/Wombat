using MediatR;
using Wombat.Application.Common;

namespace Wombat.Application.Features.Institutions.Commands.DeactivateSubSpeciality;

/// <summary>No validator: carries a single non-nullable int ID; EF lookup enforces existence.</summary>
[NoValidator]
public sealed record DeactivateSubSpecialityCommand(int Id) : IRequest;
