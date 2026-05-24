using System.Security.Claims;
using MediatR;
using Wombat.Application.Common;

namespace Wombat.Application.Features.Institutions.Commands.DeactivateInstitution;

/// <summary>No validator: carries a single non-nullable int ID; EF lookup enforces existence.</summary>
[NoValidator]
public sealed record DeactivateInstitutionCommand(int Id, ClaimsPrincipal Principal) : IRequest;
