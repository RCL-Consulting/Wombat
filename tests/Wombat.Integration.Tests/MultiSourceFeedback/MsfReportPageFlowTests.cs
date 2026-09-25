using System.Net;
using AngleSharp.Html.Parser;
using FluentAssertions;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Integration.Tests.MultiSourceFeedback;

/// <summary>
/// T269 on a full page load: the trainee a campaign is about, holding Coordinator besides, who types the coordinator's
/// report address (<c>/msf/reports/{id}</c>) is redirected by the server to their own copy (<c>/msf/my-reports/{id}</c>)
/// before any of the coordinator's page is sent, and neither response names a teaching context (T164).
/// </summary>
/// <remarks>
/// <para>
/// The bUnit tests (<c>SubjectReadsOwnReportTests</c>) cover the redirect inside a circuit, where it is a navigation
/// that replaces the history entry. A page loaded from the address bar, a bookmark or a link in an email is prerendered
/// instead, and there <c>NavigationManager.NavigateTo</c> is the server's redirect: <c>Wombat.Web.csproj</c> sets
/// <c>BlazorDisableThrowNavigationException</c>, so it throws nothing and the renderer answers with the redirect. That is
/// the path a subject takes in practice, since no page links them to the coordinator's report: the campaign list and the
/// campaign page leave out a campaign about the caller (T224).
/// </para>
/// <para>
/// The whole of <c>Wombat.Web</c>'s <c>Program.cs</c>, hosted on a schema of its own by the MSF respondent page's fixture
/// (<see cref="MsfRespondPageFlowTests.WebHost" />), with its endpoint authorization and its render modes: a signed-in
/// user's page is interactive, so a page load is its prerender.
/// </para>
/// </remarks>
public sealed class MsfReportPageFlowTests : IClassFixture<MsfRespondPageFlowTests.WebHost>
{
    /// <summary>What Chrome sends when it loads a page from the address bar or a link.</summary>
    private const string PageLoadAccept =
        "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8," +
        "application/signed-exchange;v=b3;q=0.7";

    private const string TeachingContext = "Neonatal night teaching";
    private const string Narrative = "Learners describe clear, well paced teaching.";

    private readonly MsfRespondPageFlowTests.WebHost _host;

    public MsfReportPageFlowTests(MsfRespondPageFlowTests.WebHost host)
    {
        _host = host;
    }

    [Fact]
    public async Task TheSubject_LoadingTheCoordinatorsReportPage_IsRedirectedToTheirOwnCopy_AndNeitherNamesAContext()
    {
        var campaignId = await ReleasedLearnerFeedbackAsync();

        // Guard: a coordinator who runs the campaign loads the report on this page, the teaching context named, so the
        // page load renders the report whole and what the subject is not sent below is what they would have been.
        const string coordinatorEmail = "coordinator-report-page@example.test";
        await _host.CreateUserAsync(coordinatorEmail, WombatRoles.Coordinator);
        using var coordinator = _host.NewBrowser("203.0.113.61");
        await _host.SignInAsync(coordinator, coordinatorEmail);

        using var coordinatorsLoad = await LoadPageAsync(coordinator, $"/msf/reports/{campaignId}");
        var coordinatorsPage = await coordinatorsLoad.Content.ReadAsStringAsync();
        coordinatorsLoad.StatusCode.Should().Be(HttpStatusCode.OK, coordinatorsPage);
        coordinatorsPage.Should().Contain("Coordinator actions")
            .And.Contain($"1 ({TeachingContext})", "whoever runs the campaign is told the contexts they typed")
            .And.Contain(Narrative);

        // The subject, who holds Coordinator as well, so the page's own [Authorize] admits them.
        await _host.LetTheSubjectSignInAsync(WombatRoles.Coordinator);
        using var subject = _host.NewBrowser("203.0.113.62");
        await _host.SignInAsync(subject, MsfRespondPageFlowTests.WebHost.SubjectEmail);

        using var subjectsLoad = await LoadPageAsync(subject, $"/msf/reports/{campaignId}");
        var sent = await subjectsLoad.Content.ReadAsStringAsync();
        subjectsLoad.StatusCode.Should().Be(HttpStatusCode.Redirect, sent);
        subjectsLoad.Headers.Location.Should().NotBeNull();
        var location = subjectsLoad.Headers.Location!;
        (location.IsAbsoluteUri ? location.PathAndQuery : location.OriginalString)
            .Should().Be($"/msf/my-reports/{campaignId}", "the subject reads the report on their own page");

        // The redirect has a body, which a browser does not show: the page as the prerender first rendered it, before the
        // report was read, since the prerender renders nothing after the navigation (observed, .NET 10).
        sent.Should().NotContain("Coordinator actions")
            .And.NotContain(TeachingContext)
            .And.NotContain(Narrative, "nothing of the report is sent with the redirect");

        using var ownLoad = await LoadPageAsync(subject, location.ToString());
        var own = await ownLoad.Content.ReadAsStringAsync();
        ownLoad.StatusCode.Should().Be(HttpStatusCode.OK, own);

        var page = new HtmlParser().ParseDocument(own);
        page.QuerySelector(".header-container h1")!.TextContent.Should().Be("My MSF reports");
        own.Should().Contain("Selected report")
            .And.Contain(Narrative)
            .And.Contain("Teaching contexts that responded:</strong> 1")
            .And.NotContain(TeachingContext, "the subject is told how many contexts responded, never which (T164)")
            .And.NotContain("Coordinator actions")
            .And.NotContain("Release to trainee");
        page.QuerySelectorAll(".alert-danger").Should().BeEmpty();
    }

    /// <summary>
    /// A learner-feedback campaign about the trainee, answered by four learners taught in <see cref="TeachingContext" />,
    /// closed and released with <see cref="Narrative" />, as its coordinator does it.
    /// </summary>
    private async Task<int> ReleasedLearnerFeedbackAsync()
    {
        string[] learners = Enumerable.Range(1, 4).Select(learner => $"report-page-learner-{learner}@example.test").ToArray();
        var campaign = await _host.OpenCampaignAsync(learners, MsfTemplateKind.LearnerFeedback, TeachingContext);
        var scale = await _host.QuestionIdAsync(campaign.TemplateId, MsfQuestionType.Scale);

        foreach (var learner in learners)
        {
            var link = _host.LinkMailedTo(learner);
            var token = Uri.UnescapeDataString(link[(link.IndexOf("token=", StringComparison.Ordinal) + "token=".Length)..]);
            await _host.SendAsync(new SubmitMsfResponseCommand(token, [new SubmitMsfResponseAnswerItem(scale, 4, null)]));
        }

        var closed = await _host.SendAsync(new CloseMsfCampaignCommand(campaign.Id, _host.Coordinator));
        closed.ReadyForRelease.Should().BeTrue("guard: four learners clear the thresholds");
        await _host.SendAsync(new ReleaseMsfCampaignCommand(campaign.Id, "coordinator-web-1", Narrative, null, _host.Coordinator));
        return campaign.Id;
    }

    private static Task<HttpResponseMessage> LoadPageAsync(HttpClient browser, string address)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, address);
        request.Headers.TryAddWithoutValidation("Accept", PageLoadAccept);
        return browser.SendAsync(request);
    }
}
