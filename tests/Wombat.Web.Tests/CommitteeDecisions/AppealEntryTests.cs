using System.Text.RegularExpressions;
using Bunit;
using FluentAssertions;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Web.Components.Pages.CommitteeDecisions.Review;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// One appeal, the same on both review pages (T366, flow 09; contract § 1; boards R3-2, R3-3; T308, S10): the committee's
/// heading names who lodged it, the trainee's says it is hers; Open names who hears it, or nobody when nobody can act; a
/// resolved one says its outcome and who resolved it; the reason is clamped, with "Show the whole reason" only when it
/// overflows.
/// </summary>
public sealed partial class AppealEntryTests : TestContext
{
    private const string Reason = "The single DOPS reflects the start of the year. I ask for reconsideration.";

    public AppealEntryTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void OnTheCommitteesPage_AnOpenAppeal_IsHeadedByWhoLodgedIt_AndNamesWhoHearsIt()
    {
        var cut = Render(Open(), UnderAppeal(Body("zulu", "Thandi Zulu", DecisionPanelMemberRole.Chair),
            Body("rensburg", "John van Rensburg", DecisionPanelMemberRole.External)));

        var entry = cut.Find("li.appeal-entry");
        Text(entry.QuerySelector("h3.appeal-entry-head")!).Should().Be("Appeal by Nomsa Mahlangu Open");
        entry.QuerySelector(".badge")!.ClassList.Should().Contain("badge-accepted");
        Text(entry.QuerySelector(".appeal-entry-meta")!).Should().Be("Lodged 2026-10-13");
        Text(entry.QuerySelector(".appeal-entry-outcome")!).Should().Be(
            "Heard by Thandi Zulu (chair) and John van Rensburg (external).");
        Text(entry.QuerySelector(".appeal-entry-label")!).Should().Be("Reason");
        var reason = entry.QuerySelector(".appeal-entry-reason")!;
        Text(reason).Should().Be(Reason);
        reason.ClassList.Should().Contain("appeal-entry-reason--clamped");
    }

    [Fact]
    public void AnOpenAppeal_NobodyCanHear_NamesNobody()
    {
        // Board 3 c (C12a): the chair's seat lapsed and no external member may sit.
        var cut = Render(Open(), UnderAppeal());

        cut.FindAll(".appeal-entry-outcome").Should().BeEmpty();
    }

    [Theory]
    [InlineData(CommitteeAppealOutcome.Dismissed, false, "Dismissed: the decision stands.")]
    [InlineData(CommitteeAppealOutcome.Dismissed, true, "Dismissed: the decision stands, and it is final.")]
    [InlineData(CommitteeAppealOutcome.Remitted, false, "Remitted: the appeal body replaced the decision. The replacement decision is above.")]
    [InlineData(CommitteeAppealOutcome.Remitted, true, "Remitted: the appeal body replaced the decision. The decision above is theirs, and it is final.")]
    public void AResolvedAppeal_SaysItsOutcome_AndWhoResolvedIt(CommitteeAppealOutcome outcome, bool forTrainee, string sentence)
    {
        var appeal = Open() with { ResolvedOn = Utc(2026, 10, 13, 22), ResolvedByUserId = "zulu", Outcome = outcome, ResolvedByName = "Thandi Zulu" };

        var cut = Render(appeal, Review(CommitteeReviewState.Final), forTrainee);

        Text(cut.Find("h3")).Should().Be($"{(forTrainee ? "Your appeal" : "Appeal by Nomsa Mahlangu")} {outcome}");
        cut.Find(".badge").ClassList.Should().Contain("badge-completed", "neither outcome is a failure (D51)");
        Text(cut.Find(".appeal-entry-meta")).Should().Be("Lodged 2026-10-13 · Resolved 2026-10-14 by Thandi Zulu",
            "22:00 UTC is already the next day in South Africa");
        Text(cut.Find(".appeal-entry-outcome")).Should().Be(sentence);
    }

