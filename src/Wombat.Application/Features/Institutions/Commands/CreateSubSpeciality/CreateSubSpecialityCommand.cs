using MediatR;

namespace Wombat.Application.Features.Institutions.Commands.CreateSubSpeciality;

public sealed record CreateSubSpecialityCommand(
    int SpecialityId,
    string Name,
    string? Description) : IRequest<SubSpecialityDto>;
