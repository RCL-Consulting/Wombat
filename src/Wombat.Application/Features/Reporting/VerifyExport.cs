using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Reporting;

namespace Wombat.Application.Features.Reporting;

public sealed record VerifyExportQuery(string ContentHash) : IRequest<PortfolioExportRecordDto?>;

public sealed class VerifyExportQueryHandler : IRequestHandler<VerifyExportQuery, PortfolioExportRecordDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public VerifyExportQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PortfolioExportRecordDto?> Handle(VerifyExportQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ContentHash))
        {
            return null;
        }

        var export = await _dbContext.Set<PortfolioExport>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                record => record.ContentHash == request.ContentHash,
                cancellationToken);

        if (export is null)
        {
            return null;
        }

        return new PortfolioExportRecordDto(
            export.Id,
            export.TraineeUserId,
            export.ExportedByUserId,
            export.ExportedOn,
            export.FilterFromDate,
            export.FilterToDate,
            export.ContentHash,
            export.FileName);
    }
}
