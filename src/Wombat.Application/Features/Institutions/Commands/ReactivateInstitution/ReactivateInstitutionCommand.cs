using System.Security.Claims;
using MediatR;
using Wombat.Application.Common;

namespace Wombat.Application.Features.Institutions.Commands.ReactivateInstitution;

/// <summary>
/// Makes an inactive institution active again, so invitations can be issued for it once more. The Administrator's alone,
/// as deactivating is (T056, T302).
/// </summary>
/// <remarks>No validator: carries a single non-nullable int ID; EF lookup enforces existence.</remarks>
[NoValidator]
public sealed record ReactivateInstitutionCommand(int Id, ClaimsPrincipal Principal) : IRequest;
