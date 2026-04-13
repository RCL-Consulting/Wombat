using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.Activities;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling.Jobs;

namespace Wombat.Application.Tests.Scheduling;

public sealed class ActivityDraftNudgeJobTests
{
    [Fact]
    public async Task ExecuteAsync_SendsEmail_ForStaleDrafts()
    {
        var dbName = Guid.NewGuid().ToString();
        var emailSender = new Mock<IEmailSender>();

        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(dbName));
        services.AddScoped<IApplicationDbContext>(p => p.GetRequiredService<ApplicationDbContext>());
        services.AddScoped(_ => emailSender.Object);
        var provider = services.BuildServiceProvider();

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Set<ActivityType>().Add(new ActivityType
            {
                Id = 1,
                Key = "mini-cex",
                Name = "Mini-CEX",
                OwnerUserId = "admin",
                CreatedOn = DateTime.UtcNow
            });
            db.Set<Activity>().Add(new Activity
            {
                ActivityTypeId = 1,
                SubjectUserId = "trainee-1",
                CreatedByUserId = "trainee-1",
                CurrentState = "draft",
                CreatedOn = DateTime.UtcNow.AddDays(-20),
                UpdatedOn = DateTime.UtcNow.AddDays(-20)
            });
            db.Users.Add(new WombatIdentityUser
            {
                Id = "trainee-1",
                UserName = "trainee1@test.com",
                Email = "trainee1@test.com",
                FirstName = "Test",
                LastName = "Trainee",
                NormalizedUserName = "TRAINEE1@TEST.COM",
                NormalizedEmail = "TRAINEE1@TEST.COM"
            });
            await db.SaveChangesAsync();
        }

        var job = new ActivityDraftNudgeJob(provider.GetRequiredService<IServiceScopeFactory>());
        var context = new ScheduledJobContext(DateTime.UtcNow, NullLogger.Instance);

        await job.ExecuteAsync(context, CancellationToken.None);

        emailSender.Verify(s => s.SendAsync(
            It.Is<Wombat.Application.Common.Email.EmailMessage>(m =>
                m.To == "trainee1@test.com" &&
                m.Subject.Contains("draft")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_SkipsRecentDrafts()
    {
        var dbName = Guid.NewGuid().ToString();
        var emailSender = new Mock<IEmailSender>();

        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(dbName));
        services.AddScoped<IApplicationDbContext>(p => p.GetRequiredService<ApplicationDbContext>());
        services.AddScoped(_ => emailSender.Object);
        var provider = services.BuildServiceProvider();

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Set<ActivityType>().Add(new ActivityType
            {
                Id = 1,
                Key = "mini-cex",
                Name = "Mini-CEX",
                OwnerUserId = "admin",
                CreatedOn = DateTime.UtcNow
            });
            db.Set<Activity>().Add(new Activity
            {
                ActivityTypeId = 1,
                SubjectUserId = "trainee-1",
                CreatedByUserId = "trainee-1",
                CurrentState = "draft",
                CreatedOn = DateTime.UtcNow.AddDays(-2),
                UpdatedOn = DateTime.UtcNow.AddDays(-2)
            });
            await db.SaveChangesAsync();
        }

        var job = new ActivityDraftNudgeJob(provider.GetRequiredService<IServiceScopeFactory>());
        var context = new ScheduledJobContext(DateTime.UtcNow, NullLogger.Instance);

        await job.ExecuteAsync(context, CancellationToken.None);

        emailSender.Verify(s => s.SendAsync(
            It.IsAny<Wombat.Application.Common.Email.EmailMessage>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_GroupsByRecipient()
    {
        var dbName = Guid.NewGuid().ToString();
        var emailSender = new Mock<IEmailSender>();

        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(dbName));
        services.AddScoped<IApplicationDbContext>(p => p.GetRequiredService<ApplicationDbContext>());
        services.AddScoped(_ => emailSender.Object);
        var provider = services.BuildServiceProvider();

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Set<ActivityType>().Add(new ActivityType
            {
                Id = 1,
                Key = "mini-cex",
                Name = "Mini-CEX",
                OwnerUserId = "admin",
                CreatedOn = DateTime.UtcNow
            });
            db.Set<ActivityType>().Add(new ActivityType
            {
                Id = 2,
                Key = "dops",
                Name = "DOPS",
                OwnerUserId = "admin",
                CreatedOn = DateTime.UtcNow
            });
            db.Set<Activity>().Add(new Activity
            {
                ActivityTypeId = 1,
                SubjectUserId = "trainee-1",
                CreatedByUserId = "trainee-1",
                CurrentState = "draft",
                CreatedOn = DateTime.UtcNow.AddDays(-20),
                UpdatedOn = DateTime.UtcNow.AddDays(-20)
            });
            db.Set<Activity>().Add(new Activity
            {
                ActivityTypeId = 2,
                SubjectUserId = "trainee-1",
                CreatedByUserId = "trainee-1",
                CurrentState = "draft",
                CreatedOn = DateTime.UtcNow.AddDays(-18),
                UpdatedOn = DateTime.UtcNow.AddDays(-18)
            });
            db.Users.Add(new WombatIdentityUser
            {
                Id = "trainee-1",
                UserName = "trainee1@test.com",
                Email = "trainee1@test.com",
                FirstName = "Test",
                LastName = "Trainee",
                NormalizedUserName = "TRAINEE1@TEST.COM",
                NormalizedEmail = "TRAINEE1@TEST.COM"
            });
            await db.SaveChangesAsync();
        }

        var job = new ActivityDraftNudgeJob(provider.GetRequiredService<IServiceScopeFactory>());
        var context = new ScheduledJobContext(DateTime.UtcNow, NullLogger.Instance);

        await job.ExecuteAsync(context, CancellationToken.None);

        emailSender.Verify(s => s.SendAsync(
            It.IsAny<Wombat.Application.Common.Email.EmailMessage>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
