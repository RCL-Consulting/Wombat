using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Audit;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.DataRights;

namespace Wombat.Application.Features.DataRights.Commands;

/// <remarks>
/// <c>FromValueJson</c> and <c>ToValueJson</c> are redacted from the audit summary. The
/// AuditPipelineBehavior audits every request whose type name ends in "Command" and
/// AuditPayloadSerializer writes its properties into SummaryJson; these two ARE the personal data
/// being corrected — the before value most of all, since it is the wrong or objected-to value the
/// subject asked to have changed. DataRightsRectification already persists both, under the data
/// rights feature's own access control, so the audit copy added nothing but a second, ungated one
/// that a rectification can never reach. RequestId, TargetType and TargetId stay in the clear: they
/// identify what was rectified without restating it. (T101)
/// </remarks>
public sealed record ApplyRectificationCommand(
    Guid RequestId,
    string TargetType,
    Guid TargetId,
    [property: Redact] string FromValueJson,
    [property: Redact] string ToValueJson,
    ClaimsPrincipal Principal) : IRequest<DataRightsRectificationDto>;

public sealed class ApplyRectificationCommandValidator : AbstractValidator<ApplyRectificationCommand>
{
    public ApplyRectificationCommandValidator()
    {
        RuleFor(command => command.RequestId).NotEmpty();
        RuleFor(command => command.TargetType).NotEmpty().MaximumLength(200);
        RuleFor(command => command.TargetId).NotEmpty();
        RuleFor(command => command.FromValueJson).NotEmpty();
        RuleFor(command => command.ToValueJson).NotEmpty();
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class ApplyRectificationCommandHandler : IRequestHandler<ApplyRectificationCommand, DataRightsRectificationDto>
{
    private readonly IApplicationDbContext _dbContext;

    public ApplyRectificationCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DataRightsRectificationDto> Handle(ApplyRectificationCommand request, CancellationToken cancellationToken)
    {

        var dataRightsRequest = await _dbContext.Set<DataRightsRequest>()
            .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken)
            ?? throw new InvalidOperationException("Data rights request not found.");
        // T112: the gate runs AFTER the load, because it is the REQUEST's institution being checked,
        // not merely the caller's role. Refusal and not-found are both thrown before anything is
        // mutated, so a caller out of scope changes nothing.
        DataRightsAuthorization.DemandRectificationAccess(request.Principal, dataRightsRequest);

        if (dataRightsRequest.Type != DataRightsRequestType.Rectification)
            throw new InvalidOperationException("Only rectification requests accept rectification records.");

        if (dataRightsRequest.Status != DataRightsRequestStatus.Approved)
            throw new InvalidOperationException("The request must be approved before rectifications can be applied.");

        var rectification = DataRightsRectification.Create(
            request.RequestId,
            request.TargetType,
            request.TargetId,
            request.FromValueJson,
            request.ToValueJson);

        rectification.MarkApplied(DateTime.UtcNow);

        _dbContext.Set<DataRightsRectification>().Add(rectification);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DataRightsRectificationDto(
            rectification.Id,
            rectification.RequestId,
            rectification.TargetType,
            rectification.TargetId,
            rectification.FromValueJson,
            rectification.ToValueJson,
            rectification.AppliedOn);
    }

}
