using Bunit;
using FluentAssertions;
using Wombat.Application.Features.DataRights;
using Wombat.Application.Features.DataRights.Queries;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.DataRights;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// Data rights requests, the queue (T387, flow 14, lane P; boards R3-Q01–Q13, K02): whose requests it is, the filter by
/// label with Show and Clear filters, the table's row header and labelled cells, the Administrator's Institution column,
/// the pager only past one page, the empty sentences, and the load failure's fixed words.
/// </summary>
public sealed class DataRightsQueuePageTests : DataRightsStaffPageTestBase
{
    private static readonly Guid DlaminiExport = Guid.Parse("01a17f61-f16f-77c1-860c-661586d5b733");

    private static DataRightsRequestSummaryDto Dlamini(DataRightsRequestStatus status = DataRightsRequestStatus.Submitted) => new(
        DlaminiExport, "Anele Dlamini", Submitted, DataRightsRequestType.Export, status)
    {
        RequesterEmail = "dlamini@kgk.wombat.local",
        InstitutionName = Kgk
    };

    private static PagedDataRightsResult Kgks(params DataRightsRequestSummaryDto[] rows)
        => new(rows, rows.Length, 1, 20) { InstitutionName = Kgk };

    private static PagedDataRightsResult EveryInstitution(params DataRightsRequestSummaryDto[] rows)
        => new(rows, rows.Length, 1, 20) { EveryInstitution = true };

    private IRenderedComponent<RequestsList> Open(Func<ListDataRightsRequestsQuery, PagedDataRightsResult> answer)
    {
        Sender.Answer = request => request is ListDataRightsRequestsQuery query ? answer(query) : null;
        var cut = RenderComponent<RequestsList>();
        cut.WaitForState(() => cut.FindAll(".loading-line").Count == 0);
        return cut;
    }

    [Fact]
    public void ACoordinator_ReadsTheInstitutionsRequests_TheRequesterByNameOverTheAddress_AndOneReviewNamedForItsRow()
    {
        var cut = Open(_ => Kgks(Dlamini()));

        Text(cut.Find("h1")).Should().Be("Data rights requests");
        Text(cut.Find(".page-subtitle")).Should().Be(
            "Requests from people at Kgosi Kgari Teaching Hospital about their personal data: decide each, and carry an approved correction through.");
        cut.Find("form.search-container").GetAttribute("aria-label").Should().Be("Filter Data rights requests");
        cut.FindAll("#f-type option").Select(Text).Should().Equal("All types", "Access", "Export", "Rectification", "Erasure");
        cut.FindAll("#f-status option").Select(Text).Should().Equal(
            "All statuses", "Submitted", "Approved", "Completed", "Rejected", "Withdrawn");
        cut.FindAll(".filter-actions .btn").Select(Text).Should().Equal(["Show"], "Clear filters only once a filter is set (S18)");
        cut.FindAll(".page-header .btn, .header-container .btn").Should().BeEmpty("Apply filters in the header is gone (T328)");

        Text(cut.Find("h2#rq-h")).Should().Be("Requests (1)");
        cut.Find("h2#rq-h").GetAttribute("tabindex").Should().Be("-1");
        var table = cut.Find("table");
        table.ClassList.Should().Contain(["clinic-table--stack", "clinic-table--stack-wide", "data-rights-table"]);
        Text(cut.Find("caption")).Should().Be("Kgosi Kgari Teaching Hospital's data rights requests, newest first");
        cut.FindAll("thead th").Select(Text).Should().Equal("Submitted", "Requester", "Type", "Status", "Request id", "Actions");

        var row = cut.Find("tbody tr");
        row.QuerySelectorAll("td[data-label]").Select(cell => cell.GetAttribute("data-label")).Should().Equal(
            "Submitted", "Type", "Status", "Request id");
        Text(row.QuerySelector("td[data-label='Submitted']")!).Should().Be("2026-10-28 00:01 SAST");
        var requester = row.QuerySelector("th[scope='row'] .person-cell")!;
        requester.FirstChild!.TextContent.Should().Be("Anele Dlamini");
        requester.QuerySelector("span")!.InnerHtml.Should().Be("dlamini@<wbr>kgk.wombat.local");
        row.QuerySelector(".badge")!.ClassList.Should().Contain(BadgeFor.DataRightsRequest(DataRightsRequestStatus.Submitted));
        Text(row.QuerySelector(".request-id")!).Should().Be(DlaminiExport.ToString());
        var review = row.QuerySelector("a.btn")!;
        Text(review).Should().Be("Review");
        review.GetAttribute("href").Should().Be($"/admin/data-rights/{DlaminiExport}");
        review.GetAttribute("aria-label").Should().Be("Review the Export request from Anele Dlamini, 2026-10-28 00:01 SAST");
        cut.FindAll(".pager").Should().BeEmpty("one page needs no pager (F10)");
        cut.Markup.Should().NotContain("UnderReview").And.NotContain("→").And.NotContain("←");
    }

