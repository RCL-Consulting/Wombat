using System.Security.Claims;
using MediatR;

namespace Wombat.Application.Features.Institutions.Commands.UpdateInstitution;

public sealed record UpdateInstitutionCommand(
    int Id,
    string Name,
    string ShortCode,
    string? ContactEmail,
    bool IsActive,
    ClaimsPrincipal Principal) : IRequest<InstitutionDto>;
