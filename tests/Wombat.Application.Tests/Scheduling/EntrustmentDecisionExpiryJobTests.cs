using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling.Jobs;

namespace Wombat.Application.Tests.Scheduling;

/// <summary>
/// T366, G5: the expiry job reads "today" on the South African calendar, as My authorisations does (S3, T325), so a run
/// after South African midnight, before 02:00 SAST, expires a STAR whose expiry date was yesterday there, and the states
/// row's Run now holds at any hour.
/// </summary>
public sealed class EntrustmentDecisionExpiryJobTests
{
    private static readonly DateOnly ExpiresOn = new(2026, 11, 2);

    [Fact]
    public async Task ARunAfterSouthAfricanMidnight_ExpiresAStarWhoseExpiryDateWasYesterdayThere()
    {
        // 22:30 UTC on 2026-11-02 is 00:30 SAST on 2026-11-03.
        var status = await RunAtAsync(new DateTime(2026, 11, 2, 22, 30, 0, DateTimeKind.Utc));

        status.Should().Be(EntrustmentDecisionStatus.Expired);
    }

    [Fact]
    public async Task ARunBeforeSouthAfricanMidnight_LeavesItInForce_OnItsLastDay()
    {
        // 21:30 UTC on 2026-11-02 is 23:30 SAST, still its expiry date.
        var status = await RunAtAsync(new DateTime(2026, 11, 2, 21, 30, 0, DateTimeKind.Utc));

        status.Should().Be(EntrustmentDecisionStatus.Active);
    }

    private static async Task<EntrustmentDecisionStatus> RunAtAsync(DateTime utcNow)
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(dbName));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddSingleton<IEmailSender>(new RecordingEmailSender());
        var provider = services.BuildServiceProvider();

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Set<Epa>().Add(new Epa { Id = 10, SubSpecialityId = 9, Code = "PAED-010", Title = "Leading a team", IsActive = true });
            var line = new CommitteeEvidence
            {
                Id = 501, ReviewId = 1, SourceType = CommitteeEvidenceSourceType.Activity, ActivityId = 101, EpaId = 10,
                EpaCode = "PAED-010", SourceLabel = "Mini-CEX #101", Summary = "State: completed.", ObservedOn = new DateOnly(2026, 9, 1)
            };
            db.Set<EntrustmentDecision>().Add(EntrustmentDecision.Issue(
                "trainee-1", 10, 4, new DateOnly(2026, 10, 13), ExpiresOn, 1, "chair-1", "Leads the team.",
                [EntrustmentEvidenceLink.FromSnapshot(line)]));
            await db.SaveChangesAsync();
        }

        await new EntrustmentDecisionExpiryJob(provider.GetRequiredService<IServiceScopeFactory>())
            .ExecuteAsync(new ScheduledJobContext(utcNow, NullLogger.Instance), CancellationToken.None);

        using var read = provider.CreateScope();
        return (await read.ServiceProvider.GetRequiredService<ApplicationDbContext>().Set<EntrustmentDecision>().SingleAsync()).Status;
    }
}
