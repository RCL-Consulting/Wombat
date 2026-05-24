using System.Security.Claims;
using MediatR;

namespace Wombat.Application.Features.Institutions.Queries.GetInstitutionById;

public sealed record GetInstitutionByIdQuery(int Id, ClaimsPrincipal Principal) : IRequest<InstitutionDto?>;
