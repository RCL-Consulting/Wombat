using MediatR;

namespace Wombat.Application.Features.Institutions.Queries.GetSubSpecialitiesForSpeciality;

public sealed record GetSubSpecialitiesForSpecialityQuery(int SpecialityId) : IRequest<IReadOnlyList<SubSpecialityDto>>;
