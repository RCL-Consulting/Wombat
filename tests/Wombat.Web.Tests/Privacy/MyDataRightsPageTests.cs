using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.DataRights;
using Wombat.Application.Features.DataRights.Commands;
using Wombat.Application.Features.DataRights.Queries;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Profile;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;

namespace Wombat.Web.Tests.Privacy;

/// <summary>
/// My data rights (T387, flow 14, lane M; boards R3-M01 to M18): the three sections in order, the answer on each row and
/// never its decider (Q5, T319), the described radios (Q4, Q8, Q20), the refusals under their fields with the focus and the
/// values kept (M02), one result at a time (M13), the unchanged save's words (N3), SAST (Q9), the institution sentence and
/// M18's, the row actions' names (N4, N12), and the load states (M14, M15; T329).
/// </summary>
public sealed class MyDataRightsPageTests : TestContext
{
    private const string Kgk = "Kgosi Kgari Teaching Hospital";
    private const string FocusElement = "Blazor._internal.domWrapper.focus";

    // 2026-10-27 22:01 UTC is 2026-10-28 00:01 in South Africa (A.1.2's request; Q9: the day changes too).
    private static readonly DateTime Made = new(2026, 10, 27, 22, 1, 0, DateTimeKind.Utc);
    private static readonly DateTime DecidedUtc = new(2026, 10, 27, 22, 10, 0, DateTimeKind.Utc);

    private readonly ScriptedSender _sender = new();

