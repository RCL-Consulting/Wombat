using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T102, the nominee gate at CREATE and the eligibility truth table, driven through the real
/// <see cref="ActivityService.CreateDraftAsync" />.
/// </summary>
/// <remarks>
/// <para>
/// A nominee field is every <c>user</c> field plus every field a <c>field:</c> rule names. At create, stored data is
/// empty, so every non-empty nominee value is "changed" and is judged. A judged value is permitted iff it is the exact
/// (ordinal, untrimmed) id of a user who holds every required role, belongs to the activity's stamped institution (a
/// null stamp admits nobody), is not deactivated (<c>LockoutEnd</c> at the far-future threshold; a brute-force lockout
/// of minutes does not count), and is not the subject. There is no Administrator bypass.
/// </para>
/// <para>
/// The truth table is built around one eligible control, <see cref="AssessorId" />. Every refused nominee differs from
/// it in exactly one condition, so each refusal test pins one condition and would pass for no other reason.
/// </para>
/// <para>
/// Every refusal is also checked against the audit trap. <c>AuditPipelineBehavior</c>'s catch saves the request's
/// shared DbContext, so a refusal thrown after any mutation would COMMIT that mutation under a failed command. Each
/// refusal asserts the change tracker holds nothing Added, Modified or Deleted, then performs the audit save itself
/// and asserts it writes nothing (the <see cref="ToolPermissionGateTests" /> template), and that no activity was left
/// behind.
/// </para>
/// <para>
/// Each call runs on its own DbContext over one InMemory store, as each request does in the product.
/// </para>
/// </remarks>
public sealed class NomineeGateTests
{
    private const int InstitutionId = 10;
    private const int OtherInstitutionId = 20;

    private const string TraineeId = "trainee-1";
    private const string SecondTraineeId = "trainee-2";

    /// <summary>A trainee with neither a profile nor an institution on their Identity row: their activities stamp null.</summary>
    private const string UnplacedTraineeId = "trainee-unplaced";

    /// <summary>A trainee whose Identity row says the other institution while their active profile says ours.</summary>
    private const string TransferredTraineeId = "trainee-transferred";

    /// <summary>The eligible control: an Assessor at the trainee's institution, not locked, not the subject.</summary>
    private const string AssessorId = "assessor-1";

    private const string NoRoleUserId = "coordinator-1";
    private const string OtherInstitutionAssessorId = "assessor-elsewhere";
    private const string DeactivatedAssessorId = "assessor-deactivated";
    private const string BruteForcedAssessorId = "assessor-brute-forced";
    private const string ExpiredLockoutAssessorId = "assessor-lockout-expired";
    private const string UnplacedAssessorId = "assessor-unplaced";
    private const string CommitteeMemberId = "committee-1";
    private const string AssessorAndCommitteeMemberId = "assessor-committee-1";
    private const string AdministratorId = "admin-1";
    private const string AdministratorAssessorElsewhereId = "admin-assessor-elsewhere";
    private const string OtherInstitutionCoordinatorId = "coordinator-elsewhere";
    private const string NobodyId = "no-such-user";

    private const int StandardTypeId = 200;
    private const int CommitteeTypeId = 201;
    private const int DuplicateKeyTypeId = 202;
    private const int TextFieldRuleTypeId = 203;
    private const int HiddenFieldTypeId = 204;
    private const int SchemaOrderTypeId = 205;

    /// <summary>The fragment every eligibility refusal carries, so a test can tell the gate apart from any other refusal.</summary>
    private const string GateRefusal = "cannot be named here";

    /// <summary>The fragment the gate's refusal of a non-string value carries.</summary>
    private const string NotAPersonRefusal = "must name a person";

    // ---- the eligible control ---------------------------------------------------------------------------------

    [Fact]
    public async Task AnEligibleAssessor_AtTheTraineesInstitution_IsAccepted_AndStoredExactlyAsNamed()
    {
        // The control for everything below: a gate that refused too much would pass every refusal test.
        var options = NewDatabase();
        await SeedAsync(options);

        var created = await CreateAsync(options, StandardTypeId, Data(("assessor_user_id", AssessorId)));

        created.InstitutionId.Should().Be(InstitutionId, "guard: the activity is stamped with the trainee's institution");
        ReadString(created.DataJson, "assessor_user_id").Should().Be(AssessorId);
        (await CountActivitiesAsync(options)).Should().Be(1);
    }

    // ---- one failing condition at a time ----------------------------------------------------------------------

    [Fact]
    public async Task ANomineeWithoutTheAssessorRole_IsRefused()
    {
        // At the trainee's institution, active, not the subject — only the role is missing.
        var options = NewDatabase();
        await SeedAsync(options);

        var message = await RefusedAtCreateAsync(options, StandardTypeId, Data(("assessor_user_id", NoRoleUserId)));

        message.Should().StartWith("Assessor: ").And.Contain(GateRefusal);
    }

