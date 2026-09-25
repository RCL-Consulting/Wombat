using FluentAssertions;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.ActivityTypes;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T102 fix 3, on the builder side: a <c>user</c> field's <c>role</c>, carried by <see cref="BuilderSchemaModel" />.
/// </summary>
/// <remarks>
/// <para>
/// The Form tab is the only schema-authoring path the product has, and <c>ActivityType.SaveDraft</c> stores whatever
/// <c>ToJson</c> emits. A DSL property the model does not carry is erased on every operator save (T133 is exactly that
/// defect, for two root pointers). For <c>role</c> the erasure would not be cosmetic: a User field that required a
/// Coordinator would silently go back to requiring an Assessor, and the write path would start accepting a different
/// set of people.
/// </para>
/// <para>
/// The parser refuses <c>role</c> on any field that is not a User field, and the publish check refuses inline options
/// on one that is. So the model has to drop both at the right moment, and only then, or a type switch makes the draft
/// unsaveable through the only door there is.
/// </para>
/// </remarks>
public sealed class BuilderNomineeRoleTests
{
    private const string CoordinatorField = """
        {
          "version": 1,
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "epa_id", "type": "epa", "label": "EPA", "required": true },
                { "key": "coordinator_user_id", "type": "user", "label": "Coordinator", "required": true, "role": "Coordinator" },
                { "key": "assessor_user_id", "type": "user", "label": "Assessor", "required": true }
              ]
            }
          ]
        }
        """;

    // ---- Parse -> ToJson carries the role ----

    [Fact]
    public void ANonDefaultRole_SurvivesTheBuilderRoundTrip()
    {
        var model = BuilderSchemaModel.Parse(CoordinatorField);

        Field(model, "coordinator_user_id").NomineeRole.Should().Be(WombatRoles.Coordinator);

        var reparsed = FormSchemaParser.Parse(model.ToJson());

        ParsedField(reparsed, "coordinator_user_id").NomineeRole.Should().Be(WombatRoles.Coordinator,
            "an operator who opens the type and saves must not turn a Coordinator field back into an Assessor field");
        ActorFieldRules.RequiredRolesForUserField(reparsed, "coordinator_user_id")
            .Should().Equal([WombatRoles.Coordinator], "the picker and the gate read the role through this helper");
    }

    [Fact]
    public void AUserFieldWithNoRole_StaysWithoutOne_AndStillMeansAssessor()
    {
        // Absent means Assessor. The builder must not materialise the default: a stored version with no role and a
        // draft with "role": "Assessor" would read as a change in the publish diff, and every seed would drift from
        // its on-disk form.
        var json = BuilderSchemaModel.Parse(CoordinatorField).ToJson();
        var reparsed = FormSchemaParser.Parse(json);

        ParsedField(reparsed, "assessor_user_id").NomineeRole.Should().BeNull();
        ActorFieldRules.RequiredRolesForUserField(reparsed, "assessor_user_id").Should().Equal([WombatRoles.Assessor]);
        json.Split("\"role\"").Should().HaveCount(2, "only the Coordinator field declares a role");
    }

    [Fact]
    public void ALowerCaseRole_ArrivesCanonical_AndIsEmittedCanonical()
    {
        // The parser normalises case, so the builder shows and saves the canonical name. A draft saved with
        // "coordinator" would match nothing in the directory, which matches on the role's stored name.
        var model = BuilderSchemaModel.Parse(CoordinatorField.Replace("\"role\": \"Coordinator\"", "\"role\": \"coordinator\""));

        Field(model, "coordinator_user_id").NomineeRole.Should().Be(WombatRoles.Coordinator);
        model.ToJson().Should().Contain("\"role\":\"Coordinator\"");
    }

    [Fact]
    public void SettingARoleOnTheModel_IsEmitted_AndTheDraftIsPublishable()
    {
        // What the Form tab's "Names a" select does.
        var model = BuilderSchemaModel.Parse(CoordinatorField);
        Field(model, "assessor_user_id").NomineeRole = WombatRoles.CommitteeMember;

        var reparsed = FormSchemaParser.Parse(model.ToJson());

        ParsedField(reparsed, "assessor_user_id").NomineeRole.Should().Be(WombatRoles.CommitteeMember);
        var publish = () => ActorFieldRules.EnsurePublishable(reparsed, NoFieldRules());
        publish.Should().NotThrow();
    }

