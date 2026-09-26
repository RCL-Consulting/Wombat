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
/// remit had no conditions box, so a remit to Satisfactory with Observations could not carry its observations.
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
    }

    [Fact]
    public void TheOutcome_OpensOnSelectAnOutcome_AndSaysEachOutcomeInWords()
    {
        var (cut, _) = Render(Review());

        // The select carries no value and no option is marked selected, so the browser shows the first: the empty one.
        var select = cut.Find("#appeal-outcome");
        select.GetAttribute("value").Should().BeNullOrEmpty("the resolver has chosen nothing yet");

        var options = cut.FindAll("#appeal-outcome option").ToList();
        options.Should().NotContain(option => option.HasAttribute("selected"));
        options[0].GetAttribute("value").Should().BeEmpty();
        options.Select(option => option.TextContent.Trim()).Should().Equal(
            "Select an outcome…",
            "Dismissed: the decision stands",
            "Remitted: the appeal body replaces the decision");
        options.Skip(1).Select(option => option.GetAttribute("value")).Should().Equal("Dismissed", "Remitted");

        // What each outcome does is said with the select, and a screen reader reads it with it (T193).
        select.GetAttribute("aria-describedby").Should().Be("appeal-outcome-help appeal-outcome-message");
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
        // Read before the submit: once the select re-renders as invalid, bUnit's markup no longer carries its reference.
        var outcomeReference = cut.Find("#appeal-outcome").GetAttribute("blazor:elementreference");
        outcomeReference.Should().NotBeNullOrEmpty();

        ResolveButton(cut).Click();

        cut.WaitForState(() => cut.Markup.Contains("Choose an outcome."));
        sender.Resolved.Should().BeEmpty("an appeal nobody chose an outcome for is not resolved");
        cut.Find("#appeal-outcome").GetAttribute("value").Should().BeNullOrEmpty();
        Whitespace().Replace(cut.Find("#appeal-outcome-message").TextContent, " ").Trim().Should().Be("Choose an outcome.");
        cut.Find("#appeal-outcome").GetAttribute("aria-describedby")
            .Should().Be("appeal-outcome-help appeal-outcome-message", "the message is read with the select");

        // The refused submit moves the focus to the Outcome, once, and only after the message saying why is on the page.
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<Microsoft.AspNetCore.Components.ElementReference>()
            .Which.Id.Should().Be(outcomeReference));
        messageWhenFocused.Should().Be("Choose an outcome.", "the select is focused after its message is rendered");

        // Once: choosing an outcome afterwards renders the page again, and leaves the focus where the resolver put it.
        cut.Find("#appeal-outcome").Change(CommitteeAppealOutcome.Remitted.ToString());
        cut.FindAll("#appeal-conditions").Should().ContainSingle();
        JSInterop.VerifyFocusAsyncInvoke(calledTimes: 1);
    }

    [Fact]
    public void Remitting_OffersReplacementConditions_AndSendsWhatIsWritten()
    {
        var sender = new FakeSender(Review());
        var (cut, _) = Render(sender);

        cut.FindAll("#appeal-conditions").Should().BeEmpty("only a remit takes a decision");

        cut.Find("#appeal-outcome").Change(CommitteeAppealOutcome.Remitted.ToString());
        cut.Find("label[for=appeal-conditions]").TextContent.Trim().Should().Be("Replacement conditions");
        cut.Find("#appeal-category").Change(CommitteeDecisionCategory.SatisfactoryWithObservations.ToString());
        cut.Find("#appeal-rationale").Change("Progress is adequate, with observed assessments to follow.");
        cut.Find("#appeal-conditions").Change(Conditions);
        ResolveButton(cut).Click();
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

        cut.Find("#appeal-outcome").Change(CommitteeAppealOutcome.Remitted.ToString());
        cut.FindAll("#appeal-category").Should().BeEmpty();
        cut.Find("#appeal-rationale").Change("Re-read on appeal: the levels stand.");
        cut.Find("#appeal-conditions").Change(Conditions);
        ResolveButton(cut).Click();
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

        cut.Find("#appeal-outcome").Change(CommitteeAppealOutcome.Remitted.ToString());
        cut.Find("#appeal-category").Change(CommitteeDecisionCategory.SatisfactoryWithObservations.ToString());
        cut.Find("#appeal-rationale").Change("Started as a remit.");
        cut.Find("#appeal-conditions").Change(Conditions);
        cut.Find("#appeal-outcome").Change(CommitteeAppealOutcome.Dismissed.ToString());

        cut.FindAll("#appeal-conditions").Should().BeEmpty();
        ResolveButton(cut).Click();
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
    public void TheAppealsList_NamesEachAppealsOutcome_InTheWordingsWords(CommitteeAppealOutcome? outcome, string expected)
    {
        // The T307 review: the list printed the enum's name, so the page had two sources for how an outcome reads.
        var review = Review() with
        {
            State = outcome is null ? CommitteeReviewState.UnderAppeal : CommitteeReviewState.Final,
            Appeals =
            [
                new CommitteeAppealDto(7, new DateTime(2026, 9, 20, 9, 0, 0, DateTimeKind.Utc), "trainee-1", "Reconsider.",
                    outcome is null ? null : new DateTime(2026, 9, 26, 9, 0, 0, DateTimeKind.Utc), outcome is null ? null : "chair-1",
                    outcome)
            ]
        };
        var (cut, _) = Render(review);

        var line = cut.FindAll("li").Single(item => item.TextContent.Contains("Reconsider."));
        Whitespace().Replace(line.TextContent, " ").Trim().Should().Be($"2026-09-20 Reconsider. ({expected})");
        if (outcome is { } resolved)
        {
            expected.Should().Be(CommitteeDecisionWording.AppealOutcomeName(resolved));
            CommitteeDecisionWording.AppealOutcomeLabel(resolved).Should().StartWith($"{expected}: ");
        }
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private static AngleSharp.Dom.IElement ResolveButton(IRenderedComponent<ReviewDetail> cut)
        => cut.FindAll("button").Single(button => button.TextContent.Trim() == "Resolve appeal");

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
