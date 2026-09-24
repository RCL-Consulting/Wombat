using FluentAssertions;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Activities;

/// <summary>
/// T102. Every seed meets <see cref="ActorFieldRules.EnsurePublishable" />, the check <c>ActivityType.SaveDraft</c> and
/// <c>PublishDraft</c> now run.
/// </summary>
/// <remarks>
/// <para>
/// The seeders create each type through <c>SaveDraft</c> then <c>PublishDraft</c>, and the refresher republishes
/// through the same two calls. A seed that failed the check would therefore not fail a build or a parse test: it would
/// throw on the first boot of a fresh database, or leave a seeded type stuck on its old version with an error in the
/// log. These theories move that discovery to the test run, one row per seed folder, so the failure names the seed.
/// </para>
/// <para>
/// Enumerated from the folders on disk rather than the catalogue, as <see cref="SeedRoundTripTests" /> does, so a
/// folder added without a catalogue entry is still checked.
/// </para>
/// </remarks>
public sealed class SeedPublishabilityTests
{
    /// <summary>The seeds that name a person: every generic and CPSA WBA instrument.</summary>
    private static readonly string[] SeedsWithANomineeField =
    [
        "acat", "cbd", "cbd_cpsa", "direct_observation_cpsa", "dops", "dops_cpsa", "mini_cex", "mini_cex_cpsa",
        // T120. The reflective exercise's "supervisor or mentor" is the same nominee field, requiring an Assessor.
        "cca_cpsa", "rca_cpsa", "chart_stimulated_recall_cpsa", "reflective_exercise_cpsa",
        // T154. The audit's supervisor and the portfolio's reviewer are the same field, and declare the Assessor role
        // outright rather than by default: a supervisor, not a committee member, signs each off.
        "clinical_audit_cpsa", "portfolio_review_cpsa"
    ];

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
    public void EverySeed_PassesThePublishCheck(string seedKey)
    {
        var schema = FormSchemaParser.Parse(ReadSeedFile(seedKey, "schema.json"));
        var workflow = WorkflowParser.Parse(ReadSeedFile(seedKey, "workflow.json"));

        var act = () => ActorFieldRules.EnsurePublishable(schema, workflow);

        act.Should().NotThrow("'{0}' is created and republished through SaveDraft and PublishDraft, which both run the check", seedKey);
    }

    /// <summary>
    /// What the seeders actually do, end to end on the entity: save the raw files as a draft, then publish it. This is
    /// the path that would throw at boot.
    /// </summary>
    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void EverySeed_SavesAndPublishesAsTheSeedersDo(string seedKey)
    {
        var type = new ActivityType
        {
            Key = seedKey,
            Name = seedKey,
            Scope = ActivityScope.Speciality,
            OwnerUserId = ActivityTypeSeedCatalogue.SeedActorUserId,
            CreatedOn = new DateTime(2026, 9, 24, 0, 0, 0, DateTimeKind.Utc)
        };

        type.SaveDraft(
            ReadSeedFile(seedKey, "schema.json"),
            ReadSeedFile(seedKey, "workflow.json"),
            ReadSeedFile(seedKey, "credit.json"),
            "[]",
            ActivityTypeSeedCatalogue.SeedActorUserId);
        type.PublishDraft(ActivityTypeSeedCatalogue.SeedActorUserId);

        type.Version.Should().Be(1);
        type.HasDraft.Should().BeFalse();
    }

    /// <summary>
    /// The seeds' nominee field is <c>assessor_user_id</c>, a user field that declares no role, so its nominee must be
    /// an Assessor — the rule every pinned version of these types was filed under before T102 existed. Pinned so a
    /// seed edit that changes who may be named is a deliberate one.
    /// </summary>
    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void EverySeedNomineeField_IsAUserFieldRequiringAnAssessor(string seedKey)
    {
        var schema = FormSchemaParser.Parse(ReadSeedFile(seedKey, "schema.json"));
        var workflow = WorkflowParser.Parse(ReadSeedFile(seedKey, "workflow.json"));

        var roles = ActorFieldRules.RequiredRolesByNomineeField(schema, workflow);

        if (!SeedsWithANomineeField.Contains(seedKey, StringComparer.Ordinal))
        {
            roles.Should().BeEmpty("'{0}' names nobody", seedKey);
            return;
        }

        roles.Keys.Should().Equal(new[] { "assessor_user_id" }, "'{0}' names its assessor and nobody else", seedKey);
        roles["assessor_user_id"].Should().Equal(WombatRoles.Assessor);

        var declarations = schema.Sections.SelectMany(section => section.Fields)
            .Where(field => field.Key == "assessor_user_id")
            .ToList();
        declarations.Should().ContainSingle().Which.Type.Should().Be(FieldType.User);
        ActorFieldRules.ActorFieldNames(schema, workflow).Should().Equal("assessor_user_id");
    }

    /// <summary>
    /// The theories above are only meaningful if the corpus actually exercises <c>field:</c> rules. The seeds put them
    /// on transitions, states and sections; none puts one on a single field, so that fourth location is covered by
    /// <c>ActorFieldRulesTests</c> in Domain.Tests rather than here. If a seed rewrite ever dropped the rules,
    /// <see cref="EverySeed_PassesThePublishCheck" /> would keep passing and prove nothing about them.
    /// </summary>
    [Fact]
    public void TheCorpus_ExercisesAFieldRuleOnTransitionsStatesAndSections()
    {
        var transitionActor = false;
        var stateEditableBy = false;
        var sectionEditableBy = false;

        foreach (var directory in EnumerateSeedDirectories())
        {
            var seedKey = Path.GetFileName(directory);
            var schema = FormSchemaParser.Parse(ReadSeedFile(seedKey, "schema.json"));
            var workflow = WorkflowParser.Parse(ReadSeedFile(seedKey, "workflow.json"));

            transitionActor |= workflow.Transitions.Any(transition => NamesAField(transition.Actor));
            stateEditableBy |= workflow.States.Any(state => NamesAField(state.EditableBy));
            sectionEditableBy |= schema.Sections.Any(section => NamesAField(section.EditableBy));
        }

        transitionActor.Should().BeTrue("some seeded transition is acted on by the person a field names");
        stateEditableBy.Should().BeTrue("some seeded state is writable by the person a field names");
        sectionEditableBy.Should().BeTrue("some seeded section is writable by the person a field names");
        EnumerateSeedDirectories().Select(directory => Path.GetFileName(directory)).Should().Contain(SeedsWithANomineeField,
            "the nominee seeds this file names must exist, or the per-seed theory checks them as seeds that name nobody");
    }

    private static bool NamesAField(ActorRule? rule)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        ActorFieldRules.CollectFieldNames(rule, names);
        return names.Count > 0;
    }

    private static string ReadSeedFile(string seedKey, string fileName)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", seedKey, fileName));

    private static IEnumerable<string> EnumerateSeedDirectories()
        => Directory.EnumerateDirectories(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds"));
}
