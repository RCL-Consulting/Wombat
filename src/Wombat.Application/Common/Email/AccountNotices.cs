using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;

namespace Wombat.Application.Common.Email;

/// <summary>
/// The one way a command sends a one-off notice to one account: a STAR revoked (T367), an admission (T373), an MSF report
/// released (T369), and a data-rights request decided or completed (T319, built in T387). A notice is not a digest.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Call it only after the command's save has committed: a refused command mails nobody, and nothing that can fail
/// runs after the mutation (T201).</item>
/// <item>It goes to the account (<see cref="IUserAdministrationService.GetByIdAsync" />), and skips one that is
/// deactivated (an administrator's lock, or an erasure), as T274 asks of the expiry notices. It does not skip an account
/// that opted out of digests: a one-off notice still arrives.</item>
/// <item>A failed send is logged and never fails the command, which has already committed. It is sent with no
/// cancellation, as an invitation's is, so a closed circuit does not lose it.</item>
/// <item>A notice carries no free text written about the person (no revocation reason, no decision note): mail is
/// outside Wombat's access control. It says what happened, when and by whom, and links to where it can be read. One
/// exception to "by whom": the data-rights decision notice names no decider (T387, Q5, M3), as My data rights does not.</item>
/// <item>An erasure's confirmation cannot go through <see cref="SendAsync" />: the erased account is deactivated and has
/// no address. It goes through <see cref="SendToAddressAsync" />, to the address read before the erasure ran (T387, F4).</item>
/// </list>
/// </remarks>
public sealed class AccountNotices
{
    private readonly IUserAdministrationService _users;
    private readonly IEmailSender _emailSender;
    private readonly WombatOptions _options;
    private readonly ILogger<AccountNotices> _logger;

    public AccountNotices(
        IUserAdministrationService users,
        IEmailSender emailSender,
        IOptions<WombatOptions> options,
        ILogger<AccountNotices> logger)
    {
        _users = users;
        _emailSender = emailSender;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>A page of the web app, under the configured base URL, as a notice links to it.</summary>
    public string LinkTo(string path) => $"{(_options.BaseUrl ?? string.Empty).TrimEnd('/')}{path}";

    /// <summary>
    /// Sends the notice <paramref name="build" /> writes for <paramref name="userId" />'s account. True when it was handed
    /// to the mail sender; false when the account is missing, deactivated or has no address, or the send failed.
    /// </summary>
    /// <param name="kind">What the notice is, for the log line a skip or a failure writes.</param>
    public async Task<bool> SendAsync(
        string userId,
        string kind,
        Func<UserIdentityDetails, Task<EmailMessage>> build)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(build);

        try
        {
            var account = await _users.GetByIdAsync(userId, CancellationToken.None);
            if (account is null || account.IsDeactivated || string.IsNullOrWhiteSpace(account.Email))
            {
                _logger.LogInformation(
                    "The {NoticeKind} notice to user {UserId} was not sent: the account is missing, deactivated or has no address.",
                    kind, userId);
                return false;
            }

            await _emailSender.SendAsync(await build(account), CancellationToken.None);
            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "The {NoticeKind} notice to user {UserId} could not be sent.", kind, userId);
            return false;
        }
    }

    /// <summary>
    /// Sends the notice <paramref name="build" /> writes to <paramref name="address" />, read by the caller before the
    /// command changed the account: the one notice whose account is gone by the time it is sent, an erasure's
    /// confirmation (T387, F4). True when it was handed to the mail sender; false for no address, or a failed send. The
    /// address is never logged (<c>NoAddressInLogsTests</c>): the log names <paramref name="kind" /> and
    /// <paramref name="userId" /> alone.
    /// </summary>
    public async Task<bool> SendToAddressAsync(
        string? address,
        string userId,
        string kind,
        Func<string, EmailMessage> build)
    {
        ArgumentNullException.ThrowIfNull(build);

        if (string.IsNullOrWhiteSpace(address))
        {
            _logger.LogInformation(
                "The {NoticeKind} notice to user {UserId} was not sent: no address was read before the change.", kind, userId);
            return false;
        }

        try
        {
            await _emailSender.SendAsync(build(address), CancellationToken.None);
            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "The {NoticeKind} notice to user {UserId} could not be sent.", kind, userId);
            return false;
        }
    }
}
