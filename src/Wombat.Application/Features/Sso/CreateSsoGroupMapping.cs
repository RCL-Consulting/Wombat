using MediatR;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Sso;

public sealed record CreateSsoGroupMappingCommand(
    string ProviderKey,
    string ExternalGroupId,
    string ExternalGroupDisplayName,
    string WombatRole,
    int InstitutionId,
    int? SpecialityId,
    int? SubSpecialityId) : IRequest<int>;

public sealed class CreateSsoGroupMappingCommandHandler : IRequestHandler<CreateSsoGroupMappingCommand, int>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateSsoGroupMappingCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> Handle(CreateSsoGroupMappingCommand request, CancellationToken cancellationToken)
    {
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
