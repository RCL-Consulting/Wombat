using Wombat.Application.Common.Email;

namespace Wombat.Infrastructure.Email;

/// <summary>
/// What this host's mail path tells Home (T400, flow 18, Q2, Q18): whether a mail server is set, and how many mails wait
/// in this host's queue. A singleton, registered for both hosts whether or not a mail server is set.
/// </summary>
/// <remarks>
/// The in-process half only. What became of each mail is in <c>EmailOutcomes</c>, which the worker writes
/// (<see cref="EmailOutcomeRecorder" />) and Home reads, so a restart keeps the last 24 hours and the other host's mail is
/// counted too (K20).
/// </remarks>
public sealed class EmailHealth : IEmailHealth
{
    private readonly EmailQueue _queue;

    public EmailHealth(bool isConfigured, EmailQueue queue)
    {
        IsConfigured = isConfigured;
        _queue = queue;
    }

    public bool IsConfigured { get; }

    // The queue is an unbounded channel, which counts what it holds. With no mail server nothing is queued: mail is written
    // to the log (LoggingEmailSender).
    public int QueueDepth => IsConfigured && _queue.Reader.CanCount ? _queue.Reader.Count : 0;
}
