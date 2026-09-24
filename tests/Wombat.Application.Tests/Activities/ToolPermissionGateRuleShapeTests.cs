using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Commands.RebuildCurriculumProgress;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T122: the EPA→tool gate against every shape of credit rule the DSL accepts, driven through the real
/// <see cref="ActivityService" />.
/// </summary>
/// <remarks>
/// <para>
/// The seeded CPSA types all credit one <c>epa_field</c>, so a gate tested only on seed shapes would pass while
/// being wrong for everything the builder can make. <c>CreditRulesParser</c> accepts a literal
/// <c>curriculum_item_id</c>, a <c>curriculum_item_field</c>, an <c>epa_field</c>, any mix of the three in one
/// directive, and any number of directives. The engine credits EVERY item those directives match, with
/// <c>curriculum_item_field</c> falling through to <c>epa_field</c> when it holds no parseable id. The gate
/// resolves through the engine's own <see cref="CreditTargetResolver" />, so these tests pin that the refusal
/// lands on exactly the items credit would land on, and that each sentence of the refusal names the field that
/// actually produced the match, or none for a literal.
/// </para>
/// <para>
/// They also pin the edges of D20 that the gate must NOT reach: a rebuild re-credits a completed activity whatever
/// the allow-list now says, and the MSF staged path, which credits nothing, never meets the gate. And they pin
/// the picker/gate parity the design promises: on a curriculum with tool lists, what the picker offers the gate
/// accepts, and what it withholds the gate refuses. The known divergences are pinned too, so nobody mistakes
/// them for bugs, or quietly "fixes" one side.
/// </para>
/// <para>
/// Every refusal is checked against the audit-pipeline trap (<see cref="CreditPlannedBeforeTransitionTests" />):
/// <c>AuditPipelineBehavior</c> saves the request's DbContext from its catch, so a refusal that left anything
/// Added, Modified or Deleted would commit it.
/// </para>
/// </remarks>
public sealed class ToolPermissionGateRuleShapeTests
{
    private const string Trainee = "trainee-1";
    private const int InstitutionId = 10;
    private const int OtherInstitutionId = 99;
    private const int SubSpecialityId = 1;
    private const int CurriculumId = 3000;
    private const int OtherCurriculumId = 3099;

    // EPAs on the trainee's curriculum, each with the tool list its item carries.
    private const int Paed001 = 5001;          // [cbd, mini_cex]            Mini-CEX permitted
    private const int Paed003 = 5003;          // no list                    unrestricted (D21)
    private const int Paed005 = 5005;          // [cbd, dops, msf]           Mini-CEX forbidden
    private const int Paed009 = 5009;          // [cbd, direct_observation]  Mini-CEX forbidden
    private const int LocalExtra = 5020;       // institution 10's own item, [dops]

    // EPAs that credit nothing for this trainee.
    private const int OtherInstitutionsExtra = 5021; // institution 99's item on the SAME curriculum
    private const int OffCurriculum = 5030;          // only on another curriculum

    private const int Item001 = 4001;
    private const int Item003 = 4003;
    private const int Item005 = 4005;
    private const int Item009 = 4009;
    private const int ItemLocal = 4020;
    private const int ItemOtherInstitution = 4021;
    private const int ItemOffCurriculum = 4030;

    private const string EpaLabel = "EPA being assessed";
    private const string ItemLabel = "Curriculum item reference";

    private const string EpaField = """{ "epa_field": "epa_id" }""";
    private const string ItemField = """{ "curriculum_item_field": "item_ref" }""";
    private const string ItemFieldThenEpaField = """{ "curriculum_item_field": "item_ref", "epa_field": "epa_id" }""";

    /// <summary>What a refusal sentence says about each EPA: its code, its title and the instruments its item accepts.</summary>
    private static readonly IReadOnlyDictionary<int, (string Code, string Title, string Accepted)> Catalogue =
        new Dictionary<int, (string Code, string Title, string Accepted)>
        {
            [Paed001] = ("PAED-001", "Managing an acute paediatric admission", "CBD or Mini-CEX"),
            [Paed005] = ("PAED-005", "Providing neonatal care", "CBD, DOPS or MSF"),
            [Paed009] = ("PAED-009", "Leading a ward round", "CBD or Direct observation"),
            [LocalExtra] = ("PAED-L01", "Running the sedation service", "DOPS")
        };

    // ─── 1. Two directives resolving two different items ────────────────────

