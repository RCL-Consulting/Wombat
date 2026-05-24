using System.Security.Claims;
using MediatR;

namespace Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;

public sealed record GetInstitutionsListQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<InstitutionDto>>;
