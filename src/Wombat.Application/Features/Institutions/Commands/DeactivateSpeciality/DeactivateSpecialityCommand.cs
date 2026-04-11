using MediatR;

namespace Wombat.Application.Features.Institutions.Commands.DeactivateSpeciality;

public sealed record DeactivateSpecialityCommand(int Id) : IRequest;
