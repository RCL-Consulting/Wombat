using System.Net;
using System.Text.RegularExpressions;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Infrastructure.MultiSourceFeedback;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Components.Shared;
using Wombat.Web.Security;
using Wombat.Web.Services;
using Wombat.Web.Tests.MultiSourceFeedback;

namespace Wombat.Web.Tests.Hosting;

/// <summary>
/// The MSF respondent page as a browser meets it: over HTTP, through the framework's Razor-components endpoint, under the
/// app's fallback policy, its CSP and its rate limit. (T205)
/// </summary>
/// <remarks>
/// <para>
/// A respondent is a stranger holding an emailed link, never signed in. The page is static server-rendered HTML with
/// plain form posts (<c>[ExcludeFromInteractiveRouting]</c>), so everything it does is an HTTP request this host can
/// make: the load, the post and its antiforgery token, the status a refusal answers with, and the rate limit that sees
/// the post as well as the load. The sender is a fake; the integration suite runs the same path against PostgreSQL.
/// </para>
/// </remarks>
public sealed partial class MsfRespondPageHostingTests
{
    private const string Token = "link-token-1";

    [Fact]
    public async Task ALink_OpensTheQuestionnaire_ForAVisitorWhoHasNotSignedIn()
    {
        var sender = new FakeRespondSender();
        await using var host = await StartAsync(sender);

        using var response = await host.Client.GetAsync(Link(Token));
        var html = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"the fallback policy must not send a respondent to the sign-in page ({response.Headers.Location})");
        sender.QueriedTokens.Should().Equal([Token], "the page asks for the questionnaire the link names");

        var document = Parse(html);
        document.QuerySelector("h2")!.TextContent.Should().Be($"Feedback on {FakeRespondSender.TraineeName}");
        document.Title.Should().Be($"Feedback on {FakeRespondSender.TraineeName} · Wombat",
            "a static page's title still reaches the head, in its heading's words (T190)");
        document.Body!.TextContent.Should().Contain("Last day to respond: 2026-10-15").And.Contain("Annual MSF");

        var scale = document.QuerySelectorAll($"input[type=radio][name='{MsfRespond.ScaleFieldName(FakeRespondSender.ScaleQuestionId)}']");
        scale.Select(radio => radio.GetAttribute("value")).Should().Equal(["1", "2", "3", "4", "5"]);
        scale.Select(radio => document.QuerySelector($"label[for='{radio.Id}']")!.TextContent.Trim()).Should().Equal(
            MsfRatingScale.Default.Select(point => point.Label), "each point is offered by its label");
        document.QuerySelector($"textarea[name='{MsfRespond.CommentFieldName(FakeRespondSender.CommentQuestionId)}']")
            .Should().NotBeNull();

        var form = document.QuerySelector("form[method=post]")!;
        form.QuerySelector("input[name=_handler]")!.GetAttribute("value").Should().Be(MsfRespond.FormName);
        form.HasAttribute("data-submit-once").Should().BeTrue("a double-click must not post the answers twice");

        // The link's token is the post's whole authority: the form carries no antiforgery token, and the post needs no
        // cookie back. Every post in this class is sent without one (PostAsync), as a browser that lost or refused the
        // cookie sends it (T205 review).
        form.QuerySelector("input[name=__RequestVerificationToken]").Should().BeNull();

