using System.Globalization;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Activities;

/// <summary>
/// The <c>learner_feedback_cpsa</c> seed: the record a released learner-feedback campaign leaves per covered EPA
/// (T164, D35), and the v11.1 catalogue's answer to which EPAs it may cover.
/// </summary>
/// <remarks>
/// Kept apart from <see cref="CpsaWbaSeedTests" /> for the reason <see cref="MsfSeedTests" /> is: nobody fills it in, so
/// the request, assess and feedback theories there say nothing about it. It is <c>msf_cpsa</c>'s shape, unrated.
/// </remarks>
public sealed class LearnerFeedbackSeedTests
{
    private const string SeedKey = MsfEvidenceKinds.LearnerFeedbackActivityTypeKey;
    private const string SystemActorRule = "role:Coordinator|role:Administrator";
    private const string CurriculumName = "Paediatric EPA Curriculum";
    private const string CatalogueVersion = "11.1";

    private static string ReadSeedFile(string fileName)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", SeedKey, fileName));

    [Fact]
    public void ItIsRegistered_AsASystemWrittenCollegeType_KeyedAsTheInstrumentTheReleaseNarrowsBy()
    {
        // The release writes the type MsfEvidenceKinds names and narrows the covered EPAs by the instrument it names;
        // the tool lists judge the type by its catalogue entry's key. The two must be one instrument.
        var entry = ActivityTypeSeedCatalogue.Entries.Should().ContainSingle(candidate => candidate.Key == SeedKey).Subject;

        entry.Source.Should().Be(ActivityTypeSeedSource.PaediatricCollege);
        entry.Scope.Should().Be(ActivityScope.Speciality);
        entry.DisplayFields.Should().Be(DisplayFieldsRule.None);
        entry.SystemManaged.Should().BeTrue("only a released campaign writes it; nobody is offered it or may file one");
        entry.WbaToolKey.Should().Be(MsfEvidenceKinds.LearnerFeedbackToolKey);
        MsfEvidenceKinds.CoverageToolKeyFor(Wombat.Domain.MultiSourceFeedback.MsfTemplateKind.LearnerFeedback)
            .Should().Be(entry.WbaToolKey);
    }

    [Fact]
    public void TheRecordTransitionIsTheOnlyWayOut_OnlyStaffTakeIt_AndItDeclaresItsValidation()
    {
        var workflow = WorkflowParser.Parse(ReadSeedFile("workflow.json"));

        var transition = workflow.Transitions.Should().ContainSingle().Subject;
        transition.Key.Should().Be("record");
        transition.From.Should().Equal("draft");
        transition.To.Should().Be("recorded");
        ActorRuleParser.Serialize(transition.Actor).Should().Be(SystemActorRule);

        // Every transition declares how much of the form it checks (T105), stated rather than defaulted.
        ReadSeedFile("workflow.json").Should().Contain("\"validation\": \"all\"");

        workflow.States.Where(state => state.Terminal).Select(state => state.Key).Should().Equal("recorded");
        workflow.InitialState.Should().Be("draft");
    }

    [Fact]
    public void EverySectionIsOwnedByStaff()
    {
        var schema = FormSchemaParser.Parse(ReadSeedFile("schema.json"));

        schema.Sections.Should().NotBeEmpty();
        foreach (var section in schema.Sections)
        {
            section.EditableBy.Should().NotBeNull("'{0}' would otherwise default to subject|creator", section.Key);
            ActorRuleParser.Serialize(section.EditableBy!).Should().Be(SystemActorRule);
        }
    }

    [Fact]
    public void ItCreditsNothing_AndRatesNothing_ButNamesTheEpaItIsEvidenceFor()
    {
        // D8 for MSF, and the same for the learners' feedback: evidence, not one of Annexure A's encounters. Unrated,
        // because the learners judge the teaching and nobody states a supervision level from it.
        CreditRulesParser.Parse(ReadSeedFile("credit.json")).CountsFor.Should().BeEmpty();

        var schema = FormSchemaParser.Parse(ReadSeedFile("schema.json"));
        schema.RatedLevelField.Should().BeNull();
        schema.EvidenceEpaField.Should().Be("epa_id");
        schema.ObservationDateField.Should().Be("observed_on");
        schema.Sections.SelectMany(section => section.Fields)
            .Should().NotContain(field => field.Type == FieldType.Scale, "an unrated record carries no rung");
    }

    [Fact]
    public void ItCarriesTheTeachingContextCount_AndNothingThatCouldIdentifyALearner()
    {
        // AccessReportBuilder hands an activity's whole DataJson to its subject, and the portfolio prints it. The count of
        // contexts is the one thing EPA 15 adds; the contexts' names stay on the campaign's report.
        var schema = FormSchemaParser.Parse(ReadSeedFile("schema.json"));
        var fields = schema.Sections.SelectMany(section => section.Fields).ToDictionary(field => field.Key, StringComparer.Ordinal);

        fields.Keys.Should().BeEquivalentTo(
            ["epa_id", "campaign_id", "observed_on", "respondent_count", "teaching_context_count", "summary"],
            "a new field here is a new thing disclosed in a data-subject access report");

        foreach (var key in new[] { "epa_id", "campaign_id", "observed_on", "respondent_count", "teaching_context_count" })
        {
            fields[key].Required.Should().BeTrue("'{0}' is written by every release", key);
        }

        fields["summary"].Required.Should().BeFalse();
    }

