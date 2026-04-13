using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record UpdateDecisionPanelCommand(
    int PanelId,
    IReadOnlyList<DecisionPanelMemberInput> Members,
    ClaimsPrincipal Principal) : IRequest<DecisionPanelDetailDto>;

public sealed class UpdateDecisionPanelCommandValidator : AbstractValidator<UpdateDecisionPanelCommand>
{
    public UpdateDecisionPanelCommandValidator()
    {
        RuleFor(command => command.PanelId).GreaterThan(0);
        RuleFor(command => command.Members).NotEmpty();
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class UpdateDecisionPanelCommandHandler : IRequestHandler<UpdateDecisionPanelCommand, DecisionPanelDetailDto>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateDecisionPanelCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DecisionPanelDetailDto> Handle(UpdateDecisionPanelCommand request, CancellationToken cancellationToken)
    {
        CommitteeDecisionAuthorization.DemandPanelAdministration(request.Principal);

        var panel = await _dbContext.Set<DecisionPanel>()
            .Include(entity => entity.Members)
            .SingleOrDefaultAsync(entity => entity.Id == request.PanelId, cancellationToken)
            ?? throw new InvalidOperationException("The decision panel could not be found.");

        panel.Members.Clear();
        foreach (var member in request.Members)
        {
            panel.Members.Add(new DecisionPanelMember
            {
                UserId = member.UserId.Trim(),
                Role = member.Role
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DecisionPanelDetailDto(
            panel.Id,
            panel.Name,
            panel.Scope,
            panel.InstitutionId,
            panel.SpecialityId,
            panel.Members.Select(member => new DecisionPanelMemberDto(member.Id, member.UserId, member.Role)).ToArray());
    }
}
