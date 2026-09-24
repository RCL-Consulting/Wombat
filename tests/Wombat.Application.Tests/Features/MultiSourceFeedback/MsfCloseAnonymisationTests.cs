using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Scheduling;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling.Jobs;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// A campaign closes in two places, the coordinator's close command and the hourly auto-close job, and both anonymise
/// through one routine: <see cref="MsfCampaign.Close" />, which calls <see cref="MsfInvitation.Anonymize" /> for every
/// invitation. (T184)
/// </summary>
/// <remarks>
/// Until T184 the job carried its own copy of the command's anonymise routine, free to drift from it. The copies had
/// already drifted in one detail: each stamped <c>AnonymizedOn</c> from its own clock read rather than the close time.
/// Anonymising at the close time, in both paths, is the observable mark of the one routine.
/// </remarks>
public sealed class MsfCloseAnonymisationTests
{
    private const string Respondent = "Nurse-1@Example.test";

    [Fact]
    public async Task TheCloseCommand_AndTheAutoCloseJob_AnonymiseAlike_AtTheCloseTime()
    {
        var databaseName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(databaseName));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        await using var provider = services.BuildServiceProvider();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        int byCommand, byJob;
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            byCommand = await SeedOpenCampaignAsync(db, closesOn: today.AddDays(7));
            byJob = await SeedOpenCampaignAsync(db, closesOn: today.AddDays(-3));
        }

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await new CloseMsfCampaignCommandHandler(db, new MsfAggregationService())
                .Handle(new CloseMsfCampaignCommand(byCommand, TestPrincipals.Administrator()), CancellationToken.None);
        }

        // A job run whose clock is an hour behind the wall clock, so a routine reading its own clock would show.
        var jobClock = DateTime.UtcNow.AddHours(-1);
        await new MsfCampaignAutoCloseJob(provider.GetRequiredService<IServiceScopeFactory>())
            .ExecuteAsync(new ScheduledJobContext(jobClock, NullLogger.Instance), CancellationToken.None);

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var campaigns = await db.MsfCampaigns.AsNoTracking()
                .Include(campaign => campaign.Invitations)
                .ToDictionaryAsync(campaign => campaign.Id);

            campaigns[byJob].ClosedOn.Should().Be(jobClock);

            foreach (var campaign in campaigns.Values)
            {
                campaign.State.Should().Be(MsfCampaignState.UnderReview);
                var invitation = campaign.Invitations.Single();
                invitation.RespondentEmail.Should().BeNull();
                invitation.AnonymizedOn.Should().Be(campaign.ClosedOn, "a respondent is anonymised at the close");
            }

            campaigns[byJob].Invitations.Single().RespondentEmailHash
                .Should().NotBeNullOrWhiteSpace()
                .And.Be(campaigns[byCommand].Invitations.Single().RespondentEmailHash, "one routine, one hash");
        }
    }

    private static async Task<int> SeedOpenCampaignAsync(ApplicationDbContext db, DateOnly closesOn)
    {
        var campaign = new MsfCampaign
        {
            SubjectUserId = "trainee-1",
            CreatedByUserId = "coordinator-user",
            CreatedOn = DateTime.UtcNow.AddDays(-30),
            OpensOn = closesOn.AddDays(-14),
            ClosesOn = closesOn,
            State = MsfCampaignState.Open,
            OpenedOn = DateTime.UtcNow.AddDays(-14),
            Template = new MsfTemplate { Name = "Annual MSF", IsActive = true },
            Invitations =
            [
                new MsfInvitation
                {
                    RespondentEmail = Respondent,
                    RespondentCategory = MsfRespondentCategory.Nurse,
                    TokenHash = "hash",
                    IssuedOn = DateTime.UtcNow.AddDays(-14),
                    ExpiresOn = closesOn.AddDays(7)
                }
            ]
        };

        db.MsfCampaigns.Add(campaign);
        await db.SaveChangesAsync();
        return campaign.Id;
    }
}
