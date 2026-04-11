using MediatR;

namespace Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;

public sealed record GetInstitutionsListQuery() : IRequest<IReadOnlyList<InstitutionDto>>;
