using System.Security.Claims;
using MediatR;
using Microsoft.Extensions.Options;
using Wombat.Application.Common;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Users;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Sso;

/// <summary>
/// No validator: the handler enforces the Administrator-role guard (the only non-trivial
/// business constraint). All string parameters are required by the Blazor form before dispatch.
/// </summary>
[NoValidator]
public sealed record CreateSsoGroupMappingCommand(
    string ProviderKey,
    string ExternalGroupId,
    string ExternalGroupDisplayName,
    string WombatRole,
    int InstitutionId,
    int? SpecialityId,
    int? SubSpecialityId,
    ClaimsPrincipal Principal) : IRequest<int>;

public sealed class CreateSsoGroupMappingCommandHandler : IRequestHandler<CreateSsoGroupMappingCommand, int>
{
    /// <summary>The refusal of a mapping whose provider is not configured, or serves another institution. (T288)</summary>
    public const string ProviderNotTheInstitutions =
        "That sign-in provider does not serve the institution chosen. A mapping is made for its provider's own institution.";

    private readonly IApplicationDbContext _dbContext;
    private readonly IOptions<SsoOptions> _ssoOptions;

    public CreateSsoGroupMappingCommandHandler(IApplicationDbContext dbContext, IOptions<SsoOptions> ssoOptions)
    {
        _dbContext = dbContext;
        _ssoOptions = ssoOptions;
    }

    /// <remarks>
    /// Every refusal comes before the write (T288). <b>A trainee first</b>
    /// (<see cref="UserAdministrationRules.DemandUserAdministration" />, T185, T278): a mapping grants roles at each
    /// sign-in, so a registrar who also administers makes none. Then the institution, which must be the caller's and the
    /// provider's own: the mapper applies a provider's mappings to the users who sign in through it, all of them at the
    /// provider's institution, so until T288 an InstitutionalAdmin at A could map B's provider's groups and grant B's
    /// users roles.
    /// </remarks>
    public async Task<int> Handle(CreateSsoGroupMappingCommand request, CancellationToken cancellationToken)
    {
        UserAdministrationRules.DemandUserAdministration(request.Principal);

        if (!request.Principal.CanAccessInstitution(request.InstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to create SSO mappings for that institution.");
        }

        var provider = _ssoOptions.Value.Providers
            .FirstOrDefault(entity => string.Equals(entity.Key, request.ProviderKey, StringComparison.Ordinal));
        if (provider is null || provider.InstitutionId != request.InstitutionId)
        {
            throw new InvalidOperationException(ProviderNotTheInstitutions);
        }

        if (string.Equals(request.WombatRole, WombatRoles.Administrator, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The Administrator role cannot be assigned via SSO group mapping.");
        }

        var mapping = new SsoGroupRoleMapping
        {
            ProviderKey = request.ProviderKey,
            ExternalGroupId = request.ExternalGroupId,
            ExternalGroupDisplayName = request.ExternalGroupDisplayName,
            WombatRole = request.WombatRole,
            InstitutionId = request.InstitutionId,
            SpecialityId = request.SpecialityId,
            SubSpecialityId = request.SubSpecialityId
        };

        _dbContext.Set<SsoGroupRoleMapping>().Add(mapping);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return mapping.Id;
    }
}
