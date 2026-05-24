using System.Security.Claims;
using MediatR;
using Wombat.Application.Common;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
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
    private readonly IApplicationDbContext _dbContext;

    public CreateSsoGroupMappingCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> Handle(CreateSsoGroupMappingCommand request, CancellationToken cancellationToken)
    {
        if (!request.Principal.CanAccessInstitution(request.InstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to create SSO mappings for that institution.");
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
