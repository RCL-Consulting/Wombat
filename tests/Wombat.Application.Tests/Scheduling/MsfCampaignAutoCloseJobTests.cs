using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling.Jobs;

namespace Wombat.Application.Tests.Scheduling;

public sealed class MsfCampaignAutoCloseJobTests
{
    [Fact]
    public async Task ExecuteAsync_ClosesExpiredCampaigns()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(dbName));
        services.AddScoped<IApplicationDbContext>(p => p.GetRequiredService<ApplicationDbContext>());
        var provider = services.BuildServiceProvider();

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var template = new MsfTemplate
            {
                Name = "Test Template",
                IsActive = true
            };
            db.Set<MsfTemplate>().Add(template);
            await db.SaveChangesAsync();

            var campaign = new MsfCampaign
            {
                SubjectUserId = "trainee-1",
                TemplateId = template.Id,
                CreatedByUserId = "admin",
                CreatedOn = DateTime.UtcNow.AddDays(-30),
                OpensOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-14)),
                ClosesOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                State = MsfCampaignState.Open,
                OpenedOn = DateTime.UtcNow.AddDays(-14)
            };
            db.Set<MsfCampaign>().Add(campaign);
            await db.SaveChangesAsync();
        }

        var job = new MsfCampaignAutoCloseJob(provider.GetRequiredService<IServiceScopeFactory>());
        var context = new ScheduledJobContext(DateTime.UtcNow, NullLogger.Instance);

        await job.ExecuteAsync(context, CancellationToken.None);

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var campaign = await db.Set<MsfCampaign>().FirstAsync();
            campaign.State.Should().Be(MsfCampaignState.UnderReview);
            campaign.ClosedOn.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ExecuteAsync_SkipsNonExpiredCampaigns()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(dbName));
        services.AddScoped<IApplicationDbContext>(p => p.GetRequiredService<ApplicationDbContext>());
        var provider = services.BuildServiceProvider();

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var template = new MsfTemplate
            {
                Name = "Test Template",
                IsActive = true
            };
            db.Set<MsfTemplate>().Add(template);
            await db.SaveChangesAsync();

            var campaign = new MsfCampaign
            {
                SubjectUserId = "trainee-1",
                TemplateId = template.Id,
                CreatedByUserId = "admin",
                CreatedOn = DateTime.UtcNow,
                OpensOn = DateOnly.FromDateTime(DateTime.UtcNow),
                ClosesOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                State = MsfCampaignState.Open,
                OpenedOn = DateTime.UtcNow
            };
            db.Set<MsfCampaign>().Add(campaign);
            await db.SaveChangesAsync();
        }

        var job = new MsfCampaignAutoCloseJob(provider.GetRequiredService<IServiceScopeFactory>());
        var context = new ScheduledJobContext(DateTime.UtcNow, NullLogger.Instance);

        await job.ExecuteAsync(context, CancellationToken.None);

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var campaign = await db.Set<MsfCampaign>().FirstAsync();
            campaign.State.Should().Be(MsfCampaignState.Open);
        }
    }
}
