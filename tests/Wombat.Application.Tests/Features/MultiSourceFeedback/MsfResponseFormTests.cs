using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wombat.Application.Audit;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Epas;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// What a respondent's link opens, and what its submit accepts: whom the feedback is for, the one deadline, the points of
/// each scale, and nothing else. (T205)
/// </summary>
/// <remarks>
/// The web app's respondent page and the Api's endpoint both ask <see cref="GetMsfResponseFormQuery" /> and send
/// <see cref="SubmitMsfResponseCommand" />, so these rules hold on both.
/// </remarks>
public sealed class MsfResponseFormTests
{
    private const string SubjectUserId = "trainee-1";
    private const string TraineeName = "Thandi Nkosi";

    private readonly InvitationTokenService _tokenService = new();

    [Fact]
    public async Task TheForm_NamesTheTraineeTheInvitationNamed()
    {
        var token = _tokenService.GenerateSelectorToken().Token;
        await using var dbContext = CreateDbContext();
        await SeedOpenInvitationAsync(dbContext, token);

        var form = await QueryAsync(dbContext, token, new FakeUserDirectory((SubjectUserId, $"  {TraineeName} ")));

        form.TraineeName.Should().Be(TraineeName);
        form.TemplateName.Should().Be("Annual MSF");
        form.Kind.Should().Be(MsfTemplateKind.Msf);
    }

    /// <summary>
    /// A learner's link opens learner feedback, and says so, so the page can ask about the trainee's teaching as the
    /// invitation did (T164). Where the learner was taught is the coordinator's record on the invitation: the form
    /// neither carries it nor asks it.
    /// </summary>
    [Fact]
    public async Task ALearnersForm_IsLearnerFeedback_AndCarriesNothingOfWhereTheyWereTaught()
    {
        var token = _tokenService.GenerateSelectorToken().Token;
        await using var dbContext = CreateDbContext();
        await SeedOpenInvitationAsync(
            dbContext,
            token,
            kind: MsfTemplateKind.LearnerFeedback,
            category: MsfRespondentCategory.Learner,
            teachingContext: "Neonatal night teaching");

        var form = await QueryAsync(dbContext, token);

        form.Kind.Should().Be(MsfTemplateKind.LearnerFeedback);
        form.RespondentCategory.Should().Be(MsfRespondentCategory.Learner);
        form.Questions.Select(question => question.Prompt).Should().Equal(["Judgement", "Comment"], "the template's questions, and no other");
        System.Text.Json.JsonSerializer.Serialize(form).Should().NotContain("Neonatal night teaching");
    }

    [Fact]
    public async Task ATraineeWithNoNameOnRecord_IsLeftUnnamed_NeverNamedByTheirId()
    {
        var token = _tokenService.GenerateSelectorToken().Token;
        await using var dbContext = CreateDbContext();
        await SeedOpenInvitationAsync(dbContext, token);

        var form = await QueryAsync(dbContext, token, FakeUserDirectory.Empty);

        form.TraineeName.Should().BeNull();
    }

    [Theory]
    [InlineData(10, 3, 3)]
    [InlineData(3, 10, 3)]
    public async Task TheLastDayToRespond_IsTheEarlierOfTheWindowsCloseAndTheLinksExpiry(
        int closesInDays,
        int expiresInDays,
        int lastDayInDays)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var token = _tokenService.GenerateSelectorToken().Token;
        await using var dbContext = CreateDbContext();
        await SeedOpenInvitationAsync(dbContext, token, closesOn: today.AddDays(closesInDays), expiresOn: today.AddDays(expiresInDays));

        var form = await QueryAsync(dbContext, token);

