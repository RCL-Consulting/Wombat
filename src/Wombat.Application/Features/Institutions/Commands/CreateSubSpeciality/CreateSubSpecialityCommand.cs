using System.Security.Claims;
using MediatR;

namespace Wombat.Application.Features.Institutions.Commands.CreateSubSpeciality;

public sealed record CreateSubSpecialityCommand(
    int SpecialityId,
    string Name,
    string? Description,
    ClaimsPrincipal Principal) : IRequest<SubSpecialityDto>;
