using MediatR;

namespace Wombat.Application.Features.Institutions.Queries.GetSpecialitiesForInstitution;

public sealed record GetSpecialitiesForInstitutionQuery(int InstitutionId) : IRequest<IReadOnlyList<SpecialityDto>>;
