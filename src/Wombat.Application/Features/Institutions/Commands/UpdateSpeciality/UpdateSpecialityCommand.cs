using System.Security.Claims;
using MediatR;

namespace Wombat.Application.Features.Institutions.Commands.UpdateSpeciality;

public sealed record UpdateSpecialityCommand(
    int Id,
    int CollegeId,
    string Name,
    string? Description,
    bool IsActive,
    ClaimsPrincipal Principal) : IRequest<SpecialityDto>;
