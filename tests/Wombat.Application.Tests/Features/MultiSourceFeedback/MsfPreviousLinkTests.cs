using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// The link a reminder replaced still opens the questionnaire and takes the respondent's one response until their last
/// day to respond, and not after it; either link's answer spends both. (T214)
/// </summary>
/// <remarks>
/// Until T214 a reminder retired the link it replaced outright, and a respondent part-way through the questionnaire on
/// it lost their answers on submit, told only that the link was not recognised. The reminder job keeping the link it
/// replaces is <c>MsfInvitationExpiryReminderJobTests</c>; the lookup on PostgreSQL is
/// <c>MsfLinkSelectorPostgresTests</c>.
/// </remarks>
public sealed class MsfPreviousLinkTests
{
    private const string SubjectUserId = "trainee-1";

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly InvitationTokenService _tokens = new();

    /// <summary>
    /// The last day to respond is the day the window closes; the campaign stays open until the auto-close job runs the day
    /// after. The previous link is refused from that day, as expired, while the current link still works.
    /// </summary>
    [Theory]
    [InlineData(2, true)]
    [InlineData(0, true)]
    [InlineData(-1, false)]
    public async Task ThePreviousLink_OpensTheQuestionnaire_UntilTheLastDayToRespond(int closesInDays, bool opens)
    {
        await using var dbContext = CreateDbContext();
        var (previous, current, _) = await SeedRemindedInvitationAsync(dbContext, Today.AddDays(closesInDays));

        var load = () => QueryAsync(dbContext, previous.Token);

        if (opens)
        {
            (await load()).TemplateName.Should().Be("Annual MSF");
        }
        else
        {
            (await load.Should().ThrowAsync<MsfResponseRefusedException>()).Which.Reason.Should().Be(MsfResponseRefusal.LinkExpired);
        }

        (await QueryAsync(dbContext, current.Token)).TemplateName.Should().Be("Annual MSF", "the current link is judged as before T214");
    }

    [Fact]
    public async Task ThePreviousLink_PastTheLastDayToRespond_TakesNoResponse()
    {
        await using var dbContext = CreateDbContext();
        var (previous, _, questionId) = await SeedRemindedInvitationAsync(dbContext, Today.AddDays(-1));

        var submit = () => SubmitAsync(dbContext, previous.Token, questionId);

        (await submit.Should().ThrowAsync<MsfResponseRefusedException>()).Which.Reason.Should().Be(MsfResponseRefusal.LinkExpired);
        dbContext.ChangeTracker.Clear();
        (await dbContext.MsfResponses.CountAsync()).Should().Be(0);
    }

    /// <summary>
    /// Through either link, the answer is the invitation's one response: the other link is then refused, the current one
    /// as used, the previous one as no longer recognised, since the answer retired it.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AnAnswerThroughEitherLink_IsTheInvitationsOneResponse(bool throughPrevious)
    {
        await using var dbContext = CreateDbContext();
        var (previous, current, questionId) = await SeedRemindedInvitationAsync(dbContext, Today.AddDays(2));
        var (answeredThrough, other) = throughPrevious ? (previous, current) : (current, previous);

        await SubmitAsync(dbContext, answeredThrough.Token, questionId);
        dbContext.ChangeTracker.Clear();

        var stored = await dbContext.MsfInvitations.AsNoTracking().SingleAsync();
        stored.RespondedOn.Should().NotBeNull();
        stored.PreviousTokenSelector.Should().BeNull("the answer retires the previous link");
        stored.PreviousTokenHash.Should().BeNull();
        stored.TokenSelector.Should().Be(current.Selector);

        var again = () => SubmitAsync(dbContext, answeredThrough.Token, questionId);
        var otherSubmit = () => SubmitAsync(dbContext, other.Token, questionId);
        var otherLoad = () => QueryAsync(dbContext, other.Token);

        var expectedForPrevious = MsfResponseRefusal.LinkNotRecognised;
        var expectedForCurrent = MsfResponseRefusal.LinkUsed;
        var againRefusal = (await again.Should().ThrowAsync<MsfResponseRefusedException>()).Which;
        againRefusal.Reason.Should().Be(throughPrevious ? expectedForPrevious : expectedForCurrent);
        if (throughPrevious)
        {
            // Opening the link they answered through again, as the thank-you and fault pages invite, tells them where to
            // learn whether their feedback was recorded (T214 review).
            againRefusal.Message.Should().Contain("most recent email").And.Contain("whether your feedback has already been recorded");
        }
        (await otherSubmit.Should().ThrowAsync<MsfResponseRefusedException>()).Which.Reason
            .Should().Be(throughPrevious ? expectedForCurrent : expectedForPrevious);
        (await otherLoad.Should().ThrowAsync<MsfResponseRefusedException>()).Which.Reason
            .Should().Be(throughPrevious ? expectedForCurrent : expectedForPrevious);

        dbContext.ChangeTracker.Clear();
        (await dbContext.MsfResponses.CountAsync()).Should().Be(1);
    }