    [Theory]
    [InlineData(Paed001, Item005, Paed005, false)]
    [InlineData(Paed009, Item001, Paed009, true)]
    public async Task TwoDirectives_OneItemForbidding_Refuses_NamingOnlyThatItem(
        int epaId, int literalItemId, int refusedEpaId, bool refusedThroughTheEpaField)
    {
        // The engine credits every item every directive matches, so one forbidden item is enough to refuse: the
        // other directive's permitted item does not launder it. Both directions are run, because the two
        // directives produce their matches differently (a schema field, and a literal), and the sentence must be
        // led by the field label only when a field produced the match.
        await using var db = NewSeededDatabase();
        var typeId = AddMiniCexType(db, CreditRules(EpaField, LiteralItem(literalItemId)));
        var service = Service(db);

        var message = await RefusalOfAsync(db, service, typeId, Data(epaId: epaId));

        message.Should().Be(
            Clause(refusedEpaId, refusedThroughTheEpaField ? EpaLabel : null),
            "one sentence, about the forbidden item only; the permitted one is not an error");
        SentenceCount(message).Should().Be(1);
        message.Should().NotContain(Catalogue[Paed001].Code, "PAED-001 is the permitted item in both cases");
    }

    [Fact]
    public async Task TwoDirectives_BothItemsPermitted_IsAccepted()
    {
        // The control for the theory above: the same two-directive type, with an unrestricted EPA in the field and a
        // permitting literal item. Without it, a gate that refused every multi-directive type would pass the theory.
        await using var db = NewSeededDatabase();
        var typeId = AddMiniCexType(db, CreditRules(EpaField, LiteralItem(Item001)));
        var service = Service(db);

        var created = await CreateAsync(service, typeId, Data(epaId: Paed003));

        created.CurrentState.Should().Be("draft");
        (await db.Activities.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task TwoDirectives_BothItemsForbidding_GiveOneSentencePerItem_EachWithItsOwnInstruments()
    {
        // Critique change 11: one clause per refused item, each naming THAT item's accepted instruments. A single
        // merged "the curriculum accepts ..." would tell the trainee that DOPS or MSF would do for PAED-009, which
        // is false, and they would file again and be refused again. The EPA-field clause is led by that field's
        // label from the schema (not the key, and not a hard-coded "EPA"); the literal item's clause has no label,
        // because no field the trainee can change produced it.
        await using var db = NewSeededDatabase();
        var typeId = AddMiniCexType(db, CreditRules(EpaField, LiteralItem(Item005)));
        var service = Service(db);

        var message = await RefusalOfAsync(db, service, typeId, Data(epaId: Paed009));

        message.Should().Be(
            Clause(Paed009, EpaLabel) + " " + Clause(Paed005, null),
            "two sentences, in directive order, each with its own item's instruments");
        SentenceCount(message).Should().Be(2);
    }

    [Fact]
    public async Task TwoDirectives_ResolvingTheSameItem_RefuseOnce()
    {
        // The gate unions matched items by id before judging them, exactly as the engine credits an item once per
        // activity. Two directives landing on PAED-005 must not produce the same complaint twice.
        await using var db = NewSeededDatabase();
        var typeId = AddMiniCexType(db, CreditRules(EpaField, LiteralItem(Item005)));
        var service = Service(db);

        var message = await RefusalOfAsync(db, service, typeId, Data(epaId: Paed005));

        SentenceCount(message).Should().Be(1);
        message.Should().Contain(Catalogue[Paed005].Code);
    }

    // ─── 2. A literal curriculum_item_id ────────────────────────────────────

    [Fact]
    public async Task ALiteralCurriculumItemId_WhoseItemForbidsTheTool_IsRefused_WithNoFieldLabel()
    {
        // A literal rule never reads the form, so nothing the trainee typed produced the match. Leading the
        // sentence with a field label ("EPA being assessed: ...") would point them at a field whose value is
        // irrelevant. The data here names a permitted EPA and a permitting item on purpose: neither is read.
        await using var db = NewSeededDatabase();
        var typeId = AddMiniCexType(db, CreditRules(LiteralItem(Item005)));
        var service = Service(db);

        var message = await RefusalOfAsync(db, service, typeId, Data(epaId: Paed001, itemRef: Item001.ToString()));

        message.Should().Be(Clause(Paed005, null), "the EPA code and title suffice for a literal item");
        message.Should().NotContain(EpaLabel).And.NotContain(ItemLabel);
    }

    // ─── 3. A curriculum_item_field that parses ─────────────────────────────

    [Fact]
    public async Task ACurriculumItemField_ThatParses_IsEvaluatedAsThatItem_LedByItsLabel()
    {
        // The field holds a curriculum item id as text ("4005"), which the engine's string-or-number parse accepts.
        // The gate must judge item 4005 (PAED-005), and name the item field, since that is the field to change.
        await using var db = NewSeededDatabase();
        var typeId = AddMiniCexType(db, CreditRules(ItemField));
        var service = Service(db);

        var message = await RefusalOfAsync(db, service, typeId, Data(itemRef: Item005.ToString()));

        message.Should().Be(Clause(Paed005, ItemLabel));
    }

    [Fact]
    public async Task ACurriculumItemField_NamingAPermittingItem_IsAccepted()
    {
        // The control: the same type, pointed at PAED-001, whose list names Mini-CEX.
        await using var db = NewSeededDatabase();
        var typeId = AddMiniCexType(db, CreditRules(ItemField));
        var service = Service(db);

        var created = await CreateAsync(service, typeId, Data(itemRef: Item001.ToString()));

        created.CurrentState.Should().Be("draft");
    }

    [Fact]
    public async Task AMixedDirective_WhoseItemFieldParses_IsJudgedOnTheItem_NotTheEpa()
    {
        // The engine's precedence: a parseable curriculum_item_field wins and epa_field is never read. The gate
        // must follow it both ways, or it refuses encounters that credit a permitted item and passes ones that
        // credit a forbidden item.
        await using var db = NewSeededDatabase();
        var typeId = AddMiniCexType(db, CreditRules(ItemFieldThenEpaField));
        var service = Service(db);

        // A forbidden item beside a permitted EPA: refused, and led by the item field.
        var message = await RefusalOfAsync(db, service, typeId, Data(epaId: Paed001, itemRef: Item005.ToString()));
        message.Should().Be(Clause(Paed005, ItemLabel));

        // A permitted item beside a forbidden EPA: accepted, because credit lands on item 4001 and never reads epa_id.
        var created = await CreateAsync(service, typeId, Data(epaId: Paed005, itemRef: Item001.ToString()));
        created.CurrentState.Should().Be("draft");
    }

    // ─── 4. A mixed directive whose item field does not parse ───────────────

    [Fact]
    public async Task AMixedDirective_WhoseItemFieldDoesNotParse_FallsThroughToTheEpaField_AndIsRefused()
    {
        // CreditTargetResolver falls through from an unparseable curriculum_item_field to epa_field. "abc" is not an
        // id, so the engine would credit PAED-005 through epa_id, and the gate refuses it, led by the EPA field.
        //
        // DOCUMENTED DIVERGENCE (T122 critique change 11; CreditRuleFields remarks; ToolPermissionGate remarks): the
        // picker does NOT narrow this case. CreditRuleFields skips any directive that names a curriculum_item_field,
        // so ActivityForm builds the epa_id scope with NarrowToCreditable = false and the claims arm offers PAED-005.
        // The gate is the authority here, and a trainee on such a builder-made type can be refused at submit for an
        // EPA the picker offered. The asserts below pin both halves. If the picker is ever taught to narrow mixed
        // rules, update this test and both remarks together; do not relax the gate to match the picker.
        await using var db = NewSeededDatabase();
        var rules = CreditRules(ItemFieldThenEpaField);
        var typeId = AddMiniCexType(db, rules);
        var service = Service(db);

        var message = await RefusalOfAsync(db, service, typeId, Data(epaId: Paed005, itemRef: "abc"));

        message.Should().Be(Clause(Paed005, EpaLabel), "the match came from epa_id, so epa_id's label leads");

        CreditRuleFields.ResolveCreditedEpaFieldKeys(rules).Should().NotContain(
            "epa_id", "pinned divergence: the form does not treat a fall-through epa_field as the credited field");

        var offered = await PickerAsync(db, TraineeWithClaims(), new EpaOptionScope(Trainee, NarrowToCreditable: false, CurrentValue: null, WbaToolKey: "mini_cex"));
        offered.Should().Contain(Paed005, "pinned divergence: the claims arm offers what the gate refuses");
    }

    // ─── 5. The rebuild never re-checks (D20) ───────────────────────────────

    [Fact]
    public async Task Rebuild_ReCreditsACompletedActivity_WhoseToolTheItemNowForbids()
    {
        // D20: the list binds the write path, never credit. A Mini-CEX completed against
        // PAED-005 while its list named Mini-CEX stays credited after an administrator removes Mini-CEX from the
        // list, including across a rebuild, which zeroes every row and replays every terminal activity. A rebuild
        // that consulted the list would delete evidence collected under the rule in force at the time. This is dev
        // activity 11's case (a Mini-CEX on PAED-011).
        var options = NewDatabase();
        int activityId;

        await using (var db = new ApplicationDbContext(options))
        {
            Seed(db);
            db.CurriculumItems.Single(item => item.Id == Item005).PermittedToolsJson =
                CurriculumItem.NormalizePermittedToolsJson(["cbd", "dops", "mini_cex", "msf"]);
            var typeId = AddMiniCexType(db, CreditsTheEpaWithALevel);

            var service = Service(db);
            var principal = TraineePrincipal();
            var draft = await CreateAsync(service, typeId, Data(epaId: Paed005));
            await service.TransitionAsync(new TransitionActivityInput(draft.Id, "submit", Trainee, principal, null, null), CancellationToken.None);
            await service.TransitionAsync(new TransitionActivityInput(draft.Id, "complete", Trainee, principal, null, null), CancellationToken.None);
            activityId = draft.Id;
        }

        await using (var db = new ApplicationDbContext(options))
        {
            (await db.CurriculumItemProgresses.SingleAsync()).CountsSoFar.Should().Be(1, "guard: the live completion credited");

            // The administrator's edit: Mini-CEX comes off PAED-005's list.
            db.CurriculumItems.Single(item => item.Id == Item005).PermittedToolsJson =
                CurriculumItem.NormalizePermittedToolsJson(["cbd", "dops", "msf"]);
            await db.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            // Guard: the edit really does bite the write path, so the rebuild below is replaying something the gate
            // would now refuse.
            var typeId = await db.ActivityTypes.Where(type => type.Key == "mini_cex_under_test").Select(type => type.Id).SingleAsync();
            var refusal = async () => await CreateAsync(Service(db), typeId, Data(epaId: Paed005));
            await refusal.Should().ThrowAsync<InvalidOperationException>().WithMessage("*cannot be used as evidence for PAED-005*");
        }

        await using (var db = new ApplicationDbContext(options))
        {
            var handler = new RebuildCurriculumProgressCommandHandler(db, new CreditApplier(db));
            var result = await handler.Handle(new RebuildCurriculumProgressCommand(Administrator()), CancellationToken.None);

            result.ActivitiesReplayed.Should().Be(1);
            result.CreditApplications.Should().Be(1, "the engine does not re-check the allow-list (D20)");
            result.ProgressRowsWritten.Should().Be(1);
            result.ProgressRowsRemoved.Should().Be(0, "a rebuild that re-checked tools would remove this row");
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            var row = await verify.CurriculumItemProgresses.AsNoTracking().SingleAsync();
            row.CurriculumItemId.Should().Be(Item005);
            row.CountsSoFar.Should().Be(1);
            row.CreditedActivityKeysJson.Should().Be($$"""["{{activityId}}:complete"]""");

            var completion = await verify.ActivityTransitions.SingleAsync(transition =>
                transition.ActivityId == activityId && transition.TransitionKey == "complete");
            completion.CreditedItemCount.Should().Be(1, "the rebuild re-stamps the credit it re-applied");
        }
    }

    // ─── 6. The MSF staged path ──────────────────────────────────────────────

    [Fact]
    public async Task StageCompleted_AnMsfKeyedTypeThatCreditsNothing_RecordsNormally_AgainstAnEpaWhoseListExcludesMsf()
    {
        // T121's release writes one msf_cpsa record per covered EPA through StageCompletedAsync, and msf_cpsa is keyed
        // 'msf'. PAED-009's list does not name MSF. The record must still be written: counts_for is [] (D8), so it
        // credits nothing and there is nothing for the list to protect. The staged path carries no gate at all
        // (it refuses every crediting type first); this pins that a keyed, non-crediting type is not caught by any
        // check T122 added. A release that dropped EPAs here would lose a campaign's evidence silently.
        await using var db = NewSeededDatabase();
        var typeId = AddType(db, id: 700, key: "msf_under_test", wbaToolKey: "msf", CreditsNothing, MsfWorkflowJson);
        ParsePermittedToolsOf(db, Item009).Should().NotContain("msf", "guard: the subject's item for this EPA excludes MSF");

        var service = Service(db);
        var recorded = await service.StageCompletedAsync(
            new RecordCompletedActivitiesInput(
                "msf_under_test",
                Trainee,
                "coordinator-1",
                "record",
                [Data(epaId: Paed009), Data(epaId: Paed005)],
                CoordinatorPrincipal()),
            CancellationToken.None);
        await db.SaveChangesAsync();

        recorded.Should().Be(2);
        var activities = await db.Activities.Include(activity => activity.Transitions).Where(activity => activity.ActivityTypeId == typeId).ToListAsync();
        activities.Should().HaveCount(2).And.OnlyContain(activity => activity.CurrentState == "recorded");
        activities.Should().OnlyContain(activity => activity.Transitions.Any(transition => transition.TransitionKey == "record"));
        (await db.CurriculumItemProgresses.CountAsync()).Should().Be(0, "D8: MSF evidence credits nothing");
    }

    [Fact]
    public async Task CreateDraft_AnMsfKeyedTypeThatCreditsNothing_IsNotRefusedEither()
    {
        // The interactive twin of the staged test: the gate itself returns before resolving anything when the pinned
        // rules credit nothing, so a hand-created msf draft about a PAED-009 encounter is not refused on tool grounds.
        await using var db = NewSeededDatabase();
        var typeId = AddType(db, id: 701, key: "msf_draft_under_test", wbaToolKey: "msf", CreditsNothing, WorkflowJson);

        var created = await CreateAsync(Service(db), typeId, Data(epaId: Paed009));

        created.CurrentState.Should().Be("draft");
    }

    // ─── 7. The picker and the gate agree ───────────────────────────────────

    [Theory]
    [InlineData("mini_cex", new[] { Paed001, Paed003 })]
    [InlineData("cbd", new[] { Paed001, Paed003, Paed005, Paed009 })]
    [InlineData("dops", new[] { Paed003, Paed005, LocalExtra })]
    [InlineData(" MSF ", new[] { Paed003, Paed005 })]
    [InlineData("portfolio_review", new[] { Paed003 })]
    [InlineData(null, new[] { Paed001, Paed003, Paed005, Paed009, LocalExtra })]
    public async Task ThePickerAndTheGateAgree_OnEveryCreditableEpa(string? wbaToolKey, int[] expectedOffered)
    {
        // D20's promise: one predicate, two callers. For a subject whose curriculum carries tool lists, every EPA
        // the picker offers must be accepted at create, and every creditable EPA it withholds must be refused. A
        // disagreement one way is an EPA a trainee can pick and never file; the other way is an EPA they could
        // file and are never shown.
        //
        // The cases cover a permitted list, an unrestricted item (PAED-003, offered to every tool), the subject's
        // own institution's local extra (credit reaches it, so both halves must judge it), a key stored with stray
        // casing and padding (both halves normalise), a tool no list names (only the unrestricted item is left),
        // and an unkeyed type (D21: nothing is narrowed and nothing is refused).
        await using var db = NewSeededDatabase();
        var typeId = AddType(db, id: 800, key: "tool_under_test", wbaToolKey, CreditRules(EpaField), WorkflowJson);
        var service = Service(db);
        int[] creditable = [Paed001, Paed003, Paed005, Paed009, LocalExtra];

        var offered = await PickerAsync(db, TraineePrincipal(), new EpaOptionScope(Trainee, NarrowToCreditable: true, CurrentValue: null, WbaToolKey: wbaToolKey));

        offered.Should().BeEquivalentTo(expectedOffered, "guard: the fixture's expected narrowing");
        offered.Should().BeSubsetOf(creditable, "T108: the narrowing arm never offers what cannot credit");
        offered.Should().NotBeEmpty();

        foreach (var epaId in offered)
        {
            var create = async () => await CreateAsync(service, typeId, Data(epaId: epaId));
            await create.Should().NotThrowAsync($"the picker offered EPA {epaId} for tool '{wbaToolKey}'");
        }

        foreach (var epaId in creditable.Except(offered))
        {
            var message = await RefusalOfAsync(db, service, typeId, Data(epaId: epaId));
            message.Should().Contain(
                "cannot be used as evidence for " + Catalogue[epaId].Code,
                $"the picker withheld EPA {epaId} for tool '{wbaToolKey}', so the gate must refuse it");
        }
    }

    [Theory]
    [InlineData("mini_cex", new[] { Paed001, Paed003 })]
    [InlineData("cbd", new[] { Paed001, Paed003, Paed005, Paed009 })]
    [InlineData("dops", new[] { Paed003, Paed005, LocalExtra })]
    [InlineData("portfolio_review", new[] { Paed003 })]
    public async Task ThePickerAndTheGateAgree_ForAnInstrumentThatCreditsNothing_OnItsEvidenceEpa(string wbaToolKey, int[] expectedOffered)
    {
        // T154: an unrated instrument credits nothing, but it is held to the lists by the EPA it is evidence for. The form
        // decides to narrow that field by the rule the gate uses to judge it (CreditRuleFields.ResolveNarrowedEpaFieldKeys
        // and ToolPermissionGate.GatedTargets), so the two agree exactly as they do for a crediting type above.
        await using var db = NewSeededDatabase();
        var typeId = AddType(db, id: 802, key: "unrated_tool_under_test", wbaToolKey, CreditsNothing, WorkflowJson, EvidenceEpaSchemaJson);
        var service = Service(db);
        int[] onTheCurriculum = [Paed001, Paed003, Paed005, Paed009, LocalExtra];

        CreditRuleFields.ResolveNarrowedEpaFieldKeys(CreditsNothing, "epa_id", wbaToolKey)
            .Should().BeEquivalentTo(["epa_id"], "the form narrows the evidence EPA of an instrument that credits nothing");

        var offered = await PickerAsync(db, TraineePrincipal(), new EpaOptionScope(Trainee, NarrowToCreditable: true, CurrentValue: null, WbaToolKey: wbaToolKey));
        offered.Should().BeEquivalentTo(expectedOffered, "guard: the fixture's expected narrowing");

        foreach (var epaId in offered)
        {
            var create = async () => await CreateAsync(service, typeId, Data(epaId: epaId));
            await create.Should().NotThrowAsync($"the picker offered EPA {epaId} for tool '{wbaToolKey}'");
        }

        foreach (var epaId in onTheCurriculum.Except(offered))
        {
            var message = await RefusalOfAsync(db, service, typeId, Data(epaId: epaId));
            message.Should().Contain(
                "cannot be used as evidence for " + Catalogue[epaId].Code,
                $"the picker withheld EPA {epaId} for tool '{wbaToolKey}', so the gate must refuse it");
        }
    }

    [Fact]
    public async Task PinnedDivergence_AToolNoItemPermits_ThePickerFallsBackToTheCreditableSet_AndTheGateRefusesEachWithAReason()
    {
        // The one place the parity above is broken by design (design §4). When no item on the subject's curriculum
        // admits the tool, the picker returns the whole T108 set rather than an empty required select (unsubmittable,
        // and silent) or the claims filter (T108's defect: EPAs that credit nothing). Every choice is then refused at
        // create with a sentence naming what the curriculum does accept, which is the one answer the trainee can act
        // on. trainee-2's curriculum has no unrestricted item, so an 'rca' type meets exactly this case.
        await using var db = NewSeededDatabase();
        AddSecondTraineeOnAFullyRestrictedCurriculum(db);
        var typeId = AddType(db, id: 801, key: "rca_under_test", wbaToolKey: "rca", CreditRules(EpaField), WorkflowJson);
        var service = Service(db);

        var offered = await PickerAsync(db, Principal("trainee-2"), new EpaOptionScope("trainee-2", NarrowToCreditable: true, CurrentValue: null, WbaToolKey: "rca"));

        offered.Should().BeEquivalentTo(new[] { Paed001, Paed005 }, "the fallback is the creditable set, never [] and never the claims filter");

        foreach (var epaId in offered)
        {
            var create = async () => await service.CreateDraftAsync(
                new CreateActivityInput(typeId, "trainee-2", "trainee-2", Data(epaId: epaId), Principal("trainee-2")),
                CancellationToken.None);
            var refusal = (await create.Should().ThrowAsync<InvalidOperationException>()).Which;
            refusal.Message.Should().StartWith($"{EpaLabel}: Root cause analysis cannot be used as evidence for {Catalogue[epaId].Code}");
            await ShouldLeaveNothingForTheAuditSaveAsync(db);
        }
    }

    [Fact]
    public async Task PinnedDivergence_AnEpaOutsideTheCurriculum_IsHiddenByThePicker_ButNotRefusedByTheGate()
    {
        // The parity claim is over CREDITABLE EPAs. An EPA with no item on the subject's curriculum (another
        // institution's local extra on the same curriculum; an item on a different curriculum version) is hidden by
        // the picker (T108), and the gate lets it through, because it credits nothing and D21 left curriculum
        // membership undecided. It is reachable only by a crafted request, and it produces an activity that credits
        // no item, which T108's completion signal already reports.
        await using var db = NewSeededDatabase();
        var typeId = AddMiniCexType(db, CreditRules(EpaField));
        var service = Service(db);

        var offered = await PickerAsync(db, TraineePrincipal(), new EpaOptionScope(Trainee, NarrowToCreditable: true, CurrentValue: null, WbaToolKey: "mini_cex"));
        offered.Should().NotContain(new[] { OtherInstitutionsExtra, OffCurriculum });

        foreach (var epaId in new[] { OtherInstitutionsExtra, OffCurriculum })
        {
            var create = async () => await CreateAsync(service, typeId, Data(epaId: epaId));
            await create.Should().NotThrowAsync($"EPA {epaId} has no item for this trainee, so nothing is protected");
        }
    }

    // ─── Harness ─────────────────────────────────────────────────────────────

    private static ActivityService Service(ApplicationDbContext db)
        => new(db, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator());

    private static Task<ActivityDto> CreateAsync(ActivityService service, int typeId, string dataJson)
        => service.CreateDraftAsync(
            new CreateActivityInput(typeId, Trainee, Trainee, dataJson, TraineePrincipal()),
            CancellationToken.None);

    /// <summary>Creates, expects a refusal, checks the audit trap, and returns the message.</summary>
    private static async Task<string> RefusalOfAsync(ApplicationDbContext db, ActivityService service, int typeId, string dataJson)
    {
        var before = await db.Activities.CountAsync();
        var create = async () => await CreateAsync(service, typeId, dataJson);

        var refusal = (await create.Should().ThrowAsync<InvalidOperationException>()).Which;

        await ShouldLeaveNothingForTheAuditSaveAsync(db);
        (await db.Activities.CountAsync()).Should().Be(before, "a refused create leaves no draft behind");
        return refusal.Message;
    }

    /// <summary>
    /// The audit pipeline's catch saves the request's DbContext, so a refusal must leave nothing Added, Modified or
    /// Deleted for it to commit. Then does what that catch does: saves.
    /// </summary>
    private static async Task ShouldLeaveNothingForTheAuditSaveAsync(ApplicationDbContext db)
    {
        db.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Should().BeEmpty("the audit pipeline's catch saves this context, so anything dirty here would be committed");

        (await db.SaveChangesAsync()).Should().Be(0);
    }

    private static async Task<IReadOnlyList<int>> PickerAsync(ApplicationDbContext db, ClaimsPrincipal principal, EpaOptionScope scope)
    {
        var picker = new ActivityReferenceDataService(db);
        var options = await picker.GetEpaOptionsAsync(principal, scope, CancellationToken.None);
        return options.Select(option => int.Parse(option.Value, System.Globalization.CultureInfo.InvariantCulture)).ToList();
    }

    /// <summary>The expected refusal sentence for a Mini-CEX filed against an EPA, optionally led by a field label.</summary>
    private static string Clause(int epaId, string? label)
    {
        var (code, title, accepted) = Catalogue[epaId];
        var sentence = $"Mini-CEX cannot be used as evidence for {code} — {title}. The curriculum accepts {accepted} for this EPA.";
        return label is null ? sentence : $"{label}: {sentence}";
    }

    private static int SentenceCount(string message)
        => Regex.Matches(message, "cannot be used as evidence for").Count;

    private static IReadOnlyList<string> ParsePermittedToolsOf(ApplicationDbContext db, int itemId)
        => CurriculumItem.ParsePermittedTools(db.CurriculumItems.Single(item => item.Id == itemId).PermittedToolsJson);

    private static string CreditRules(params string[] curriculumItemMatches)
        => "{ \"counts_for\": [ "
           + string.Join(", ", curriculumItemMatches.Select(match => "{ \"curriculum_item_match\": " + match + ", \"amount\": 1 }"))
           + " ] }";

    private static string LiteralItem(int curriculumItemId)
        => "{ \"curriculum_item_id\": " + curriculumItemId.ToString(System.Globalization.CultureInfo.InvariantCulture) + " }";

    private static string Data(int? epaId = null, string? itemRef = null)
    {
        var properties = new Dictionary<string, object> { ["score"] = 4 };
        if (epaId is int epa)
        {
            properties["epa_id"] = epa;
        }

        if (itemRef is not null)
        {
            properties["item_ref"] = itemRef;
        }

        return JsonSerializer.Serialize(properties);
    }

    private static ClaimsPrincipal TraineePrincipal() => Principal(Trainee);

    /// <summary>The trainee as the claims arm sees them: scoped to the sub-speciality every EPA here hangs off.</summary>
    private static ClaimsPrincipal TraineeWithClaims()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, Trainee),
                new Claim(WombatClaimTypes.SubSpecialityId, SubSpecialityId.ToString(System.Globalization.CultureInfo.InvariantCulture))
            ],
            "test"));

    private static ClaimsPrincipal CoordinatorPrincipal()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "coordinator-1"),
                new Claim(ClaimTypes.Role, WombatRoles.Coordinator)
            ],
            "test"));

    private static ClaimsPrincipal Administrator()
        => new(new ClaimsIdentity([new Claim(ClaimTypes.Role, WombatRoles.Administrator)], "test"));

    private static ClaimsPrincipal Principal(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));

    private static DbContextOptions<ApplicationDbContext> NewDatabase()
        => new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static ApplicationDbContext NewSeededDatabase()
    {
        var db = new ApplicationDbContext(NewDatabase());
        Seed(db);
        return db;
    }

    private static void Seed(ApplicationDbContext db)
    {
        // The vocabulary, with the display names a refusal speaks.
        db.WbaTools.AddRange(
            new WbaTool { Id = 1, Key = "mini_cex", Name = "Mini-CEX" },
            new WbaTool { Id = 2, Key = "cbd", Name = "CBD" },
            new WbaTool { Id = 3, Key = "dops", Name = "DOPS" },
            new WbaTool { Id = 4, Key = "msf", Name = "MSF" },
            new WbaTool { Id = 5, Key = "direct_observation", Name = "Direct observation" },
            new WbaTool { Id = 6, Key = "portfolio_review", Name = "Portfolio review" },
            new WbaTool { Id = 7, Key = "rca", Name = "Root cause analysis" });

        db.Epas.AddRange(
            new Epa { Id = Paed001, SubSpecialityId = SubSpecialityId, Code = "PAED-001", Title = "Managing an acute paediatric admission" },
            new Epa { Id = Paed003, SubSpecialityId = SubSpecialityId, Code = "PAED-003", Title = "Managing chronic illness" },
            new Epa { Id = Paed005, SubSpecialityId = SubSpecialityId, Code = "PAED-005", Title = "Providing neonatal care" },
            new Epa { Id = Paed009, SubSpecialityId = SubSpecialityId, Code = "PAED-009", Title = "Leading a ward round" },
            new Epa { Id = LocalExtra, SubSpecialityId = SubSpecialityId, Code = "PAED-L01", Title = "Running the sedation service", OwningInstitutionId = InstitutionId },
            new Epa { Id = OtherInstitutionsExtra, SubSpecialityId = SubSpecialityId, Code = "PAED-L99", Title = "Another hospital's extra", OwningInstitutionId = OtherInstitutionId },
            new Epa { Id = OffCurriculum, SubSpecialityId = SubSpecialityId, Code = "PAED-X01", Title = "On another curriculum version" });

        // PAED-001's list is stored with stray casing, padding and a duplicate, as only direct SQL could store it. Both
        // the picker and the gate read it through CurriculumItem.ParsePermittedTools, so both must see [cbd, mini_cex].
        db.CurriculumItems.AddRange(
            Item(Item001, CurriculumId, Paed001, """[" Mini_CEX ", "cbd", "CBD"]"""),
            Item(Item003, CurriculumId, Paed003, null),
            Item(Item005, CurriculumId, Paed005, CurriculumItem.NormalizePermittedToolsJson(["cbd", "dops", "msf"])),
            Item(Item009, CurriculumId, Paed009, CurriculumItem.NormalizePermittedToolsJson(["cbd", "direct_observation"])),
            Item(ItemLocal, CurriculumId, LocalExtra, CurriculumItem.NormalizePermittedToolsJson(["dops"]), InstitutionId),
            Item(ItemOtherInstitution, CurriculumId, OtherInstitutionsExtra, CurriculumItem.NormalizePermittedToolsJson(["cbd"]), OtherInstitutionId),
            Item(ItemOffCurriculum, OtherCurriculumId, OffCurriculum, CurriculumItem.NormalizePermittedToolsJson(["cbd"])));

        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = Trainee,
            InstitutionId = InstitutionId,
            CurriculumId = CurriculumId,
            ProgrammeStartDate = new DateOnly(2025, 1, 1),
            ExpectedCompletionDate = new DateOnly(2029, 1, 1),
            IsActive = true
        });

        db.SaveChanges();
    }

    /// <summary>trainee-2 is on a curriculum where every item carries a list, so a tool no list names empties the permitted set.</summary>
    private static void AddSecondTraineeOnAFullyRestrictedCurriculum(ApplicationDbContext db)
    {
        const int restrictedCurriculumId = 3001;

        db.CurriculumItems.AddRange(
            Item(4101, restrictedCurriculumId, Paed001, CurriculumItem.NormalizePermittedToolsJson(["cbd", "mini_cex"])),
            Item(4105, restrictedCurriculumId, Paed005, CurriculumItem.NormalizePermittedToolsJson(["cbd", "dops", "msf"])));

        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 2,
            UserId = "trainee-2",
            InstitutionId = InstitutionId,
            CurriculumId = restrictedCurriculumId,
            ProgrammeStartDate = new DateOnly(2025, 1, 1),
            ExpectedCompletionDate = new DateOnly(2029, 1, 1),
            IsActive = true
        });

        db.SaveChanges();
    }

    private static CurriculumItem Item(int id, int curriculumId, int epaId, string? permittedToolsJson, int? owningInstitutionId = null)
        => new()
        {
            Id = id,
            CurriculumId = curriculumId,
            EpaId = epaId,
            OwningInstitutionId = owningInstitutionId,
            RequiredCount = 3,
            MinimumLevelOrder = 3,
            WindowMonths = 12,
            PermittedToolsJson = permittedToolsJson
        };

    private static int AddMiniCexType(ApplicationDbContext db, string creditRulesJson)
        => AddType(db, id: 600, key: "mini_cex_under_test", wbaToolKey: "mini_cex", creditRulesJson, WorkflowJson);

    private static int AddType(
        ApplicationDbContext db,
        int id,
        string key,
        string? wbaToolKey,
        string creditRulesJson,
        string workflowJson,
        string schemaJson = SchemaJson)
    {
        var publishedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var activityType = new ActivityType
        {
            Id = id,
            Key = key,
            Name = key,
            Scope = ActivityScope.Institution,
            ScopeId = InstitutionId,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditRulesJson,
            DisplayFieldsJson = """["epa_id","score"]""",
            OwnerUserId = "admin-1",
            CreatedOn = publishedOn,
            WbaToolKey = wbaToolKey
        };

        activityType.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = id,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditRulesJson,
            DisplayFieldsJson = activityType.DisplayFieldsJson,
            PublishedByUserId = "admin-1",
            PublishedOn = publishedOn
        });

        db.ActivityTypes.Add(activityType);
        db.SaveChanges();
        return id;
    }

    private const string CreditsNothing = """{ "counts_for": [] }""";

    private const string CreditsTheEpaWithALevel = """
        {
          "counts_for": [
            { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1, "minimum_level_field": "score" }
          ]
        }
        """;

    // Labels deliberately differ from the keys and from the word "EPA" alone, so a refusal that leads with a key, or
    // with a hard-coded word, cannot pass for one that leads with the schema's label.
    private const string SchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "details",
              "title": "Details",
              "fields": [
                { "key": "epa_id", "type": "epa", "label": "EPA being assessed" },
                { "key": "item_ref", "type": "text", "label": "Curriculum item reference" },
                { "key": "score", "type": "number", "label": "Score" }
              ]
            }
          ]
        }
        """;

    /// <summary>The same form, declaring its EPA field as the EPA each activity is evidence for (T137).</summary>
    private static readonly string EvidenceEpaSchemaJson = SchemaJson.Replace(
        "\"version\": 1,",
        "\"version\": 1, \"evidence_epa_field\": \"epa_id\",",
        StringComparison.Ordinal);

    private const string WorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "submitted", "label": "Submitted" },
            { "key": "completed", "label": "Completed", "terminal": true }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject" },
            { "key": "complete", "from": "submitted", "to": "completed", "actor": "subject" }
          ]
        }
        """;

    // The msf_cpsa shape: created in draft, recorded straight to terminal by a coordinator.
    private const string MsfWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "recorded", "label": "Recorded", "terminal": true }
          ],
          "transitions": [
            { "key": "record", "from": "draft", "to": "recorded", "actor": "role:Coordinator|role:Administrator" }
          ]
        }
        """;
}
