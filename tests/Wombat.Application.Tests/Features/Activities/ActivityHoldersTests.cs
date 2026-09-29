using FluentAssertions;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Application.Tests.Features.Activities;

/// <summary>
/// T342 (B6, B7): who has an activity now, its nominee, and whether it was returned, on the shapes a registrar files:
/// the Mini-CEX (a request to a named assessor), the reflective exercise and the portfolio review (a named supervisor
/// who may return it), and KGK's teaching log (the registrar's own, straight to a terminal state).
/// </summary>
public sealed class ActivityHoldersTests
{
    private const string Registrar = "sipho";
    private const string Assessor = "david";
    private const string NamesDavid = """{ "assessor_user_id": "david" }""";

    private static readonly DateTime Since = new(2026, 9, 29, 7, 12, 0, DateTimeKind.Utc);

    /// <summary>Act 1 steps 1.26–1.29: draft → logged, by the subject or the creator; its supervisor receives nothing.</summary>
    private static readonly Workflow KgkTeachingLog = WorkflowParser.Parse("""
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "logged", "label": "Logged", "terminal": true }
          ],
          "transitions": [
            { "key": "log", "from": "draft", "to": "logged", "actor": "subject|creator" }
          ]
        }
        """);

    private static Workflow Seed(string key) => WorkflowParser.Parse(ShippedSeeds.Workflow(key));

    private static ActivityHolderDto Holder(Workflow? workflow, string state, string data = NamesDavid, string? caller = Registrar)
        => ActivityHolders.Resolve(workflow, state, Registrar, Registrar, data, Since, caller);

    [Theory]
    [InlineData("mini_cex_cpsa", "draft", ActivityHolderKind.Author)]
    [InlineData("mini_cex_cpsa", "requested", ActivityHolderKind.Person)]
    [InlineData("mini_cex_cpsa", "completed", ActivityHolderKind.Done)]
    [InlineData("mini_cex_cpsa", "declined", ActivityHolderKind.Closed)]
    [InlineData("mini_cex_cpsa", "cancelled", ActivityHolderKind.Closed)]
    [InlineData("reflective_exercise_cpsa", "draft", ActivityHolderKind.Author)]
    [InlineData("reflective_exercise_cpsa", "submitted", ActivityHolderKind.Person)]
    [InlineData("reflective_exercise_cpsa", "discussed", ActivityHolderKind.Done)]
    [InlineData("reflective_exercise_cpsa", "cancelled", ActivityHolderKind.Closed)]
    [InlineData("portfolio_review_cpsa", "draft", ActivityHolderKind.Author)]
    [InlineData("portfolio_review_cpsa", "submitted", ActivityHolderKind.Person)]
    [InlineData("portfolio_review_cpsa", "signed_off", ActivityHolderKind.Done)]
    [InlineData("portfolio_review_cpsa", "cancelled", ActivityHolderKind.Closed)]
    [InlineData("teaching_session", "submitted", ActivityHolderKind.Waiting)]
    public void EachShippedShape_IsHeldByWhomItsMovesThatLeadOnSay(string seed, string state, ActivityHolderKind expected)
    {
        Holder(Seed(seed), state).Kind.Should().Be(expected);
    }

    [Fact]
    public void ADraft_IsTheRegistrars_AndIsHersToHer()
    {
        var holder = Holder(Seed("mini_cex_cpsa"), "draft");

        holder.Should().Be(new ActivityHolderDto(ActivityHolderKind.Author, Registrar, null, IsViewer: true, Since));
        Holder(Seed("mini_cex_cpsa"), "draft", caller: Assessor).IsViewer.Should().BeFalse("the assessor reads her name");
    }

    /// <summary>
    /// A request her registrar may still cancel is with its assessor: the withdrawal leads nowhere, so it does not make
    /// her a holder (Step 3.12's "Thandi Zulu" column).
    /// </summary>
    [Fact]
    public void ARequest_IsTheNamedAssessors_ThoughItsRegistrarMayCancelIt()
    {
        var holder = Holder(Seed("mini_cex_cpsa"), "requested");

        holder.Should().Be(new ActivityHolderDto(ActivityHolderKind.Person, Assessor, null, IsViewer: false, Since));
        Holder(Seed("mini_cex_cpsa"), "requested", caller: Assessor).IsViewer.Should().BeTrue("it is with you, to the assessor");
    }

