using System.Text.Json;
using FluentAssertions;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;
using Wombat.Infrastructure.Activities;

namespace Wombat.Infrastructure.Tests.Activities;

/// <summary>
/// Guards the paediatric (CPSA) WBA seeds against the specific ways this platform lets a schema be
/// quietly wrong. Every one of these asserts a behaviour that, if broken, fails silently in
/// production rather than throwing.
/// </summary>
public sealed class CpsaWbaSeedTests
{
    private const string ScaleName = "CPSA Paediatric Entrustment Scale v11.1";

    /// <summary>
    /// The four assessor-completed v11.1 WBA tools. <b>Do not add <c>msf_cpsa</c> here.</b>
    /// </summary>
    /// <remarks>
    /// It is a CPSA seed and the name of this class invites the mistake, but the theories below assume
    /// the request → assess → feedback shape: a <c>submit</c> transition, a <c>requested</c> state,
    /// exactly one terminal state called <c>completed</c>, and strengths/improvements/plan fields.
    /// <c>msf_cpsa</c> is written by the system on release and has none of them; it is guarded by
    /// <see cref="MsfSeedTests" /> instead. (T121)
    /// </remarks>
    public static TheoryData<string> SeedKeys =>
    [
        "mini_cex_cpsa",
        "dops_cpsa",
        "cbd_cpsa",
        "direct_observation_cpsa"
    ];

    private static string SeedDirectory(string key)
        => Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", key);

    private static FormSchema Schema(string key)
        => FormSchemaParser.Parse(File.ReadAllText(Path.Combine(SeedDirectory(key), "schema.json")));

    private static Workflow Workflow(string key)
        => WorkflowParser.Parse(File.ReadAllText(Path.Combine(SeedDirectory(key), "workflow.json")));

    private static FormField RatingField(string key)
        => Schema(key).Sections.SelectMany(section => section.Fields)
            .Single(field => field.Key == "overall_level");

