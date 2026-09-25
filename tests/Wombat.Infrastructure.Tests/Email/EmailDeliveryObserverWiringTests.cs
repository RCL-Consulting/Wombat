using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Infrastructure.Email;
using Wombat.Infrastructure.Invitations;
using Wombat.Infrastructure.MultiSourceFeedback;
using Wombat.Infrastructure.Scheduling;

namespace Wombat.Infrastructure.Tests.Email;

/// <summary>
/// T251's and T283's wiring: the host hands the mail worker every recorder of what became of a mail. Every delivery test
/// registers the recorder or the tally it is about by hand, so a line dropped from <c>AddInfrastructure</c> left every
/// suite green while no MSF link, no account invitation and no job's run was ever told what became of its mail
/// (T283 review).
/// </summary>
public sealed class EmailDeliveryObserverWiringTests
{
    [Fact]
    public void TheMailWorker_ReportsToEveryRecorder_AndTheJobsCountOnTheTallyItReportsTo()
    {
        using var provider = Host(smtpHost: "smtp.example.test");
        using var scope = provider.CreateScope();

        // The worker reads its observers as this does: every one registered, on a scope of its own.
        var observers = scope.ServiceProvider.GetServices<IEmailDeliveryObserver>().ToList();

        observers.Should().ContainSingle(observer => observer is MsfLinkDeliveryRecorder, "an MSF link's outcome (T251)");
        observers.Should().ContainSingle(observer => observer is AccountInvitationDeliveryRecorder,
            "an account invitation's outcome (T283)");

        // The jobs open their run on the tally they resolve (ScheduledJobMailTally.Start); the worker must report to that
        // same instance, or every run's mail is counted nowhere.
        var tally = observers.OfType<ScheduledJobMailTally>().Should().ContainSingle().Which;
        tally.Should().BeSameAs(scope.ServiceProvider.GetRequiredService<ScheduledJobMailTally>());
        using (var otherScope = provider.CreateScope())
        {
            otherScope.ServiceProvider.GetServices<IEmailDeliveryObserver>().OfType<ScheduledJobMailTally>()
                .Should().ContainSingle().Which.Should().BeSameAs(tally, "one tally for the host, whichever scope reports");
        }

        scope.ServiceProvider.GetRequiredService<IEmailSender>().Should().BeOfType<QueuedEmailSender>();
        HostedServices(smtpHost: "smtp.example.test").Should().Contain(typeof(EmailWorker));
    }

    [Fact]
    public void WithNoMailServer_NoWorkerRuns_AndMailIsOnlyLogged()
    {
        using var provider = Host(smtpHost: null);
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IEmailSender>().Should().BeOfType<LoggingEmailSender>();
        HostedServices(smtpHost: null).Should().NotContain(typeof(EmailWorker));
    }

    private static ServiceProvider Host(string? smtpHost) => Services(smtpHost).BuildServiceProvider();

    private static IEnumerable<Type?> HostedServices(string? smtpHost)
        => Services(smtpHost)
            .Where(descriptor => descriptor.ServiceType == typeof(IHostedService))
            .Select(descriptor => descriptor.ImplementationType);

    /// <summary>The host's composition. No connection is opened: nothing here queries.</summary>
    private static ServiceCollection Services(string? smtpHost)
    {
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=t283_never_opened"
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
