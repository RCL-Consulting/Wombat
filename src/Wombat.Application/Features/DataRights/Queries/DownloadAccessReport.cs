using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.DataRights.Queries;

public sealed record DownloadAccessReportQuery(
    Guid RequestId,
    ClaimsPrincipal Principal) : IRequest<AccessExportResult>;

public sealed class DownloadAccessReportQueryValidator : AbstractValidator<DownloadAccessReportQuery>
{
    public DownloadAccessReportQueryValidator()
    {
        RuleFor(query => query.RequestId).NotEmpty();
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class DownloadAccessReportQueryHandler : IRequestHandler<DownloadAccessReportQuery, AccessExportResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAccessReportBuilder _reportBuilder;

    public DownloadAccessReportQueryHandler(IApplicationDbContext dbContext, IAccessReportBuilder reportBuilder)
    {
        _dbContext = dbContext;
        _reportBuilder = reportBuilder;
    }

    public async Task<AccessExportResult> Handle(DownloadAccessReportQuery request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Set<DataRightsRequest>()
            .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken)
            ?? throw new InvalidOperationException("Data rights request not found.");

        if (entity.Type is not (DataRightsRequestType.Access or DataRightsRequestType.Export))
            throw new InvalidOperationException("This request is not an access or export request.");

        if (entity.Status != DataRightsRequestStatus.Completed)
            throw new InvalidOperationException("The request must be approved and completed before the report can be downloaded.");

        DemandAccess(request.Principal, entity);

        return await _reportBuilder.BuildAsync(entity.RequesterUserId, cancellationToken);
    }

    private static void DemandAccess(ClaimsPrincipal principal, DataRightsRequest entity)
    {
        if (principal.IsInRole(WombatRoles.Administrator) ||
            principal.IsInRole(WombatRoles.Coordinator))
            return;

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.Equals(userId, entity.RequesterUserId, StringComparison.Ordinal))
            return;

        throw new UnauthorizedAccessException("You are not authorized to download this report.");
    }
}