    [Fact]
    public async Task AnAssessorAtAnotherInstitution_IsRefused()
    {
        // An Assessor, active, not the subject — only the institution differs from the trainee's.
        var options = NewDatabase();
        await SeedAsync(options);

        var message = await RefusedAtCreateAsync(options, StandardTypeId, Data(("assessor_user_id", OtherInstitutionAssessorId)));

        message.Should().StartWith("Assessor: ").And.Contain(GateRefusal);
    }

    [Fact]
    public async Task ADeactivatedAssessor_LockedOutUntilDateTimeOffsetMaxValue_IsRefused()
    {
        // What an administrator's lock and an erasure both write. Otherwise identical to the control.
        var options = NewDatabase();
        await SeedAsync(options);

        var message = await RefusedAtCreateAsync(options, StandardTypeId, Data(("assessor_user_id", DeactivatedAssessorId)));

        message.Should().StartWith("Assessor: ").And.Contain(GateRefusal);
    }

    [Fact]
    public async Task AnAssessorUnderABruteForceLockout_FifteenMinutesOut_IsNotRefused()
    {
        // Identity's failed-password lockout writes the same column a few minutes out. Treating it as deactivation
        // would let anyone make an assessor un-nominable by typing five wrong passwords at their account.
        var options = NewDatabase();
        await SeedAsync(options);

        var created = await CreateAsync(options, StandardTypeId, Data(("assessor_user_id", BruteForcedAssessorId)));

        ReadString(created.DataJson, "assessor_user_id").Should().Be(BruteForcedAssessorId);
    }

    [Fact]
    public async Task AnAssessorWhoseLockoutHasExpired_IsNotRefused()
    {
        var options = NewDatabase();
        await SeedAsync(options);

        var created = await CreateAsync(options, StandardTypeId, Data(("assessor_user_id", ExpiredLockoutAssessorId)));

        ReadString(created.DataJson, "assessor_user_id").Should().Be(ExpiredLockoutAssessorId);
    }

    [Fact]
    public async Task TheSubjectThemself_InAUserFieldNoFieldRuleNames_IsRefusedByTheGate_ThoughOtherwiseEligible()
    {
        // `witness_user_id` is a user field no `field:` rule names, so fix 1 (the actor-field self-nomination guard)
        // never looks at it. The trainee is given the Assessor role here, so the ONLY condition they fail is being the
        // activity's subject.
        var options = NewDatabase();
        await SeedAsync(options);
        await AddRoleAsync(options, TraineeId, WombatRoles.Assessor);

        var message = await RefusedAtCreateAsync(options, StandardTypeId, Data(("witness_user_id", TraineeId)));

        message.Should().StartWith("Witness: ").And.Contain(GateRefusal);
        message.Should().NotContain("the person the activity is about", "this is the general gate, not fix 1's narrow guard");

        // Guard: the same person, in the same field, on another trainee's activity is accepted — so the refusal above
        // was for being the subject and for nothing else.
        var other = await CreateAsync(options, StandardTypeId, Data(("witness_user_id", TraineeId)), subject: SecondTraineeId);
        ReadString(other.DataJson, "witness_user_id").Should().Be(TraineeId);
    }

    [Fact]
    public async Task AnIdMatchingNobody_IsRefused()
    {
        var options = NewDatabase();
        await SeedAsync(options);

        var message = await RefusedAtCreateAsync(options, StandardTypeId, Data(("assessor_user_id", NobodyId)));

        message.Should().StartWith("Assessor: ").And.Contain(GateRefusal);
    }

    [Theory]
    [InlineData(" assessor-1")]
    [InlineData("assessor-1 ")]
    [InlineData(" assessor-1 ")]
    [InlineData("ASSESSOR-1")]
    [InlineData("Assessor-1")]
    public async Task AWhitespacePaddedOrReCasedCopyOfAnEligibleId_IsRefused(string nearMiss)
    {
        // The actor grammar matches ids exactly and ordinally, so a near-miss names nobody who could ever act on the
        // activity. Accepting it would store a value that grants nothing to anyone the picker offered.
        var options = NewDatabase();
        await SeedAsync(options);

        var message = await RefusedAtCreateAsync(options, StandardTypeId, Data(("assessor_user_id", nearMiss)));

        message.Should().StartWith("Assessor: ").And.Contain(GateRefusal);
        message.Should().Contain("that person", "a near-miss id resolves to nobody, so nobody is named");
    }

