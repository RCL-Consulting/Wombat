extern alias WombatWeb;

using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling;

namespace Wombat.Integration.Tests.MultiSourceFeedback;

/// <summary>
/// T205 end to end: the web app hosted whole on a real PostgreSQL server, from the link in a respondent's invitation to
/// the response stored and the link spent, through the page a respondent actually lands on.
/// </summary>
/// <remarks>
/// <para>
/// Everything a respondent does here is an HTTP request through <c>Wombat.Web</c>'s own <c>Program.cs</c>: its fallback
/// authorization policy, CSP middleware, rate limiter, antiforgery and Razor-components endpoint, and the query and
/// command the Api's endpoint also sends. The host migrates and seeds its schema at startup, as the server does.
/// </para>
/// <para>
/// One host for the class (<see cref="WebHost" />): starting the web app seeds the catalogue, which is slow. The tests
/// share its schema and its rate limiter, so each uses its own campaign and its own links. The schema is its own
/// (<c>SearchPath = it_&lt;guid&gt;</c>) and is dropped however setup ends (T140).
/// </para>
/// </remarks>
public sealed class MsfRespondPageFlowTests : IClassFixture<MsfRespondPageFlowTests.WebHost>
{
    private readonly WebHost _host;

    public MsfRespondPageFlowTests(WebHost host)
    {
        _host = host;
    }

    [Fact]
    public async Task ARespondentsLink_OpensTheQuestionnaire_TakesTheirAnswers_AndIsThenSpent()
    {
        var started = DateTime.UtcNow;
        var campaign = await _host.OpenCampaignAsync(["consultant-e2e@example.test", "nurse-e2e@example.test"]);
        var link = _host.LinkMailedTo("consultant-e2e@example.test");
        link.Should().StartWith($"{WebHost.RespondUrl}?token=", "the invitation links to the web app's page (Wombat:MsfRespondUrl)");

        // The questionnaire, for a respondent who has never signed in.
        using var load = await _host.Client.GetAsync(link);
        var page = Parse(await load.Content.ReadAsStringAsync());
        load.StatusCode.Should().Be(HttpStatusCode.OK, $"not a redirect to the sign-in page ({load.Headers.Location})");

        page.QuerySelector("h2")!.TextContent.Should().Be($"Feedback on {WebHost.TraineeName}");
        var lastDay = MsfInvitation.LastDayToRespond(campaign.ClosesOn, campaign.ClosesOn.AddDays(7));
        page.QuerySelector(".page-subtitle")!.TextContent.Should().Contain(
            $"Last day to respond: {lastDay.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");

        var scale = await _host.QuestionIdAsync(campaign.TemplateId, MsfQuestionType.Scale);
        var comment = await _host.QuestionIdAsync(campaign.TemplateId, MsfQuestionType.LongText);
        var radios = page.QuerySelectorAll($"input[type=radio][name='Answers.Scale[{scale}]']");
        radios.Select(radio => page.QuerySelector($"label[for='{radio.Id}']")!.TextContent.Trim())
            .Should().Equal(MsfRatingScale.Default.Select(point => point.Label));
        page.QuerySelector($"textarea[name='Answers.Comment[{comment}]']").Should().NotBeNull();

        // The answers, posted as the browser posts the form.
        using var submit = await _host.PostAsync(link, page,
        [
            new($"Answers.Scale[{scale}]", "4"),
            new($"Answers.Comment[{comment}]", "Calm with anxious parents.")
        ]);
        var thanks = Parse(await submit.Content.ReadAsStringAsync());
        submit.StatusCode.Should().Be(HttpStatusCode.OK);
        thanks.QuerySelector("h2")!.TextContent.Should().Be("Thank you");

        // Stored, against this respondent's invitation only.
        await using (var scope = _host.Factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var invitations = await dbContext.MsfInvitations.AsNoTracking()
                .Where(invitation => invitation.CampaignId == campaign.Id)
                .ToListAsync();
            invitations.Should().ContainSingle(invitation => invitation.RespondedOn != null)
                .Which.RespondentEmail.Should().Be("consultant-e2e@example.test");

            var response = await dbContext.MsfResponses.AsNoTracking()
                .Include(entity => entity.Answers)
                .SingleAsync(entity => entity.CampaignId == campaign.Id);
            response.Answers.Should().BeEquivalentTo(
                [
                    new { QuestionId = scale, ScaleValue = (int?)4, LongText = (string?)null },
                    new { QuestionId = comment, ScaleValue = (int?)null, LongText = (string?)"Calm with anxious parents." }
                ]);

            // The audit row says a response was submitted, and carries neither the link nor what was said (T101).
            var audit = await dbContext.AuditEntries.AsNoTracking()
                .Where(entry => entry.Action == nameof(SubmitMsfResponseCommand) && entry.Success && entry.OccurredAt >= started)
                .ToListAsync();
            audit.Should().ContainSingle();
            audit[0].SummaryJson.Should().NotContain("Calm with anxious parents").And.NotContain(TokenOf(link));
        }

        // Spent: the same link now says so, on the page, with the status the Api answers.
        using var again = await _host.Client.GetAsync(link);
        var spent = Parse(await again.Content.ReadAsStringAsync());
        again.StatusCode.Should().Be(HttpStatusCode.Gone);
        spent.QuerySelector("h2")!.TextContent.Should().Be("Feedback link already used");
        spent.QuerySelector(".alert")!.TextContent.Trim().Should().Be(MsfCampaignRules.LinkUsedMessage);
        spent.QuerySelectorAll("input[type=radio], textarea").Should().BeEmpty();
    }

