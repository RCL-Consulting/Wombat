using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.DataRights.Commands;

public sealed record ApplyRectificationCommand(
    Guid RequestId,
    string TargetType,
    Guid TargetId,
    string FromValueJson,
    string ToValueJson,
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
        DemandReviewAccess(request.Principal);

        var dataRightsRequest = await _dbContext.Set<DataRightsRequest>()
            .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken)
            ?? throw new InvalidOperationException("Data rights request not found.");

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

    private static void DemandReviewAccess(ClaimsPrincipal principal)
    {
        if (principal.IsInRole(WombatRoles.Administrator) ||
            principal.IsInRole(WombatRoles.SpecialityAdmin))
            return;

        throw new UnauthorizedAccessException("Only administrators and speciality admins may apply rectifications.");
    }
}
