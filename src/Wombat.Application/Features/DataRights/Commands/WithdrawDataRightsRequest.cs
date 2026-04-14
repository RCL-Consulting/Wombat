using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.DataRights;

namespace Wombat.Application.Features.DataRights.Commands;

public sealed record WithdrawDataRightsRequestCommand(
    Guid RequestId,
    ClaimsPrincipal Principal) : IRequest;

public sealed class WithdrawDataRightsRequestCommandValidator : AbstractValidator<WithdrawDataRightsRequestCommand>
{
    public WithdrawDataRightsRequestCommandValidator()
    {
        RuleFor(command => command.RequestId).NotEmpty();
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class WithdrawDataRightsRequestCommandHandler : IRequestHandler<WithdrawDataRightsRequestCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public WithdrawDataRightsRequestCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(WithdrawDataRightsRequestCommand request, CancellationToken cancellationToken)
    {
        var userId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User identity is required.");

        var entity = await _dbContext.Set<DataRightsRequest>()
            .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken)
            ?? throw new InvalidOperationException("Data rights request not found.");

        if (!string.Equals(entity.RequesterUserId, userId, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("You can only withdraw your own requests.");

        entity.Withdraw();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
