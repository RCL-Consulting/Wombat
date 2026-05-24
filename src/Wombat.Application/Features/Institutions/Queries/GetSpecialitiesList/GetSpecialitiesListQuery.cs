using System.Security.Claims;
using MediatR;

namespace Wombat.Application.Features.Institutions.Queries.GetSpecialitiesList;

public sealed record GetSpecialitiesListQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<SpecialityDto>>;
