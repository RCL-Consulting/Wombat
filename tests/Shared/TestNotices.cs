using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;

namespace Wombat.Tests.Shared;

/// <summary>
/// The one-off notice sender (<see cref="AccountNotices" />) for a handler built by a test: a capturing mail sender, the
/// base URL <c>https://wombat.test</c>, and a logger that keeps what was logged. Linked into the test projects that
/// build a handler that sends one. (T367)
/// </summary>
internal sealed class TestNotices
{
    public TestNotices(IUserAdministrationService users, Exception? failSendWith = null)
    {
        Sender = new CapturingSender(failSendWith);
        Notices = new AccountNotices(
            users,
            Sender,
            Options.Create(new WombatOptions { BaseUrl = "https://wombat.test" }),
            Logger);
    }

    /// <summary>A sender for a test that does not read the mail: nobody in the directory, nothing sent.</summary>
    public static AccountNotices None(IUserAdministrationService users) => new TestNotices(users).Notices;

    public AccountNotices Notices { get; }

    public CapturingSender Sender { get; }

    public CapturingLogger Logger { get; } = new();

    internal sealed class CapturingSender(Exception? failWith) : IEmailSender
    {
        public List<EmailMessage> Sent { get; } = [];

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            if (failWith is not null)
            {
                throw failWith;
            }

            Sent.Add(message);
            return Task.CompletedTask;
        }
    }

    internal sealed class CapturingLogger : ILogger<AccountNotices>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullLogger.Instance.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Entries.Add((logLevel, formatter(state, exception)));
    }
}
