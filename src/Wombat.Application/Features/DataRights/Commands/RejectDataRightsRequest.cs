using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Audit;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.DataRights.Commands;

/// <remarks>
/// <c>DecisionNote</c> is redacted from the audit summary, for the same reason as on
/// ApproveDataRightsRequestCommand: the AuditPipelineBehavior would otherwise write the reviewer's
/// narrative about a named data subject's request into SummaryJson. A rejection note says more than
/// an approval does — it has to argue why the data is being kept. The RequestId stays in the
/// clear. (T101)
/// </remarks>
public sealed record RejectDataRightsRequestCommand(
    Guid RequestId,
    [property: Redact] string DecisionNote,
    ClaimsPrincipal Principal) : IRequest<DataRightsRequestDto>;

public sealed class RejectDataRightsRequestCommandValidator : AbstractValidator<RejectDataRightsRequestCommand>
{
    public RejectDataRightsRequestCommandValidator()
    {
        RuleFor(command => command.RequestId).NotEmpty();
        RuleFor(command => command.DecisionNote).NotEmpty().MaximumLength(4000);
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class RejectDataRightsRequestCommandHandler : IRequestHandler<RejectDataRightsRequestCommand, DataRightsRequestDto>
{
    private readonly IApplicationDbContext _dbContext;

    public RejectDataRightsRequestCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DataRightsRequestDto> Handle(RejectDataRightsRequestCommand request, CancellationToken cancellationToken)
    {
        DemandReviewAccess(request.Principal);

        var actorUserId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User identity is required.");

        var entity = await _dbContext.Set<DataRightsRequest>()
            .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken)
            ?? throw new InvalidOperationException("Data rights request not found.");

        entity.Reject(actorUserId, request.DecisionNote, DateTime.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DataRightsRequestDto(
            entity.Id,
            entity.RequesterUserId,
            entity.RequesterDisplayName,
            entity.RequestedOn,
            entity.Type,
            entity.Status,
            entity.Reason,
            entity.DecisionNote,
            entity.DecidedByUserId,
            entity.DecidedOn,
            entity.CompletedOn);
    }

    private static void DemandReviewAccess(ClaimsPrincipal principal)
    {
        if (principal.IsInRole(WombatRoles.Administrator) ||
            principal.IsInRole(WombatRoles.Coordinator))
            return;

        throw new UnauthorizedAccessException("Only administrators and coordinators may review data rights requests.");
    }
}
