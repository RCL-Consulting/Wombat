using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

public sealed class MsfCampaignAnonymityTests
{
    private readonly InvitationTokenService _tokenService = new();
    private readonly MsfAggregationService _aggregationService = new();

    [Fact]
    public async Task CloseCampaign_AnonymizesInvitationEmails_AndSuppressesSmallCategories()
    {
        await using var dbContext = CreateDbContext();
        var campaign = await SeedCampaignAsync(dbContext, minimumCategoryResponses: 3);

        await AddResponseAsync(dbContext, campaign, campaign.Invitations.Single(invitation => invitation.RespondentCategory == MsfRespondentCategory.Consultant), 4, "Strong performer.");
        await AddResponseAsync(dbContext, campaign, campaign.Invitations.Single(invitation => invitation.RespondentEmail == "nurse-1@example.test"), 5, "Great team member.");
        await AddResponseAsync(dbContext, campaign, campaign.Invitations.Single(invitation => invitation.RespondentEmail == "nurse-2@example.test"), 4, "Reliable.");

        var handler = new CloseMsfCampaignCommandHandler(dbContext, _aggregationService);
        var report = await handler.Handle(new CloseMsfCampaignCommand(campaign.Id), CancellationToken.None);

        var invitations = await dbContext.MsfInvitations.OrderBy(invitation => invitation.Id).ToListAsync();
        invitations.Should().OnlyContain(invitation => invitation.RespondentEmail == null);
        invitations.Should().OnlyContain(invitation => !string.IsNullOrWhiteSpace(invitation.RespondentEmailHash));

        report.TotalResponses.Should().Be(3);
        report.Categories.Should().ContainSingle(category => category.Category == MsfRespondentCategory.Consultant && category.IsSuppressed);
        report.Categories.Should().ContainSingle(category => category.Category == MsfRespondentCategory.Nurse && category.IsSuppressed);
    }

    [Fact]
    public async Task AggregateReport_ShowsCategoryDetails_WhenThresholdIsMet()
    {
        await using var dbContext = CreateDbContext();
        var campaign = await SeedCampaignAsync(dbContext, minimumCategoryResponses: 2);

        await AddResponseAsync(dbContext, campaign, campaign.Invitations.Single(invitation => invitation.RespondentEmail == "nurse-1@example.test"), 5, "Clear communicator.");
        await AddResponseAsync(dbContext, campaign, campaign.Invitations.Single(invitation => invitation.RespondentEmail == "nurse-2@example.test"), 3, "Needs pacing.");

        var report = await new CloseMsfCampaignCommandHandler(dbContext, _aggregationService)
            .Handle(new CloseMsfCampaignCommand(campaign.Id), CancellationToken.None);

        var nurseCategory = report.Categories.Single(category => category.Category == MsfRespondentCategory.Nurse);
        nurseCategory.IsSuppressed.Should().BeFalse();
        nurseCategory.Questions.Should().HaveCount(2);
        nurseCategory.Questions.Single(question => question.Type == MsfQuestionType.Scale).Scale!.Average.Should().BeApproximately(4d, 0.001);
        nurseCategory.Questions.Single(question => question.Type == MsfQuestionType.LongText).Comments.Should().Contain(["Clear communicator.", "Needs pacing."]);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private async Task<MsfCampaign> SeedCampaignAsync(ApplicationDbContext dbContext, int minimumCategoryResponses)
    {
        var template = new MsfTemplate
        {
            Name = "Annual MSF",
            Questions =
            [
                new MsfQuestion { Order = 1, Prompt = "Clinical judgement", Type = MsfQuestionType.Scale, Required = true },
                new MsfQuestion { Order = 2, Prompt = "Comments", Type = MsfQuestionType.LongText, Required = false }
            ]
        };

        var campaign = new MsfCampaign
        {
            SubjectUserId = "trainee-1",
            CreatedByUserId = "coord-1",
            CreatedOn = DateTime.UtcNow,
            OpensOn = DateOnly.FromDateTime(DateTime.UtcNow),
            ClosesOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            MinimumResponses = 2,
            MinimumCategoryResponses = minimumCategoryResponses,
            State = MsfCampaignState.Open,
            Template = template,
            Invitations =
            [
                CreateInvitation("consultant-1@example.test", MsfRespondentCategory.Consultant),
                CreateInvitation("nurse-1@example.test", MsfRespondentCategory.Nurse),
                CreateInvitation("nurse-2@example.test", MsfRespondentCategory.Nurse)
            ]
        };

        dbContext.MsfCampaigns.Add(campaign);
        await dbContext.SaveChangesAsync();
        return campaign;
    }

    private MsfInvitation CreateInvitation(string email, MsfRespondentCategory category)
    {
        var token = _tokenService.GenerateToken();
        return new MsfInvitation
        {
            RespondentEmail = email,
            RespondentCategory = category,
            TokenHash = _tokenService.HashToken(token),
            IssuedOn = DateTime.UtcNow,
            ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14))
        };
    }

    private static async Task AddResponseAsync(ApplicationDbContext dbContext, MsfCampaign campaign, MsfInvitation invitation, int scaleValue, string comment)
    {
        dbContext.MsfResponses.Add(new MsfResponse
        {
            CampaignId = campaign.Id,
            InvitationId = invitation.Id,
            SubmittedOn = DateTime.UtcNow,
            Answers =
            [
                new MsfResponseAnswer { QuestionId = campaign.Template.Questions.Single(question => question.Type == MsfQuestionType.Scale).Id, ScaleValue = scaleValue },
                new MsfResponseAnswer { QuestionId = campaign.Template.Questions.Single(question => question.Type == MsfQuestionType.LongText).Id, LongText = comment }
            ]
        });

        invitation.RespondedOn = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
    }
}