    [Fact]
    public void ShowTheWholeReason_IsOffered_OnlyWhenTheClampedReasonOverflows()
    {
        var module = JSInterop.SetupModule("./js/review-appeal.js");
        module.Setup<bool>("overflows", _ => true).SetResult(true);

        var cut = Render(Open(), UnderAppeal());

        cut.WaitForState(() => cut.FindAll(".appeal-entry-more").Count == 1);
        var more = cut.Find("button.appeal-entry-more");
        Text(more).Should().Be("Show the whole reason");
        more.GetAttribute("aria-controls").Should().Be("appeal-9-reason");
        more.GetAttribute("aria-expanded").Should().Be("false");

        more.Click();

        cut.FindAll(".appeal-entry-more").Should().BeEmpty();
        var reason = cut.Find("#appeal-9-reason");
        reason.ClassList.Should().NotContain("appeal-entry-reason--clamped");
        reason.GetAttribute("tabindex").Should().Be("-1", "the whole reason takes the focus once shown");
    }

    [Fact]
    public void AReasonThatFits_OffersNoButton()
    {
        var module = JSInterop.SetupModule("./js/review-appeal.js");
        module.Setup<bool>("overflows", _ => true).SetResult(false);

        var cut = Render(Open(), UnderAppeal());

        cut.FindAll(".appeal-entry-more").Should().BeEmpty();
    }

    [Fact]
    public async Task AResizeOrALateFont_MeasuresAgain_AndTheButtonFollows()
    {
        // T366, D5: measured once, a reason that fits at 1280 kept no button at 390. The module watches the reason
        // (ResizeObserver, and the fonts once loaded) and says when the answer changes.
        var module = JSInterop.SetupModule("./js/review-appeal.js");
        module.Setup<bool>("overflows", _ => true).SetResult(false);
        module.SetupVoid("watchOverflow", _ => true).SetVoidResult();

        var cut = Render(Open(), UnderAppeal());

        cut.WaitForAssertion(() => module.VerifyInvoke("watchOverflow"));
        cut.FindAll(".appeal-entry-more").Should().BeEmpty();

        await cut.InvokeAsync(() => cut.Instance.OverflowChanged(true));
        cut.FindAll(".appeal-entry-more").Should().ContainSingle();

        await cut.InvokeAsync(() => cut.Instance.OverflowChanged(false));
        cut.FindAll(".appeal-entry-more").Should().BeEmpty();
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private IRenderedComponent<AppealEntry> Render(CommitteeAppealDto appeal, CommitteeReviewDetailDto review, bool forTrainee = false)
        => RenderComponent<AppealEntry>(parameters => parameters
            .Add(entry => entry.Appeal, appeal)
            .Add(entry => entry.Review, review)
            .Add(entry => entry.ForTrainee, forTrainee));

    private static CommitteeAppealDto Open()
        => new(9, Utc(2026, 10, 13, 9), "mahlangu", Reason, null, null, null) { LodgedByName = "Nomsa Mahlangu" };

    private static CommitteeReviewDetailDto UnderAppeal(params CommitteePersonDto[] body)
        => Review(CommitteeReviewState.UnderAppeal) with { AppealBody = body };

    private static CommitteeReviewDetailDto Review(CommitteeReviewState state)
        => new(4, "mahlangu", 20, "Paed Annual Review Panel", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31),
            new DateOnly(2026, 10, 13), state, null, null, null, null, null, [], [], [])
        {
            AcademicYear = 2026,
            Semester = 2
        };

    private static CommitteePersonDto Body(string userId, string name, DecisionPanelMemberRole role)
        => new(userId, role) { Name = name };

    private static DateTime Utc(int year, int month, int day, int hour) => new(year, month, day, hour, 0, 0, DateTimeKind.Utc);

    private static string Text(AngleSharp.Dom.IElement element) => Whitespace().Replace(element.TextContent, " ").Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
