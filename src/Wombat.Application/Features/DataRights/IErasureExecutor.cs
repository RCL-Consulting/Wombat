using Wombat.Domain.DataRights;

namespace Wombat.Application.Features.DataRights;

public interface IErasureExecutor
{
    /// <summary>
    /// Erases the request's requester in one transaction, and says what it ended (T387, S6: the approval's result counts
    /// from it). The <see cref="DataRightsErasureRecord" /> it writes pairs the person's id, the request and the pseudonym.
    /// </summary>
    Task<ErasureOutcome> ExecuteAsync(
        DataRightsRequest request,
        string pseudonymSalt,
        CancellationToken cancellationToken);
}
