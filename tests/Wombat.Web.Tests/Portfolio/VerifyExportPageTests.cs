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
/// T379 (flow 13, R3-V01–V10): the answer above the form, one Alert with its first sentence bold and the details list in
/// it (F9, S20, S21), in SAST; what is not a hash refused under the field, named first by it, the field focused (S10, N4,
/// N15); the field's help and command list (Q6); no h3 and no inline style (T280).
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

    private const string Hash = "aba6ce7943c20681a22f7bbfd89a51b9e19ff005892e318806c28dd215a6c995";

    private const string Lerato = "2b5abc2f-9e37-4714-bb7a-8e48fa11f699";

    /// <summary>Step 5.13: the file exported three times, first at 22:51 SAST (20:51 UTC), last at 22:52.</summary>
    private static readonly PortfolioExportRecordDto ThreeTimes = new(
        Lerato, Lerato, new DateTime(2026, 10, 26, 20, 51, 0, DateTimeKind.Utc),
        new DateTime(2026, 10, 26, 20, 52, 0, DateTimeKind.Utc), 3,
        new DateOnly(2023, 1, 15), new DateOnly(2026, 10, 26), Hash, "portfolio-aba6ce7943c2.pdf");

    private static readonly VerifyExportAnswer Verified = new() { Export = ThreeTimes };

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

    /// <summary>V01: the form alone, its help and the command list, with nothing asked and nothing focused.</summary>
    [Fact]
    public void Blank_IsTheForm_WithTheFieldsHelpAndTheCommands_AndNothingElse()
    {
        var sender = new VerifySender(Verified);
        var cut = Render("/portfolio/verify", sender);

        cut.Find("h1").TextContent.Trim().Should().Be("Verify portfolio export");
        cut.Find(".page-subtitle").TextContent.Trim()
            .Should().Be("Check whether a PDF was produced by Wombat, without signing in.");

        var form = cut.Find(".verify-page > form.form-container");
        form.GetAttribute("method").Should().Be("get");
        form.GetAttribute("action").Should().Be("/portfolio/verify");
        form.QuerySelector(".form-group")!.Should().NotBeNull("A2: the form holds the group and the actions");

        var input = cut.Find("#hash");
        input.GetAttribute("name").Should().Be("hash");
        input.HasAttribute("required").Should().BeTrue("a browser sends no empty hash");
        input.GetAttribute("aria-describedby").Should().Be("hash-h hash-cmds");
        input.HasAttribute("aria-invalid").Should().BeFalse();
        input.HasAttribute("autofocus").Should().BeFalse();
        input.HasAttribute("placeholder").Should().BeFalse("the help says what to enter (Q6)");
        cut.Find("label[for=hash]").TextContent.Should().Contain("Content hash (SHA-256)");

        cut.Find("#hash-h").TextContent.Trim().Should().Be(
            "The file's SHA-256: 64 hexadecimal digits, computed on your computer from the PDF you were sent. Upper or " +
            "lower case, and spaces or line breaks in it, are accepted.");
        cut.FindAll("#hash-cmds li").Select(item => item.TextContent.Trim()).Should().Equal(
            "Windows: certutil -hashfile <file> SHA256, or in PowerShell Get-FileHash <file>",
            "macOS: shasum -a 256 <file>",
            "Linux: sha256sum <file>");
        cut.FindAll("#hash-cmds code").Should().HaveCount(4);

        var verify = cut.Find("form button[type=submit]");
        verify.TextContent.Trim().Should().Be("Verify");
        verify.GetAttribute("name").Should().Be(VerifyExport.PressedParameter);
        verify.GetAttribute("value").Should().Be("1");
        verify.ClassList.Should().Contain("btn-primary");
        cut.FindAll("[blazor\\:onclick], [blazor\\:onsubmit]").Should().BeEmpty(
            "nothing on the page may need a circuit: a visitor who has not signed in has none");

        sender.Checked.Should().BeEmpty("nothing was asked");
        cut.FindAll(".action-result, .alert, .validation-message").Should().BeEmpty("no answer, no refusal");
        FocusCalls().Should().BeEmpty();
        IdReferences.Broken(cut).Should().BeEmpty();
        NoInlineStyleNorH3(cut);
    }

    /// <summary>V02: the answer above the form, focused after a press; its first sentence bold; the details in SAST.</summary>
    [Fact]
    public void AfterAPressOfVerify_TheAnswerIsAboveTheForm_InSast_AndTakesTheFocusAsThePageLoads()
    {
        var sender = new VerifySender(Verified);
        var cut = Render($"/portfolio/verify?hash={Hash}&check=1", sender);

        sender.Checked.Should().Equal([Hash]);
        var page = cut.Find(".verify-page");
        page.Children[0].ClassList.Should().Contain("action-result", "the answer stands above the form (F9)");
        page.Children[1].TagName.Should().Be("FORM");

        var result = cut.Find(".action-result");
        result.QuerySelectorAll(".alert").Should().ContainSingle("one Alert holds the whole answer (S20)");
        var alert = result.QuerySelector(".alert.alert-success")!;
        alert.GetAttribute("role").Should().Be("status");
        alert.QuerySelector("p strong")!.TextContent.Should().Be("Export verified.");
        alert.QuerySelector("p")!.TextContent.Trim().Should().Be(
            "Export verified. Wombat generated this PDF: its content is byte for byte an export Wombat recorded.");

        Details(cut).Should().Equal(
            ("First generated", "2026-10-26 22:51 SAST"),
            ("Exported", "3 times, the last on 2026-10-26 22:52 SAST"),
            ("Trainee", Lerato),
            ("First exported by", Lerato),
            ("Filter", "2023-01-15 to 2026-10-26"),
            ("File", "portfolio-aba6ce7943c2.pdf"));
        cut.FindAll(".detail-list dd.hash-value").Should().HaveCount(2, "the two ids wrap in their cells");
        cut.Markup.Should().NotContain("UTC", "a time is SAST on a page (T325)");
        alert.QuerySelector(".verify-ids-note")!.TextContent.Trim().Should().Be(
            "People are shown by their Wombat user ids, never by name. The institution that issued the portfolio can say " +
            "whose they are.");

        cut.Find("#hash").GetAttribute("value").Should().Be(Hash, "the field keeps what was checked");
        cut.Find("#hash").HasAttribute("autofocus").Should().BeFalse("the answer takes the focus, not the field");
        result.HasAttribute("autofocus").Should().BeTrue("a static page takes the focus from its HTML");
        var region = cut.FindComponent<ActionResult>();
        FocusCalls().Should().ContainSingle().Which.Arguments[0].Should().BeOfType<ElementReference>()
            .Which.Id.Should().Be(region.Instance.Element.Id);
        IdReferences.Broken(cut).Should().BeEmpty();
        NoInlineStyleNorH3(cut);
    }

    /// <summary>V03, V09: the hash goes to the query as typed, upper case and spaces kept; the field shows it as typed.</summary>
    [Fact]
    public void AHashInUpperCase_OrSpaced_GoesToTheQueryAsTyped_AndTheFieldKeepsIt()
    {
        var spaced = "AB A6 CE 79 43 C2 06 81 A2 2F 7B BF D8 9A 51 B9 E1 9F F0 05 89 2E 31 88 06 C2 8D D2 15 A6 C9 95";
        var sender = new VerifySender(Verified);
        var cut = Render($"/portfolio/verify?hash={Uri.EscapeDataString(spaced)}&check=1", sender);

        sender.Checked.Should().Equal([spaced], "the query drops the spaces and the case itself (S10)");
        cut.Find("#hash").GetAttribute("value").Should().Be(spaced);
        cut.Find(".action-result .alert-success strong").TextContent.Should().Be("Export verified.");
    }

    /// <summary>V07: a file generated once.</summary>
    [Fact]
    public void AFileGeneratedOnce_SaysOnce()
    {
        var once = ThreeTimes with
        {
            FirstExportedOn = new DateTime(2026, 10, 26, 20, 56, 0, DateTimeKind.Utc),
            LastExportedOn = new DateTime(2026, 10, 26, 20, 56, 0, DateTimeKind.Utc),
            Count = 1,
            FileName = "portfolio-344dd52bdded.pdf"
        };
        var cut = Render($"/portfolio/verify?hash={Hash}&check=1", new VerifySender(new VerifyExportAnswer { Export = once }));

        Details(cut).Should().Contain([("First generated", "2026-10-26 22:56 SAST"), ("Exported", "Once")]);
    }

    /// <summary>An export with no filter has no Filter row.</summary>
    [Fact]
    public void AnExportWithNoFilter_HasNoFilterRow()
    {
        var unfiltered = ThreeTimes with { FilterFromDate = null, FilterToDate = null };
        var cut = Render($"/portfolio/verify?hash={Hash}&check=1", new VerifySender(new VerifyExportAnswer { Export = unfiltered }));

        Details(cut).Select(row => row.Term).Should().NotContain("Filter");
    }

    /// <summary>V10: a link that carries the hash is answered the same, and the focus is left on the page.</summary>
    [Fact]
    public void ALinkThatCarriesTheHash_ChecksIt_AndLeavesTheFocusAlone()
    {
        var sender = new VerifySender(Verified);
        var cut = Render($"/portfolio/verify?hash={Hash}", sender);

        sender.Checked.Should().Equal([Hash]);
        cut.Find(".action-result .alert-success strong").TextContent.Should().Be("Export verified.");
        cut.Find(".action-result").HasAttribute("autofocus").Should().BeFalse("the visitor arrived; nothing was pressed");
        FocusCalls().Should().BeEmpty();
    }

    /// <summary>V04: the built two sentences, the first bold, and the NEW third; a danger Alert that takes the focus.</summary>
    [Fact]
    public void AHashNoExportHas_IsSaidToMatchNothing_WithWhatToCheck()
    {
        var cut = Render($"/portfolio/verify?hash={Hash}&check=1", new VerifySender(new VerifyExportAnswer()));

        var alert = cut.Find(".action-result .alert.alert-danger");
        alert.GetAttribute("role").Should().Be("alert");
        alert.QuerySelector("strong")!.TextContent.Should().Be("No matching export found.");
        alert.TextContent.Trim().Should().Be(
            "No matching export found. This hash does not correspond to any PDF generated by this system. One changed byte " +
            "changes the whole hash: check that it was computed from the file you were sent.");
        cut.FindAll(".detail-list, .validation-message").Should().BeEmpty();
        FocusCalls().Should().ContainSingle();
    }

    /// <summary>V08: the fixed words, "Please" dropped, the first sentence bold; the cause only in the log.</summary>
    [Fact]
    public void ACheckThatFails_IsSaidOnThePage_AndNotWhy()
    {
        const string detail = "connection-refused-detail-for-the-log";
        var sender = new VerifySender(Verified) { Failure = new InvalidOperationException(detail) };

        var cut = Render($"/portfolio/verify?hash={Hash}&check=1", sender);

        var refusal = cut.Find(".action-result .alert.alert-danger");
        refusal.TextContent.Trim().Should().Be("The export could not be checked just now. Try again in a few minutes.");
        refusal.QuerySelector("strong")!.TextContent.Should().Be("The export could not be checked just now.");
        refusal.GetAttribute("role").Should().Be("alert");
        cut.Markup.Should().NotContain(detail, "the fault's message is for the log");
        cut.FindAll(".detail-list").Should().BeEmpty("no answer was given");
        FocusCalls().Should().ContainSingle("the failure arrives with the page, and the focus reads it");
        cut.Find("#hash").GetAttribute("value").Should().Be(Hash, "the visitor can try again with what was typed");
    }

    /// <summary>V05: only spaces, refused under the field before anything is asked; the field focused, the value kept.</summary>
    [Fact]
    public void OnlySpaces_AreRefusedUnderTheField_AndNothingIsChecked()
    {
        var sender = new VerifySender(Verified);
        var cut = Render("/portfolio/verify?hash=%20%20%20&check=1", sender);

        sender.Checked.Should().BeEmpty();
        AssertRefusedUnderTheField(cut, "Enter the content hash to check.", focused: true);
        cut.Find("#hash").GetAttribute("value").Should().Be("   ");
    }

    /// <summary>S10's three, worded from the query's reason (V06 the third); the field focused after a press (N4).</summary>
    [Theory]
    [InlineData(VerifyExportRefusal.FileNamePrefix, 12, null,
        "This is not a SHA-256 hash: it has 12 characters, and a SHA-256 hash has 64 hexadecimal digits (0 to 9, a to f). The 12 in a file's name are only its start.")]
    [InlineData(VerifyExportRefusal.WrongLength, 63, null,
        "This is not a SHA-256 hash: it has 63 characters, and a SHA-256 hash has 64 hexadecimal digits (0 to 9, a to f).")]
    [InlineData(VerifyExportRefusal.WrongLength, 1, null,
        "This is not a SHA-256 hash: it has 1 character, and a SHA-256 hash has 64 hexadecimal digits (0 to 9, a to f).")]
    [InlineData(VerifyExportRefusal.NotHexadecimal, 0, "O",
        "This is not a SHA-256 hash: “O” is not a hexadecimal digit (0 to 9, a to f).")]
    public void WhatIsNotAHash_IsRefusedUnderTheField_InTheQuerysReason(
        VerifyExportRefusal reason, int length, string? character, string words)
    {
        var answer = new VerifyExportAnswer { Refusal = reason, Length = length, Character = character };
        var cut = Render("/portfolio/verify?hash=aba6ce7943c2&check=1", new VerifySender(answer));

        AssertRefusedUnderTheField(cut, words, focused: true);
        cut.Find("#hash").GetAttribute("value").Should().Be("aba6ce7943c2", "what was typed is kept");
        cut.FindAll(".action-result").Should().BeEmpty("a refusal is the field's, not an answer");
        FocusCalls().Should().BeEmpty("autofocus puts the focus in the field; no answer region takes it");
    }

    /// <summary>A link carrying what is not a hash shows the refusal, and leaves the focus alone (V10's rule).</summary>
    [Fact]
    public void ALinkCarryingWhatIsNotAHash_ShowsTheRefusal_AndLeavesTheFocusAlone()
    {
        var answer = new VerifyExportAnswer { Refusal = VerifyExportRefusal.FileNamePrefix, Length = 12 };
        var cut = Render("/portfolio/verify?hash=aba6ce7943c2", new VerifySender(answer));

        AssertRefusedUnderTheField(cut, VerifyExportWords.Refusal(answer), focused: false);
    }

    private static void AssertRefusedUnderTheField(IRenderedComponent<VerifyExport> cut, string words, bool focused)
    {
        var input = cut.Find("#hash");
        var refusal = cut.Find("#hash-e");
        refusal.ClassList.Should().Contain("validation-message");
        refusal.TextContent.Trim().Should().Be(words);
        refusal.PreviousElementSibling!.Id.Should().Be("hash", "the refusal stands directly under the field");
        input.GetAttribute("aria-describedby").Should().Be("hash-e hash-h hash-cmds", "the refusal first (N15)");
        input.GetAttribute("aria-invalid").Should().Be("true");
        input.ClassList.Should().Contain("input-validation-error");
        input.HasAttribute("autofocus").Should().Be(focused, "after a press the field takes the focus (N4)");
        cut.FindAll(".alert").Should().BeEmpty();
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    private static List<(string Term, string Value)> Details(IRenderedComponent<VerifyExport> cut)
        => cut.FindAll(".action-result .detail-list dt")
            .Select(term => (term.TextContent.Trim(), term.NextElementSibling!.TextContent.Trim()))
            .ToList();

    private static void NoInlineStyleNorH3(IRenderedComponent<VerifyExport> cut)
    {
        cut.FindAll("[style]").Should().BeEmpty("T280: no inline style");
        cut.FindAll("h3").Should().BeEmpty("T280: no h3 under the h1");
    }

    private IRenderedComponent<VerifyExport> Render(string address, VerifySender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo(address);
        return RenderComponent<VerifyExport>();
    }

    private IReadOnlyList<JSRuntimeInvocation> FocusCalls()
        => JSInterop.Invocations.Where(invocation => invocation.Identifier == FocusIdentifier).ToList();

    /// <summary>Answers the export check with <paramref name="answer" />, or fails it.</summary>
    private sealed class VerifySender(VerifyExportAnswer answer) : IScopedSender
    {
        public List<string> Checked { get; } = [];

        public Exception? Failure { get; init; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var query = request.Should().BeOfType<VerifyExportQuery>().Subject;
            Checked.Add(query.ContentHash);
            return Failure is not null ? Task.FromException<TResponse>(Failure) : Task.FromResult((TResponse)(object)answer);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("The export check sends no command.");
    }
}
