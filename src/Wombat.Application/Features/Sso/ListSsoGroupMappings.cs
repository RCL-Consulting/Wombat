using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Users;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Sso;

public sealed record ListSsoGroupMappingsQuery(ClaimsPrincipal Principal, string? ProviderKey = null) : IRequest<IReadOnlyList<SsoGroupMappingDto>>;

public sealed class ListSsoGroupMappingsQueryHandler : IRequestHandler<ListSsoGroupMappingsQuery, IReadOnlyList<SsoGroupMappingDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IOptions<SsoOptions> _ssoOptions;

    public ListSsoGroupMappingsQueryHandler(IApplicationDbContext dbContext, IOptions<SsoOptions> ssoOptions)
    {
        _dbContext = dbContext;
        _ssoOptions = ssoOptions;
    }

    /// <remarks>
    /// Each mapping carries its provider's name and whether the provider is gone (T376, S5): no longer set up for the
    /// mapping's institution, so the mapper gives nothing from it (<c>SsoGroupMapper</c> reads only the signing provider's
    /// mappings at the provider's institution). The page lists those apart, read only to everyone but the Administrator.
    /// </remarks>
    public async Task<IReadOnlyList<SsoGroupMappingDto>> Handle(ListSsoGroupMappingsQuery request, CancellationToken cancellationToken)
    {
        // A trainee first (T288), as the commands: the page says why in place of the list.
        UserAdministrationRules.DemandUserAdministration(request.Principal);

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

        var rows = await (
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
                subSpeciality != null ? subSpeciality.Name : null,
                null,
                false))
            .ToListAsync(cancellationToken);

        var options = _ssoOptions.Value;
        return rows
            .Select(row => row with
            {
                ProviderName = SsoMappingProviders.NameOf(options, row.ProviderKey),
                ProviderGone = !SsoMappingProviders.Serves(options, row.ProviderKey, row.InstitutionId)
            })
            .ToList();
    }
}

/// <summary>
/// Which configured provider serves which institution (T288, T376 S5): the one rule the create, the delete and the list
/// read, so "gone" means the same on the page and in the handlers.
/// </summary>
public static class SsoMappingProviders
{
    /// <summary>Whether a provider with this key is set up and serves this institution.</summary>
    public static bool Serves(SsoOptions options, string providerKey, int institutionId)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.Providers.Any(provider =>
            string.Equals(provider.Key, providerKey, StringComparison.Ordinal) && provider.InstitutionId == institutionId);
    }

    /// <summary>The provider's display name, or its key when it is not set up (a gone provider's name is not known).</summary>
    public static string NameOf(SsoOptions options, string providerKey)
    {
        ArgumentNullException.ThrowIfNull(options);
        var provider = options.Providers.FirstOrDefault(entity => string.Equals(entity.Key, providerKey, StringComparison.Ordinal));
        return provider is null || string.IsNullOrWhiteSpace(provider.DisplayName) ? providerKey : provider.DisplayName;
    }
}
