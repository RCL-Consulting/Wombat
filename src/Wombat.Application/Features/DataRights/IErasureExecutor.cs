using Wombat.Domain.DataRights;

namespace Wombat.Application.Features.DataRights;

public interface IErasureExecutor
{
    Task<DataRightsErasureRecord> ExecuteAsync(
        DataRightsRequest request,
        string pseudonymSalt,
        CancellationToken cancellationToken);
}