    /// <summary>
    /// The previous link's selector is no more a secret than the current one's: with another secret it is refused as not
    /// recognised, and uses nothing up.
    /// </summary>
    [Fact]
    public async Task ThePreviousSelector_WithTheWrongSecret_IsNotRecognised()
    {
        await using var dbContext = CreateDbContext();
        var (previous, _, questionId) = await SeedRemindedInvitationAsync(dbContext, Today.AddDays(2));
        var forged = previous.Selector + _tokens.GenerateSelectorToken().Token[InvitationTokenService.SelectorLength..];

        var load = () => QueryAsync(dbContext, forged);
        var submit = () => SubmitAsync(dbContext, forged, questionId);

        (await load.Should().ThrowAsync<MsfResponseRefusedException>()).Which.Reason.Should().Be(MsfResponseRefusal.LinkNotRecognised);
        (await submit.Should().ThrowAsync<MsfResponseRefusedException>()).Which.Reason.Should().Be(MsfResponseRefusal.LinkNotRecognised);
        dbContext.ChangeTracker.Clear();
        (await dbContext.MsfResponses.CountAsync()).Should().Be(0);
        (await QueryAsync(dbContext, previous.Token)).TemplateName.Should().Be("Annual MSF", "the real previous link still opens");
    }

    private Task<MsfResponseFormDto> QueryAsync(ApplicationDbContext dbContext, string token)
        => new GetMsfResponseFormQueryHandler(dbContext, _tokens, new FakeUserDirectory((SubjectUserId, "Thandi Nkosi")))
            .Handle(new GetMsfResponseFormQuery(token), CancellationToken.None);

    private Task SubmitAsync(ApplicationDbContext dbContext, string token, int questionId)
        => new SubmitMsfResponseCommandHandler(dbContext, _tokens)
            .Handle(new SubmitMsfResponseCommand(token, [new(questionId, null, "Calm with parents.")]), CancellationToken.None);

    /// <summary>
    /// An open campaign's invitation whose opening link a reminder has replaced; the window closes on
    /// <paramref name="closesOn" /> and the invitation expires a week later, as the product writes it.
    /// </summary>
    private async Task<(SelectorToken Previous, SelectorToken Current, int QuestionId)> SeedRemindedInvitationAsync(
        ApplicationDbContext dbContext, DateOnly closesOn)
    {
        var question = new MsfQuestion { Order = 1, Prompt = "Comment", Type = MsfQuestionType.LongText, Required = true };
        var opening = _tokens.GenerateSelectorToken();
        var reminder = _tokens.GenerateSelectorToken();

        var invitation = new MsfInvitation
        {
            Campaign = new MsfCampaign
            {
                SubjectUserId = SubjectUserId,
                CreatedByUserId = "coord-1",
                CreatedOn = DateTime.UtcNow,
                OpensOn = closesOn.AddDays(-14),
                ClosesOn = closesOn,
                State = MsfCampaignState.Open,
                Template = new MsfTemplate { Name = "Annual MSF", Questions = [question] }
            },
            RespondentEmail = "respondent@example.test",
            RespondentCategory = MsfRespondentCategory.Nurse,
            TokenHash = "added-hash",
            IssuedOn = DateTime.UtcNow.AddDays(-14),
            ExpiresOn = closesOn.AddDays(7)
        };
        invitation.IssueLink(opening.Selector, opening.Hash, DateTime.UtcNow.AddDays(-14));
        invitation.ReplaceLink(reminder.Selector, reminder.Hash, DateTime.UtcNow.AddDays(-1));
        dbContext.MsfInvitations.Add(invitation);

        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
        return (opening, reminder, question.Id);
    }

    private static ApplicationDbContext CreateDbContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
