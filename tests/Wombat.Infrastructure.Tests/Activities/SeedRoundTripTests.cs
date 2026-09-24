using System.Text.Json;
using FluentAssertions;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Infrastructure.Tests.Activities;

/// <summary>
/// The round-trip guard for the seed corpus (T103).
/// </summary>
/// <remarks>
/// <para>
/// <c>ActivityType.SaveDraft</c> stores <c>Serialize(Parse(json))</c>, never the bytes it was
/// handed. A DSL property that has a <c>Parse</c> half but no <c>Serialize</c> half is therefore
/// dropped at publish, silently — the seed file still reads correctly on disk and the published
/// version quietly loses it. Worse, once <see cref="Wombat.Infrastructure.Persistence.ActivityTypeSeedRefresher"/>
/// compares canonical-to-canonical, the loss becomes invisible forever: both sides drop the same
/// property, so the refresher reports "in sync".
/// </para>
/// <para>
/// Two properties are asserted here, per DSL, per seed folder:
/// <list type="number">
/// <item><b>Fixed point</b> — <c>canon(canon(x)) == canon(x)</c>, byte for byte. This is what the
/// refresher's idempotency rests on.</item>
/// <item><b>No loss</b> — every property present in the raw file survives into the canonical form
/// with the same value. Canonical may add properties (it materialises the omitted
/// <c>"required": false</c> default); it may never drop one.</item>
/// </list>
/// </para>
/// </remarks>
public sealed class SeedRoundTripTests
{
    public static TheoryData<string> SeedDirectories
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var directory in EnumerateSeedDirectories())
            {
                data.Add(Path.GetFileName(directory));
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void Schema_CanonicalisationIsAFixedPoint(string seedKey)
    {
        var raw = ReadSeedFile(seedKey, "schema.json");
        var once = FormSchemaParser.Serialize(FormSchemaParser.Parse(raw));
        var twice = FormSchemaParser.Serialize(FormSchemaParser.Parse(once));

        twice.Should().Be(once, "schema.json for '{0}' must canonicalise to a fixed point", seedKey);
    }

    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void Workflow_CanonicalisationIsAFixedPoint(string seedKey)
    {
        var raw = ReadSeedFile(seedKey, "workflow.json");
        var once = WorkflowParser.Serialize(WorkflowParser.Parse(raw));
        var twice = WorkflowParser.Serialize(WorkflowParser.Parse(once));

        twice.Should().Be(once, "workflow.json for '{0}' must canonicalise to a fixed point", seedKey);
    }

    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void Credit_CanonicalisationIsAFixedPoint(string seedKey)
    {
        var raw = ReadSeedFile(seedKey, "credit.json");
        var once = CreditRulesParser.Serialize(CreditRulesParser.Parse(raw));
        var twice = CreditRulesParser.Serialize(CreditRulesParser.Parse(once));

        twice.Should().Be(once, "credit.json for '{0}' must canonicalise to a fixed point", seedKey);
    }

    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void Schema_RoundTripDropsNothingOnDisk(string seedKey)
    {
        var raw = ReadSeedFile(seedKey, "schema.json");
        var canonical = FormSchemaParser.Serialize(FormSchemaParser.Parse(raw));

        AssertNothingLost(raw, canonical, $"{seedKey}/schema.json");
    }

    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void Workflow_RoundTripDropsNothingOnDisk(string seedKey)
    {
        var raw = ReadSeedFile(seedKey, "workflow.json");
        var canonical = WorkflowParser.Serialize(WorkflowParser.Parse(raw));

        AssertNothingLost(raw, canonical, $"{seedKey}/workflow.json");
    }

    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void Credit_RoundTripDropsNothingOnDisk(string seedKey)
    {
        var raw = ReadSeedFile(seedKey, "credit.json");
        var canonical = CreditRulesParser.Serialize(CreditRulesParser.Parse(raw));

        AssertNothingLost(raw, canonical, $"{seedKey}/credit.json");
    }

    /// <summary>
    /// T070 added <c>editable_by</c> at three levels. Naming them explicitly means a future
    /// Serialize that forgets one fails here by name, not by an opaque corpus-wide diff.
    /// </summary>
    [Fact]
    public void EditableBy_SurvivesTheRoundTripAtSectionFieldAndStateLevel()
    {
        const string schemaJson = """
            {
              "version": 1,
              "sections": [
                {
                  "key": "assessor",
                  "title": "Assessor",
                  "editable_by": "field:assessor_user_id",
                  "fields": [
                    { "key": "overall", "type": "number", "label": "Overall", "editable_by": "field:assessor_user_id" }
                  ]
                }
              ]
            }
            """;

        const string workflowJson = """
            {
              "version": 1,
              "initial_state": "draft",
              "states": [
                { "key": "draft", "label": "Draft" },
                { "key": "accepted", "label": "Accepted", "editable_by": "field:assessor_user_id" }
              ],
              "transitions": [
                { "key": "request", "from": "draft", "to": "accepted", "actor": "subject" }
              ]
            }
            """;

        var canonicalSchema = FormSchemaParser.Serialize(FormSchemaParser.Parse(schemaJson));
        var canonicalWorkflow = WorkflowParser.Serialize(WorkflowParser.Parse(workflowJson));

        canonicalSchema.Should().Contain("\"editable_by\":\"field:assessor_user_id\"");
        AssertNothingLost(schemaJson, canonicalSchema, "editable_by schema fixture");

        canonicalWorkflow.Should().Contain("\"editable_by\":\"field:assessor_user_id\"");
        AssertNothingLost(workflowJson, canonicalWorkflow, "editable_by workflow fixture");
    }

    /// <summary>
    /// T105 added a transition's <c>validation</c>. Named explicitly: a Serialize that forgot it would turn every
    /// <c>draft</c> and <c>owned</c> transition back into <c>all</c> at publish, and a half-filled draft could not be
    /// cancelled again, with nothing failing but the cancel.
    /// </summary>
    [Fact]
    public void TransitionValidation_SurvivesParseSerializeParse()
    {
        const string workflowJson = """
            {
              "version": 1,
              "initial_state": "draft",
              "states": [
                { "key": "draft", "label": "Draft" },
                { "key": "requested", "label": "Requested" },
                { "key": "completed", "label": "Completed", "terminal": true },
                { "key": "cancelled", "label": "Cancelled" }
              ],
              "transitions": [
                { "key": "submit", "from": "draft", "to": "requested", "actor": "subject", "validation": "owned" },
                { "key": "complete", "from": "requested", "to": "completed", "actor": "role:Assessor", "validation": "all" },
                { "key": "cancel", "from": ["draft", "requested"], "to": "cancelled", "actor": "subject", "validation": "draft" }
              ]
            }
            """;

        var canonical = WorkflowParser.Serialize(WorkflowParser.Parse(workflowJson));

        AssertNothingLost(workflowJson, canonical, "validation workflow fixture");
        WorkflowParser.Parse(canonical).Transitions.Select(transition => transition.Validation)
            .Should().Equal(TransitionValidation.Owned, TransitionValidation.All, TransitionValidation.Draft);
    }

    /// <summary>
    /// Every seed's transitions keep exactly the validation their file declares. A seed that says nothing gets
    /// <c>all</c>, the strict default, and its canonical form says so.
    /// </summary>
    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void EverySeedsTransitionValidation_SurvivesTheRoundTrip(string seedKey)
    {
        var raw = WorkflowParser.Parse(ReadSeedFile(seedKey, "workflow.json"));
        var canonical = WorkflowParser.Parse(WorkflowParser.Serialize(raw));

        canonical.Transitions.Select(transition => (transition.Key, transition.Validation))
            .Should().Equal(raw.Transitions.Select(transition => (transition.Key, transition.Validation)));
    }

    /// <summary>
    /// T119 added a root <c>observation_date_field</c> pointer at the field that records when the
    /// encounter happened. Named explicitly for the same reason <c>editable_by</c> is: a Serialize
    /// that forgets it fails here by name rather than as an opaque corpus-wide diff.
    /// </summary>
    /// <remarks>
    /// This particular loss is invisible in the product rather than noisy. An activity whose pinned
    /// schema carries no pointer falls back to the audit clock, so it silently dates itself by when
    /// the paperwork was filed instead of when the encounter happened — which is precisely the
    /// defect T119 exists to remove, quietly re-created by a missing Serialize half.
    /// </remarks>
    [Fact]
    public void ObservationDateField_SurvivesParseSerializeParse()
    {
        const string schemaJson = """
            {
              "version": 1,
              "observation_date_field": "observed_on",
              "sections": [
                {
                  "key": "request",
                  "title": "Request",
                  "fields": [
                    { "key": "observed_on", "type": "date", "label": "Date observed", "required": true }
                  ]
                }
              ]
            }
            """;

        var canonical = FormSchemaParser.Serialize(FormSchemaParser.Parse(schemaJson));

        canonical.Should().Contain("\"observation_date_field\":\"observed_on\"");
        FormSchemaParser.Parse(canonical).ObservationDateField.Should().Be("observed_on");
        AssertNothingLost(schemaJson, canonical, "observation_date_field fixture");
    }

    /// <summary>
    /// The mirror. <c>reflective_note</c> and <c>qi_project</c> deliberately declare no pointer — a
    /// reflection and a months-long QI project have no single encounter date — so canonicalisation
    /// must not invent one and make them assert a date they do not hold.
    /// </summary>
    [Fact]
    public void ObservationDateField_StaysAbsentWhenTheSchemaDeclaresNone()
    {
        const string schemaJson = """
            {
              "version": 1,
              "sections": [
                {
                  "key": "reflection",
                  "title": "Reflection",
                  "fields": [
                    { "key": "what_i_learned", "type": "longtext", "label": "What I learned" }
                  ]
                }
              ]
            }
            """;

        var canonical = FormSchemaParser.Serialize(FormSchemaParser.Parse(schemaJson));

        canonical.Should().NotContain("observation_date_field");
        FormSchemaParser.Parse(canonical).ObservationDateField.Should().BeNull();
    }

    /// <summary>
    /// T102 added <c>role</c> on a <c>user</c> field: the role its nominee must hold. Named explicitly for the
    /// same reason <c>editable_by</c> is: no seed declares one yet, so the corpus-wide theories above cannot
    /// see a Serialize that forgets it.
    /// </summary>
    /// <remarks>
    /// This loss would be a silent widening, not a narrowing. A published version that dropped the role reads
    /// as "absent", which means Assessor, so a field an author restricted to a Coordinator would quietly accept
    /// any Assessor at the institution — and the picker and the gate would both agree that was right.
    /// </remarks>
    [Fact]
    public void NomineeRole_SurvivesParseSerializeParse()
    {
        const string schemaJson = """
            {
              "version": 1,
              "sections": [
                {
                  "key": "request",
                  "title": "Request",
                  "fields": [
                    { "key": "coordinator_user_id", "type": "user", "label": "Coordinator", "required": true, "role": "Coordinator" }
                  ]
                }
              ]
            }
            """;

        var canonical = FormSchemaParser.Serialize(FormSchemaParser.Parse(schemaJson));

        canonical.Should().Contain("\"role\":\"Coordinator\"");
        FormSchemaParser.Parse(canonical).Sections[0].Fields[0].NomineeRole.Should().Be("Coordinator");
        FormSchemaParser.Serialize(FormSchemaParser.Parse(canonical)).Should().Be(canonical, "a role must canonicalise to a fixed point");
        AssertNothingLost(schemaJson, canonical, "role fixture");
    }

    /// <summary>
    /// The mirror, and the one that protects the corpus. Eight seeds carry a <c>user</c> field and none says
    /// <c>role</c>; absent means Assessor. If canonicalisation materialised that default, every one of them would
    /// canonicalise differently from its stored version and the refresher would republish all eight at the next
    /// boot. The no-loss theory cannot see that, because canonical is allowed to ADD properties.
    /// </summary>
    [Fact]
    public void NomineeRole_StaysAbsentWhenTheFieldDeclaresNone()
    {
        const string schemaJson = """
            {
              "version": 1,
              "sections": [
                {
                  "key": "request",
                  "title": "Request",
                  "fields": [
                    { "key": "assessor_user_id", "type": "user", "label": "Assessor", "required": true }
                  ]
                }
              ]
            }
            """;

        var canonical = FormSchemaParser.Serialize(FormSchemaParser.Parse(schemaJson));

        canonical.Should().NotContain("\"role\"");
        FormSchemaParser.Parse(canonical).Sections[0].Fields[0].NomineeRole.Should().BeNull();
    }

    /// <summary>
    /// The same guard, corpus-wide: canonicalising a seed adds a <c>role</c> to no field that did not declare one.
    /// </summary>
    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void Schema_CanonicalFormInventsNoNomineeRole(string seedKey)
    {
        var raw = FormSchemaParser.Parse(ReadSeedFile(seedKey, "schema.json"));
        var canonical = FormSchemaParser.Parse(FormSchemaParser.Serialize(raw));

        var rawRoles = raw.Sections.SelectMany(section => section.Fields).Select(field => (field.Key, field.NomineeRole));
        var canonicalRoles = canonical.Sections.SelectMany(section => section.Fields).Select(field => (field.Key, field.NomineeRole));

        canonicalRoles.Should().Equal(rawRoles, "'{0}' must carry exactly the roles its seed file declares", seedKey);

        // A property walk, not a substring test: procedure_log has a FIELD whose key is "role".
        using var onDisk = JsonDocument.Parse(ReadSeedFile(seedKey, "schema.json"));
        using var canonicalDocument = JsonDocument.Parse(FormSchemaParser.Serialize(raw));
        if (!ContainsProperty(onDisk.RootElement, "role"))
        {
            ContainsProperty(canonicalDocument.RootElement, "role").Should().BeFalse(
                "'{0}' declares no role on disk, so its canonical form must not either", seedKey);
        }
    }

    private static bool ContainsProperty(JsonElement element, string name)
        => element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject().Any(property =>
                property.NameEquals(name) || ContainsProperty(property.Value, name)),
            JsonValueKind.Array => element.EnumerateArray().Any(item => ContainsProperty(item, name)),
            _ => false
        };

    /// <summary>
    /// The no-loss assertion is only worth having if it actually fails when a property is dropped.
    /// </summary>
    [Fact]
    public void NothingLostAssertion_DetectsADroppedProperty()
    {
        var losses = FindLosses(
            """{"key":"a","editable_by":"subject"}""",
            """{"key":"a"}""");

        losses.Should().ContainSingle().Which.Should().Contain("editable_by");
    }

    [Fact]
    public void NothingLostAssertion_DetectsAChangedValue()
    {
        var losses = FindLosses(
            """{"fields":[{"key":"a","required":true}]}""",
            """{"fields":[{"key":"a","required":false}]}""");

        losses.Should().ContainSingle().Which.Should().Contain("required");
    }

    [Fact]
    public void NothingLostAssertion_AllowsAnAddedDefault()
    {
        var losses = FindLosses(
            """{"key":"a"}""",
            """{"key":"a","required":false}""");

        losses.Should().BeEmpty();
    }

    /// <summary>
    /// T126 added a root <c>rated_level_field</c> pointer at the scale field carrying THE entrustment
    /// rating. Named explicitly for the same reason <c>observation_date_field</c> is: a Serialize that
    /// forgets it fails here by name rather than as an opaque corpus-wide diff.
    /// </summary>
    /// <remarks>
    /// This loss is silent in exactly the way T126 exists to stop. An activity whose pinned schema
    /// carries no pointer cannot say which ladder its ordinal sits on, so a five-rung "4" plotted
    /// against the six-rung CPSA axis draws as rung "3b" and looks entirely normal.
    /// </remarks>
    [Fact]
    public void RatedLevelField_SurvivesParseSerializeParse()
    {
        const string schemaJson = """
            {
              "version": 1,
              "rated_level_field": "overall_level",
              "sections": [
                {
                  "key": "assessment",
                  "title": "Assessment",
                  "fields": [
                    { "key": "overall_level", "type": "scale", "label": "Overall", "options": ["1", "2", "3"], "scale_key": "O-R Scale" }
                  ]
                }
              ]
            }
            """;

        var canonical = FormSchemaParser.Serialize(FormSchemaParser.Parse(schemaJson));

        canonical.Should().Contain("\"rated_level_field\":\"overall_level\"");
        FormSchemaParser.Parse(canonical).RatedLevelField.Should().Be("overall_level");
        AssertNothingLost(schemaJson, canonical, "rated_level_field fixture");
    }

    /// <summary>
    /// The mirror. Six seeds rate nothing — a reflection, a procedure log, a journal club — so
    /// canonicalisation must not invent a rating they do not assert.
    /// </summary>
    [Fact]
    public void RatedLevelField_StaysAbsentWhenTheSchemaDeclaresNone()
    {
        const string schemaJson = """
            {
              "version": 1,
              "sections": [
                {
                  "key": "reflection",
                  "title": "Reflection",
                  "fields": [
                    { "key": "what_i_learned", "type": "longtext", "label": "What I learned" }
                  ]
                }
              ]
            }
            """;

        var canonical = FormSchemaParser.Serialize(FormSchemaParser.Parse(schemaJson));

        canonical.Should().NotContain("rated_level_field");
        FormSchemaParser.Parse(canonical).RatedLevelField.Should().BeNull();
    }

    /// <summary>
    /// The corpus-wide invariant, and the one that will catch the next seed rather than this one:
    /// a schema that carries a scale field declares which of them is the rating, and a schema that
    /// carries none declares nothing.
    /// </summary>
    /// <remarks>
    /// The parser already refuses a pointer naming a missing or non-scale field, so what is left to
    /// assert is the omission — which the parser cannot catch, because "declares no rating" is a
    /// legitimate state. Ten more v11.1 tools are queued in T120; each carries a scale field, and each
    /// will fail here if it forgets the pointer. Before T126 the only thing that knew was the credit
    /// rules' <c>minimum_level_field</c>, so a tool that credited nothing had no stated rated field.
    /// </remarks>
    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void Schema_DeclaresARatedFieldExactlyWhenItCarriesAScale(string seedKey)
    {
        var schema = FormSchemaParser.Parse(ReadSeedFile(seedKey, "schema.json"));

        var scaleFields = schema.Sections
            .SelectMany(section => section.Fields)
            .Where(field => field.Type == FieldType.Scale)
            .Select(field => field.Key)
            .ToArray();

        if (scaleFields.Length == 0)
        {
            schema.RatedLevelField.Should().BeNull(
                "'{0}' declares no scale field, so it rates nothing and must not claim to", seedKey);
            return;
        }

        schema.RatedLevelField.Should().NotBeNull(
            "'{0}' declares {1} scale field(s) ({2}) and nothing else can say which is the entrustment " +
            "rating — that is the gap T126 closed",
            seedKey, scaleFields.Length, string.Join(", ", scaleFields));

        scaleFields.Should().Contain(schema.RatedLevelField!);
    }

    /// <summary>
    /// The pointer and the credit directive answer different questions, and today they agree across the
    /// whole corpus. Pinned so that the day they diverge is a deliberate decision with a failing test
    /// behind it, rather than a discovery.
    /// </summary>
    /// <remarks>
    /// They are NOT the same question. <c>minimum_level_field</c> names the field a particular directive
    /// gates on, which could legitimately be one of the five or six component scales that <c>dops</c>,
    /// <c>acat</c>, <c>mini_cex</c> and <c>cbd</c> each declare. That they coincide everywhere today is
    /// why the credit rules could stand in for a rated field at all — and why the substitution looked
    /// safe enough to survive until T126.
    /// </remarks>
    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void Schema_RatedFieldAgreesWithTheCreditDirective(string seedKey)
    {
        var schema = FormSchemaParser.Parse(ReadSeedFile(seedKey, "schema.json"));
        var creditJson = ReadSeedFile(seedKey, "credit.json");

        using var document = JsonDocument.Parse(creditJson);
        var minimumLevelFields = new List<string>();
        CollectMinimumLevelFields(document.RootElement, minimumLevelFields);

        foreach (var field in minimumLevelFields.Distinct(StringComparer.Ordinal))
        {
            schema.RatedLevelField.Should().Be(field,
                "'{0}' credits on '{1}', and while the two questions differ the corpus has never disagreed",
                seedKey, field);
        }
    }

    /// <summary>
    /// T137 added a root <c>evidence_epa_field</c> pointer at the <c>epa</c> field naming the EPA an activity is
    /// evidence for. Named explicitly, like the other two pointers, so a Serialize that forgets it fails here by name.
    /// </summary>
    /// <remarks>
    /// This loss would re-create the defect T137 exists for without failing anything: every activity created after the
    /// publish would stamp no EPA, and a released MSF campaign would list as identical rows again. Worse than the other
    /// two pointers, it would also make every EPA-crediting type unsaveable, because <c>EvidenceEpa.EnsureCreditAgrees</c>
    /// refuses credit that reads an EPA field the form does not declare.
    /// </remarks>
    [Fact]
    public void EvidenceEpaField_SurvivesParseSerializeParse()
    {
        const string schemaJson = """
            {
              "version": 1,
              "evidence_epa_field": "epa_id",
              "sections": [
                {
                  "key": "evidence",
                  "title": "Evidence",
                  "fields": [
                    { "key": "epa_id", "type": "epa", "label": "EPA", "required": true }
                  ]
                }
              ]
            }
            """;

        var canonical = FormSchemaParser.Serialize(FormSchemaParser.Parse(schemaJson));

        canonical.Should().Contain("\"evidence_epa_field\":\"epa_id\"");
        FormSchemaParser.Parse(canonical).EvidenceEpaField.Should().Be("epa_id");
        FormSchemaParser.Serialize(FormSchemaParser.Parse(canonical)).Should().Be(canonical, "the pointer must canonicalise to a fixed point");
        AssertNothingLost(schemaJson, canonical, "evidence_epa_field fixture");
    }

    /// <summary>
    /// The mirror. A journal club, a procedure log, a QI project, a research output and a teaching session are about no
    /// single EPA, so canonicalisation must not invent one.
    /// </summary>
    [Fact]
    public void EvidenceEpaField_StaysAbsentWhenTheSchemaDeclaresNone()
    {
        const string schemaJson = """
            {
              "version": 1,
              "sections": [
                {
                  "key": "evidence",
                  "title": "Evidence",
                  "fields": [
                    { "key": "epa_id", "type": "epa", "label": "EPA" }
                  ]
                }
              ]
            }
            """;

        var canonical = FormSchemaParser.Serialize(FormSchemaParser.Parse(schemaJson));

        canonical.Should().NotContain("evidence_epa_field");
        FormSchemaParser.Parse(canonical).EvidenceEpaField.Should().BeNull();
    }

    /// <summary>
    /// The corpus-wide invariant that catches the next seed: a schema carrying an <c>epa</c> field says which field is
    /// its EPA, and one carrying none says nothing. The parser cannot catch the omission, because "about no EPA" is a
    /// legitimate state; this is what stops a new instrument shipping with rows the lists cannot tell apart.
    /// </summary>
    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void Schema_DeclaresAnEvidenceEpaFieldExactlyWhenItCarriesAnEpaField(string seedKey)
    {
        var schema = FormSchemaParser.Parse(ReadSeedFile(seedKey, "schema.json"));

        var epaFields = schema.Sections
            .SelectMany(section => section.Fields)
            .Where(field => field.Type == FieldType.Epa)
            .Select(field => field.Key)
            .ToArray();

        if (epaFields.Length == 0)
        {
            schema.EvidenceEpaField.Should().BeNull(
                "'{0}' declares no EPA field, so it is about no EPA and must not claim to be", seedKey);
            return;
        }

        schema.EvidenceEpaField.Should().NotBeNull(
            "'{0}' declares EPA field(s) ({1}) and nothing else can stamp which EPA its activities are about — the gap " +
            "T137 closed",
            seedKey, string.Join(", ", epaFields));

        epaFields.Should().Contain(schema.EvidenceEpaField!);
    }

    /// <summary>
    /// The list and credit can never name different EPAs, across the whole corpus: every directive that reads an EPA
    /// field reads the schema's pointer. Asserted here by the same check publish runs, and restated by hand so the test
    /// does not merely agree with the rule it guards.
    /// </summary>
    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void Schema_EvidenceEpaFieldIsTheFieldEveryCreditDirectiveReads(string seedKey)
    {
        var schema = FormSchemaParser.Parse(ReadSeedFile(seedKey, "schema.json"));
        var credit = CreditRulesParser.Parse(ReadSeedFile(seedKey, "credit.json"));

        foreach (var directive in credit.CountsFor.Where(directive => directive.CurriculumItemMatchRule.EpaField is not null))
        {
            directive.CurriculumItemMatchRule.EpaField.Should().Be(schema.EvidenceEpaField,
                "'{0}' credits the EPA in '{1}', so that is the EPA its activities must be stamped with",
                seedKey, directive.CurriculumItemMatchRule.EpaField);
        }

        var agrees = () => Wombat.Domain.Activities.EvidenceEpa.EnsureCreditAgrees(schema, credit);
        agrees.Should().NotThrow();
    }

    private static void CollectMinimumLevelFields(JsonElement element, List<string> into)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    if (property.NameEquals("minimum_level_field") && property.Value.ValueKind == JsonValueKind.String)
                    {
                        var value = property.Value.GetString();
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            into.Add(value);
                        }
                    }

                    CollectMinimumLevelFields(property.Value, into);
                }

                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    CollectMinimumLevelFields(item, into);
                }

                break;
        }
    }

    private static void AssertNothingLost(string rawJson, string canonicalJson, string context)
    {
        var losses = FindLosses(rawJson, canonicalJson);

        losses.Should().BeEmpty(
            "{0} must survive Parse+Serialize intact — a missing Serialize half is dropped silently at publish. Losses: {1}",
            context,
            string.Join("; ", losses));
    }

    private static IReadOnlyList<string> FindLosses(string rawJson, string canonicalJson)
    {
        using var raw = JsonDocument.Parse(rawJson);
        using var canonical = JsonDocument.Parse(canonicalJson);

        var losses = new List<string>();
        Compare(raw.RootElement, canonical.RootElement, "$", losses);
        return losses;
    }

    private static void Compare(JsonElement raw, JsonElement canonical, string path, List<string> losses)
    {
        // WorkflowParser.Serialize collapses a single-element `from` array to a bare string. That
        // is a documented equivalence in the DSL, not a loss.
        if (raw.ValueKind == JsonValueKind.Array &&
            canonical.ValueKind != JsonValueKind.Array &&
            raw.GetArrayLength() == 1)
        {
            Compare(raw[0], canonical, path + "[0]", losses);
            return;
        }

        if (raw.ValueKind != canonical.ValueKind)
        {
            losses.Add($"{path}: raw is {raw.ValueKind}, canonical is {canonical.ValueKind}");
            return;
        }

        switch (raw.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in raw.EnumerateObject())
                {
                    if (!canonical.TryGetProperty(property.Name, out var canonicalValue))
                    {
                        losses.Add($"{path}.{property.Name}: present on disk, missing after Parse+Serialize");
                        continue;
                    }

                    Compare(property.Value, canonicalValue, $"{path}.{property.Name}", losses);
                }

                break;

            case JsonValueKind.Array:
                if (raw.GetArrayLength() != canonical.GetArrayLength())
                {
                    losses.Add($"{path}: raw has {raw.GetArrayLength()} entries, canonical has {canonical.GetArrayLength()}");
                    break;
                }

                for (var index = 0; index < raw.GetArrayLength(); index++)
                {
                    Compare(raw[index], canonical[index], $"{path}[{index}]", losses);
                }

                break;

            case JsonValueKind.String:
                // The parsers trim strings, which is a deliberate normalisation rather than a loss.
                var rawText = raw.GetString()?.Trim();
                if (!string.Equals(rawText, canonical.GetString(), StringComparison.Ordinal))
                {
                    losses.Add($"{path}: '{rawText}' became '{canonical.GetString()}'");
                }

                break;

            case JsonValueKind.Number:
                if (raw.GetDecimal() != canonical.GetDecimal())
                {
                    losses.Add($"{path}: {raw.GetRawText()} became {canonical.GetRawText()}");
                }

                break;

            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
            default:
                break;
        }
    }

    private static string ReadSeedFile(string seedKey, string fileName)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", seedKey, fileName));

    private static IEnumerable<string> EnumerateSeedDirectories()
        => Directory.EnumerateDirectories(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds"));
}
