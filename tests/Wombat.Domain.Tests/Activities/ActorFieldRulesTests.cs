using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Identity;
using WorkflowModel = Wombat.Domain.Activities.Workflow.Workflow;

namespace Wombat.Domain.Tests.Activities;

/// <summary>
/// T102. <see cref="ActorFieldRules" />: which fields name a person who may act on an activity, what that person must
/// be, and the invariants a type must meet before it is published with them.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ActorFieldRules.EnsurePublishable" /> is the save- and publish-time check. It is deliberately NOT in the
/// parsers, so a stored version that predates it still parses and its in-flight activities still move; the tests at the
/// bottom pin that split.
/// </para>
/// <para>
/// <see cref="ActorFieldRules.RequiredRolesByNomineeField" /> is what the nominee gate judges against and
/// <see cref="ActorFieldRules.RequiredRolesForUserField" /> what the picker lists by. They must agree, and on a
/// duplicated key they must take the conjunction: the weaker declaration can never be the one that decides.
/// </para>
/// </remarks>
public sealed class ActorFieldRulesTests
{
    private const string Nominee = "field:assessor_user_id";

    /// <summary>Where a <c>field:</c> rule can sit, and how the refusal names each place.</summary>
    public enum RuleLocation
    {
        TransitionActor,
        StateEditableBy,
        SectionEditableBy,
        FieldEditableBy
    }

    public static TheoryData<RuleLocation> LocationKinds() => new()
    {
        RuleLocation.TransitionActor,
        RuleLocation.StateEditableBy,
        RuleLocation.SectionEditableBy,
        RuleLocation.FieldEditableBy
    };

    public static TheoryData<RuleLocation, string> Locations() => new()
    {
        { RuleLocation.TransitionActor, "Transition 'complete' actor" },
        { RuleLocation.StateEditableBy, "State 'requested' editable_by" },
        { RuleLocation.SectionEditableBy, "Section 'assessment' editable_by" },
        { RuleLocation.FieldEditableBy, "Field 'overall' editable_by" }
    };

    // ---- EnsurePublishable: what passes -------------------------------------------------------

    /// <summary>
    /// The shape every seeded WBA instrument has: one user field, named by a <c>field:</c> rule at all four places a
    /// rule can sit. Without this control every refusal below could pass by refusing everything.
    /// </summary>
    [Fact]
    public void EnsurePublishable_AUserFieldNamedAtEveryLocation_Passes()
    {
        var (schema, workflow) = Build();

        ActorFieldRules.EnsurePublishable(schema, workflow);
    }

    [Fact]
    public void EnsurePublishable_ATypeWithNoUserFieldAndNoFieldRule_Passes()
    {
        ActorFieldRules.EnsurePublishable(
            FormSchemaParser.Parse(ActivityTestData.ValidSchemaJson),
            WorkflowParser.Parse(ActivityTestData.ValidWorkflowJson));
    }

    /// <summary>
    /// A user field no rule names still lands a person on the record, and is legal: it just grants nothing.
    /// </summary>
    [Fact]
    public void EnsurePublishable_AUserFieldNoRuleNames_Passes()
    {
        var (schema, workflow) = Build(
            transitionActor: "role:Assessor",
            stateEditableBy: "subject",
            sectionEditableBy: "subject",
            fieldEditableBy: "subject");

        ActorFieldRules.EnsurePublishable(schema, workflow);
    }

    /// <summary>A user field with a declared role and none of its own options is exactly what the check wants.</summary>
    [Fact]
    public void EnsurePublishable_AUserFieldWithARoleAndNoOptions_Passes()
    {
        var (schema, workflow) = Build(userFieldExtra: "\"role\": \"Coordinator\"");

        ActorFieldRules.EnsurePublishable(schema, workflow);
    }

    // ---- EnsurePublishable: duplicate keys ----------------------------------------------------

