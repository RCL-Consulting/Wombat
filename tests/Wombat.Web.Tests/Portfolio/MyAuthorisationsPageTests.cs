using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Portfolio;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;

namespace Wombat.Web.Tests.Portfolio;

/// <summary>
/// My authorisations (T366, flow 09, boards R3-5; replaces T367's interim section): In force as cards in an equal-column
/// grid, each named for its EPA (F4), its expiry counted on the South African calendar (S3); No longer in force as a
/// captioned stacking table with Since and Why (T367); the states (none, none in force, loading, load error); and a
/// download's refusal and failure under its button in fixed words (C19, T272).
/// </summary>
public sealed class MyAuthorisationsPageTests : TestContext
{
    // 2026-10-12 22:30 UTC is already 2026-10-13 in South Africa: "today" must be read there (S3, T325).
    private static readonly DateTimeOffset Now = new(2026, 10, 12, 22, 30, 0, TimeSpan.Zero);

    private readonly StarsSender _sender = new();

    public MyAuthorisationsPageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("lerato@test");
        auth.SetRoles(WombatRoles.Trainee);
        auth.SetClaims(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "lerato"));
        Services.AddSingleton<IScopedSender>(_sender);
        Services.AddSingleton<TimeProvider>(new FixedClock(Now));
    }

    [Fact]
    public void NoStarAtAll_IsTheEmptyCard_AndNoLongerInForceIsHidden()
    {
        var cut = Render([]);

        cut.Find(".state-panel-title").TextContent.Should().Be("No active authorisations yet");
        cut.Markup.Should().NotContain("No longer in force");
    }

    [Fact]
    public void ThreeInForce_AreCardsInTheGrid_WithIsoDates_AndOneExpiringOnTheSouthAfricanCalendar()
    {
        var cut = Render(
        [
            Star(1, "PAED-001"),
            Star(2, "PAED-010") with { ExpiresOn = new DateOnly(2026, 11, 2) },
            Star(3, "PAED-012")
        ]);

        cut.Find(".my-authorisations h2").TextContent.Should().Be("In force");
        var cards = cut.FindAll(".star-grid > article.detail-card.star-card").ToArray();
        cards.Should().HaveCount(3);
        Facts(cards[0]).Should().Equal(("Authorised level", "5"), ("Issued", "2026-10-13"), ("Expires", "No expiry"));
        Facts(cards[1])[2].Should().Be(("Expires", "2026-11-02 Expires in 20 days"));
        cards[1].QuerySelector("dd .badge")!.TextContent.Should().Be("Expires in 20 days");
        cards[0].QuerySelector(".star-rationale")!.TextContent.Should().Be("Entrusted.");

        cut.FindAll(".star-card .form-actions button").Select(button => AccessibleNames.NameOf(cut, button))
            .Should().Equal("Download certificate for PAED-001", "Download certificate for PAED-010", "Download certificate for PAED-012");
        cut.FindAll(".star-card .form-actions button").Should().OnlyContain(button => button.TextContent.Trim() == "Download certificate");
        cut.Markup.Should().NotContain("No longer in force");
    }

    [Fact]
    public void ARevokedStar_IsNoLongerInForce_SinceItsSouthAfricanDay_WhyInTheRevisersNameAndWords()
    {
        var cut = Render(
        [
            Star(1, "PAED-001"),
            Star(2, "PAED-002", "Managing common paediatric presentations") with
            {
                AuthorisedLevelLabel = "3b",
                Status = EntrustmentDecisionStatus.Revoked,
                RevokedOn = new DateTime(2026, 10, 12, 23, 10, 0, DateTimeKind.Utc),
                RevokedByName = "Refilwe Mokoena",
                RevocationReason = "Staged on evidence about PAED-001, not PAED-002; to be decided again."
            }
        ]);

        cut.FindAll(".star-card").Should().ContainSingle();
        var table = cut.Find("table.clinic-table.clinic-table--stack.clinic-table--stack-wide");
        table.QuerySelector("caption")!.TextContent.Should().Be("STARs that are no longer in force: revoked, or past their expiry");
        var row = table.QuerySelector("tbody tr")!;
        row.QuerySelector("th[scope=row]")!.TextContent.Should().Be("PAED-002 — Managing common paediatric presentations");
        Cell(row, "Level").Should().Be("3b");
        Cell(row, "Status").Should().Be("Revoked");
        Cell(row, "Since").Should().Be("2026-10-13", "23:10 UTC is the next day in South Africa");
        Cell(row, "Why").Should().Be("Revoked by Refilwe Mokoena: Staged on evidence about PAED-001, not PAED-002; to be decided again.");
        var download = row.QuerySelector(".actions-cell button")!;
        download.TextContent.Trim().Should().Be("Download");
        AccessibleNames.NameOf(cut, download).Should().Be("Download certificate for PAED-002 (revoked)");
    }

    [Fact]
    public void AnExpiredStar_IsNoLongerInForce_FromTheDayAfterItsExpiryDate()
    {
        var cut = Render(
        [
            Star(1, "PAED-001"),
            Star(2, "PAED-010") with { Status = EntrustmentDecisionStatus.Expired, ExpiresOn = new DateOnly(2026, 11, 2) }
        ]);

        var row = cut.Find("tbody tr");
        Cell(row, "Since").Should().Be("2026-11-03", "a STAR is valid through its expiry date (S3)");
        Cell(row, "Why").Should().Be("Its expiry date, 2026-11-02, passed.");
        AccessibleNames.NameOf(cut, row.QuerySelector("button")!).Should().Be("Download certificate for PAED-010 (expired)");
    }

    [Fact]
    public void NoneInForce_KeepsItsHeading_WithItsSentence_ThenNoLongerInForce()
    {
        var cut = Render([Star(2, "PAED-002") with { Status = EntrustmentDecisionStatus.Revoked, RevokedByName = "Refilwe Mokoena", RevocationReason = "Wrong EPA." }]);

        cut.FindAll(".my-authorisations h2").Select(heading => heading.TextContent).Should().Equal("In force", "No longer in force");
        cut.Find(".stars-section > p.muted").TextContent.Should().Be(MyAuthorisations.NoneInForce);
        cut.FindAll(".star-card").Should().BeEmpty();
    }

    [Theory]
    [InlineData(2026, 11, 2, "Expires today")]
    [InlineData(2026, 11, 3, "Expires in 1 day")]
    [InlineData(2026, 11, 12, "Expires in 10 days")]
    [InlineData(2026, 12, 2, "Expires in 30 days")]
    public void TheExpiry_CountsOnTheSouthAfricanCalendar(int year, int month, int day, string words)
        => StarWords.ExpiresIn(new DateOnly(year, month, day), new DateOnly(2026, 11, 2)).Should().Be(words);

    [Fact]
    public void TheExpiry_SaysNothing_MoreThan30DaysOff_OrOncePassed()
    {
        StarWords.ExpiresIn(new DateOnly(2026, 12, 3), new DateOnly(2026, 11, 2)).Should().BeNull();
        StarWords.ExpiresIn(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 2)).Should().BeNull();
    }

    [Fact]
    public void ACertificateNotFound_IsSaidUnderItsButton_WhichResultTakesTheFocus()
    {
        var cut = Render([Star(1, "PAED-001"), Star(2, "PAED-010")]);
        _sender.Certificate = null;

        cut.FindAll(".star-card .form-actions button").ToArray()[1].Click();

        cut.WaitForAssertion(() => cut.FindAll(".star-card").ToArray()[1].QuerySelector(".download-result .alert-danger")!
            .TextContent.Trim().Should().Be("The certificate could not be found."));
        cut.FindAll(".star-card").ToArray()[0].QuerySelector(".download-result").Should().BeNull();
        JSInterop.Invocations.Should().Contain(call => call.Identifier == "Blazor._internal.domWrapper.focus");
        JSInterop.Invocations.Should().NotContain(call => call.Identifier == "wombatFileDownload");
    }

    [Fact]
    public void AFailedDownload_IsFixedWords_WithTryAgain_NeverTheExceptionsText()
    {
        var cut = Render([Star(1, "PAED-012")]);
        _sender.DownloadFailure = new IOException("QuestPDF ran out of fonts.");

        cut.Find(".star-card .form-actions button").Click();

        cut.WaitForAssertion(() => cut.Find(".download-result .alert-row-text").TextContent.Should().Be("The certificate could not be made just now."));
        cut.Markup.Should().NotContain("QuestPDF");

        _sender.DownloadFailure = null;
        cut.Find(".download-result button").Click();
        cut.WaitForAssertion(() => JSInterop.Invocations.Should().Contain(call => call.Identifier == "wombatFileDownload"));
        cut.FindAll(".download-result").Should().BeEmpty("a new press clears the last result");

        // T366, D1: Try again goes with its alert, so the focus moves to the card's Download, by its id, not to the page.
        var download = cut.Find(".star-card .form-actions button");
        download.Id.Should().Be("download-1");
        JSInterop.Invocations.Should().Contain(call => call.Identifier == "wombat.focusById" && Equals(call.Arguments[0], "download-1"));
    }

    [Fact]
    public void WhileTheStarsAreRead_TheStatusSaysSo_AndNoDownloadIsOffered()
    {
        _sender.Pending = new TaskCompletionSource<IReadOnlyList<EntrustmentDecisionDto>>();

        var cut = RenderComponent<MyAuthorisations>();

        cut.Find("p[role=status]").TextContent.Should().Be("Loading your authorisations.");
        cut.FindAll(".skeleton").Should().NotBeEmpty();
        cut.FindAll("button").Should().BeEmpty();
    }

    [Fact]
    public void AFailedRead_IsFixedWords_WithTryAgain_NeverTheExceptionsText()
    {
        _sender.ListFailure = new TimeoutException("The database did not answer within 30 seconds.");

        var cut = RenderComponent<MyAuthorisations>();

        cut.WaitForAssertion(() => cut.Find(".alert-danger").TextContent.Should().Contain(MyAuthorisations.LoadFailed));
        cut.FindAll("button").Should().Contain(button => button.TextContent.Trim() == "Try again");
        cut.Markup.Should().NotContain("30 seconds");
    }

    private IRenderedComponent<MyAuthorisations> Render(IReadOnlyList<EntrustmentDecisionDto> stars)
    {
        _sender.List = stars;
        var cut = RenderComponent<MyAuthorisations>();
        cut.WaitForState(() => cut.FindAll(".skeleton").Count == 0);
        return cut;
    }

    private static (string, string)[] Facts(AngleSharp.Dom.IElement card)
    {
        var terms = card.QuerySelectorAll("dl.detail-list dt").Select(term => term.TextContent.Trim()).ToArray();
        var values = card.QuerySelectorAll("dl.detail-list dd")
            .Select(value => System.Text.RegularExpressions.Regex.Replace(value.TextContent, @"\s+", " ").Trim()).ToArray();
        return terms.Zip(values).ToArray();
    }

    private static string Cell(AngleSharp.Dom.IElement row, string label)
        => row.QuerySelector($"td[data-label={label}]")!.TextContent.Trim();

    private static EntrustmentDecisionDto Star(int id, string code, string title = "An EPA")
        => new(
            id, "lerato", id, code, title, true, 13, "5", 5, new DateOnly(2026, 10, 13), null, 1, "chair-1",
            "Entrusted.", EntrustmentDecisionStatus.Active, null, null, null, null, []);

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class StarsSender : IScopedSender
    {
        public IReadOnlyList<EntrustmentDecisionDto> List { get; set; } = [];

        public TaskCompletionSource<IReadOnlyList<EntrustmentDecisionDto>>? Pending { get; set; }

        public Exception? ListFailure { get; set; }

        public Exception? DownloadFailure { get; set; }

        public EntrustmentCertificateResult? Certificate { get; set; } = new([1, 2, 3], "star.pdf", "hash");

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case GetDecisionsForTraineeQuery:
                    if (ListFailure is not null)
                    {
                        throw ListFailure;
                    }

                    return (TResponse)(object)(Pending is null ? List : await Pending.Task);
                case DownloadEntrustmentCertificateCommand:
                    if (DownloadFailure is not null)
                    {
                        throw DownloadFailure;
                    }

                    return (TResponse)(object)Certificate!;
                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
