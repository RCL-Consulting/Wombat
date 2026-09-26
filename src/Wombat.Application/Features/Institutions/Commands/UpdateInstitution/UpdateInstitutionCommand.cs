using System.Security.Claims;
using MediatR;

namespace Wombat.Application.Features.Institutions.Commands.UpdateInstitution;

/// <summary>
/// Saves an institution's name, short code and contact email, for an Administrator or the institution's own
/// InstitutionalAdmin.
/// </summary>
/// <remarks>
/// It carries no active state (T302). Until T302 it carried <c>IsActive</c> and wrote it for any caller in scope, so an
/// InstitutionalAdmin refused Deactivate could untick Active and save her institution inactive, or reactivate one an
/// Administrator had deactivated. The state changes only through <c>DeactivateInstitutionCommand</c> and
/// <c>ReactivateInstitutionCommand</c>, both the Administrator's.
/// </remarks>
public sealed record UpdateInstitutionCommand(
    int Id,
    string Name,
    string ShortCode,
    string? ContactEmail,
    ClaimsPrincipal Principal) : IRequest<InstitutionDto>;
