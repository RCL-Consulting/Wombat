using FluentAssertions;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Activities;

/// <summary>
/// The <c>msf_cpsa</c> seed, whose shape is load-bearing in ways the corpus-wide guards cannot see. (T121)
/// </summary>
/// <remarks>
/// <para>
/// This is deliberately NOT part of <see cref="CpsaWbaSeedTests" />, whose theories assume the
/// request → assess → feedback shape of an assessor-completed WBA: a <c>submit</c> transition, a
/// <c>requested</c> state, exactly one terminal state called <c>completed</c>, and
/// <c>strengths</c>/<c>improvements</c>/<c>plan</c> fields. <c>msf_cpsa</c> has none of those, because it
/// is not filled in by anybody — it is written by the system when a campaign is released. Adding its key
/// to that file's list would fail five theories for reasons that say nothing about this seed.
/// </para>
/// <para>
/// What it does share with them is the CPSA ladder, which <see cref="SeedScaleKeyTests" /> already pins.
/// </para>
/// </remarks>
public sealed class MsfSeedTests
{
    private const string SeedKey = "msf_cpsa";
    private const string SystemActorRule = "role:Coordinator|role:Administrator";

    private static string ReadSeedFile(string fileName)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", SeedKey, fileName));

    [Fact]
    public void TheRecordTransitionIsTheOnlyWayOut_AndOnlyStaffCanTakeIt()
    {
        // The gate that stops a trainee crediting themselves. ListActivityTypesQuery offers every
        // published speciality-scoped type to every member of that speciality, and nothing in the
        // product expresses "system-managed", so a trainee CAN create a stray draft. This rule is what
        // makes that draft permanently inert.
        var workflow = WorkflowParser.Parse(ReadSeedFile("workflow.json"));

        var transition = workflow.Transitions.Should().ContainSingle().Subject;
        transition.Key.Should().Be("record");
        transition.From.Should().Equal("draft");
        transition.To.Should().Be("recorded");
        ActorRuleParser.Serialize(transition.Actor).Should().Be(SystemActorRule);

        workflow.States.Where(state => state.Terminal).Select(state => state.Key).Should().Equal("recorded");
        workflow.InitialState.Should().Be("draft");
    }

    [Fact]
    public void EverySectionIsOwnedByStaff_SoATraineesStrayDraftStaysEmpty()
    {
        // FieldPermissionEvaluator's default is `subject|creator`, and a trainee creating their own
        // stray draft is both. Without these rules they could fill in the reviewer's entrustment level
        // and narrative on a row that carries the product's MSF branding.
        var schema = FormSchemaParser.Parse(ReadSeedFile("schema.json"));

        schema.Sections.Should().NotBeEmpty();
        foreach (var section in schema.Sections)
        {
            section.EditableBy.Should().NotBeNull("'{0}' would otherwise default to subject|creator", section.Key);
            ActorRuleParser.Serialize(section.EditableBy!).Should().Be(SystemActorRule);
        }
    }

    [Fact]
    public void ItCreditsNothing_BecauseMsfConsumesNoneOfTheFiftyFiveEncounters()
    {
        // College decision D8, and the reason it had to be right first time: `counts_for` is permanent
        // per pinned version. Activity.SchemaVersion is assigned exactly once, there is no re-pin path
        // ([T107]), and a rebuild replays each activity against its pinned version — so whatever v1
        // ships with is permanent for every activity created under v1, in both directions.
        CreditRulesParser.Parse(ReadSeedFile("credit.json")).CountsFor.Should().BeEmpty();
    }

    [Fact]
    public void TheLevelIsOptional_AndTheProvenanceFieldsAreNot()
    {
        // D10: the releasing reviewer states the level, or states none. An ungated directive would have
        // been worse than no level at all — a directive with neither minimum_level_field nor
        // minimum_level_fixed returns NotGated(), which is MinimumMet: true, recording the trainee as
        // having met the supervision minimum on the strength of a questionnaire that never asked.
        var schema = FormSchemaParser.Parse(ReadSeedFile("schema.json"));
        var fields = schema.Sections.SelectMany(section => section.Fields).ToDictionary(field => field.Key, StringComparer.Ordinal);

        fields["overall_level"].Required.Should().BeFalse();
        fields["summary"].Required.Should().BeFalse();

        foreach (var key in new[] { "epa_id", "campaign_id", "observed_on", "respondent_count" })
        {
            fields[key].Required.Should().BeTrue("'{0}' is what makes the record traceable", key);
        }

        schema.ObservationDateField.Should().Be("observed_on");
        schema.RatedLevelField.Should().Be("overall_level");
    }

    [Fact]
    public void ItCarriesNoFieldThatCouldIdentifyARespondent()
    {
        // AccessReportBuilder puts an activity's whole DataJson into the subject's data-subject access
        // report and the portfolio PDF prints it. MSF only works because it is confidential and
        // aggregated: no email, no per-respondent comment, no per-category breakdown, ever.
        var schema = FormSchemaParser.Parse(ReadSeedFile("schema.json"));
        var keys = schema.Sections.SelectMany(section => section.Fields).Select(field => field.Key).ToArray();

        keys.Should().BeEquivalentTo(
            ["epa_id", "campaign_id", "observed_on", "respondent_count", "overall_level", "summary"],
            "a new field here is a new thing disclosed in a data-subject access report");
    }

    [Fact]
    public void ItIsRegisteredWithTheSeedCatalogue_AsACollegeOwnedSpecialityType()
    {
        // An unregistered folder is invisible to the T103 refresher, so later edits to it would be inert
        // on any database that has already booted.
        var entry = ActivityTypeSeedCatalogue.Entries.Should().ContainSingle(candidate => candidate.Key == SeedKey).Subject;

        entry.Source.Should().Be(ActivityTypeSeedSource.PaediatricCollege);
        entry.DisplayFields.Should().Be(DisplayFieldsRule.None);
    }
}
