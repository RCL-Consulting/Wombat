using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Email;
using Wombat.Infrastructure.Email;

namespace Wombat.Infrastructure.Tests.Email;

/// <summary>
/// T400 (flow 18, Q2, Q18, K20): both hosts can tell Home whether mail is configured and what waits in their queue, and
/// only a host whose worker runs records what became of each mail.
/// </summary>
public sealed class EmailHealthWiringTests
{
    [Fact]
    public void WithAMailServer_HomeReadsConfigured_AndTheQueuesDepth_AndTheWorkerHasItsRecorder()
    {
        using var provider = Services(smtpHost: "smtp.example.test").BuildServiceProvider();
        using var scope = provider.CreateScope();

        var health = scope.ServiceProvider.GetRequiredService<IEmailHealth>();
        health.Should().BeOfType<EmailHealth>();
        health.IsConfigured.Should().BeTrue();
        health.QueueDepth.Should().Be(0);
        health.Should().BeSameAs(provider.GetRequiredService<IEmailHealth>(), "one for the host");

        var queue = provider.GetRequiredService<EmailQueue>();
        queue.Writer.TryWrite(new QueuedEmail(new EmailMessage("a@b.test", "Digest", "<p/>", "T"))).Should().BeTrue();
        queue.Writer.TryWrite(new QueuedEmail(new EmailMessage("c@d.test", "Digest", "<p/>", "T"))).Should().BeTrue();
        health.QueueDepth.Should().Be(2, "the depth is the host's own queue, read as Home is drawn");

        scope.ServiceProvider.GetService<EmailOutcomeRecorder>().Should().NotBeNull("the worker records each mail's outcome");
    }

    [Fact]
    public void WithNoMailServer_HomeReadsNotConfigured_AndNothingIsRecorded()
    {
        using var provider = Services(smtpHost: null).BuildServiceProvider();
        using var scope = provider.CreateScope();

        var health = scope.ServiceProvider.GetRequiredService<IEmailHealth>();
        health.IsConfigured.Should().BeFalse();
        health.QueueDepth.Should().Be(0);

        scope.ServiceProvider.GetService<EmailOutcomeRecorder>().Should().BeNull(
            "a host that writes its mail to the log records no outcome (Q18: LoggingEmailSender writes none)");
    }

    [Fact]
    public void IEmailHealthIsRegisteredOnce_InBothBranches()
    {
        Services(smtpHost: "smtp.example.test").Count(descriptor => descriptor.ServiceType == typeof(IEmailHealth))
            .Should().Be(1);
        Services(smtpHost: null).Count(descriptor => descriptor.ServiceType == typeof(IEmailHealth)).Should().Be(1);
    }

    /// <summary>The host's composition, as <c>EmailDeliveryObserverWiringTests</c> builds it. No connection is opened.</summary>
    private static ServiceCollection Services(string? smtpHost)
    {
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=t400_never_opened"
        };
        if (smtpHost is not null)
        {
            settings["Email:SmtpHost"] = smtpHost;
        }

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);
        return services;
    }
}
