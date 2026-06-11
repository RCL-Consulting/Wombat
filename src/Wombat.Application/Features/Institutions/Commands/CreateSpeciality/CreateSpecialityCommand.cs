using System.Security.Claims;
using MediatR;

namespace Wombat.Application.Features.Institutions.Commands.CreateSpeciality;

public sealed record CreateSpecialityCommand(
    int CollegeId,
    string Name,
    string? Description,
    ClaimsPrincipal Principal) : IRequest<SpecialityDto>;
