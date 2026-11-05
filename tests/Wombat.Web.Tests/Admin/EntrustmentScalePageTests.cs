using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.Epas.Commands.CreateEntrustmentScale;
using Wombat.Application.Features.Epas.Commands.UpdateEntrustmentScale;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.EntrustmentScales;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T394 (flow 16, lane B): a scale's page as R3-C02–C05, C11–C16 and R2-C17–C19b draw it. Titled by the scale (T1), h2
/// Details and Levels, Order the row's header (S5). T395 and Q16: on a scale in use no Up, Down or Remove and Q16's line;
/// on one nothing uses, the moves. Q18's refusals in the result region: Q6's needs with "Put rung … back" (S14), which
/// re-inserts the rung in its place and focuses its Label; Q16's, after which the page is read again and nothing is put
/// back; the ended record's. Refusals under their fields (T324), the fixed failure (T393), "Entrustment scale created." by
/// the token (S13), and Unavailable (Q7).
/// </summary>
public sealed class EntrustmentScalePageTests : TestContext
{
    private const int CnsaId = 3;

    private static readonly string[] CnsaRungs =
        ["Observe only", "Direct supervision", "Indirect supervision", "Distant supervision", "Unsupervised practice", "Supervises others"];

    private static EntrustmentScaleDto Cnsa(bool inUse) =>
        new EntrustmentScaleDto(CnsaId, "CNSA Neurology Entrustment Scale", "Supervision ladder for CNSA EPAs.",
            CnsaRungs.Select((label, index) => new EntrustmentLevelDto(300 + index + 1, index + 1, label, null)).ToList())
        {
            InUse = inUse
        };

    public EntrustmentScalePageTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void AScaleInUse_IsTitledByItsName_WithNoMoves_AndQ16sLineAboveAddLevel()
    {
        // R3-C02 (T395): every 11.1 item is pinned to the CPSA ladder, so none of its rungs moves or goes.
        var cut = Render(new Sender { Scale = Cnsa(inUse: true) }, CnsaId);

        Text(cut.Find("h1")).Should().Be("CNSA Neurology Entrustment Scale");
        Text(cut.Find(".page-subtitle")).Should().Be("A ladder shared by every College and institution.");
        cut.Find(".user-page.scale-page").Should().NotBeNull();
        cut.FindAll("h2").Select(Text).Should().Equal("Details", "Levels");
        cut.FindAll("a").Should().NotContain(link => Text(link) == "Back to scales");
        Text(cut.Find("caption")).Should().Be("Levels of CNSA Neurology Entrustment Scale, lowest first");
        cut.FindAll("tbody th[scope='row']").Select(Text).Should().Equal("1", "2", "3", "4", "5", "6");
        cut.FindAll("thead th").Should().HaveCount(3, "no row offers an action, so no Actions column (T239)");
        cut.FindAll("tbody button").Should().BeEmpty("no Up, Down or Remove on a scale in use (Q16)");
        Text(cut.Find(".scale-in-use-line")).Should().Be("Rungs can be reordered or removed only while nothing uses this scale.");
        cut.FindAll("textarea").Should().HaveCount(7, "the description and each rung's descriptor are textareas (S4)");
        cut.FindAll("button").Where(button => button.Closest("dialog") is null).Select(Text)
            .Should().Equal("Add level", "Save");
    }