    /// <summary>
    /// A fresh boot creates the type as the catalogue declares it, and the v11.1 lists permit learner feedback on
    /// PAED-015 alone: Annexure A names it for EPA 15 only.
    /// </summary>
    [Fact]
    public async Task AFreshBoot_CreatesIt_AndTheCataloguePermitsItOnPaed015Alone()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        await using var dbContext = database.NewContext();
        var type = await dbContext.ActivityTypes.AsNoTracking().SingleAsync(entity => entity.Key == SeedKey);
        type.SystemManaged.Should().BeTrue();
        type.WbaToolKey.Should().Be("learner_feedback");
        type.Version.Should().Be(1);
        type.Name.Should().Be("Learner Feedback (Paediatrics)");

        var items = await LoadCatalogueItemsAsync(dbContext);
        items.Should().HaveCount(15);
        items.Where(item => ToolPermission.Evaluate(CurriculumItem.ParsePermittedTools(item.PermittedToolsJson), type.WbaToolKey)
                            == ToolPermissionVerdict.Permitted)
            .Select(item => item.Code)
            .Should().Equal("PAED-015");
    }

    /// <summary>
    /// The campaign form's EPA picker, on a booted v11.1 catalogue: asked for learner feedback it offers PAED-015 and
    /// nothing else; asked for multi-source feedback it offers the whole curriculum, as it always has. The create command
    /// and the release ask the same question (<c>MsfCampaignRules.CoverableEpaIdsAsync</c>).
    /// </summary>
    [Fact]
    public async Task TheCoverageList_ForLearnerFeedback_IsPaed015_AndForMsf_IsTheWholeCurriculum()
    {
        const string traineeId = "t164-trainee";
        var database = new SeedDatabase();
        await database.BootAsync();
        await database.EditAsync(async dbContext =>
        {
            var curriculumId = await dbContext.Curricula
                .Where(curriculum => curriculum.Name == CurriculumName && curriculum.Version == CatalogueVersion)
                .Select(curriculum => curriculum.Id)
                .SingleAsync();
            dbContext.Set<TraineeProfile>().Add(new TraineeProfile
            {
                UserId = traineeId,
                InstitutionId = 1,
                CurriculumId = curriculumId,
                ProgrammeStartDate = new DateOnly(2026, 1, 12),
                ExpectedCompletionDate = new DateOnly(2030, 1, 11),
                IsActive = true
            });
        });

        await using var read = database.NewContext();
        var service = new ActivityReferenceDataService(read);

        var learnerFeedback = await CodesAsync(read, await service.GetSubjectCurriculumEpaOptionsAsync(
            traineeId, MsfEvidenceKinds.CoverageToolKeyFor(Wombat.Domain.MultiSourceFeedback.MsfTemplateKind.LearnerFeedback)));
        var msf = await CodesAsync(read, await service.GetSubjectCurriculumEpaOptionsAsync(
            traineeId, MsfEvidenceKinds.CoverageToolKeyFor(Wombat.Domain.MultiSourceFeedback.MsfTemplateKind.Msf)));

        learnerFeedback.Should().Equal("PAED-015");
        msf.Should().HaveCount(15);
    }

    private static async Task<IReadOnlyList<string>> CodesAsync(ApplicationDbContext dbContext, IReadOnlyList<ActivityCatalogueOption> options)
    {
        var ids = options.Select(option => int.Parse(option.Value, CultureInfo.InvariantCulture)).ToList();
        return (await dbContext.Epas.AsNoTracking().Where(epa => ids.Contains(epa.Id)).Select(epa => epa.Code).ToListAsync())
            .Order(StringComparer.Ordinal)
            .ToList();
    }

    private static async Task<IReadOnlyList<(string Code, string? PermittedToolsJson)>> LoadCatalogueItemsAsync(ApplicationDbContext dbContext)
        => (await (
                from item in dbContext.CurriculumItems.AsNoTracking()
                join curriculum in dbContext.Curricula on item.CurriculumId equals curriculum.Id
                join epa in dbContext.Epas on item.EpaId equals epa.Id
                where curriculum.Name == CurriculumName
                      && curriculum.Version == CatalogueVersion
                      && item.OwningInstitutionId == null
                      && epa.OwningInstitutionId == null
                select new { epa.Code, item.PermittedToolsJson })
            .ToListAsync())
            .Select(row => (row.Code, (string?)row.PermittedToolsJson))
            .ToList();

    /// <summary>One in-memory database booted the way <c>Program.cs</c> seeds, as the catalogue tool tests boot it.</summary>
    private sealed class SeedDatabase
    {
        private readonly InMemoryDatabaseRoot _root = new();
        private readonly string _name = Guid.NewGuid().ToString();

        public ApplicationDbContext NewContext()
            => new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(_name, _root)
                .Options);

        public async Task BootAsync()
        {
            await using var dbContext = NewContext();
            await new DataSeeder(dbContext).SeedAsync();
            await new PaediatricCatalogueSeeder(dbContext).SeedAsync();
        }

        public async Task EditAsync(Func<ApplicationDbContext, Task> edit)
        {
            await using var dbContext = NewContext();
            await edit(dbContext);
            await dbContext.SaveChangesAsync();
        }
    }
}
