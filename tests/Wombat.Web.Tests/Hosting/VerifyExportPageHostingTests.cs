using System.Net;
using System.Text.RegularExpressions;
using AngleSharp.Html.Parser;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Reporting;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Hosting;

/// <summary>
/// The export check as a browser meets it: over HTTP, through the framework's Razor-components endpoint, under the app's
/// fallback policy (T265).
/// </summary>
/// <remarks>
/// A visitor who has not signed in gets static pages (T181), so until T265 their Verify button, a circuit's, did nothing.
/// The page is static for every visitor now, and Verify is a GET form: the check is a page load, which is all this host
/// has to make.
/// </remarks>
public sealed partial class VerifyExportPageHostingTests
{
    private const string Hash = "9b74c9897bac770ffc029102a200c5de2bdb54a4f0a5f6c1b1d6d0e2c6a9e3f1";

    [Fact]
    public async Task AVisitorWhoHasNotSignedIn_GetsTheForm_AndPressingVerifyChecksTheHash()
    {
        var sender = new VerifySender();
        await using var host = await StartAsync(sender);

        using var load = await host.Client.GetAsync("/portfolio/verify");
        load.StatusCode.Should().Be(HttpStatusCode.OK, $"not a redirect to the sign-in page ({load.Headers.Location})");
        var page = new HtmlParser().ParseDocument(await load.Content.ReadAsStringAsync());
        var form = page.QuerySelector("form")!;
        form.GetAttribute("method").Should().Be("get");
        form.GetAttribute("action").Should().Be("/portfolio/verify");
        var verify = form.QuerySelector("button[type=submit]")!;

        // What the browser loads when Verify is pressed: the form's fields, and the button's own name and value.
        var address = $"{form.GetAttribute("action")}?{form.QuerySelector("input")!.GetAttribute("name")}={Hash}" +
                      $"&{verify.GetAttribute("name")}={verify.GetAttribute("value")}";
        using var check = await host.Client.GetAsync(address);
        var html = await check.Content.ReadAsStringAsync();

        check.StatusCode.Should().Be(HttpStatusCode.OK);
        sender.Checked.Should().Equal([Hash]);
        var answer = new HtmlParser().ParseDocument(html);
        answer.QuerySelector(".action-result h3")!.TextContent.Should().Be("Export verified");
        answer.QuerySelector(".action-result")!.HasAttribute("autofocus").Should().BeTrue("the answer to a press takes the focus");
        ServerComponentMarker.IsIn(html).Should().BeFalse("the page has no interactive root");
    }

    [Fact]
    public async Task ASignedInVisitor_GetsTheSameStaticPage()
    {
        var sender = new VerifySender();
        await using var host = await StartAsync(sender, signedIn: true);

        var (_, html, document) = await host.LoadAsync($"/portfolio/verify?hash={Hash}&check=1");

        ServerComponentMarker.IsIn(html).Should().BeFalse("[ExcludeFromInteractiveRouting]: one page for every visitor");
        document.QuerySelector(".action-result h3")!.TextContent.Should().Be("Export verified");
        var (_, otherHtml, _) = await host.LoadAsync(AppTestHost.AnonymousPage);
        ServerComponentMarker.IsIn(otherHtml).Should().BeTrue("guard: other pages are still interactive for them");
    }

    [Fact]
    public async Task ACheckThatFails_IsAnsweredWithThePage_SayingSo_NotWithTheErrorPage()
    {
        await using var host = await StartAsync(new VerifySender { Failure = new InvalidOperationException("db down") });

        using var check = await host.Client.GetAsync($"/portfolio/verify?hash={Hash}&check=1");
        var document = new HtmlParser().ParseDocument(await check.Content.ReadAsStringAsync());

        check.StatusCode.Should().Be(HttpStatusCode.OK);
        document.QuerySelector(".action-result .alert-danger")!.TextContent.Trim()
            .Should().Be("The export could not be checked just now. Please try again in a few minutes.");
        document.QuerySelector("form").Should().NotBeNull();
    }

    private static Task<AppTestHost> StartAsync(VerifySender sender, bool signedIn = false)
        => AppTestHost.StartAsync(services =>
        {
            services.AddSingleton<IScopedSender>(sender);
            if (signedIn)
            {
                SignedInVisitor.Register(services);
            }
        });

    private sealed class VerifySender : IScopedSender
    {
        public List<string> Checked { get; } = [];

        public Exception? Failure { get; init; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var query = (VerifyExportQuery)(object)request;
            Checked.Add(query.ContentHash);
            if (Failure is not null)
            {
                return Task.FromException<TResponse>(Failure);
            }

            var export = new PortfolioExportRecordDto(
                7, "trainee-1", "admin-1", new DateTime(2026, 9, 1, 8, 30, 0, DateTimeKind.Utc), null, null, Hash, "portfolio.pdf");
            return Task.FromResult((TResponse)(object)export);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
