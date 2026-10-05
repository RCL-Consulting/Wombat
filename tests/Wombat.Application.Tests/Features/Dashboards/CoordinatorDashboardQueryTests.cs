using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Dashboards.Coordinator;
using Wombat.Application.Scheduling;
using Wombat.Application.Tests.Scheduling;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Domain.Invitations;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling;
using Wombat.Infrastructure.Scheduling.Jobs;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.Dashboards;

/// <summary>
/// The Coordinator's Waiting for assessors card and the nightly assessor nudge read one predicate (T297, T358): a request
/// the nudge mails a named assessor about is on the card, since both read "awaiting a reviewer" through
/// <c>ActivityWaiting</c> and both need a named nominee (E3). The card's other rules are Waiting for assessors' own
/// (<c>WaitingForAssessorsTests</c>) and the Home's composition is <c>OversightHomesQueryTests</c>'.
/// </summary>
/// <remarks>
/// Until T358 the card was "Stalled requests", any activity awaiting a reviewer untouched for the stall days; until T297
/// it read the literal state <c>submitted</c>, so no stalled Mini-CEX ever reached it, though the nudge mailed about it the
/// same morning (Step 3.30).
/// </remarks>
public sealed class CoordinatorDashboardQueryTests
{
    private const int InstitutionId = 1;
    private const int MiniCexTypeId = 1;
    private const int PortfolioReviewTypeId = 2;

    /// <summary>
    /// The shared predicate (T297): a request the assessor nudge writes about is on the card. Both read "awaiting a
    /// reviewer" through <c>ActivityWaiting</c>, and both need the named nominee the nudge mails (E3).
    /// </summary>
    [Fact]
    public async Task AnActivityTheAssessorNudgeMailsAbout_IsOnTheCard()
    {
        var emailSender = new RecordingEmailSender();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(dbName));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IEmailSender>(_ => emailSender);
        services.AddSingleton<ScheduledJobMailTally>();
        await using var provider = services.BuildServiceProvider();

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Institutions.Add(new Wombat.Domain.Institutions.Institution { Id = InstitutionId, Name = "Kgosi Kgari Teaching Hospital" });
            SeedTypes(db);
            AddActivity(db, 21, MiniCexTypeId, "requested", "trainee-1", daysAgo: 8, assessorId: "assessor-zulu");
            AddActivity(db, 10, PortfolioReviewTypeId, "submitted", "trainee-2", daysAgo: 8, assessorId: "assessor-patel");
            NomineeSeed.AddUser(db, "trainee-1", InstitutionId, WombatRoles.Trainee);
            NomineeSeed.AddUser(db, "trainee-2", InstitutionId, WombatRoles.Trainee);
            NomineeSeed.AddUser(db, "assessor-zulu", InstitutionId, WombatRoles.Assessor);
            NomineeSeed.AddUser(db, "assessor-patel", InstitutionId, WombatRoles.Assessor);
            await db.SaveChangesAsync();
        }

        await new AssessorPendingNudgeJob(
                provider.GetRequiredService<IServiceScopeFactory>(),
                Microsoft.Extensions.Options.Options.Create(new Wombat.Application.Common.Options.DashboardThresholds()))
            .ExecuteAsync(new ScheduledJobContext(DateTime.UtcNow, new CapturingLogger()), CancellationToken.None);

        emailSender.Recipients.Should().BeEquivalentTo(["assessor-zulu@test.local", "assessor-patel@test.local"]);
        emailSender.Sent.Single(message => message.To == "assessor-zulu@test.local").TextBody.Should().Contain("Mini-CEX (Paediatrics)");

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var result = await Handle(db);

            result.Waiting!.Items.Select(item => item.Id).Should().BeEquivalentTo([21, 10]);
        }
    }

    private static async Task<CoordinatorDashboardSummaryDto> Handle(ApplicationDbContext db)
        => await new GetCoordinatorDashboardSummaryQueryHandler(
                db,
                new FakeUserDirectory(("trainee-1", "Nomsa Mahlangu"), ("trainee-2", "Pieter du Plessis"),
                    ("assessor-zulu", "Thandi Zulu"), ("assessor-patel", "Mohammed Patel")),
                new ReminderRecipients(db),
                Options.Create(new DashboardThresholds()),
                TimeProvider.System)
            .Handle(new GetCoordinatorDashboardSummaryQuery(CreatePrincipal("coord-1", InstitutionId)), CancellationToken.None);

    /// <summary>The shipped Mini-CEX, waiting in <c>requested</c>, and portfolio review, waiting in <c>submitted</c>.</summary>
    private static void SeedTypes(ApplicationDbContext db)
    {
        ShippedSeeds.AddType(db, MiniCexTypeId, "mini_cex_cpsa", "Mini-CEX (Paediatrics)");
        ShippedSeeds.AddType(db, PortfolioReviewTypeId, "portfolio_review_cpsa", "Portfolio and Logbook Review (Paediatrics)");
    }

    private static void AddActivity(
        ApplicationDbContext db,
        int id,
        int typeId,
        string state,
        string subject,
        int daysAgo,
        int institutionId = InstitutionId,
        string assessorId = "assessor-1")
    {
        var updatedOn = DateTime.UtcNow.AddDays(-daysAgo).AddHours(-1);
        db.Activities.Add(new Activity
        {
            Id = id, ActivityTypeId = typeId, SchemaVersion = 1,
            SubjectUserId = subject, CreatedByUserId = subject, CurrentState = state,
            DataJson = $$"""{ "assessor_user_id": "{{assessorId}}" }""",
            InstitutionId = institutionId,
            CreatedOn = updatedOn.AddDays(-1), UpdatedOn = updatedOn
        });
    }

    private static ClaimsPrincipal CreatePrincipal(string userId, int? institutionId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, "Coordinator")
        };
        if (institutionId.HasValue)
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, institutionId.Value.ToString()));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}
