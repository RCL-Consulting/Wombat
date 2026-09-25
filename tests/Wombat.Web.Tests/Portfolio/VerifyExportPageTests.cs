using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Reporting;
using Wombat.Web.Components.Pages.Portfolio;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;

namespace Wombat.Web.Tests.Portfolio;

/// <summary>
/// T265: the export check works for whoever holds the PDF, signed in or not, and a check that fails says so on the page.
/// </summary>
/// <remarks>
/// <para>
/// Until T265 Verify was a button in a circuit. A visitor who has not signed in never has a circuit (T181), so for them it
/// did nothing: only a link that carried the hash checked it. And the check caught nothing, so a failed one was an
/// unhandled exception. The page is now static for every visitor, with a GET form: Verify loads the page with the hash
/// in its address, and the check is made as the page renders. <c>Hosting/VerifyExportPageHostingTests</c> loads it over
/// HTTP; here bUnit renders it from its address, which is what the browser's GET gives the page.
/// </para>
/// </remarks>
public sealed class VerifyExportPageTests : TestContext
{
    /// <summary>What <see cref="ElementReference" />.FocusAsync calls.</summary>
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    private const string Hash = "9b74c9897bac770ffc029102a200c5de2bdb54a4f0a5f6c1b1d6d0e2c6a9e3f1";

    private static readonly PortfolioExportRecordDto Export = new(
        7, "trainee-1", "admin-1", new DateTime(2026, 9, 1, 8, 30, 0, DateTimeKind.Utc),
        new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), Hash, "portfolio-9b74c9897bac.pdf");

    public VerifyExportPageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void ThePage_IsStaticForEveryVisitor_AndOpenToOneWhoHasNotSignedIn()
    {
        typeof(VerifyExport).GetCustomAttributes(typeof(ExcludeFromInteractiveRoutingAttribute), inherit: true)
            .Should().ContainSingle("a signed-in visitor gets the same GET form, not a circuit button");
        typeof(VerifyExport).GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true)
            .Should().ContainSingle("whoever holds the PDF checks it");
    }

    [Fact]
    public void Verify_IsAFormTheBrowserSends_WithTheHashInTheAddress_NotAButtonForACircuit()
    {
        var sender = new VerifySender(Export);
        var cut = Render("/portfolio/verify", sender);

        var form = cut.Find("form");
        form.GetAttribute("method").Should().Be("get");
        form.GetAttribute("action").Should().Be("/portfolio/verify");
        cut.Find("#contentHash").GetAttribute("name").Should().Be("hash");
        cut.Find("#contentHash").HasAttribute("required").Should().BeTrue("a browser sends no empty hash");

        var verify = cut.Find("form button[type=submit]");
        verify.TextContent.Trim().Should().Be("Verify");
        verify.GetAttribute("name").Should().Be(VerifyExport.PressedParameter);
        verify.GetAttribute("value").Should().Be("1");
        cut.FindAll("[blazor\\:onclick], [blazor\\:onsubmit]").Should().BeEmpty(
            "nothing on the page may need a circuit: a visitor who has not signed in has none");

        sender.Checked.Should().BeEmpty("nothing was asked");
        cut.FindAll(".alert, .detail-card").Should().BeEmpty();
        cut.Find(".action-result").HasAttribute("autofocus").Should().BeFalse();
        FocusCalls().Should().BeEmpty();
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void AfterAPressOfVerify_TheHashIsChecked_AndTheResultTakesTheFocusAsThePageLoads()
    {
        var sender = new VerifySender(Export);
        var cut = Render($"/portfolio/verify?hash=%20{Hash}%20&check=1", sender);

        sender.Checked.Should().Equal([Hash], "the hash from the address, trimmed");
        var result = cut.Find(".action-result");
        result.QuerySelector("h3")!.TextContent.Should().Be("Export verified");
        result.TextContent.Should().Contain("2026-09-01 08:30 UTC").And.Contain("2026-01-01 to 2026-06-30")
            .And.Contain("portfolio-9b74c9897bac.pdf");
        cut.Find("#contentHash").GetAttribute("value").Should().Be($" {Hash} ", "the field keeps what was checked");

        result.HasAttribute("autofocus").Should().BeTrue("a static page takes the focus from its HTML");
        var region = cut.FindComponent<ActionResult>();
        FocusCalls().Should().ContainSingle().Which.Arguments[0].Should().BeOfType<ElementReference>()
            .Which.Id.Should().Be(region.Instance.Element.Id);
    }

    [Fact]
    public void ALinkThatCarriesTheHash_ChecksIt_AndLeavesTheFocusAlone()
    {
        var sender = new VerifySender(Export);
        var cut = Render($"/portfolio/verify?hash={Hash}", sender);

        sender.Checked.Should().Equal([Hash]);
        cut.Find(".action-result h3").TextContent.Should().Be("Export verified");
        cut.Find(".action-result").HasAttribute("autofocus").Should().BeFalse("the visitor arrived; nothing was pressed");
        FocusCalls().Should().BeEmpty();
    }

    [Fact]
    public void AHashNoExportHas_IsSaidToMatchNothing()
    {
        var cut = Render($"/portfolio/verify?hash={Hash}&check=1", new VerifySender(null));

        var warning = cut.Find(".action-result .alert.alert-warning");
        warning.TextContent.Should().Contain("No matching export found");
        warning.GetAttribute("role").Should().Be("status");
        FocusCalls().Should().ContainSingle();
    }

    [Fact]
    public void ACheckThatFails_IsSaidOnThePage_AsARefusal_AndNotWhy()
    {
        const string detail = "connection-refused-detail-for-the-log";
        var sender = new VerifySender(Export) { Failure = new InvalidOperationException(detail) };

        var cut = Render($"/portfolio/verify?hash={Hash}&check=1", sender);

        var refusal = cut.Find(".action-result .alert.alert-danger");
        refusal.TextContent.Trim().Should().Be("The export could not be checked just now. Please try again in a few minutes.");
        refusal.GetAttribute("role").Should().Be("alert");
        cut.Markup.Should().NotContain(detail, "the fault's message is for the log");
        cut.FindAll(".detail-card").Should().BeEmpty("no answer was given");
        FocusCalls().Should().ContainSingle("the refusal arrives with the page, and the focus reads it");
        cut.Find("form").Should().NotBeNull("the visitor can try again");
    }

    [Fact]
    public void ABlankHash_IsRefused_AndNothingIsChecked()
    {
        var sender = new VerifySender(Export);
        var cut = Render("/portfolio/verify?hash=%20%20&check=1", sender);

        sender.Checked.Should().BeEmpty();
        cut.Find(".action-result .alert.alert-danger").TextContent.Trim().Should().Be("Enter the content hash to check.");
        FocusCalls().Should().ContainSingle();
    }

    private IRenderedComponent<VerifyExport> Render(string address, VerifySender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo(address);
        return RenderComponent<VerifyExport>();
    }

    private IReadOnlyList<JSRuntimeInvocation> FocusCalls()
        => JSInterop.Invocations.Where(invocation => invocation.Identifier == FocusIdentifier).ToList();

    /// <summary>Answers the export check with <paramref name="export" />, or fails it.</summary>
    private sealed class VerifySender(PortfolioExportRecordDto? export) : IScopedSender
    {
        public List<string> Checked { get; } = [];

        public Exception? Failure { get; init; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var query = request.Should().BeOfType<VerifyExportQuery>().Subject;
            Checked.Add(query.ContentHash);
            return Failure is not null ? Task.FromException<TResponse>(Failure) : Task.FromResult((TResponse)(object?)export!);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("The export check sends no command.");
    }
}
