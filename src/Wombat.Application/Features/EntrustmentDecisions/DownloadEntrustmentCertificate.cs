using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Application.Features.EntrustmentDecisions;

/// <summary>
/// A decision's STAR certificate, or null when the caller may not have it or the id names nothing: the two are one
/// answer, so an id cannot be tested for existence (404, not 403). (T183)
/// </summary>
/// <remarks>
/// <para>
/// The certificate is a rendering of the decision, so it is read on the ladder every trainee read uses
/// (<see cref="TraineeScopeResolver.MayReadAsync" />): the trainee it is about, always; a global Administrator; and
/// whoever oversees that trainee, at the trainee's own institution. Beside that ladder, the chair who issued it, because
/// that is a claim on this one decision, not on the trainee; and it is read from the decision
/// (<see cref="EntrustmentDecisionAuthorization.IsIssuer" />), the same arm revoking has.
/// </para>
/// <para>
/// Nothing is read from the issuing panel's CURRENT members. A panel's members can be replaced after the fact, so
/// "sat on the issuing panel" would hand the certificate to whoever was put on it since. A member who belongs there loses
/// nothing by it: they are a CommitteeMember of the institution whose trainees the panel reviews, and read the
/// certificate as an overseer.
/// </para>
/// <para>
/// Until T183 every InstitutionalAdmin, SpecialityAdmin, SubSpecialityAdmin and Coordinator in the country could
/// download every trainee's certificate by role alone.
/// </para>
/// </remarks>
public sealed record DownloadEntrustmentCertificateCommand(
    int DecisionId,
    ClaimsPrincipal Principal) : IRequest<EntrustmentCertificateResult?>;

public sealed class DownloadEntrustmentCertificateCommandValidator : AbstractValidator<DownloadEntrustmentCertificateCommand>
{
    public DownloadEntrustmentCertificateCommandValidator()
    {
        RuleFor(command => command.DecisionId).GreaterThan(0);
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class DownloadEntrustmentCertificateCommandHandler
    : IRequestHandler<DownloadEntrustmentCertificateCommand, EntrustmentCertificateResult?>
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

    public async Task<EntrustmentCertificateResult?> Handle(DownloadEntrustmentCertificateCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Principal);

        var decision = await _dbContext.Set<EntrustmentDecision>()
            .AsNoTracking()
            .Where(d => d.Id == request.DecisionId)
            .Select(d => new { d.Id, d.TraineeUserId, d.IssuedByChairUserId })
            .SingleOrDefaultAsync(cancellationToken);

        if (decision is null ||
            !(EntrustmentDecisionAuthorization.IsIssuer(request.Principal, decision.IssuedByChairUserId) ||
              await TraineeScopeResolver.MayReadAsync(_dbContext, request.Principal, decision.TraineeUserId, cancellationToken)))
        {
            return null;
        }

        return await _pdfService.GenerateAsync(new EntrustmentCertificateRequest(decision.Id), cancellationToken);
    }
}