    /// <summary>
    /// A learner-feedback campaign (T164, D35) on the same page: the learner is asked about the trainee's teaching in
    /// their invitation's words, is never asked where they were taught, and their answers are stored against their own
    /// invitation, whose teaching context the coordinator recorded.
    /// </summary>
    [Fact]
    public async Task ALearnersLink_AsksAboutTheTraineesTeaching_NeverWhereTheyWereTaught_AndTakesTheirAnswers()
    {
        const string learner = "student-e2e@example.test";
        const string teachingContext = "Neonatal night teaching";
        var campaign = await _host.OpenCampaignAsync([learner], MsfTemplateKind.LearnerFeedback, teachingContext);
        var link = _host.LinkMailedTo(learner);

        using var load = await _host.Client.GetAsync(link);
        var html = await load.Content.ReadAsStringAsync();
        var page = Parse(html);
        load.StatusCode.Should().Be(HttpStatusCode.OK);

        page.QuerySelector("h2")!.TextContent.Should().Be($"Feedback on the teaching of {WebHost.TraineeName}");
        var words = Regex.Replace(page.QuerySelector(".account-form-container")!.TextContent, @"\s+", " ");
        words.Should().Contain(
            $"You have been asked to give feedback on the teaching of {WebHost.TraineeName}, a trainee who has taught you.");
        words.Should().Contain("together with the other learners' feedback").And.NotContain("multi-source");
        html.Should().NotContain(teachingContext, "where the learner was taught is the coordinator's record, not the page's");

        var scale = await _host.QuestionIdAsync(campaign.TemplateId, MsfQuestionType.Scale);
        var comment = await _host.QuestionIdAsync(campaign.TemplateId, MsfQuestionType.LongText);
        page.QuerySelectorAll("form[method=post] input:not([type=hidden]), form[method=post] textarea, form[method=post] select")
            .Select(field => field.GetAttribute("name"))
            .Distinct()
            .Should().BeEquivalentTo([$"Answers.Scale[{scale}]", $"Answers.Comment[{comment}]"], "the questionnaire's questions, and nothing else");

        using var submit = await _host.PostAsync(link, page,
        [
            new($"Answers.Scale[{scale}]", "5"),
            new($"Answers.Comment[{comment}]", "Explained it twice, kindly.")
        ]);
        var thanks = Parse(await submit.Content.ReadAsStringAsync());
        submit.StatusCode.Should().Be(HttpStatusCode.OK);
        thanks.QuerySelector(".alert-success")!.TextContent.Trim()
            .Should().Be($"Your feedback on the teaching of {WebHost.TraineeName} has been recorded.");

        await using var scope = _host.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var invitation = await dbContext.MsfInvitations.AsNoTracking().SingleAsync(entity => entity.CampaignId == campaign.Id);
        invitation.RespondedOn.Should().NotBeNull();
        invitation.RespondentCategory.Should().Be(MsfRespondentCategory.Learner);
        invitation.TeachingContext.Should().Be(teachingContext, "the coordinator's record stands, untouched by the response");
        (await dbContext.MsfResponseAnswers.AsNoTracking().CountAsync(answer => answer.Response.CampaignId == campaign.Id))
            .Should().Be(2);
    }

