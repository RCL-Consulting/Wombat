using System.Security.Claims;
using MediatR;

namespace Wombat.Application.Features.Institutions.Queries.GetSubSpecialitiesList;

public sealed record GetSubSpecialitiesListQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<SubSpecialityDto>>;
