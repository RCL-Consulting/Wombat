using Bunit;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Wombat.Application.Features.DataRights;
using Wombat.Application.Features.DataRights.Commands;
using Wombat.Application.Features.DataRights.Queries;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Components.Pages.Admin.DataRights;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// A data rights request's page (T387, flow 14, lane P; boards R3-D01–D22, C01–C11, K01, U01, U02): the title by type
/// and person, the sections per state, the decider's name over the id, the own-request lines, Decide's refusals under the
/// note, the erasure's dialog from its forecast and its result's clauses, every refusal's words and where it shows,
/// Request unavailable for a null read, the failed load's fixed words, and the Correction section.
/// </summary>
public sealed class DataRightsRequestPageTests : DataRightsStaffPageTestBase
{
    private static readonly Guid RequestId = Guid.Parse("01a17f61-f16f-77c1-860c-661586d5b733");
    private const string SmitId = "43ad0cb7-5ce6-46d7-8595-7117d130c7dc";

    private DataRightsRequestDetailDto? _page;
    private DataRightsRequestDetailDto? _after;
    private Func<object, object?> _command = _ => null;
    private ErasureForecastDto? _forecast;
    private Exception? _forecastThrows;
    private bool _done;

    private static DataRightsRequestDetailDto Export(DataRightsRequestStatus status = DataRightsRequestStatus.Submitted) => new(
        RequestId, "67083efa-bbaa-48d5-a753-b01fdf38530d", "Anele Dlamini", Submitted, DataRightsRequestType.Export, status,
        "A copy of my training record for my own files.",
        status == DataRightsRequestStatus.Submitted ? null : "Identity confirmed; export released to the data subject.",
        status == DataRightsRequestStatus.Submitted ? null : SmitId,
        status == DataRightsRequestStatus.Submitted ? null : Submitted,
        status == DataRightsRequestStatus.Completed ? Submitted : null)
    {
        RequesterEmail = "dlamini@kgk.wombat.local",
        InstitutionName = Kgk,
        DecidedByName = status == DataRightsRequestStatus.Submitted ? null : "Pieter Smit",
        MayDecide = status == DataRightsRequestStatus.Submitted
    };

    private static DataRightsRequestDetailDto Erasure(DataRightsRequestStatus status = DataRightsRequestStatus.Submitted)
        => Export(status) with
        {
            RequesterName = "Sipho Ndlovu",
            RequesterEmail = "ndlovu@kgk.wombat.local",
            Type = DataRightsRequestType.Erasure,
            Reason = "I am leaving the programme and want my personal data removed."
        };

    private static DataRightsRequestDetailDto Rectification(params DataRightsCorrectionDto[] corrections)
        => Export(DataRightsRequestStatus.Approved) with
        {
            RequesterName = "Nomsa Mahlngu",
            Type = DataRightsRequestType.Rectification,
            MayRecord = true,
            Corrections = corrections,
            ProgrammeStart = new ProgrammeStartRecordDto(5, Ended: false, MayOpen: false)
        };

    private static DataRightsRequestDto Dto(DataRightsRequestStatus status) => new(
        RequestId, "requester", "Anele Dlamini", Submitted, DataRightsRequestType.Export, status, "A reason.", "A note.", SmitId,
        Submitted, null);

    private static DataRightsCorrectionDto LastName()
        => new(Guid.NewGuid(), DataRightsRecords.AccountLastName, "Mahlngu", "Mahlangu", SmitId, "Pieter Smit", Submitted.AddMinutes(5));

    private static ErasureForecastDto Forecast() => new(
        "Sipho Ndlovu", [],
        [new ErasureForecastReview(8, "Formative check-in", "Paed Annual Review Panel", CommitteeReviewState.Scheduled)],
        0, [], [], "Paediatric EPA Curriculum (11.2)",
        [
            new ErasureForecastReview(5, "Annual progression review", "Paed Annual Review Panel", CommitteeReviewState.Ratified),
            new ErasureForecastReview(6, "Formative check-in", "Paed Annual Review Panel", CommitteeReviewState.Final)
        ]);

    /// <summary>Opens the page on <paramref name="page" />; after a command has answered, the page reads <paramref name="after" />.</summary>
    private IRenderedComponent<RequestDetail> Open(DataRightsRequestDetailDto? page, DataRightsRequestDetailDto? after = null)
    {
        _page = page;
        _after = after ?? page;
        Sender.Answer = request =>
        {
            switch (request)
            {
                case GetDataRightsRequestByIdQuery:
                    return _done ? _after : _page;
                case GetErasureForecastQuery:
                    return (object?)_forecastThrows ?? _forecast;
                default:
                    var answer = _command(request);
                    _done |= answer is not null and not Exception || answer is DataRightsRefusedException { Reason: DataRightsRefusal.Withdrawn or DataRightsRefusal.DecidedMeanwhile };
                    return answer;
            }
        };
        var cut = RenderComponent<RequestDetail>(parameters => parameters.Add(page => page.Id, RequestId));
        cut.WaitForState(() => cut.FindAll(".loading-line").Count == 0);
        return cut;
    }

    // ---- the page ----

    [Fact]
    public void ASubmittedExport_IsNamedByTypeAndPerson_WithRequestThenDecide_AndDecidesHelpByType()
    {
        var cut = Open(Export());

        Text(cut.Find("h1")).Should().Be("Export request from Anele Dlamini");
        Text(cut.Find(".page-subtitle")).Should().Be("Submitted 2026-10-28 00:01 SAST · Kgosi Kgari Teaching Hospital");
        Headings(cut).Should().Equal("Request", "Decide");
        cut.FindAll("section.user-section h2[tabindex]").Should().BeEmpty("no h2 takes the focus (N18)");

        var request = Section(cut, "Request");
        request.QuerySelectorAll("dt").Select(Text).Should().Equal("Requester", "Email", "User id", "Request id", "Type", "Status", "Reason");
        request.QuerySelectorAll("dd").Select(Text).Should().Equal(
            "Anele Dlamini", "dlamini@kgk.wombat.local", "67083efa-bbaa-48d5-a753-b01fdf38530d", RequestId.ToString(), "Export",
            "Submitted", "A copy of my training record for my own files.");
        request.QuerySelectorAll("dd .request-id").Should().HaveCount(2);

        var decide = Section(cut, "Decide");
        Text(decide.QuerySelector("p")!).Should().Be(
            "Approving makes the export ready at once: Anele Dlamini downloads it from My data rights, and it is built at that " +
            "moment. You cannot download it yourself.");
        Text(decide.QuerySelector("label[for='decision-note']")!).Should().Be("Decision note * required");
        Text(decide.QuerySelector("#decision-note-help")!).Should().Be(
            "Anele Dlamini reads this note with the decision on My data rights, and is emailed that it was decided. Who decided is not shown.");
        decide.QuerySelector("#decision-note")!.GetAttribute("aria-describedby").Should().Be("decision-note-help decision-note-count");
        Text(decide.QuerySelector("#decision-note-count")!).Should().Be("0 of 4,000 characters", "the note's counter (review P4)");
        decide.QuerySelector("#decision-note")!.HasAttribute("maxlength").Should().BeFalse("a paste is never cut silently");
        decide.QuerySelectorAll(".form-actions .btn").Select(button => (Text(button), button.ClassList.Contains("btn-primary")))
            .Should().Equal(("Reject", false), ("Approve", true));
        cut.Markup.Should().NotContain("Back to list").And.NotContain("<code");
    }

