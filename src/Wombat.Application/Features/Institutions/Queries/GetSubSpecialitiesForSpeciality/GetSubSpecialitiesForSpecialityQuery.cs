using System.Security.Claims;
using MediatR;

namespace Wombat.Application.Features.Institutions.Queries.GetSubSpecialitiesForSpeciality;

public sealed record GetSubSpecialitiesForSpecialityQuery(int SpecialityId, ClaimsPrincipal Principal) : IRequest<IReadOnlyList<SubSpecialityDto>>;
