using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions.Queries.GetMyInstitutionName;

/// <summary>
/// The caller's own institution's name, read now (T376, flow 12, S9; build-contracts § 0): what the people pages' subtitles
/// and Home's card name it by ("Everyone with an account at Kgosi Kgari Teaching Hospital: …"). The claims carry the id
/// only, and an Administrator can rename an institution (flow 18), so the name is read, never kept in a claim. Null for a
/// caller with no institution (the Administrator) or one whose institution is gone. A page reads it before its list, and
/// while it loads or after its list fails the page says nothing that needs the name (I05, U11, T12).
/// </summary>
public sealed record GetMyInstitutionNameQuery(ClaimsPrincipal Principal) : IRequest<string?>;

public sealed class GetMyInstitutionNameQueryHandler : IRequestHandler<GetMyInstitutionNameQuery, string?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetMyInstitutionNameQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string?> Handle(GetMyInstitutionNameQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Principal);

        if (request.Principal.GetInstitutionId() is not int institutionId)
        {
            return null;
        }

        return await _dbContext.Set<Institution>()
            .AsNoTracking()
            .Where(entity => entity.Id == institutionId)
            .Select(entity => (string?)entity.Name)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
