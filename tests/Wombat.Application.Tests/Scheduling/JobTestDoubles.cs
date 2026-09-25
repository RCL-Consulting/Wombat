using FluentAssertions;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Application.Tests.Scheduling;

/// <summary>
/// Keeps every message a job hands over, in order, so a test asserts on exactly who was written to and what. Addresses
/// follow <c>NomineeSeed</c>'s: <c>{userId}@test.local</c>.
/// </summary>
internal sealed class RecordingEmailSender : IEmailSender
{
    public List<EmailMessage> Sent { get; } = [];

    public IReadOnlyList<string> Recipients => Sent.Select(message => message.To).ToList();

    public static string EmailOf(string userId) => $"{userId}@test.local";

    /// <summary>The one message sent to <paramref name="userId" />.</summary>
    public EmailMessage To(string userId) => Sent.Should().ContainSingle(message => message.To == EmailOf(userId)).Which;

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        Sent.Add(message);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Keeps every entry with its structured values, so a test asserts on a count such as <c>{DeactivatedCount}</c> itself
/// rather than on a substring of a rendered message that a reworded template would silently stop matching.
/// </summary>
internal sealed class CapturingLogger : ILogger
{
    public List<CapturedLogEntry> Entries { get; } = [];

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

        Entries.Add(new CapturedLogEntry(logLevel, formatter(state, exception), values));
    }

    /// <summary>The run's one log line, at Information: every job that mails people logs exactly one.</summary>
    public CapturedLogEntry OneLine()
    {
        var entry = Entries.Should().ContainSingle().Which;
        entry.Level.Should().Be(LogLevel.Information);
        return entry;
    }
}

internal sealed record CapturedLogEntry(LogLevel Level, string Message, IReadOnlyDictionary<string, object?> Values)
{
    /// <summary>A count the line carries under <paramref name="key" />; fails if it carries none.</summary>
    public int Count(string key)
    {
        Values.Should().ContainKey(key);
        return (int)Values[key]!;
    }
}
