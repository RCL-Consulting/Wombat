namespace Wombat.Application.Common.Email;

/// <summary>
/// What the mail path in this process can tell Home (T400, flow 18, Q2, Q18): whether a mail server is set
/// (<c>Email:SmtpHost</c>), and how many mails wait in this host's queue. Home's Mail line reads them beside the
/// outcomes table.
/// </summary>
/// <remarks>
/// The in-process half only. The last 24 hours, the last mail sent and the last drop are read from
/// <c>EmailOutcomes</c>, which either host writes, and never kept in memory: the host that draws Home is not always the
/// one that sent the mail (K20). Implemented and registered by Infrastructure for both hosts.
/// </remarks>
public interface IEmailHealth
{
    /// <summary>True when <c>Email:SmtpHost</c> is set, so mail is sent; false when it is written to the log instead.</summary>
    bool IsConfigured { get; }

    /// <summary>The mails waiting in this host's queue now; 0 when none, and when mail is not configured.</summary>
    int QueueDepth { get; }
}
