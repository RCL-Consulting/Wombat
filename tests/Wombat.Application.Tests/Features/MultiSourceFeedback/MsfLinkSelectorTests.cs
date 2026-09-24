using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// A respondent's link names its invitation by the selector its token begins with, and the whole token is then checked
/// against that one invitation's hash. (T163)
/// </summary>
/// <remarks>
/// Until T163 the hash was the only key, so every load and every submit of the public respondent page read every
/// invitation there was and hashed the token against each. That the lookup is one indexed read on PostgreSQL is
/// <c>MsfLinkSelectorPostgresTests</c>; these hold what the lookup accepts and refuses.
/// </remarks>
public sealed class MsfLinkSelectorTests
{
    private const string SubjectUserId = "trainee-1";

    private readonly InvitationTokenService _tokens = new();

    [Fact]
    public void EverySelectorToken_BeginsWithItsSelector_AndItsHashIsOfTheWholeToken()
    {
        var made = Enumerable.Range(0, 200).Select(_ => _tokens.GenerateSelectorToken()).ToList();

        foreach (var token in made)
        {
            token.Token.Should().HaveLength(InvitationTokenService.SelectorTokenLength);
            token.Selector.Should().HaveLength(InvitationTokenService.SelectorLength);
            _tokens.SelectorOf(token.Token).Should().Be(token.Selector, "the selector is read back from the link alone");
            token.Token.Should().StartWith(token.Selector);
            token.Hash.Should().Be(_tokens.HashToken(token.Token));
            _tokens.VerifyToken(token.Token, token.Hash).Should().BeTrue();
            _tokens.VerifyToken(token.Selector, token.Hash).Should().BeFalse("the selector is not the secret");
        }

        made.Select(token => token.Selector).Should().OnlyHaveUniqueItems();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    [InlineData("AAAAAAAAAAAAAAA+AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")]
    public void ATokenOfAnyOtherShape_HasNoSelector(string? token)
        => _tokens.SelectorOf(token).Should().BeNull();

    /// <summary>A link token is the link's whole authority, and a record prints every property it has.</summary>
    [Fact]
    public void ASelectorToken_NeverPrintsItsToken()
    {
        var token = _tokens.GenerateSelectorToken();

        token.ToString().Should().Contain(token.Selector)
            .And.NotContain(token.Token)
            .And.NotContain(token.Hash);
    }

    [Fact]
    public async Task ALink_OpensItsOwnInvitation_AndNoOther()
    {
        await using var dbContext = CreateDbContext();
        var first = _tokens.GenerateSelectorToken();
        var second = _tokens.GenerateSelectorToken();
        await SeedOpenInvitationAsync(dbContext, "First questionnaire", first.Selector, first.Hash);
        await SeedOpenInvitationAsync(dbContext, "Second questionnaire", second.Selector, second.Hash);

        (await QueryAsync(dbContext, first.Token)).TemplateName.Should().Be("First questionnaire");
        (await QueryAsync(dbContext, second.Token)).TemplateName.Should().Be("Second questionnaire");
    }

    /// <summary>
    /// The selector is not a secret: it is stored in the clear and printed in every link. A token that begins with a real
    /// selector and carries the wrong secret is refused, as the same "not recognised" as a link that names nothing, and
    /// uses nothing up.
    /// </summary>
    [Fact]
    public async Task ARealSelector_WithTheWrongSecret_IsNotRecognised_AndUsesNothingUp()
    {
        await using var dbContext = CreateDbContext();
        var issued = _tokens.GenerateSelectorToken();
        var questionId = await SeedOpenInvitationAsync(dbContext, "Annual MSF", issued.Selector, issued.Hash);
        var forged = issued.Selector + _tokens.GenerateSelectorToken().Token[InvitationTokenService.SelectorLength..];
        _tokens.SelectorOf(forged).Should().Be(issued.Selector, "guard: the forgery names the real invitation");

        var load = () => QueryAsync(dbContext, forged);
        var submit = () => new SubmitMsfResponseCommandHandler(dbContext, _tokens)
            .Handle(new SubmitMsfResponseCommand(forged, [new(questionId, null, "Forged.")]), CancellationToken.None);

        (await load.Should().ThrowAsync<MsfResponseRefusedException>()).Which.Reason.Should().Be(MsfResponseRefusal.LinkNotRecognised);
        (await submit.Should().ThrowAsync<MsfResponseRefusedException>()).Which.Reason.Should().Be(MsfResponseRefusal.LinkNotRecognised);

        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
        (await dbContext.MsfInvitations.SingleAsync()).RespondedOn.Should().BeNull();
        (await dbContext.MsfResponses.CountAsync()).Should().Be(0);
        (await QueryAsync(dbContext, issued.Token)).TemplateName.Should().Be("Annual MSF", "the real link still opens it");
    }

    /// <summary>
    /// The selector finds the row; the hash only checks it. An invitation holding the hash of a well-formed token but no
    /// selector, as every invitation stored before T163 and every draft's invitee does, is found by no link, the one
    /// whose hash it holds included.
    /// </summary>
    [Fact]
    public async Task AnInvitationHoldingNoSelector_IsFoundByNoLink_EvenTheOneWhoseHashItHolds()
    {
        await using var dbContext = CreateDbContext();
        var token = _tokens.GenerateSelectorToken();
        await SeedOpenInvitationAsync(dbContext, "Annual MSF", selector: null, token.Hash);

        var load = () => QueryAsync(dbContext, token.Token);

        (await load.Should().ThrowAsync<MsfResponseRefusedException>()).Which.Reason.Should().Be(MsfResponseRefusal.LinkNotRecognised);
    }

    private Task<MsfResponseFormDto> QueryAsync(ApplicationDbContext dbContext, string token)
        => new GetMsfResponseFormQueryHandler(dbContext, _tokens, new FakeUserDirectory((SubjectUserId, "Thandi Nkosi")))
            .Handle(new GetMsfResponseFormQuery(token), CancellationToken.None);

    /// <summary>An open campaign's invitation holding the given link, with one free-text question; its question's id.</summary>
    private static async Task<int> SeedOpenInvitationAsync(
        ApplicationDbContext dbContext, string templateName, string? selector, string hash)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var question = new MsfQuestion { Order = 1, Prompt = "Comment", Type = MsfQuestionType.LongText, Required = true };

        dbContext.MsfInvitations.Add(new MsfInvitation
        {
            Campaign = new MsfCampaign
            {
                SubjectUserId = SubjectUserId,
                CreatedByUserId = "coord-1",
                CreatedOn = DateTime.UtcNow,
                OpensOn = today.AddDays(-1),
                ClosesOn = today.AddDays(3),
                State = MsfCampaignState.Open,
                Template = new MsfTemplate { Name = templateName, Questions = [question] }
            },
            RespondentEmail = "respondent@example.test",
            RespondentCategory = MsfRespondentCategory.Nurse,
            TokenSelector = selector,
            TokenHash = hash,
            IssuedOn = DateTime.UtcNow,
            ExpiresOn = today.AddDays(10)
        });

        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
        return question.Id;
    }

    private static ApplicationDbContext CreateDbContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