    [Theory]
    [InlineData("42")]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("""["assessor-1"]""")]
    [InlineData("""{ "id": "assessor-1" }""")]
    public async Task ANonStringValue_InAUserFieldHiddenByShowIf_IsRefusedByTheGate(string rawJson)
    {
        // A field hidden by show_if skips the schema validator's type check entirely, so the gate is the only thing
        // between this value and the record. The actor grammar reads a non-string as empty today, but it is not a
        // person, and an eligible id wrapped in an array or object is no exception.
        var options = NewDatabase();
        await SeedAsync(options);

        var message = await RefusedAtCreateAsync(options, HiddenFieldTypeId, Data(("second_opinion_user_id", Raw(rawJson))));

        message.Should().StartWith("Second opinion: ").And.Contain(NotAPersonRefusal);
    }

    [Theory]
    [InlineData("42")]
    [InlineData("true")]
    [InlineData("""["assessor-1"]""")]
    public async Task ANonStringValue_InAVisibleUserField_IsRefused(string rawJson)
    {
        // Visible, the schema validator's type check refuses it first. Either way it never reaches the record.
        var options = NewDatabase();
        await SeedAsync(options);

        var message = await RefusedAtCreateAsync(options, StandardTypeId, Data(("assessor_user_id", Raw(rawJson))));

        message.Should().Contain("assessor_user_id");
    }

    [Fact]
    public async Task AnIneligibleId_InAUserFieldHiddenByShowIf_IsStillJudged()
    {
        // Hidden is a rendering decision, not an authorization one: the value is stored, and a `field:` rule or a later
        // show_if flip would read it. So it is judged exactly like a visible one.
        var options = NewDatabase();
        await SeedAsync(options);

        var message = await RefusedAtCreateAsync(options, HiddenFieldTypeId, Data(
            ("assessor_user_id", AssessorId),
            ("second_opinion_user_id", OtherInstitutionAssessorId)));

        message.Should().StartWith("Second opinion: ").And.Contain(GateRefusal);

        // Guard: the same hidden field naming an eligible assessor is accepted, so the refusal is about the person.
        var created = await CreateAsync(options, HiddenFieldTypeId, Data(
            ("assessor_user_id", AssessorId),
            ("second_opinion_user_id", AssessorAndCommitteeMemberId)));
        ReadString(created.DataJson, "second_opinion_user_id").Should().Be(AssessorAndCommitteeMemberId);
    }

    // ---- what is not judged -----------------------------------------------------------------------------------

    [Theory]
    [InlineData("""{ "assessor_user_id": "" }""")]
    [InlineData("""{ "assessor_user_id": null }""")]
    [InlineData("""{ "presenting_problem": "Fever for three days" }""")]
    public async Task AnEmptyNullOrAbsentNomineeValue_IsNotJudged(string dataJson)
    {
        // Nothing is being nominated. Whether the field may be empty is the schema validator's question, and a draft
        // may leave it empty. No user has the id "", so a gate that judged it would refuse.
        var options = NewDatabase();
        await SeedAsync(options);

        var created = await CreateAsync(options, StandardTypeId, dataJson);

        created.CurrentState.Should().Be("draft");
        (await CountActivitiesAsync(options)).Should().Be(1);
    }

    // ---- the institution --------------------------------------------------------------------------------------

    [Fact]
    public async Task ANullStampedInstitution_AdmitsNobody_NotEvenAnAssessorWhoseInstitutionIsAlsoNull()
    {
        // A trainee with no profile and no institution on their Identity row stamps null. Null is not "anywhere": a
        // nominee with a null institution must not match it by null-equals-null either.
        var options = NewDatabase();
        await SeedAsync(options);

        foreach (var nominee in new[] { AssessorId, UnplacedAssessorId })
        {
            var message = await RefusedAtCreateAsync(
                options, StandardTypeId, Data(("assessor_user_id", nominee)), subject: UnplacedTraineeId);

            message.Should().StartWith("Assessor: ").And.Contain(GateRefusal);
            message.Should().Contain("that person", "with no institution to be in, nobody is named");
        }

        // Guard: the same subject with nobody named can still file, so the refusals are the gate's.
        var created = await CreateAsync(options, StandardTypeId, Data(("presenting_problem", "Fever")), subject: UnplacedTraineeId);
        created.InstitutionId.Should().BeNull("guard: this subject really stamps no institution");
    }

    [Fact]
    public async Task TheInstitutionJudged_IsTheSubjectsAsStamped_NotTheCreatorsNorTheSubjectsIdentityRow()
    {
        // The transferred trainee's Identity row says the other institution; their active profile says ours, and that
        // is what the create stamps. The creator is a coordinator at the other institution. The gate must judge
        // against the stamp — the institution every `scope:` rule on this activity will read.
        var options = NewDatabase();
        await SeedAsync(options);

        var message = await RefusedAtCreateAsync(
            options,
            StandardTypeId,
            Data(("assessor_user_id", OtherInstitutionAssessorId)),
            subject: TransferredTraineeId,
            creator: OtherInstitutionCoordinatorId);

        message.Should().Contain(GateRefusal);

        var created = await CreateAsync(
            options,
            StandardTypeId,
            Data(("assessor_user_id", AssessorId)),
            subject: TransferredTraineeId,
            creator: OtherInstitutionCoordinatorId);

        created.InstitutionId.Should().Be(InstitutionId, "guard: the stamp comes from the active profile");
        ReadString(created.DataJson, "assessor_user_id").Should().Be(AssessorId);
    }

    // ---- required roles ---------------------------------------------------------------------------------------

    [Fact]
    public async Task AUserFieldWithRoleCommitteeMember_RequiresThatRole_AndNotAssessor()
    {
        // `role` replaces the default, it does not add to it. The field is also named by a `field:` rule, which on its
        // own would mean Assessor; the user declaration decides.
        var options = NewDatabase();
        await SeedAsync(options);

        var message = await RefusedAtCreateAsync(options, CommitteeTypeId, Data(("reviewer_user_id", AssessorId)));

        message.Should().StartWith("Reviewer: ").And.Contain(GateRefusal);
        message.Should().Contain("active Committee member at the trainee's institution");

        var created = await CreateAsync(options, CommitteeTypeId, Data(("reviewer_user_id", CommitteeMemberId)));
        ReadString(created.DataJson, "reviewer_user_id").Should().Be(CommitteeMemberId);
    }

    [Fact]
    public async Task AKeyDeclaredTwiceWithDifferentRoles_RequiresEveryDeclaredRole()
    {
        // Refused at publish from T102 on, but a pinned version may predate the rule, and its activities still move.
        // Built directly as a stored type (bypassing SaveDraft), which is the only way such a shape exists. Whichever
        // declaration the form happened to render, the weaker one must never decide.
        var guard = () => ActorFieldRules.EnsurePublishable(
            FormSchemaParser.Parse(DuplicateKeySchemaJson), WorkflowParser.Parse(SimpleWorkflowJson));
        guard.Should().Throw<SchemaParseException>("guard: this shape can only be a stored version, never a new publish");

        var options = NewDatabase();
        await SeedAsync(options);

        foreach (var holdsOnlyOneRole in new[] { AssessorId, CommitteeMemberId })
        {
            var message = await RefusedAtCreateAsync(options, DuplicateKeyTypeId, Data(("reviewer_user_id", holdsOnlyOneRole)));

            message.Should().Contain(GateRefusal);
            message.Should().Contain("active Assessor and Committee member at the trainee's institution");
        }

        var created = await CreateAsync(options, DuplicateKeyTypeId, Data(("reviewer_user_id", AssessorAndCommitteeMemberId)));
        ReadString(created.DataJson, "reviewer_user_id").Should().Be(AssessorAndCommitteeMemberId);
    }

    [Fact]
    public async Task ATextFieldNamedOnlyByAFieldRule_IsStillANomineeField_AndRequiresAnAssessor()
    {
        // The union: a `field:` rule makes whoever the field names an actor, whatever the field's type. Refused at
        // publish from T102 on, but a stored version can carry it, and it renders as free text — so the gate is the
        // only thing that keeps a typed id honest.
        var guard = () => ActorFieldRules.EnsurePublishable(
            FormSchemaParser.Parse(TextFieldRuleSchemaJson), WorkflowParser.Parse(TextFieldRuleWorkflowJson));
        guard.Should().Throw<SchemaParseException>("guard: this shape can only be a stored version, never a new publish");

        var options = NewDatabase();
        await SeedAsync(options);

        var elsewhere = await RefusedAtCreateAsync(options, TextFieldRuleTypeId, Data(("supervisor_id", OtherInstitutionAssessorId)));
        elsewhere.Should().StartWith("Supervisor: ").And.Contain(GateRefusal);

        var notAnAssessor = await RefusedAtCreateAsync(options, TextFieldRuleTypeId, Data(("supervisor_id", CommitteeMemberId)));
        notAnAssessor.Should().StartWith("Supervisor: ").And.Contain(GateRefusal);
        notAnAssessor.Should().Contain("active Assessor at the trainee's institution");

        var created = await CreateAsync(options, TextFieldRuleTypeId, Data(("supervisor_id", AssessorId)));
        ReadString(created.DataJson, "supervisor_id").Should().Be(AssessorId);
    }

    // ---- which field, and what the refusal says ---------------------------------------------------------------

    [Fact]
    public async Task TwoBadNominees_AreRefusedOnTheFirstInSchemaOrder_NotPayloadOrKeyOrder()
    {
        // The primary field comes first in the schema but last in both the payload and ordinal key order, so only
        // schema order refuses on it. A stable choice means the same payload always gets the same message.
        var options = NewDatabase();
        await SeedAsync(options);

        const string payload = $$"""
            {
              "a_secondary_user_id": "{{NobodyId}}",
              "z_primary_user_id": "{{OtherInstitutionAssessorId}}"
            }
            """;

        var message = await RefusedAtCreateAsync(options, SchemaOrderTypeId, payload);

        message.Should().StartWith("Primary assessor: ").And.Contain(GateRefusal);

        // Guard: the second field is judged too, when the first passes.
        var second = await RefusedAtCreateAsync(options, SchemaOrderTypeId, Data(
            ("a_secondary_user_id", NobodyId),
            ("z_primary_user_id", AssessorId)));
        second.Should().StartWith("Secondary assessor: ").And.Contain(GateRefusal);
    }

    [Fact]
    public async Task TheRefusal_NamesThePerson_OnlyWhenTheyAreInTheActivitysInstitution()
    {
        // The name is what the trainee needs to see which pick was wrong. Naming someone from another institution would
        // confirm that id belongs to a real person there.
        var options = NewDatabase();
        await SeedAsync(options);

        var sameInstitution = await RefusedAtCreateAsync(options, StandardTypeId, Data(("assessor_user_id", NoRoleUserId)));
        sameInstitution.Should().StartWith("Assessor: Nomsa Dlamini cannot be named here.");
        sameInstitution.Should().Contain("active Assessor at the trainee's institution").And.Contain("choose someone else");

        var elsewhere = await RefusedAtCreateAsync(options, StandardTypeId, Data(("assessor_user_id", OtherInstitutionAssessorId)));
        elsewhere.Should().Contain("that person");
        elsewhere.Should().NotContain("Pieter").And.NotContain("Botha").And.NotContain(OtherInstitutionAssessorId);
    }

    [Fact]
    public async Task TheRefusalForARealPersonAtAnotherInstitution_IsIdenticalToTheRefusalForAnIdMatchingNobody()
    {
        // One message for every failed condition: the refusal must not tell a caller whether an id they typed belongs
        // to someone real, elsewhere.
        var options = NewDatabase();
        await SeedAsync(options);

        var elsewhere = await RefusedAtCreateAsync(options, StandardTypeId, Data(("assessor_user_id", OtherInstitutionAssessorId)));
        var nobody = await RefusedAtCreateAsync(options, StandardTypeId, Data(("assessor_user_id", NobodyId)));

        elsewhere.Should().Be(nobody);
        nobody.Should().StartWith("Assessor: that person cannot be named here.");
    }

    [Fact]
    public async Task ADeactivatedPersonAtTheInstitution_IsNamed_ButTheMessageDoesNotSayWhyTheyWereRefused()
    {
        // The refusal does not say which condition failed; it reads the same as for someone without the role.
        var options = NewDatabase();
        await SeedAsync(options);

        var deactivated = await RefusedAtCreateAsync(options, StandardTypeId, Data(("assessor_user_id", DeactivatedAssessorId)));
        var noRole = await RefusedAtCreateAsync(options, StandardTypeId, Data(("assessor_user_id", NoRoleUserId)));

        deactivated.Should().Contain("Lindiwe Mokoena cannot be named here");
        deactivated.Replace("Lindiwe Mokoena", "X", StringComparison.Ordinal)
            .Should().Be(noRole.Replace("Nomsa Dlamini", "X", StringComparison.Ordinal));
        deactivated.Should().NotContainAny("locked", "deactivated", "lockout", "role");
    }

    // ---- no Administrator bypass ------------------------------------------------------------------------------

    [Fact]
    public async Task AnAdministratorCreating_GetsNoBypass_AndAnAdministratorNominee_GetsNoneEither()
    {
        // The answer depends on the activity, not on who is asking, and holding Administrator is not holding Assessor.
        var options = NewDatabase();
        await SeedAsync(options);
        var administrator = AdministratorPrincipal(AdministratorId);

        // An Administrator who also holds Assessor, but at another institution.
        var elsewhere = await RefusedAtCreateAsync(
            options,
            StandardTypeId,
            Data(("assessor_user_id", AdministratorAssessorElsewhereId)),
            creator: AdministratorId,
            principal: administrator);
        elsewhere.Should().StartWith("Assessor: ").And.Contain(GateRefusal);

        // The Administrator naming themself: at the trainee's institution, but without the Assessor role.
        var themself = await RefusedAtCreateAsync(
            options,
            StandardTypeId,
            Data(("assessor_user_id", AdministratorId)),
            creator: AdministratorId,
            principal: administrator);
        themself.Should().StartWith("Assessor: ").And.Contain(GateRefusal);

        // Guard: the same Administrator can file for the trainee when the nominee is eligible.
        var created = await CreateAsync(
            options, StandardTypeId, Data(("assessor_user_id", AssessorId)), creator: AdministratorId, principal: administrator);
        created.CreatedByUserId.Should().Be(AdministratorId);
        ReadString(created.DataJson, "assessor_user_id").Should().Be(AssessorId);
    }

    // ---- helpers ----------------------------------------------------------------------------------------------

    /// <summary>
    /// Runs one refused request on its own context, asserts it left nothing for the audit save to commit, performs
    /// that save, and returns the refusal message.
    /// </summary>
    private static async Task<string> ShouldBeRefusedAsync(
        DbContextOptions<ApplicationDbContext> options,
        Func<ActivityService, Task> act)
    {
        await using var db = new ApplicationDbContext(options);
        var service = Service(db);

        var attempt = async () => await act(service);
        var thrown = await attempt.Should().ThrowAsync<InvalidOperationException>();

        db.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(entry => $"{entry.Metadata.ClrType.Name}: {entry.State}")
            .Should().BeEmpty("the audit pipeline's catch saves this context, so anything dirty here would be committed");

        // What the audit pipeline's catch does next: save the same context.
        (await db.SaveChangesAsync()).Should().Be(0);

        return thrown.Which.Message;
    }

    /// <summary>A create that is refused, leaves the audit save nothing to commit, and leaves no activity behind.</summary>
    private static async Task<string> RefusedAtCreateAsync(
        DbContextOptions<ApplicationDbContext> options,
        int activityTypeId,
        string dataJson,
        string subject = TraineeId,
        string? creator = null,
        ClaimsPrincipal? principal = null)
    {
        var activitiesBefore = await CountActivitiesAsync(options);
        var transitionsBefore = await CountTransitionsAsync(options);
        var createdBy = creator ?? subject;

        var message = await ShouldBeRefusedAsync(options, service => service.CreateDraftAsync(
            new CreateActivityInput(activityTypeId, subject, createdBy, dataJson, principal ?? Principal(createdBy))));

        (await CountActivitiesAsync(options)).Should().Be(activitiesBefore, "a refused create must leave no activity behind");
        (await CountTransitionsAsync(options)).Should().Be(transitionsBefore);
        return message;
    }

    private static async Task<ActivityDto> CreateAsync(
        DbContextOptions<ApplicationDbContext> options,
        int activityTypeId,
        string dataJson,
        string subject = TraineeId,
        string? creator = null,
        ClaimsPrincipal? principal = null)
    {
        var createdBy = creator ?? subject;

        await using var db = new ApplicationDbContext(options);
        return await Service(db).CreateDraftAsync(
            new CreateActivityInput(activityTypeId, subject, createdBy, dataJson, principal ?? Principal(createdBy)));
    }

    private static async Task AddRoleAsync(DbContextOptions<ApplicationDbContext> options, string userId, string role)
    {
        await using var db = new ApplicationDbContext(options);
        NomineeSeed.AddRole(db, userId, role);
        await db.SaveChangesAsync();
    }

    private static async Task<int> CountActivitiesAsync(DbContextOptions<ApplicationDbContext> options)
    {
        await using var db = new ApplicationDbContext(options);
        return await db.Activities.CountAsync();
    }

    private static async Task<int> CountTransitionsAsync(DbContextOptions<ApplicationDbContext> options)
    {
        await using var db = new ApplicationDbContext(options);
        return await db.ActivityTransitions.CountAsync();
    }

    private static ActivityService Service(ApplicationDbContext db)
        => new(db, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator());

    /// <summary>A payload of the given fields. A <see cref="JsonElement" /> value is written as the raw JSON it holds.</summary>
    private static string Data(params (string Key, object? Value)[] fields)
        => JsonSerializer.Serialize(fields.ToDictionary(field => field.Key, field => field.Value, StringComparer.Ordinal));

    private static JsonElement Raw(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static string? ReadString(string dataJson, string key)
    {
        using var document = JsonDocument.Parse(dataJson);
        return document.RootElement.TryGetProperty(key, out var value) ? value.GetString() : null;
    }

    private static DbContextOptions<ApplicationDbContext> NewDatabase()
        => new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static ClaimsPrincipal Principal(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));

    private static ClaimsPrincipal AdministratorPrincipal(string userId)
        => new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId), new Claim(ClaimTypes.Role, WombatRoles.Administrator)],
            "test",
            ClaimTypes.Name,
            ClaimTypes.Role));

    // ---- fixture ----------------------------------------------------------------------------------------------

    /// <summary>
    /// The CPSA request form, plus a <c>witness_user_id</c> user field that no <c>field:</c> rule names. Labels differ
    /// from keys, so a message led by the key can never pass for one led by the label.
    /// </summary>
    private const string StandardSchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "assessor_user_id", "type": "user", "label": "Assessor", "required": true },
                { "key": "witness_user_id", "type": "user", "label": "Witness" },
                { "key": "presenting_problem", "type": "text", "label": "Presenting problem" }
              ]
            },
            {
              "key": "assessment",
              "title": "Entrustment",
              "editable_by": "field:assessor_user_id",
              "fields": [
                { "key": "overall_level", "type": "number", "label": "Supervision required for this encounter" }
              ]
            }
          ]
        }
        """;

    /// <summary>The CPSA seeds' shape: draft, the author's submit, the assessor's complete or decline, and cancel.</summary>
    private const string CpsaWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true },
            { "key": "declined", "label": "Declined" },
            { "key": "cancelled", "label": "Cancelled" }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id", "requires_fields": ["overall_level"] },
            { "key": "decline", "from": "requested", "to": "declined", "actor": "field:assessor_user_id", "requires_note": true },
            { "key": "cancel", "from": ["draft", "requested"], "to": "cancelled", "actor": "subject|creator" }
          ]
        }
        """;

    /// <summary>A workflow that names no field at all, so every nominee field in its schema comes from a user field.</summary>
    private const string SimpleWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "submitted", "label": "Submitted", "terminal": true }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject|creator" }
          ]
        }
        """;

    /// <summary>A committee review whose reviewer must be a committee member, and who acts through a `field:` rule.</summary>
    private const string CommitteeSchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "reviewer_user_id", "type": "user", "label": "Reviewer", "role": "CommitteeMember" },
                { "key": "summary", "type": "text", "label": "Summary" }
              ]
            },
            {
              "key": "review",
              "title": "Review",
              "editable_by": "field:reviewer_user_id",
              "fields": [
                { "key": "recommendation", "type": "longtext", "label": "Recommendation" }
              ]
            }
          ]
        }
        """;

    private const string CommitteeWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "with_reviewer", "label": "With reviewer", "editable_by": "field:reviewer_user_id" },
            { "key": "reviewed", "label": "Reviewed", "terminal": true },
            { "key": "cancelled", "label": "Cancelled" }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "with_reviewer", "actor": "subject|creator" },
            { "key": "review", "from": "with_reviewer", "to": "reviewed", "actor": "field:reviewer_user_id" },
            { "key": "cancel", "from": ["draft", "with_reviewer"], "to": "cancelled", "actor": "subject|creator" }
          ]
        }
        """;

    /// <summary>One key declared twice: once with the default role, once as a committee member.</summary>
    private const string DuplicateKeySchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "reviewer_user_id", "type": "user", "label": "Reviewer" }
              ]
            },
            {
              "key": "panel",
              "title": "Panel",
              "fields": [
                { "key": "reviewer_user_id", "type": "user", "label": "Panel reviewer", "role": "CommitteeMember" }
              ]
            }
          ]
        }
        """;

    /// <summary>A pre-T102 shape: a `field:` rule naming a free-text field.</summary>
    private const string TextFieldRuleSchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "supervisor_id", "type": "text", "label": "Supervisor" },
                { "key": "presenting_problem", "type": "text", "label": "Presenting problem" }
              ]
            },
            {
              "key": "assessment",
              "title": "Entrustment",
              "editable_by": "field:supervisor_id",
              "fields": [
                { "key": "overall_level", "type": "number", "label": "Supervision required for this encounter" }
              ]
            }
          ]
        }
        """;

    private static readonly string TextFieldRuleWorkflowJson =
        CpsaWorkflowJson.Replace("field:assessor_user_id", "field:supervisor_id", StringComparison.Ordinal);

    /// <summary>A second-opinion user field shown only when the checkbox is ticked.</summary>
    private const string HiddenFieldSchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "assessor_user_id", "type": "user", "label": "Assessor" },
                { "key": "needs_second_opinion", "type": "checkbox", "label": "A second opinion is needed" },
                {
                  "key": "second_opinion_user_id",
                  "type": "user",
                  "label": "Second opinion",
                  "show_if": { "field": "needs_second_opinion", "operator": "equals", "value": "true" }
                }
              ]
            }
          ]
        }
        """;

    /// <summary>
    /// Two user fields whose schema order is the reverse of their ordinal key order: <c>z_primary_user_id</c> is
    /// declared first.
    /// </summary>
    private const string SchemaOrderSchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "first",
              "title": "First",
              "fields": [
                { "key": "z_primary_user_id", "type": "user", "label": "Primary assessor" }
              ]
            },
            {
              "key": "second",
              "title": "Second",
              "fields": [
                { "key": "a_secondary_user_id", "type": "user", "label": "Secondary assessor" }
              ]
            }
          ]
        }
        """;

    private static async Task SeedAsync(DbContextOptions<ApplicationDbContext> options)
    {
        await using var db = new ApplicationDbContext(options);

        // The subjects.
        NomineeSeed.AddUser(db, TraineeId, InstitutionId, WombatRoles.Trainee);
        NomineeSeed.AddUser(db, SecondTraineeId, InstitutionId, WombatRoles.Trainee);
        NomineeSeed.AddUser(db, UnplacedTraineeId, null, WombatRoles.Trainee);
        NomineeSeed.AddUser(db, TransferredTraineeId, OtherInstitutionId, WombatRoles.Trainee);

        db.TraineeProfiles.AddRange(
            Profile(1, TraineeId),
            Profile(2, SecondTraineeId),
            Profile(3, TransferredTraineeId));

        // The eligible control, and one nominee per failing condition.
        NomineeSeed.AddUser(db, AssessorId, InstitutionId, WombatRoles.Assessor);
        Name(NomineeSeed.AddUser(db, NoRoleUserId, InstitutionId, WombatRoles.Coordinator), "Nomsa", "Dlamini");
        Name(NomineeSeed.AddUser(db, OtherInstitutionAssessorId, OtherInstitutionId, WombatRoles.Assessor), "Pieter", "Botha");
        Name(
            NomineeSeed.AddUser(db, DeactivatedAssessorId, InstitutionId, DateTimeOffset.MaxValue, WombatRoles.Assessor),
            "Lindiwe",
            "Mokoena");
        NomineeSeed.AddUser(db, BruteForcedAssessorId, InstitutionId, DateTimeOffset.UtcNow.AddMinutes(15), WombatRoles.Assessor);
        NomineeSeed.AddUser(db, ExpiredLockoutAssessorId, InstitutionId, DateTimeOffset.UtcNow.AddDays(-1), WombatRoles.Assessor);
        NomineeSeed.AddUser(db, UnplacedAssessorId, null, WombatRoles.Assessor);

        // For the role tests.
        NomineeSeed.AddUser(db, CommitteeMemberId, InstitutionId, WombatRoles.CommitteeMember);
        NomineeSeed.AddUser(db, AssessorAndCommitteeMemberId, InstitutionId, WombatRoles.Assessor, WombatRoles.CommitteeMember);

        // For the Administrator and creator tests.
        NomineeSeed.AddUser(db, AdministratorId, InstitutionId, WombatRoles.Administrator);
        NomineeSeed.AddUser(db, AdministratorAssessorElsewhereId, OtherInstitutionId, WombatRoles.Administrator, WombatRoles.Assessor);
        NomineeSeed.AddUser(db, OtherInstitutionCoordinatorId, OtherInstitutionId, WombatRoles.Coordinator);

        db.ActivityTypes.AddRange(
            Type(StandardTypeId, "standard_under_test", StandardSchemaJson, CpsaWorkflowJson),
            Type(CommitteeTypeId, "committee_under_test", CommitteeSchemaJson, CommitteeWorkflowJson),
            Type(DuplicateKeyTypeId, "duplicate_key_under_test", DuplicateKeySchemaJson, SimpleWorkflowJson),
            Type(TextFieldRuleTypeId, "text_field_rule_under_test", TextFieldRuleSchemaJson, TextFieldRuleWorkflowJson),
            Type(HiddenFieldTypeId, "hidden_field_under_test", HiddenFieldSchemaJson, SimpleWorkflowJson),
            Type(SchemaOrderTypeId, "schema_order_under_test", SchemaOrderSchemaJson, SimpleWorkflowJson));

        await db.SaveChangesAsync();
    }

    private static void Name(WombatIdentityUser user, string firstName, string lastName)
    {
        user.FirstName = firstName;
        user.LastName = lastName;
    }

    private static TraineeProfile Profile(int id, string userId)
        => new()
        {
            Id = id,
            UserId = userId,
            InstitutionId = InstitutionId,
            CurriculumId = 3000,
            ProgrammeStartDate = new DateOnly(2025, 4, 14),
            ExpectedCompletionDate = new DateOnly(2029, 4, 13),
            IsActive = true
        };

    /// <summary>
    /// A published type, written directly as stored rows, so a shape SaveDraft would now refuse can still be pinned.
    /// No tool key and no credit, so the T122 gate is a no-op and every refusal here is the nominee gate's.
    /// </summary>
    private static ActivityType Type(int id, string key, string schemaJson, string workflowJson)
    {
        var publishedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        const string creditRulesJson = """{ "counts_for": [] }""";
        const string displayFieldsJson = "[]";

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
            DisplayFieldsJson = displayFieldsJson,
            WbaToolKey = null,
            OwnerUserId = AdministratorId,
            CreatedOn = publishedOn
        };

        activityType.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = id,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditRulesJson,
            DisplayFieldsJson = displayFieldsJson,
            PublishedByUserId = AdministratorId,
            PublishedOn = publishedOn
        });

        return activityType;
    }
}
