using System.Security.Claims;
using MediatR;

namespace Wombat.Application.Features.Institutions.Queries.GetSpecialitiesForInstitution;

public sealed record GetSpecialitiesForInstitutionQuery(int InstitutionId, ClaimsPrincipal Principal) : IRequest<IReadOnlyList<SpecialityDto>>;
