using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <param name="DecisionBodyKey">
/// The College committee the new panel sits as (T131), or null for a general panel. Only an InstitutionalAdmin or a
/// global Administrator may name one (<see cref="CommitteeDecisionAuthorization.MaySetDecisionBody" />).
/// </param>
public sealed record CreateDecisionPanelCommand(
    string Name,
    DecisionPanelScope Scope,
    int? InstitutionId,
    int? SpecialityId,
    IReadOnlyList<DecisionPanelMemberInput> Members,
    ClaimsPrincipal Principal,
    string? DecisionBodyKey = null) : IRequest<DecisionPanelDetailDto>;

public sealed class CreateDecisionPanelCommandValidator : AbstractValidator<CreateDecisionPanelCommand>
{
    public CreateDecisionPanelCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Principal).NotNull();
        RuleFor(command => command.DecisionBodyKey).MaximumLength(DecisionBody.KeyMaxLength);
        // T165: each member once, exactly one chair, and the chair plus at least one other, so that no decision the panel
        // takes can be one person's.
        RuleFor(command => command.Members).MustBeAPanelsMembers();
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
    private readonly IUserAdministrationService _users;

    public CreateDecisionPanelCommandHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<DecisionPanelDetailDto> Handle(CreateDecisionPanelCommand request, CancellationToken cancellationToken)
    {
        // Who may manage panels at all, before anything is read: never someone who holds Trainee (T256).
        CommitteeDecisionAuthorization.DemandPanelAdministration(request.Principal);

        var institutionId = await ResolveInstitutionIdAsync(request, cancellationToken);
        var specialityId = request.Scope == DecisionPanelScope.Speciality ? request.SpecialityId : null;

        // A SpecialityAdmin creates panels for their own speciality only (T131 slice 3, T194 item 2).
        if (!await CommitteeDecisionAuthorization.MayAdministerPanelAsync(
                _dbContext, request.Principal, institutionId, request.Scope, specialityId, cancellationToken))
        {
            throw new UnauthorizedAccessException(CommitteeDecisionAuthorization.PanelOutOfScope);
        }

        // The College committee it sits as, if any (T131): a stricter right than creating the panel, asked with the scope,
        // before anything about the members or the body is read.
        if (DecisionBody.NormalizeKey(request.DecisionBodyKey) is not null &&
            !CommitteeDecisionAuthorization.MaySetDecisionBody(request.Principal, institutionId))
        {
            throw new UnauthorizedAccessException(CommitteeDecisionAuthorization.DecisionBodyNeedsInstitutionalAdmin);
        }

        // T165: each member must be someone who may sit (an active committee member at the panel's institution who is not
        // a trainee, T237), in any seat, the rule the picker lists by and a decision's attendance is held to. Before the
        // panel is built.
        await PanelSeat.DemandMembersAsync(_users, institutionId, request.Members, cancellationToken);

        // The body named, and its slot at this institution free (T131); null for a general panel. Reads only, so it runs
        // with the rest before anything is added.
        var body = await DecisionPanelBodies.DemandAsync(
            _dbContext, request.DecisionBodyKey, institutionId, specialityId, panelId: null, cancellationToken);

        var panel = new DecisionPanel
        {
            Name = request.Name.Trim(),
            Scope = request.Scope,
            // The panel carries its own institution regardless of scope (T091/T094): a Speciality-scoped
            // panel still belongs to the institution running that programme, and reviews only its trainees (T182).
            InstitutionId = institutionId,
            SpecialityId = specialityId,
            DecisionBodyKey = body?.Key,
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

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (body is not null)
        {
            // Another request tagged a panel in the same slot between the check above and this save, and the unique
            // index refused this one: say which panel, as the check would have, and keep the refused save underneath.
            var taken = await DecisionPanelBodies.TakenRefusalAsync(
                _dbContext, body, institutionId, specialityId, panelId: null, cancellationToken);
            if (taken is null)
            {
                throw;
            }

            throw new InvalidOperationException(taken, exception);
        }

        return DecisionPanelBodies.ToDetailDto(panel, body?.Name);
    }

    /// <summary>
    /// Which institution the new panel runs at. Every panel has one (T182).
    /// </summary>
    /// <remarks>
    /// Anyone but a global Administrator creates panels at their own institution: the form offers them no institution
    /// for a Speciality-scoped panel, and naming one other than their own is refused by the scope rule that follows.
    /// Until T182 only an InstitutionalAdmin was pinned (T094), so a SpecialityAdmin's panel carried no institution
    /// and could review nobody once reviews were held to the panel's institution. An Administrator belongs to no
    /// institution and must name one that exists, for every scope.
    /// </remarks>
    private async Task<int> ResolveInstitutionIdAsync(CreateDecisionPanelCommand request, CancellationToken cancellationToken)
    {
        if (!request.Principal.IsAdministrator())
        {
            return request.InstitutionId
                   ?? request.Principal.GetInstitutionId()
                   ?? throw new UnauthorizedAccessException(CommitteeDecisionAuthorization.PanelOutOfScope);
        }

        if (request.InstitutionId is not int institutionId)
        {
            throw new InvalidOperationException("Choose the institution that runs this panel.");
        }

        if (!await _dbContext.Set<Institution>().AnyAsync(entity => entity.Id == institutionId, cancellationToken))
        {
            throw new InvalidOperationException("The institution could not be found.");
        }

        return institutionId;
    }
}
