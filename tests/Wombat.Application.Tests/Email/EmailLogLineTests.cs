using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email;
using Wombat.Infrastructure.Email;

namespace Wombat.Application.Tests.Email;

/// <summary>
/// No log line about a mail carries an address it is sent to: each names the mail by its tags and a reference drawn for
/// that mail alone (T282).
/// </summary>
/// <remarks>
/// Until T282 the worker logged "Email to {To}" on every send, retry and drop, beside tags that carry <c>campaign:N</c> on
/// an MSF mail, so the log named every respondent of every campaign, and its reminders said who had not yet answered.
/// Each test reads every part of every line that a sink could write: the rendered message, each structured value, and
/// the exception's full text, because a mail server that refuses a recipient usually quotes the address in its reply.
/// </remarks>
public sealed class EmailLogLineTests
{
    private const string Respondent = "nurse-1@example.test";
    private const string CopiedTo = "ward-7@example.test";
    private const string Tags = "msf-invite, campaign:1";

    [Fact]
    public async Task EveryLineAboutOneMail_CarriesItsReferenceAndTags_AndNeverItsAddresses()
    {
        var lines = new List<LogLine>();
        var queue = new EmailQueue();
        var smtp = new ScriptedSmtp(failures: 1);
        var sender = new QueuedEmailSender(queue, new LogRecorder<QueuedEmailSender>(lines));

        await sender.SendAsync(Mail(Respondent, cc: CopiedTo));
        queue.Writer.Complete();
        await Worker(queue, smtp, lines).ExecutePublicAsync(FiveSeconds());

        lines.Select(line => (line.Level, line.Template)).Should().Equal(
            (LogLevel.Debug, "Email {Reference} (tags: {Tags}) queued."),
            (LogLevel.Warning, "Email {Reference} (tags: {Tags}) failed (attempt {Attempt}/{MaxRetries}). Retrying in {Delay}s."),
            (LogLevel.Information, "Email {Reference} (tags: {Tags}) sent on attempt {Attempt}."));

        var reference = lines[0].Values["Reference"].Should().BeOfType<string>().Which;
        reference.Should().MatchRegex("^[0-9a-f]{12}$");
        lines.Should().AllSatisfy(line =>
        {
            line.Values["Reference"].Should().Be(reference, "one reference follows a mail from the queue to its outcome");
            line.Values["Tags"].Should().Be(Tags);
        });
        NoLineNames(lines, Respondent, CopiedTo);
    }

    [Fact]
    public async Task AMailTheServerRefuses_IsLoggedWithTheServersReason_LessTheAddressItQuoted()
    {
        var lines = new List<LogLine>();
        var queue = new EmailQueue();

        // Postfix's reply to a mistyped domain, as MailKit makes it the exception's message; in capitals the second time,
        // as a server may echo it.
        var smtp = new ScriptedSmtp(failures: int.MaxValue, reply: attempt => attempt == 1
            ? $"5.1.2 <{Respondent}>: Recipient address rejected: Domain not found"
            : $"5.1.2 <{Respondent.ToUpperInvariant()}>: Recipient address rejected: Domain not found");

        await queue.Writer.WriteAsync(new QueuedEmail(Mail(Respondent)));
        queue.Writer.Complete();
        await Worker(queue, smtp, lines).ExecutePublicAsync(FiveSeconds());

        var dropped = lines.Should().ContainSingle(line => line.Level == LogLevel.Error).Which;
        dropped.Template.Should().Be("Email {Reference} (tags: {Tags}) failed after {MaxRetries} attempts. Message dropped.");
        dropped.Exception.Should().NotBeNull();
        dropped.Exception!.Message.Should().Be("5.1.2 <[recipient]>: Recipient address rejected: Domain not found");
        dropped.Exception.ToString().Should()
            .Contain("System.InvalidOperationException: 5.1.2 <[recipient]>: Recipient address rejected: Domain not found",
                "the log keeps what was thrown and the server's reason")
            .And.Contain(nameof(ScriptedSmtp), "and where it was thrown");
        lines.Where(line => line.Level == LogLevel.Warning).Should().HaveCount(EmailWorker.MaxRetries - 1)
            .And.AllSatisfy(line => line.Exception!.Message.Should().Contain("[recipient]"));
        NoLineNames(lines, Respondent);
    }

