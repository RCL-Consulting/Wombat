using MediatR;

namespace Wombat.Application.Features.Institutions.Commands.DeactivateInstitution;

public sealed record DeactivateInstitutionCommand(int Id) : IRequest;
