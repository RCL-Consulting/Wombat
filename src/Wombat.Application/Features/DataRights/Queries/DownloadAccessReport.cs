using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.DataRights;

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

        // Authorize BEFORE the state checks, so a caller who may not have this request cannot learn
        // its type or whether it has completed.
        DataRightsAuthorization.DemandExportAccess(request.Principal, entity);

        if (entity.Type is not (DataRightsRequestType.Access or DataRightsRequestType.Export))
            throw new InvalidOperationException("This request is not an access or export request.");

        if (entity.Status != DataRightsRequestStatus.Completed)
            throw new InvalidOperationException("The request must be approved and completed before the report can be downloaded.");

        return await _reportBuilder.BuildAsync(entity.RequesterUserId, cancellationToken);
    }

}