    [Fact]
    public void ApproveWithNoNote_IsRefusedUnderTheNote_TheFocusInIt_AndNothingIsSent()
    {
        var cut = Open(Export());

        Button(cut, "Approve").Click();

        var note = cut.Find("#decision-note");
        note.GetAttribute("aria-invalid").Should().Be("true");
        note.GetAttribute("aria-describedby").Should().Be(
            "decision-note-err decision-note-help decision-note-count", "the refusal first, then the help, then the counter (N17)");
        Text(cut.Find("#decision-note-err")).Should().Be("A decision note is required.");
        cut.Find("#decision-note-err").GetAttribute("role").Should().Be("alert");
        FocusedIds().Should().Equal("decision-note");
        Sender.SentOf<ApproveDataRightsRequestCommand>().Should().BeEmpty();
    }

    [Fact]
    public void AnApprovedExport_HeadsDecisionWithItsResult_TheDeciderByNameOverTheId_AndTheResultTakesTheFocus()
    {
        _command = request => request is ApproveDataRightsRequestCommand ? new ApproveDataRightsResult(null!, null) : null;
        var cut = Open(Export(), Export(DataRightsRequestStatus.Completed));

        cut.Find("#decision-note").Change("Identity confirmed; export released to the data subject.");
        Button(cut, "Approve").Click();

        cut.WaitForAssertion(() => Headings(cut).Should().Equal("Request", "Decision"));
        var decision = Section(cut, "Decision");
        var result = decision.QuerySelector(".action-result .alert-success")!;
        result.QuerySelector("strong")!.TextContent.Should().Be("Request approved.");
        Text(result).Should().Be("Request approved. Anele Dlamini is emailed, and can download the export from My data rights.");
        decision.QuerySelectorAll("dt").Select(Text).Should().Equal("Decision", "Decided by", "Decided on", "Note", "Completed on");
        var decidedBy = decision.QuerySelectorAll("dd")[1];
        decidedBy.FirstChild!.TextContent.Trim().Should().Be("Pieter Smit");
        Text(decidedBy.QuerySelector(".request-id")!).Should().Be(SmitId);
        Text(decision.QuerySelectorAll("dd")[2]).Should().Be("2026-10-28 00:01 SAST");
        ElementFocusCalls().Should().Be(1, "the result takes the focus (A5)");
    }

