using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;
using DecisionsPage = Wombat.Web.Components.Pages.Admin.EntrustmentDecisions.Index;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// The STAR register (T366, flow 09, boards R3-6): its states (none issued, five, filtered with the status, filtered to
/// nothing, loading, load error), Issued's review link (S7, M9), the revoked and superseded lines (S4, C22), and Revoke's
/// dialog with its reason box (M5): open, blank refused (F3), refused in the dialog (M4) and done.
/// </summary>
public sealed class StarRegisterTests : TestContext
{
    private const string FocusById = "wombat.focusById";

    private readonly RegisterSender _sender = new();

    public StarRegisterTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("refilwe@test");
        auth.SetRoles(WombatRoles.SpecialityAdmin);
        Services.AddSingleton<IScopedSender>(_sender);
    }

    [Fact]
    public void NoneIssued_IsTheEmptyCard_WithNoFormToFilter()
    {
        var cut = Render([]);

        cut.Find(".state-panel-title").TextContent.Should().Be("No STARs issued yet");
        cut.Find(".state-panel-copy").TextContent.Should().Be("When a committee ratifies an entrustment decision, its STAR is listed here.");
        cut.FindAll("form.search-container").Should().BeEmpty("there is nothing to filter");
        cut.FindAll("table").Should().BeEmpty();
    }

    [Fact]
    public void FiveIssued_AreCaptionedCounted_WithTheTraineeAsTheRowHeader_AndIssuedLinkingItsReview()
    {
        var cut = Render(Five());

        var table = cut.Find("table");
        table.ClassList.Should().Contain(["clinic-table", "clinic-table--stack", "clinic-table--stack-wide"]);
        table.QuerySelector("caption")!.TextContent.Should().Be("Issued STARs (entrustment decisions) · 5 shown");
        cut.FindAll("thead th").Should().OnlyContain(header => header.GetAttribute("scope") == "col");

        var first = cut.FindAll("tbody tr").ToArray()[0];
        var trainee = first.QuerySelector("th")!;
        trainee.GetAttribute("scope").Should().Be("row");
        trainee.TextContent.Should().Be("Anele Dlamini");

        var link = first.QuerySelector("td[data-label=Issued] a.issued-review")!;
        link.GetAttribute("href").Should().Be("/committee/reviews/2");
        link.TextContent.Should().Be("review #2");
        AccessibleNames.NameOf(cut, link).Should().Be("Open review #2", "the visible words inside the name (the nit)");
        first.QuerySelector("td[data-label=Issued]")!.TextContent.Should().StartWith("2026-10-13");
        first.QuerySelector("td[data-label=Expires]")!.TextContent.Should().Be("—");

        cut.FindAll("tbody .actions-cell button").Should().OnlyContain(button => button.ClassList.Contains("btn-outline"),
            "a row's Revoke is an outline button; the red one is the dialog's (C21)");
        cut.Find("p[role=status]").TextContent.Should().BeEmpty("nothing is announced before Show (S11)");
    }

    [Fact]
    public void Issued_IsPlainText_WhereTheReaderMayNotOpenTheReview()
    {
        var cut = Render([Star(1, "Anele Dlamini", "PAED-002", reviewId: 2) with { IssuingReviewReadable = false }]);

        var issued = cut.Find("td[data-label=Issued]");
        issued.QuerySelectorAll("a").Should().BeEmpty("S7: a link only where the review will open");
        issued.TextContent.Should().Contain("review #2");
    }

    [Fact]
    public void Show_AppliesTheFilters_AnnouncesTheCount_AndOffersClearFilters()
    {
        var cut = Render(Five());
        cut.FindAll("button").Should().NotContain(button => button.TextContent.Trim() == "Clear filters");
        _sender.List = [.. Five().Where(star => star.TraineeName == "Lerato Molefe")];

        cut.Find("#filter-trainee").Change("Molefe");
        _sender.Lists.Should().HaveCount(1, "the field binds on change and applies nothing until Show (F9)");
        cut.Find("form.search-container").Submit();

        cut.WaitForAssertion(() => cut.Find("p[role=status]").TextContent.Should().Be("3 shown"));
        _sender.Lists.Last().TraineeFilter.Should().Be("Molefe");
        cut.Find("caption").TextContent.Should().Be("Issued STARs (entrustment decisions) · 3 shown");
        cut.Find("form.search-container").GetAttribute("aria-label").Should().Be("Filter Entrustment decisions");
        cut.FindAll("form button").Select(button => button.TextContent.Trim()).Should().Equal("Show", "Clear filters");
    }

    [Fact]
    public void FilteredToNothing_KeepsTheCount_AndSaysWhatWasAsked_WithClearFilters()
    {
        var cut = Render(Five());
        _sender.List = [];

        cut.Find("#filter-trainee").Change("Mahlangu");
        cut.Find("#filter-status").Change("Revoked");
        cut.Find("form.search-container").Submit();

        cut.WaitForAssertion(() => cut.Find(".list-section-count").TextContent.Should().Be("Issued STARs (entrustment decisions) · 0 shown"));
        _sender.Lists.Last().StatusFilter.Should().Be(EntrustmentDecisionStatus.Revoked);
        cut.Find(".state-panel-title").TextContent.Should().Be("No STAR matches these filters.");
        cut.Find(".state-panel-copy").TextContent.Should().Be("Trainee “Mahlangu”, status Revoked.");

        // T366, D10: one Clear filters, the filter bar's; the empty card says what was asked and offers no second one.
        cut.Find(".detail-card--empty").QuerySelectorAll("button").Should().BeEmpty();
        var clear = cut.FindAll("button").Where(button => button.TextContent.Trim() == "Clear filters").ToArray();
        clear.Should().ContainSingle();

        _sender.List = Five();
        clear[0].Click();

        cut.WaitForAssertion(() => cut.FindAll("tbody tr").Should().HaveCount(5));
        _sender.Lists.Last().TraineeFilter.Should().BeNull();
        _sender.Lists.Last().StatusFilter.Should().BeNull();
        cut.Find("#filter-trainee").GetAttribute("value").Should().BeEmpty();
        JSInterop.Invocations.Should().Contain(call => call.Identifier == FocusById && Equals(call.Arguments[0], "filter-trainee"),
            "Clear filters is gone once pressed; the Trainee field takes the focus");
    }

    [Fact]
    public void ARevokedRow_CarriesItsLine_WhenWhoAndWhy()
    {
        var revoked = Star(1, "Anele Dlamini", "PAED-002", reviewId: 2) with
        {
            Status = EntrustmentDecisionStatus.Revoked,
            RevokedOn = new DateTime(2026, 10, 13, 7, 12, 0, DateTimeKind.Utc),
            RevokedByName = "Refilwe Mokoena",
            RevocationReason = "Staged on evidence about PAED-001, not PAED-002; to be decided again."
        };
        var cut = Render([revoked]);

        var rows = cut.FindAll("tbody tr").ToArray();
        rows.Should().HaveCount(2);
        rows[0].ClassList.Should().Contain("has-note");
        rows[0].QuerySelectorAll(".actions-cell button").Select(button => button.TextContent.Trim()).Should().Equal("Download");
        var line = rows[1].QuerySelector("td.history-note")!;
        line.GetAttribute("colspan").Should().Be("7");
        line.TextContent.Should().Be(
            "Revoked 2026-10-13 at 09:12 SAST by Refilwe Mokoena: Staged on evidence about PAED-001, not PAED-002; to be decided again.");
    }

    [Fact]
    public void ASupersededRow_NamesItsSuccessor_AndTheDayItWasIssued()
    {
        var superseded = Star(2, "Lerato Molefe", "PAED-010", reviewId: 1) with
        {
            Status = EntrustmentDecisionStatus.Superseded,
            SupersededByDecisionId = 18,
            SupersededByIssuedOn = new DateOnly(2026, 10, 13)
        };
        var cut = Render([superseded]);

        cut.Find("td.history-note").TextContent.Should().Be("Superseded by STAR #18, issued 2026-10-13.");
    }

    [Fact]
    public async Task Revoke_OpensTheDialog_WithTheFocusInTheReason_AndItsButtonEnabled()
    {
        var cut = Render(Five());

        await RevokeButton(cut).ClickAsync(new());

        var dialog = cut.Find("dialog");
        cut.Find($"#{dialog.GetAttribute("aria-labelledby")}").TextContent.Should().Be("Revoke Anele Dlamini's PAED-002 STAR?");
        var body = cut.Find($"#{dialog.GetAttribute("aria-describedby")}").QuerySelectorAll("p").Select(p => p.TextContent).ToArray();
        body[0].Should().Be("PAED-002 — Managing common paediatric presentations, level 3b, issued 2026-10-13. Revocation is immediate and cannot be undone.");
        body[1].Should().Be("Anele Dlamini is sent an email saying the STAR was revoked and by whom, with a link to My authorisations, unless Anele Dlamini's account is deactivated. The reason is not in the email: Anele Dlamini reads it on My authorisations and on the certificate.");
        body[2].Should().Be("The EPA reads 'Revoked: re-decide' on Decisions due, and Anele Dlamini's review agenda keeps its line.");

        JSInterop.Invocations.Select(call => call.Identifier).Should().ContainInOrder("wombatDialog.showModal", FocusById);
        JSInterop.Invocations.Last(call => call.Identifier == FocusById).Arguments[0].Should().Be(DecisionsPage.ReasonId);
        cut.Find("#revoke-reason-count").TextContent.Should().Be("0 of 1,000 characters");
        var confirm = Confirm(cut);
        confirm.HasAttribute("disabled").Should().BeFalse("F3: always enabled");
        confirm.HasAttribute("aria-disabled").Should().BeFalse();
        confirm.ClassList.Should().Contain("btn-danger");
    }

    [Fact]
    public async Task TheDialogIsDescribedByItsSentences_TheReasonBoxOutsideThem()
    {
        // T366, D9: the box's label and counter are read once, as the box's, not again as the dialog's description.
        var cut = Render(Five());
        await RevokeButton(cut).ClickAsync(new());

        var description = cut.Find($"#{cut.Find("dialog").GetAttribute("aria-describedby")}");
        description.QuerySelectorAll("p").Should().HaveCount(3);
        description.QuerySelector("textarea").Should().BeNull();
        description.QuerySelector("label").Should().BeNull();
        cut.Find("dialog #revoke-reason").Should().NotBeNull();
    }

    [Fact]
    public async Task ABlankReason_IsRefusedUnderTheBox_WhichTakesTheFocus_AndNothingIsSent()
    {
        var cut = Render(Five());
        await RevokeButton(cut).ClickAsync(new());
        var focusCalls = FocusCalls();

        Confirm(cut).Click();

        cut.WaitForAssertion(() => cut.Find(".validation-message").TextContent.Should().Be("Give a reason for the revocation."));
        var box = cut.Find("#revoke-reason");
        box.GetAttribute("aria-describedby")!.Split(' ').Should().Contain(cut.Find(".validation-message").Id!);
        box.GetAttribute("aria-invalid").Should().Be("true");
        FocusCalls().Should().BeGreaterThan(focusCalls, "the box takes the focus");
        _sender.Revokes.Should().BeEmpty();
        JSInterop.Invocations.Should().NotContain(call => call.Identifier == "wombatDialog.close");
    }

    [Fact]
    public async Task TheBox_IsNeverCut_ItsCounterCountedInTheBrowser_AndOverTheLimitIsRefusedUnderIt()
    {
        // T366, D2 and R3: no maxlength (a long paste is never cut silently), no round trip a keystroke (the counter is
        // counted by the page's script module), and a reason over 1,000 characters refused under the box in Wombat's words.
        var module = JSInterop.SetupModule("./js/review-appeal.js");
        module.SetupVoid("count", _ => true).SetVoidResult();
        var cut = Render(Five());
        await RevokeButton(cut).ClickAsync(new());

        var box = cut.Find("#revoke-reason");
        box.HasAttribute("maxlength").Should().BeFalse();
        cut.WaitForAssertion(() => module.Invocations.Should().Contain(call => call.Identifier == "count"
            && call.Arguments.Last()!.Equals(RevokeEntrustmentDecisionCommand.MaxReasonLength)));

        box.Change(new string('a', RevokeEntrustmentDecisionCommand.MaxReasonLength + 1));
        var focusCalls = FocusCalls();
        Confirm(cut).Click();

        cut.WaitForAssertion(() => cut.Find(".validation-message").TextContent.Should().Be(RevokeEntrustmentDecisionCommand.ReasonTooLong));
        RevokeEntrustmentDecisionCommand.ReasonTooLong.Should().Be("Give the reason in 1,000 characters or fewer.");
        cut.Find("#revoke-reason").GetAttribute("value")!.Length.Should().Be(1001, "what was pasted is kept");
        FocusCalls().Should().BeGreaterThan(focusCalls, "the box takes the focus");
        _sender.Revokes.Should().BeEmpty();
    }

    [Fact]
    public async Task ARefusedRevoke_ShowsInTheDialog_WithTheReasonKept_AndTheListIsReadAgain()
    {
        var cut = Render(Five());
        _sender.RevokeFailure = new InvalidOperationException(EntrustmentRevocationRefusals.NotActive);
        await RevokeButton(cut).ClickAsync(new());
        cut.Find("#revoke-reason").Change("Staged on evidence about PAED-001, not PAED-002; to be decided again.");
        var reads = _sender.Lists.Count;

        Confirm(cut).Click();

        cut.WaitForAssertion(() => cut.Find("dialog .alert-danger").TextContent.Trim().Should().Be("Only active entrustment decisions can be revoked."));
        cut.Find("#revoke-reason").GetAttribute("value").Should().Be("Staged on evidence about PAED-001, not PAED-002; to be decided again.");
        cut.Find("#revoke-reason-count").TextContent.Should().Be("69 of 1,000 characters");
        JSInterop.Invocations.Should().NotContain(call => call.Identifier == "wombatDialog.close");
        _sender.Lists.Count.Should().Be(reads + 1);
    }

    [Fact]
    public async Task AnUnexpectedFailure_IsAFixedSentence_NeverTheExceptionsText()
    {
        var cut = Render(Five());
        _sender.RevokeFailure = new InvalidOperationException("A second operation was started on this context instance.");
        await RevokeButton(cut).ClickAsync(new());
        cut.Find("#revoke-reason").Change("Concern raised.");

        Confirm(cut).Click();

        cut.WaitForAssertion(() => cut.Find("dialog .alert-danger").TextContent.Trim().Should().Be(DecisionsPage.RevokeFailed));
        cut.Markup.Should().NotContain("second operation");
    }

    [Fact]
    public async Task ADoneRevoke_ClosesTheDialog_AndItsResultTakesTheFocus_ClaimingNoDelivery()
    {
        var cut = Render(Five());
        await RevokeButton(cut).ClickAsync(new());
        cut.Find("#revoke-reason").Change("Staged on evidence about PAED-001, not PAED-002; to be decided again.");

        Confirm(cut).Click();

        cut.WaitForAssertion(() => JSInterop.Invocations.Should().Contain(call => call.Identifier == "wombatDialog.close"));
        _sender.Revokes.Should().ContainSingle().Which.Reason.Should().Be("Staged on evidence about PAED-001, not PAED-002; to be decided again.");
        var result = cut.Find(".entrustment-list > .action-result .alert-success");
        result.TextContent.Trim().Should().Be("Anele Dlamini's PAED-002 STAR is revoked.");
        result.GetAttribute("role").Should().Be("status");
        cut.Markup.Should().NotContain("has been emailed");

        var focusCalls = FocusCalls();
        cut.Find("dialog").TriggerEvent("onclose", EventArgs.Empty);
        cut.WaitForAssertion(() => FocusCalls().Should().BeGreaterThan(focusCalls, "the result takes the focus once the dialog has closed"));
    }

    [Fact]
    public void WhileTheListIsRead_TheFormStands_WithSkeletons_AndNoActions()
    {
        _sender.Pending = new TaskCompletionSource<IReadOnlyList<EntrustmentDecisionDto>>();

        var cut = RenderComponent<DecisionsPage>();

        cut.FindAll("form.search-container").Should().ContainSingle();
        cut.FindAll(".skeleton").Should().NotBeEmpty();
        cut.Find("p[role=status]").TextContent.Should().Be("Loading the STARs.");
        cut.FindAll("tbody button").Should().BeEmpty();
    }

    [Fact]
    public void AFailedRead_IsFixedWords_WithTryAgain_NeverTheExceptionsText()
    {
        _sender.ListFailure = new TimeoutException("The database did not answer within 30 seconds.");

        var cut = RenderComponent<DecisionsPage>();

        cut.WaitForAssertion(() => cut.Find(".alert-danger").TextContent.Should().Contain(DecisionsPage.LoadFailed));
        cut.FindAll("button").Should().Contain(button => button.TextContent.Trim() == "Try again");
        cut.Markup.Should().NotContain("30 seconds");
    }

    [Fact]
    public void AFailedDownload_IsFixedWords_NeverTheExceptionsText()
    {
        var cut = Render(Five());
        _sender.DownloadFailure = new IOException("QuestPDF ran out of fonts.");

        cut.FindAll("tbody .actions-cell button").ToArray()[0].Click();

        cut.WaitForAssertion(() => cut.Find(".action-result .alert-danger").TextContent.Trim().Should().Be(StarWords.CertificateFailed));
        cut.Markup.Should().NotContain("QuestPDF");
    }

    private IRenderedComponent<DecisionsPage> Render(IReadOnlyList<EntrustmentDecisionDto> stars)
    {
        _sender.List = stars;
        var cut = RenderComponent<DecisionsPage>();
        cut.WaitForState(() => cut.FindAll(".skeleton").Count == 0);
        return cut;
    }

    private int FocusCalls()
        => JSInterop.Invocations.Count(call => call.Identifier is FocusById or "Blazor._internal.domWrapper.focus");

    private static AngleSharp.Dom.IElement RevokeButton(IRenderedComponent<DecisionsPage> cut)
        => cut.FindAll("tbody .actions-cell button").First(button => button.TextContent.Trim() == "Revoke");

    private static AngleSharp.Dom.IElement Confirm(IRenderedComponent<DecisionsPage> cut)
        => cut.FindAll("dialog .form-actions button").Single(button => button.TextContent.Trim() == "Revoke the STAR");

    /// <summary>Act 4's five, newest issued first then STAR number descending (4.35).</summary>
    private static IReadOnlyList<EntrustmentDecisionDto> Five() =>
    [
        Star(5, "Anele Dlamini", "PAED-002", reviewId: 2, title: "Managing common paediatric presentations", level: "3b"),
        Star(4, "Anele Dlamini", "PAED-001", reviewId: 2, level: "4"),
        Star(3, "Lerato Molefe", "PAED-012", reviewId: 1),
        Star(2, "Lerato Molefe", "PAED-010", reviewId: 1, level: "4") with { ExpiresOn = new DateOnly(2026, 11, 2) },
        Star(1, "Lerato Molefe", "PAED-001", reviewId: 1)
    ];

    private static EntrustmentDecisionDto Star(int id, string trainee, string code, int reviewId, string title = "An EPA", string level = "5")
        => new(
            id, $"user-{trainee}", id, code, title, true, 13, level, 5, new DateOnly(2026, 10, 13), null, reviewId, "chair-1",
            "Entrusted.", EntrustmentDecisionStatus.Active, null, null, null, null, [])
        {
            TraineeName = trainee,
            IssuingReviewReadable = true
        };

    private sealed class RegisterSender : IScopedSender
    {
        public IReadOnlyList<EntrustmentDecisionDto> List { get; set; } = [];

        public TaskCompletionSource<IReadOnlyList<EntrustmentDecisionDto>>? Pending { get; set; }

        public Exception? ListFailure { get; set; }

        public Exception? RevokeFailure { get; set; }

        public Exception? DownloadFailure { get; set; }

        public List<ListEntrustmentDecisionsForAdminQuery> Lists { get; } = [];

        public List<RevokeEntrustmentDecisionCommand> Revokes { get; } = [];

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case ListEntrustmentDecisionsForAdminQuery list:
                    Lists.Add(list);
                    if (ListFailure is not null)
                    {
                        throw ListFailure;
                    }

                    return (TResponse)(object)(Pending is null ? List : await Pending.Task);
                case RevokeEntrustmentDecisionCommand revoke:
                    if (RevokeFailure is not null)
                    {
                        throw RevokeFailure;
                    }

                    Revokes.Add(revoke);
                    return (TResponse)(object)List.Single(star => star.Id == revoke.DecisionId);
                case DownloadEntrustmentCertificateCommand:
                    if (DownloadFailure is not null)
                    {
                        throw DownloadFailure;
                    }

                    return (TResponse)(object)new EntrustmentCertificateResult([1, 2, 3], "star.pdf", "hash");
                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
