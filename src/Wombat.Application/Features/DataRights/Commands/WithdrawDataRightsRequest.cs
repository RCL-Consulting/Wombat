using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.DataRights;

namespace Wombat.Application.Features.DataRights.Commands;

/// <summary>
/// The requester takes back a request while it waits for a decision (T387, Q14). One decided meanwhile, by a reviewer in
/// another tab or by the erasure of another of the requester's requests, is refused as
/// <see cref="DataRightsRefusal.DecidedMeanwhile" />, and nothing is withdrawn.
/// </summary>
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

        // Asked here, before anything is staged, so the refusal is one My data rights words (T387, Q14, T329); the domain's
        // own rule stays the last guard. Until T387 an approved or rejected request could be withdrawn by the command.
        if (entity.Status != DataRightsRequestStatus.Submitted)
        {
            throw new DataRightsRefusedException(DataRightsRefusal.DecidedMeanwhile, DecidedMeanwhileMessage);
        }

        entity.Withdraw();

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            // The request's xmin token (DataRightsRequestConfiguration, T387, F8) refused the save: it was decided between
            // the read and the save. Carried as the inner exception, so the audit pipeline discards the refused change and
            // writes its row alone (T201).
            throw new DataRightsRefusedException(DataRightsRefusal.DecidedMeanwhile, DecidedMeanwhileMessage, exception);
        }
    }

    /// <summary>The refusal's own message, for the audit row and the log; the page says it in its own words.</summary>
    public const string DecidedMeanwhileMessage = "This request was decided before it could be withdrawn. Nothing was withdrawn.";
}