    [Fact]
    public void AWhitespaceRoleOnTheModel_IsTheDefault_NotAnUnknownRole()
    {
        // The select's "Assessor (default)" option binds "", and a stray space must read the same way rather than as a
        // role called " " that the parser would refuse.
        var model = BuilderSchemaModel.Parse(CoordinatorField);
        Field(model, "coordinator_user_id").NomineeRole = "  ";

        var json = model.ToJson();

        json.Should().NotContain("\"role\"");
        ParsedField(FormSchemaParser.Parse(json), "coordinator_user_id").NomineeRole.Should().BeNull();
    }

    // ---- switching a field's type ----

    [Fact]
    public void SwitchingAUserFieldToText_DropsItsRole_AndTheJsonReparses()
    {
        // The parser refuses a role on any field that is not a User field. Emitting it anyway would make the draft
        // unsaveable the moment an operator changed the type, with no way to clear the role from the UI (the Role
        // select is only shown for User fields).
        var model = BuilderSchemaModel.Parse(CoordinatorField);
        Field(model, "coordinator_user_id").Type = FieldType.Text;

        var json = model.ToJson();

        json.Should().NotContain("\"role\"");
        var reparse = () => FormSchemaParser.Parse(json);
        var reparsed = reparse.Should().NotThrow().Subject;
        var field = ParsedField(reparsed, "coordinator_user_id");
        field.Type.Should().Be(FieldType.Text);
        field.NomineeRole.Should().BeNull();
        field.Options.Should().BeEmpty("a User field had no options to carry into a Text field");
        field.CatalogueKey.Should().BeNull();
    }

    [Fact]
    public void SwitchingAUserFieldAwayAndBack_RestoresItsRole()
    {
        // The model keeps the role while the field is not a User field, like a root pointer whose field is
        // temporarily the wrong type; an operator who fumbles the Type select does not lose the setting.
        var model = BuilderSchemaModel.Parse(CoordinatorField);
        var field = Field(model, "coordinator_user_id");

        field.Type = FieldType.Text;
        model.ToJson().Should().NotContain("\"role\"");

        field.Type = FieldType.User;
        var reparsed = FormSchemaParser.Parse(model.ToJson());

        ParsedField(reparsed, "coordinator_user_id").NomineeRole.Should().Be(WombatRoles.Coordinator);
    }

    [Fact]
    public void SwitchingAChoiceFieldToUser_DropsItsOptionsAndCatalogue_SoTheDraftStaysPublishable()
    {
        // The publish check refuses inline options or a catalogue on a User field: the people a User field offers come
        // from the directory, which is what the server judges against. A Choice field turned into a User field must
        // not carry its old options into a draft that then cannot be published.
        const string choice = """
            {
              "version": 1,
              "sections": [
                {
                  "key": "s",
                  "title": "S",
                  "fields": [
                    { "key": "who", "type": "choice", "label": "Who", "options": ["assessor-1", "assessor-2"], "catalogue": "people" }
                  ]
                }
              ]
            }
            """;

        var model = BuilderSchemaModel.Parse(choice);
        Field(model, "who").Type = FieldType.User;

        var json = model.ToJson();

        json.Should().NotContain("\"options\"").And.NotContain("\"catalogue\"")
            .And.NotContain("assessor-1", "a User field names whoever the directory offers, not an inline list");
        var reparsed = FormSchemaParser.Parse(json);
        ParsedField(reparsed, "who").Options.Should().BeEmpty();
        ParsedField(reparsed, "who").CatalogueKey.Should().BeNull();

        var publish = () => ActorFieldRules.EnsurePublishable(reparsed, NoFieldRules());
        publish.Should().NotThrow();
    }

    [Fact]
    public void SwitchingAChoiceFieldToUserAndBack_RestoresItsOptions()
    {
        // The other half of "kept on the model, emitted only while it applies".
        const string choice = """
            {
              "version": 1,
              "sections": [
                { "key": "s", "title": "S", "fields": [ { "key": "setting", "type": "choice", "label": "Setting", "options": ["ward", "clinic"] } ] }
              ]
            }
            """;

        var model = BuilderSchemaModel.Parse(choice);
        var field = Field(model, "setting");

        field.Type = FieldType.User;
        field.Type = FieldType.Choice;

        ParsedField(FormSchemaParser.Parse(model.ToJson()), "setting").Options.Select(option => option.Value)
            .Should().Equal("ward", "clinic");
    }

