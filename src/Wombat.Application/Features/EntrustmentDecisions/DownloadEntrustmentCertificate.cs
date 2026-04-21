using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.EntrustmentDecisions;

public sealed record DownloadEntrustmentCertificateCommand(
    int DecisionId,
    ClaimsPrincipal Principal) : IRequest<EntrustmentCertificateResult>;

public sealed class DownloadEntrustmentCertificateCommandValidator : AbstractValidator<DownloadEntrustmentCertificateCommand>
{
    public DownloadEntrustmentCertificateCommandValidator()
    {
        RuleFor(command => command.DecisionId).GreaterThan(0);
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class DownloadEntrustmentCertificateCommandHandler
    : IRequestHandler<DownloadEntrustmentCertificateCommand, EntrustmentCertificateResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IEntrustmentCertificatePdfService _pdfService;

    public DownloadEntrustmentCertificateCommandHandler(
        IApplicationDbContext dbContext,
        IEntrustmentCertificatePdfService pdfService)
    {
        _dbContext = dbContext;
        _pdfService = pdfService;
    }

    public async Task<EntrustmentCertificateResult> Handle(DownloadEntrustmentCertificateCommand request, CancellationToken cancellationToken)
    {
        var decision = await _dbContext.Set<EntrustmentDecision>()
            .AsNoTracking()
            .SingleOrDefaultAsync(d => d.Id == request.DecisionId, cancellationToken)
            ?? throw new InvalidOperationException("The entrustment decision could not be found.");

        await DemandCertificateAccessAsync(request.Principal, decision, cancellationToken);

        return await _pdfService.GenerateAsync(new EntrustmentCertificateRequest(decision.Id), cancellationToken);
    }

    private async Task DemandCertificateAccessAsync(ClaimsPrincipal principal, EntrustmentDecision decision, CancellationToken cancellationToken)
    {
        if (principal.IsInRole(WombatRoles.Administrator) ||
            principal.IsInRole(WombatRoles.InstitutionalAdmin) ||
            principal.IsInRole(WombatRoles.SpecialityAdmin) ||
            principal.IsInRole(WombatRoles.SubSpecialityAdmin) ||
            principal.IsInRole(WombatRoles.Coordinator))
        {
            return;
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("The current user identifier is missing.");

        if (string.Equals(userId, decision.TraineeUserId, StringComparison.Ordinal))
        {
            return;
        }

        var review = await _dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .Include(r => r.Panel)
                .ThenInclude(p => p.Members)
            .SingleOrDefaultAsync(r => r.Id == decision.IssuedByCommitteeReviewId, cancellationToken);

        if (review is not null && review.Panel.Members.Any(m =>
            string.Equals(m.UserId, userId, StringComparison.Ordinal) &&
            m.Role is DecisionPanelMemberRole.Chair or DecisionPanelMemberRole.Member))
        {
            return;
        }

        throw new UnauthorizedAccessException("You are not authorised to download this entrustment certificate.");
    }
}
