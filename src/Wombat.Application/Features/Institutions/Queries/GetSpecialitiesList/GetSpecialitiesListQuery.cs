using MediatR;

namespace Wombat.Application.Features.Institutions.Queries.GetSpecialitiesList;

public sealed record GetSpecialitiesListQuery() : IRequest<IReadOnlyList<SpecialityDto>>;
