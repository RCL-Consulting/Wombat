using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Infrastructure.Scheduling;

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

/// <summary>
/// What the mail worker would report of a job's mail, handed to the tally the job counted it on (T283).
/// </summary>
internal static class JobMailReports
{
    /// <summary>
    /// Reports each of <paramref name="sent" /> to the provider's <see cref="ScheduledJobMailTally" />: those addressed to
    /// one of <paramref name="droppedTo" /> as given up on, the rest as sent.
    /// </summary>
    public static async Task ReportAsync(IServiceProvider provider, IEnumerable<EmailMessage> sent, params string[] droppedTo)
    {
        var tally = provider.GetRequiredService<ScheduledJobMailTally>();
        var at = new DateTime(2029, 3, 5, 7, 1, 0, DateTimeKind.Utc);
        foreach (var message in sent.ToList())
        {
            await tally.RecordAsync(
                message,
                droppedTo.Contains(message.To) ? EmailDeliveryOutcome.Dropped(3, at) : EmailDeliveryOutcome.Delivered(1, at),
                CancellationToken.None);
        }
    }

    /// <summary>The run's line counting what became of its mail; fails unless there is exactly one.</summary>
    public static CapturedLogEntry DeliveryLine(this CapturingLogger logger)
        => logger.Entries.Should().ContainSingle(entry => entry.Values.ContainsKey("NotDeliveredCount")).Which;

    /// <summary>Every mail the run handed over carries the one key of that run, and it is not a tag.</summary>
    public static void ShouldAllCarryOneRunKey(this IReadOnlyCollection<EmailMessage> sent)
    {
        sent.Should().NotBeEmpty();
        sent.Should().OnlyContain(message => message.DeliveryKey != null && message.DeliveryKey.StartsWith("job-mail:"));
        sent.Select(message => message.DeliveryKey).Distinct().Should().ContainSingle("one run, one key");
        sent.Should().OnlyContain(message => message.Tags == null || !message.Tags.Contains(message.DeliveryKey!));
    }
}