        // Each comment box names its help text, which says the comment may reach the trainee as written (T205 review).
        var commentId = $"msf-q{FakeRespondSender.CommentQuestionId}";
        var help = document.GetElementById(FieldHelp.Id(commentId))!;
        document.GetElementById(commentId)!.GetAttribute("aria-describedby").Should().Be(help.Id);
        help.TextContent.Should().Contain($"may be shown to {FakeRespondSender.TraineeName} word for word")
            .And.Contain("leave out anything that would identify you");
        document.Body!.TextContent.Should().Contain("Questions marked * must be answered.");
    }

    /// <summary>
    /// The link is in the page's address, and the trainee's name is on the page. The app's referrer policy sent that
    /// address as the Referer of every stylesheet and script the page loads, into the access log a second time; a leaked
    /// link could be indexed; and a shared computer could keep the page in its cache. None of these, in any state of the
    /// page. (T205 review)
    /// </summary>
    [Fact]
    public async Task EveryStateOfThePage_KeepsTheLinkFromRefererSearchAndCache()
    {
        var sender = new FakeRespondSender();
        await using var host = await StartAsync(sender);

        using var questionnaire = await host.Client.GetAsync(Link(Token));
        using var thanks = await PostAsync(host, Token, [new(MsfRespond.ScaleFieldName(FakeRespondSender.ScaleQuestionId), "4")]);
        sender.FormFailure = new MsfResponseRefusedException(MsfResponseRefusal.LinkNotRecognised, "Not recognised.");
        using var refusal = await host.Client.GetAsync(Link(Token));

        foreach (var (state, response) in new[] { ("questionnaire", questionnaire), ("thanks", thanks), ("refusal", refusal) })
        {
            response.Headers.GetValues("Referrer-Policy").Should().Equal(["no-referrer"], state);
            string.Join(", ", response.Headers.GetValues("X-Robots-Tag")).Should().Contain("noindex", state);
            response.Headers.CacheControl!.NoStore.Should().BeTrue(state);
        }

        refusal.StatusCode.Should().Be(HttpStatusCode.NotFound, "guard: the refusal was served");

        // The app's own policy stands everywhere else.
        var (other, _, _) = await host.LoadAsync(AppTestHost.AnonymousPage);
        other.Headers.GetValues("Referrer-Policy").Should().Equal(["strict-origin-when-cross-origin"]);
    }

    /// <summary>
    /// A post carries only what the browser sends: a radio group with nothing chosen sends no field at all, so a
    /// questionnaire whose ratings are all optional, answered with a comment alone, posts no rating. The form mapper then
    /// leaves that kind null, and the page failed as it rendered, before its own handling: a 500 with an empty body.
    /// (T205 review)
    /// </summary>
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task APostCarryingOnlyOneKindOfAnswer_IsSent_NotAFault(bool rating, bool comment)
    {
        var sender = new FakeRespondSender();
        await using var host = await StartAsync(sender);
        var answers = new List<KeyValuePair<string, string>>();
        if (rating)
        {
            answers.Add(new(MsfRespond.ScaleFieldName(FakeRespondSender.ScaleQuestionId), "4"));
        }

        if (comment)
        {
            answers.Add(new(MsfRespond.CommentFieldName(FakeRespondSender.CommentQuestionId), "Only a comment."));
        }

        using var response = await PostAsync(host, Token, answers);
        var html = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, html);
        Parse(html).QuerySelector("h2")!.TextContent.Should().Be("Thank you");
        sender.Submitted.Should().ContainSingle().Which.Answers.Should().ContainSingle().Which.QuestionId.Should().Be(
            rating ? FakeRespondSender.ScaleQuestionId : FakeRespondSender.CommentQuestionId);
    }

    /// <summary>
    /// Static, not interactive: the page renders no interactive root, so a respondent's browser opens no circuit, the
    /// Blazor hub's fallback policy (T181) cannot stop it, and the submit is an HTTP post the rate limit sees.
    /// </summary>
    [Fact]
    public async Task ThePage_IsStaticHtml_WithNoInteractiveRoot_UnderTheAppsCsp()
    {
        await using var host = await StartAsync(new FakeRespondSender());

        using var response = await host.Client.GetAsync(Link(Token));
        var html = await response.Content.ReadAsStringAsync();

        ServerComponentMarker.IsIn(html).Should().BeFalse("no part of the page, its head included, is an interactive root");

        // The framework adds a frame-ancestors-only policy of its own beside Wombat's; the one that governs scripts is
        // Wombat's, nonce-backed (T097). The page has no inline script at all: only the import map carries the nonce.
        var policy = response.Headers.GetValues("Content-Security-Policy")
            .Should().ContainSingle(value => value.Contains("script-src")).Which;
        var nonce = CspNonce().Match(policy).Groups["nonce"].Value;
        nonce.Should().NotBeEmpty();
        var document = Parse(html);
        document.QuerySelectorAll("script:not([src])").Should().OnlyContain(
            script => script.GetAttribute("type") == "importmap" && script.GetAttribute("nonce") == nonce);
        document.QuerySelectorAll("script[src]").Select(script => script.GetAttribute("src")!)
            .Should().OnlyContain(src => src.StartsWith('/') || !src.Contains("://"), "every script is same-origin");

        // Signed in, the page is still static, and every other page is interactive: the change is confined to the page
        // that asks for it. Since T181 no page is interactive for a visitor who has not signed in, so only a signed-in
        // load tells [ExcludeFromInteractiveRouting]'s work apart; it is what keeps a colleague who holds a link, and is
        // signed in, off a circuit the link's rate limit cannot see.
        await using var signedIn = await StartAsync(new FakeRespondSender(), signedIn: true);
        var (_, signedInHtml, _) = await signedIn.LoadAsync(Link(Token));
        ServerComponentMarker.IsIn(signedInHtml).Should().BeFalse("the page is static for a signed-in respondent too");
        var (_, otherHtml, _) = await signedIn.LoadAsync(AppTestHost.AnonymousPage);
        ServerComponentMarker.IsIn(otherHtml).Should().BeTrue("every other page renders in a signed-in user's circuit");
    }

    [Fact]
    public async Task APostedQuestionnaire_IsSentThroughTheSubmitCommand_AndTheRespondentIsThanked()
    {
        var sender = new FakeRespondSender();
        await using var host = await StartAsync(sender);

        using var response = await PostAsync(host, Token,
        [
            new(MsfRespond.ScaleFieldName(FakeRespondSender.ScaleQuestionId), "4"),
            new(MsfRespond.CommentFieldName(FakeRespondSender.CommentQuestionId), "Calm with anxious parents."),
            new(MsfRespond.CommentFieldName(999), "A field for a question the questionnaire does not ask.")
        ]);
        var html = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, html);
        var command = sender.Submitted.Should().ContainSingle().Which;
        command.Token.Should().Be(Token);
        command.Answers.Should().BeEquivalentTo(
            [
                new SubmitMsfResponseAnswerItem(FakeRespondSender.ScaleQuestionId, 4, null),
                new SubmitMsfResponseAnswerItem(FakeRespondSender.CommentQuestionId, null, "Calm with anxious parents.")
            ],
            options => options.WithStrictOrdering(),
            "the answers to the questions asked, and nothing else the post carried");

        var document = Parse(html);
        document.Title.Should().Be("Thank you · Wombat", "each state has its own title, its heading's words (T190)");
        document.QuerySelector("h2")!.TextContent.Should().Be("Thank you");
        document.Body!.TextContent.Should().Contain($"Your feedback on {FakeRespondSender.TraineeName} has been recorded.");
        document.QuerySelectorAll("input[type=radio], textarea, button[type=submit]").Should().BeEmpty("the link is spent");
    }

    public static TheoryData<MsfResponseRefusal, HttpStatusCode, string> LinkRefusals => new()
    {
        { MsfResponseRefusal.LinkNotRecognised, HttpStatusCode.NotFound, "Feedback link not recognised" },
        { MsfResponseRefusal.LinkExpired, HttpStatusCode.Gone, "Feedback link expired" },
        { MsfResponseRefusal.LinkRevoked, HttpStatusCode.Gone, "Feedback link revoked" },
        { MsfResponseRefusal.LinkUsed, HttpStatusCode.Gone, "Feedback link already used" },
        { MsfResponseRefusal.CampaignNotOpen, HttpStatusCode.Gone, "Feedback request closed" }
    };

    [Theory]
    [MemberData(nameof(LinkRefusals))]
    public async Task ADeadLink_AnswersItsStatus_WithItsPlainMessage_AndNoQuestionnaire(
        MsfResponseRefusal reason,
        HttpStatusCode status,
        string title)
    {
        const string message = "The refusal's own message, written for the respondent.";
        var sender = new FakeRespondSender { FormFailure = new MsfResponseRefusedException(reason, message) };
        await using var host = await StartAsync(sender);

        using var response = await host.Client.GetAsync(Link(Token));
        var html = await response.Content.ReadAsStringAsync();
        var document = Parse(html);

        response.StatusCode.Should().Be(status, "the page answers each refusal as the Api's endpoint does");
        document.QuerySelector("h2").Should().NotBeNull($"the refusal is a page, whatever its status: {html}");
        document.QuerySelector("h2")!.TextContent.Should().Be(title);
        document.Title.Should().Be($"{title} · Wombat", "the tab says what happened, in the heading's words (T190)");
        document.QuerySelector(".alert")!.TextContent.Trim().Should().Be(message);
        document.QuerySelectorAll("input[type=radio], textarea, button[type=submit]").Should().BeEmpty();
    }

    /// <summary>
    /// A link can die while its questionnaire is open: the window closes, or the last day passes. The post must then say
    /// so, on the page. Were the form left out of the page once the link was refused, Blazor would answer the post with a
    /// bare 400 ("Cannot submit the form"), because a post is dispatched to a form that is on the page.
    /// </summary>
    [Fact]
    public async Task ALinkThatDiesBeforeTheSubmit_IsToldSo_NotAnsweredWithABare400()
    {
        var sender = new FakeRespondSender
        {
            FormFailureOnCall = call => call == 1
                ? null
                : new MsfResponseRefusedException(MsfResponseRefusal.CampaignNotOpen, "This feedback request has closed.")
        };
        await using var host = await StartAsync(sender);

        using var response = await PostAsync(host, Token, [new(MsfRespond.ScaleFieldName(FakeRespondSender.ScaleQuestionId), "4")]);
        var document = Parse(await response.Content.ReadAsStringAsync());

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
        document.QuerySelector("h2")!.TextContent.Should().Be("Feedback request closed");
        document.QuerySelector(".alert")!.TextContent.Trim().Should().Be("This feedback request has closed.");
        sender.Submitted.Should().BeEmpty("nothing is sent through a link the page has just been told is dead");
    }

    [Fact]
    public async Task AnIncompleteSubmission_KeepsTheQuestionnaireAndWhatWasTyped_AndSaysWhatIsMissing()
    {
        const string missing = "A response is required for 'Rates the trainee's overall professional performance.'.";
        var sender = new FakeRespondSender
        {
            SubmitFailure = new MsfResponseRefusedException(MsfResponseRefusal.AnswersIncomplete, missing)
            {
                QuestionId = FakeRespondSender.ScaleQuestionId
            }
        };
        await using var host = await StartAsync(sender);

        using var response = await PostAsync(host, Token,
        [
            new(MsfRespond.ScaleFieldName(FakeRespondSender.ScaleQuestionId), "not-a-number"),
            new(MsfRespond.CommentFieldName(FakeRespondSender.CommentQuestionId), "Kept after the refusal.")
        ]);
        var document = Parse(await response.Content.ReadAsStringAsync());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        document.QuerySelector(".alert-danger")!.TextContent.Trim().Should().Be(missing);

        // A refused submit is a full page load: its title says so, its summary takes the focus as the page loads, and the
        // question it is about points at it (T205 review).
        document.Title.Should().Be($"Error: Feedback on {FakeRespondSender.TraineeName} · Wombat");
        var summary = document.GetElementById(MsfRespond.ErrorSummaryId)!;
        summary.QuerySelector(".alert-danger").Should().NotBeNull("the summary holds the refusal");
        summary.GetAttribute("tabindex").Should().Be("-1");
        summary.HasAttribute("autofocus").Should().BeTrue();
        var fieldsets = document.QuerySelectorAll("fieldset");
        fieldsets.Should().ContainSingle().Which.GetAttribute("aria-describedby").Should().Be(MsfRespond.ErrorSummaryId);
        document.GetElementById($"msf-q{FakeRespondSender.CommentQuestionId}")!.HasAttribute("aria-invalid")
            .Should().BeFalse("only the question the refusal is about is marked");
        document.QuerySelector("textarea")!.TextContent.Should().Be("Kept after the refusal.");
        document.QuerySelectorAll("button[type=submit]").Should().ContainSingle("the respondent can put it right and send again");
        sender.Submitted.Should().ContainSingle().Which.Answers.Should().ContainSingle(
                "a rating that is not a number is left unanswered, for the command to name as missing")
            .Which.QuestionId.Should().Be(FakeRespondSender.CommentQuestionId);
    }

    [Fact]
    public async Task AFault_AnswersA500_ThatNamesNeitherTheExceptionNorItsMessage()
    {
        const string detail = "fault-detail-for-developers-only";
        await using var host = await StartAsync(new FakeRespondSender { FormFailure = new InvalidOperationException(detail) });

        using var response = await host.Client.GetAsync(Link(Token));
        var html = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        html.Should().Contain("Something went wrong on our side").And.NotContain(detail).And.NotContain("Exception");
        Parse(html).Title.Should().Be("Something went wrong · Wombat");
        Parse(html).QuerySelector("h2")!.TextContent.Should().Be("Something went wrong", "the tab says what the heading says (T190)");
    }

    /// <summary>
    /// Ten requests a minute for one link from one address, the Api's limit (<c>MsfRespondRateLimit</c>). The eleventh is
    /// a 429 that says what happened, not the sign-in page the web host's other throttle redirects to.
    /// </summary>
    [Fact]
    public async Task TheEleventhRequestInAMinute_ForOneLink_IsRefusedWithAReadable429()
    {
        var sender = new FakeRespondSender();
        await using var host = await StartAsync(sender);

        for (var request = 1; request <= 10; request++)
        {
            using var allowed = await host.Client.GetAsync(Link(Token));
            allowed.StatusCode.Should().Be(HttpStatusCode.OK, $"request {request} of ten is inside the limit");
        }

        using var refused = await host.Client.GetAsync(Link(Token));
        refused.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        refused.Headers.RetryAfter!.Delta.Should().Be(TimeSpan.FromSeconds(60));
        refused.Content.Headers.ContentType!.MediaType.Should().Be("text/plain");
        (await refused.Content.ReadAsStringAsync()).Should().Be(MsfRespondThrottle.Message);
        sender.QueriedTokens.Should().HaveCount(10, "a refused request reaches nothing");

        // The post is limited too: it is an HTTP request like the load, not a message on a circuit.
        using var refusedPost = await host.Client.PostAsync(Link(Token), new FormUrlEncodedContent([]));
        refusedPost.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

        using var anotherLink = await host.Client.GetAsync(Link("another-respondents-link"));
        anotherLink.StatusCode.Should().Be(HttpStatusCode.OK, "the limit is per link: a colleague on the same network is not refused");
    }

    /// <summary>
    /// The per-link limit alone limits no client: every made-up link had a fresh ten, and each request loaded every
    /// invitation. Sixty a minute from one address, whatever the links, and the rest of the app is not counted.
    /// (T205 review)
    /// </summary>
    [Fact]
    public async Task SixtyRequestsAMinuteFromOneAddress_WhateverTheLinks_ThenA429()
    {
        var sender = new FakeRespondSender();
        await using var host = await StartAsync(sender);

        for (var request = 1; request <= MsfRespondRateLimit.AddressPermitLimit; request++)
        {
            using var allowed = await host.Client.GetAsync(Link($"made-up-link-{request}"));
            allowed.StatusCode.Should().Be(HttpStatusCode.OK, $"request {request} of sixty is inside the limit");
        }

        using var refused = await host.Client.GetAsync(Link("one-more-made-up-link"));
        refused.StatusCode.Should().Be(HttpStatusCode.TooManyRequests, "a fresh link is no fresh allowance");
        (await refused.Content.ReadAsStringAsync()).Should().Be(MsfRespondThrottle.Message);
        sender.QueriedTokens.Should().HaveCount(MsfRespondRateLimit.AddressPermitLimit, "a refused request reaches nothing");

        var (other, _, _) = await host.LoadAsync(AppTestHost.AnonymousPage);
        other.StatusCode.Should().Be(HttpStatusCode.OK, "the limit is the respondent page's, not the app's");
    }

    /// <summary>The link builder's default path is this page's route (T205 review: an unset MsfRespondUrl).</summary>
    [Fact]
    public void ThePagesRoute_IsThePathAnUnsetRespondUrlDefaultsTo()
        => typeof(MsfRespond).GetCustomAttributes(typeof(Microsoft.AspNetCore.Components.RouteAttribute), inherit: false)
            .Cast<Microsoft.AspNetCore.Components.RouteAttribute>()
            .Should().ContainSingle().Which.Template.Should().Be(WombatOptionsExtensions.MsfRespondPath);

    private static Task<AppTestHost> StartAsync(FakeRespondSender sender, bool signedIn = false)
        => AppTestHost.StartAsync(services =>
        {
            services.AddSingleton<IScopedSender>(sender);
            if (signedIn)
            {
                SignedInVisitor.Register(services);
            }
        });

    private static string Link(string token) => $"/msf/respond?token={Uri.EscapeDataString(token)}";

    /// <summary>
    /// Loads the page, then posts its form as a browser would: the handler name from the page, and
    /// <paramref name="answers" />. No cookie: the page sets none, and needs none back (no antiforgery token).
    /// </summary>
    private static async Task<HttpResponseMessage> PostAsync(
        AppTestHost host,
        string token,
        IReadOnlyList<KeyValuePair<string, string>> answers)
    {
        using var load = await host.Client.GetAsync(Link(token));
        load.StatusCode.Should().Be(HttpStatusCode.OK, "guard: the questionnaire loads before it is posted");
        var form = Parse(await load.Content.ReadAsStringAsync()).QuerySelector("form[method=post]")!;

        var fields = new List<KeyValuePair<string, string>>
        {
            new("_handler", form.QuerySelector("input[name=_handler]")!.GetAttribute("value")!)
        };
        fields.AddRange(answers);

        return await host.Client.PostAsync(Link(token), new FormUrlEncodedContent(fields));
    }

    private static IHtmlDocument Parse(string html) => new HtmlParser().ParseDocument(html);

    [GeneratedRegex(@"script-src 'self' 'nonce-(?<nonce>[^']+)'")]
    private static partial Regex CspNonce();
}
