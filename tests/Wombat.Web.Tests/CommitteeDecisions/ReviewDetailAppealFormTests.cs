using System.Security.Claims;
using System.Text.RegularExpressions;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// The appeal body's resolve form (T307). Resolving is irreversible, so the Outcome opens on no outcome and a resolver who
/// leaves it there is told to choose one; each outcome is said in words, with what it does to the decision; and a remit
/// records the replacement's conditions as a first-time decision does, on a progression and an entrustment-only review
/// alike. Before T307 the select opened on Dismissed, so pressing Resolve appeal untouched dismissed the appeal, and the
/// remit had no conditions box, so a remit to Satisfactory with Observations could not carry its observations. Since T366
/// (flow 09, T323) the Outcome is two radios, read whole at every width, and each outcome confirms in its dialog.
/// </summary>
public sealed partial class ReviewDetailAppealFormTests : TestContext
{
    private const string Conditions = "Two observed Mini-CEX and one DOPS before the next review.";

    /// <summary>What <see cref="Microsoft.AspNetCore.Components.ElementReference" />.FocusAsync calls.</summary>
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    private readonly TestAuthorizationContext _auth;

    public ReviewDetailAppealFormTests()
    {
        _auth = this.AddTestAuthorization();
        _auth.SetAuthorized("external-1@test");
        _auth.SetRoles("CommitteeMember");
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "external-1"));
        // The appeal entry measures its reason, and Resolve appeal opens a native dialog (T366).
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void TheOutcome_OpensOnNeither_AndSaysEachOutcomeInWords()
    {
        var (cut, _) = Render(Review());

        // Two radios, neither checked: the resolver has chosen nothing yet (T307), and each reads whole (T323).
        var radios = cut.FindAll("#appeal-outcome input[type=radio]").ToList();
        radios.Should().NotContain(radio => radio.HasAttribute("checked"));
        radios.Select(radio => radio.GetAttribute("value")).Should().Equal("Dismissed", "Remitted");
        cut.FindAll("#appeal-outcome label").Select(label => label.TextContent.Trim()).Should().Equal(
            "Dismissed: the decision stands",
            "Remitted: the appeal body replaces the decision");

        // What each outcome does is said with the group, and a screen reader reads it with it (T193).
        cut.Find("#appeal-outcome").GetAttribute("aria-describedby").Should().Be("appeal-outcome-help appeal-outcome-message");
        Whitespace().Replace(cut.Find("#appeal-outcome-help").TextContent, " ").Trim()
            .Should().Be(CommitteeDecisionWording.AppealOutcomeHelp);
    }

    [Fact]
    public void ResolvingWithoutAnOutcome_SaysChooseAnOutcome_AndSendsNothing()
    {
        var sender = new FakeSender(Review());
        var (cut, _) = Render(sender);

        // What the region under the Outcome says at the moment the focus moves to the select. The T307 review: focused
        // from the submit handler, the select took the focus before the render that puts the message there, so a screen
        // reader read the help alone and never the message.
        string? messageWhenFocused = null;
        JSInterop.SetupVoid(FocusIdentifier, _ =>
        {
            messageWhenFocused = Whitespace().Replace(cut.Find("#appeal-outcome-message").TextContent, " ").Trim();
            return true;
        }).SetVoidResult();
        JSInterop.Invocations.Should().NotContain(invocation => invocation.Identifier == FocusIdentifier,
            "nothing moves the focus before a submit");
        // Read before the submit: once the group re-renders as invalid, bUnit's markup no longer carries its reference.
        var outcomeReference = cut.Find("#appeal-outcome-dismissed").GetAttribute("blazor:elementreference");
        outcomeReference.Should().NotBeNullOrEmpty();

        ResolveButton(cut).Click();

        cut.WaitForState(() => cut.Markup.Contains("Choose an outcome."));
        sender.Resolved.Should().BeEmpty("an appeal nobody chose an outcome for is not resolved");
        cut.FindAll("#appeal-outcome input[type=radio]").Should().NotContain(radio => radio.HasAttribute("checked"));
        Whitespace().Replace(cut.Find("#appeal-outcome-message").TextContent, " ").Trim().Should().Be("Choose an outcome.");
        cut.Find("#appeal-outcome").GetAttribute("aria-describedby")
            .Should().Be("appeal-outcome-help appeal-outcome-message", "the message is read with the group");

        // The refused submit moves the focus to the Outcome, once, and only after the message saying why is on the page.
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<Microsoft.AspNetCore.Components.ElementReference>()
            .Which.Id.Should().Be(outcomeReference));
        messageWhenFocused.Should().Be("Choose an outcome.", "the radio is focused after its message is rendered");

        // Once: choosing an outcome afterwards renders the page again, and leaves the focus where the resolver put it.
        Choose(cut, CommitteeAppealOutcome.Remitted);
        cut.FindAll("#appeal-conditions").Should().ContainSingle();
        JSInterop.VerifyFocusAsyncInvoke(calledTimes: 1);
    }

    [Fact]
    public void Remitting_OffersReplacementConditions_AndSendsWhatIsWritten()
    {
        var sender = new FakeSender(Review());
        var (cut, _) = Render(sender);

        cut.FindAll("#appeal-conditions").Should().BeEmpty("only a remit takes a decision");

        Choose(cut, CommitteeAppealOutcome.Remitted);
        cut.Find("label[for=appeal-conditions]").TextContent.Trim().Should().Be("Replacement conditions");
        cut.Find("#appeal-category").Change(CommitteeDecisionCategory.SatisfactoryWithObservations.ToString());
        cut.Find("#appeal-rationale").Change("Progress is adequate, with observed assessments to follow.");
        cut.Find("#appeal-conditions").Change(Conditions);
        ResolveButton(cut).Click();
        Confirm(cut, "Remit the appeal");
        cut.WaitForState(() => sender.Resolved.Count == 1);

        var resolved = sender.Resolved.Single();
        resolved.Outcome.Should().Be(CommitteeAppealOutcome.Remitted);
        resolved.RemittedCategory.Should().Be(CommitteeDecisionCategory.SatisfactoryWithObservations);
        resolved.RemittedConditions.Should().Be(Conditions);
    }

    [Fact]
    public void RemittingAnEntrustmentOnlyReview_AlsoRecordsConditions()
    {
        // The Decision form asks for conditions on either type of review, so the remit does too.
        var sender = new FakeSender(Review() with { ReviewType = CommitteeReviewType.EntrustmentOnly });
        var (cut, _) = Render(sender);

        Choose(cut, CommitteeAppealOutcome.Remitted);
        cut.FindAll("#appeal-category").Should().BeEmpty();
        cut.Find("#appeal-rationale").Change("Re-read on appeal: the levels stand.");
        cut.Find("#appeal-conditions").Change(Conditions);
        ResolveButton(cut).Click();
        Confirm(cut, "Remit the appeal");
        cut.WaitForState(() => sender.Resolved.Count == 1);

        sender.Resolved.Single().RemittedConditions.Should().Be(Conditions);
    }

    [Fact]
    public void Dismissing_SendsNoReplacement_EvenWhatWasWrittenForARemit()
    {
        // A resolver who starts a remit and then dismisses: what was typed for the replacement is not sent with the
        // dismissal, which takes no decision.
        var sender = new FakeSender(Review());
        var (cut, _) = Render(sender);

        Choose(cut, CommitteeAppealOutcome.Remitted);
        cut.Find("#appeal-category").Change(CommitteeDecisionCategory.SatisfactoryWithObservations.ToString());
        cut.Find("#appeal-rationale").Change("Started as a remit.");
        cut.Find("#appeal-conditions").Change(Conditions);
        Choose(cut, CommitteeAppealOutcome.Dismissed);

        cut.FindAll("#appeal-conditions").Should().BeEmpty();
        ResolveButton(cut).Click();
        Confirm(cut, "Dismiss the appeal");
        cut.WaitForState(() => sender.Resolved.Count == 1);

        var resolved = sender.Resolved.Single();
        resolved.Outcome.Should().Be(CommitteeAppealOutcome.Dismissed);
        resolved.RemittedCategory.Should().BeNull();
        resolved.RemittedRationale.Should().BeNull();
        resolved.RemittedConditions.Should().BeNull();
        resolved.PresentUserIds.Should().BeNull();
    }

    [Fact]
    public void TheRemittedDecisionsConditions_ShowOnItsCard()
    {
        var replacement = Decision(42, CommitteeDecisionCategory.SatisfactoryWithObservations, Conditions, supersedes: 41);
        var review = Review() with
        {
            State = CommitteeReviewState.Final,
            Decisions = [replacement, Decision(41, CommitteeDecisionCategory.InadequateProgressAdditionalTraining, null, null)],
            Appeals =
            [
                new CommitteeAppealDto(7, new DateTime(2026, 9, 20, 9, 0, 0, DateTimeKind.Utc), "trainee-1", "Reconsider.",
                    new DateTime(2026, 9, 26, 9, 0, 0, DateTimeKind.Utc), "chair-1", CommitteeAppealOutcome.Remitted)
            ]
        };
        var (cut, _) = Render(review);

        Whitespace().Replace(cut.Find("#decision-42").TextContent, " ").Should().Contain($"Conditions: {Conditions}");
        cut.FindAll("#appeal-outcome").Should().BeEmpty("a resolved appeal offers no form");
    }

    [Theory]
    [InlineData(CommitteeAppealOutcome.Dismissed, "Dismissed")]
    [InlineData(CommitteeAppealOutcome.Remitted, "Remitted")]
    [InlineData(null, "Open")]
    public void EachAppealsEntry_NamesItsOutcome_InTheWordingsWords(CommitteeAppealOutcome? outcome, string expected)
    {
        // The T307 review: the list printed the enum's name, so the page had two sources for how an outcome reads. Since
        // T366 each appeal is lane T's AppealEntry, its badge the outcome's word.
        var review = Review() with
        {
            State = outcome is null ? CommitteeReviewState.UnderAppeal : CommitteeReviewState.Final,
            Appeals =
            [
                new CommitteeAppealDto(7, new DateTime(2026, 9, 20, 9, 0, 0, DateTimeKind.Utc), "trainee-1", "Reconsider.",
                    outcome is null ? null : new DateTime(2026, 9, 26, 9, 0, 0, DateTimeKind.Utc), outcome is null ? null : "chair-1",
                    outcome)
                {
                    LodgedByName = "Nomsa Mahlangu"
                }
            ]
        };
        var (cut, _) = Render(review);

        var entry = cut.Find("#appeals li.appeal-entry");
        Whitespace().Replace(entry.QuerySelector("h3")!.TextContent, " ").Trim().Should().Be($"Appeal by Nomsa Mahlangu {expected}");
        Whitespace().Replace(entry.TextContent, " ").Should().Contain("Lodged 2026-09-20").And.Contain("Reconsider.");
        if (outcome is { } resolved)
        {
            expected.Should().Be(CommitteeDecisionWording.AppealOutcomeName(resolved));
            CommitteeDecisionWording.AppealOutcomeLabel(resolved).Should().StartWith($"{expected}: ");
        }
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private static AngleSharp.Dom.IElement ResolveButton(IRenderedComponent<ReviewDetail> cut)
        => cut.FindAll("button").Single(button => button.Closest("dialog") is null && button.TextContent.Trim() == "Resolve appeal");

    private static void Choose(IRenderedComponent<ReviewDetail> cut, CommitteeAppealOutcome outcome)
        => cut.Find($"#appeal-outcome-{outcome.ToString().ToLowerInvariant()}").Change(outcome.ToString());

    /// <summary>The outcome's dialog's confirm button (T366, C14b, C14c).</summary>
    private static void Confirm(IRenderedComponent<ReviewDetail> cut, string words)
        => cut.FindAll("dialog button").Single(button => button.TextContent.Trim() == words).Click();

    private (IRenderedComponent<ReviewDetail> Cut, string Text) Render(CommitteeReviewDetailDto review)
        => Render(new FakeSender(review));

    private (IRenderedComponent<ReviewDetail> Cut, string Text) Render(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, 30));
        cut.WaitForState(() => cut.Markup.Contains("Evidence snapshot"));

        var text = string.Join(" ", cut.Nodes.Select(node => node.TextContent));
        return (cut, Whitespace().Replace(text, " "));
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    private static CommitteePersonDto Person(string userId, DecisionPanelMemberRole role, string name)
        => new(userId, role) { Name = name, MaySit = true };

    private static CommitteeDecisionDto Decision(int id, CommitteeDecisionCategory category, string? conditions, int? supersedes)
        => new(id, category, $"Decision {id}.", conditions,
            new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc).AddDays(id), "chair-1", supersedes)
        {
            Attendees =
            [
                Person("chair-1", DecisionPanelMemberRole.Chair, "Thandi Zulu") with { MaySit = false },
                Person("external-1", DecisionPanelMemberRole.External, "John van Rensburg") with { MaySit = false }
            ]
        };

    /// <summary>Dr Mahlangu's review under appeal, read by the panel's external member, who sits on the appeal body.</summary>
    private static CommitteeReviewDetailDto Review()
        => new CommitteeReviewDetailDto(
            30,
            "trainee-1",
            20,
            "Paed Annual Review Panel",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            new DateOnly(2026, 9, 1),
            CommitteeReviewState.UnderAppeal,
            new DateTime(2026, 9, 1, 9, 0, 0, DateTimeKind.Utc),
            "chair-1",
            new DateTime(2026, 9, 2, 9, 0, 0, DateTimeKind.Utc),
            "chair-1",
            null,
            [Decision(41, CommitteeDecisionCategory.InadequateProgressAdditionalTraining, null, null)],
            [
                new CommitteeAppealDto(7, new DateTime(2026, 9, 20, 9, 0, 0, DateTimeKind.Utc), "trainee-1", "Reconsider.",
                    null, null, null)
            ],
            [])
        {
            AcademicYear = 2026,
            Semester = 2,
            CallerChairs = false,
            CallerResolvesAppeals = true,
            TraineeName = "Nomsa Mahlangu",
            PanelMembers =
            [
                Person("chair-1", DecisionPanelMemberRole.Chair, "Thandi Zulu"),
                Person("member-1", DecisionPanelMemberRole.Member, "Sarah Botha"),
                Person("external-1", DecisionPanelMemberRole.External, "John van Rensburg")
            ]
        };

    private sealed class FakeSender : IScopedSender
    {
        private readonly CommitteeReviewDetailDto _review;

        public FakeSender(CommitteeReviewDetailDto review) => _review = review;

        public List<ResolveAppealCommand> Resolved { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is ResolveAppealCommand resolve)
            {
                Resolved.Add(resolve);
            }

            object? response = request switch
            {
                GetCommitteeReviewByIdQuery => _review,
                ResolveAppealCommand => _review with { State = CommitteeReviewState.Final },
                ListPendingEntrustmentDecisionsForReviewQuery => Array.Empty<PendingEntrustmentDecisionDto>(),
                GetSamplingConcentrationWarningsQuery => null,
                CountMsfCampaignsOutsideSnapshotQuery => MsfCampaignsOutsideSnapshotDto.None,
                GetEpaTrajectoryForTraineeQuery => Array.Empty<EpaTrajectoryDto>(),
                ListStarEpaOptionsForReviewQuery => Array.Empty<StarEpaOptionDto>(),
                GetEntrustmentScalesListQuery => Array.Empty<EntrustmentScaleDto>(),
                GetEntrustmentStandingForTraineeQuery => null,
                GetMsfCoverageForTraineeQuery => null,
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response!);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