    [Fact]
    public void AStoredUserFieldWithInlineOptions_IsSavedWithoutThem()
    {
        // A version stored before T102 may carry inline options on a User field; the parser still accepts it so the
        // pinned version keeps loading. Opening it in the builder and saving must produce a draft the publish check
        // accepts, not one it refuses for options the operator cannot even see (the Options box is hidden for User).
        const string legacy = """
            {
              "version": 1,
              "sections": [
                { "key": "s", "title": "S", "fields": [ { "key": "assessor_user_id", "type": "user", "label": "Assessor", "options": ["someone"] } ] }
              ]
            }
            """;

        var reparsed = FormSchemaParser.Parse(BuilderSchemaModel.Parse(legacy).ToJson());

        ParsedField(reparsed, "assessor_user_id").Options.Should().BeEmpty();
        var publish = () => ActorFieldRules.EnsurePublishable(reparsed, NoFieldRules());
        publish.Should().NotThrow();
    }

    // ---- the seed corpus through the builder ----

    public static TheoryData<string> SeedsWithAUserField
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var seedKey in SeedKeysWithAUserField())
            {
                data.Add(seedKey);
            }

            return data;
        }
    }

    [Fact]
    public void TheSeedCorpusStillHasUserFields_SoTheTheoryBelowIsNotVacuous()
    {
        // Eight seeds name an assessor today. If the output copy of the seed folders ever stops reaching this test
        // project, the theory would pass with no rows; this fails instead.
        SeedKeysWithAUserField().Should().HaveCountGreaterThanOrEqualTo(8)
            .And.Contain(["mini_cex_cpsa", "cbd_cpsa", "dops_cpsa", "direct_observation_cpsa"]);
    }

    [Theory]
    [MemberData(nameof(SeedsWithAUserField))]
    public void ASeedWithAUserField_SurvivesTheBuilderRoundTrip_Unchanged(string seedKey)
    {
        // The builder's output must be exactly the canonical form of the seed: every field, every setting, every
        // pointer, and the role of every User field. Anything less and the first operator save of a seeded type drops
        // something, and the seed refresher then stops reaching that type (its newest version is no longer the
        // seeder's).
        var raw = ReadSeed(seedKey, "schema.json");
        var canonical = FormSchemaParser.Serialize(FormSchemaParser.Parse(raw));

        var throughTheBuilder = BuilderSchemaModel.Parse(raw).ToJson();

        throughTheBuilder.Should().Be(canonical, "the builder must round-trip '{0}' without loss or addition", seedKey);
    }

    [Theory]
    [MemberData(nameof(SeedsWithAUserField))]
    public void ASeedWithAUserField_KeepsEveryNomineeRole_AndEveryFieldSetting(string seedKey)
    {
        // The same claim as above, field by field, so a failure names what went missing rather than showing a diff of
        // two long JSON strings.
        var original = FormSchemaParser.Parse(ReadSeed(seedKey, "schema.json"));
        var reparsed = FormSchemaParser.Parse(BuilderSchemaModel.Parse(ReadSeed(seedKey, "schema.json")).ToJson());

        reparsed.ObservationDateField.Should().Be(original.ObservationDateField);
        reparsed.RatedLevelField.Should().Be(original.RatedLevelField);

        var before = original.Sections.SelectMany(section => section.Fields).ToList();
        var after = reparsed.Sections.SelectMany(section => section.Fields).ToList();
        after.Select(field => field.Key).Should().Equal(before.Select(field => field.Key));

        foreach (var (was, now) in before.Zip(after))
        {
            now.Type.Should().Be(was.Type, "{0}.{1} type", seedKey, was.Key);
            now.NomineeRole.Should().Be(was.NomineeRole, "{0}.{1} role", seedKey, was.Key);
            now.Required.Should().Be(was.Required, "{0}.{1} required", seedKey, was.Key);
            now.ScaleKey.Should().Be(was.ScaleKey, "{0}.{1} scale_key", seedKey, was.Key);
            now.CatalogueKey.Should().Be(was.CatalogueKey, "{0}.{1} catalogue", seedKey, was.Key);
            now.Options.Should().Equal(was.Options, "{0}.{1} options", seedKey, was.Key);
            now.EditableBy.Should().Be(was.EditableBy, "{0}.{1} editable_by", seedKey, was.Key);
        }

        foreach (var userField in before.Where(field => field.Type == FieldType.User))
        {
            ActorFieldRules.RequiredRolesForUserField(reparsed, userField.Key)
                .Should().Equal(ActorFieldRules.RequiredRolesForUserField(original, userField.Key),
                    "the picker and the gate must ask for the same roles after an operator save of '{0}'", seedKey);
        }
    }

    [Theory]
    [MemberData(nameof(SeedsWithAUserField))]
    public void ASeedWithAUserField_OpenedAndSavedInTheBuilder_PassesThePublishCheck(string seedKey)
    {
        // T102 added publish-time refusals (duplicate keys, field: rules naming a non-User field, options on a User
        // field). A seed an operator opens and saves unchanged must not trip one of them.
        var schema = FormSchemaParser.Parse(BuilderSchemaModel.Parse(ReadSeed(seedKey, "schema.json")).ToJson());
        var workflow = WorkflowParser.Parse(ReadSeed(seedKey, "workflow.json"));

        var publish = () => ActorFieldRules.EnsurePublishable(schema, workflow);

        publish.Should().NotThrow();
    }

    // ---- default keys ----

    [Fact]
    public void NextFieldKey_SkipsAKeyUsedInAnotherSection()
    {
        // The defect: AddField numbered per section (field_{count+1}), so a second section's first added field was
        // field_1 again. Publish now refuses a key declared twice, which would have made the builder's own default
        // unpublishable.
        var model = Model(("first", ["field_1", "field_2"]), ("second", []));

        var key = model.NextFieldKey();

        AllFieldKeys(model).Should().NotContain(key);
        key.Should().StartWith("field_");
    }

    [Fact]
    public void NextFieldKey_SkipsTheSectionCountToo()
    {
        // The old AddSection seeded the new section's first field as field_{sections+1}, which collides as soon as an
        // earlier section has that many fields.
        var model = Model(("first", ["field_1", "field_2"]));

        model.NextFieldKey().Should().NotBe("field_2").And.NotBe("field_1");
    }

    [Fact]
    public void NextFieldKey_TreatsAKeyWithStraySpacesAsTaken()
    {
        // ToJson trims every key, so " field_1 " is emitted as field_1: proposing field_1 again is still a duplicate.
        var model = Model(("first", [" field_1 "]));

        model.NextFieldKey().Should().NotBe("field_1");
    }

    [Fact]
    public void NextSectionKey_SkipsAKeyAlreadyUsed_AfterADelete()
    {
        // section_{count+1} collided once a section had been deleted: two sections, delete the first, add one, and the
        // new section was section_2 again.
        var model = Model(("section_2", ["a"]));

        var key = model.NextSectionKey();

        key.Should().NotBe("section_2");
        model.Sections.Select(section => section.Key).Should().NotContain(key);
    }

    [Fact]
    public void RepeatedlyAddingSectionsAndFields_NeverProducesADuplicateKey_OrAnUnpublishableDraft()
    {
        // What the Form tab does on "Add section" and "Add field", in an order that broke the old numbering: sections
        // added after fields, fields added to earlier sections after later ones exist, and a deletion in between.
        var model = new BuilderSchemaModel();

        void AddSection()
        {
            var fieldKey = model.NextFieldKey();
            model.Sections.Add(new BuilderSectionModel
            {
                Key = model.NextSectionKey(),
                Title = "Section",
                Fields = [new BuilderFieldModel { Key = fieldKey, Label = "New field" }]
            });
        }

        void AddField(int sectionIndex)
            => model.Sections[sectionIndex].Fields.Add(new BuilderFieldModel { Key = model.NextFieldKey(), Label = "New field" });

        AddSection();
        AddField(0);
        AddSection();
        AddSection();
        AddField(1);
        AddField(0);
        model.Sections.RemoveAt(0);
        AddSection();
        AddField(0);
        AddField(2);

        AllFieldKeys(model).Should().OnlyHaveUniqueItems();
        model.Sections.Select(section => section.Key).Should().OnlyHaveUniqueItems();

        var publish = () => ActorFieldRules.EnsurePublishable(FormSchemaParser.Parse(model.ToJson()), NoFieldRules());
        publish.Should().NotThrow();
    }

    // ---- publish warnings ----

    [Fact]
    public void ChangingARole_IsWarnedAboutAtPublish()
    {
        // A role change changes who submitting accepts from then on. Activities already filed keep their nominee (the
        // gate never re-judges an unchanged value after the author's hand-on), and the operator should hear both.
        var model = BuilderSchemaModel.Parse(CoordinatorField);
        Field(model, "assessor_user_id").NomineeRole = WombatRoles.Coordinator;

        var warnings = BuilderSchemaModel.GetPublishWarnings(CoordinatorField, model.ToJson());

        warnings.Should().ContainSingle(warning => warning.Contains("'Assessor'"))
            .Which.Should().Contain(WombatRoles.Coordinator).And.Contain("already filed");
    }

    [Fact]
    public void ClearingANonDefaultRole_IsWarnedAboutAtPublish()
    {
        var model = BuilderSchemaModel.Parse(CoordinatorField);
        Field(model, "coordinator_user_id").NomineeRole = null;

        var warnings = BuilderSchemaModel.GetPublishWarnings(CoordinatorField, model.ToJson());

        warnings.Should().ContainSingle(warning => warning.Contains("'Coordinator'"))
            .Which.Should().Contain(WombatRoles.Assessor);
    }

    [Fact]
    public void AnUnchangedRole_IsNotWarnedAbout()
    {
        var warnings = BuilderSchemaModel.GetPublishWarnings(
            CoordinatorField,
            BuilderSchemaModel.Parse(CoordinatorField).ToJson());

        warnings.Should().BeEmpty();
    }

    [Fact]
    public void AUserFieldBecomingText_IsWarnedAsATypeChange_NotAsARoleChange()
    {
        // The role warning only makes sense between two User fields; a type change has its own warning, and a second
        // one about "naming an Assessor" for a field that no longer names anyone would be noise.
        var model = BuilderSchemaModel.Parse(CoordinatorField);
        Field(model, "coordinator_user_id").Type = FieldType.Text;

        var warnings = BuilderSchemaModel.GetPublishWarnings(CoordinatorField, model.ToJson());

        warnings.Should().ContainSingle().Which.Should().Contain("changes type");
    }

    [Fact]
    public void DeclaringTheDefaultRoleExplicitly_IsNotARoleChange()
    {
        // Absent means Assessor (the parser's contract, and RequiredRolesForUserField's). A draft that spells the
        // default out changes nothing about who may be named, so it must not tell the operator that it does.
        var draft = CoordinatorField.Replace(
            "\"label\": \"Assessor\", \"required\": true }",
            "\"label\": \"Assessor\", \"required\": true, \"role\": \"Assessor\" }");
        FormSchemaParser.Parse(draft).Sections.Single().Fields.Single(field => field.Key == "assessor_user_id")
            .NomineeRole.Should().Be(WombatRoles.Assessor, "the fixture must actually spell the default out");

        var warnings = BuilderSchemaModel.GetPublishWarnings(CoordinatorField, draft);

        warnings.Should().BeEmpty();
    }

    // ---- helpers ----

    private static BuilderFieldModel Field(BuilderSchemaModel model, string key)
        => model.Sections.SelectMany(section => section.Fields).Single(field => field.Key == key);

    private static Wombat.Domain.Activities.Schema.FormField ParsedField(FormSchema schema, string key)
        => schema.Sections.SelectMany(section => section.Fields).Single(field => field.Key == key);

    private static IReadOnlyList<string> AllFieldKeys(BuilderSchemaModel model)
        => model.Sections.SelectMany(section => section.Fields).Select(field => field.Key.Trim()).ToList();

    private static BuilderSchemaModel Model(params (string SectionKey, string[] FieldKeys)[] sections)
    {
        var model = new BuilderSchemaModel();
        foreach (var (sectionKey, fieldKeys) in sections)
        {
            model.Sections.Add(new BuilderSectionModel
            {
                Key = sectionKey,
                Title = sectionKey,
                Fields = fieldKeys.Select(key => new BuilderFieldModel { Key = key, Label = key }).ToList()
            });
        }

        return model;
    }

    private static Workflow NoFieldRules()
        => WorkflowParser.Parse("""
            {
              "version": 1,
              "initial_state": "draft",
              "states": [ { "key": "draft", "label": "Draft" }, { "key": "done", "label": "Done", "terminal": true } ],
              "transitions": [ { "key": "submit", "from": "draft", "to": "done", "actor": "subject" } ]
            }
            """);

    private static IEnumerable<string> SeedKeysWithAUserField()
        => Directory.EnumerateDirectories(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds"))
            .Select(Path.GetFileName)
            .Select(seedKey => seedKey!)
            .Where(seedKey => File.Exists(SeedPath(seedKey, "schema.json")))
            .Where(seedKey => FormSchemaParser.Parse(ReadSeed(seedKey, "schema.json"))
                .Sections.SelectMany(section => section.Fields)
                .Any(field => field.Type == FieldType.User))
            .Order(StringComparer.Ordinal)
            .ToList();

    private static string ReadSeed(string seedKey, string fileName)
        => File.ReadAllText(SeedPath(seedKey, fileName));

    private static string SeedPath(string seedKey, string fileName)
        => Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", seedKey, fileName);
}
