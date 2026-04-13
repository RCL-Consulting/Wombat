using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email;

namespace Wombat.Infrastructure.Email;

public sealed class EmailWorker : BackgroundService
{
    internal const int MaxRetries = 3;

    private readonly EmailQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailWorker> _logger;
    private readonly Func<int, TimeSpan> _retryDelay;

    public EmailWorker(EmailQueue queue, IServiceScopeFactory scopeFactory, ILogger<EmailWorker> logger)
        : this(queue, scopeFactory, logger, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)))
    {
    }

    // Constructor used by unit tests to inject a zero-delay strategy.
    internal EmailWorker(
        EmailQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<EmailWorker> logger,
        Func<int, TimeSpan> retryDelay)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _retryDelay = retryDelay;
    }

    // Exposed for unit tests — production code uses the BackgroundService host lifecycle.
    internal Task ExecutePublicAsync(CancellationToken ct) => ExecuteAsync(ct);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            await SendWithRetryAsync(message, stoppingToken);
        }
    }

    private async Task SendWithRetryAsync(EmailMessage message, CancellationToken stoppingToken)
    {
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISmtpSender>();
                await sender.SendAsync(message, stoppingToken);
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // App is shutting down — don't retry.
                _logger.LogWarning(
                    "Email to {To} (subject: {Subject}) abandoned — app shutdown.",
                    message.To, message.Subject);
                return;
            }
            catch (Exception ex)
            {
                if (attempt == MaxRetries)
                {
                    _logger.LogError(ex,
                        "Email to {To} (subject: {Subject}, tags: {Tags}) failed after {MaxRetries} attempts. Message dropped.",
                        message.To, message.Subject,
                        message.Tags is { Count: > 0 } ? string.Join(", ", message.Tags) : "(none)",
                        MaxRetries);
                    return;
                }

                var delay = _retryDelay(attempt);
                _logger.LogWarning(ex,
                    "Email to {To} failed (attempt {Attempt}/{MaxRetries}). Retrying in {Delay}s.",
                    message.To, attempt, MaxRetries, delay.TotalSeconds);

                await Task.Delay(delay, stoppingToken);
            }
        }
    }
}
