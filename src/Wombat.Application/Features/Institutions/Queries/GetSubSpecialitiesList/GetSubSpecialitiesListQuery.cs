using MediatR;

namespace Wombat.Application.Features.Institutions.Queries.GetSubSpecialitiesList;

public sealed record GetSubSpecialitiesListQuery() : IRequest<IReadOnlyList<SubSpecialityDto>>;