    [Fact]
    public async Task EveryDeadLink_IsToldSoOnThePage_WithTheApisStatus_NeverA500()
    {
        var campaign = await _host.OpenCampaignAsync(["peer-dead@example.test"]);
        var link = _host.LinkMailedTo("peer-dead@example.test");

        await ShouldBeRefusedAsync("/msf/respond?token=not-a-token-anyone-was-sent", HttpStatusCode.NotFound, "Feedback link not recognised");
        await ShouldBeRefusedAsync("/msf/respond", HttpStatusCode.NotFound, "Feedback link not recognised");

        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var expiresOn = await _host.UpdateInvitationAsync(campaign.Id, invitation => invitation.ExpiresOn, yesterday);
        await ShouldBeRefusedAsync(link, HttpStatusCode.Gone, "Feedback link expired");
        await _host.UpdateInvitationAsync(campaign.Id, invitation => invitation.ExpiresOn, expiresOn);

        await _host.UpdateInvitationAsync(campaign.Id, invitation => invitation.RevokedOn, DateTime.UtcNow);
        await ShouldBeRefusedAsync(link, HttpStatusCode.Gone, "Feedback link revoked");
        await _host.UpdateInvitationAsync(campaign.Id, invitation => invitation.RevokedOn, (DateTime?)null);

        await _host.SendAsync(new WithdrawMsfCampaignCommand(campaign.Id, MsfCampaignState.Open, _host.Coordinator));
        await ShouldBeRefusedAsync(link, HttpStatusCode.Gone, "Feedback request closed");
    }

    /// <summary>
    /// Ten requests a minute for one link from one address, as on the Api. <c>Program.cs</c> answers the eleventh with a
    /// readable 429, not with the redirect to the sign-in page its sign-in throttle answers with, which still stands. The
    /// sign-in throttle counts failed sign-ins from one address (T156), so a browser of its own makes eleven.
    /// </summary>
    [Fact]
    public async Task TheEleventhRequestInAMinuteForOneLink_IsAReadable429_AndTheSignInThrottleStillRedirects()
    {
        var link = $"/msf/respond?token=rate-limited-{Guid.NewGuid():N}";
        for (var request = 1; request <= 10; request++)
        {
            using var allowed = await _host.Client.GetAsync(link);
            allowed.StatusCode.Should().Be(HttpStatusCode.NotFound, $"request {request} of ten reaches the page");
        }

        using var refused = await _host.Client.GetAsync(link);
        refused.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        refused.Headers.Location.Should().BeNull("a respondent has no account to sign in to");
        refused.Content.Headers.ContentType!.MediaType.Should().Be("text/plain");
        (await refused.Content.ReadAsStringAsync()).Should().StartWith("Too many requests have been made to this feedback page");

        using var browser = _host.NewBrowser("203.0.113.57");
        HttpResponseMessage? lastSignIn = null;
        for (var attempt = 1; attempt <= 11; attempt++)
        {
            lastSignIn?.Dispose();
            using var loginPage = await browser.GetAsync("/account/login");
            var form = Parse(await loginPage.Content.ReadAsStringAsync()).QuerySelector("form[action='/account/login/submit']")!;
            lastSignIn = await browser.PostAsync("/account/login/submit", new FormUrlEncodedContent(
            [
                new("__RequestVerificationToken", form.QuerySelector("input[name=__RequestVerificationToken]")!.GetAttribute("value")!),
                new("Email", $"nobody-{attempt}@example.test"),
                new("Password", "Not-the-Pa55word!")
            ]));
        }

        lastSignIn!.StatusCode.Should().Be(HttpStatusCode.Redirect);
        lastSignIn.Headers.Location!.ToString().Should().Contain("Too%20many%20failed%20sign-in%20attempts");
        lastSignIn.Dispose();
    }

