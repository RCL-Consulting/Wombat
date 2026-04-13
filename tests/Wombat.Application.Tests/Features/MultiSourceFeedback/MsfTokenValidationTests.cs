using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

public sealed class MsfTokenValidationTests
{
    private readonly InvitationTokenService _tokenService = new();

    [Theory]
    [InlineData(true, false, false, "expired")]
    [InlineData(false, true, false, "revoked")]
    [InlineData(false, false, true, "used")]
    public async Task ResponseTokenValidation_RejectsInvalidLinks(bool expired, bool revoked, bool used, string expectedMessagePart)
    {
        var token = _tokenService.GenerateToken();

        await using var dbContext = CreateDbContext();
        dbContext.MsfInvitations.Add(new MsfInvitation
        {
            Campaign = new MsfCampaign
            {
                SubjectUserId = "trainee-1",
                CreatedByUserId = "coord-1",
                CreatedOn = DateTime.UtcNow,
                OpensOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                ClosesOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
                State = MsfCampaignState.Open,
                Template = new MsfTemplate
                {
                    Name = "MSF",
                    Questions = [new MsfQuestion { Order = 1, Prompt = "Judgement", Type = MsfQuestionType.Scale, Required = true }]
                }
            },
            RespondentEmail = "respondent@example.test",
            RespondentCategory = MsfRespondentCategory.Other,
            TokenHash = _tokenService.HashToken(token),
            IssuedOn = DateTime.UtcNow,
            ExpiresOn = expired ? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)) : DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            RevokedOn = revoked ? DateTime.UtcNow : null,
            RespondedOn = used ? DateTime.UtcNow : null
        });

        await dbContext.SaveChangesAsync();

        var queryHandler = new GetMsfResponseFormQueryHandler(dbContext, _tokenService);
        var commandHandler = new SubmitMsfResponseCommandHandler(dbContext, _tokenService);

        var getAct = () => queryHandler.Handle(new GetMsfResponseFormQuery(token), CancellationToken.None);
        var submitAct = () => commandHandler.Handle(
            new SubmitMsfResponseCommand(token, [new SubmitMsfResponseAnswerItem(1, 4, null)]),
            CancellationToken.None);

        await getAct.Should().ThrowAsync<InvalidOperationException>().WithMessage($"*{expectedMessagePart}*");
        await submitAct.Should().ThrowAsync<InvalidOperationException>().WithMessage($"*{expectedMessagePart}*");
    }

    [Fact]
    public async Task SubmitResponse_MarksInvitationUsed_AndPersistsAnswers()
    {
        var token = _tokenService.GenerateToken();

        await using var dbContext = CreateDbContext();
        dbContext.MsfInvitations.Add(new MsfInvitation
        {
            Campaign = new MsfCampaign
            {
                SubjectUserId = "trainee-1",
                CreatedByUserId = "coord-1",
                CreatedOn = DateTime.UtcNow,
                OpensOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                ClosesOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
                State = MsfCampaignState.Open,
                Template = new MsfTemplate
                {
                    Name = "MSF",
                    Questions =
                    [
                        new MsfQuestion { Order = 1, Prompt = "Judgement", Type = MsfQuestionType.Scale, Required = true },
                        new MsfQuestion { Order = 2, Prompt = "Comment", Type = MsfQuestionType.LongText, Required = true }
                    ]
                }
            },
            RespondentEmail = "respondent@example.test",
            RespondentCategory = MsfRespondentCategory.Other,
            TokenHash = _tokenService.HashToken(token),
            IssuedOn = DateTime.UtcNow,
            ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
        });

        await dbContext.SaveChangesAsync();

        var invitation = await dbContext.MsfInvitations
            .Include(candidate => candidate.Campaign)
                .ThenInclude(campaign => campaign.Template)
                    .ThenInclude(template => template.Questions)
            .SingleAsync();

        var answers = invitation.Campaign.Template.Questions
            .OrderBy(question => question.Order)
            .Select(question => question.Type == MsfQuestionType.Scale
                ? new SubmitMsfResponseAnswerItem(question.Id, 5, null)
                : new SubmitMsfResponseAnswerItem(question.Id, null, "Thoughtful and reliable."))
            .ToList();

        await new SubmitMsfResponseCommandHandler(dbContext, _tokenService)
            .Handle(new SubmitMsfResponseCommand(token, answers), CancellationToken.None);

        var savedInvitation = await dbContext.MsfInvitations.SingleAsync();
        savedInvitation.RespondedOn.Should().NotBeNull();
        var response = await dbContext.MsfResponses.Include(candidate => candidate.Answers).SingleAsync();
        response.Answers.Should().HaveCount(2);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