    [Fact]
    public void ARejection_SaysTheRequesterReadsTheNote()
    {
        _command = request => request is RejectDataRightsRequestCommand ? Dto(DataRightsRequestStatus.Rejected) : null;
        var cut = Open(Export(), Export(DataRightsRequestStatus.Rejected));

        cut.Find("#decision-note").Change("Nothing to correct.");
        Button(cut, "Reject").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result")).Should().Be(
            "Request rejected. Anele Dlamini is emailed, and reads your note on My data rights."));
        Text(Section(cut, "Decision").QuerySelector("dd")!).Should().Be("Rejected");
    }

    [Fact]
    public void AWithdrawnRequest_SaysThereIsNothingToDecide_AndOffersNoDecide()
    {
        var cut = Open(Export(DataRightsRequestStatus.Withdrawn) with { RequesterName = "Pieter du Plessis", MayDecide = false, DecidedByUserId = null, DecidedOn = null, DecisionNote = null });

        Headings(cut).Should().Equal("Request");
        Text(Section(cut, "Request").QuerySelector("dl + p")!).Should().Be(
            "Pieter du Plessis withdrew this request before it was decided. There is nothing to decide.");
        cut.Find(".badge").ClassList.Should().Contain(BadgeFor.DataRightsRequest(DataRightsRequestStatus.Withdrawn));
    }

    [Theory]
    [InlineData(DataRightsOwnRequest.AnotherCoordinatorDecides,
        "You cannot decide your own request: another Coordinator at Kgosi Kgari Teaching Hospital, or the Wombat administrator, decides it.")]
    [InlineData(DataRightsOwnRequest.AnotherAdministratorDecides, "You cannot decide your own request: another Administrator decides it.")]
    public void TheCallersOwnRequest_SaysWhoDecidesIt_InDecidesPlace(DataRightsOwnRequest own, string line)
    {
        var cut = Open(Export() with { MayDecide = false, OwnRequest = own });

        Headings(cut).Should().Equal("Request");
        Text(Section(cut, "Request").QuerySelector("dl + p")!).Should().Be(line);
    }

    [Fact]
    public void ARequestWithNoInstitution_HasNoInstitutionInItsSubtitle()
    {
        var cut = Open(Export() with { InstitutionName = null, OwnRequest = DataRightsOwnRequest.AnotherAdministratorDecides, MayDecide = false });

        Text(cut.Find(".page-subtitle")).Should().Be("Submitted 2026-10-28 00:01 SAST");
    }

    [Fact]
    public void AnErasedRequestersRequest_StandsTheNotice_ThePseudonymOverTheDay_AndNoAddress()
    {
        var cut = Open(Erasure(DataRightsRequestStatus.Completed) with
        {
            RequesterName = "deleted_user_0ae1d64b",
            RequesterEmail = null,
            ErasedOn = new DateOnly(2026, 10, 28),
            DecidedByName = "Demo Administrator"
        });

        Text(cut.Find("h1")).Should().Be("Erasure request from deleted_user_0ae1d64b");
        var notice = cut.Find(".alert-info");
        Text(notice).Should().Be(
            "This person's personal data was erased on 2026-10-28, at this request. The request is kept as the record of it, under the pseudonym.");
        notice.HasAttribute("role").Should().BeFalse("standing page content is no live region");
        var request = Section(cut, "Request");
        request.QuerySelectorAll("dt").Select(Text).Should().NotContain("Email");
        var person = request.QuerySelector("dd .person-cell")!;
        person.FirstChild!.TextContent.Should().Be("deleted_user_0ae1d64b");
        Text(person.QuerySelector("span")!).Should().Be("Erased 2026-10-28");
    }

    [Fact]
    public void ANullRead_IsRequestUnavailable_TheSameForAnUnknownIdAndAForeignOne()
    {
        var cut = Open(null);

        Text(cut.Find("h1")).Should().Be("Request unavailable");
        Text(cut.Find(".detail-card--empty .state-panel-copy")).Should().Be("This data rights request does not exist, or you cannot open it.");
        var way = cut.Find(".detail-card--empty a.btn-primary");
        Text(way).Should().Be("Go to Data rights requests");
        way.GetAttribute("href").Should().Be("/admin/data-rights");
        cut.FindAll(".alert").Should().BeEmpty("no alert: the page says it plainly (A.5.7)");
        cut.FindAll(".page-subtitle").Should().BeEmpty();
        cut.Markup.Should().NotContain("Not found");
    }

    [Fact]
    public void AFailedRead_IsFixedWordsWithTryAgain_NeverTheExceptionsText()
    {
        Sender.Answer = _ => new InvalidOperationException("Npgsql: timeout");
        var cut = RenderComponent<RequestDetail>(parameters => parameters.Add(page => page.Id, RequestId));

        cut.WaitForAssertion(() => cut.Find(".alert-danger"));
        Text(cut.Find("h1")).Should().Be("Data rights request");
        Text(cut.Find(".alert-danger .alert-row-text")).Should().Be(
            "Could not load this request. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("Npgsql");
        Button(cut, "Try again").Should().NotBeNull();
    }

    [Fact]
    public void WhileTheReadRuns_TheLoadingLine_TheSkeleton_AndNoDecide()
    {
        Sender.Held = request => request is GetDataRightsRequestByIdQuery;
        Sender.Answer = request => request is GetDataRightsRequestByIdQuery ? Export() : null;
        var cut = RenderComponent<RequestDetail>(parameters => parameters.Add(page => page.Id, RequestId));

        Text(cut.Find("h1")).Should().Be("Data rights request");
        Text(cut.Find(".loading-line")).Should().Be("Loading the request.");
        cut.FindAll("section.user-section").Should().BeEmpty();

        Sender.Gate.SetResult();
        cut.WaitForAssertion(() => Text(cut.Find("h1")).Should().Be("Export request from Anele Dlamini"),
            Wombat.Web.Tests.TestSupport.WombatTestContext.AsyncWorkTimeout);
    }

    // ---- the erasure ----

    [Fact]
    public void AnErasure_OffersApproveAndEraseAsAnOutlineTrigger_WhichReadsTheForecastAndOpensTheDialog()
    {
        _forecast = Forecast();
        var cut = Open(Erasure());

        var decide = Section(cut, "Decide");
        Text(decide.QuerySelector("p")!).Should().Be(
            "Approving erases Sipho Ndlovu's personal data at once and cannot be undone. You are asked to confirm, with what it ends and keeps.");
        Text(decide.QuerySelector("#decision-note-help")!).Should().Be(
            "Kept with the request. Sipho Ndlovu will not read it: approving ends the account.");
        var trigger = cut.Find("#approve-and-erase");
        Text(trigger).Should().Be("Approve and erase…");
        trigger.ClassList.Should().Contain("btn-outline").And.NotContain("btn-danger");
        trigger.GetAttribute("aria-haspopup").Should().Be("dialog");

        cut.Find("#decision-note").Change("Identity confirmed. Programme records are kept under a pseudonym.");
        trigger.Click();

        cut.WaitForAssertion(() => Invoked("wombatDialog.showModal").Should().Be(1));
        Sender.SentOf<GetErasureForecastQuery>().Should().ContainSingle();
        Sender.SentOf<ApproveDataRightsRequestCommand>().Should().BeEmpty("nothing is erased before the confirm");
        var dialog = cut.Find("dialog");
        Text(dialog.QuerySelector("h2")!).Should().Be("Erase Sipho Ndlovu's personal data?");
        dialog.QuerySelector(".dialog-body .dialog-body--scroll").Should().NotBeNull();
        dialog.QuerySelectorAll(".dialog-body--scroll > p").Select(Text).Should().Equal(
            "This cannot be undone.",
            "Approving erases it now, in one step, and completes the request.",
            "It ends:",
            "It keeps, under a pseudonym that names nobody: the ratified review #5, closed check-in #6, the activities, progress and audit entries, and this request.");
        dialog.QuerySelectorAll(".dialog-list li").Select(Text).Should().Equal(
            "The account and its sign-in. Sipho Ndlovu can no longer sign in, and an open session ends within a minute.",
            "1 open committee review: Formative check-in #8, Paed Annual Review Panel, Scheduled. It is withdrawn, and nothing more is decided at it.",
            "No MSF campaign: none about Sipho Ndlovu is open.",
            "The programme, Paediatric EPA Curriculum (11.2), which ends today as Withdrawn.");
        dialog.QuerySelectorAll(".form-actions button").Select(button => (Text(button), button.ClassList.Contains("btn-danger")))
            .Should().Equal(("Cancel", false), ("Erase and approve", true));
    }

    [Fact]
    public void TheDialog_ForAStaffRoleAndAReviewElsewhere_CountsWhatTheCallerMayNotName()
    {
        var forecast = Forecast() with
        {
            StaffRoleKeys = [WombatRoles.Assessor, WombatRoles.CommitteeMember],
            OpenReviewsElsewhere = 1,
            OpenCampaigns = [new ErasureForecastCampaign(3, "MSF for Sipho Ndlovu, 2026 S2", MsfCampaignState.Open)],
            OtherOpenRequests = [new ErasureForecastRequest(Guid.NewGuid(), DataRightsRequestType.Access, Submitted)],
            KeptReviews = [new ErasureForecastReview(9, "Annual progression review", "Paed Annual Review Panel", CommitteeReviewState.UnderAppeal)]
        };

        var lines = DataRightsRequestWords.DialogEndsLines(forecast);

        lines.Should().Equal(
            "The account and its sign-in. Sipho Ndlovu can no longer sign in, and an open session ends within a minute.",
            "The roles Assessor and Committee member and their scopes.",
            "2 open committee reviews: Formative check-in #8, Paed Annual Review Panel, Scheduled, and 1 open review at another institution. Each is withdrawn, and nothing more is decided at it.",
            "1 MSF campaign not yet released: MSF for Sipho Ndlovu, 2026 S2.",
            "1 other open data rights request: Access, made 2026-10-28 00:01 SAST. It is withdrawn.",
            "The programme, Paediatric EPA Curriculum (11.2), which ends today as Withdrawn.");
        DataRightsRequestWords.DialogKeeps(forecast).Should().Be(
            "It keeps, under a pseudonym that names nobody: the activities, progress and audit entries, this request, and the review under appeal, which can still be answered (Annual progression review #9, Paed Annual Review Panel).");
    }

    [Fact]
    public void EraseAndApprove_ClosesTheDialog_AndHeadsDecisionWithWhatTheErasureEnded()
    {
        _forecast = Forecast();
        _command = request => request is ApproveDataRightsRequestCommand
            ? new ApproveDataRightsResult(null!, new ErasureOutcome("deleted_user_0ae1d64b", 1, 0, 0, 1))
            : null;
        var cut = Open(Erasure(), Erasure(DataRightsRequestStatus.Completed) with
        {
            RequesterName = "deleted_user_0ae1d64b",
            RequesterEmail = null,
            ErasedOn = new DateOnly(2026, 10, 28)
        });

        cut.Find("#decision-note").Change("Identity confirmed.");
        cut.Find("#approve-and-erase").Click();
        cut.WaitForAssertion(() => Invoked("wombatDialog.showModal").Should().Be(1));
        DialogButton(cut, "Erase and approve").Click();

        cut.WaitForAssertion(() => Headings(cut).Should().Equal("Request", "Decision"));
        Invoked("wombatDialog.close").Should().BeGreaterThan(0);
        var result = Section(cut, "Decision").QuerySelector(".action-result .alert-success")!;
        result.QuerySelector("strong")!.TextContent.Should().Be("The personal data is erased.");
        Text(result).Should().Be(
            "The personal data is erased. The request is approved and completed. 1 committee review was withdrawn and the programme " +
            "ended today. The account is now deleted_user_0ae1d64b; its open session ends within a minute.");
        Text(cut.Find("h1")).Should().Be("Erasure request from deleted_user_0ae1d64b");
    }

    [Theory]
    [InlineData(0, 0, 0, 0, "Nothing else was open to end.")]
    [InlineData(2, 1, 1, 0, "2 committee reviews were withdrawn, 1 MSF campaign was withdrawn and 1 other data rights request was withdrawn.")]
    [InlineData(0, 0, 2, 1, "2 other data rights requests were withdrawn and the programme ended today.")]
    public void TheErasuresResult_NamesOnlyWhatItEnded(int reviews, int campaigns, int requests, int profiles, string ended)
        => DataRightsRequestWords.Erased(new ErasureOutcome("deleted_user_1", reviews, campaigns, requests, profiles)).ToString()
            .Should().Be($"The personal data is erased. The request is approved and completed. {ended} The account is now deleted_user_1; its open session ends within a minute.");

    [Fact]
    public void ApproveAndEraseWithNoNote_OpensNoDialog()
    {
        _forecast = Forecast();
        var cut = Open(Erasure());

        cut.Find("#approve-and-erase").Click();

        Text(cut.Find("#decision-note-err")).Should().Be("A decision note is required.");
        Invoked("wombatDialog.showModal").Should().Be(0);
        Sender.SentOf<GetErasureForecastQuery>().Should().BeEmpty();
    }

    // ---- the refusals ----

    [Theory]
    [InlineData(DataRightsRefusal.ErasureNotConfigured, "Nothing has been erased.",
        "Nothing has been erased. Erasure is not set up on this server. The request is still Submitted. Ask whoever runs the server to set it up, then approve again.")]
    [InlineData(DataRightsRefusal.LastAdministrator, "Nothing has been erased.",
        "Nothing has been erased. Sipho Ndlovu is Wombat's only Administrator, and erasing the account would leave nobody to run it. Add another Administrator first, then approve again.")]
    [InlineData(DataRightsRefusal.PersonChanged, null,
        "Something about this person changed while they were being erased: a review, a feedback campaign or their account was changed elsewhere at the same moment. Nothing has been erased, and the request has not been approved. Approve it again.")]
    public void AnErasureRefusedByTheCommand_HeadsDecide_TheNoteKept_InTheBoardsWords(DataRightsRefusal reason, string? lead, string words)
    {
        _forecast = Forecast();
        _command = request => request is ApproveDataRightsRequestCommand
            ? new DataRightsRefusedException(reason, "The command's own message, never shown.")
            : null;
        var cut = Open(Erasure());

        cut.Find("#decision-note").Change("Identity confirmed.");
        cut.Find("#approve-and-erase").Click();
        cut.WaitForAssertion(() => Invoked("wombatDialog.showModal").Should().Be(1));
        DialogButton(cut, "Erase and approve").Click();

        cut.WaitForAssertion(() => Section(cut, "Decide").QuerySelector(".action-result .alert-danger").Should().NotBeNull());
        var alert = Section(cut, "Decide").QuerySelector(".action-result .alert-danger")!;
        Text(alert).Should().Be(words);
        alert.GetAttribute("role").Should().Be("alert");
        alert.QuerySelector("strong")?.TextContent.Should().Be(lead);
        if (lead is null)
        {
            alert.QuerySelector("strong").Should().BeNull();
        }

        cut.Find("#decision-note").GetAttribute("value").Should().Be("Identity confirmed.", "the note is kept (D21)");
        cut.Markup.Should().NotContain("never shown");
        ElementFocusCalls().Should().Be(1);
    }

    [Fact]
    public void WithdrawnMeanwhile_ReadsThePageAgain_AsWithdrawn_TheRefusalEndingRequest()
    {
        _command = request => request is ApproveDataRightsRequestCommand
            ? new DataRightsRefusedException(DataRightsRefusal.Withdrawn, "Withdrawn.")
            : null;
        var cut = Open(Erasure(), Erasure(DataRightsRequestStatus.Withdrawn) with
        {
            MayDecide = false, DecidedByUserId = null, DecidedOn = null, DecisionNote = null
        });
        _forecast = Forecast();

        cut.Find("#decision-note").Change("Identity confirmed.");
        cut.Find("#approve-and-erase").Click();
        cut.WaitForAssertion(() => Invoked("wombatDialog.showModal").Should().Be(1));
        DialogButton(cut, "Erase and approve").Click();

        cut.WaitForAssertion(() => Headings(cut).Should().Equal("Request"));
        var alert = Section(cut, "Request").QuerySelector(".action-result .alert-danger")!;
        alert.QuerySelector("strong")!.TextContent.Should().Be("Sipho Ndlovu withdrew this request.");
        Text(alert).Should().Be("Sipho Ndlovu withdrew this request. Nothing was decided or erased.");
        Section(cut, "Request").QuerySelectorAll("dl + p").Should().BeEmpty("the refusal alone, not the standing line too");
    }

    [Fact]
    public void DecidedMeanwhile_ReadsThePageAgain_TheRefusalHeadingDecision()
    {
        _command = request => request is RejectDataRightsRequestCommand
            ? new DataRightsRefusedException(DataRightsRefusal.DecidedMeanwhile, "Decided.")
            : null;
        var cut = Open(Export(), Export(DataRightsRequestStatus.Completed) with { MayDecide = false });

        cut.Find("#decision-note").Change("Not confirmed.");
        Button(cut, "Reject").Click();

        cut.WaitForAssertion(() => Headings(cut).Should().Equal("Request", "Decision"));
        Text(Section(cut, "Decision").QuerySelector(".action-result")!).Should().Be(
            "This request was decided while this page was open. Nothing more was done.");
    }

    [Fact]
    public void AFailedDecision_IsFixedWords_TheNoteKept_NeverTheExceptionsText()
    {
        _command = request => request is ApproveDataRightsRequestCommand ? new TimeoutException("Npgsql: the pool is exhausted") : null;
        var cut = Open(Export());

        cut.Find("#decision-note").Change("Identity confirmed.");
        Button(cut, "Approve").Click();

        cut.WaitForAssertion(() => Text(Section(cut, "Decide").QuerySelector(".action-result")!).Should().Be(
            "The decision could not be saved just now. Nothing has changed. Try again, or come back in a few minutes."));
        cut.Markup.Should().NotContain("Npgsql");
        cut.Find("#decision-note").GetAttribute("value").Should().Be("Identity confirmed.");
    }

    [Fact]
    public void TheNoteRefusedByTheValidator_ShowsUnderTheNote()
    {
        _command = request => request is ApproveDataRightsRequestCommand
            ? new ValidationException([new ValidationFailure(nameof(ApproveDataRightsRequestCommand.DecisionNote), "too long")])
            : null;
        var cut = Open(Export());

        cut.Find("#decision-note").Change("x");
        Button(cut, "Approve").Click();

        cut.WaitForAssertion(() => Text(cut.Find("#decision-note-err")).Should().Be("A decision note is required."));
        FocusedIds().Should().Equal("decision-note");
    }

    // ---- the Correction section ----

    [Fact]
    public void AnApprovedRectification_OffersTheFourRecords_EachWithWhereItIsChanged()
    {
        var cut = Open(Rectification());

        Headings(cut).Should().Equal("Request", "Decision", "Correction");
        var correction = Section(cut, "Correction");
        correction.Children.Where(child => child.TagName == "P").Select(Text).Should().Equal(
            "Record each change as it is made on the page that holds the data.",
            "No correction is recorded yet.",
            "Once every correction is made and recorded, complete the request. It then reads Completed here and on Nomsa Mahlngu's My data rights.");
        Text(correction.QuerySelector("legend")!).Should().Be("Record * required");
        correction.QuerySelectorAll(".form-check label").Select(Text).Should().Equal(
            "Account › First name", "Account › Last name", "Trainee profile › Programme start", "Other");
        correction.QuerySelectorAll(".form-check .choice-help").Select(Text).Should().Equal(
            "Changed by Nomsa Mahlngu on My account. No staff page changes a name.",
            "Changed by Nomsa Mahlngu on My account. No staff page changes a name.",
            "Changed on Nomsa Mahlngu's programme page, which an Institutional admin at Kgosi Kgari Teaching Hospital opens; you cannot.",
            "Name the record in the field under it.");
        correction.QuerySelectorAll(".correction-values label").Select(Text).Should().Equal("Value before * required", "Value after * required");
        correction.QuerySelectorAll(".form-actions .btn").Select(Text).Should().Equal("Record correction", "Complete request");
        cut.FindAll("#rec-name").Should().BeEmpty("Other's name only once Other is chosen");
    }

    [Fact]
    public void TheProgrammeStart_LinksForOneWhoMayOpenIt_ReadsArchivedOnceEnded_AndIsNotOfferedWithNoProfile()
    {
        var mayOpen = Open(Rectification() with { ProgrammeStart = new ProgrammeStartRecordDto(5, Ended: false, MayOpen: true) });
        var help = mayOpen.Find("#rec-start-h");
        Text(help).Should().Be("Changed on Nomsa Mahlngu's programme page. Open Nomsa Mahlngu's programme");
        help.QuerySelector("a")!.GetAttribute("href").Should().Be("/admin/trainees/edit?id=5");

        _page = Rectification() with { ProgrammeStart = new ProgrammeStartRecordDto(5, Ended: true, MayOpen: true) };
        var ended = RenderComponent<RequestDetail>(parameters => parameters.Add(page => page.Id, RequestId));
        ended.WaitForAssertion(() => Text(ended.Find("#rec-start-h")).Should().Be(
            "This programme has ended, so its record is archived and cannot be changed."));
        ended.Find("#rec-start-h").QuerySelector("a").Should().BeNull();

        _page = Rectification() with { ProgrammeStart = null };
        var none = RenderComponent<RequestDetail>(parameters => parameters.Add(page => page.Id, RequestId));
        none.WaitForAssertion(() => none.FindAll(".form-check label").Select(Text).Should().Equal(
            "Account › First name", "Account › Last name", "Other"));
    }

    [Fact]
    public void ANameFromAnInstitutionsSignIn_DisablesTheTwoNameRadios_WithWhoCorrectsIt()
    {
        var cut = Open(Rectification() with { NameFromSignIn = "Kgosi Kgari SSO" });

        foreach (var id in new[] { "rec-first", "rec-last" })
        {
            cut.Find($"#{id}").HasAttribute("disabled").Should().BeTrue();
            Text(cut.Find($"#{id}-h")).Should().Be("This name comes from Kgosi Kgari SSO's sign-in; the institution corrects it.");
        }

        cut.Find("#rec-start").HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void RecordCorrectionWithNothingChosenOrTyped_RefusesEachUnderItsField_TheFocusOnTheFirstRadio()
    {
        var cut = Open(Rectification());

        Button(cut, "Record correction").Click();

        Text(cut.Find("#rec-err")).Should().Be("Choose the record.");
        cut.Find("fieldset.choice-group").ClassList.Should().Contain("is-invalid");
        cut.Find("fieldset.choice-group").GetAttribute("aria-describedby").Should().Be("rec-err");
        Text(cut.Find("#before-err")).Should().Be("Enter the value before.");
        Text(cut.Find("#after-err")).Should().Be("Enter the value after.");
        cut.Find("#before").GetAttribute("aria-invalid").Should().Be("true");
        cut.Find("#before").GetAttribute("aria-describedby").Should().Be("before-err");
        FocusedIds().Should().Equal("rec-first");
        Sender.SentOf<ApplyRectificationCommand>().Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "Name the record.")]
    [InlineData("   ", "Name the record.")]
    public void OthersName_IsRefusedUnderIt_WhenBlank(string name, string refusal)
    {
        var cut = Open(Rectification());

        cut.Find("#rec-other").Change(true);
        cut.Find("#rec-name").Change(name);
        cut.Find("#before").Change("a");
        cut.Find("#after").Change("b");
        Button(cut, "Record correction").Click();

        Text(cut.Find("#rec-name-err")).Should().Be(refusal);
        FocusedIds().Should().Equal("rec-name");
    }

    [Fact]
    public void OthersName_IsRefusedPast194Characters()
    {
        var cut = Open(Rectification());

        cut.Find("#rec-other").Change(true);
        cut.Find("#rec-name").Change(new string('n', 195));
        cut.Find("#before").Change("a");
        cut.Find("#after").Change("b");
        Button(cut, "Record correction").Click();

        Text(cut.Find("#rec-name-err")).Should().Be("Name the record in 194 characters or fewer.");
    }

    [Fact]
    public void ARecordedCorrection_HeadsCorrectionWithItsResult_ItsRowInTheTable_AndTheFormCleared()
    {
        var recorded = LastName();
        _command = request => request is ApplyRectificationCommand ? recorded : null;
        var cut = Open(Rectification(), Rectification(recorded) with { RequesterName = "Nomsa Mahlangu" });

        cut.Find("#rec-last").Change(true);
        cut.Find("#before").Change(" Mahlngu ");
        cut.Find("#after").Change("Mahlangu");
        Button(cut, "Record correction").Click();

        cut.WaitForAssertion(() => Text(Section(cut, "Correction").QuerySelector(".action-result")!).Should().Be(
            "Correction recorded: Account › Last name."));
        Sender.SentOf<ApplyRectificationCommand>().Single().Should().Match<ApplyRectificationCommand>(command =>
            command.TargetType == DataRightsRecords.AccountLastName && command.FromValue == "Mahlngu" && command.ToValue == "Mahlangu");
        var table = Section(cut, "Correction").QuerySelector("table")!;
        table.ClassList.Should().Contain("data-rights-table");
        Text(table.QuerySelector("caption")!).Should().Be("Corrections recorded on this request");
        Text(table.QuerySelector("tbody th[scope='row']")!).Should().Be("Account › Last name");
        table.QuerySelectorAll("tbody td").Select(cell => (cell.GetAttribute("data-label"), Text(cell))).Should().Equal(
            ("Value before", "Mahlngu"), ("Value after", "Mahlangu"), ("Recorded", "Pieter Smit · 2026-10-28 00:06 SAST"));
        cut.Find("#before").GetAttribute("value").Should().BeEmpty();
        cut.Find("#rec-last").HasAttribute("checked").Should().BeFalse();
        ElementFocusCalls().Should().Be(1);
    }

    [Fact]
    public void AFailedRecord_IsFixedWordsAtTheSectionsTop_TheValuesKept()
    {
        _command = request => request is ApplyRectificationCommand ? new TimeoutException("Npgsql") : null;
        var cut = Open(Rectification());

        cut.Find("#rec-last").Change(true);
        cut.Find("#before").Change("Mahlngu");
        cut.Find("#after").Change("Mahlangu");
        Button(cut, "Record correction").Click();

        cut.WaitForAssertion(() => Text(Section(cut, "Correction").QuerySelector(".action-result")!).Should().Be(
            "The correction could not be recorded just now. Nothing has changed. Try again, or come back in a few minutes."));
        cut.Find("#before").GetAttribute("value").Should().Be("Mahlngu");
        cut.Find("#after").GetAttribute("value").Should().Be("Mahlangu");
    }

    [Fact]
    public void CompleteWithNoneRecorded_IsRefusedBesideCompleteRequest_WhichKeepsTheFocus_AndNothingIsSent()
    {
        var cut = Open(Rectification());

        Button(cut, "Complete request").Click();

        var alert = cut.Find(".action-result .alert-danger");
        Text(alert).Should().Be("Record at least one correction before completing the request.");
        alert.GetAttribute("role").Should().Be("alert");
        alert.Closest(".action-result")!.NextElementSibling!.TextContent.Should().StartWith("Once every correction is made");
        ElementFocusCalls().Should().Be(0, "the focus stays on Complete request (C03)");
        Sender.SentOf<CompleteRectificationRequestCommand>().Should().BeEmpty();
    }

    [Fact]
    public void ACompletedRequest_HeadsCorrectionWithItsResult_AndOffersNoForm()
    {
        var recorded = LastName();
        _command = request => request is CompleteRectificationRequestCommand ? Dto(DataRightsRequestStatus.Completed) : null;
        var cut = Open(Rectification(recorded) with { RequesterName = "Nomsa Mahlangu" },
            Rectification(recorded) with { RequesterName = "Nomsa Mahlangu", Status = DataRightsRequestStatus.Completed, CompletedOn = Submitted.AddMinutes(6), MayRecord = false });

        Button(cut, "Complete request").Click();

        cut.WaitForAssertion(() => Text(Section(cut, "Correction").QuerySelector(".action-result")!).Should().Be(
            "Request completed. Nomsa Mahlangu is emailed, and reads it as Completed on My data rights."));
        Section(cut, "Correction").QuerySelectorAll("fieldset, .form-actions").Should().BeEmpty();
        Section(cut, "Correction").QuerySelector("table").Should().NotBeNull();
        Section(cut, "Decision").QuerySelectorAll("dt").Select(Text).Should().Contain("Completed on");
    }

    [Fact]
    public void ARejectedRectification_HasNoCorrectionSection()
    {
        var cut = Open(Rectification() with { Status = DataRightsRequestStatus.Rejected, MayRecord = false });

        Headings(cut).Should().Equal("Request", "Decision");
    }

    // ---- review P4: the note's length ----

    [Fact]
    public void ANoteOver4000Characters_IsRefusedUnderTheNoteInWords_BeforeAnySend_AndTheCounterShowsTheExcess()
    {
        var cut = Open(Export());

        cut.Find("#decision-note").Change(new string('n', 4001));
        Text(cut.Find("#decision-note-count")).Should().Be("4,001 of 4,000 characters");
        Button(cut, "Approve").Click();

        Text(cut.Find("#decision-note-err")).Should().Be("Give your note in 4,000 characters or fewer.");
        cut.Find("#decision-note").GetAttribute("aria-invalid").Should().Be("true");
        FocusedIds().Should().Equal("decision-note");
        Sender.SentOf<ApproveDataRightsRequestCommand>().Should().BeEmpty();

        // P6: the refusal goes once the note changes.
        cut.Find("#decision-note").Change("Identity confirmed.");
        cut.FindAll("#decision-note-err").Should().BeEmpty();
    }

    [Fact]
    public void ANote4000CharactersLong_IsSent()
    {
        _command = request => request is ApproveDataRightsRequestCommand ? new ApproveDataRightsResult(null!, null) : null;
        var cut = Open(Export(), Export(DataRightsRequestStatus.Completed));

        cut.Find("#decision-note").Change(new string('n', 4000));
        Button(cut, "Approve").Click();

        cut.WaitForAssertion(() => Sender.SentOf<ApproveDataRightsRequestCommand>().Should().ContainSingle());
    }

    // ---- review S3: the refusals no test reached ----

    [Fact]
    public void AForecastThatCannotBeRead_OpensNoDialog_AndDecideSaysSo_TheNoteKept()
    {
        _forecastThrows = new TimeoutException("Npgsql: timeout");
        var cut = Open(Erasure());

        cut.Find("#decision-note").Change("Identity confirmed.");
        cut.Find("#approve-and-erase").Click();

        cut.WaitForAssertion(() => Text(Section(cut, "Decide").QuerySelector(".action-result .alert-danger")!).Should().Be(
            DataRightsRequestWords.ForecastFailed.ToString()));
        Invoked("wombatDialog.showModal").Should().Be(0);
        Sender.SentOf<ApproveDataRightsRequestCommand>().Should().BeEmpty();
        cut.Find("#decision-note").GetAttribute("value").Should().Be("Identity confirmed.");
        cut.Markup.Should().NotContain("Npgsql");
    }

    [Fact]
    public void ANullForecast_WhileTheRequestMayStillBeDecided_IsTheForecastFailure()
    {
        _forecast = null;
        var cut = Open(Erasure());

        cut.Find("#decision-note").Change("Identity confirmed.");
        cut.Find("#approve-and-erase").Click();

        cut.WaitForAssertion(() => Text(Section(cut, "Decide").QuerySelector(".action-result")!).Should().Be(
            DataRightsRequestWords.ForecastFailed.ToString()));
        Invoked("wombatDialog.showModal").Should().Be(0);
    }

    [Fact]
    public void ANullForecast_ForARequestWithdrawnMeanwhile_ReadsThePageAgain_AsWithdrawn_AndOpensNoDialog()
    {
        _forecast = null;
        var cut = Open(Erasure());
        _page = Erasure(DataRightsRequestStatus.Withdrawn) with { MayDecide = false, DecidedByUserId = null, DecidedOn = null, DecisionNote = null };

        cut.Find("#decision-note").Change("Identity confirmed.");
        cut.Find("#approve-and-erase").Click();

        cut.WaitForAssertion(() => Headings(cut).Should().Equal("Request"));
        Text(Section(cut, "Request").QuerySelector(".action-result .alert-danger")!).Should().Be(
            "Sipho Ndlovu withdrew this request. Nothing was decided or erased.");
        Invoked("wombatDialog.showModal").Should().Be(0);
    }

    [Fact]
    public void ANullForecast_ForARequestDecidedMeanwhile_HeadsDecisionWithTheRefusal()
    {
        _forecast = null;
        var cut = Open(Erasure());
        _page = Erasure(DataRightsRequestStatus.Rejected) with { MayDecide = false };

        cut.Find("#decision-note").Change("Identity confirmed.");
        cut.Find("#approve-and-erase").Click();

        cut.WaitForAssertion(() => Headings(cut).Should().Equal("Request", "Decision"));
        Text(Section(cut, "Decision").QuerySelector(".action-result")!).Should().Be(
            "This request was decided while this page was open. Nothing more was done.");
    }

    [Fact]
    public void CompleteRefusedByTheCommand_AsNoLongerApproved_ReadsAgain_AndSaysSoAtTheSectionsTop()
    {
        var recorded = LastName();
        _command = request => request is CompleteRectificationRequestCommand
            ? new DataRightsRefusedException(DataRightsRefusal.NotAnApprovedRectification, "Only an approved Rectification request can be completed.")
            : null;
        var cut = Open(Rectification(recorded));

        Button(cut, "Complete request").Click();

        cut.WaitForAssertion(() => Text(Section(cut, "Correction").QuerySelector(".action-result .alert-danger")!).Should().Be(
            "This request is no longer waiting for a correction. Nothing was recorded."));
        Sender.SentOf<GetDataRightsRequestByIdQuery>().Should().HaveCount(2, "the page is read again");
        ElementFocusCalls().Should().Be(1);
        cut.Markup.Should().NotContain("Only an approved");
    }

    [Fact]
    public void CompleteRefusedByTheCommand_ForNoCorrection_IsRefusedBesideCompleteRequest_TheFocusKept()
    {
        var recorded = LastName();
        _command = request => request is CompleteRectificationRequestCommand
            ? new DataRightsRefusedException(DataRightsRefusal.NoCorrectionRecorded, "Record at least one correction before completing the request.")
            : null;
        var cut = Open(Rectification(recorded));

        Button(cut, "Complete request").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-danger")).Should().Be(
            "Record at least one correction before completing the request."));
        cut.Find(".action-result").NextElementSibling!.TextContent.Should().StartWith("Once every correction is made");
        ElementFocusCalls().Should().Be(0, "the focus stays on Complete request (C03)");
    }

    [Fact]
    public void ACompleteThatFails_IsFixedWords_NeverTheExceptionsText()
    {
        var recorded = LastName();
        _command = request => request is CompleteRectificationRequestCommand ? new TimeoutException("Npgsql: timeout") : null;
        var cut = Open(Rectification(recorded));

        Button(cut, "Complete request").Click();

        cut.WaitForAssertion(() => Text(Section(cut, "Correction").QuerySelector(".action-result .alert-danger")!).Should().Be(
            DataRightsRequestWords.CompleteFailed.ToString()));
        cut.Markup.Should().NotContain("Npgsql");
    }

    [Fact]
    public void ARecordRefusedByTheValidator_PutsEachRefusalUnderItsField_TheFocusInTheFirst_TheValuesKept()
    {
        _command = request => request is ApplyRectificationCommand
            ? new ValidationException([
                new ValidationFailure(nameof(ApplyRectificationCommand.FromValue), "Enter the value before."),
                new ValidationFailure(nameof(ApplyRectificationCommand.ToValue), "Enter the value after.")
            ])
            : null;
        var cut = Open(Rectification());

        cut.Find("#rec-last").Change(true);
        cut.Find("#before").Change("Mahlngu");
        cut.Find("#after").Change("Mahlangu");
        Button(cut, "Record correction").Click();

        cut.WaitForAssertion(() => Text(cut.Find("#before-err")).Should().Be("Enter the value before."));
        Text(cut.Find("#after-err")).Should().Be("Enter the value after.");
        FocusedIds().Should().Equal("before");
        cut.Find("#before").GetAttribute("value").Should().Be("Mahlngu");
        cut.FindAll(".action-result").Should().BeEmpty();

        // P6: a refusal goes once its field changes.
        cut.Find("#before").Change("Mahlngu ");
        cut.FindAll("#before-err").Should().BeEmpty();
        cut.FindAll("#after-err").Should().ContainSingle();
    }

    [Fact]
    public void ARecordRefusedByTheValidatorOnTheRecord_WithOtherChosen_IsUnderOthersName()
    {
        _command = request => request is ApplyRectificationCommand
            ? new ValidationException([new ValidationFailure(nameof(ApplyRectificationCommand.TargetType), "Name the record in 194 characters or fewer.")])
            : null;
        var cut = Open(Rectification());

        cut.Find("#rec-other").Change(true);
        cut.Find("#rec-name").Change("Home address");
        cut.Find("#before").Change("a");
        cut.Find("#after").Change("b");
        Button(cut, "Record correction").Click();

        cut.WaitForAssertion(() => Text(cut.Find("#rec-name-err")).Should().Be("Name the record in 194 characters or fewer."));
        FocusedIds().Should().Equal("rec-name");
    }

    [Fact]
    public void ARecordRefusedByTheValidatorOnTheRecord_WithAFixedRecord_IsUnderTheRecord_TheRadiosInvalid()
    {
        _command = request => request is ApplyRectificationCommand
            ? new ValidationException([new ValidationFailure(nameof(ApplyRectificationCommand.TargetType), "Choose the record.")])
            : null;
        var cut = Open(Rectification());

        cut.Find("#rec-last").Change(true);
        cut.Find("#before").Change("a");
        cut.Find("#after").Change("b");
        Button(cut, "Record correction").Click();

        cut.WaitForAssertion(() => Text(cut.Find("#rec-err")).Should().Be("Choose the record."));
        cut.FindAll("input[name='rec']").Should().OnlyContain(radio => radio.GetAttribute("aria-invalid") == "true");
        FocusedIds().Should().Equal("rec-first");
    }

    [Fact]
    public void ARecordRefusedByTheValidatorOnNoField_IsTheFixedFailure()
    {
        _command = request => request is ApplyRectificationCommand
            ? new ValidationException([new ValidationFailure(nameof(ApplyRectificationCommand.Principal), "required")])
            : null;
        var cut = Open(Rectification());

        cut.Find("#rec-last").Change(true);
        cut.Find("#before").Change("a");
        cut.Find("#after").Change("b");
        Button(cut, "Record correction").Click();

        cut.WaitForAssertion(() => Text(Section(cut, "Correction").QuerySelector(".action-result .alert-danger")!).Should().Be(
            DataRightsRequestWords.CorrectionFailed.ToString()));
    }

    [Fact]
    public void ARecordRefusedAsNoLongerApproved_ReadsAgain_AndSaysSo()
    {
        _command = request => request is ApplyRectificationCommand
            ? new DataRightsRefusedException(DataRightsRefusal.NotAnApprovedRectification, "A correction is recorded only on an approved Rectification request.")
            : null;
        var cut = Open(Rectification());

        cut.Find("#rec-last").Change(true);
        cut.Find("#before").Change("a");
        cut.Find("#after").Change("b");
        Button(cut, "Record correction").Click();

        cut.WaitForAssertion(() => Text(Section(cut, "Correction").QuerySelector(".action-result .alert-danger")!).Should().Be(
            "This request is no longer waiting for a correction. Nothing was recorded."));
        Sender.SentOf<GetDataRightsRequestByIdQuery>().Should().HaveCount(2, "the page is read again");
    }
}
