using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.DataRights.Commands;

public sealed record ApproveDataRightsRequestCommand(
    Guid RequestId,
    string DecisionNote,
    ClaimsPrincipal Principal) : IRequest<DataRightsRequestDto>;

public sealed class ApproveDataRightsRequestCommandValidator : AbstractValidator<ApproveDataRightsRequestCommand>
{
    public ApproveDataRightsRequestCommandValidator()
    {
        RuleFor(command => command.RequestId).NotEmpty();
        RuleFor(command => command.DecisionNote).NotEmpty().MaximumLength(4000);
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class ApproveDataRightsRequestCommandHandler : IRequestHandler<ApproveDataRightsRequestCommand, DataRightsRequestDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IErasureExecutor _erasureExecutor;
    private readonly IAccessReportBuilder _accessReportBuilder;
    private readonly WombatOptions _options;

    public ApproveDataRightsRequestCommandHandler(
        IApplicationDbContext dbContext,
        IErasureExecutor erasureExecutor,
        IAccessReportBuilder accessReportBuilder,
        IOptions<WombatOptions> options)
    {
        _dbContext = dbContext;
        _erasureExecutor = erasureExecutor;
        _accessReportBuilder = accessReportBuilder;
        _options = options.Value;
    }

    public async Task<DataRightsRequestDto> Handle(ApproveDataRightsRequestCommand request, CancellationToken cancellationToken)
    {
        DemandReviewAccess(request.Principal);

        var actorUserId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User identity is required.");

        var entity = await _dbContext.Set<DataRightsRequest>()
            .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken)
            ?? throw new InvalidOperationException("Data rights request not found.");

        // Validate type-specific prerequisites BEFORE mutating the request, so a precondition
        // failure (e.g. missing PseudonymSalt) leaves it actionable (Submitted/UnderReview) rather
        // than stranded in Approved-but-not-executed with no UI recovery. (T084 / F-A1-1)
        string? erasureSalt = null;
        if (entity.Type == DataRightsRequestType.Erasure)
        {
            erasureSalt = _options.PseudonymSalt
                ?? throw new InvalidOperationException(
                    "PseudonymSalt is not configured. Cannot execute erasure without it.");
        }

        var utcNow = DateTime.UtcNow;
        entity.Approve(actorUserId, request.DecisionNote, utcNow);

        // Execute type-specific actions on approval
        switch (entity.Type)
        {
            case DataRightsRequestType.Erasure:
                await _erasureExecutor.ExecuteAsync(entity, erasureSalt!, cancellationToken);
                entity.Complete(DateTime.UtcNow);
                break;

            case DataRightsRequestType.Access:
            case DataRightsRequestType.Export:
                // Access/Export reports are generated on-demand when the user downloads.
                // Mark completed immediately — the download endpoint serves the report.
                entity.Complete(DateTime.UtcNow);
                break;

            case DataRightsRequestType.Objection:
                // Objection flags are toggled by the user directly on their profile.
                // Approval confirms the institution's acknowledgement.
                entity.Complete(DateTime.UtcNow);
                break;

            case DataRightsRequestType.Rectification:
                // Rectification is applied manually by admin via ApplyRectificationCommand.
                // Stays in Approved status until rectifications are applied.
                break;
        }

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
