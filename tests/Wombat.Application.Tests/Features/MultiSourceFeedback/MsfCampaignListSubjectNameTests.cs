using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// The coordinator's campaign list names each campaign's subject. It used to print their user id. (T142)
/// </summary>
public sealed class MsfCampaignListSubjectNameTests
{
    private const int HostInstitution = 1;
    private const int OtherInstitution = 2;

    [Fact]
    public async Task EachCampaignNamesItsSubject_InOneLookupForTheCampaignsThisCoordinatorRuns()
    {
        await using var db = NewDb();
        await SeedCampaignAsync(db, "amara", HostInstitution);
        await SeedCampaignAsync(db, "amara", HostInstitution);
        await SeedCampaignAsync(db, "departed-trainee", HostInstitution);
        await SeedCampaignAsync(db, "gugu", OtherInstitution);
        var users = new FakeUserDirectory(("amara", "Amara Okafor"), ("gugu", "Gugu Zulu"));

        var campaigns = await new ListMsfCampaignsForCoordinatorQueryHandler(db, users).Handle(
            new ListMsfCampaignsForCoordinatorQuery(TestPrincipals.Coordinator(HostInstitution)), CancellationToken.None);

        campaigns.Select(campaign => campaign.SubjectName).Should().BeEquivalentTo(
            ["Amara Okafor", "Amara Okafor", "departed-trainee"],
            "a subject who no longer exists is shown by id");
        users.Lookups.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(["amara", "departed-trainee"], "the other institution's campaign is not listed");
    }

    private static async Task SeedCampaignAsync(ApplicationDbContext db, string subjectUserId, int institutionId)
    {
        if (!await db.Set<TraineeProfile>().AnyAsync(profile => profile.UserId == subjectUserId))
        {
            db.Set<TraineeProfile>().Add(new TraineeProfile
            {
                UserId = subjectUserId,
                InstitutionId = institutionId,
                CurriculumId = 1,
                ProgrammeStartDate = new DateOnly(2025, 1, 1),
                ExpectedCompletionDate = new DateOnly(2029, 1, 1),
                IsActive = true
            });
        }

        db.MsfCampaigns.Add(new MsfCampaign
        {
            SubjectUserId = subjectUserId,
            CreatedByUserId = "coordinator-user",
            CreatedOn = DateTime.UtcNow,
            OpensOn = new DateOnly(2026, 9, 1),
            ClosesOn = new DateOnly(2026, 9, 15),
            MinimumResponses = 1,
            MinimumCategoryResponses = 1,
            MinimumRespondentCategories = 1,
            State = MsfCampaignState.Open,
            Template = new MsfTemplate { Name = "Annual MSF" }
        });
        await db.SaveChangesAsync();
    }

    private static ApplicationDbContext NewDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
