using System.Security.Claims;
using MediatR;

namespace Wombat.Application.Features.Institutions.Queries.GetSpecialitiesForInstitution;

// Named "ForInstitution" historically; now resolves a College's national speciality catalogue (T091).
public sealed record GetSpecialitiesForInstitutionQuery(int CollegeId, ClaimsPrincipal Principal) : IRequest<IReadOnlyList<SpecialityDto>>;