    /// <summary>
    /// Two submissions through one link can both pass the used-link check before either commits: two tabs, or a post
    /// sent again while the first is on its way. The second is refused by the unique index on a response's invitation,
    /// and until T205 that was a fault. It is the used link now, and the audit row of the refusal is kept.
    /// </summary>
    [Fact]
    public async Task ASubmissionThatLosesTheRaceToAnotherThroughTheSameLink_IsRefusedAsAUsedLink_NotAFault()
    {
        var campaign = await _host.OpenCampaignAsync(["ahp-race@example.test"]);
        var token = TokenOf(_host.LinkMailedTo("ahp-race@example.test"));
        var scale = await _host.QuestionIdAsync(campaign.TemplateId, MsfQuestionType.Scale);

        // The winner has committed its response; this request read the invitation before it did, so RespondedOn is unset.
        await using (var scope = _host.Factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var invitation = await dbContext.MsfInvitations.SingleAsync(entity => entity.CampaignId == campaign.Id);
            dbContext.MsfResponses.Add(new MsfResponse
            {
                CampaignId = campaign.Id,
                InvitationId = invitation.Id,
                SubmittedOn = DateTime.UtcNow,
                Answers = [new MsfResponseAnswer { QuestionId = scale, ScaleValue = 3 }]
            });
            await dbContext.SaveChangesAsync();
        }

        var submit = () => _host.SendAsync(new SubmitMsfResponseCommand(token, [new SubmitMsfResponseAnswerItem(scale, 5, null)]));

        var refusal = (await submit.Should().ThrowAsync<MsfResponseRefusedException>()).Which;
        refusal.Reason.Should().Be(MsfResponseRefusal.LinkUsed);
        refusal.Message.Should().Be(MsfCampaignRules.LinkUsedMessage);

        await using (var scope = _host.Factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            (await dbContext.MsfResponses.CountAsync(response => response.CampaignId == campaign.Id))
                .Should().Be(1, "the winner's response stands, alone");
            (await dbContext.MsfResponseAnswers.SingleAsync(answer => answer.Response.CampaignId == campaign.Id))
                .ScaleValue.Should().Be(3, "the loser's answers were not stored");

            // The audit pipeline discarded the refused insert and wrote its row (T201).
            (await dbContext.AuditEntries.AsNoTracking()
                    .Where(entry => entry.Action == nameof(SubmitMsfResponseCommand) && !entry.Success)
                    .Select(entry => entry.ErrorMessage)
                    .ToListAsync())
                .Should().Contain(MsfCampaignRules.LinkUsedMessage);
        }
    }

    /// <summary>
    /// A respondent is often a Wombat user too: a consultant who assesses, a peer registrar. Opened in the browser where
    /// they use Wombat, their link arrives signed in, because the page is on the app's own origin and
    /// <c>[AllowAnonymous]</c> admits a caller without a sign-in, it does not remove one. The audit rows of what they send
    /// must still not name them: not their id or email, and not their institution, which would put the rows in front of
    /// that institution's admins at <c>/admin/audit</c>, beside the response's own timestamp. (T205 review)
    /// </summary>
    [Fact]
    public async Task ARespondentSignedInToWombat_IsNamedOnNeitherAuditRowOfWhatTheySent()
    {
        const string respondent = "assessor-signed-in@example.test";
        const string address = "203.0.113.41";
        var campaign = await _host.OpenCampaignAsync([respondent]);
        var link = _host.LinkMailedTo(respondent);
        var scale = await _host.QuestionIdAsync(campaign.TemplateId, MsfQuestionType.Scale);
        var comment = await _host.QuestionIdAsync(campaign.TemplateId, MsfQuestionType.LongText);
        var assessor = await _host.CreateAssessorAsync(respondent);

        using var client = _host.NewBrowser(address);
        await _host.SignInAsync(client, respondent);

        using var load = await client.GetAsync(link);
        load.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = Parse(await load.Content.ReadAsStringAsync());

        // Refused first (the rating is required), then accepted: a failure row and a success row.
        using (var refused = await _host.PostAsync(client, link, page, [new($"Answers.Comment[{comment}]", "No rating yet.")]))
        {
            refused.StatusCode.Should().Be(HttpStatusCode.BadRequest, "guard: the incomplete submit is refused");
        }

        using (var accepted = await _host.PostAsync(client, link, page,
                   [new($"Answers.Scale[{scale}]", "5"), new($"Answers.Comment[{comment}]", "Unflappable.")]))
        {
            Parse(await accepted.Content.ReadAsStringAsync()).QuerySelector("h2")!.TextContent.Should().Be("Thank you");
        }

        await using var scope = _host.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var fromThisBrowser = await dbContext.AuditEntries.AsNoTracking()
            .Where(entry => entry.ActorIpAddress == "203.0.113.0/24")
            .ToListAsync();

        // Guard: the browser was signed in as the assessor, and the audit knows them by it.
        fromThisBrowser.Should().Contain(entry => entry.Action == "Login" && entry.Success && entry.ActorUserId == assessor.Id,
            "the respondent signed in to Wombat in this browser");

        var submissions = fromThisBrowser.Where(entry => entry.Action == nameof(SubmitMsfResponseCommand)).ToList();
        submissions.Select(entry => entry.Success).Should().BeEquivalentTo([false, true]);
        submissions.Should().OnlyContain(entry =>
            entry.ActorUserId == null &&
            entry.ActorDisplay == null &&
            entry.ActorUserAgent == null &&
            entry.InstitutionId == null,
            "a respondent's sign-in must not name them on what they sent");
    }

