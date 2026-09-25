using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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

    /// <summary>
    /// T267: one campaign whose save fails for any reason but a concurrent write holds back none of the others. The run
    /// closes them, then fails, naming the campaign it could not close, so the scheduler records the run as failed and the
    /// next run tries it again. The concurrent write is <c>MsfAutoCloseRacePostgresTests</c>: EF InMemory has no xmin.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_OneCampaignsFailedSave_ClosesTheOthers_ThenFailsNamingIt()
    {
        var dbName = Guid.NewGuid().ToString();
        var failing = new FailTheSaveOf();
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(dbName).AddInterceptors(failing));
        services.AddScoped<IApplicationDbContext>(p => p.GetRequiredService<ApplicationDbContext>());
        var provider = services.BuildServiceProvider();

        var now = DateTime.UtcNow;
        var ids = new List<int>();
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var template = new MsfTemplate { Name = "Test Template", IsActive = true };
            db.Set<MsfTemplate>().Add(template);
            await db.SaveChangesAsync();

            for (var index = 0; index < 3; index++)
            {
                var campaign = new MsfCampaign
                {
                    SubjectUserId = $"trainee-{index + 1}",
                    TemplateId = template.Id,
                    CreatedByUserId = "admin",
                    CreatedOn = now.AddDays(-30),
                    OpensOn = DateOnly.FromDateTime(now.AddDays(-14)),
                    ClosesOn = DateOnly.FromDateTime(now.AddDays(-1)),
                    State = MsfCampaignState.Open,
                    OpenedOn = now.AddDays(-14)
                };
                db.Set<MsfCampaign>().Add(campaign);
                await db.SaveChangesAsync();
                ids.Add(campaign.Id);
            }
        }

        failing.CampaignId = ids[1];

        var job = new MsfCampaignAutoCloseJob(provider.GetRequiredService<IServiceScopeFactory>());
        var run = () => job.ExecuteAsync(new ScheduledJobContext(now, NullLogger.Instance), CancellationToken.None);

        (await run.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage($"*1 expired campaign(s) could not be closed ({ids[1]})*")
            .WithInnerException<DbUpdateException>();

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var states = await db.Set<MsfCampaign>().AsNoTracking().ToDictionaryAsync(campaign => campaign.Id, campaign => campaign.State);
            states[ids[0]].Should().Be(MsfCampaignState.UnderReview, "it came before the failure");
            states[ids[1]].Should().Be(MsfCampaignState.Open, "its save failed, and nothing of it was stored");
            states[ids[2]].Should().Be(MsfCampaignState.UnderReview, "it came after the failure, and was still closed");
        }
    }

    /// <summary>Fails every save that would close <see cref="CampaignId" />, as a fault would, not a concurrent write.</summary>
    private sealed class FailTheSaveOf : SaveChangesInterceptor
    {
        public int? CampaignId { get; set; }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context!.ChangeTracker.Entries<MsfCampaign>()
                .Any(entry => entry.State == EntityState.Modified && entry.Entity.Id == CampaignId))
            {
                throw new DbUpdateException("A fault in this campaign's save.");
            }

            return ValueTask.FromResult(result);
        }
    }
}
