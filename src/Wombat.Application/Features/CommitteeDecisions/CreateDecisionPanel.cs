using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record CreateDecisionPanelCommand(
    string Name,
    DecisionPanelScope Scope,
    int? InstitutionId,
    int? SpecialityId,
    IReadOnlyList<DecisionPanelMemberInput> Members,
    ClaimsPrincipal Principal) : IRequest<DecisionPanelDetailDto>;

public sealed class CreateDecisionPanelCommandValidator : AbstractValidator<CreateDecisionPanelCommand>
{
    public CreateDecisionPanelCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Principal).NotNull();
        RuleFor(command => command.Members).NotEmpty();
        RuleForEach(command => command.Members).ChildRules(member =>
        {
            member.RuleFor(item => item.UserId).NotEmpty();
        });
        RuleFor(command => command)
            .Must(command => command.Scope != DecisionPanelScope.Institution || command.InstitutionId.HasValue)
            .WithMessage("Institution-scoped panels require an institution.");
        RuleFor(command => command)
            .Must(command => command.Scope != DecisionPanelScope.Speciality || command.SpecialityId.HasValue)
            .WithMessage("Speciality-scoped panels require a speciality.");
    }
}

public sealed class CreateDecisionPanelCommandHandler : IRequestHandler<CreateDecisionPanelCommand, DecisionPanelDetailDto>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateDecisionPanelCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DecisionPanelDetailDto> Handle(CreateDecisionPanelCommand request, CancellationToken cancellationToken)
    {
        CommitteeDecisionAuthorization.DemandPanelAdministration(request.Principal);

        var panel = new DecisionPanel
        {
            Name = request.Name.Trim(),
            Scope = request.Scope,
            InstitutionId = request.Scope == DecisionPanelScope.Institution ? request.InstitutionId : null,
            SpecialityId = request.Scope == DecisionPanelScope.Speciality ? request.SpecialityId : null,
            CreatedOn = DateTime.UtcNow,
            Members = request.Members
                .Select(member => new DecisionPanelMember
                {
                    UserId = member.UserId.Trim(),
                    Role = member.Role
                })
                .ToArray()
        };

        _dbContext.Set<DecisionPanel>().Add(panel);
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
