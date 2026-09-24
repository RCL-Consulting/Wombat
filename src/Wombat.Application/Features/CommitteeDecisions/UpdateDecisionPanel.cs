using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Institutions;

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
        // T165: the rules a new panel is held to. Before T165 an update checked only that the list was not empty, so a
        // valid panel could be cut down to its chair alone.
        RuleFor(command => command.Members).MustBeAPanelsMembers();
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class UpdateDecisionPanelCommandHandler : IRequestHandler<UpdateDecisionPanelCommand, DecisionPanelDetailDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public UpdateDecisionPanelCommandHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<DecisionPanelDetailDto> Handle(UpdateDecisionPanelCommand request, CancellationToken cancellationToken)
    {
        CommitteeDecisionAuthorization.DemandPanelAdministration(request.Principal);

        var panel = await _dbContext.Set<DecisionPanel>()
            .Include(entity => entity.Members)
            .SingleOrDefaultAsync(entity => entity.Id == request.PanelId, cancellationToken);

        // Anyone but an Administrator is refused an unknown panel exactly as another institution's, so the refusal does
        // not say which panel ids exist (T194 item 1, applied where T131 touches).
        if (panel is null)
        {
            throw request.Principal.IsAdministrator()
                ? new InvalidOperationException("The decision panel could not be found.")
                : new UnauthorizedAccessException(CommitteeDecisionAuthorization.PanelOutOfScope);
        }

        // Every panel carries its institution now (T182). This check used to be skipped for a panel without one, which
        // let any panel administrator in the country rewrite that panel's members. Its speciality is read too (T131
        // slice 3): a Surgery SpecialityAdmin could name the chair of the Paediatrics panel, and so of the neonatal CCC
        // that decides EPAs 4 and 5 for every paediatric trainee there. Before the member list is touched: the audit
        // pipeline saves the request's context from its catch.
        if (!await CommitteeDecisionAuthorization.MayAdministerPanelAsync(
                _dbContext, request.Principal, panel.InstitutionId, panel.Scope, panel.SpecialityId, cancellationToken))
        {
            throw new UnauthorizedAccessException(CommitteeDecisionAuthorization.PanelOutOfScope);
        }

        // T165: the panel as saved holds only people who may sit on it now, the members it already had included: an
        // erased, deactivated or departed member is taken off, not carried over. Before the members are touched.
        await PanelSeat.DemandMembersAsync(_users, panel.InstitutionId, request.Members, cancellationToken);

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

        return DecisionPanelBodies.ToDetailDto(
            panel, await DecisionPanelBodies.NameOfAsync(_dbContext, panel.DecisionBodyKey, cancellationToken));
    }
}