    /// <summary>
    /// DataJson, the writable set and the actor grammar all treat a key as one field, so a second declaration can
    /// only disagree with the first. The message names every declaration by label and section so the author can find
    /// both.
    /// </summary>
    [Fact]
    public void EnsurePublishable_AFieldKeyDeclaredInTwoSections_IsRefusedNamingBoth()
    {
        var (schema, workflow) = Build(assessmentExtraField: """{ "key": "title", "type": "text", "label": "Case title" }""");

        var exception = Assert.Throws<SchemaParseException>(() => ActorFieldRules.EnsurePublishable(schema, workflow));

        Assert.Contains("Field key 'title' is used by 2 fields", exception.Message, StringComparison.Ordinal);
        Assert.Contains("'Title' in 'Request'", exception.Message, StringComparison.Ordinal);
        Assert.Contains("'Case title' in 'Assessment'", exception.Message, StringComparison.Ordinal);
        Assert.Contains("give each field its own key", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EnsurePublishable_AFieldKeyDeclaredTwiceInOneSection_IsRefused()
    {
        var (schema, workflow) = Build(requestExtraField: """{ "key": "title", "type": "longtext", "label": "Title again" }""");

        var exception = Assert.Throws<SchemaParseException>(() => ActorFieldRules.EnsurePublishable(schema, workflow));

        Assert.Contains("Field key 'title' is used by 2 fields", exception.Message, StringComparison.Ordinal);
        Assert.Contains("'Title again' in 'Request'", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// The case the rule exists for: two user declarations of one key with different roles. The gate would take the
    /// conjunction, but no author means that; they mean one of them, and the check makes them say which.
    /// </summary>
    [Fact]
    public void EnsurePublishable_AUserFieldKeyDeclaredTwiceWithDifferentRoles_IsRefused()
    {
        var (schema, workflow) = Build(
            assessmentExtraField: """{ "key": "assessor_user_id", "type": "user", "label": "Second assessor", "role": "Trainee" }""");

        var exception = Assert.Throws<SchemaParseException>(() => ActorFieldRules.EnsurePublishable(schema, workflow));

        Assert.Contains("Field key 'assessor_user_id' is used by 2 fields", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EnsurePublishable_ASectionKeyDeclaredTwice_IsRefusedNamingIt()
    {
        var schema = FormSchemaParser.Parse("""
            {
              "version": 1,
              "sections": [
                { "key": "request", "title": "Request", "fields": [ { "key": "title", "type": "text", "label": "Title" } ] },
                { "key": "request", "title": "Request, again", "fields": [ { "key": "notes", "type": "text", "label": "Notes" } ] }
              ]
            }
            """);

        var exception = Assert.Throws<SchemaParseException>(
            () => ActorFieldRules.EnsurePublishable(schema, WorkflowParser.Parse(ActivityTestData.ValidWorkflowJson)));

        Assert.Contains("Section key 'request' is used by 2 sections", exception.Message, StringComparison.Ordinal);
        Assert.Contains("give each section its own key", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>Keys are compared ordinally, as DataJson and the actor grammar compare them.</summary>
    [Fact]
    public void EnsurePublishable_KeysDifferingOnlyInCase_AreDistinct()
    {
        var (schema, workflow) = Build(assessmentExtraField: """{ "key": "Title", "type": "text", "label": "Other" }""");

        ActorFieldRules.EnsurePublishable(schema, workflow);
    }

    // ---- EnsurePublishable: field: rules ------------------------------------------------------

    /// <summary>
    /// A <c>field:</c> rule naming no field grants whatever id someone manages to put under that key. Each of the four
    /// places a rule can sit is checked, and the refusal names the place.
    /// </summary>
    [Theory]
    [MemberData(nameof(Locations))]
    public void EnsurePublishable_AFieldRuleNamingNoField_IsRefusedNamingTheLocation(RuleLocation location, string expectedLocation)
    {
        var (schema, workflow) = BuildWithRuleAt(location, "field:ghost");

        var exception = Assert.Throws<SchemaParseException>(() => ActorFieldRules.EnsurePublishable(schema, workflow));

        Assert.Contains($"{expectedLocation} names 'field:ghost'", exception.Message, StringComparison.Ordinal);
        Assert.Contains("the form has no field 'ghost'", exception.Message, StringComparison.Ordinal);
        Assert.Contains("add a User field with that key on the Form tab", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// A <c>field:</c> rule naming a text field is the T102 escalation itself: the field renders as free text, so the
    /// actor is whatever id the trainee types.
    /// </summary>
    [Theory]
    [MemberData(nameof(Locations))]
    public void EnsurePublishable_AFieldRuleNamingANonUserField_IsRefusedNamingTheLocation(RuleLocation location, string expectedLocation)
    {
        var (schema, workflow) = BuildWithRuleAt(location, "field:title");

        var exception = Assert.Throws<SchemaParseException>(() => ActorFieldRules.EnsurePublishable(schema, workflow));

        Assert.Contains($"{expectedLocation} names 'field:title'", exception.Message, StringComparison.Ordinal);
        Assert.Contains("'title' is not a User field", exception.Message, StringComparison.Ordinal);
        Assert.Contains("a field: rule must name a User field", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>A <c>field:</c> rule inside a combination is still a <c>field:</c> rule: the walk must recurse.</summary>
    [Theory]
    [InlineData("subject|field:ghost")]
    [InlineData("field:ghost+role:Assessor")]
    [InlineData("role:Coordinator|field:ghost+role:Assessor")]
    public void EnsurePublishable_AFieldRuleInsideACombination_IsStillChecked(string actor)
    {
        var (schema, workflow) = Build(transitionActor: actor);

        var exception = Assert.Throws<SchemaParseException>(() => ActorFieldRules.EnsurePublishable(schema, workflow));

        Assert.Contains("Transition 'complete' actor names 'field:ghost'", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// A key declared once as a user field and once as something else is refused as a duplicate AND as a rule naming
    /// a non-user field: one user declaration among several does not make the key safe to name.
    /// </summary>
    [Fact]
    public void EnsurePublishable_AFieldRuleNamingAKeyThatIsAlsoANonUserField_IsRefused()
    {
        var (schema, workflow) = Build(
            assessmentExtraField: """{ "key": "assessor_user_id", "type": "text", "label": "Assessor (typed)" }""");

        var exception = Assert.Throws<SchemaParseException>(() => ActorFieldRules.EnsurePublishable(schema, workflow));

        Assert.Contains("Field key 'assessor_user_id' is used by 2 fields", exception.Message, StringComparison.Ordinal);
        Assert.Contains("names 'field:assessor_user_id', but 'assessor_user_id' is not a User field", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// One refusal names every offending location, so the builder shows the author everything to fix at once rather
    /// than one problem per save.
    /// </summary>
    [Fact]
    public void EnsurePublishable_NamesEveryOffendingLocationInOneMessage()
    {
        var (schema, workflow) = Build(
            transitionActor: "field:ghost",
            stateEditableBy: "field:ghost",
            sectionEditableBy: "field:title",
            fieldEditableBy: "field:title");

        var exception = Assert.Throws<SchemaParseException>(() => ActorFieldRules.EnsurePublishable(schema, workflow));

        Assert.Contains("Transition 'complete' actor names 'field:ghost'", exception.Message, StringComparison.Ordinal);
        Assert.Contains("State 'requested' editable_by names 'field:ghost'", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Section 'assessment' editable_by names 'field:title'", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Field 'overall' editable_by names 'field:title'", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// Two transitions naming the same missing field are two locations to repair, and each is named.
    /// </summary>
    [Fact]
    public void EnsurePublishable_TheSameMissingFieldInTwoTransitions_NamesBoth()
    {
        var schema = FormSchemaParser.Parse(ActivityTestData.ValidSchemaJson);
        var workflow = WorkflowParser.Parse("""
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
                { "key": "accept", "from": "submitted", "to": "completed", "actor": "field:assessor_user_id" },
                { "key": "decline", "from": "submitted", "to": "draft", "actor": "field:assessor_user_id" }
              ]
            }
            """);

        var exception = Assert.Throws<SchemaParseException>(() => ActorFieldRules.EnsurePublishable(schema, workflow));

        Assert.Contains("Transition 'accept' actor names 'field:assessor_user_id'", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Transition 'decline' actor names 'field:assessor_user_id'", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// The scenario-paediatrics shortcut T102 now refuses: a title-only schema beside the Mini-CEX workflow. Every
    /// rule names a field the form does not have.
    /// </summary>
    [Fact]
    public void EnsurePublishable_ATitleOnlySchemaBesideAFieldRuleWorkflow_IsRefused()
    {
        var schema = FormSchemaParser.Parse("""
            { "version": 1, "sections": [ { "key": "main", "title": "Main", "fields": [ { "key": "title", "type": "text", "label": "Title" } ] } ] }
            """);
        var (_, workflow) = Build();

        var exception = Assert.Throws<SchemaParseException>(() => ActorFieldRules.EnsurePublishable(schema, workflow));

        Assert.Contains("the form has no field 'assessor_user_id'", exception.Message, StringComparison.Ordinal);
    }

    // ---- EnsurePublishable: a user field's own options ----------------------------------------

    /// <summary>
    /// The people a user field offers come from the directory, which is also what the server checks. Inline options
    /// would be a second list the picker could drift to.
    /// </summary>
    [Fact]
    public void EnsurePublishable_InlineOptionsOnAUserField_AreRefusedNamingTheField()
    {
        var (schema, workflow) = Build(userFieldExtra: "\"options\": [\"user-1\", \"user-2\"]");

        var exception = Assert.Throws<SchemaParseException>(() => ActorFieldRules.EnsurePublishable(schema, workflow));

        Assert.Contains("User field 'assessor_user_id' declares its own options", exception.Message, StringComparison.Ordinal);
        Assert.Contains("remove them", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EnsurePublishable_ACatalogueOnAUserField_IsRefusedNamingTheField()
    {
        var (schema, workflow) = Build(userFieldExtra: "\"catalogue\": \"assessors\"");

        var exception = Assert.Throws<SchemaParseException>(() => ActorFieldRules.EnsurePublishable(schema, workflow));

        Assert.Contains("User field 'assessor_user_id' declares its own options", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>Options on a choice field are the normal case and must not be caught by the user-field rule.</summary>
    [Fact]
    public void EnsurePublishable_OptionsOnAChoiceField_Pass()
    {
        var (schema, workflow) = Build(
            requestExtraField: """{ "key": "setting", "type": "choice", "label": "Setting", "options": ["Ward", "Clinic"] }""");

        ActorFieldRules.EnsurePublishable(schema, workflow);
    }

    // ---- The check is not in the parsers ------------------------------------------------------

    /// <summary>
    /// A version stored before T102 may break every rule above. It must still parse, or its in-flight activities
    /// could not be read, moved or credited. The check runs at save and publish, and nowhere else.
    /// </summary>
    [Fact]
    public void AVersionThatFailsTheCheck_StillParses()
    {
        var schemaJson = """
            {
              "version": 1,
              "sections": [
                { "key": "request", "title": "Request", "fields": [
                    { "key": "title", "type": "text", "label": "Title" },
                    { "key": "title", "type": "text", "label": "Title again" },
                    { "key": "assessor_user_id", "type": "user", "label": "Assessor", "options": ["someone"] }
                ] },
                { "key": "request", "title": "Duplicate", "editable_by": "field:ghost", "fields": [
                    { "key": "notes", "type": "text", "label": "Notes", "editable_by": "field:title" }
                ] }
              ]
            }
            """;

        var schema = FormSchemaParser.Parse(schemaJson);
        var (_, workflow) = BuildWithRuleAt(RuleLocation.TransitionActor, "field:ghost");

        Assert.Equal(2, schema.Sections.Count);
        Assert.Throws<SchemaParseException>(() => ActorFieldRules.EnsurePublishable(schema, workflow));
    }

    [Fact]
    public void EnsurePublishable_NullArguments_Throw()
    {
        var (schema, workflow) = Build();

        Assert.Throws<ArgumentNullException>(() => ActorFieldRules.EnsurePublishable(null!, workflow));
        Assert.Throws<ArgumentNullException>(() => ActorFieldRules.EnsurePublishable(schema, null!));
    }

    // ---- ActorFieldNames ----------------------------------------------------------------------

    /// <summary>
    /// The one walker every site uses: it must reach all four locations and recurse into combinations. Every other
    /// location names no field, so the rule under test is the only way the name can be found.
    /// </summary>
    [Theory]
    [MemberData(nameof(LocationKinds))]
    public void ActorFieldNames_FindsAFieldRuleAtEachLocation(RuleLocation location)
    {
        const string rule = "subject|field:second_user+role:Assessor";
        var (schema, workflow) = location switch
        {
            RuleLocation.TransitionActor => Build(rule, "subject", "subject", "subject"),
            RuleLocation.StateEditableBy => Build("subject", rule, "subject", "subject"),
            RuleLocation.SectionEditableBy => Build("subject", "subject", rule, "subject"),
            RuleLocation.FieldEditableBy => Build("subject", "subject", "subject", rule),
            _ => throw new ArgumentOutOfRangeException(nameof(location), location, null)
        };

        var names = ActorFieldRules.ActorFieldNames(schema, workflow);

        Assert.Equal(new[] { "second_user" }, names.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void ActorFieldNames_ATypeWhoseRulesNameNoField_IsEmpty()
    {
        var (schema, workflow) = Build("role:Assessor", "subject", "creator", "subject+role:Assessor");

        Assert.Empty(ActorFieldRules.ActorFieldNames(schema, workflow));
    }

    // ---- RequiredRolesForUserField ------------------------------------------------------------

    [Fact]
    public void RequiredRolesForUserField_AUserFieldWithoutARole_RequiresAnAssessor()
    {
        var (schema, _) = Build();

        Assert.Equal(new[] { WombatRoles.Assessor }, ActorFieldRules.RequiredRolesForUserField(schema, "assessor_user_id"));
    }

    [Fact]
    public void RequiredRolesForUserField_AUserFieldWithARole_RequiresThatRoleAlone()
    {
        var (schema, _) = Build(userFieldExtra: "\"role\": \"committeemember\"");

        Assert.Equal(new[] { WombatRoles.CommitteeMember }, ActorFieldRules.RequiredRolesForUserField(schema, "assessor_user_id"));
    }

    /// <summary>
    /// A pinned version with a duplicated user key (refused at publish from T102 on, but storable before it): the
    /// nominee must hold EVERY declared role, so the weaker declaration can never be the one that decides.
    /// </summary>
    [Fact]
    public void RequiredRolesForUserField_ADuplicatedKey_RequiresEveryDeclaredRole()
    {
        var (schema, _) = Build(
            userFieldExtra: "\"role\": \"Trainee\"",
            assessmentExtraField: """{ "key": "assessor_user_id", "type": "user", "label": "Assessor", "role": "Coordinator" }""");

        Assert.Equal(
            new[] { WombatRoles.Coordinator, WombatRoles.Trainee },
            ActorFieldRules.RequiredRolesForUserField(schema, "assessor_user_id"));
    }

    /// <summary>A declaration that says nothing counts as Assessor, so it joins the conjunction rather than vanishing.</summary>
    [Fact]
    public void RequiredRolesForUserField_ADuplicateThatDeclaresNoRole_AddsAssessor()
    {
        var (schema, _) = Build(
            assessmentExtraField: """{ "key": "assessor_user_id", "type": "user", "label": "Assessor", "role": "Coordinator" }""");

        Assert.Equal(
            new[] { WombatRoles.Assessor, WombatRoles.Coordinator },
            ActorFieldRules.RequiredRolesForUserField(schema, "assessor_user_id"));
    }

    [Fact]
    public void RequiredRolesForUserField_ADuplicateDeclaringTheSameRole_ListsItOnce()
    {
        var (schema, _) = Build(
            userFieldExtra: "\"role\": \"Coordinator\"",
            assessmentExtraField: """{ "key": "assessor_user_id", "type": "user", "label": "Assessor", "role": "coordinator" }""");

        Assert.Equal(new[] { WombatRoles.Coordinator }, ActorFieldRules.RequiredRolesForUserField(schema, "assessor_user_id"));
    }

    /// <summary>
    /// Only user declarations carry a role. A same-key text field (a pinned pre-T102 duplicate) neither adds a role
    /// nor removes the user field's.
    /// </summary>
    [Fact]
    public void RequiredRolesForUserField_ASameKeyNonUserField_ContributesNothing()
    {
        var (schema, _) = Build(
            userFieldExtra: "\"role\": \"Coordinator\"",
            assessmentExtraField: """{ "key": "assessor_user_id", "type": "text", "label": "Assessor (typed)" }""");

        Assert.Equal(new[] { WombatRoles.Coordinator }, ActorFieldRules.RequiredRolesForUserField(schema, "assessor_user_id"));
    }

    [Theory]
    [InlineData("title")]
    [InlineData("ghost")]
    [InlineData("Assessor_User_Id")]
    public void RequiredRolesForUserField_AKeyWithNoUserDeclaration_RequiresAnAssessor(string key)
    {
        var (schema, _) = Build(userFieldExtra: "\"role\": \"Coordinator\"");

        Assert.Equal(new[] { WombatRoles.Assessor }, ActorFieldRules.RequiredRolesForUserField(schema, key));
    }

    // ---- RequiredRolesByNomineeField ----------------------------------------------------------

    [Fact]
    public void RequiredRolesByNomineeField_AUserFieldNamedByRules_RequiresItsDeclaredRole()
    {
        var (schema, workflow) = Build(userFieldExtra: "\"role\": \"Coordinator\"");

        var roles = ActorFieldRules.RequiredRolesByNomineeField(schema, workflow);

        Assert.Equal(new[] { "assessor_user_id" }, roles.Keys);
        Assert.Equal(new[] { WombatRoles.Coordinator }, roles["assessor_user_id"]);
    }

    /// <summary>
    /// The union, not either half: a user field no rule names still lands a person on the record, so it is a nominee
    /// field and is judged.
    /// </summary>
    [Fact]
    public void RequiredRolesByNomineeField_IncludesAUserFieldNoRuleNames()
    {
        var (schema, workflow) = Build(
            transitionActor: "role:Assessor",
            stateEditableBy: "subject",
            sectionEditableBy: "subject",
            fieldEditableBy: "subject");

        var roles = ActorFieldRules.RequiredRolesByNomineeField(schema, workflow);

        Assert.Equal(new[] { WombatRoles.Assessor }, roles["assessor_user_id"]);
    }

    /// <summary>
    /// A key only a <c>field:</c> rule names — a missing field, or a text field, in a pinned pre-T102 version — still
    /// grants rights, so it is still judged, and requires an Assessor: the role every such rule in the corpus means.
    /// </summary>
    [Theory]
    [InlineData("field:ghost", "ghost")]
    [InlineData("field:title", "title")]
    public void RequiredRolesByNomineeField_AKeyOnlyAFieldRuleNames_RequiresAnAssessor(string rule, string key)
    {
        var (schema, workflow) = Build(transitionActor: rule, userFieldExtra: "\"role\": \"Coordinator\"");

        var roles = ActorFieldRules.RequiredRolesByNomineeField(schema, workflow);

        Assert.Equal(new[] { WombatRoles.Assessor }, roles[key]);
        Assert.Equal(new[] { WombatRoles.Coordinator }, roles["assessor_user_id"]);
    }

    /// <summary>
    /// A field: rule naming a user field does not overwrite the field's declared role with the rule-only default.
    /// </summary>
    [Fact]
    public void RequiredRolesByNomineeField_ARuleNamingAUserField_KeepsTheFieldsDeclaredRole()
    {
        var (schema, workflow) = Build(userFieldExtra: "\"role\": \"Trainee\"");

        var roles = ActorFieldRules.RequiredRolesByNomineeField(schema, workflow);

        Assert.Equal(new[] { WombatRoles.Trainee }, roles["assessor_user_id"]);
    }

    [Fact]
    public void RequiredRolesByNomineeField_ADuplicatedUserKey_RequiresTheConjunction()
    {
        var (schema, workflow) = Build(
            assessmentExtraField: """{ "key": "assessor_user_id", "type": "user", "label": "Assessor", "role": "Coordinator" }""");

        var roles = ActorFieldRules.RequiredRolesByNomineeField(schema, workflow);

        Assert.Equal(new[] { WombatRoles.Assessor, WombatRoles.Coordinator }, roles["assessor_user_id"]);
    }

    /// <summary>The picker and the gate read one answer for every user field: the by-field map is built from the per-field one.</summary>
    [Fact]
    public void RequiredRolesByNomineeField_AgreesWithRequiredRolesForUserFieldOnEveryUserField()
    {
        var (schema, workflow) = Build(
            userFieldExtra: "\"role\": \"Trainee\"",
            requestExtraField: """{ "key": "second_user", "type": "user", "label": "Second", "role": "CommitteeMember" }""",
            assessmentExtraField: """{ "key": "assessor_user_id", "type": "user", "label": "Assessor" }""");

        var roles = ActorFieldRules.RequiredRolesByNomineeField(schema, workflow);

        foreach (var key in new[] { "assessor_user_id", "second_user" })
        {
            Assert.Equal(ActorFieldRules.RequiredRolesForUserField(schema, key), roles[key]);
        }
    }

    [Fact]
    public void RequiredRolesByNomineeField_OmitsFieldsThatNameNobody()
    {
        var (schema, workflow) = Build();

        var roles = ActorFieldRules.RequiredRolesByNomineeField(schema, workflow);

        Assert.DoesNotContain("title", roles.Keys);
        Assert.DoesNotContain("overall", roles.Keys);
    }

    [Fact]
    public void RequiredRolesByNomineeField_ATypeWithNoNomineeField_IsEmpty()
    {
        var roles = ActorFieldRules.RequiredRolesByNomineeField(
            FormSchemaParser.Parse(ActivityTestData.ValidSchemaJson),
            WorkflowParser.Parse(ActivityTestData.ValidWorkflowJson));

        Assert.Empty(roles);
    }

    // ---- fixtures -----------------------------------------------------------------------------

    /// <summary>
    /// A request/assessment instrument in the shape of the seeded WBAs. <c>title</c> is a text field and
    /// <c>assessor_user_id</c> the user field; each actor rule defaults to naming the user field.
    /// </summary>
    private static (FormSchema Schema, WorkflowModel Workflow) Build(
        string transitionActor = Nominee,
        string stateEditableBy = Nominee,
        string sectionEditableBy = Nominee,
        string fieldEditableBy = Nominee,
        string? userFieldExtra = null,
        string? requestExtraField = null,
        string? assessmentExtraField = null)
    {
        var userExtra = userFieldExtra is null ? string.Empty : $", {userFieldExtra}";
        var requestExtra = requestExtraField is null ? string.Empty : $",\n{requestExtraField}";
        var assessmentExtra = assessmentExtraField is null ? string.Empty : $",\n{assessmentExtraField}";

        var schemaJson = $$"""
            {
              "version": 1,
              "sections": [
                {
                  "key": "request",
                  "title": "Request",
                  "fields": [
                    { "key": "title", "type": "text", "label": "Title", "required": true },
                    { "key": "assessor_user_id", "type": "user", "label": "Assessor", "required": true{{userExtra}} }{{requestExtra}}
                  ]
                },
                {
                  "key": "assessment",
                  "title": "Assessment",
                  "editable_by": "{{sectionEditableBy}}",
                  "fields": [
                    { "key": "overall", "type": "number", "label": "Overall", "editable_by": "{{fieldEditableBy}}" }{{assessmentExtra}}
                  ]
                }
              ]
            }
            """;

        var workflowJson = $$"""
            {
              "version": 1,
              "initial_state": "draft",
              "states": [
                { "key": "draft", "label": "Draft" },
                { "key": "requested", "label": "Requested", "editable_by": "{{stateEditableBy}}" },
                { "key": "completed", "label": "Completed", "terminal": true }
              ],
              "transitions": [
                { "key": "request", "from": "draft", "to": "requested", "actor": "subject" },
                { "key": "complete", "from": "requested", "to": "completed", "actor": "{{transitionActor}}" }
              ]
            }
            """;

        return (FormSchemaParser.Parse(schemaJson), WorkflowParser.Parse(workflowJson));
    }

    /// <summary><see cref="Build" /> with <paramref name="rule" /> at one location and the valid rule everywhere else.</summary>
    private static (FormSchema Schema, WorkflowModel Workflow) BuildWithRuleAt(RuleLocation location, string rule)
        => location switch
        {
            RuleLocation.TransitionActor => Build(transitionActor: rule),
            RuleLocation.StateEditableBy => Build(stateEditableBy: rule),
            RuleLocation.SectionEditableBy => Build(sectionEditableBy: rule),
            RuleLocation.FieldEditableBy => Build(fieldEditableBy: rule),
            _ => throw new ArgumentOutOfRangeException(nameof(location), location, null)
        };
}