    public MyDataRightsPageTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("dlamini@kgk.wombat.local");
        auth.SetRoles(WombatRoles.Trainee);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "trainee-1"));

        Services.AddSingleton<IScopedSender>(_sender);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    // ---- the page (M01) ----

    [Fact]
    public void ThePage_IsThreeSectionsInOrder_UnderItsHeader()
    {
        var cut = RenderComponent<DataRights>();

        cut.Find("h1").TextContent.Trim().Should().Be("My data rights");
        Text(cut.Find(".page-subtitle")).Should().Be(
            "Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.");
        var page = cut.Find("div.user-page.data-rights-page");
        page.QuerySelectorAll("section.detail-card.user-section > h2").Select(Text)
            .Should().Equal("Your requests", "Make a request", "Reminder emails");
        cut.FindAll("h2[tabindex]").Should().BeEmpty("each result focuses its own region (N18)");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void WithNoRequests_YourRequestsSaysSo_AndDrawsNoTable()
    {
        var cut = RenderComponent<DataRights>();

        var empty = Section(cut, "requests").QuerySelector(".detail-card--empty")!;
        Text(empty.QuerySelector(".state-panel-title")!).Should().Be("No requests yet.");
        Text(empty.QuerySelector(".state-panel-copy")!).Should()
            .Be("You have not asked for your data, a correction or erasure. Make a request below.");
        cut.FindAll("table").Should().BeEmpty();
    }

    [Fact]
    public void WhileItReads_ThePageSaysSo_OverStillCards()
    {
        _sender.HoldLoad = new TaskCompletionSource();
        var cut = RenderComponent<DataRights>();

        var loading = cut.Find("p.loading-line");
        loading.GetAttribute("role").Should().Be("status");
        Text(loading).Should().Be("Loading your data rights.");
        cut.FindAll("div.detail-card[aria-hidden='true']").Should().HaveCount(3);
        cut.FindAll("section").Should().BeEmpty();
    }

    /// <summary>M15, T329: a failed read is fixed words with Try again, never the exception's text.</summary>
    [Fact]
    public void AFailedRead_IsFixedWordsWithTryAgain_AndTryAgainThatReadsFocusesTheHeading()
    {
        _sender.FailLoads = 1;
        var cut = RenderComponent<DataRights>();

        var alert = cut.Find(".alert.alert-danger");
        alert.GetAttribute("role").Should().Be("alert");
        Text(alert).Should().Be(
            "Could not load your data rights. Nothing has changed. Try again, or come back in a few minutes. Try again");
        cut.Markup.Should().NotContain(ScriptedSender.Boom);

        cut.FindAll("button").Single(button => Text(button) == "Try again").Click();

        cut.WaitForAssertion(() => cut.FindAll("section").Should().HaveCount(3));
        cut.WaitForAssertion(() => JSInterop.Invocations.Select(call => call.Identifier)
            .Should().Contain(PageFocus.FocusHeadingIdentifier));
    }

    // ---- Your requests (M03–M10, M17) ----

    [Fact]
    public void ARow_ReadsInSast_ByLabel_WithItsAnswer_AsTheBoardsDrawIt()
    {
        _sender.Requests = [Request(DataRightsRequestType.Export, DataRightsRequestStatus.Submitted)];
        var cut = RenderComponent<DataRights>();

        var table = cut.Find("table");
        table.ClassList.Should().Contain(["clinic-table--stack", "clinic-table--stack-wide", "data-rights-table"]);
        Text(table.QuerySelector("caption")!).Should().Be("Your data rights requests, newest first");
        table.QuerySelectorAll("thead th").Select(Text).Should().Equal("Submitted", "Type", "Status", "Answer", "Actions");

        var row = table.QuerySelector("tbody tr")!;
        var header = row.QuerySelector("th")!;
        header.GetAttribute("scope").Should().Be("row");
        Text(header).Should().Be("2026-10-28 00:01 SAST");
        row.QuerySelectorAll("td").Select(cell => cell.GetAttribute("data-label"))
            .Should().Equal("Type", "Status", "Answer", null);
        Text(row.QuerySelector("td[data-label='Type']")!).Should().Be("Export");
        var badge = row.QuerySelector("td[data-label='Status'] .badge")!;
        Text(badge).Should().Be("Submitted");
        badge.ClassList.Should().Contain(BadgeFor.DataRightsRequest(DataRightsRequestStatus.Submitted));
        var answer = row.QuerySelector("td.answer-cell")!;
        answer.GetAttribute("data-label").Should().Be("Answer");
        Text(answer).Should().Be("Waiting for a decision.");
    }

    public static TheoryData<DataRightsRequestType, DataRightsRequestStatus, string?, string[], string> Answers() => new()
    {
        { DataRightsRequestType.Export, DataRightsRequestStatus.Submitted, null, [], "Waiting for a decision." },
        { DataRightsRequestType.Access, DataRightsRequestStatus.Withdrawn, null, [], "You withdrew it before a decision." },
        // M04: the note ends its own sentence, so no second full stop.
        {
            DataRightsRequestType.Export, DataRightsRequestStatus.Completed,
            "Identity confirmed; export released to the data subject.", [],
            "Approved on 2026-10-28: “Identity confirmed; export released to the data subject.”"
        },
        // M08: the approved Rectification, being corrected.
        {
            DataRightsRequestType.Rectification, DataRightsRequestStatus.Approved, "Confirmed with the registry", [],
            "Approved on 2026-10-28: “Confirmed with the registry”. Being corrected; it reads Completed once done."
        },
        {
            DataRightsRequestType.Rectification, DataRightsRequestStatus.Rejected,
            "The whole January 2026 intake is admitted on 15 January, the semester boundary (D42). Nothing to correct.", [],
            "Rejected on 2026-10-28: “The whole January 2026 intake is admitted on 15 January, the semester boundary " +
            "(D42). Nothing to correct.”"
        },
        // M09: what was corrected, by record.
        {
            DataRightsRequestType.Rectification, DataRightsRequestStatus.Completed, "Confirmed.",
            [DataRightsRecords.AccountLastName], "Completed on 2026-10-29. Corrected: Account › Last name."
        },
        {
            DataRightsRequestType.Rectification, DataRightsRequestStatus.Completed, "Confirmed.",
            [DataRightsRecords.AccountFirstName, DataRightsRecords.Other("Home address")],
            "Completed on 2026-10-29. Corrected: Account › First name, Other › Home address."
        }
    };

    [Theory]
    [MemberData(nameof(Answers))]
    public void TheAnswer_SaysTheDecisionItsDayAndItsNote(
        DataRightsRequestType type, DataRightsRequestStatus status, string? note, string[] records, string expected)
    {
        var request = Request(type, status) with
        {
            DecidedOn = status is DataRightsRequestStatus.Submitted or DataRightsRequestStatus.Withdrawn ? null : DecidedUtc,
            DecisionNote = note,
            // 2026-10-28 23:30 UTC is 2026-10-29 in South Africa.
            CompletedOn = status == DataRightsRequestStatus.Completed ? new DateTime(2026, 10, 28, 23, 30, 0, DateTimeKind.Utc) : null,
            CorrectedRecords = records
        };
        _sender.Requests = [request];
        var cut = RenderComponent<DataRights>();

        Text(cut.Find("td.answer-cell")).Should().Be(expected);
        Text(cut.Find("td[data-label='Status'] .badge")).Should().Be(DataRightsLabels.Status(status));
    }

    /// <summary>Q5: the requester reads the decision, never who decided; the DTO carries no decider to show.</summary>
    [Fact]
    public void TheAnswer_NeverNamesWhoDecided()
    {
        typeof(MyDataRightsRequestDto).GetProperties().Select(property => property.Name)
            .Should().NotContain(name => name.Contains("DecidedBy", StringComparison.Ordinal));
        _sender.Requests = [Request(DataRightsRequestType.Export, DataRightsRequestStatus.Rejected) with
        {
            DecidedOn = DecidedUtc,
            DecisionNote = "No."
        }];
        var cut = RenderComponent<DataRights>();

        cut.Find("table").TextContent.Should().NotContain("by ");
    }

    [Fact]
    public void Withdraw_IsOfferedOnlyWhileSubmitted_AndDownload_OnlyOnACompletedAccessOrExport()
    {
        var submitted = Request(DataRightsRequestType.Erasure, DataRightsRequestStatus.Submitted);
        _sender.Requests =
        [
            submitted,
            Request(DataRightsRequestType.Rectification, DataRightsRequestStatus.Approved, 2, Made.AddMinutes(-10)) with { DecidedOn = DecidedUtc },
            Request(DataRightsRequestType.Export, DataRightsRequestStatus.Completed, 3, Made.AddMinutes(-20)) with { DecidedOn = DecidedUtc },
            Request(DataRightsRequestType.Access, DataRightsRequestStatus.Completed, 4, Made.AddMinutes(-30)) with { DecidedOn = DecidedUtc },
            Request(DataRightsRequestType.Access, DataRightsRequestStatus.Withdrawn, 5, Made.AddMinutes(-40))
        ];
        var cut = RenderComponent<DataRights>();

        var actions = cut.FindAll("tbody tr").Select(row => row.QuerySelectorAll(".actions-cell .btn")
            .Select(control => AccessibleNames.NameOf(cut, control)).ToList()).ToList();
        actions.Should().HaveCount(5);
        actions[0].Should().Equal("Withdraw the Erasure request made 2026-10-28 00:01 SAST");
        actions[1].Should().BeEmpty("an approved request is in hand: Withdraw is gone (Q14)");
        // N12: each Download names what it downloads, starting with the visible word.
        actions[2].Should().Equal("Download the export of the Export request made 2026-10-27 23:41 SAST");
        actions[3].Should().Equal("Download the copy of your data for the Access request made 2026-10-27 23:31 SAST");
        actions[4].Should().BeEmpty();

        var download = cut.FindAll("tbody a.btn").First();
        download.GetAttribute("href").Should().Be(DataRightsWords.DownloadLink(new Guid("6a1f0000-0000-0000-0000-000000000003")));
        Text(download).Should().Be("Download");
    }

    /// <summary>N4: two requests of one type in one minute are named to the second, so their Withdraws differ.</summary>
    [Fact]
    public void TwoRequestsOfOneTypeInOneMinute_AreNamedToTheSecond()
    {
        _sender.Requests =
        [
            Request(DataRightsRequestType.Export, DataRightsRequestStatus.Submitted, 1, Made.AddSeconds(41)),
            Request(DataRightsRequestType.Export, DataRightsRequestStatus.Submitted, 2, Made.AddSeconds(7)),
            Request(DataRightsRequestType.Access, DataRightsRequestStatus.Submitted, 3, Made.AddSeconds(3))
        ];
        var cut = RenderComponent<DataRights>();

        cut.FindAll("tbody th").Select(Text).Should().Equal(
            "2026-10-28 00:01 SAST", "2026-10-28 00:01 SAST", "2026-10-28 00:01 SAST");
        cut.FindAll("tbody button").Select(button => AccessibleNames.NameOf(cut, button)).Should().Equal(
            "Withdraw the Export request made 2026-10-28 00:01:41 SAST",
            "Withdraw the Export request made 2026-10-28 00:01:07 SAST",
            "Withdraw the Access request made 2026-10-28 00:01 SAST");
    }

    // ---- Make a request (M01, M02, M16, M17, M18) ----

    [Fact]
    public void TheType_IsFourDescribedRadios_NoneChosen_AndNeverObjection()
    {
        var cut = RenderComponent<DataRights>();

        var group = cut.Find("fieldset");
        group.ClassList.Should().Contain(["form-group", "choice-group", "choice-help-stack"]);
        Text(group.QuerySelector("legend")!).Should().Be("Request type * required");
        group.QuerySelector("legend .required-mark")!.GetAttribute("aria-hidden").Should().Be("true");
        group.QuerySelector("legend .visually-hidden")!.TextContent.Should().Be(" required");

        var radios = group.QuerySelectorAll("input[type='radio']");
        radios.Select(radio => AccessibleNames.NameOf(cut, radio)).Should().Equal("Access", "Export", "Rectification", "Erasure");
        radios.Should().OnlyContain(radio => !radio.HasAttribute("checked"), "the boards draw none chosen (M01)");
        radios.Select(radio => Text(cut.Find($"#{radio.GetAttribute("aria-describedby")}"))).Should().Equal(
            "Everything Wombat holds about you, as a download, once the request is approved.",
            "The same download, to take your data elsewhere: machine-readable data with a PDF summary.",
            "Something about you is recorded wrongly. Say what it is and what it should be.",
            "Your account and personal data are removed; your training records are kept under a name that identifies " +
            "nobody. Once approved it cannot be undone, and you can no longer sign in.");
        cut.Markup.Should().NotContain("Objection");
        cut.FindAll("[required]").Should().BeEmpty("the page validates on press (N16)");
    }

    [Fact]
    public void TheReason_IsRequired_WithItsCounter_AndNoMaxlength()
    {
        var cut = RenderComponent<DataRights>();

        var reason = cut.Find($"#{DataRights.ReasonId}");
        AccessibleNames.NameOf(cut, reason).Should().Be("Reason required");
        reason.GetAttribute("aria-required").Should().Be("true");
        reason.HasAttribute("maxlength").Should().BeFalse("a paste is never cut silently (DESIGN.md § Form system)");
        reason.GetAttribute("aria-describedby").Should().Be(DataRights.ReasonCountId);
        Text(cut.Find($"#{DataRights.ReasonCountId}")).Should().Be("0 of 4,000 characters");

        reason.Change("A copy of my training record for my own files.");
        Text(cut.Find($"#{DataRights.ReasonCountId}")).Should().Be("46 of 4,000 characters");
    }

    [Fact]
    public void TheIntroAndTheNextStep_NameTheInstitutionThatDecides()
    {
        var cut = RenderComponent<DataRights>();

        var make = Section(cut, "make");
        make.QuerySelectorAll("p").Select(Text).Should().Contain(
            "Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. " +
            "Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests.");
        make.QuerySelectorAll("p.field-help").Select(Text).Should().Contain(
            "What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and " +
            "you are emailed when it is decided. While it waits you can withdraw it under Your requests.");
    }

    /// <summary>M18: a requester with no institution is decided by the Administrator.</summary>
    [Fact]
    public void WithNoInstitution_TheAdministratorDecides()
    {
        _sender.InstitutionName = null;
        var cut = RenderComponent<DataRights>();

        var make = Section(cut, "make");
        make.QuerySelectorAll("p").Select(Text).Should().Contain(
            "Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. " +
            "The Wombat administrator decides it; the answer shows under Your requests.");
        make.QuerySelectorAll("p.field-help").Select(Text).Should().Contain(
            "What happens next: the Wombat administrator reads it and approves or rejects it with a note, and you are " +
            "emailed when it is decided. While it waits you can withdraw it under Your requests.");
    }

    /// <summary>M02: the refusal under Reason, the field marked and focused, Export kept, nothing sent.</summary>
    [Fact]
    public void SubmitWithNoReason_IsRefusedUnderReason_TheFocusInIt_AndTheTypeKept()
    {
        var cut = RenderComponent<DataRights>();

        cut.Find("#dr-type-Export").Change(true);
        Submit(cut).Click();

        var reason = cut.Find($"#{DataRights.ReasonId}");
        reason.GetAttribute("aria-invalid").Should().Be("true");
        reason.ClassList.Should().Contain("input-validation-error");
        reason.GetAttribute("aria-describedby").Should().Be($"{DataRights.ReasonMessageId} {DataRights.ReasonCountId}",
            "the refusal first, then the counter (N17)");
        var message = cut.Find($"#{DataRights.ReasonMessageId}");
        message.TagName.Should().Be("DIV");
        message.ClassList.Should().Contain("validation-message");
        message.GetAttribute("role").Should().Be("alert");
        Text(message).Should().Be("Please provide a reason for your request.");
        cut.Find("#dr-type-Export").HasAttribute("checked").Should().BeTrue("the type is kept");
        cut.FindAll(".action-result").Should().BeEmpty("a refusal found before any send stays under its field");
        FocusedById().Should().Equal(DataRights.ReasonId);
        _sender.Received.OfType<SubmitDataRightsRequestCommand>().Should().BeEmpty();
    }

    [Fact]
    public void SubmitWithNoType_IsRefusedUnderTheLegend_AndTheFirstRadioTakesTheFocus()
    {
        var cut = RenderComponent<DataRights>();

        cut.Find($"#{DataRights.ReasonId}").Change("A copy, please.");
        Submit(cut).Click();

        var group = cut.Find("fieldset");
        group.ClassList.Should().Contain("is-invalid");
        group.GetAttribute("aria-describedby").Should().Be(DataRights.TypeMessageId);
        Text(cut.Find($"#{DataRights.TypeMessageId}")).Should().Be("Choose the request type.");
        group.QuerySelectorAll("input[type='radio']").Should().OnlyContain(radio => radio.GetAttribute("aria-invalid") == "true");
        cut.Find($"#{DataRights.ReasonId}").GetAttribute("value").Should().Be("A copy, please.", "the reason is kept");
        FocusedById().Should().Equal("dr-type-Access");
        _sender.Received.OfType<SubmitDataRightsRequestCommand>().Should().BeEmpty();

        cut.Find("#dr-type-Access").Change(true);
        cut.FindAll($"#{DataRights.TypeMessageId}").Should().BeEmpty("a changed value drops its refusal");
        cut.Find("fieldset").ClassList.Should().NotContain("is-invalid");
    }

    [Fact]
    public void AReasonPastTheLimit_IsRefusedUnderReason()
    {
        var cut = RenderComponent<DataRights>();

        cut.Find("#dr-type-Access").Change(true);
        cut.Find($"#{DataRights.ReasonId}").Change(new string('a', 4001));
        Text(cut.Find($"#{DataRights.ReasonCountId}")).Should().Be("4,001 of 4,000 characters");
        Submit(cut).Click();

        Text(cut.Find($"#{DataRights.ReasonMessageId}")).Should().Be("Give your reason in 4,000 characters or fewer.");
        _sender.Received.OfType<SubmitDataRightsRequestCommand>().Should().BeEmpty();
    }

    [Fact]
    public void ASubmittedRequest_IsSaidUnderYourRequests_TheFormCleared_AndTheRowReadAgain()
    {
        var cut = RenderComponent<DataRights>();

        cut.Find("#dr-type-Export").Change(true);
        cut.Find($"#{DataRights.ReasonId}").Change("A copy of my training record for my own files.");
        _sender.AfterSubmit = [Request(DataRightsRequestType.Export, DataRightsRequestStatus.Submitted)];
        Submit(cut).Click();

        var command = _sender.Received.OfType<SubmitDataRightsRequestCommand>().Should().ContainSingle().Which;
        command.Type.Should().Be(DataRightsRequestType.Export);
        command.Reason.Should().Be("A copy of my training record for my own files.");

        cut.WaitForAssertion(() => Text(Section(cut, "requests").QuerySelector(".action-result")!)
            .Should().Be("Request submitted. You will be notified when it is reviewed."));
        Section(cut, "requests").QuerySelector(".action-result .alert")!.GetAttribute("role").Should().Be("status");
        cut.FindAll(".action-result").Should().HaveCount(1);
        cut.FindAll("tbody tr").Should().HaveCount(1);
        cut.Find($"#{DataRights.ReasonId}").GetAttribute("value").Should().BeEmpty();
        cut.FindAll("input[type='radio'][checked]").Should().BeEmpty();
        cut.WaitForAssertion(() => JSInterop.Invocations.Count(call => call.Identifier == FocusElement).Should().Be(1));
    }

    /// <summary>M16, T329: a failed send is fixed words in Make a request, the type and reason kept, never the exception.</summary>
    [Fact]
    public void AFailedSubmit_IsFixedWordsInMakeARequest_AndTheTypeAndReasonAreKept()
    {
        _sender.FailSubmit = new InvalidOperationException(ScriptedSender.Boom);
        var cut = RenderComponent<DataRights>();

        cut.Find("#dr-type-Export").Change(true);
        cut.Find($"#{DataRights.ReasonId}").Change("A copy of my training record for my own files.");
        Submit(cut).Click();

        cut.WaitForAssertion(() => Text(Section(cut, "make").QuerySelector(".action-result")!).Should().Be(
            "The request could not be sent just now. Nothing was sent. Try again, or come back in a few minutes."));
        Section(cut, "make").QuerySelector(".action-result .alert")!.GetAttribute("role").Should().Be("alert");
        cut.Markup.Should().NotContain(ScriptedSender.Boom);
        cut.Find("#dr-type-Export").HasAttribute("checked").Should().BeTrue();
        cut.Find($"#{DataRights.ReasonId}").GetAttribute("value").Should().Be("A copy of my training record for my own files.");
        cut.WaitForAssertion(() => JSInterop.Invocations.Count(call => call.Identifier == FocusElement).Should().Be(1));
    }

    /// <summary>The validator's refusal of Reason is said under Reason, as the page's own is.</summary>
    [Fact]
    public void TheValidatorsRefusalOfTheReason_IsSaidUnderReason()
    {
        _sender.FailSubmit = new ValidationException([new ValidationFailure("Reason", "'Reason' must not be empty.")]);
        var cut = RenderComponent<DataRights>();

        cut.Find("#dr-type-Export").Change(true);
        cut.Find($"#{DataRights.ReasonId}").Change("   x");
        Submit(cut).Click();

        cut.WaitForAssertion(() => Text(cut.Find($"#{DataRights.ReasonMessageId}"))
            .Should().Be("Please provide a reason for your request."));
        cut.Markup.Should().NotContain("must not be empty");
        cut.FindAll(".action-result").Should().BeEmpty();
    }

    // ---- Withdraw (Q14) ----

    [Fact]
    public void Withdraw_SaysSoUnderYourRequests_AndReadsTheRowAgain()
    {
        var request = Request(DataRightsRequestType.Access, DataRightsRequestStatus.Submitted);
        _sender.Requests = [request];
        var cut = RenderComponent<DataRights>();

        _sender.Requests = [request with { Status = DataRightsRequestStatus.Withdrawn }];
        cut.Find("tbody button").Click();

        _sender.Received.OfType<WithdrawDataRightsRequestCommand>().Should().ContainSingle()
            .Which.RequestId.Should().Be(request.Id);
        cut.WaitForAssertion(() => Text(Section(cut, "requests").QuerySelector(".action-result")!).Should().Be("Request withdrawn."));
        Text(cut.Find("td.answer-cell")).Should().Be("You withdrew it before a decision.");
        cut.FindAll("tbody button").Should().BeEmpty("nothing is offered on a withdrawn request (M06)");
    }

    [Fact]
    public void AWithdrawOfARequestDecidedMeanwhile_SaysSo_AndShowsTheDecision()
    {
        var request = Request(DataRightsRequestType.Access, DataRightsRequestStatus.Submitted);
        _sender.Requests = [request];
        var cut = RenderComponent<DataRights>();

        _sender.FailWithdraw = new DataRightsRefusedException(DataRightsRefusal.DecidedMeanwhile, ScriptedSender.Boom);
        _sender.Requests = [request with { Status = DataRightsRequestStatus.Rejected, DecidedOn = DecidedUtc, DecisionNote = "No." }];
        cut.Find("tbody button").Click();

        cut.WaitForAssertion(() => Text(Section(cut, "requests").QuerySelector(".action-result")!).Should().Be(
            "This request was decided before it could be withdrawn. Nothing was withdrawn."));
        Section(cut, "requests").QuerySelector(".action-result .alert")!.GetAttribute("role").Should().Be("alert");
        Text(cut.Find("td.answer-cell")).Should().Be("Rejected on 2026-10-28: “No.”");
        cut.Markup.Should().NotContain(ScriptedSender.Boom);
    }

    [Fact]
    public void AFailedWithdraw_IsFixedWords()
    {
        _sender.Requests = [Request(DataRightsRequestType.Access, DataRightsRequestStatus.Submitted)];
        var cut = RenderComponent<DataRights>();

        _sender.FailWithdraw = new UnauthorizedAccessException(ScriptedSender.Boom);
        cut.Find("tbody button").Click();

        cut.WaitForAssertion(() => Text(Section(cut, "requests").QuerySelector(".action-result")!).Should().Be(
            "The request could not be withdrawn just now. Nothing has changed. Try again, or come back in a few minutes."));
        cut.Markup.Should().NotContain(ScriptedSender.Boom);
    }

    // ---- one result at a time (M13) ----

    [Fact]
    public void ARefusalAfterAnEarlierResult_ShowsAlone()
    {
        var cut = RenderComponent<DataRights>();

        cut.Find($"#{DataRights.RemindersId}").Change(true);
        cut.FindAll("button").Single(button => Text(button) == "Save").Click();
        cut.WaitForAssertion(() => Text(Section(cut, "reminders").QuerySelector(".action-result")!)
            .Should().Be("Reminder emails stopped."));

        cut.Find("#dr-type-Export").Change(true);
        Submit(cut).Click();

        cut.FindAll(".action-result").Should().BeEmpty("Reminder emails stopped. is gone (M13)");
        Text(cut.Find($"#{DataRights.ReasonMessageId}")).Should().Be("Please provide a reason for your request.");
        cut.Find($"#{DataRights.RemindersId}").HasAttribute("checked").Should().BeTrue();
    }

    [Fact]
    public void AFailedSave_IsFixedWordsInReminderEmails()
    {
        _sender.FailSave = new InvalidOperationException(ScriptedSender.Boom);
        var cut = RenderComponent<DataRights>();

        cut.FindAll("button").Single(button => Text(button) == "Save").Click();

        cut.WaitForAssertion(() => Text(Section(cut, "reminders").QuerySelector(".action-result")!).Should().Be(
            "The reminder emails could not be saved just now. Nothing has changed. Try again, or come back in a few minutes."));
        cut.Markup.Should().NotContain(ScriptedSender.Boom);
    }

    // ---- the words, without the page ----

    [Fact]
    public void TheWords_PrintNoEnumNameAndNoUtc()
    {
        MyDataRightsWords.ToTheSecond(Made.AddSeconds(7)).Should().Be("2026-10-28 00:01:07 SAST");
        MyDataRightsWords.Counter(4000).Should().Be("4,000 of 4,000 characters");
        DataRightsLabels.OfferedTypes.Select(MyDataRightsWords.TypeHelp).Should().OnlyContain(help => help.Length > 0);
    }

    // ---- helpers ----

    private static MyDataRightsRequestDto Request(
        DataRightsRequestType type, DataRightsRequestStatus status, int n = 1, DateTime? made = null)
        => new(new Guid($"6a1f0000-0000-0000-0000-00000000000{n}"), made ?? Made, type, status, null, null, null, []);

    private static IElement Submit(IRenderedFragment cut)
        => cut.FindAll("button").Single(button => Text(button) == "Submit request");

    private static IElement Section(IRenderedFragment cut, string which)
        => cut.Find($"section[aria-labelledby='dr-{which}-heading']");

    private List<string> FocusedById()
        => JSInterop.Invocations.Where(call => call.Identifier == PageFocus.FocusByIdIdentifier)
            .Select(call => (string)call.Arguments[0]!).ToList();

    private static string Text(IElement element)
        => string.Join(' ', element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    /// <summary>Answers the page's reads and commands as a test scripts them.</summary>
    private sealed class ScriptedSender : IScopedSender
    {
        public const string Boom = "An exception's own text, never shown.";

        public List<object> Received { get; } = [];

        public string? InstitutionName { get; set; } = Kgk;

        public IReadOnlyList<MyDataRightsRequestDto> Requests { get; set; } = [];

        public IReadOnlyList<MyDataRightsRequestDto>? AfterSubmit { get; set; }

        public TaskCompletionSource? HoldLoad { get; set; }

        public int FailLoads { get; set; }

        public Exception? FailSubmit { get; set; }

        public Exception? FailWithdraw { get; set; }

        public Exception? FailSave { get; set; }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Received.Add(request);
            switch (request)
            {
                case GetMyDataRightsRequestsQuery:
                    if (HoldLoad is not null)
                    {
                        await HoldLoad.Task;
                    }

                    if (FailLoads > 0)
                    {
                        FailLoads--;
                        throw new InvalidOperationException(Boom);
                    }

                    return (TResponse)(object)new MyDataRightsDto(InstitutionName, Requests);
                case GetObjectionFlagsQuery:
                    return (TResponse)(object)new ObjectionFlagsDto(false);
                case SubmitDataRightsRequestCommand submit:
                    if (FailSubmit is not null)
                    {
                        throw FailSubmit;
                    }

                    if (AfterSubmit is not null)
                    {
                        Requests = AfterSubmit;
                    }

                    return (TResponse)(object)new DataRightsRequestDto(
                        Guid.NewGuid(), "trainee-1", "Anele Dlamini", Made, submit.Type, DataRightsRequestStatus.Submitted,
                        submit.Reason, null, null, null, null);
                case UpdateObjectionFlagsCommand save:
                    if (FailSave is not null)
                    {
                        throw FailSave;
                    }

                    return (TResponse)(object)new ReminderEmailsSaved(save.OptOutOfDigestEmails, Changed: true);
                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            Received.Add(request);
            if (request is WithdrawDataRightsRequestCommand)
            {
                return FailWithdraw is null ? Task.CompletedTask : Task.FromException(FailWithdraw);
            }

            throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
        }
    }
}
