using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;

using Wombat.Application.Common;

namespace Wombat.Application.Features.Sso;

/// <summary>No validator: carries a single non-nullable int ID; EF lookup enforces existence.</summary>
[NoValidator]
public sealed record DeleteSsoGroupMappingCommand(int Id) : IRequest;

public sealed class DeleteSsoGroupMappingCommandHandler : IRequestHandler<DeleteSsoGroupMappingCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteSsoGroupMappingCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteSsoGroupMappingCommand request, CancellationToken cancellationToken)
    {
        var mapping = await _dbContext.Set<SsoGroupRoleMapping>()
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (mapping is null)
        {
            throw new InvalidOperationException("SSO group mapping not found.");
        }

        _dbContext.Set<SsoGroupRoleMapping>().Remove(mapping);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