    [Fact]
    public void TheAdministrator_ReadsEveryInstitution_WithAnInstitutionColumn_NoneForARequestWithNone()
    {
        SignIn(WombatRoles.Administrator);
        var cut = Open(_ => EveryInstitution(Dlamini(), Dlamini() with { Id = Guid.NewGuid(), InstitutionName = null }));

        Text(cut.Find(".page-subtitle")).Should().StartWith("Requests from people at every institution about their personal data");
        Text(cut.Find("caption")).Should().Be("Every institution's data rights requests, newest first");
        cut.FindAll("thead th").Select(Text).Should().Equal(
            "Submitted", "Requester", "Institution", "Type", "Status", "Request id", "Actions");
        cut.FindAll("td[data-label='Institution']").Select(Text).Should().Equal(Kgk, "None");
    }

    [Fact]
    public void Show_SendsTheFilterByItsValues_OffersClearFilters_AndAnEmptyAnswerSaysWhatWasAsked_TheHeadingFocused()
    {
        var cut = Open(query => query.Type is null ? Kgks(Dlamini()) : Kgks());
        var focusBefore = ElementFocusCalls();

        cut.Find("#f-type").Change(nameof(DataRightsRequestType.Erasure));
        cut.Find("form.search-container").Submit();

        cut.WaitForAssertion(() => Text(cut.Find("h2#rq-h")).Should().Be("Requests (0)"));
        Sender.SentOf<ListDataRightsRequestsQuery>().Last().Should().Match<ListDataRightsRequestsQuery>(
            query => query.Type == DataRightsRequestType.Erasure && query.Status == null && query.Page == 1);
        Text(cut.Find(".detail-card--empty .state-panel-title")).Should().Be("No requests match");
        Text(cut.Find(".detail-card--empty .state-panel-copy")).Should().Be(
            "No Erasure request at Kgosi Kgari Teaching Hospital. Clear filters to see every request.");
        cut.FindAll(".filter-actions .btn").Select(Text).Should().Equal("Show", "Clear filters");
        ElementFocusCalls().Should().Be(focusBefore + 1, "Show focuses \"Requests (n)\"");
    }

    [Fact]
    public void TheCaption_CarriesTheFiltersTail_AndClearFilters_ReadsEveryRequestAgain()
    {
        var cut = Open(_ => Kgks(Dlamini()));

        cut.Find("#f-type").Change(nameof(DataRightsRequestType.Export));
        cut.Find("#f-status").Change(nameof(DataRightsRequestStatus.Submitted));
        cut.Find("form.search-container").Submit();
        cut.WaitForAssertion(() => Text(cut.Find("caption")).Should().Be(
            "Kgosi Kgari Teaching Hospital's data rights requests, newest first: Export, Submitted"));

        Button(cut, "Clear filters").Click();

        cut.WaitForAssertion(() => Text(cut.Find("caption")).Should().Be("Kgosi Kgari Teaching Hospital's data rights requests, newest first"));
        Sender.SentOf<ListDataRightsRequestsQuery>().Last().Should().Match<ListDataRightsRequestsQuery>(
            query => query.Type == null && query.Status == null && query.Page == 1);
        cut.FindAll(".filter-actions .btn").Select(Text).Should().Equal("Show");
    }