    private async Task ShouldBeRefusedAsync(string link, HttpStatusCode status, string title)
    {
        using var response = await _host.Client.GetAsync(link);
        var html = await response.Content.ReadAsStringAsync();
        var page = Parse(html);

        response.StatusCode.Should().Be(status, html);
        page.QuerySelector("h2").Should().NotBeNull($"the refusal is a page: {html}");
        page.QuerySelector("h2")!.TextContent.Should().Be(title);
        page.QuerySelector(".alert")!.TextContent.Should().NotBeNullOrWhiteSpace();
        page.QuerySelectorAll("input[type=radio], textarea").Should().BeEmpty();
        html.Should().NotContain("Exception");
    }

    private static string TokenOf(string link)
        => Uri.UnescapeDataString(Regex.Match(link, @"token=([^&\s]+)", RegexOptions.CultureInvariant).Groups[1].Value);

    private static IHtmlDocument Parse(string html) => new HtmlParser().ParseDocument(html);

    public sealed record OpenedCampaign(int Id, int TemplateId, DateOnly ClosesOn);

    /// <summary>The web app on a schema of its own, with a named trainee and the coordinator who runs their campaigns.</summary>
    public sealed class WebHost : IAsyncLifetime
    {
        public const string RespondUrl = "http://localhost/msf/respond";
        public const string TraineeName = "Thandi Nkosi";

        /// <summary>The trainee's sign-in name, once <see cref="LetTheSubjectSignInAsync" /> has given them a password.</summary>
        public const string SubjectEmail = "trainee-web-1@example.test";

        private const string PaediatricCurriculumName = "Paediatric EPA Curriculum";
        private const string DemoInstitutionShortCode = "DEMO";
        private const string SubjectUserId = "trainee-web-1";

        private readonly string _schemaName = TestDatabase.NewSchemaName();
        private WebFactory? _factory;
        private HttpClient? _client;
        private int _coveredEpaId;
        private int _learnerFeedbackEpaId;

        public WebFactory Factory => _factory ?? throw new InvalidOperationException("The web host is not running.");

        public HttpClient Client => _client ?? throw new InvalidOperationException("The web host is not running.");

        public ClaimsPrincipal Coordinator { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            try
            {
                await TestDatabase.CreateSchemaAsync(_schemaName);
                _factory = new WebFactory(TestDatabase.SchemaConnectionString(_schemaName), RespondUrl);

                // Starts the host: it migrates the schema and seeds the catalogue before it serves anything. Redirects
                // are not followed, so a redirect to the sign-in page is seen as one; cookies are kept, as a browser keeps
                // the antiforgery cookie between loading a form and posting it.
                _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

                await AdmitSubjectAsync();
            }
            catch (Exception setupFailure)
            {
                // xUnit 2 does not call DisposeAsync after a failed InitializeAsync, so the schema is dropped here (T140).
                try
                {
                    await DisposeAsync();
                }
                catch (Exception teardownFailure)
                {
                    throw new AggregateException(
                        "InitializeAsync failed, and so did the teardown that should have dropped its schema.",
                        setupFailure,
                        teardownFailure);
                }

                throw;
            }
        }