    [Fact]
    public async Task TheMailCutOffByAStop_AndEveryMailStillQueued_AreNamedByReference()
    {
        var lines = new List<LogLine>();
        var queue = new EmailQueue();
        var smtp = new ScriptedSmtp(failures: 0, hangs: true);
        using var stopping = new CancellationTokenSource();

        var inFlight = new QueuedEmail(Mail(Respondent));
        var queued = new QueuedEmail(Mail("nurse-2@example.test"));
        await queue.Writer.WriteAsync(inFlight);
        await queue.Writer.WriteAsync(queued);

        var running = Worker(queue, smtp, lines).ExecutePublicAsync(stopping.Token);
        await smtp.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await stopping.CancelAsync();
        await running.WaitAsync(TimeSpan.FromSeconds(5));

        lines.Select(line => (line.Template, line.Values["Reference"])).Should().Equal(
            ("Email {Reference} (tags: {Tags}) cut off by the app's shutdown; whether it was sent is not known.", inFlight.Reference),
            ("Email {Reference} (tags: {Tags}) abandoned unsent: the app is shutting down.", queued.Reference));
        NoLineNames(lines, Respondent, "nurse-2@example.test");
    }

    [Fact]
    public async Task AMailWhoseRetryAStopCutsShort_IsNamedByReference()
    {
        var lines = new List<LogLine>();
        var queue = new EmailQueue();
        using var stopping = new CancellationTokenSource();
        var mail = new QueuedEmail(Mail(Respondent));
        await queue.Writer.WriteAsync(mail);

        var worker = Worker(queue, new ScriptedSmtp(failures: int.MaxValue), lines, retryDelay: _ =>
        {
            // The host stops while the worker waits to retry.
            stopping.Cancel();
            return TimeSpan.FromMinutes(5);
        });
        await worker.ExecutePublicAsync(stopping.Token).WaitAsync(TimeSpan.FromSeconds(5));

        lines.Last().Template.Should().Be("Email {Reference} (tags: {Tags}) abandoned after attempt {Attempt}: the app is shutting down.");
        lines.Should().AllSatisfy(line => line.Values["Reference"].Should().Be(mail.Reference));
        NoLineNames(lines, Respondent);
    }

    [Fact]
    public async Task AnOutcomeThatCannotBeRecorded_IsLoggedLessTheAddress()
    {
        var lines = new List<LogLine>();
        var queue = new EmailQueue();
        var mail = new QueuedEmail(Mail(Respondent) with { DeliveryKey = "msf-link:1:first" });
        await queue.Writer.WriteAsync(mail);
        queue.Writer.Complete();

        var observer = new ThrowingObserver($"Could not record the outcome for {Respondent}.");
        await Worker(queue, new ScriptedSmtp(failures: 0), lines, observer: observer).ExecutePublicAsync(FiveSeconds());

        var failed = lines.Should().ContainSingle(line => line.Level == LogLevel.Error).Which;
        failed.Template.Should().Be("The outcome of email {Reference} (tags: {Tags}, sent: {Sent}) could not be recorded.");
        failed.Values["Reference"].Should().Be(mail.Reference);
        failed.Exception!.Message.Should().Be("Could not record the outcome for [recipient].");
        NoLineNames(lines, Respondent);
    }

    /// <summary>
    /// The "sent" line is written once the send is over, outside it: a logger that threw inside it would count a mail the
    /// server took as a failed attempt, and send it again. (Until T282 that line was MailKitEmailSender's, inside it.)
    /// </summary>
    [Fact]
    public async Task ALoggerThatFailsOnTheSentLine_DoesNotSendTheMailAgain()
    {
        var queue = new EmailQueue();
        var smtp = new ScriptedSmtp(failures: 0);
        await queue.Writer.WriteAsync(new QueuedEmail(Mail(Respondent)));
        queue.Writer.Complete();

        var worker = new EmailWorker(
            queue,
            Services(smtp).GetRequiredService<IServiceScopeFactory>(),
            new LogRecorder<EmailWorker>([], failAt: LogLevel.Information),
            _ => TimeSpan.Zero);
        try
        {
            await worker.ExecutePublicAsync(FiveSeconds());
        }
        catch (InvalidOperationException)
        {
            // What the worker does about a logger that fails is not this test's business; that it sent once is.
        }

        smtp.Attempts.Should().Be(1);
    }

