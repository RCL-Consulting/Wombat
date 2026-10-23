using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Users;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Sso;

/// <summary>No validator: carries a single non-nullable int ID; EF lookup enforces existence.</summary>
[NoValidator]
public sealed record DeleteSsoGroupMappingCommand(int Id, ClaimsPrincipal Principal) : IRequest;

public sealed class DeleteSsoGroupMappingCommandHandler : IRequestHandler<DeleteSsoGroupMappingCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteSsoGroupMappingCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// <b>A trainee first</b> (<see cref="UserAdministrationRules.DemandUserAdministration" />, T288), before the id is
    /// looked up: a registrar who also administers could otherwise delete the mapping that gave them Trainee.
    /// </remarks>
    public async Task Handle(DeleteSsoGroupMappingCommand request, CancellationToken cancellationToken)
    {
        UserAdministrationRules.DemandUserAdministration(request.Principal);

        var mapping = await _dbContext.Set<SsoGroupRoleMapping>()
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (mapping is null)
        {
            throw new InvalidOperationException("SSO group mapping not found.");
        }

        if (!request.Principal.CanAccessInstitution(mapping.InstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this SSO mapping.");
        }

        _dbContext.Set<SsoGroupRoleMapping>().Remove(mapping);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
