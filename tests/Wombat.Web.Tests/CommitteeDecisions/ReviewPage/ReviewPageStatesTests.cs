using System.Text.RegularExpressions;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityById;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Web.Components.Shared;
using Wombat.Web.Components.Shared.Activities;
using Wombat.Web.Services;
using Wombat.Web.Tests.Activities;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.CommitteeDecisions.ReviewPage;

/// <summary>
/// T361 (flow 07, round 3): the page's own states (board 8; T329), the notes card (13, 31), the snapshot (Q6; 23, 30),
/// Read in place (C3; 8), and the rules every shipped word keeps: no pronoun (round-3-check 3), no deferral (D53).
/// </summary>
public sealed partial class ReviewPageStatesTests : ReviewPageTestBase
{
    // ─── Loading, load error, not found (board 8, T329) ──────────────────────────

    [Fact]
    public void WhileTheReviewIsRead_TheHeaderSaysCommitteeReview_WithASkeleton_AndNoAction()
    {
        var gate = new TaskCompletionSource<CommitteeReviewDetailDto>();
        Services.AddSingleton<IScopedSender>(new GatedSender(gate.Task));

        var cut = RenderComponent<Wombat.Web.Components.Pages.CommitteeDecisions.ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, ReviewId));

        Text(cut.Find("h1")).Should().Be("Committee review");
        TabTitle.Of(this, cut).Should().Be("Committee review · Wombat");
        Text(cut.Find("p[role=status]")).Should().Be("Loading the review…");
        cut.FindAll(".skeleton").Should().NotBeEmpty();
        cut.FindAll("button").Should().BeEmpty("no action until loaded");
    }

    [Fact]
    public void AFailedRead_SaysOneFixedSentence_WithTryAgain_NeverTheExceptionsText()
    {
        Sender.On<GetCommitteeReviewByIdQuery>(_ => throw new InvalidOperationException("Npgsql exploded: secret detail"));

        var cut = RenderComponent<Wombat.Web.Components.Pages.CommitteeDecisions.ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, ReviewId));

        Text(cut.Find("h1")).Should().Be("Committee review");
        Text(cut.Find(".alert-danger .alert-row-text")).Should().Be("The review could not be loaded. Nothing on it has changed.");
        Buttons(cut).Should().Contain("Try again");
        Page(cut).Should().NotContain("Npgsql").And.NotContain("secret");
    }

    [Fact]
    public void AReviewOutOfReach_IsNotFound_InTheReviewsOwnSentence_WithTheWayBack()
    {
        Sender.On<GetCommitteeReviewByIdQuery>(_ => throw new UnauthorizedAccessException(
            "The committee review could not be found among the reviews you can view."));

        var cut = RenderComponent<Wombat.Web.Components.Pages.CommitteeDecisions.ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, ReviewId));

        Text(cut.Find("h1")).Should().Be("Page not found");
        TabTitle.Of(this, cut).Should().Be("Page not found · Wombat");
        Text(cut.Find(".system-panel p")).Should().Be("The committee review could not be found among the reviews you can view.");
        var back = cut.Find(".system-panel a");
        Text(back).Should().Be("Back to Committee reviews");
        back.GetAttribute("href").Should().Be("/committee/reviews");
    }

    // ─── The notes card (13, 31) ─────────────────────────────────────────────────

    [Fact]
    public void TheNotesCard_ListsEachEpasSamplingLine_MarksOneNoLongerInUse_AndSaysWhatIsNotCounted()
    {
        Sender.On<GetSamplingConcentrationWarningsQuery>(_ => new SamplingConcentrationReportDto(
            ReviewId, 3, 2, true,
            [
                new EpaSamplingConcentrationDto(10, "PAED-010", "Leading", 1, 1, 1, Chair, 1, false, false, true),
                new EpaSamplingConcentrationDto(12, "PAED-012", "Counselling", 2, 2, 2, Chair, 1, false, false, true) { EpaInForce = false }
            ],
            0, 0, 2));
        var cut = Render(Review(CommitteeReviewState.Scheduled));

        Text(cut.Find("#notes h2")).Should().Be("Before the committee decides");
        Text(cut.Find("#notes h3")).Should().Be("Sampling concentration warnings");
        cut.FindAll("#notes li").Select(li => Text(li)).Should().Equal(
            "PAED-010 · 1 rating · 1 assessor · 1 source. Fewer than three distinct assessors across this EPA's evidence.",
            "PAED-012 (no longer in use) · 2 ratings · 2 assessors · 2 sources. Fewer than three distinct assessors across this EPA's evidence.");
        Text(cut.Find("#notes p.muted")).Should().StartWith("Not counted: 2 records in the review window with no rating by a named assessor");
        Buttons(cut).Should().NotContain("×", "the warnings are not dismissible");
    }

    [Fact]
    public void WithNothingToSay_ThereIsNoNotesCard()
    {
        var cut = Render(Review(CommitteeReviewState.Scheduled));

        cut.FindAll("#notes").Should().BeEmpty();
    }

    // ─── The snapshot (Q6; 23, 30) ───────────────────────────────────────────────

    [Fact]
    public void TheSnapshot_SaysWhenItWasFrozen_ByEpaThenInstrumentAtoZ_EachLineGoingToFlow03WithTheWayBack()
    {
        var cut = Render(Review());

        Text(cut.Find("#evidence-snapshot h2")).Should().Be("Evidence snapshot");
        Text(cut.Find("#evidence-snapshot p.muted")).Should().Be("Frozen when the review started, 2026-10-05 · 6 items");
        cut.FindAll("#evidence-snapshot h3").Select(Text).Should().Equal(
            "PAED-001 — Providing paediatric emergency care to children",
            "PAED-010 — Leading and operating within a clinical team",
            "Not about a single EPA");
        cut.FindAll("#evidence-epa-1 + .table-container thead th").Select(Text).Should().Equal(
            "Item", "Assessor", "Rating", "Encounter", "State");
        cut.FindAll("#evidence-epa-1 + .table-container tbody th").Select(Text).Should().Equal(
            "Case-Based Discussion (Paediatrics) #13", "DOPS (Paediatrics) #14", "Mini-CEX (Paediatrics) #12");

        var line = cut.Find("#snap-12");
        Text(line).Should().Be("Mini-CEX (Paediatrics) #12 Thandi Zulu 5 2026-09-23 Completed");
        line.QuerySelector("a")!.GetAttribute("href").Should().Be("/activities/12?from=review-30&at=snap-12");
        Text(cut.Find("#snap-1")).Should().Be("Default MSF #1 · Released · 6 responses");
    }

    [Fact]
    public void BeforeStart_TheSnapshotSaysNothingIsFrozenYet()
    {
        var cut = Render(Review(CommitteeReviewState.Scheduled));

        Text(cut.Find("#evidence-snapshot")).Should().Contain("No evidence has been frozen yet. Starting the review captures the current bundle.");
    }

    // ─── Read, in place (C3; 8) ──────────────────────────────────────────────────

    [Fact]
    public void Read_SwapsTheFormForTheActivityAsItIsNow_WithNoAction_AndBackRestoresTheFormAsTyped()
    {
        Services.AddSingleton<IActivityReferenceDataService>(new StubActivityReferenceDataService());
        Sender.On<GetActivityByIdQuery>(_ => ActivityDetail("completed", "Completed"));
        var cut = Render(Review());
        cut.Find("#agenda-stage-1").Click();
        cut.Find("#stage-1-rationale").Change("Leads resuscitation.");
        cut.Find("#stage-1-evidence-12").Change(true);

        cut.Find("#stage-1-read-12").Click();

        cut.FindAll("#stage-1-rationale").Should().BeEmpty("the reader takes the form's place");
        var reader = cut.Find("#agenda-form-1 .evidence-reader");
        Text(reader.QuerySelector("h3")!).Should().Be("Mini-CEX (Paediatrics) #12 · PAED-001");
        FocusedIds().Should().EndWith("stage-1-reader-heading");
        reader.QuerySelectorAll(".evidence-reader-moved").Should().BeEmpty("unchanged since Start");
        reader.QuerySelectorAll(".activity-status").Should().ContainSingle();
        reader.QuerySelectorAll(".activity-status-action").Should().BeEmpty("no action and no move (8)");
        cut.FindAll(".chart-back").Should().BeEmpty();

        cut.Find("#stage-1-reader-back").Click();

        cut.Find("#stage-1-rationale").GetAttribute("value").Should().Be("Leads resuscitation.");
        cut.Find("#stage-1-evidence-12").HasAttribute("checked").Should().BeTrue();
        FocusedIds().Should().EndWith("stage-1-read-12");
    }

    [Fact]
    public void Read_KeepsTheHeadingOutline_TheReaderAnH3UnderAgenda_TheActivitysAboutAndSectionsH4UnderIt()
    {
        // T361, review D1: the reader sits under the page's h2 Agenda, and the activity it embeds opens About and its form
        // sections, h2 on the activity's own page, one level under the reader's heading here.
        Services.AddSingleton<IActivityReferenceDataService>(new StubActivityReferenceDataService());
        Sender.On<GetActivityByIdQuery>(_ => ActivityDetail("completed", "Completed"));
        var cut = Render(Review());
        cut.Find("#agenda-stage-1").Click();

        cut.Find("#stage-1-read-12").Click();

        var reader = cut.Find("#agenda-form-1 .evidence-reader");
        var outline = reader.QuerySelectorAll("h1, h2, h3, h4, h5, h6")
            .Select(heading => (heading.TagName, Text(heading)))
            .ToList();
        outline.Should().Equal(
            ("H3", "Mini-CEX (Paediatrics) #12 · PAED-001"),
            ("H4", "About"),
            ("H4", "About"));
        reader.QuerySelector("#activity-about-title")!.TagName.Should().Be("H4", "the About card");
        reader.QuerySelector($"#{ActivityFieldIds.Section("about")}")!.TagName.Should().Be("H4", "the form's section");
    }

    [Fact]
    public void Read_OnAnItemThatMovedSinceStart_SaysSoUnderItsHeading()
    {
        Services.AddSingleton<IActivityReferenceDataService>(new StubActivityReferenceDataService());
        Sender.On<GetActivityByIdQuery>(_ => ActivityDetail("completed", "Completed"));
        var review = Review();
        review = review with
        {
            EvidenceItems = review.EvidenceItems
                .Select(item => item.Id == 12 ? item with { SourceStateLabel = "Awaiting review", SourceState = "submitted" } : item)
                .ToArray()
        };
        var cut = Render(review);
        cut.Find("#agenda-stage-1").Click();

        cut.Find("#stage-1-read-12").Click();

        Text(cut.Find(".evidence-reader-moved")).Should().Be("Now Completed. Frozen at Start as Awaiting review.");
    }

    // ─── The words every board keeps ─────────────────────────────────────────────

    [Theory]
    [InlineData(CommitteeReviewState.Scheduled, true)]
    [InlineData(CommitteeReviewState.InProgress, true)]
    [InlineData(CommitteeReviewState.InProgress, false)]
    [InlineData(CommitteeReviewState.Decided, true)]
    [InlineData(CommitteeReviewState.Decided, false)]
    [InlineData(CommitteeReviewState.Ratified, false)]
    public void NoShippedWord_IsAPronoun_OrADeferral(CommitteeReviewState state, bool chair)
    {
        if (!chair)
        {
            SignInAs(Naidoo);
        }

        var cut = Render(Review(state, callerChairs: chair), [Pending(1, "5", [12])]);
        if (chair && state == CommitteeReviewState.InProgress)
        {
            cut.Find("#agenda-stage-2").Click();
        }

        var words = Page(cut);
        Pronoun().Matches(words).Select(match => match.Value).Should().BeEmpty("round-3-check 3: the person's name, the chair, the registrar");
        // "Outcome Deferred" is a progression category, not a deferral, and stays (D53).
        words.Replace("Outcome Deferred", string.Empty, StringComparison.Ordinal)
            .Should().NotContainEquivalentOf("defer", "there is no deferral (D53)").And.NotContain("Reinstate");
    }

    [GeneratedRegex(@"\b(she|he|her|hers|herself|him|his|himself)\b", RegexOptions.IgnoreCase)]
    private static partial Regex Pronoun();

    private static ActivityDetailDto ActivityDetail(string state, string label)
    {
        const string schema = """
            { "version": 1, "sections": [ { "key": "about", "title": "About", "fields": [ { "key": "note", "type": "text", "label": "Note" } ] } ] }
            """;
        const string workflow = """
            {
              "version": 1,
              "initial_state": "completed",
              "states": [ { "key": "completed", "label": "Completed", "terminal": true } ],
              "transitions": []
            }
            """;
        var activity = new ActivityDto(
            12, 2, "mini_cex_cpsa", "Mini-CEX (Paediatrics)", "mini_cex", 1, schema, workflow, "[]",
            """{ "counts_for": [] }""", "molefe", 7, Chair, state, label, "{}", null, null, new DateOnly(2026, 9, 23), true,
            new DateTime(2026, 9, 23, 8, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 23, 8, 0, 0, DateTimeKind.Utc), []);
        return new ActivityDetailDto(activity, [], []);
    }

    /// <summary>A sender whose review read never answers, for the loading state.</summary>
    private sealed class GatedSender(Task<CommitteeReviewDetailDto> review) : IScopedSender
    {
        public async Task<TResponse> Send<TResponse>(MediatR.IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request is GetCommitteeReviewByIdQuery
                ? (TResponse)(object)await review
                : throw new NotSupportedException();

        public Task Send(MediatR.IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
