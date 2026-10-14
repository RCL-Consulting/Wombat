using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Audit;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Application.Features.EntrustmentDecisions;

/// <remarks>
/// <c>Reason</c> is redacted from the audit summary. The AuditPipelineBehavior audits every request
/// whose type name ends in "Command" and AuditPayloadSerializer writes its properties into
/// SummaryJson; withdrawing an entrustment is the most damaging thing that can be written about a
/// trainee and the reason text usually says why — a concern, an incident, a performance judgement.
/// The decision id and the actor stay in the clear, so the audit row still proves who revoked what
/// and when; the narrative is read from the decision, under its own access control. (T101)
/// </remarks>
public sealed record RevokeEntrustmentDecisionCommand(
    int DecisionId,
    [property: Redact] string Reason,
    ClaimsPrincipal Principal) : IRequest<EntrustmentDecisionDto>
{
    /// <summary>The longest reason a revocation keeps, in characters, as the register's counter says (T366).</summary>
    public const int MaxReasonLength = 1000;

    /// <summary>A reason over <see cref="MaxReasonLength" />, refused under the box (T366, R3, D2).</summary>
    public const string ReasonTooLong = "Give the reason in 1,000 characters or fewer.";
}

public sealed class RevokeEntrustmentDecisionCommandValidator : AbstractValidator<RevokeEntrustmentDecisionCommand>
{
    public RevokeEntrustmentDecisionCommandValidator()
    {
        RuleFor(command => command.DecisionId).GreaterThan(0);
        // T366, R3: Wombat's words, as the register says them under the box, never the validator's default.
        RuleFor(command => command.Reason)
            .NotEmpty()
            .MaximumLength(RevokeEntrustmentDecisionCommand.MaxReasonLength).WithMessage(RevokeEntrustmentDecisionCommand.ReasonTooLong);
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class RevokeEntrustmentDecisionCommandHandler : IRequestHandler<RevokeEntrustmentDecisionCommand, EntrustmentDecisionDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;
    private readonly AccountNotices _notices;

    public RevokeEntrustmentDecisionCommandHandler(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        AccountNotices notices)
    {
        _dbContext = dbContext;
        _users = users;
        _notices = notices;
    }

    /// <remarks>
    /// Every check runs before <see cref="EntrustmentDecision.Revoke" />, the one mutation: the audit pipeline saves the
    /// request's DbContext from its catch, so a refusal after it would commit the revocation it refused. A decision id
    /// that names nothing is refused exactly as one out of the caller's scope is (T183); only an Administrator, who may
    /// revoke every decision, is told plainly that it does not exist.
    /// </remarks>
    public async Task<EntrustmentDecisionDto> Handle(RevokeEntrustmentDecisionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Principal);

        var decision = await _dbContext.Set<EntrustmentDecision>()
            .Include(d => d.Epa)
            .Include(d => d.AuthorisedLevel)
            .Include(d => d.EvidenceLinks)
            .SingleOrDefaultAsync(d => d.Id == request.DecisionId, cancellationToken);

        if (decision is null && EntrustmentDecisionAuthorization.MayRevokeEveryDecision(request.Principal))
        {
            throw new InvalidOperationException(EntrustmentRevocationRefusals.NotFound);
        }

        if (decision is null)
        {
            throw new UnauthorizedAccessException(EntrustmentDecisionAuthorization.DecisionNotRevocableByCaller);
        }

        await EntrustmentDecisionAuthorization.DemandRevocationAccessAsync(
            _dbContext, request.Principal, decision, cancellationToken);
        var actorUserId = EntrustmentDecisionAuthorization.GetRequiredUserId(request.Principal);

        decision.Revoke(request.Reason, actorUserId, DateTime.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // T367: the trainee is told, after the save, so a refused revoke mails nobody; a failed send is logged and the
        // revocation stands. The mail names the reviser, never the reason (AccountNotices).
        await _notices.SendAsync(decision.TraineeUserId, "entrustment-revoked", async trainee =>
        {
            var names = await _users.GetDisplayNamesAsync([actorUserId], CancellationToken.None);
            return EntrustmentDecisionRevokedEmail.Build(
                trainee.Email,
                decision.Epa?.Code ?? string.Empty,
                decision.Epa?.Title ?? string.Empty,
                decision.AuthorisedLevel?.Label ?? string.Empty,
                ProgrammeCalendar.DateOf(decision.RevokedOn!.Value),
                names.TryGetValue(actorUserId, out var name) ? name : null,
                _notices.LinkTo("/portfolio/authorisations"));
        });

        return decision.ToDto();
    }
}

/// <summary>
/// Which of a revoke's failures are its own refusals, written in Wombat's words for the person who pressed Revoke, and
/// which are unexpected (T366, M4; T338). The register's dialog shows a refusal under the reason, the reason kept, and
/// any other failure as a fixed sentence, logged: an exception's own text never reaches the page.
/// </summary>
/// <remarks>
/// The decision refuses a second revoke with a plain <see cref="InvalidOperationException" />
/// (<see cref="EntrustmentDecision.Revoke" />), as the handler refuses an unknown id to an Administrator, so a refusal is
/// told from the framework's own failures by its words. <c>EntrustmentDecisionHandlersTests</c> holds the words to what
/// the decision and the handler throw.
/// </remarks>
public static class EntrustmentRevocationRefusals
{
    /// <summary>A second tab revoked it first, or it has expired or been superseded since the list was read.</summary>
    public const string NotActive = "Only active entrustment decisions can be revoked.";

    /// <summary>An unknown id, told plainly only to an Administrator, who may revoke every decision.</summary>
    public const string NotFound = "The entrustment decision could not be found.";

    /// <summary>
    /// Whether <paramref name="exception" /> is the revoke's own refusal: the validator's, a scope refusal
    /// (<see cref="UnauthorizedAccessException" />, in the handler's words), or one of the two above.
    /// </summary>
    public static bool IsRefusal(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception switch
        {
            ValidationException or UnauthorizedAccessException => true,
            InvalidOperationException => exception.Message is NotActive or NotFound,
            _ => false
        };
    }
}
