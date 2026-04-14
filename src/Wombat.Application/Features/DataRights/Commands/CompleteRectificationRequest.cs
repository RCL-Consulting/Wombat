using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.DataRights.Commands;

public sealed record CompleteRectificationRequestCommand(
    Guid RequestId,
    ClaimsPrincipal Principal) : IRequest<DataRightsRequestDto>;

public sealed class CompleteRectificationRequestCommandValidator : AbstractValidator<CompleteRectificationRequestCommand>
{
    public CompleteRectificationRequestCommandValidator()
    {
        RuleFor(command => command.RequestId).NotEmpty();
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class CompleteRectificationRequestCommandHandler : IRequestHandler<CompleteRectificationRequestCommand, DataRightsRequestDto>
{
    private readonly IApplicationDbContext _dbContext;

    public CompleteRectificationRequestCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DataRightsRequestDto> Handle(CompleteRectificationRequestCommand request, CancellationToken cancellationToken)
    {
        DemandReviewAccess(request.Principal);

        var entity = await _dbContext.Set<DataRightsRequest>()
            .Include(r => r.Rectifications)
            .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken)
            ?? throw new InvalidOperationException("Data rights request not found.");

        if (entity.Type != DataRightsRequestType.Rectification)
            throw new InvalidOperationException("Only rectification requests can be completed this way.");

        if (!entity.Rectifications.Any())
            throw new InvalidOperationException("At least one rectification must be applied before completing the request.");

        entity.Complete(DateTime.UtcNow);
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
            principal.IsInRole(WombatRoles.SpecialityAdmin))
            return;

        throw new UnauthorizedAccessException("Only administrators and speciality admins may complete rectification requests.");
    }
}
