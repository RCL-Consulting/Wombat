using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Scheduling;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling.Jobs;

namespace Wombat.Application.Tests.Scheduling;

/// <summary>
/// T132. The job used to mail <c>invitation.TokenHash</c> — the stored one-way hash — inside a
/// relative URL, so every expiry reminder was both unclickable and unusable, while the job logged
/// success. It now re-issues a token, which is the only way to produce a working link when the
/// original plaintext is unrecoverable by design.
/// </summary>
public sealed class MsfInvitationExpiryReminderJobTests
{
    private const string RespondUrl = "https://wombat.example/msf/respond";

    private sealed class CapturingEmailSender : IEmailSender
    {
        public List<EmailMessage> Sent { get; } = [];

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            Sent.Add(message);
            return Task.CompletedTask;
        }
    }

    private static ServiceProvider BuildProvider(CapturingEmailSender emailSender, string? respondUrl = RespondUrl)
    {
        // The name is computed once, not inside the lambda: the lambda runs per DbContext, so an
        // inline Guid gives every scope its own database and nothing seeded is ever read back.
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(dbName));
        services.AddScoped<IApplicationDbContext>(p => p.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IEmailSender>(_ => emailSender);
        services.AddScoped<IInvitationTokenService, InvitationTokenService>();
        services.AddScoped(_ => Options.Create(new WombatOptions { MsfRespondUrl = respondUrl }));
        return services.BuildServiceProvider();
    }

    /// <summary>Seeds an open campaign with one invitation expiring inside the 48-hour window.</summary>
    private static async Task<string> SeedExpiringInvitationAsync(ServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IInvitationTokenService>();

        var template = new MsfTemplate { Name = "Paediatrics MSF", IsActive = true };
        db.Set<MsfTemplate>().Add(template);
        await db.SaveChangesAsync();

        var campaign = new MsfCampaign
        {
            SubjectUserId = "trainee-1",
            TemplateId = template.Id,
            CreatedByUserId = "coordinator",
            CreatedOn = DateTime.UtcNow.AddDays(-20),
            OpensOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-14)),
            ClosesOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            State = MsfCampaignState.Open,
            OpenedOn = DateTime.UtcNow.AddDays(-14)
        };
        db.Set<MsfCampaign>().Add(campaign);
        await db.SaveChangesAsync();

        // The token the respondent was given when the campaign opened.
        var originalToken = tokenService.GenerateToken();
        db.Set<MsfInvitation>().Add(new MsfInvitation
        {
            CampaignId = campaign.Id,
            RespondentEmail = "consultant@example.com",
            RespondentCategory = MsfRespondentCategory.Consultant,
            TokenHash = tokenService.HashToken(originalToken),
            IssuedOn = DateTime.UtcNow.AddDays(-14),
            ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
        });
        await db.SaveChangesAsync();

        return originalToken;
    }

    private static async Task RunJobAsync(ServiceProvider provider)
    {
        var job = new MsfInvitationExpiryReminderJob(provider.GetRequiredService<IServiceScopeFactory>());
        await job.ExecuteAsync(new ScheduledJobContext(DateTime.UtcNow, NullLogger.Instance), CancellationToken.None);
    }

    private static string ExtractToken(string body)
    {
        var match = Regex.Match(body, @"token=([A-Za-z0-9_\-%]+)");
        match.Success.Should().BeTrue("the reminder must carry a token in its link");
        return Uri.UnescapeDataString(match.Groups[1].Value);
    }

    [Fact]
    public async Task ExecuteAsync_MailsATokenThatVerifiesAgainstTheStoredHash()
    {
        var emailSender = new CapturingEmailSender();
        var provider = BuildProvider(emailSender);
        await SeedExpiringInvitationAsync(provider);

        await RunJobAsync(provider);

        emailSender.Sent.Should().ContainSingle();
        var mailedToken = ExtractToken(emailSender.Sent[0].TextBody);

        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IInvitationTokenService>();
        var invitation = await db.Set<MsfInvitation>().SingleAsync();

        // The defect: the job mailed the hash itself, so this verification failed.
        tokenService.VerifyToken(mailedToken, invitation.TokenHash)
            .Should().BeTrue("the reminder must mail the token, not its hash");
        mailedToken.Should().NotBe(invitation.TokenHash);
    }

    [Fact]
    public async Task ExecuteAsync_MailsAnAbsoluteUrlBuiltFromConfiguration()
    {
        var emailSender = new CapturingEmailSender();
        var provider = BuildProvider(emailSender);
        await SeedExpiringInvitationAsync(provider);

        await RunJobAsync(provider);

        // A relative URL is not clickable in a mail client; the job used to hardcode "/msf/respond".
        emailSender.Sent[0].TextBody.Should().Contain(RespondUrl);
        var link = Regex.Match(emailSender.Sent[0].TextBody, @"https?://\S+").Value;
        Uri.TryCreate(link, UriKind.Absolute, out _).Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_PersistsTheReissuedHash()
    {
        var emailSender = new CapturingEmailSender();
        var provider = BuildProvider(emailSender);
        await SeedExpiringInvitationAsync(provider);

        await RunJobAsync(provider);
        var mailedToken = ExtractToken(emailSender.Sent[0].TextBody);

        // A fresh scope: the new hash is only useful if the job saved it.
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IInvitationTokenService>();
        var invitation = await db.Set<MsfInvitation>().AsNoTracking().SingleAsync();

        tokenService.VerifyToken(mailedToken, invitation.TokenHash).Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_RetiresTheTokenIssuedWhenTheCampaignOpened()
    {
        // This is the accepted cost of re-issuing, recorded here rather than discovered in
        // production. MsfExpiryReminderEmail tells the respondent the new link replaces the old one.
        var emailSender = new CapturingEmailSender();
        var provider = BuildProvider(emailSender);
        var originalToken = await SeedExpiringInvitationAsync(provider);

        await RunJobAsync(provider);

        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IInvitationTokenService>();
        var invitation = await db.Set<MsfInvitation>().AsNoTracking().SingleAsync();

        tokenService.VerifyToken(originalToken, invitation.TokenHash)
            .Should().BeFalse("re-issuing retires the original link");
        emailSender.Sent[0].TextBody.Should().Contain("replaces the one in your original invitation");
    }

    [Fact]
    public async Task ExecuteAsync_RefusesToSendWhenTheRespondUrlIsNotConfigured()
    {
        // Fail before mailing anything, rather than re-issuing every token and then discovering
        // there is nowhere to send them — which would retire working links for nothing.
        var emailSender = new CapturingEmailSender();
        var provider = BuildProvider(emailSender, respondUrl: null);
        var originalToken = await SeedExpiringInvitationAsync(provider);

        var act = async () => await RunJobAsync(provider);

        await act.Should().ThrowAsync<InvalidOperationException>();
        emailSender.Sent.Should().BeEmpty();

        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IInvitationTokenService>();
        var invitation = await db.Set<MsfInvitation>().AsNoTracking().SingleAsync();
        tokenService.VerifyToken(originalToken, invitation.TokenHash)
            .Should().BeTrue("a misconfigured job must not retire a working token");
    }
}