    /// <summary>
    /// A reference follows one mail, not one person: a reference that were the same on every mail to one address would
    /// join a respondent's invitation and reminders to the mail they get as a user of the app, and name them as surely.
    /// </summary>
    [Fact]
    public async Task TwoMailsToOnePerson_AreGivenTwoReferences()
    {
        var lines = new List<LogLine>();
        var queue = new EmailQueue();
        var sender = new QueuedEmailSender(queue, new LogRecorder<QueuedEmailSender>(lines));

        await sender.SendAsync(Mail(Respondent));
        await sender.SendAsync(Mail(Respondent));

        lines.Select(line => line.Values["Reference"]).Distinct().Should().HaveCount(2);
    }

    [Fact]
    public async Task TheStubSender_NamesTheMailByReferenceAndTags_NotItsAddress()
    {
        var lines = new List<LogLine>();

        await new LoggingEmailSender(new LogRecorder<LoggingEmailSender>(lines)).SendAsync(Mail(Respondent, cc: CopiedTo));

        var line = lines.Should().ContainSingle().Which;
        line.Values["Tags"].Should().Be(Tags);
        line.Values["Reference"].Should().BeOfType<string>().Which.Should().MatchRegex("^[0-9a-f]{12}$");
        line.Message.Should().Contain("Give feedback: https://wombat.test/msf/respond/abc", "the stub shows the text it did not send");
        NoLineNames(lines, Respondent, CopiedTo);
    }

    [Fact]
    public void Redact_TakesOutEveryFormOfEveryAddress_InTheWholeText()
    {
        var message = Mail($"Nurse One <{Respondent}>", cc: CopiedTo);
        var thrown = Thrown(new InvalidOperationException(
            $"Refused Nurse One <{Respondent}>, then {Respondent.ToUpperInvariant()}.",
            new IOException($"The copy to {CopiedTo} was refused too.")));

        var redacted = EmailLog.Redact(thrown, message);

        redacted.Message.Should().Be("Refused [recipient], then [recipient].");
        redacted.ToString().Should().Contain("The copy to [recipient] was refused too.", "inner exceptions are in the text")
            .And.NotContainAny(Respondent, CopiedTo, Respondent.ToUpperInvariant());
        redacted.StackTrace.Should().Be(thrown.StackTrace);
        redacted.InnerException.Should().BeNull("the original, chained, would be rendered in full");
    }

    [Fact]
    public void Redact_TakesOutAnInternationalAddress_InTheAsciiFormItWasSentIn()
    {
        // MailKit sends an international domain in its ASCII form to a server that does not take UTF-8, and the server
        // quotes it as sent; the mail's header has it in Unicode.
        var redacted = EmailLog.Redact(
            Thrown(new InvalidOperationException("5.1.1 <nurse@xn--bcher-kva.example>: Recipient address rejected")),
            Mail("nurse@bücher.example"));

        redacted.Message.Should().Be("5.1.1 <[recipient]>: Recipient address rejected");
    }

    [Fact]
    public void Redact_TakesOutALocalPartStandingAlone_ButNotOneInsideAnotherName()
    {
        // Sendmail quotes only the local part. The same letters inside a longer local part, or a dotted name, are
        // someone or something else; and a local part too short to name anyone is left in the server's words.
        var redacted = EmailLog.Redact(
            Thrown(new InvalidOperationException(
                "5.1.1 <nurse-1>... User unknown; NURSE-1. Not nurse-12, ward.nurse-1 or nurse-1.ward.")),
            Mail("nurse-1@example.test", cc: "ab@example.test"));

        redacted.Message.Should().Be(
            "5.1.1 <[recipient]>... User unknown; [recipient]. Not nurse-12, ward.nurse-1 or nurse-1.ward.");
        EmailLog.Redact(Thrown(new InvalidOperationException("ab refused ab@example.test")), Mail("ab@example.test"))
            .Message.Should().Be("ab refused [recipient]", $"a local part under {EmailLog.ShortestRedactedLocalPart} characters stays");
    }