    [Fact]
    public void AScaleNothingUses_OffersTheMoves_UpOnTheFirstAndDownOnTheLastDisabled()
    {
        var cut = Render(new Sender { Scale = Cnsa(inUse: false) }, CnsaId);

        cut.FindAll(".scale-in-use-line").Should().BeEmpty();
        var first = cut.FindAll("tbody tr").First().QuerySelectorAll("button").ToList();
        first.Select(button => button.GetAttribute("aria-label")).Should().Equal(
            "Move Observe only up", "Move Observe only down", "Remove Observe only");
        first[0].HasAttribute("disabled").Should().BeTrue();
        first[2].ClassList.Should().Contain("btn-outline").And.NotContain("btn-danger", "Q11: Remove asks nothing");
        cut.FindAll("tbody tr").Last().QuerySelectorAll("button")[1].HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void ArrivingByCreated_WritesEntrustmentScaleCreated_FocusesIt_AndDropsTheToken()
    {
        Services.AddSingleton<IScopedSender>(new Sender { Scale = Cnsa(inUse: false) });
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("/admin/entrustment-scales/3?created");
        var cut = RenderComponent<EntrustmentScaleEdit>(parameters => parameters.Add(page => page.Id, CnsaId));

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-success")).Should().Be("Entrustment scale created."));
        cut.WaitForAssertion(() => navigation.Uri.Should().EndWith("/admin/entrustment-scales/3"));
        JSInterop.Invocations.Should().Contain(call => call.Identifier == "Blazor._internal.domWrapper.focus");
    }

    [Fact]
    public void Create_StartsWithTwoEmptyRungs_AndLeavesForItsPageByTheCreatedToken()
    {
        var sender = new Sender { Created = Cnsa(inUse: false) };
        var cut = Render(sender, null);

        Text(cut.Find("h1")).Should().Be("Create entrustment scale");
        Text(cut.Find(".page-subtitle")).Should().Be("A ladder of at least two rungs, shared by every College and institution.");
        Text(cut.Find("caption")).Should().Be("Levels of the new scale, lowest first");
        Text(cut.Find("form button[type=submit]")).Should().Be("Create scale");

        cut.Find("#scale-name").Change("CNSA Neurology Entrustment Scale");
        cut.FindAll("tbody input").ElementAt(0).Change("Observe only");
        cut.FindAll("tbody input").ElementAt(1).Change("Direct supervision");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith("/admin/entrustment-scales/3?created"));
        sender.Sent.OfType<CreateEntrustmentScaleCommand>().Single().Levels.Select(level => level.Label)
            .Should().Equal("Observe only", "Direct supervision");
    }

    [Fact]
    public void TheFormsOwnChecks_PutEachRefusalUnderItsField_InItsWords_AndSendNothing()
    {
        // R3-C15a, C15b, C03b: an empty name, an empty label, a repeated label under the later rung.
        var sender = new Sender { Scale = Cnsa(inUse: false) };
        var cut = Render(sender, CnsaId);

        cut.Find("#scale-name").Change("");
        cut.FindAll("tbody input").ElementAt(1).Change("");
        cut.FindAll("tbody input").ElementAt(3).Change("Indirect supervision");
        cut.Find("form").Submit();

        Text(cut.Find("#scale-name-refusal")).Should().Be("Enter a name.");
        var rows = cut.FindAll("tbody tr").ToList();
        Text(rows[1].QuerySelector(".validation-message")!).Should().Be("Enter this rung's label.");
        Text(rows[3].QuerySelector(".validation-message")!).Should().Be(
            "“Indirect supervision” is already the label of another rung. Give each rung its own label.");
        rows[2].QuerySelector(".validation-message").Should().BeNull("the first rung with the label keeps it");
        var refusedLabel = rows[3].QuerySelector("input")!;
        refusedLabel.GetAttribute("aria-invalid").Should().Be("true");
        refusedLabel.GetAttribute("aria-describedby").Should().Be(rows[3].QuerySelector(".validation-message")!.Id);
        JSInterop.Invocations.Should().Contain(call => call.Identifier == "wombat.focusById" && call.Arguments.Contains("scale-name"));
        sender.Sent.Should().BeEmpty();
    }

    [Fact]
    public void OneRungLeft_IsRefusedAtTheLevelsSection()
    {
        var cut = Render(new Sender(), null);

        cut.Find("#scale-name").Change("CNSA Neurology Entrustment Scale");
        cut.FindAll("tbody input").ElementAt(0).Change("Observe only");
        cut.FindAll("tbody button").Single(button => button.GetAttribute("aria-label") == "Remove the rung in order 2").Click();
        cut.Find("form").Submit();

        Text(cut.Find("#scale-levels-refusal")).Should().Be("A scale needs at least two rungs. Add another.");
        JSInterop.Invocations.Should().Contain(call => call.Identifier == "wombat.focusById" && call.Arguments.Contains("scale-levels-refusal"));
    }

