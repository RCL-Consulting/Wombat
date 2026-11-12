using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Email;

namespace Wombat.Infrastructure.Email;

/// <summary>
/// Writes one <see cref="EmailOutcome" /> for a mail the worker finished with (T400, flow 18, Q18, K3), on a scope of the
/// worker's own. Registered only where the worker runs (a mail server set), so a host that only logs its mail writes none.
/// </summary>
/// <remarks>
/// The row holds the moment, the kind and the redacted refusal only: never an address, a subject, a body, the mail's
/// reference or its tags (Q18). The worker catches and logs a recording that fails, so the next mail is still sent.
/// </remarks>
public sealed class EmailOutcomeRecorder
{
    private readonly IApplicationDbContext _dbContext;

    public EmailOutcomeRecorder(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RecordAsync(EmailOutcome outcome, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(outcome);

        _dbContext.Set<EmailOutcome>().Add(outcome);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
