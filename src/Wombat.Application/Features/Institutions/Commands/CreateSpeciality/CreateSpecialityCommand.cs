using MediatR;

namespace Wombat.Application.Features.Institutions.Commands.CreateSpeciality;

public sealed record CreateSpecialityCommand(
    int InstitutionId,
    string Name,
    string? Description) : IRequest<SpecialityDto>;