    [Fact]
    public void ARequestNamingNoOne_WaitsWithNoName()
    {
        Holder(Seed("mini_cex_cpsa"), "requested", data: "{}").Should().Be(
            new ActivityHolderDto(ActivityHolderKind.Waiting, null, null, false, Since));
        Holder(Seed("mini_cex_cpsa"), "requested", data: "not json").Kind.Should().Be(ActivityHolderKind.Waiting);
    }

    [Fact]
    public void KgksTeachingLog_IsTheRegistrarsUntilLogged_ThenDone()
    {
        Holder(KgkTeachingLog, "draft", data: """{ "supervisor_user_id": "david" }""").Kind.Should().Be(ActivityHolderKind.Author);
        Holder(KgkTeachingLog, "logged").Kind.Should().Be(ActivityHolderKind.Done);
    }

    [Fact]
    public void NoWorkflow_OrAStateItDoesNotDeclare_WaitsWithNoName()
    {
        Holder(null, "requested").Kind.Should().Be(ActivityHolderKind.Waiting);
        Holder(Seed("mini_cex_cpsa"), "archived").Kind.Should().Be(ActivityHolderKind.Waiting);
    }

    [Fact]
    public void WithName_FillsTheUsersName_AndLeavesNoOneAlone()
    {
        ActivityHolders.WithName(Holder(Seed("mini_cex_cpsa"), "requested"), id => id == Assessor ? "David Naidoo" : id)
            .Name.Should().Be("David Naidoo");
        ActivityHolders.WithName(Holder(Seed("mini_cex_cpsa"), "completed"), _ => "anyone").Name.Should().BeNull();
    }

    [Theory]
    [InlineData("mini_cex_cpsa", "draft", "assessor_user_id")]
    [InlineData("mini_cex_cpsa", "requested", "assessor_user_id")]
    [InlineData("mini_cex_cpsa", "completed", "assessor_user_id")]
    [InlineData("mini_cex_cpsa", "declined", "assessor_user_id")]
    [InlineData("reflective_exercise_cpsa", "draft", "assessor_user_id")]
    [InlineData("portfolio_review_cpsa", "submitted", "assessor_user_id")]
    [InlineData("teaching_session", "submitted", null)]
    public void TheNominee_IsTheFieldTheWorkflowHandsItTo(string seed, string state, string? expected)
    {
        ActivityHolders.NomineeField(Seed(seed), state).Should().Be(expected);
    }

    /// <summary>C3: the log's Supervising consultant is a user field that receives nothing, so the log has no nominee.</summary>
    [Fact]
    public void KgksTeachingLog_HasNoNominee()
    {
        ActivityHolders.NomineeField(KgkTeachingLog, "draft").Should().BeNull();
        ActivityHolders.NomineeField(KgkTeachingLog, "logged").Should().BeNull();
        ActivityHolders.NomineeField(null, "draft").Should().BeNull();
    }

    /// <summary>Step 3.16: Dr Botha returned Ndlovu's reflection with a note; its newest move entered the draft state.</summary>
    [Fact]
    public void AReflectionSentBackToDraft_IsReturned_ByWhomWhenAndWithHerNote()
    {
        var returned = ActivityHolders.ReturnOf(
            Seed("reflective_exercise_cpsa"),
            "draft",
            Registrar,
            Registrar,
            new ActivityLastMove("submitted", "draft", "sarah", Since, "Say what you would do differently.", "return"));

        returned.Should().Be(new ActivityReturnDto("sarah", "sarah", Since, "Say what you would do differently."));
    }