        public async Task DisposeAsync()
        {
            try
            {
                _client?.Dispose();
                _client = null;

                if (_factory is not null)
                {
                    var factory = _factory;
                    _factory = null;
                    await factory.DisposeAsync();
                }
            }
            finally
            {
                await TestDatabase.DropSchemaAsync(_schemaName);
            }
        }

        /// <summary>
        /// A two-question campaign about the trainee, opened, with one invitation per address. Multi-source feedback from
        /// consultants unless <paramref name="kind" /> says learner feedback (T164): then learners, each taught in
        /// <paramref name="teachingContext" />, covering PAED-015.
        /// </summary>
        public async Task<OpenedCampaign> OpenCampaignAsync(
            IReadOnlyList<string> respondents,
            MsfTemplateKind kind = MsfTemplateKind.Msf,
            string? teachingContext = null)
        {
            var learnerFeedback = kind == MsfTemplateKind.LearnerFeedback;
            List<CreateMsfTemplateQuestionItem> questions = learnerFeedback
                ?
                [
                    new CreateMsfTemplateQuestionItem("Rates the trainee's teaching overall.", MsfQuestionType.Scale, null, true),
                    new CreateMsfTemplateQuestionItem("What should the trainee keep doing or change in their teaching?", MsfQuestionType.LongText, null, false)
                ]
                :
                [
                    new CreateMsfTemplateQuestionItem("Rates the trainee's overall professional performance.", MsfQuestionType.Scale, null, true),
                    new CreateMsfTemplateQuestionItem("What should the trainee keep doing or improve?", MsfQuestionType.LongText, null, false)
                ];
            var template = await SendAsync(new CreateMsfTemplateCommand(
                learnerFeedback ? $"Learner feedback {Guid.NewGuid():N}" : $"Annual MSF {Guid.NewGuid():N}",
                null,
                false,
                questions,
                Coordinator,
                kind));

            var campaign = await SendAsync(new CreateMsfCampaignCommand(
                SubjectUserId,
                template.Id,
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)),
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                4,
                2,
                learnerFeedback ? 1 : 2,
                [learnerFeedback ? _learnerFeedbackEpaId : _coveredEpaId],
                "coordinator-web-1",
                Coordinator));

            foreach (var respondent in respondents)
            {
                await SendAsync(learnerFeedback
                    ? new AddMsfInvitationCommand(campaign.Id, respondent, MsfRespondentCategory.Learner, Coordinator, teachingContext)
                    : new AddMsfInvitationCommand(campaign.Id, respondent, MsfRespondentCategory.Consultant, Coordinator));
            }

