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
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;
using Wombat.Web.Tests.Activities;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// The report page names each respondent group by its label, and Close campaign and Release to trainee answer as the
/// campaign page's actions do: their result above the report, the report read again after a refusal, and the focus on
/// the result when the button that acted is gone. (T225)
/// </summary>
/// <remarks>
/// Until T225 each group's card was headed by its key ("PeerDoctor", "Ahp"), and an action's refusal was also the
/// panel's load error: a refused close hid the whole report and showed its refusal twice, and its words sent the
/// coordinator to the campaign list to see whether the campaign was still open.
/// </remarks>
public sealed class CampaignReportActionsTests : WombatTestContext
{
    private const int CampaignId = 5;
    private const string TraineeUserId = "trainee-1";
    private const string TypedNarrative = "Colleagues describe a careful registrar.";
    private const string ReleasedNarrative = "Released elsewhere: colleagues describe steady progress.";

    private readonly TestAuthorizationContext _auth;

    public CampaignReportActionsTests()
    {
        _auth = this.AddTestAuthorization();
        _auth.SetAuthorized("coordinator@test");
        _auth.SetRoles(WombatRoles.Coordinator);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "coordinator-1"));

        Services.AddSingleton<IActivityReferenceDataService>(new StubActivityReferenceDataService());
    }

    // ─── Respondent groups by their labels ───────────────────────────────────

    [Fact]
    public void EachGroupsCard_IsHeadedByItsLabel_NotItsKey()
    {
        var cut = Render(new ReportSender(Report(MsfCampaignState.UnderReview,
            Group(MsfRespondentCategory.PeerDoctor, suppressed: false),
            Group(MsfRespondentCategory.Ahp, suppressed: false),
            Group(MsfRespondentCategory.Patient, suppressed: true))));

        cut.FindAll("h3").Select(Text).Should().Equal(
            "Summary", "Evidence for", "Coordinator actions", "Peer doctor", "Allied health professional", "Patient");
        cut.Markup.Should().NotContain("PeerDoctor").And.NotContain(">Ahp<");
    }

    [Fact]
    public void TheTraineesCopy_NamesEachGroupByItsLabel_NotItsKey()
    {
        _auth.SetRoles(WombatRoles.Trainee);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, TraineeUserId));
        Services.AddSingleton<IScopedSender>(new ReportSender(Report(MsfCampaignState.Released,
            Group(MsfRespondentCategory.PeerDoctor, suppressed: false),
            Group(MsfRespondentCategory.Ahp, suppressed: false)) with { IsSubjectsCopy = true }));

        var cut = RenderComponent<MyMsfReports>(parameters => parameters.Add(page => page.CampaignId, CampaignId));
        cut.WaitForState(() => cut.Markup.Contains("Selected report"));

        cut.FindAll(".detail-card .mb-3 > strong").Select(Text).Should().Equal("Peer doctor", "Allied health professional");
        cut.Markup.Should().NotContain("PeerDoctor").And.NotContain(">Ahp<");
    }

    // ─── Closing ─────────────────────────────────────────────────────────────

    [Fact]
    public void ClosingACampaign_ShowsItUnderReview_AndTheResultTakesTheFocus()
    {
        var sender = new ReportSender(Report(MsfCampaignState.Open));
        var cut = Render(sender);

        cut.Find("#msf-close-campaign").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".alert-success")).Should().Be("Campaign closed and anonymised for review."));
        cut.Find(".alert-success").GetAttribute("role").Should().Be("status");
        cut.Find(".alert-success").Closest(".action-result").Should().NotBeNull();
        Summary(cut).Should().Contain("State: Under review");
        cut.FindAll("#msf-close-campaign").Should().BeEmpty();

        // The Close campaign button that had the focus is gone.
        FocusWentToTheResult(cut);
    }

    [Fact]
    public void ACloseRefusedBecauseTheCampaignChangedElsewhere_KeepsTheReport_SaysWhyOnce_AndShowsItAsItIsNow()
    {
        // Closed in another tab: the refusal is shown once, the report stays in view, and it is read again, so it shows
        // the campaign under review and the refusal need name no other page.
        var sender = new ReportSender(Report(MsfCampaignState.Open))
        {
            Refusal = new InvalidOperationException(CloseMsfCampaignCommandHandler.CampaignChanged),
            StateAfterRefusal = MsfCampaignState.UnderReview
        };
        var cut = Render(sender);

        cut.Find("#msf-close-campaign").Click();

        cut.WaitForAssertion(() => cut.FindAll(".alert-danger").Select(Text)
            .Should().Equal(CloseMsfCampaignCommandHandler.CampaignChanged));
        cut.Find(".alert-danger").GetAttribute("role").Should().Be("alert");
        cut.Find(".alert-danger").Closest(".action-result").Should().NotBeNull("in the page's one result region");
        cut.FindAll(".alert-success").Should().BeEmpty();

        Summary(cut).Should().Contain("State: Under review", "the page reads the campaign again after a refusal");
        cut.FindAll("#msf-close-campaign").Should().BeEmpty();
        cut.Find("#msf-release-campaign");
        FocusWentToTheResult(cut);

        // Worded as T217's open and withdraw refusals: this page shows the state, so it sends nobody to the list.
        CloseMsfCampaignCommandHandler.CampaignChanged.Should().NotContain("list")
            .And.Contain("This attempt did not close it.")
            .And.EndWith("If it is still open, close it again.");
    }

    /// <summary>
    /// A report loaded while the campaign was open still offers Close once another tab or the auto-close job has closed
    /// it, or it has been released or withdrawn. The campaign refuses that close in its own words, and its close date
    /// stands; the report is read again and shows it as it is now, and Close is gone, so the refusal takes the focus. Until
    /// T246 the close succeeded on a campaign under review, said "Campaign closed and anonymised for review." and moved
    /// its close date. (T246)
    /// </summary>
    /// <remarks>
    /// Under review, Release is offered and the narrative typed is kept for it. Released or withdrawn, nothing can be sent,
    /// so the card shows the narrative as stored and no field: until the T246 review it kept this tab's own text in an
    /// editable box beside "State: Released", where the narrative the trainee was given belonged.
    /// </remarks>
    [Theory]
    [InlineData(MsfCampaignState.UnderReview, "Under review", null, null)]
    [InlineData(MsfCampaignState.Released, "Released", ReleasedNarrative, "Narrative: " + ReleasedNarrative)]
    [InlineData(MsfCampaignState.Withdrawn, "Withdrawn", null, "Narrative: Not given")]
    public void ACloseRefusedBecauseTheCampaignIsNoLongerOpen_SaysWhyOnce_ShowsItAsItIsNow_AndTheRefusalTakesTheFocus(
        MsfCampaignState stateNow, string stateText, string? storedNarrative, string? narrativeShown)
    {
        var sender = new ReportSender(Report(MsfCampaignState.Open))
        {
            Refusal = new InvalidOperationException(MsfCampaign.OnlyOpenCanBeClosed),
            StateAfterRefusal = stateNow,
            NarrativeAfterRefusal = storedNarrative
        };
        var cut = Render(sender);
        cut.Find("#msf-narrative").Change(TypedNarrative);

        cut.Find("#msf-close-campaign").Click();

        cut.WaitForAssertion(() => cut.FindAll(".alert-danger").Select(Text).Should().Equal("Only open campaigns can be closed."));
        cut.Find(".alert-danger").GetAttribute("role").Should().Be("alert");
        cut.Find(".alert-danger").Closest(".action-result").Should().NotBeNull("in the page's one result region");
        cut.FindAll(".alert-success").Should().BeEmpty("nothing was closed");
        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<CloseMsfCampaignCommand>();

        sender.Reads.Should().Be(2, "the report is read again after the refusal");
        Summary(cut).Should().Contain($"State: {stateText}", "the page shows the campaign as it is now");
        cut.FindAll("#msf-close-campaign").Should().BeEmpty("it is not open, so Close is not offered");

        if (narrativeShown is null)
        {
            cut.FindAll("#msf-release-campaign").Should().ContainSingle("a campaign under review is released here");
            cut.Find("#msf-narrative").GetAttribute("value").Should().Be(TypedNarrative,
                "reading the report again keeps what was typed, for the release");
        }
        else
        {
            cut.FindAll("#msf-release-campaign").Should().BeEmpty();
            cut.FindAll("form").Should().BeEmpty("nothing on this campaign can be sent any more");
            Actions(cut).Should().Equal(narrativeShown);
            cut.Markup.Should().NotContain(TypedNarrative, "this tab's text was never given to the trainee");
        }

        // The Close campaign button that had the focus is gone.
        FocusWentToTheResult(cut);
    }

    [Fact]
    public void ACloseRefusedOnACampaignStillOpen_LeavesCloseAndTheFocus_AndTheNarrativeTyped()
    {
        var sender = new ReportSender(Report(MsfCampaignState.Open))
        {
            Refusal = new InvalidOperationException(CloseMsfCampaignCommandHandler.CampaignChanged)
        };
        var cut = Render(sender);
        cut.Find("#msf-narrative").Change("Colleagues describe a careful registrar.");

        cut.Find("#msf-close-campaign").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".alert-danger")).Should().Be(CloseMsfCampaignCommandHandler.CampaignChanged));
        sender.Reads.Should().Be(2, "the report is read again after the refusal");
        Summary(cut).Should().Contain("State: Open");
        cut.Find("#msf-close-campaign").HasAttribute("disabled").Should().BeFalse("for the retry the refusal asks for");
        cut.Find("#msf-narrative").GetAttribute("value").Should().Be("Colleagues describe a careful registrar.",
            "reading the report again keeps what was typed");
        JSInterop.Invocations.Should().NotContain(invocation => invocation.Identifier == FocusIdentifier,
            "the button that had the focus is still there");
    }

    [Fact]
    public void ARefusal_IsKept_WhenTheReportCannotThenBeReadAgain()
    {
        var sender = new ReportSender(Report(MsfCampaignState.Open))
        {
            Refusal = new InvalidOperationException(CloseMsfCampaignCommandHandler.CampaignChanged),
            ReadFailureAfterRefusal = new InvalidOperationException("The database could not be reached.")
        };
        var cut = Render(sender);

        cut.Find("#msf-close-campaign").Click();

        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Report unavailable"));
        cut.FindAll(".alert-danger").Select(Text).Should().Equal(CloseMsfCampaignCommandHandler.CampaignChanged);
    }

    [Fact]
    public void WhileACloseRuns_ItsButtonKeepsTheFocus_AndASecondClickSendsNothing()
    {
        var sender = new ReportSender(Report(MsfCampaignState.Open)) { Hold = true };
        var cut = Render(sender);

        cut.Find("#msf-close-campaign").Click();
        cut.Find("#msf-close-campaign").HasAttribute("disabled").Should().BeFalse(
            "it has the focus, and a browser drops the focus of a button it disables (T225 review)");
        cut.Find("#msf-close-campaign").Click();

        sender.Commands.Should().ContainSingle("one close is in flight, and a second must not be sent");

        sender.Release();
        cut.WaitForAssertion(() => cut.FindAll(".alert-success").Should().ContainSingle(), AsyncWorkTimeout);
        cut.FindAll(".alert-danger").Should().BeEmpty();
        sender.Commands.Should().ContainSingle();
    }

    // ─── Releasing ───────────────────────────────────────────────────────────

    [Fact]
    public void ReleasingTheReport_ShowsItReleased_AndTheResultTakesTheFocus()
    {
        var sender = new ReportSender(Report(MsfCampaignState.UnderReview));
        var cut = Render(sender);
        cut.Find("#msf-narrative").Change(TypedNarrative);

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".alert-success")).Should().Be("Report released to the trainee."));
        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<ReleaseMsfCampaignCommand>();
        Summary(cut).Should().Contain("State: Released");
        cut.FindAll("#msf-release-campaign").Should().BeEmpty();
        Actions(cut).Should().Equal("Narrative: " + TypedNarrative); // as released, no longer in a field (T246 review)
        cut.FindAll("#msf-narrative").Should().BeEmpty();
        FocusWentToTheResult(cut);
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void WhileAReleaseRuns_ItsButtonKeepsTheFocus_AndASecondPressSendsNothing()
    {
        var sender = new ReportSender(Report(MsfCampaignState.UnderReview)) { Hold = true };
        var cut = Render(sender);

        cut.Find("form").Submit();
        cut.Find("#msf-release-campaign").HasAttribute("disabled").Should().BeFalse(
            "it has the focus, and a browser drops the focus of a button it disables (T225 review)");
        cut.Find("form").Submit();

        sender.Commands.Should().ContainSingle("one release is in flight, and a second must not be sent");

        sender.Release();
        cut.WaitForAssertion(() => cut.FindAll(".alert-success").Should().ContainSingle(), AsyncWorkTimeout);
        cut.FindAll(".alert-danger").Should().BeEmpty();
        sender.Commands.Should().ContainSingle();
    }

    [Fact]
    public void AReleaseRefusedOnACampaignStillUnderReview_KeepsTheReport_SaysWhyOnce_AndLeavesReleaseAndTheFocus()
    {
        // Until T225 a refused release was the panel's load error: the report was hidden and the refusal shown twice.
        const string refusal = "The campaign changed while it was being released.";
        var sender = new ReportSender(Report(MsfCampaignState.UnderReview,
            Group(MsfRespondentCategory.PeerDoctor, suppressed: false)))
        {
            Refusal = new InvalidOperationException(refusal)
        };
        var cut = Render(sender);
        cut.Find("#msf-narrative").Change("Colleagues describe a careful registrar.");

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => cut.FindAll(".alert-danger").Select(Text).Should().Equal(refusal));
        cut.Find(".alert-danger").GetAttribute("role").Should().Be("alert");
        cut.Find(".alert-danger").Closest(".action-result").Should().NotBeNull("in the page's one result region");
        cut.FindAll(".alert-success").Should().BeEmpty();
        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<ReleaseMsfCampaignCommand>();

        sender.Reads.Should().Be(2, "the report is read again after the refusal");
        Summary(cut).Should().Contain("State: Under review", "the report stays in view");
        cut.FindAll("h3").Select(Text).Should().Contain("Peer doctor");
        cut.Find("#msf-narrative").GetAttribute("value").Should().Be("Colleagues describe a careful registrar.",
            "reading the report again keeps what was typed");
        cut.Find("#msf-release-campaign").HasAttribute("disabled").Should().BeFalse("for the retry");
        JSInterop.Invocations.Should().NotContain(invocation => invocation.Identifier == FocusIdentifier,
            "the button that had the focus is still there, and enabled");
    }

    /// <summary>
    /// A release refused because another tab has released the campaign: the report read again shows the narrative the
    /// trainee was given, in no field, and not the one this tab typed. (T246 review)
    /// </summary>
    [Fact]
    public void AReleaseRefusedOnACampaignReleasedElsewhere_ShowsTheNarrativeReleased_NotTheOneTyped()
    {
        var sender = new ReportSender(Report(MsfCampaignState.UnderReview))
        {
            Refusal = new InvalidOperationException("Only campaigns under review can be released."),
            StateAfterRefusal = MsfCampaignState.Released,
            NarrativeAfterRefusal = ReleasedNarrative
        };
        var cut = Render(sender);
        cut.Find("#msf-narrative").Change(TypedNarrative);

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => cut.FindAll(".alert-danger").Should().ContainSingle());
        Summary(cut).Should().Contain("State: Released");
        cut.FindAll("#msf-narrative").Should().BeEmpty();
        cut.FindAll("#msf-release-campaign").Should().BeEmpty();
        Actions(cut).Should().Equal("Narrative: " + ReleasedNarrative);
        cut.Markup.Should().NotContain(TypedNarrative);
        FocusWentToTheResult(cut);
    }

    /// <summary>
    /// A report already released shows what was released, the level by its rung's label (order 5 is rung "4" on the CPSA
    /// ladder, T100), and no field: nothing here can send either again. (T246 review)
    /// </summary>
    [Fact]
    public void AReleasedReport_ShowsItsNarrativeAndLevelAsReleased_InNoField()
    {
        Services.AddSingleton<IActivityReferenceDataService>(new RatedLevelsReferenceData());
        var cut = Render(new ReportSender(Report(MsfCampaignState.Released) with
        {
            CoordinatorNarrative = ReleasedNarrative,
            ReviewerEntrustmentLevel = 5
        }));

        cut.WaitForAssertion(() => Actions(cut).Should().Equal(
            "Narrative: " + ReleasedNarrative,
            "Supervision level this feedback supports: 4"));
        cut.FindAll("#msf-narrative").Should().BeEmpty();
        cut.FindAll("#msf-level").Should().BeEmpty();
        cut.FindAll("form").Should().BeEmpty();
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    /// <summary>
    /// A refusal by a command's validator shows each failure's message, never FluentValidation's log line ("Validation
    /// failed: -- Narrative: ..."), for a close and a release alike: T213's <c>RefusalText</c>, which DESIGN.md asks every
    /// page to use, and which T246 moved this page onto. (T246 review)
    /// </summary>
    [Theory]
    [InlineData(MsfCampaignState.Open)]
    [InlineData(MsfCampaignState.UnderReview)]
    public void ARefusalByAValidator_ShowsEachFailuresMessage_NotItsLogLine(MsfCampaignState state)
    {
        var sender = new ReportSender(Report(state))
        {
            Refusal = new ValidationException(
            [
                new ValidationFailure("Narrative", "The narrative can be at most 4000 characters."),
                new ValidationFailure("EntrustmentLevel", "That level is not on this campaign's scale.")
            ])
        };
        var cut = Render(sender);

        if (state == MsfCampaignState.Open)
        {
            cut.Find("#msf-close-campaign").Click();
        }
        else
        {
            cut.Find("form").Submit();
        }

        cut.WaitForAssertion(() => cut.FindAll(".alert-danger").Select(Text).Should().Equal(
            "The narrative can be at most 4000 characters. That level is not on this campaign's scale."));
        cut.Markup.Should().NotContain("Validation failed");
    }

    [Fact]
    public void AReleaseRefusedOnACampaignNoLongerReady_DisablesRelease_AndTheResultTakesTheFocus()
    {
        // The button that had the focus is still there but disabled, which loses the focus as surely as removing it.
        var sender = new ReportSender(Report(MsfCampaignState.UnderReview))
        {
            Refusal = new InvalidOperationException("The report is not ready for release."),
            ReadyAfterRefusal = false
        };
        var cut = Render(sender);

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => cut.FindAll(".alert-danger").Should().ContainSingle());
        cut.Find("#msf-release-campaign").HasAttribute("disabled").Should().BeTrue("the report read again is not ready");
        FocusWentToTheResult(cut);
    }

    [Fact]
    public void AReleaseThatWasTaken_ButTheReportCannotThenBeRead_SaysSoInThePanel_NotAsARefusal()
    {
        var sender = new ReportSender(Report(MsfCampaignState.UnderReview))
        {
            ReadFailureAfterCommand = new InvalidOperationException("The database could not be reached.")
        };
        var cut = Render(sender);

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".alert-success")).Should().Be("Report released to the trainee."));
        cut.FindAll(".action-result .alert-danger").Should().BeEmpty("the release was taken; nothing was refused");
        cut.FindAll(".alert-danger").Select(Text).Should().Equal("The database could not be reached.");
        cut.Markup.Should().NotContain("Report unavailable", "the panel says why it could not be read");
        FocusWentToTheResult(cut);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>What <see cref="ElementReference" />.FocusAsync calls.</summary>
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    private IRenderedComponent<CampaignReport> Render(ReportSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<CampaignReport>(parameters => parameters.Add(page => page.CampaignId, CampaignId));
        // The report has loaded: a released report's card holds no narrative field (T246 review).
        cut.WaitForState(() => cut.FindAll("h3").Any(heading => Text(heading) == "Coordinator actions"));
        return cut;
    }

    private void FocusWentToTheResult(IRenderedComponent<CampaignReport> cut)
    {
        cut.Find(".action-result").GetAttribute("tabindex").Should().Be("-1");
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.ResultRegion.Id));
    }

    /// <summary>The Summary card's lines, one "Name: value" each.</summary>
    private static IReadOnlyList<string> Summary(IRenderedFragment cut)
        => cut.FindAll("section.detail-card")
            .Single(section => Text(section.QuerySelector("h3")!) == "Summary")
            .QuerySelectorAll("p")
            .Select(Text)
            .ToList();

    /// <summary>The Coordinator actions card's lines, when it shows the campaign as stored rather than a form.</summary>
    private static IReadOnlyList<string> Actions(IRenderedFragment cut)
        => cut.FindAll("section.detail-card")
            .Single(section => Text(section.QuerySelector("h3")!) == "Coordinator actions")
            .QuerySelectorAll("p")
            .Select(Text)
            .ToList();

    private static MsfCampaignAggregateReportDto Report(MsfCampaignState state, params MsfCategoryAggregateDto[] groups)
        => new(CampaignId, TraineeUserId, "Annual MSF", state, 8, 3, 9, null, true, groups, 2, 2,
            [new MsfCoveredEpaDto(101, "PAED-001", "Resuscitate a critically ill child", false)], null, null);

    private static MsfCategoryAggregateDto Group(MsfRespondentCategory category, bool suppressed)
        => new(category, suppressed ? 1 : 3, suppressed,
            suppressed
                ? []
                : [new MsfQuestionAggregateDto(1, "Professional performance", MsfQuestionType.Scale,
                    new MsfScaleAggregateDto(4, 3, new Dictionary<int, int> { [4] = 3 }), [])]);

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    /// <summary>The CPSA ladder's rungs by their order: order 5 is rung "4" (T100).</summary>
    private sealed class RatedLevelsReferenceData : StubActivityReferenceDataService
    {
        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetRatedLevelOptionsForActivityTypeAsync(
            string activityTypeKey, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>(
            [
                new("1", "1"), new("2", "2"), new("3", "3a"), new("4", "3b"), new("5", "4"), new("6", "5")
            ]);
    }

    /// <summary>
    /// Answers the report as it stands, and closes and releases it as the handlers would: it reads back under review or
    /// released. A refusal leaves it in <see cref="StateAfterRefusal" />, as a campaign changed elsewhere.
    /// </summary>
    private sealed class ReportSender(MsfCampaignAggregateReportDto report) : IScopedSender
    {
        private MsfCampaignAggregateReportDto _report = report;
        private TaskCompletionSource? _held;
        private bool _refused;
        private bool _taken;

        /// <summary>Every close and release sent, in order.</summary>
        public List<object> Commands { get; } = [];

        /// <summary>How many times the report was read.</summary>
        public int Reads { get; private set; }

        public Exception? Refusal { get; init; }

        public MsfCampaignState? StateAfterRefusal { get; init; }

        /// <summary>The narrative stored once a command has been refused, as another tab's release left it.</summary>
        public string? NarrativeAfterRefusal { get; init; }

        /// <summary>What every read of the report throws once a command has been refused.</summary>
        public Exception? ReadFailureAfterRefusal { get; init; }

        /// <summary>What every read of the report throws once a command has been taken.</summary>
        public Exception? ReadFailureAfterCommand { get; init; }

        /// <summary>Whether the report is ready for release once a command has been refused, as it may be no longer.</summary>
        public bool? ReadyAfterRefusal { get; init; }

        /// <summary>Holds each close and release until <see cref="Release" />.</summary>
        public bool Hold { get; init; }

        public void Release() => (_held ?? throw new InvalidOperationException("Nothing was held.")).SetResult();

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case GetCampaignAggregateReportQuery:
                    if (_refused && ReadFailureAfterRefusal is not null)
                    {
                        return Task.FromException<TResponse>(ReadFailureAfterRefusal);
                    }

                    if (_taken && ReadFailureAfterCommand is not null)
                    {
                        return Task.FromException<TResponse>(ReadFailureAfterCommand);
                    }

                    Reads++;
                    return Task.FromResult((TResponse)(object)_report);

                case ListMsfCampaignsForTraineeQuery:
                    return Task.FromResult((TResponse)(object)(IReadOnlyList<MsfCampaignSummaryDto>)[]);

                case CloseMsfCampaignCommand close:
                    Commands.Add(close);
                    if (Refusal is not null)
                    {
                        Refuse();
                        return Task.FromException<TResponse>(Refusal);
                    }

                    _report = _report with { State = MsfCampaignState.UnderReview };
                    _taken = true;
                    return Held((TResponse)(object)_report);

                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            if (request is not ReleaseMsfCampaignCommand)
            {
                throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }

            Commands.Add(request);
            if (Refusal is not null)
            {
                Refuse();
                return Task.FromException(Refusal);
            }

            var release = (ReleaseMsfCampaignCommand)request;
            _report = _report with
            {
                State = MsfCampaignState.Released,
                CoordinatorNarrative = release.Narrative,
                ReviewerEntrustmentLevel = release.EntrustmentLevel
            };
            _taken = true;
            return Held(true);
        }

        private Task<T> Held<T>(T result)
        {
            if (!Hold)
            {
                return Task.FromResult(result);
            }

            _held = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            return AfterAsync(_held.Task, result);
        }

        private static async Task<T> AfterAsync<T>(Task held, T result)
        {
            await held;
            return result;
        }

        private void Refuse()
        {
            _refused = true;
            if (StateAfterRefusal is { } state)
            {
                _report = _report with { State = state, CoordinatorNarrative = NarrativeAfterRefusal };
            }

            if (ReadyAfterRefusal is { } ready)
            {
                _report = _report with { ReadyForRelease = ready };
            }
        }
    }
}
