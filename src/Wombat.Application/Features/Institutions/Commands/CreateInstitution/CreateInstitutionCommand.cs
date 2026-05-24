using System.Security.Claims;
using MediatR;

namespace Wombat.Application.Features.Institutions.Commands.CreateInstitution;

public sealed record CreateInstitutionCommand(
    string Name,
    string ShortCode,
    string? ContactEmail,
    ClaimsPrincipal Principal) : IRequest<InstitutionDto>;
