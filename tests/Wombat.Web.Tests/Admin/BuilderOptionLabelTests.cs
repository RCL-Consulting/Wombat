using FluentAssertions;
using Wombat.Domain.Activities.Schema;
using Wombat.Web.Components.Pages.Admin.ActivityTypes;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T191 on the builder side: an option's label, carried through the Form tab's Options box as <c>value | Label</c>.
/// </summary>
/// <remarks>
/// The Form tab is the only schema-authoring path, and <c>ActivityType.SaveDraft</c> stores whatever <c>ToJson</c> emits.
/// A box that listed only the values would erase every label on the first operator save of a seeded type: the form would
/// show keys again, and the refresher would stop reaching the type (its newest version is no longer the seeder's).
/// </remarks>
public sealed class BuilderOptionLabelTests
{
    public static TheoryData<string> SeedsWithLabelledOptions
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var seedKey in SeedKeysWithLabelledOptions())
            {
                data.Add(seedKey);
            }

            return data;
        }
    }

    [Fact]
    public void TheCorpusHasLabelledOptions_SoTheTheoryIsNotVacuous()
    {
        SeedKeysWithLabelledOptions().Should().HaveCountGreaterThanOrEqualTo(15)
            .And.Contain(["cca_cpsa", "procedure_log", "teaching_session"]);
    }

    [Theory]
    [MemberData(nameof(SeedsWithLabelledOptions))]
    public void ASeedWithLabelledOptions_KeepsEveryOptionAndLabel_ThroughTheBuilder(string seedKey)
    {
        var original = FormSchemaParser.Parse(ReadSeed(seedKey));
        var reparsed = FormSchemaParser.Parse(BuilderSchemaModel.Parse(ReadSeed(seedKey)).ToJson());

        var before = original.Sections.SelectMany(section => section.Fields).ToList();
        var after = reparsed.Sections.SelectMany(section => section.Fields).ToList();
        after.Select(field => field.Key).Should().Equal(before.Select(field => field.Key));

        foreach (var (was, now) in before.Zip(after))
        {
            now.Options.Should().Equal(was.Options, "{0}.{1}: the builder must keep every value and every label", seedKey, was.Key);
        }
    }

    [Fact]
    public void TheOptionsBox_ShowsALabelledOptionAsValueBarLabel_AndABareOneAsItself()
    {
        var model = BuilderSchemaModel.Parse(Schema("""[ { "value": "picu", "label": "PICU" }, "ward" ]"""));

        model.Sections.Single().Fields.Single().OptionsText
            .Should().Be($"picu | PICU{Environment.NewLine}ward");
    }

    [Fact]
    public void ALineWithABar_IsOneLabelledOption_AndALabelMayHoldACommaOrABar()
    {
        var options = BuilderSchemaModel.ParseOptions(
            "admission_notes | Admission notes\n  notes |  Notes, letters and charts  \nother | Other | not listed");

        options.Should().Equal(
            new FieldOption("admission_notes", "Admission notes"),
            new FieldOption("notes", "Notes, letters and charts"),
            new FieldOption("other", "Other | not listed"));
    }

    [Fact]
    public void ALineWithNoBar_IsOneBareOption_CommaAndAll()
    {
        // One option per line, as the help text says. A comma is text: splitting on it would split a bare value that
        // holds one ("Notes, letters") into two on every open-and-save.
        var options = BuilderSchemaModel.ParseOptions("Notes, letters\r\nhigh");

        options.Should().Equal(FieldOption.Unlabelled("Notes, letters"), FieldOption.Unlabelled("high"));
    }

    public static TheoryData<string> OptionsTheParserAccepts => new()
    {
        """ "Notes, letters", "high care" """,
        """{ "value": "a,b", "label": "A, or B" }""",
        """{ "value": "other", "label": "Other | not listed" }""",
        """{ "value": "picu", "label": "| PICU |" }""",
        """{ "value": "ward", "label": "ward" }, "Ward" """,
        """{ "value": "arztin", "label": "Ärztin" }, "1", "2" """,
    };

    [Theory]
    [MemberData(nameof(OptionsTheParserAccepts))]
    public void EveryOptionTheParserAccepts_ComesBackFromTheBoxExactly(string options)
    {
        // The box is how an operator edits a schema, so opening one and saving it must not change a value or a label.
        // The parser refuses what a line cannot carry (a bar in a value, a line break anywhere); the rest must survive.
        var declared = FormSchemaParser.Parse(Schema($"[ {options} ]")).Sections.Single().Fields.Single().Options;

        BuilderSchemaModel.ParseOptions(BuilderSchemaModel.FormatOptions(declared)).Should().Equal(declared);

        var model = BuilderSchemaModel.Parse(Schema($"[ {options} ]"));
        FormSchemaParser.Parse(model.ToJson()).Sections.Single().Fields.Single().Options.Should().Equal(declared);
    }

    [Fact]
    public void ABlankLabel_LeavesTheValueUnlabelled_AndABlankValueIsNoOption()
    {
        var options = BuilderSchemaModel.ParseOptions("ward |\n | Orphan label");

        options.Should().Equal(FieldOption.Unlabelled("ward"));
    }

    [Fact]
    public void AValueGivenTwice_IsPassedOn_AndRefusedByName_NotDropped()
    {
        // The value is what an activity stores, so two options cannot share one. The box does not choose between the
        // lines and say nothing: the parser refuses the schema by name, which the live preview and Save both show.
        var options = BuilderSchemaModel.ParseOptions("ward | Ward\nward | General ward\nclinic");

        options.Should().Equal(
            new FieldOption("ward", "Ward"), new FieldOption("ward", "General ward"), FieldOption.Unlabelled("clinic"));

        var model = BuilderSchemaModel.Parse(Schema("""[ "ward" ]"""));
        model.Sections.Single().Fields.Single().OptionsText = "ward | Ward\nward | General ward\nclinic";

        var refusal = FluentActions.Invoking(() => FormSchemaParser.Parse(model.ToJson()))
            .Should().Throw<SchemaParseException>().Which;
        refusal.Message.Should().Contain("'setting'").And.Contain("'ward' twice");
    }

    [Fact]
    public void AnOperatorsLabelledOptions_ArePublishedAsLabels()
    {
        var model = BuilderSchemaModel.Parse(Schema("""[ "ward" ]"""));
        model.Sections.Single().Fields.Single().OptionsText = "ward | Ward\npicu | PICU";

        var published = FormSchemaParser.Parse(model.ToJson()).Sections.Single().Fields.Single();

        published.Options.Should().Equal(new FieldOption("ward", "Ward"), new FieldOption("picu", "PICU"));
    }

    private static string Schema(string options) => $$"""
        {
          "version": 1,
          "sections": [
            { "key": "s", "title": "S", "fields": [ { "key": "setting", "type": "choice", "label": "Setting", "options": {{options}} } ] }
          ]
        }
        """;

    private static IEnumerable<string> SeedKeysWithLabelledOptions()
        => Directory.EnumerateDirectories(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds"))
            .Select(directory => Path.GetFileName(directory)!)
            .Where(seedKey => File.Exists(SeedPath(seedKey)))
            .Where(seedKey => FormSchemaParser.Parse(ReadSeed(seedKey))
                .Sections.SelectMany(section => section.Fields)
                .Any(field => field.Options.Any(option => option.IsLabelled)))
            .Order(StringComparer.Ordinal)
            .ToList();

    private static string ReadSeed(string seedKey) => File.ReadAllText(SeedPath(seedKey));

    private static string SeedPath(string seedKey)
        => Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", seedKey, "schema.json");
}