    [Theory]
    [InlineData(nameof(DataRightsRequestStatus.Completed), null, "No request is Completed at Kgosi Kgari Teaching Hospital.")]
    [InlineData(nameof(DataRightsRequestStatus.Rejected), nameof(DataRightsRequestType.Access), "No Access request is Rejected at Kgosi Kgari Teaching Hospital.")]
    public void AnEmptyFilteredAnswer_SaysWhatWasAsked(string status, string? type, string sentence)
    {
        var cut = Open(query => query.Status is null ? Kgks(Dlamini()) : Kgks());

        cut.Find("#f-status").Change(status);
        if (type is not null)
        {
            cut.Find("#f-type").Change(type);
        }

        cut.Find("form.search-container").Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".detail-card--empty .state-panel-copy")).Should().Be(
            $"{sentence} Clear filters to see every request."));
    }

    [Fact]
    public void TheAdministratorsEmptyFilteredAnswer_SaysAnyInstitution()
    {
        SignIn(WombatRoles.Administrator);
        var cut = Open(query => query.Type is null ? EveryInstitution(Dlamini()) : EveryInstitution());

        cut.Find("#f-type").Change(nameof(DataRightsRequestType.Erasure));
        cut.Find("form.search-container").Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".detail-card--empty .state-panel-copy")).Should().Be(
            "No Erasure request at any institution. Clear filters to see every request."));
    }

    [Fact]
    public void ACoordinatorWithNoInstitution_ReadsTheSentenceWithoutAPlace_AndWhyNoRequestIsTheirs()
    {
        var cut = Open(_ => new PagedDataRightsResult([], 0, 1, 20));

        Text(cut.Find(".page-subtitle")).Should().Be(
            "Requests about personal data: decide each, and carry an approved correction through.");
        Text(cut.Find(".detail-card--empty .state-panel-title")).Should().Be("No requests");
        Text(cut.Find(".detail-card--empty .state-panel-copy")).Should().Be(
            "Your account is not set to an institution, so no request is yours to decide.");
    }

    [Fact]
    public void ThePager_IsDrawnOnlyPastOnePage_ItsSizeNamedForRequests_AndATurnReadsThatPage_TheHeadingFocused()
    {
        var rows = Enumerable.Range(0, 20)
            .Select(index => Dlamini() with { Id = Guid.NewGuid(), RequestedOn = Submitted.AddMinutes(-index) })
            .ToArray();
        var cut = Open(query => new PagedDataRightsResult(rows, 50, query.Page, query.PageSize) { EveryInstitution = true });

        var pager = cut.Find("nav.pager");
        pager.GetAttribute("aria-label").Should().Be("Requests, pages");
        Text(pager.QuerySelector(".pager-info")!).Should().Be("Showing 1–20 of 50");
        Text(pager.QuerySelector("label")!).Should().Be("Requests per page");
        pager.QuerySelectorAll("button").Select(Text).Should().Equal("Previous", "Next");
        var focusBefore = ElementFocusCalls();

        cut.FindAll("nav.pager button").Single(button => Text(button) == "Next").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".pager-info")).Should().Be("Showing 21–40 of 50"));
        Sender.SentOf<ListDataRightsRequestsQuery>().Last().Page.Should().Be(2);
        ElementFocusCalls().Should().Be(focusBefore + 1, "a page turn focuses \"Requests (50)\" (Q10)");
    }

    [Fact]
    public void AnErasedRequester_ReadsAsThePseudonymOverTheDayErased_WithNoAddress()
    {
        var cut = Open(_ => Kgks(Dlamini(DataRightsRequestStatus.Completed) with
        {
            RequesterName = "deleted_user_0ae1d64b",
            RequesterEmail = null,
            ErasedOn = new DateOnly(2026, 10, 28),
            Type = DataRightsRequestType.Erasure
        }));

        var requester = cut.Find("th[scope='row'] .person-cell");
        requester.FirstChild!.TextContent.Should().Be("deleted_user_0ae1d64b");
        Text(requester.QuerySelector("span")!).Should().Be("Erased 2026-10-28");
        cut.Find("tbody a.btn").GetAttribute("aria-label").Should().Be(
            "Review the Erasure request from deleted_user_0ae1d64b, 2026-10-28 00:01 SAST");
    }

    [Fact]
    public void TwoRequestsOfOneTypeFromOnePersonInOneMinute_AreNamedToTheSecond()
    {
        var later = Dlamini() with { Id = Guid.NewGuid(), RequestedOn = Submitted.AddSeconds(42) };
        var cut = Open(_ => Kgks(later, Dlamini()));

        cut.FindAll("tbody a.btn").Select(link => link.GetAttribute("aria-label")).Should().Equal(
            "Review the Export request from Anele Dlamini, 2026-10-28 00:01:42 SAST",
            "Review the Export request from Anele Dlamini, 2026-10-28 00:01:00 SAST");
    }

    [Fact]
    public void AFailedRead_IsFixedWordsWithTryAgain_NeverTheExceptionsText()
    {
        Sender.Answer = _ => new InvalidOperationException("Npgsql: connection refused");
        var cut = RenderComponent<RequestsList>();

        cut.WaitForAssertion(() => cut.Find(".alert-danger"));
        Text(cut.Find(".alert-danger .alert-row-text")).Should().Be(
            "Could not load the data rights requests. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("Npgsql");
        cut.FindAll("form.search-container").Should().BeEmpty("nothing the read would have filled is drawn");

        Sender.Answer = request => request is ListDataRightsRequestsQuery ? Kgks(Dlamini()) : null;
        Button(cut, "Try again").Click();

        cut.WaitForAssertion(() => Text(cut.Find("h2#rq-h")).Should().Be("Requests (1)"));
        Invoked("wombat.focusHeading").Should().Be(1, "after a Try again that read, the h1 (N15)");
    }

    [Fact]
    public void WhileTheFirstReadRuns_TheLoadingLineAndSkeletons_NoFilter()
    {
        Sender.Held = request => request is ListDataRightsRequestsQuery;
        Sender.Answer = request => request is ListDataRightsRequestsQuery ? Kgks(Dlamini()) : null;
        var cut = RenderComponent<RequestsList>();

        Text(cut.Find(".loading-line")).Should().Be("Loading the data rights requests.");
        cut.FindAll("form.search-container").Should().BeEmpty();
        cut.FindAll(".detail-card[aria-hidden='true']").Should().HaveCount(2);

        Sender.Gate.SetResult();
        cut.WaitForAssertion(() => cut.Find("h2#rq-h"), Wombat.Web.Tests.TestSupport.WombatTestContext.AsyncWorkTimeout);
    }

    [Fact]
    public void Show_APageTurn_AndAChangeOfSize_AnswerInPlace_TheFormTableAndPagerKept_TheSizeKeepingTheFocus()
    {
        // T387 review P2: as Users does, only the first read and Try again draw the loading state.
        var rows = Enumerable.Range(0, 20)
            .Select(index => Dlamini() with { Id = Guid.NewGuid(), RequestedOn = Submitted.AddMinutes(-index) })
            .ToArray();
        var cut = Open(query => new PagedDataRightsResult(rows, 50, query.Page, query.PageSize) { EveryInstitution = true });
        var focusBefore = ElementFocusCalls();
        Sender.Held = request => request is ListDataRightsRequestsQuery;

        cut.Find("nav.pager select").Change("50");

        cut.FindAll(".loading-line").Should().BeEmpty("the list answers in place");
        cut.FindAll("form.search-container").Should().ContainSingle();
        cut.FindAll("nav.pager select").Should().ContainSingle();
        cut.FindAll("table").Should().ContainSingle();
        Sender.Gate.SetResult();
        cut.WaitForAssertion(() => Sender.SentOf<ListDataRightsRequestsQuery>().Last().PageSize.Should().Be(50));
        ElementFocusCalls().Should().Be(focusBefore, "a change of size leaves the focus on the select");
    }
}
