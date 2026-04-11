using MediatR;

namespace Wombat.Application.Features.Institutions.Commands.DeactivateSubSpeciality;

public sealed record DeactivateSubSpecialityCommand(int Id) : IRequest;
