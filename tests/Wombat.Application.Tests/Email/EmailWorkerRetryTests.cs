using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Wombat.Application.Common.Email;
using Wombat.Infrastructure.Email;

namespace Wombat.Application.Tests.Email;

public sealed class EmailWorkerRetryTests
{
    [Fact]
    public async Task Worker_RetriesTransientFailure_AndEventuallySucceeds()
    {
        var queue = new EmailQueue();
        var message = new EmailMessage("a@b.test", "Test", "<p>Hi</p>", "Hi", Tags: ["test"]);

        var callCount = 0;
        var smtpSender = new Mock<ISmtpSender>();
        smtpSender
            .Setup(s => s.SendAsync(message, It.IsAny<CancellationToken>()))
            .Returns<EmailMessage, CancellationToken>((_, _) =>
            {
                callCount++;
                if (callCount < 2)
                {
                    throw new InvalidOperationException("Transient SMTP error");
                }
                return Task.CompletedTask;
            });

        var worker = BuildWorker(queue, smtpSender.Object);
        await queue.Writer.WriteAsync(message);
        queue.Writer.Complete();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await worker.ExecutePublicAsync(cts.Token);

        callCount.Should().Be(2, "worker should retry once after the first transient failure");
    }

    [Fact]
    public async Task Worker_DropsMessage_AfterMaxRetriesExhausted()
    {
        var queue = new EmailQueue();
        var message = new EmailMessage("a@b.test", "Fail", "<p>Fail</p>", "Fail");

        var callCount = 0;
        var smtpSender = new Mock<ISmtpSender>();
        smtpSender
            .Setup(s => s.SendAsync(message, It.IsAny<CancellationToken>()))
            .Returns<EmailMessage, CancellationToken>((_, _) =>
            {
                callCount++;
                throw new InvalidOperationException("Persistent SMTP error");
            });

        var worker = BuildWorker(queue, smtpSender.Object);
        await queue.Writer.WriteAsync(message);
        queue.Writer.Complete();

        // Should complete without throwing — message dropped after MaxRetries
        var act = async () =>
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await worker.ExecutePublicAsync(cts.Token);
        };

        await act.Should().NotThrowAsync();
        callCount.Should().Be(EmailWorker.MaxRetries, "worker should attempt exactly MaxRetries times before dropping");
    }

    [Fact]
    public async Task Worker_ProcessesMultipleMessages_InOrder()
    {
        var queue = new EmailQueue();
        var sent = new List<string>();

        var smtpSender = new Mock<ISmtpSender>();
        smtpSender
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns<EmailMessage, CancellationToken>((msg, _) =>
            {
                sent.Add(msg.To);
                return Task.CompletedTask;
            });

        var worker = BuildWorker(queue, smtpSender.Object);

        await queue.Writer.WriteAsync(new EmailMessage("first@test", "S", "<p/>", "T"));
        await queue.Writer.WriteAsync(new EmailMessage("second@test", "S", "<p/>", "T"));
        queue.Writer.Complete();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await worker.ExecutePublicAsync(cts.Token);

        sent.Should().Equal("first@test", "second@test");
    }

    private static EmailWorker BuildWorker(EmailQueue queue, ISmtpSender smtpSender)
    {
        var services = new ServiceCollection();
        services.AddSingleton(smtpSender);
        var provider = services.BuildServiceProvider();

        var scopeFactory = new Mock<IServiceScopeFactory>();
        scopeFactory
            .Setup(f => f.CreateScope())
            .Returns(() =>
            {
                var scope = new Mock<IServiceScope>();
                scope.Setup(s => s.ServiceProvider).Returns(provider);
                return scope.Object;
            });

        return new EmailWorker(
            queue,
            scopeFactory.Object,
            NullLogger<EmailWorker>.Instance,
            retryDelay: _ => TimeSpan.Zero); // no delay in tests
    }
}
