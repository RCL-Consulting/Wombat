using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Sso;

public sealed record ListSsoGroupMappingsQuery(ClaimsPrincipal Principal, string? ProviderKey = null) : IRequest<IReadOnlyList<SsoGroupMappingDto>>;

public sealed class ListSsoGroupMappingsQueryHandler : IRequestHandler<ListSsoGroupMappingsQuery, IReadOnlyList<SsoGroupMappingDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListSsoGroupMappingsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SsoGroupMappingDto>> Handle(ListSsoGroupMappingsQuery request, CancellationToken cancellationToken)
    {
        var mappings = _dbContext.Set<SsoGroupRoleMapping>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.ProviderKey))
        {
            mappings = mappings.Where(m => m.ProviderKey == request.ProviderKey);
        }

        if (!request.Principal.IsAdministrator())
        {
            var scopedInstitutionId = request.Principal.GetInstitutionId();
            if (!scopedInstitutionId.HasValue)
            {
                return Array.Empty<SsoGroupMappingDto>();
            }
            mappings = mappings.Where(m => m.InstitutionId == scopedInstitutionId.Value);
        }

        var institutions = _dbContext.Set<Institution>();
        var specialities = _dbContext.Set<Speciality>();
        var subSpecialities = _dbContext.Set<SubSpeciality>();

        return await (
            from mapping in mappings
            join institution in institutions on mapping.InstitutionId equals institution.Id
            join speciality in specialities on mapping.SpecialityId equals speciality.Id into specialityGroup
            from speciality in specialityGroup.DefaultIfEmpty()
            join subSpeciality in subSpecialities on mapping.SubSpecialityId equals subSpeciality.Id into subSpecialityGroup
            from subSpeciality in subSpecialityGroup.DefaultIfEmpty()
            orderby mapping.ProviderKey, mapping.ExternalGroupDisplayName
            select new SsoGroupMappingDto(
                mapping.Id,
                mapping.ProviderKey,
                mapping.ExternalGroupId,
                mapping.ExternalGroupDisplayName,
                mapping.WombatRole,
                mapping.InstitutionId,
                institution.Name,
                mapping.SpecialityId,
                speciality != null ? speciality.Name : null,
                mapping.SubSpecialityId,
                subSpeciality != null ? subSpeciality.Name : null))
            .ToListAsync(cancellationToken);
    }
}
