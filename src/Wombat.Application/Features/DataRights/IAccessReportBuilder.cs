namespace Wombat.Application.Features.DataRights;

public interface IAccessReportBuilder
{
    Task<AccessExportResult> BuildAsync(string userId, CancellationToken cancellationToken);
}
