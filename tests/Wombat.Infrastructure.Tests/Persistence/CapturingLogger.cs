using Microsoft.Extensions.Logging;

namespace Wombat.Infrastructure.Tests.Persistence;

/// <summary>
/// An <see cref="ILogger{TCategoryName}" /> that keeps every entry, so a test can assert on what a startup
/// component announced as well as on what it did.
/// </summary>
/// <remarks>
/// Several startup components in this layer deliberately never throw and never write (the seeders' drift
/// warning, the progress bootstrapper's kill switch and failure path). For those, the log line IS the
/// behaviour, and a <c>NullLogger</c> would let it vanish without a failing test.
/// </remarks>
internal sealed class CapturingLogger<T> : ILogger<T>
{
    public List<CapturedLogEntry> Entries { get; } = [];

    public IEnumerable<CapturedLogEntry> Warnings => Entries.Where(entry => entry.Level == LogLevel.Warning);

    public IEnumerable<CapturedLogEntry> Errors => Entries.Where(entry => entry.Level == LogLevel.Error);

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
        // The structured values, so a test can assert on {EpaCode} itself rather than on a substring of the
        // rendered message that a reworded template would silently stop matching.
        var values = state is IReadOnlyList<KeyValuePair<string, object?>> pairs
            ? pairs.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal)
            : new Dictionary<string, object?>(StringComparer.Ordinal);

        Entries.Add(new CapturedLogEntry(logLevel, formatter(state, exception), values, exception));
    }
}

internal sealed record CapturedLogEntry(
    LogLevel Level,
    string Message,
    IReadOnlyDictionary<string, object?> Values,
    Exception? Exception);