    [Fact]
    public void ANewDraft_ARequest_AndAnActivityWithNoHistory_AreNotReturned()
    {
        var workflow = Seed("reflective_exercise_cpsa");

        ActivityHolders.ReturnOf(workflow, "draft", Registrar, Registrar, new ActivityLastMove("draft", "draft", Registrar, Since, null))
            .Should().BeNull("the create row runs from the first state to itself");
        ActivityHolders.ReturnOf(workflow, "submitted", Registrar, Registrar, new ActivityLastMove("draft", "submitted", Registrar, Since, null))
            .Should().BeNull();
        ActivityHolders.ReturnOf(workflow, "draft", Registrar, Registrar, null).Should().BeNull();
        ActivityHolders.ReturnOf(null, "draft", Registrar, Registrar, new ActivityLastMove("submitted", "draft", "sarah", Since, "x")).Should().BeNull();
    }

    /// <summary>
    /// R2: the demo reflective note's <c>revise</c> takes a declined note back to draft, by its subject. That is her own
    /// move, not a return, whether the mover or only the recorded move's rule gives it away.
    /// </summary>
    [Fact]
    public void TheAuthorsOwnMoveBackToDraft_IsNotAReturn()
    {
        var workflow = Seed("reflective_note");

        ActivityHolders.ReturnOf(workflow, "draft", Registrar, Registrar,
                new ActivityLastMove("declined", "draft", Registrar, Since, null, "revise"))
            .Should().BeNull("she revised her own declined note");
        ActivityHolders.ReturnOf(workflow, "draft", Registrar, "coordinator",
                new ActivityLastMove("declined", "draft", "coordinator", Since, null, "revise"))
            .Should().BeNull("the creator's move is the author's too");
        ActivityHolders.ReturnOf(workflow, "draft", Registrar, Registrar,
                new ActivityLastMove("declined", "draft", "someone-else", Since, null, "revise"))
            .Should().BeNull("revise is the subject's move alone, whoever the history says made it");

        ActivityHolders.ReturnOf(Seed("reflective_exercise_cpsa"), "draft", Registrar, Registrar,
                new ActivityLastMove("submitted", "draft", Registrar, Since, "x", "return"))
            .Should().BeNull("a mover who is the subject never returns work to herself");
    }

    [Theory]
    [InlineData("Mini-CEX (Paediatrics)", "PAED-003", true, true, true, "Mini-CEX (Paediatrics) · PAED-003 · 2026-09-25")]
    [InlineData("Mini-CEX (Paediatrics)", null, false, true, true, "Mini-CEX (Paediatrics) · no EPA yet · no date yet")]
    [InlineData("Mini-CEX (Paediatrics)", "PAED-003", false, true, true, "Mini-CEX (Paediatrics) · PAED-003 · no date")]
    [InlineData("Mini-CEX (Paediatrics)", null, true, true, true, "Mini-CEX (Paediatrics) · no EPA yet · 2026-09-25")]
    [InlineData("Journal Club", null, false, false, false, "Journal Club")]
    [InlineData("QI Project", null, false, false, true, "QI Project · no date")]
    public void AnActivity_IsNamedTypeEpaDate_WithEachSegmentItsFormHas(
        string type, string? epa, bool declared, bool formHasEpa, bool formHasDate, string expected)
    {
        ActivityDisplayNames.Compose(type, epa, new DateOnly(2026, 9, 25), declared, formHasEpa, formHasDate).Should().Be(expected);
    }

    /// <summary>E7: the nominee is added only when another activity shares the rest, and only when there is one.</summary>
    [Fact]
    public void TheNominee_IsAddedOnlyWhenAnotherActivitySharesTheRest()
    {
        const string name = "Mini-CEX (Paediatrics) · PAED-002 · 2026-09-09";

        ActivityDisplayNames.WithNominee(name, "Sarah Botha", sharesTheRest: true).Should().Be(name + " · Sarah Botha");
        ActivityDisplayNames.WithNominee(name, "Sarah Botha", sharesTheRest: false).Should().Be(name);
        ActivityDisplayNames.WithNominee(name, null, sharesTheRest: true).Should().Be(name);
    }
}