    [Fact]
    public void ATakenName_IsUnderName_InTheBuiltWords()
    {
        var sender = new Sender
        {
            Scale = Cnsa(inUse: false),
            UpdateFailure = new FieldRefusalException("Name", "An entrustment scale named 'CPSA Paediatric Entrustment Scale v11.1' already exists.")
        };
        var cut = Render(sender, CnsaId);

        cut.Find("#scale-name").Change("CPSA Paediatric Entrustment Scale v11.1");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find("#scale-name-refusal")).Should().Be(
            "An entrustment scale named 'CPSA Paediatric Entrustment Scale v11.1' already exists."));
        cut.FindAll(".action-result .alert").Should().BeEmpty();
    }

    [Fact]
    public void AValidatorsRefusalOfOneRung_IsUnderThatRungsLabel()
    {
        var sender = new Sender
        {
            Scale = Cnsa(inUse: false),
            UpdateFailure = new ValidationException([new ValidationFailure("Levels[4].Label", "A label is at most 200 characters.")])
        };
        var cut = Render(sender, CnsaId);

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.FindAll("tbody tr").ElementAt(4).QuerySelector(".validation-message")!)
            .Should().Be("A label is at most 200 characters."));
    }

    [Fact]
    public void Q6sRefusal_NamesEachNeed_AndPutBackReinsertsTheRungInItsPlace_FocusingItsLabel()
    {
        // R3-C11 (S14): rung 6 removed and Save pressed after another save pinned items to it.
        var needs = Enumerable.Range(1, 3)
            .Select(index => new EntrustmentScaleRungNeed($"NEU-00{index}", "Neurology EPA Curriculum", "1.0", 306, "Supervises others"))
            .ToList();
        var sender = new Sender
        {
            Scale = Cnsa(inUse: false),
            UpdateFailure = new EntrustmentScaleUpdateRefusedException(
                EntrustmentScaleUpdateRefusal.RungNeeded, needs, 2, [306], "for the audit row")
        };
        var cut = Render(sender, CnsaId);

        cut.FindAll("tbody button").Single(button => button.GetAttribute("aria-label") == "Remove Supervises others").Click();
        cut.FindAll("tbody tr").Should().HaveCount(5);
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => cut.Find(".action-result .alert-danger"));
        var alert = cut.Find(".action-result .alert-danger");
        Text(alert.QuerySelector("p")!).Should().Be("Not saved.");
        var lines = alert.QuerySelectorAll("li").Select(Text).ToList();
        lines.Should().HaveCount(3);
        lines[0].Should().Be(
            "NEU-001 on Neurology EPA Curriculum (1.0) needs the rung “Supervises others” of this scale, which the new levels leave out. Change that item's minimum first.");
        lines[2].Should().EndWith("Change that item's minimum first. and 2 more.");
        cut.Markup.Should().NotContain("for the audit row", "the page words the typed refusal, never its message");
        cut.FindAll("tbody tr").Should().HaveCount(5, "the rungs as the user left them");

        var putBack = alert.QuerySelector("button")!;
        Text(putBack).Should().Be("Put rung “Supervises others” back");
        putBack.Click();

        var rows = cut.FindAll("tbody tr").ToList();
        rows.Should().HaveCount(6);
        rows[5].QuerySelector("input")!.GetAttribute("value").Should().Be("Supervises others");
        Text(rows[5].QuerySelector("th")!).Should().Be("6");
        cut.Find(".action-result .alert-danger").QuerySelectorAll("button").Should().BeEmpty("it has been put back");
        JSInterop.Invocations.Should().Contain(call => call.Identifier == "wombat.focusById"
                                                       && call.Arguments.Contains(rows[5].QuerySelector("input")!.Id));

        cut.Find("form").Submit();
        sender.Sent.OfType<UpdateEntrustmentScaleCommand>().Last().Levels.Last().Id.Should().Be(306, "the rung keeps its id: not a delete and an add");
    }

    [Fact]
    public void Q16sRefusal_ReadsThePageAgain_TheMovesGone_AndNothingToPutBack()
    {
        // R3-C12: the scale came into use after the page was read.
        var sender = new Sender
        {
            Scale = Cnsa(inUse: false),
            ScaleAfterRefusal = Cnsa(inUse: true),
            UpdateFailure = new EntrustmentScaleUpdateRefusedException(
                EntrustmentScaleUpdateRefusal.InUse, [], 0, [], EntrustmentScaleRefusalWords.InUse)
        };
        var cut = Render(sender, CnsaId);

        cut.FindAll("tbody button").Single(button => button.GetAttribute("aria-label") == "Move Direct supervision up").Click();
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-danger")).Should().Be(
            "Not saved. Rungs can be reordered or removed only while nothing uses this scale, and this scale is now in use. The page has been read again."));
        cut.Find(".action-result .alert-danger").QuerySelectorAll("button").Should().BeEmpty("no Put … back after Q16 (S14)");
        cut.FindAll("tbody button").Should().BeEmpty();
        cut.FindAll("tbody input").Select(input => input.GetAttribute("value")).Should().Equal(CnsaRungs, "read again, in the stored order");
        Text(cut.Find(".scale-in-use-line")).Should().Be("Rungs can be reordered or removed only while nothing uses this scale.");
    }

    [Fact]
    public void TheEndedRecordsRefusal_IsItsSentence()
    {
        var need = new EntrustmentScaleRungNeed("NEU-001", "Neurology EPA Curriculum", "1.0", 306, "Supervises others");
        var sender = new Sender
        {
            Scale = Cnsa(inUse: false),
            UpdateFailure = new EntrustmentScaleUpdateRefusedException(
                EntrustmentScaleUpdateRefusal.RungOnAnEndedRecord, [need], 0, [], "audit")
        };
        var cut = Render(sender, CnsaId);

        cut.FindAll("tbody button").Single(button => button.GetAttribute("aria-label") == "Remove Supervises others").Click();
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-danger")).Should().Be(
            "Not saved. NEU-001 on Neurology EPA Curriculum (1.0) was on the record of a programme that has ended, which keeps the rung “Supervises others” of this scale as its minimum. A closed record keeps the targets it ended with, so the rung “Supervises others” cannot be removed."));
    }

    [Fact]
    public void AnyOtherFailure_IsTheFixedSentence_NeverTheExceptionsText()
    {
        var sender = new Sender { Scale = Cnsa(inUse: false), UpdateFailure = new InvalidOperationException("Level 9 was not found on scale 3.") };
        var cut = Render(sender, CnsaId);

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-danger")).Should().Be(
            "The entrustment scale could not be saved. Nothing has changed. Try again, or come back in a few minutes."));
        cut.Markup.Should().NotContain("not found on scale");
    }

    [Fact]
    public void AnUnknownId_IsEntrustmentScaleUnavailable()
    {
        var cut = Render(new Sender { Scale = null }, 99);

        Text(cut.Find("h1")).Should().Be("Entrustment scale unavailable");
        Text(cut.Find(".state-panel-copy")).Should().Be("This entrustment scale does not exist, or you cannot open it.");
        var go = cut.Find(".detail-card--empty a");
        Text(go).Should().Be("Go to Entrustment scales");
        go.GetAttribute("href").Should().Be("/admin/entrustment-scales");
    }

    [Fact]
    public void AFailedRead_IsTitledEntrustmentScale_WithFixedWords()
    {
        var cut = Render(new Sender { ReadFailure = new TimeoutException("10.0.0.4") }, CnsaId);

        Text(cut.Find("h1")).Should().Be("Entrustment scale");
        Text(cut.Find(".alert-row-text")).Should().Be(
            "Could not load this entrustment scale. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("10.0.0.4");
    }

    private IRenderedComponent<EntrustmentScaleEdit> Render(Sender sender, int? id)
    {
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<EntrustmentScaleEdit>(parameters => parameters.Add(page => page.Id, id));
        cut.WaitForState(() => cut.FindAll(".loading-line").Count == 0);
        return cut;
    }

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class Sender : IScopedSender
    {
        private bool _refused;

        public EntrustmentScaleDto? Scale { get; init; }
        public EntrustmentScaleDto? ScaleAfterRefusal { get; init; }
        public EntrustmentScaleDto? Created { get; init; }
        public Exception? UpdateFailure { get; set; }
        public Exception? ReadFailure { get; init; }
        public List<object> Sent { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case GetEntrustmentScaleByIdQuery:
                    if (ReadFailure is not null)
                    {
                        return Task.FromException<TResponse>(ReadFailure);
                    }

                    return Task.FromResult((TResponse)(object)(_refused && ScaleAfterRefusal is not null ? ScaleAfterRefusal : Scale)!);
                case CreateEntrustmentScaleCommand:
                    Sent.Add(request);
                    return Task.FromResult((TResponse)(object)(Created ?? throw new NotSupportedException("No create expected.")));
                case UpdateEntrustmentScaleCommand:
                    Sent.Add(request);
                    if (UpdateFailure is { } failure)
                    {
                        // Refused once; a later save goes through.
                        UpdateFailure = null;
                        _refused = true;
                        return Task.FromException<TResponse>(failure);
                    }

                    return Task.FromResult((TResponse)(object)new UpdateEntrustmentScaleResult(Scale!, null));
                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