    [Theory]
    [MemberData(nameof(SeedKeys))]
    public void RatingFieldDeclaresNoOptions(string key)
    {
        // Declaring options would do two bad things at once: cap the validator at those exact values
        // (rejecting the sixth rung) and override the College's ladder in the picker, so the
        // clinician sees bare numbers instead of 3a / 3b.
        RatingField(key).Options.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(SeedKeys))]
    public void RatingFieldBindsToTheCollegeScaleByExactName(string key)
    {
        // scale_key has no key column to bind to — it must be the scale's exact Name (or its numeric
        // Id). A wrong string raises no error; the field silently degrades to a plain number box.
        RatingField(key).ScaleKey.Should().Be(ScaleName);
    }

    [Theory]
    [MemberData(nameof(SeedKeys))]
    public void RatingFieldIsBoundedToTheSixRungs(string key)
    {
        // With options omitted, validation.min/max is the ONLY remaining server-side guard.
        var validation = RatingField(key).Validation;
        validation.Should().NotBeNull();
        validation!.Min.Should().Be(1);
        validation.Max.Should().Be(6);
    }

    [Theory]
    [MemberData(nameof(SeedKeys))]
    public void AcceptsTheTopRung(string key)
    {
        // The whole point: rung 6 ("5" on the College's ladder) must submit. The previously seeded
        // types hard-coded options 1-5 and rejected it.
        Errors(key, 6).Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(SeedKeys))]
    public void AcceptsTheSplitThirdRungs(string key)
    {
        // 3a and 3b are stored as orders 3 and 4 — a scale field can never store the label itself.
        Errors(key, 3).Should().BeEmpty();
        Errors(key, 4).Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(SeedKeys))]
    public void RejectsRungsOutsideTheLadder(string key)
    {
        Errors(key, 0).Should().NotBeEmpty();
        Errors(key, 7).Should().NotBeEmpty();
    }

    private static IReadOnlyList<ActivityValidationErrorDto> Errors(string key, int rung)
    {
        var data = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["epa_id"] = 1,
            ["assessor_user_id"] = "assessor-1",
            ["observed_on"] = "2026-09-16",
            ["overall_level"] = rung
        });

        return new SchemaValidator()
            .Validate(Schema(key), data, SchemaValidationMode.Draft)
            .Where(error => error.FieldKey == "overall_level")
            .ToList();
    }

    [Theory]
    [MemberData(nameof(SeedKeys))]
    public void CapturesTheEncounterDateAndPointsAtIt(string key)
    {
        // Capturing the date was only ever half of it. Until T119 the field was validated, stored in
        // DataJson and read by nothing at all: every date the product showed or credited against came
        // from the row's audit timestamp. The root pointer is what makes ActivityService stamp
        // Activity.ObservedOn from this field, so a schema that keeps the field but loses the pointer
        // is silently back to being dated by whenever the paperwork was filed.
        var schema = Schema(key);

        schema.Sections.SelectMany(section => section.Fields)
            .Should().Contain(field => field.Key == "observed_on" && field.Type == FieldType.Date);

        schema.ObservationDateField.Should().Be("observed_on");
    }

    [Theory]
    [MemberData(nameof(SeedKeys))]
    public void SubmitIsTheFirstTransitionOutOfTheInitialState(string key)
    {
        // The New Activity page fires the FIRST transition out of the initial state that the creator
        // is allowed to take. If 'cancel' were declared first, pressing Submit would cancel the
        // request outright — and, were 'cancelled' terminal, credit it on the way out.
        var workflow = Workflow(key);
        var firstFromInitial = workflow.Transitions
            .First(transition => transition.From.Contains(workflow.InitialState, StringComparer.Ordinal));

        firstFromInitial.Key.Should().Be("submit");
    }

    [Theory]
    [MemberData(nameof(SeedKeys))]
    public void OnlyCompletionIsTerminal(string key)
    {
        // Credit fires on any transition into a terminal state, and an abandoned request still
        // carries a filled-in epa_id. Marking declined/cancelled terminal would count refused and
        // withdrawn requests toward the trainee's observation volume.
        Workflow(key).States.Where(state => state.Terminal).Select(state => state.Key)
            .Should().Equal("completed");
    }

    [Theory]
    [MemberData(nameof(SeedKeys))]
    public void AssessorFieldsAreSchemaRequired_AndEachTransitionDeclaresHowMuchItChecks(string key)
    {
        // T105. The schema says what is mandatory and each transition says how much of it counts. Before T105 every
        // transition validated the whole schema, so these four could not be required without also blocking decline and
        // cancel; they hid in requires_fields on 'complete' instead. requires_fields is back to meaning "additionally".
        var schema = Schema(key);
        var assessorEntered = new[] { "overall_level", "strengths", "improvements", "plan" };

        schema.Sections.SelectMany(section => section.Fields)
            .Where(field => assessorEntered.Contains(field.Key))
            .Should().HaveCount(4).And.OnlyContain(field => field.Required);

        var transitions = Workflow(key).Transitions.ToDictionary(transition => transition.Key, StringComparer.Ordinal);
        transitions["complete"].RequiresFields.Should().BeEmpty("the schema now says it");

        // The trainee's submit checks what is theirs to fill; completion checks everything; the two ways out check
        // formats only, so a half-filled draft can be withdrawn and a decline needs no ratings.
        transitions["submit"].Validation.Should().Be(TransitionValidation.Owned);
        transitions["complete"].Validation.Should().Be(TransitionValidation.All);
        transitions["decline"].Validation.Should().Be(TransitionValidation.Draft);
        transitions["cancel"].Validation.Should().Be(TransitionValidation.Draft);
    }

    [Theory]
    [MemberData(nameof(SeedKeys))]
    public void AssessorOwnershipIsDeclaredOnTheRequestedStateAndTheAssessorSections(string key)
    {
        // Ownership is declared, never inferred. Without 'editable_by' on the requested state the
        // bound assessor falls back to the subject|creator default and cannot write a rating at all;
        // without it on the two assessor sections the state gate would hand them the whole form.
        //
        // The request section deliberately keeps the default. That is the one assertion here with
        // teeth: if request were assessor-owned, a 'complete' patch could rewrite epa_id,
        // assessor_user_id or observed_on and silently redirect which curriculum item gets credited.
        ActorRule assessor = new FieldUserActorRule("assessor_user_id");

        Workflow(key).States.Single(state => state.Key == "requested")
            .EditableBy.Should().Be(assessor);

        var sections = Schema(key).Sections.ToDictionary(section => section.Key, StringComparer.Ordinal);
        sections["assessment"].EditableBy.Should().Be(assessor);
        sections["feedback"].EditableBy.Should().Be(assessor);
        sections["request"].EditableBy.Should().BeNull();
    }

    public static TheoryData<string> LegacySeedKeys =>
    [
        "mini_cex",
        "dops",
        "cbd",
        "acat"
    ];

    [Theory]
    [MemberData(nameof(LegacySeedKeys))]
    public void LegacyAssessorSectionsAreAssessorOwned_AndOnlyCompletionAsksForTheirRequiredFields(string key)
    {
        // The legacy seeds route the assessor through 'accept' before they can write anything. Before T105 every
        // transition validated the whole schema, so a required rating made 'accept', 'decline' and 'cancel'
        // unsatisfiable; the ratings hid in requires_fields on 'complete'. Now the schema marks them required and only
        // 'complete' checks the whole form.
        ActorRule assessor = new FieldUserActorRule("assessor_user_id");
        var schema = Schema(key);
        var assessorSections = schema.Sections
            .Where(section => !string.Equals(section.Key, "request", StringComparison.Ordinal))
            .ToList();

        assessorSections.Should().HaveCount(2);
        assessorSections.Should().OnlyContain(section => section.EditableBy == assessor);
        assessorSections.SelectMany(section => section.Fields)
            .Where(field => field.Required)
            .Select(field => field.Key)
            .Should().Contain("overall", "the overall rating is what completion is for");

        Workflow(key).States.Single(state => state.Key == "accepted")
            .EditableBy.Should().Be(assessor);

        var transitions = Workflow(key).Transitions.ToDictionary(transition => transition.Key, StringComparer.Ordinal);
        transitions["complete"].Validation.Should().Be(TransitionValidation.All);
        transitions["complete"].RequiresFields.Should().BeEmpty("the schema now says it");
        transitions["accept"].Validation.Should().Be(TransitionValidation.Draft);
        transitions["decline"].Validation.Should().Be(TransitionValidation.Draft);
        transitions["cancel"].Validation.Should().Be(TransitionValidation.Draft);
    }

    [Theory]
    [MemberData(nameof(LegacySeedKeys))]
    public void LegacySeedsCaptureTheEncounterDateInTheRequestSection(string key)
    {
        // These four shipped with no date field at all, so every one of them was dated for ever by
        // whenever its form happened to be filed. T119 gave them observed_on, and 'request' is the
        // section it has to live in: that section keeps the default subject|creator permission, so
        // the trainee states the date when raising the request and the assessor cannot move it
        // afterwards. In an assessor-owned section a 'complete' patch could re-date the encounter,
        // and with it the training stage its credit is graded against.
        var schema = Schema(key);

        schema.Sections.Single(section => section.Key == "request").Fields
            .Should().Contain(field =>
                field.Key == "observed_on" && field.Type == FieldType.Date && field.Required);

        schema.ObservationDateField.Should().Be("observed_on");
    }
}

