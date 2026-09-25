using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email;

namespace Wombat.Architecture.Tests;

// Deliberately bad logging, for NoAddressInLogsTests to catch; nothing calls it. CA2254 would flag the interpolated
// template, which is the point of that specimen.
#pragma warning disable CA2254

/// <summary>
/// One log call for each way of putting an address in a log line that <see cref="NoAddressInLogsTests" /> must catch, and
/// seven it must pass. <c>The_scan_catches_each_way_a_specimen_logs_an_address</c> names what each one should be refused
/// for, so a check in the scan that stops working fails that test, not silently.
/// </summary>
internal static partial class LogAddressSpecimens
{
    private static readonly string s_adminEmail = "admin@example.test";

    public static void Placeholder(ILogger logger, string subject)
        => logger.LogInformation("Mail to {To} sent.", subject);

    public static void AlignedPlaceholder(ILogger logger, int id)
        => logger.LogInformation("Mail to {RespondentEmail,-20} sent.", id);

    public static void UserNamePlaceholder(ILogger logger, int id)
        => logger.LogInformation("Signed in as {UserName}.", id);

    public static void QuotedInTemplate(ILogger logger)
        => logger.LogWarning("Mail to admin@example.test failed.");

    public static void Getter(ILogger logger, EmailMessage message)
        => logger.LogInformation("Mail {Reference} sent.", message.To);

    public static void Parameter(ILogger logger, string respondentEmail)
        => logger.LogInformation("Mail {Reference} sent.", respondentEmail);

    public static void StaticField(ILogger logger)
        => logger.LogInformation("Mail {Reference} sent.", s_adminEmail);

    public static void Local(ILogger logger, EmailMessage message)
    {
        var recipientEmail = message.To.Trim();
        logger.LogInformation("Mail {Reference} sent.", recipientEmail);
    }

    public static void Literal(ILogger logger)
        => logger.LogInformation("Mail {Reference} sent.", "admin@example.test");

    public static void WholeMessage(ILogger logger, EmailMessage message)
        => logger.LogInformation("Mail {Mail} sent.", message);

    public static void Interpolated(ILogger logger, string reference)
        => logger.LogInformation($"Mail {reference} sent.");

    public static void Define()
        => _ = LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, nameof(Define)), "Mail to {Email} sent.");

    public static void Scope(ILogger logger, string email)
    {
        using (logger.BeginScope("Mailing {Recipient}", email))
        {
        }
    }

    public static async Task Hoisted(ILogger logger, Func<Task<string>> lookUp)
    {
        // Used after a second await, so the local lives in a field of the state machine.
        var email = await lookUp();
        await Task.Yield();
        logger.LogInformation("Mail {Reference} sent.", email);
    }

    public static Action Captured(ILogger logger, string toAddress)
        => () => logger.LogInformation("Mail {Reference} sent.", toAddress);

    public static void Direct(ILogger logger)
        => logger.Log(LogLevel.Information, new EventId(1), "state", null, static (state, _) => state);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Mail to {Email} sent.")]
    public static partial void GeneratedMessage(ILogger logger, string email);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Mail to {Email}: {A} {B} {C} {D} {E} {F}.")]
    public static partial void SevenValues(ILogger logger, string email, int a, int b, int c, int d, int e, int f);

    // A clean template, handed an address: the call to a [LoggerMessage] method and to a Define's delegate are log calls
    // too, and what they are given is judged as a LogInformation's values are (T282 review).
    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Mail {Reference} sent.")]
    public static partial void MailSent(ILogger logger, string reference);

    private static readonly Action<ILogger, string, Exception?> s_mailSent =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(5, nameof(s_mailSent)), "Mail {Reference} sent.");

    public static void GeneratedCalled(ILogger logger, EmailMessage message)
        => MailSent(logger, message.To);

    public static void DefinedInvoked(ILogger logger, EmailMessage message)
        => s_mailSent(logger, message.To, null);

    // The signed-in user's name, which Wombat sets to the address, read from the identity, a claim, or a look-up.
    public static void IdentityName(ILogger logger, ClaimsPrincipal principal)
        => logger.LogInformation("Signed in {Who}.", principal.Identity!.Name);

    public static void EmailClaim(ILogger logger, ClaimsPrincipal principal)
        => logger.LogInformation("Signed in {Who}.", principal.FindFirst(ClaimTypes.Email)!.Value);

    public static void LookedUp(ILogger logger, IAddressBook book, int userId)
        => logger.LogInformation("Mail {Reference} sent.", book.GetUserName(userId));

    // What is evaluated before the template, or outside the call.
    public static void InException(ILogger logger, EmailMessage message)
        => logger.LogError(new InvalidOperationException(message.To), "Mail failed.");

    public static void ValuesBuiltBeforehand(ILogger logger, EmailMessage message)
    {
        var values = new object[] { message.To };
        logger.LogInformation("Mail {Reference} sent.", values);
    }

    // A template chosen by a conditional, whose arm that names an address is not the one that falls through to the call.
    public static void ConditionalTemplate(ILogger logger, bool retry, int attempt)
        => logger.LogInformation(retry ? "Mail {Reference} sent." : "Mail to {To} sent.", attempt);

    // ─── Clean: none of these names an address ───────────────────────────────

    public static void Counts(
        ILogger logger, int noEmailCount, DateOnly to, IReadOnlyDictionary<DayOfWeek, int> skippedRecipients)
        => logger.LogInformation(
            "Skipped: no email address {NoEmailCount}, on Mondays {MondayCount}, window ends {ToDate}.",
            noEmailCount,
            skippedRecipients[DayOfWeek.Monday],
            to);

    public static void Tagged(ILogger logger, string reference, EmailMessage message)
        => logger.LogInformation(
            "Mail {Reference} (tags: {Tags}) sent: {Subject}.",
            reference,
            message.Tags is { Count: > 0 } ? string.Join(", ", message.Tags) : "(none)",
            message.Subject);

    public static void Failed(ILogger logger, Exception exception, string reference)
        => logger.LogError(exception, "Mail {Reference} failed.", reference);

    public static void ClaimNamed(ILogger logger)
        => logger.LogWarning("The provider sent no {Claim} claim.", "email");

    public static void Chosen(ILogger logger, bool retry, string reference)
        => logger.LogInformation(retry ? "Mail {Reference} retried." : "Mail {Reference} sent.", reference);

    public static void GeneratedClean(ILogger logger, string reference)
    {
        MailSent(logger, reference);
        s_mailSent(logger, reference, null);
    }
}

/// <summary>A look-up whose name says it returns an address, for <see cref="LogAddressSpecimens.LookedUp" />.</summary>
internal interface IAddressBook
{
    string GetUserName(int userId);
}
