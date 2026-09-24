using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// Answers that answer a question twice, or answer one the questionnaire does not ask, are refused as the respondent's
/// to fix, and use nothing up. (T202 review)
/// </summary>
/// <remarks>
/// Before T202 the first threw from <c>SingleOrDefault</c> and the second failed at the save (a foreign key), both as
/// faults the Api answered 500. They are <see cref="MsfResponseRefusal.AnswersIncomplete" /> refusals now, which the
/// respond endpoint answers 400 with the message below.
/// </remarks>
public sealed class MsfResponseAnswersRefusalTests
{
    private readonly InvitationTokenService _tokenService = new();

    public enum Fault
    {
        AQuestionAnsweredTwice,
        AQuestionTheFormDoesNotAsk
    }

    [Theory]
    [InlineData(Fault.AQuestionAnsweredTwice, "Each question can be answered once.")]
    [InlineData(Fault.AQuestionTheFormDoesNotAsk, "An answer names a question that is not on this questionnaire.")]
    public async Task AnswersThatDoNotFitTheForm_AreRefusedAsIncomplete_AndUseNothingUp(Fault fault, string expectedMessage)
    {
        var token = _tokenService.GenerateSelectorToken().Token;
        await using var dbContext = CreateDbContext();
        var (scaleId, commentId) = await SeedOpenInvitationAsync(dbContext, token);

        var complete = new List<SubmitMsfResponseAnswerItem>
        {
            new(scaleId, 4, null),
            new(commentId, null, "Consistent and helpful.")
        };

        List<SubmitMsfResponseAnswerItem> answers = fault switch
        {
            Fault.AQuestionAnsweredTwice => [.. complete, new SubmitMsfResponseAnswerItem(scaleId, 5, null)],
            Fault.AQuestionTheFormDoesNotAsk => [.. complete, new SubmitMsfResponseAnswerItem(scaleId + commentId + 1_000, 3, null)],
            _ => throw new ArgumentOutOfRangeException(nameof(fault))
        };

        var submit = () => new SubmitMsfResponseCommandHandler(dbContext, _tokenService)
            .Handle(new SubmitMsfResponseCommand(token, answers), CancellationToken.None);

        var refusal = (await submit.Should().ThrowAsync<MsfResponseRefusedException>()).Which;
        refusal.Reason.Should().Be(MsfResponseRefusal.AnswersIncomplete);
        refusal.Message.Should().Be(expectedMessage);

        // The audit pipeline saves the request's context from its catch, so nothing may be pending for it to commit.
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
        (await dbContext.MsfInvitations.SingleAsync()).RespondedOn.Should().BeNull("a refused submission uses nothing up");
        (await dbContext.MsfResponses.CountAsync()).Should().Be(0);
        (await dbContext.MsfResponseAnswers.CountAsync()).Should().Be(0);

        // And the link still takes the complete response.
        await new SubmitMsfResponseCommandHandler(dbContext, _tokenService)
            .Handle(new SubmitMsfResponseCommand(token, complete), CancellationToken.None);
        (await dbContext.MsfResponses.CountAsync()).Should().Be(1);
    }

    private async Task<(int ScaleId, int CommentId)> SeedOpenInvitationAsync(ApplicationDbContext dbContext, string token)
    {
        var scale = new MsfQuestion { Order = 1, Prompt = "Judgement", Type = MsfQuestionType.Scale, Required = true };
        var comment = new MsfQuestion { Order = 2, Prompt = "Comment", Type = MsfQuestionType.LongText, Required = false };

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
                Template = new MsfTemplate { Name = "MSF", Questions = [scale, comment] }
            },
            RespondentEmail = "respondent@example.test",
            RespondentCategory = MsfRespondentCategory.Other,
            TokenSelector = _tokenService.SelectorOf(token),
            TokenHash = _tokenService.HashToken(token),
            IssuedOn = DateTime.UtcNow,
            ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10))
        });

        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
        return (scale.Id, comment.Id);
    }

    private static ApplicationDbContext CreateDbContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
