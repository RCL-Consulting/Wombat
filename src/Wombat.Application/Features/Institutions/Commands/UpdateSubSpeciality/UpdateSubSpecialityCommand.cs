using System.Security.Claims;
using MediatR;

namespace Wombat.Application.Features.Institutions.Commands.UpdateSubSpeciality;

public sealed record UpdateSubSpecialityCommand(
    int Id,
    int SpecialityId,
    string Name,
    string? Description,
    bool IsActive,
    int? DefaultEntrustmentScaleId,
    ClaimsPrincipal Principal) : IRequest<SubSpecialityDto>;
