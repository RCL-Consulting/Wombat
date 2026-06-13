using System.Security.Claims;
using MediatR;

namespace Wombat.Application.Features.Institutions.Queries.GetSpecialityById;

public sealed record GetSpecialityByIdQuery(int Id, ClaimsPrincipal Principal) : IRequest<SpecialityDto?>;
