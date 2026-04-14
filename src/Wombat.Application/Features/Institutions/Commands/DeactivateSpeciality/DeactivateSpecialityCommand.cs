using MediatR;
using Wombat.Application.Common;

namespace Wombat.Application.Features.Institutions.Commands.DeactivateSpeciality;

/// <summary>No validator: carries a single non-nullable int ID; EF lookup enforces existence.</summary>
[NoValidator]
public sealed record DeactivateSpecialityCommand(int Id) : IRequest;