        form.LastDayToRespond.Should().Be(today.AddDays(lastDayInDays), "the page gives the one deadline the invitation gave (T202)");
    }

    [Fact]
    public async Task AScaleQuestionThatNamesNoScale_IsAnsweredOnTheDefaultFivePoints_AndACommentOnNone()
    {
        var token = _tokenService.GenerateSelectorToken().Token;
        await using var dbContext = CreateDbContext();
        await SeedOpenInvitationAsync(dbContext, token);

        var form = await QueryAsync(dbContext, token);

        form.Questions.Select(question => question.Prompt).Should().Equal(["Judgement", "Comment"], "in the template's order");
        form.Questions[0].ScalePoints.Should().Equal(MsfRatingScale.Default);
        form.Questions[0].ScalePoints.Select(point => point.Value).Should().Equal([1, 2, 3, 4, 5]);
        form.Questions[1].ScalePoints.Should().BeEmpty();
    }

    [Fact]
    public async Task AScaleQuestionThatNamesAScale_IsAnsweredOnItsLevels_StoredByOrder()
    {
        var token = _tokenService.GenerateSelectorToken().Token;
        await using var dbContext = CreateDbContext();
        var scale = new EntrustmentScale
        {
            Name = "Three rungs",
            Levels =
            [
                new EntrustmentLevel { Order = 3, Label = "3a", Description = "Indirect supervision" },
                new EntrustmentLevel { Order = 1, Label = "1", Description = "Observe only" },
                new EntrustmentLevel { Order = 2, Label = "2", Description = null }
            ]
        };
        dbContext.EntrustmentScales.Add(scale);
        await dbContext.SaveChangesAsync();
        await SeedOpenInvitationAsync(dbContext, token, scaleId: scale.Id);

        var form = await QueryAsync(dbContext, token);

        form.Questions[0].ScalePoints.Should().Equal(
            [
                new MsfScalePointDto(1, "1", "Observe only"),
                new MsfScalePointDto(2, "2", null),
                new MsfScalePointDto(3, "3a", "Indirect supervision")
            ],
            "lowest first, each stored as its order, as every rating in the product is");
    }

    /// <summary>
    /// Anonymity runs one way: the respondent is hidden from the trainee, and learns nothing of anyone else who answers.
    /// The form is everything the page can show, so it must carry nothing about other responses or respondents.
    /// </summary>
    [Fact]
    public void TheForm_CarriesNothingAboutOtherRespondentsOrTheirResponses()
    {
        typeof(MsfResponseFormDto).GetProperties().Select(property => property.Name).Should().BeEquivalentTo(
            ["TemplateName", "Kind", "TraineeName", "LastDayToRespond", "RespondentCategory", "Questions"],
            "a count, a threshold or another response added here would reach every respondent's page");
        typeof(MsfResponsePromptDto).GetProperties().Select(property => property.Name).Should().BeEquivalentTo(
            ["QuestionId", "Prompt", "Type", "Required", "ScalePoints"]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(999)]
    [InlineData(-1)]
    public async Task ARatingThatIsNotAPointOfTheScale_IsRefusedAsIncomplete_AndUsesNothingUp(int value)
    {
        var token = _tokenService.GenerateSelectorToken().Token;
        await using var dbContext = CreateDbContext();
        var (scaleId, commentId) = await SeedOpenInvitationAsync(dbContext, token);

        var refusal = await SubmitRefusedAsync(dbContext, token, [new(scaleId, value, null), new(commentId, null, "Fine.")]);

        refusal.Reason.Should().Be(MsfResponseRefusal.AnswersIncomplete);
        refusal.Message.Should().Be("Choose one of the points on the scale for 'Judgement'.");
        await ShouldHaveUsedNothingAsync(dbContext);

        await SubmitAsync(dbContext, token, [new(scaleId, 5, null)]);
        (await dbContext.MsfResponseAnswers.SingleAsync()).ScaleValue.Should().Be(5, "the scale's top point is accepted");
    }

    [Fact]
    public async Task ARatingOnANamedScale_IsHeldToThatScalesLevels()
    {
        var token = _tokenService.GenerateSelectorToken().Token;
        await using var dbContext = CreateDbContext();
        var scale = new EntrustmentScale
        {
            Name = "Two rungs",
            Levels = [new EntrustmentLevel { Order = 1, Label = "1" }, new EntrustmentLevel { Order = 2, Label = "2" }]
        };
        dbContext.EntrustmentScales.Add(scale);
        await dbContext.SaveChangesAsync();
        var (scaleId, _) = await SeedOpenInvitationAsync(dbContext, token, scaleId: scale.Id);

        var refusal = await SubmitRefusedAsync(dbContext, token, [new(scaleId, 3, null)]);

        refusal.Reason.Should().Be(MsfResponseRefusal.AnswersIncomplete, "3 is a point of the default scale, not of this one");
        await ShouldHaveUsedNothingAsync(dbContext);
    }

    [Fact]
    public async Task ACommentLongerThanTheColumnHolds_IsRefusedForTheRespondentToShorten_AndUsesNothingUp()
    {
        var token = _tokenService.GenerateSelectorToken().Token;
        await using var dbContext = CreateDbContext();
        var (scaleId, commentId) = await SeedOpenInvitationAsync(dbContext, token);
        var tooLong = new string('x', MsfResponseAnswer.LongTextMaxLength + 1);

        var refusal = await SubmitRefusedAsync(dbContext, token, [new(scaleId, 4, null), new(commentId, null, tooLong)]);

        refusal.Reason.Should().Be(MsfResponseRefusal.AnswersIncomplete);
        refusal.Message.Should().Be("The comment for 'Comment' is longer than 4000 characters. Please shorten it.");
        await ShouldHaveUsedNothingAsync(dbContext);

        // Surrounding whitespace is trimmed before it is stored, so it does not count against the respondent.
        var longest = $"  {new string('x', MsfResponseAnswer.LongTextMaxLength)}\n";
        await SubmitAsync(dbContext, token, [new(scaleId, 4, null), new(commentId, null, longest)]);
        (await dbContext.MsfResponseAnswers.SingleAsync(answer => answer.QuestionId == commentId)).LongText
            .Should().HaveLength(MsfResponseAnswer.LongTextMaxLength);
    }

    /// <summary>
    /// A text box counts a line break as one character, and the browser posts it as two (CRLF). A comment the box let
    /// the respondent type must not then be refused as too long, and is stored with the line breaks it was typed with.
    /// (T205 review)
    /// </summary>
    [Fact]
    public async Task ACommentTheTextBoxAccepted_IsNotRefusedForItsLineBreaks_AndIsStoredWithOneCharacterEach()
    {
        var token = _tokenService.GenerateSelectorToken().Token;
        await using var dbContext = CreateDbContext();
        var (scaleId, commentId) = await SeedOpenInvitationAsync(dbContext, token);
        const int lineBreaks = 10;
        var line = new string('x', (MsfResponseAnswer.LongTextMaxLength - lineBreaks) / (lineBreaks + 1));
        var typed = string.Join('\n', Enumerable.Repeat(line, lineBreaks + 1));
        typed += new string('x', MsfResponseAnswer.LongTextMaxLength - typed.Length);
        typed.Should().HaveLength(MsfResponseAnswer.LongTextMaxLength, "guard: as long as the text box allows, counted as it counts");
        var posted = typed.Replace("\n", "\r\n", StringComparison.Ordinal);

        await SubmitAsync(dbContext, token, [new(scaleId, 4, null), new(commentId, null, posted)]);

        (await dbContext.MsfResponseAnswers.SingleAsync(answer => answer.QuestionId == commentId)).LongText
            .Should().Be(typed, "each line break is stored as the one character the text box counted");
    }

    /// <summary>
    /// The page marks the question a refusal is about (T205 review). Every refusal of one question's answer says which.
    /// </summary>
    [Fact]
    public async Task ARefusalOfOneQuestionsAnswer_NamesThatQuestion()
    {
        var token = _tokenService.GenerateSelectorToken().Token;
        await using var dbContext = CreateDbContext();
        var (scaleId, commentId) = await SeedOpenInvitationAsync(dbContext, token);

        (await SubmitRefusedAsync(dbContext, token, [new(commentId, null, "No rating.")])).QuestionId
            .Should().Be(scaleId, "the required rating is missing");
        (await SubmitRefusedAsync(dbContext, token, [new(scaleId, 9, null)])).QuestionId
            .Should().Be(scaleId, "the rating is off the scale");
        (await SubmitRefusedAsync(dbContext, token, [new(scaleId, 4, null), new(commentId, null, new string('x', 4001))])).QuestionId
            .Should().Be(commentId, "the comment is too long");
        (await SubmitRefusedAsync(dbContext, token, [new(scaleId, 4, null), new(scaleId, 5, null)])).QuestionId
            .Should().BeNull("a duplicate answer is not one question's to put right");
    }

    /// <summary>
    /// The page is public, so a link that no token could be is refused before the database is asked anything: this
    /// database throws on any use. (T205 review) So is a link mailed before T163: forty-three characters, the shape of
    /// every token then, with no selector to find a row by.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("not-a-token-anyone-was-sent")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA+")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA+")]
    [InlineData("AAAAAAAAAAAAAAA.AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    public async Task ALinkNoTokenCouldBe_IsNotRecognised_WithoutTheDatabaseBeingAsked(string token)
    {
        var database = new Mock<IApplicationDbContext>(MockBehavior.Strict).Object;

        var load = () => new GetMsfResponseFormQueryHandler(database, _tokenService, FakeUserDirectory.Empty)
            .Handle(new GetMsfResponseFormQuery(token), CancellationToken.None);
        var submit = () => new SubmitMsfResponseCommandHandler(database, _tokenService)
            .Handle(new SubmitMsfResponseCommand(token, [new(1, 3, null)]), CancellationToken.None);

        (await load.Should().ThrowAsync<MsfResponseRefusedException>()).Which.Reason.Should().Be(MsfResponseRefusal.LinkNotRecognised);
        (await submit.Should().ThrowAsync<MsfResponseRefusedException>()).Which.Reason.Should().Be(MsfResponseRefusal.LinkNotRecognised);
    }

    [Fact]
    public async Task ALinkOfSeveralKilobytes_IsNotRecognised_WithoutTheDatabaseBeingAsked()
        => await ALinkNoTokenCouldBe_IsNotRecognised_WithoutTheDatabaseBeingAsked(new string('A', 8 * 1024));

    /// <summary>
    /// A respondent signed in to Wombat in the same browser must not be named on their submission's audit row (T205
    /// review). The pipeline honours the marker (AuditPipelineBehaviorTests); this holds the command to it.
    /// </summary>
    [Fact]
    public void TheSubmission_IsAuditedWithoutItsSender()
        => typeof(SubmitMsfResponseCommand).Should().Implement<IAnonymousAuditedCommand>();

    /// <summary>
    /// Every refusal a respondent can meet has a 4xx answer, on the page and at the Api alike, so none can surface as a
    /// 500. (T202 review; the mapping moved here from the Api's endpoint in T205, and this test with it.)
    /// </summary>
    [Fact]
    public void EveryRefusal_HasAClientErrorAnswer_WhereverTheRespondentMeetsIt()
    {
        foreach (var reason in Enum.GetValues<MsfResponseRefusal>())
        {
            var (status, title) = MsfResponseRefusals.Describe(reason);
            status.Should().BeInRange(400, 499, reason.ToString());
            title.Should().NotBeNullOrWhiteSpace(reason.ToString());
        }
    }

    private async Task<MsfResponseFormDto> QueryAsync(
        ApplicationDbContext dbContext,
        string token,
        FakeUserDirectory? users = null)
        => await new GetMsfResponseFormQueryHandler(dbContext, _tokenService, users ?? new FakeUserDirectory((SubjectUserId, TraineeName)))
            .Handle(new GetMsfResponseFormQuery(token), CancellationToken.None);

    private async Task SubmitAsync(ApplicationDbContext dbContext, string token, List<SubmitMsfResponseAnswerItem> answers)
    {
        await new SubmitMsfResponseCommandHandler(dbContext, _tokenService)
            .Handle(new SubmitMsfResponseCommand(token, answers), CancellationToken.None);
        dbContext.ChangeTracker.Clear();
    }

    private async Task<MsfResponseRefusedException> SubmitRefusedAsync(
        ApplicationDbContext dbContext,
        string token,
        List<SubmitMsfResponseAnswerItem> answers)
    {
        var submit = () => new SubmitMsfResponseCommandHandler(dbContext, _tokenService)
            .Handle(new SubmitMsfResponseCommand(token, answers), CancellationToken.None);

        return (await submit.Should().ThrowAsync<MsfResponseRefusedException>()).Which;
    }

    /// <summary>
    /// The audit pipeline saves the request's context from its catch, so a refusal must leave nothing pending for it to
    /// commit: the link is not spent and no answer is stored.
    /// </summary>
    private static async Task ShouldHaveUsedNothingAsync(ApplicationDbContext dbContext)
    {
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
        (await dbContext.MsfInvitations.SingleAsync()).RespondedOn.Should().BeNull("a refused submission uses nothing up");
        (await dbContext.MsfResponses.CountAsync()).Should().Be(0);
        (await dbContext.MsfResponseAnswers.CountAsync()).Should().Be(0);
    }

    private async Task<(int ScaleId, int CommentId)> SeedOpenInvitationAsync(
        ApplicationDbContext dbContext,
        string token,
        DateOnly? closesOn = null,
        DateOnly? expiresOn = null,
        int? scaleId = null,
        MsfTemplateKind kind = MsfTemplateKind.Msf,
        MsfRespondentCategory category = MsfRespondentCategory.Nurse,
        string? teachingContext = null)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var scale = new MsfQuestion { Order = 1, Prompt = "Judgement", Type = MsfQuestionType.Scale, ScaleId = scaleId, Required = true };
        var comment = new MsfQuestion { Order = 2, Prompt = "Comment", Type = MsfQuestionType.LongText, Required = false };

        dbContext.MsfInvitations.Add(new MsfInvitation
        {
            Campaign = new MsfCampaign
            {
                SubjectUserId = SubjectUserId,
                CreatedByUserId = "coord-1",
                CreatedOn = DateTime.UtcNow,
                OpensOn = today.AddDays(-1),
                ClosesOn = closesOn ?? today.AddDays(3),
                State = MsfCampaignState.Open,
                // Added out of order, so the form's order is the template's, not the insert's.
                Template = new MsfTemplate { Name = "Annual MSF", Kind = kind, Questions = [comment, scale] }
            },
            RespondentEmail = "respondent@example.test",
            RespondentCategory = category,
            TeachingContext = teachingContext,
            TokenSelector = _tokenService.SelectorOf(token),
            TokenHash = _tokenService.HashToken(token),
            IssuedOn = DateTime.UtcNow,
            ExpiresOn = expiresOn ?? today.AddDays(10)
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