            await SendAsync(new OpenMsfCampaignCommand(campaign.Id, Coordinator));
            return new OpenedCampaign(campaign.Id, template.Id, campaign.ClosesOn);
        }

        /// <summary>The link in the one invitation mailed to this address.</summary>
        public string LinkMailedTo(string respondent)
        {
            var body = Factory.EmailSender.Messages.Should().ContainSingle(message => message.To == respondent).Which.TextBody;
            var match = Regex.Match(body, @"(\S+/msf/respond\?token=[^&\s]+)", RegexOptions.CultureInvariant);
            match.Success.Should().BeTrue("the invitation carries the respondent's link");
            return match.Groups[1].Value;
        }

        public async Task<int> QuestionIdAsync(int templateId, MsfQuestionType type)
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().MsfQuestions
                .Where(question => question.TemplateId == templateId && question.Type == type)
                .Select(question => question.Id)
                .SingleAsync();
        }

        /// <summary>Sets one property of a campaign's one invitation, and returns what it held before.</summary>
        public async Task<TValue> UpdateInvitationAsync<TValue>(
            int campaignId,
            System.Linq.Expressions.Expression<Func<MsfInvitation, TValue>> property,
            TValue value)
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            var invitations = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().MsfInvitations
                .Where(invitation => invitation.CampaignId == campaignId);

            var before = await invitations.Select(property).SingleAsync();
            (await invitations.ExecuteUpdateAsync(setters => setters.SetProperty(property, value))).Should().Be(1);
            return before;
        }

        /// <summary>
        /// Posts the form of a page this client loaded, as the browser posts it: its handler name, and
        /// <paramref name="answers" />. The page carries no antiforgery token: the link is the post's authority.
        /// </summary>
        public Task<HttpResponseMessage> PostAsync(string link, IHtmlDocument page, IReadOnlyList<KeyValuePair<string, string>> answers)
            => PostAsync(Client, link, page, answers);

        /// <inheritdoc cref="PostAsync(string, IHtmlDocument, IReadOnlyList{KeyValuePair{string, string}})" />
        public async Task<HttpResponseMessage> PostAsync(
            HttpClient client,
            string link,
            IHtmlDocument page,
            IReadOnlyList<KeyValuePair<string, string>> answers)
        {
            var form = page.QuerySelector("form[method=post]")!;
            var fields = new List<KeyValuePair<string, string>>
            {
                new("_handler", form.QuerySelector("input[name=_handler]")!.GetAttribute("value")!)
            };
            fields.AddRange(answers);

            return await client.PostAsync(link, new FormUrlEncodedContent(fields));
        }

        /// <summary>
        /// A browser of its own (its own cookies), whose requests come from <paramref name="address" /> as Caddy forwards
        /// it. Its own address keeps it out of the sign-in and respondent throttles the shared client's requests count
        /// against (the test server has no client address of its own).
        /// </summary>
        public HttpClient NewBrowser(string address)
        {
            var client = Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            client.DefaultRequestHeaders.Add("X-Forwarded-For", address);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("RespondentBrowser/1.0");
            return client;
        }

        /// <summary>
        /// The password of everyone a test signs in (<see cref="SignInAsync" />): whoever <see cref="CreateUserAsync" />
        /// made, and the trainee once <see cref="LetTheSubjectSignInAsync" /> has run.
        /// </summary>
        public const string SignInPassword = "Respondent-Pa55word!";

        /// <summary>An Assessor at the trainee's institution who signs in with a password: a respondent who is also a user.</summary>
        public Task<WombatIdentityUser> CreateAssessorAsync(string email) => CreateUserAsync(email, WombatRoles.Assessor);

        /// <summary>A user at the trainee's institution who holds <paramref name="role" /> and signs in with a password.</summary>
        public async Task<WombatIdentityUser> CreateUserAsync(string email, string role)
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var institutionId = await dbContext.Institutions
                .Where(entity => entity.ShortCode == DemoInstitutionShortCode)
                .Select(entity => entity.Id)
                .SingleAsync();

            var users = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
            var user = new WombatIdentityUser
            {
                UserName = email,
                Email = email,
                FirstName = "Signed",
                LastName = role,
                InstitutionId = institutionId
            };
            (await users.CreateAsync(user, SignInPassword)).Succeeded.Should().BeTrue($"guard: the {role} exists");
            (await users.AddToRoleAsync(user, role)).Succeeded.Should().BeTrue($"guard: the {role} holds the role");
            return user;
        }

        /// <summary>
        /// Gives the trainee the campaigns are about a password, and <paramref name="roles" /> beside Trainee, so a browser
        /// can sign in as them (<see cref="SubjectEmail" />). Once per host: it is their one password.
        /// </summary>
        public async Task LetTheSubjectSignInAsync(params string[] roles)
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
            var subject = await users.FindByIdAsync(SubjectUserId);
            subject.Should().NotBeNull("guard: the subject exists");

            (await users.AddPasswordAsync(subject!, SignInPassword)).Succeeded.Should().BeTrue("guard: the subject has a password");
            (await users.AddToRolesAsync(subject!, roles)).Succeeded.Should().BeTrue("guard: the subject holds the roles");
        }

        /// <summary>Signs a browser in through the sign-in page's own form, as a person does.</summary>
        public async Task SignInAsync(HttpClient client, string email)
        {
            using var loginPage = await client.GetAsync("/account/login");
            var form = new HtmlParser().ParseDocument(await loginPage.Content.ReadAsStringAsync())
                .QuerySelector("form[action='/account/login/submit']")!;

            using var signIn = await client.PostAsync("/account/login/submit", new FormUrlEncodedContent(
            [
                new("__RequestVerificationToken", form.QuerySelector("input[name=__RequestVerificationToken]")!.GetAttribute("value")!),
                new("Email", email),
                new("Password", SignInPassword)
            ]));

            signIn.StatusCode.Should().Be(HttpStatusCode.Redirect);
            signIn.Headers.Location!.ToString().Should().NotContain("/account/login", "guard: the sign-in succeeded");
        }

        public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<ISender>().Send(request);
        }

        public async Task SendAsync(IRequest request)
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<ISender>().Send(request);
        }

        /// <summary>
        /// Admits the trainee onto the College's paediatric curriculum under a name, and builds the coordinator who runs
        /// their campaigns, as <c>MsfRespondEndpointFlowTests</c> does for the Api host.
        /// </summary>
        private async Task AdmitSubjectAsync()
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var curriculumId = await dbContext.Curricula
                .Where(entity => entity.Name == PaediatricCurriculumName)
                .Select(entity => entity.Id)
                .SingleAsync();
            _coveredEpaId = await dbContext.CurriculumItems
                .Where(item => item.CurriculumId == curriculumId && item.OwningInstitutionId == null)
                .OrderBy(item => item.Epa.Code)
                .Select(item => item.EpaId)
                .FirstAsync();
            _learnerFeedbackEpaId = await dbContext.CurriculumItems
                .Where(item => item.CurriculumId == curriculumId && item.OwningInstitutionId == null && item.Epa.Code == "PAED-015")
                .Select(item => item.EpaId)
                .SingleAsync();
            var institutionId = await dbContext.Institutions
                .Where(entity => entity.ShortCode == DemoInstitutionShortCode)
                .Select(entity => entity.Id)
                .SingleAsync();

            dbContext.TraineeProfiles.Add(new TraineeProfile
            {
                UserId = SubjectUserId,
                InstitutionId = institutionId,
                CurriculumId = curriculumId,
                ProgrammeStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                ExpectedCompletionDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(3)),
                IsActive = true
            });
            await dbContext.SaveChangesAsync();

            var names = TraineeName.Split(' ');
            var users = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
            var subject = new WombatIdentityUser
            {
                Id = SubjectUserId,
                UserName = SubjectEmail,
                Email = SubjectEmail,
                FirstName = names[0],
                LastName = names[1],
                InstitutionId = institutionId
            };
            (await users.CreateAsync(subject)).Succeeded.Should().BeTrue("guard: the subject exists as a named user");

            // A campaign is started only about a current trainee, whose account holds Trainee (T238). The web host's
            // startup seeds the roles.
            (await users.AddToRoleAsync(subject, WombatRoles.Trainee)).Succeeded
                .Should().BeTrue("guard: the subject holds Trainee");

            Coordinator = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, "coordinator-web-1"),
                    new Claim(ClaimTypes.Role, WombatRoles.Coordinator),
                    new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
                ],
                "IntegrationTest",
                ClaimTypes.Name,
                ClaimTypes.Role));
        }
    }

    /// <summary>
    /// <c>Wombat.Web</c> as the server runs it, but for the database it is pointed at, the email it sends (captured) and
    /// the scheduler (removed: nothing in these flows wants a background job writing beside them).
    /// </summary>
    public sealed class WebFactory : WebApplicationFactory<WombatWeb::Program>
    {
        private readonly string _connectionString;
        private readonly string _respondUrl;

        public WebFactory(string connectionString, string respondUrl)
        {
            _connectionString = connectionString;
            _respondUrl = respondUrl;
        }

        public MsfRespondEndpointFlowTests.CapturingEmailSender EmailSender { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Not Development: that would load the developer's own settings and seed the dev users. Not Production
            // either, whose settings file is the server's; this environment has none.
            builder.UseEnvironment("IntegrationTest");
            builder.UseSetting("ConnectionStrings:DefaultConnection", _connectionString);
            builder.UseSetting("Wombat:MsfRespondUrl", _respondUrl);

            // The static-asset endpoints a publish maps. A build's manifest (this one) switches on the development
            // reloader, which also maps a GET/HEAD fallback, {**path:file}, that the server does not have. Its route
            // matches every address before its constraint is checked, so it turned a POST to an unknown address into a
            // 405, and a GET of a POST-only address into a 404, which the server answers the other way round (T233).
            builder.UseSetting("ReloadStaticAssetsAtRuntime", "false");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IEmailSender>();
                services.AddSingleton<IEmailSender>(EmailSender);
                services.Remove(services.Single(descriptor =>
                    descriptor.ServiceType == typeof(IHostedService)
                    && descriptor.ImplementationType == typeof(ScheduledJobHost)));
            });
        }
    }
}