    /// <summary>
    /// The redaction is proved on the exception MailKit really throws when a mail server refuses a recipient, not only on a
    /// stand-in: the worker sends through <see cref="MailKitEmailSender" /> to a server on this machine that refuses every
    /// recipient in the words of a real one, quoting what it was sent (T282 review).
    /// </summary>
    [Theory]
    [InlineData("nurse-1@example.test", "5.1.1 <{0}>: Recipient address rejected: User unknown in local recipient table", "nurse-1@example.test")]
    [InlineData("nurse@bücher.example", "5.1.1 <{0}>: Recipient address rejected: User unknown in local recipient table", "nurse@xn--bcher-kva.example")]
    [InlineData("nurse-1@example.test", "5.1.1 <{1}>... User unknown", "nurse-1@example.test")]
    public async Task ARecipientTheMailServerRefuses_IsLoggedFromMailKitsOwnException_LessEveryFormOfTheAddress(
        string recipient, string refusal, string sentAs)
    {
        await using var server = new RefusingSmtpServer(address => string.Format(
            System.Globalization.CultureInfo.InvariantCulture, refusal, address, address.Split('@')[0]));
        var lines = new List<LogLine>();
        var queue = new EmailQueue();
        await queue.Writer.WriteAsync(new QueuedEmail(Mail(recipient)));
        queue.Writer.Complete();

        var mailKit = new MailKitEmailSender(Microsoft.Extensions.Options.Options.Create(new EmailSettings
        {
            SmtpHost = "127.0.0.1",
            SmtpPort = server.Port,
            FromAddress = "wombat@example.test",
            TimeoutSeconds = 5
        }));
        await Worker(queue, mailKit, lines).ExecutePublicAsync(new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token);

        server.Recipients.Should().HaveCount(EmailWorker.MaxRetries)
            .And.AllSatisfy(sent => sent.Should().Be(sentAs, "guard: the server was sent the address in the form it quotes"));
        var dropped = lines.Should().ContainSingle(line => line.Level == LogLevel.Error).Which;
        dropped.Exception!.ToString().Should()
            .StartWith("MailKit.Net.Smtp.SmtpCommandException: ", "the log keeps what MailKit threw")
            .And.Contain(string.Format(System.Globalization.CultureInfo.InvariantCulture, refusal, "[recipient]", "[recipient]"),
                "and the server's reason, less the address");
        NoLineNames(lines, recipient, sentAs, recipient.Split('@')[0]);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static EmailMessage Mail(string to, string? cc = null)
        => new(to, "Feedback request: Dr Test Trainee", "<p/>", "Give feedback: https://wombat.test/msf/respond/abc",
            Cc: cc, Tags: ["msf-invite", "campaign:1"]);

    /// <summary>Every part of every line a sink could write, none of which may hold any form of the addresses.</summary>
    private static void NoLineNames(List<LogLine> lines, params string[] addresses)
    {
        lines.Should().NotBeEmpty("guard: a test that captured nothing proves nothing");
        foreach (var line in lines)
        {
            var parts = new List<string?> { line.Message, line.Exception?.Message, line.Exception?.ToString() };
            parts.AddRange(line.Values.Values.Select(value => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)));

            foreach (var part in parts.OfType<string>())
            {
                foreach (var address in addresses)
                {
                    part.Should().NotContainEquivalentOf(address, $"no log line carries an address (T282): \"{line.Message}\"");
                }
            }
        }
    }

    private static Exception Thrown(Exception exception)
    {
        try
        {
            throw exception;
        }
        catch (Exception caught)
        {
            return caught;
        }
    }

    private static CancellationToken FiveSeconds() => new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;

    private static EmailWorker Worker(
        EmailQueue queue,
        ISmtpSender smtp,
        List<LogLine> lines,
        Func<int, TimeSpan>? retryDelay = null,
        IEmailDeliveryObserver? observer = null)
    {
        return new EmailWorker(
            queue,
            Services(smtp, observer).GetRequiredService<IServiceScopeFactory>(),
            new LogRecorder<EmailWorker>(lines),
            retryDelay ?? (_ => TimeSpan.Zero));
    }

    private static ServiceProvider Services(ISmtpSender smtp, IEmailDeliveryObserver? observer = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton(smtp);
        if (observer is not null)
        {
            services.AddSingleton(observer);
        }

        return services.BuildServiceProvider();
    }

    private sealed record LogLine(
        LogLevel Level,
        string Message,
        IReadOnlyDictionary<string, object?> Values,
        Exception? Exception)
    {
        public string? Template => Values.TryGetValue("{OriginalFormat}", out var template) ? template as string : null;
    }

