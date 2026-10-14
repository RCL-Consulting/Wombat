using Microsoft.Extensions.Logging;

namespace Wombat.Web.Tests.TestSupport;

/// <summary>A logger that keeps what was logged, so a test can say an unexpected failure went to the log (T338).</summary>
public sealed class CapturingLogger<T> : ILogger<T>
{
    public List<(LogLevel Level, Exception? Exception)> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        => Entries.Add((logLevel, exception));
}
