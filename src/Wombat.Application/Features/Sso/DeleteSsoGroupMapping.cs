using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Users;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Sso;

/// <summary>No validator: carries a single non-nullable int ID; EF lookup enforces existence.</summary>
[NoValidator]
public sealed record DeleteSsoGroupMappingCommand(int Id, ClaimsPrincipal Principal) : IRequest;

public sealed class DeleteSsoGroupMappingCommandHandler : IRequestHandler<DeleteSsoGroupMappingCommand>
{
    /// <summary>The refusal of a mapping that is not there or is outside the caller's scope: the two read the same.</summary>
    public const string NotFound = "SSO group mapping not found.";

    /// <summary>The refusal of a gone provider's mapping to anyone but the Administrator (T376, S5; O03's words).</summary>
    public const string ProviderGoneAdministratorOnly =
        "Only a Wombat administrator can delete a mapping whose provider is no longer set up.";

    private readonly IApplicationDbContext _dbContext;
    private readonly IOptions<SsoOptions> _ssoOptions;

    public DeleteSsoGroupMappingCommandHandler(IApplicationDbContext dbContext, IOptions<SsoOptions> ssoOptions)
    {
        _dbContext = dbContext;
        _ssoOptions = ssoOptions;
    }

    /// <remarks>
    /// <b>A trainee first</b> (<see cref="UserAdministrationRules.DemandUserAdministration" />, T288), before the id is
    /// looked up: a registrar who also administers could otherwise delete the mapping that gave them Trainee. Then the
    /// mapping, where an id outside the caller's institution reads as one that is not there (T376; CLAUDE.md § Roles: 404,
    /// not 403, so another institution's ids are not confirmed). Then <b>a gone provider's mapping is the Administrator's
    /// alone</b> (S5): its provider is no longer set up for the institution, so it gives nothing new, and it is his to
    /// clear, with the configuration he changed. The roles it already gave remain either way.
    /// </remarks>
    public async Task Handle(DeleteSsoGroupMappingCommand request, CancellationToken cancellationToken)
    {
        UserAdministrationRules.DemandUserAdministration(request.Principal);

        var mapping = await _dbContext.Set<SsoGroupRoleMapping>()
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (mapping is null || !request.Principal.CanAccessInstitution(mapping.InstitutionId))
        {
            throw new SsoMappingRefusedException(SsoMappingRefusal.NotFound, SsoMappingField.None, NotFound);
        }

        if (!request.Principal.IsAdministrator()
            && !SsoMappingProviders.Serves(_ssoOptions.Value, mapping.ProviderKey, mapping.InstitutionId))
        {
            throw new SsoMappingRefusedException(SsoMappingRefusal.ProviderGone, SsoMappingField.None, ProviderGoneAdministratorOnly);
        }

        _dbContext.Set<SsoGroupRoleMapping>().Remove(mapping);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
