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
    public void CapturesTheEncounterDate(string key)
    {
        // The older seeds carry no observation date, so the only date available is the audit
        // timestamp. These capture the real one, ready for the per-year quota work (T098 phase 3).
        Schema(key).Sections.SelectMany(section => section.Fields)
            .Should().Contain(field => field.Key == "observed_on" && field.Type == FieldType.Date);
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
    public void AssessorFieldsAreNotSchemaRequired(string key)
    {
        // Every transition validates the whole schema in Submit mode, so a required assessor field
        // would also block decline and cancel. They are gated by requires_fields on 'complete'.
        var schema = Schema(key);
        var assessorEntered = new[] { "overall_level", "strengths", "improvements", "plan" };

        schema.Sections.SelectMany(section => section.Fields)
            .Where(field => assessorEntered.Contains(field.Key))
            .Should().OnlyContain(field => !field.Required);

        Workflow(key).Transitions.Single(transition => transition.Key == "complete")
            .RequiresFields.Should().Contain(assessorEntered);
    }
}