    /// <summary>Keeps every line; or, given <c>failAt</c>, throws on a line at that level, as a failing sink makes the logger do.</summary>
    private sealed class LogRecorder<T>(List<LogLine> lines, LogLevel? failAt = null) : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var values = state is IReadOnlyList<KeyValuePair<string, object?>> pairs
                ? pairs.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal)
                : new Dictionary<string, object?>(StringComparer.Ordinal);

            if (logLevel == failAt)
            {
                throw new InvalidOperationException("An error occurred while writing to logger(s).");
            }

            lines.Add(new LogLine(logLevel, formatter(state, exception), values, exception));
        }
    }

    /// <summary>
    /// Fails the first <c>failures</c> attempts with the reply <c>reply</c> gives for the attempt; or, hanging, waits
    /// until the host stops.
    /// </summary>
    private sealed class ScriptedSmtp(int failures, bool hangs = false, Func<int, string>? reply = null) : ISmtpSender
    {
        public int Attempts { get; private set; }

        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
        {
            Attempts++;
            Entered.TrySetResult();

            if (hangs)
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }

            if (Attempts <= failures)
            {
                throw new InvalidOperationException(reply?.Invoke(Attempts) ?? $"The mail server refused {message.To}.");
            }
        }
    }

    private sealed class ThrowingObserver(string reason) : IEmailDeliveryObserver
    {
        public Task RecordAsync(EmailMessage message, EmailDeliveryOutcome outcome, CancellationToken cancellationToken)
            => Task.FromException(new InvalidOperationException(reason));
    }

    /// <summary>
    /// An SMTP server on the loopback that takes every command but <c>RCPT TO</c>, which it refuses with a 550 reply
    /// <c>refusal</c> words from the address it was sent. It offers no extensions, so MailKit speaks plain ASCII to it.
    /// </summary>
    private sealed class RefusingSmtpServer : IAsyncDisposable
    {
        private readonly System.Net.Sockets.TcpListener _listener = new(System.Net.IPAddress.Loopback, 0);
        private readonly Func<string, string> _refusal;
        private readonly CancellationTokenSource _stop = new();
        private readonly Task _serving;
        private readonly List<string> _recipients = [];

        public RefusingSmtpServer(Func<string, string> refusal)
        {
            _refusal = refusal;
            _listener.Start();
            _serving = ServeAsync();
        }

        public int Port => ((System.Net.IPEndPoint)_listener.LocalEndpoint).Port;

        /// <summary>The address of every <c>RCPT TO</c> it was sent, as sent.</summary>
        public IReadOnlyList<string> Recipients
        {
            get
            {
                lock (_recipients)
                {
                    return [.. _recipients];
                }
            }
        }

        private async Task ServeAsync()
        {
            while (!_stop.IsCancellationRequested)
            {
                System.Net.Sockets.TcpClient client;
                try
                {
                    client = await _listener.AcceptTcpClientAsync(_stop.Token);
                }
                catch (Exception ex) when (ex is OperationCanceledException or System.Net.Sockets.SocketException or ObjectDisposedException)
                {
                    return;
                }

                // One connection at a time: the worker sends one mail, one attempt, at a time.
                await ConverseAsync(client);
            }
        }

        private async Task ConverseAsync(System.Net.Sockets.TcpClient client)
        {
            using (client)
            {
                try
                {
                    var stream = client.GetStream();
                    using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                    await using var writer = new StreamWriter(stream, new System.Text.UTF8Encoding(false))
                    {
                        NewLine = "\r\n",
                        AutoFlush = true
                    };

                    await writer.WriteLineAsync("220 localhost ESMTP");
                    while (await reader.ReadLineAsync(_stop.Token) is { } line)
                    {
                        var verb = line.Split(' ', 2)[0].ToUpperInvariant();
                        if (verb == "RCPT")
                        {
                            var address = System.Text.RegularExpressions.Regex.Match(line, "<([^>]*)>").Groups[1].Value;
                            lock (_recipients)
                            {
                                _recipients.Add(address);
                            }

                            await writer.WriteLineAsync($"550 {_refusal(address)}");
                        }
                        else if (verb == "QUIT")
                        {
                            await writer.WriteLineAsync("221 2.0.0 Bye");
                            return;
                        }
                        else
                        {
                            await writer.WriteLineAsync(verb is "EHLO" or "HELO" ? "250 localhost" : "250 2.0.0 Ok");
                        }
                    }
                }
                catch (Exception ex) when (ex is IOException or OperationCanceledException or ObjectDisposedException)
                {
                    // The client hung up, or the test is over.
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _stop.CancelAsync();
            _listener.Stop();
            await _serving;
            _stop.Dispose();
        }
    }
}
