namespace Wombat.Application.Features.Reporting;

public interface IPortfolioPdfService
{
    Task<PortfolioExportResult> GenerateAsync(PortfolioExportRequest request, CancellationToken cancellationToken);
}
