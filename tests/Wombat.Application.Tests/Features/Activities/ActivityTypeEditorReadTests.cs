using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Commands.SaveActivityTypeDraft;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.Activities;

/// <summary>
/// The builder's read of a type (T398, flow 17, lane A; build-contracts § 1): null for an unknown or unreadable id; whose
/// it is; the draft's saver and each version's publisher by name, "Wombat's catalogue" for the seeder, in one lookup;
/// seed ownership; the preview's EPAs by the type's own scope; the workflow and the credit rules in words; the draft's
/// JSON laid out. The filing page's read (<c>ForBuilder</c> false) keeps the compact text and fills none of it.
/// </summary>
public sealed class ActivityTypeEditorReadTests : IAsyncLifetime
{
    private const int Kgk = 1;
    private const int OtherInstitution = 2;
    private const int Cpsa = 10;
    private const int DemoCollege = 11;
    private const int Paediatrics = 20;
    private const int DemoSpeciality = 21;
    private const int PaediatricsSub = 30;
    private const int DemoSub = 31;
    private const int CpsaCurriculum = 40;
    private const int DemoCurriculum = 41;

    private const int MiniCexId = 100;
    private const int KgkLogId = 101;
    private const int GlobalId = 102;
    private const int MsfId = 103;

    private const string Nolwazi = "nolwazi-user";
    private const string Kruger = "kruger-user";

    private ApplicationDbContext _db = null!;
    private FakeUserDirectory _users = null!;

    public async Task InitializeAsync()
    {
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        _users = new FakeUserDirectory((Nolwazi, "Nolwazi Mbatha"), (Kruger, "Anton Kruger"));

        _db.Institutions.AddRange(
            new Institution { Id = Kgk, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = OtherInstitution, Name = "Baragwanath", ShortCode = "CHBAH", IsActive = true, CreatedOn = DateTime.UtcNow });
        _db.Colleges.AddRange(
            new College { Id = Cpsa, Name = "College of Paediatricians of South Africa", ShortCode = "CPSA" },
            new College { Id = DemoCollege, Name = "Demo College", ShortCode = "DEMO-C" });
        _db.Specialities.AddRange(
            new Speciality { Id = Paediatrics, CollegeId = Cpsa, Name = "Paediatrics" },
            new Speciality { Id = DemoSpeciality, CollegeId = DemoCollege, Name = "Internal Medicine" });
        _db.SubSpecialities.AddRange(
            new SubSpeciality { Id = PaediatricsSub, SpecialityId = Paediatrics, Name = "General Paediatrics" },
            new SubSpeciality { Id = DemoSub, SpecialityId = DemoSpeciality, Name = "General Medicine" });

        _db.Epas.AddRange(
            new Epa { Id = 1, SubSpecialityId = PaediatricsSub, Code = "PAED-002", Title = "Managing common paediatric presentations" },
            new Epa { Id = 2, SubSpecialityId = PaediatricsSub, Code = "PAED-001", Title = "Providing paediatric emergency care to children" },
            new Epa { Id = 3, SubSpecialityId = PaediatricsSub, Code = "PAED-099", Title = "Retired", IsActive = false },
            new Epa { Id = 4, SubSpecialityId = DemoSub, Code = "EPA-001", Title = "Demo EPA" },
            new Epa { Id = 5, SubSpecialityId = PaediatricsSub, OwningInstitutionId = Kgk, Code = "KGK-001", Title = "KGK's own" },
            new Epa { Id = 6, SubSpecialityId = PaediatricsSub, OwningInstitutionId = OtherInstitution, Code = "BARA-001", Title = "Another's own" });

        _db.Curricula.AddRange(
            new Curriculum { Id = CpsaCurriculum, SubSpecialityId = PaediatricsSub, Name = "CPSA Paediatrics", Version = "11.1" },
            new Curriculum { Id = DemoCurriculum, SubSpecialityId = DemoSub, Name = "IM Core", Version = "1" });
        _db.CurriculumItems.AddRange(
            new CurriculumItem { Id = 1, CurriculumId = CpsaCurriculum, EpaId = 2 },
            new CurriculumItem { Id = 2, CurriculumId = CpsaCurriculum, EpaId = 3 },
            new CurriculumItem { Id = 3, CurriculumId = CpsaCurriculum, EpaId = 5, OwningInstitutionId = Kgk },
            new CurriculumItem { Id = 4, CurriculumId = CpsaCurriculum, EpaId = 6, OwningInstitutionId = OtherInstitution },
            new CurriculumItem { Id = 5, CurriculumId = DemoCurriculum, EpaId = 4 });
        AdoptionSeed.Adopt(_db, 1, Kgk, CpsaCurriculum, PaediatricsSub);

        _db.ActivityTypes.Add(SeededType(MiniCexId, "mini_cex_cpsa", "Mini-CEX (Paediatrics)", ActivityScope.Speciality, Paediatrics));
        var msf = SeededType(MsfId, "msf_cpsa", "Multi-Source Feedback (Paediatrics)", ActivityScope.Speciality, Paediatrics);
        msf.SystemManaged = true;
        _db.ActivityTypes.Add(msf);

        var kgkLog = new ActivityType
        {
            Id = KgkLogId,
            Key = "kgk_teaching_log",
            Name = "KGK Teaching Session Log",
            Scope = ActivityScope.Institution,
            ScopeId = Kgk,
            OwnerUserId = Nolwazi,
            CreatedOn = DateTime.UtcNow
        };
        kgkLog.SaveDraft(Seed("teaching_session", "schema.json"), Seed("teaching_session", "workflow.json"), Seed("teaching_session", "credit.json"), "[]", Nolwazi);
        kgkLog.PublishDraft(Nolwazi);
        kgkLog.SaveDraft(Seed("teaching_session", "schema.json"), Seed("teaching_session", "workflow.json"), Seed("teaching_session", "credit.json"), "[]", Kruger);
        _db.ActivityTypes.Add(kgkLog);

        _db.ActivityTypes.Add(new ActivityType
        {
            Id = GlobalId,
            Key = "global_probe",
            Name = "Global probe",
            Scope = ActivityScope.Global,
            OwnerUserId = "admin",
            CreatedOn = DateTime.UtcNow,
            Version = 1,
            SchemaJson = StartSchemaJson,
            WorkflowJson = StartWorkflowJson,
            CreditRulesJson = StartCreditRulesJson
        });

        await _db.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        _db.Dispose();
        return Task.CompletedTask;
    }

    // ---- null for unknown or unreadable (N3) -------------------------------------------------------

    [Theory]
    [InlineData(999)]
    [InlineData(0)]
    public async Task AnUnknownId_IsNull_NotAThrow(int activityTypeId)
        => (await ReadAsync(activityTypeId, TestPrincipals.Administrator())).Should().BeNull();

    [Fact]
    public async Task ADraftTheCallerMayNotRead_IsNull_AsAnUnknownOneIs()
    {
        var unpublished = new ActivityType
        {
            Id = 200, Key = "other_draft", Name = "Another's draft", Scope = ActivityScope.Institution, ScopeId = OtherInstitution,
            OwnerUserId = "someone", CreatedOn = DateTime.UtcNow
        };
        _db.ActivityTypes.Add(unpublished);
        await _db.SaveChangesAsync();

        (await ReadAsync(200, TestPrincipals.InstitutionalAdmin(Kgk))).Should().BeNull();
        (await ReadAsync(200, TestPrincipals.InstitutionalAdmin(Kgk), forBuilder: false)).Should().BeNull();
    }

    // ---- whose it is, and the names --------------------------------------------------------------

    [Fact]
    public async Task TheOwner_IsTheCollegeByItsShortCode_TheInstitutionByName_OrThePlatform()
    {
        var admin = TestPrincipals.Administrator();

        (await ReadAsync(MiniCexId, admin))!.Owner.Should().Be(new ActivityTypeOwnerDto(ActivityTypeOwnerKind.College, "CPSA"));
        (await ReadAsync(KgkLogId, admin))!.Owner.Should().Be(new ActivityTypeOwnerDto(ActivityTypeOwnerKind.Institution, "Kgosi Kgari Teaching Hospital"));
        (await ReadAsync(GlobalId, admin))!.Owner.Should().Be(new ActivityTypeOwnerDto(ActivityTypeOwnerKind.Platform, null));
    }

    [Fact]
    public async Task ANewType_IsOwnedWhereItStarts()
    {
        var editor = (await ReadAsync(null, TestPrincipals.InstitutionalAdmin(Kgk)))!;

        editor.Owner.Should().Be(new ActivityTypeOwnerDto(ActivityTypeOwnerKind.Institution, "Kgosi Kgari Teaching Hospital"));
        editor.IsSeedOwned.Should().BeFalse();
        editor.SystemManaged.Should().BeFalse();
    }

    /// <summary>
    /// The fix pass (pages review N2): a speciality or sub-speciality the caller may choose carries its College's short
    /// code, so the owner line names the College of a scope chosen on the page; an institution carries none.
    /// </summary>
    [Fact]
    public async Task EachCollegeTargetOffered_CarriesItsCollegesShortCode()
    {
        var scopes = (await ReadAsync(null, TestPrincipals.Administrator()))!.WritableScopes;

        scopes.Single(choice => choice.Scope == ActivityScope.Speciality).Targets
            .Select(target => (target.Name, target.OwningCollegeShortCode))
            .Should().Equal(("Internal Medicine", "DEMO-C"), ("Paediatrics", "CPSA"));
        scopes.Single(choice => choice.Scope == ActivityScope.SubSpeciality).Targets
            .Select(target => target.OwningCollegeShortCode).Should().BeEquivalentTo(["DEMO-C", "CPSA"]);
        scopes.Single(choice => choice.Scope == ActivityScope.Institution).Targets
            .Should().OnlyContain(target => target.OwningCollegeShortCode == null);
    }

    /// <summary>Q3, T16: the strip names people, read in one lookup; the seeder is "Wombat's catalogue", never an id.</summary>
    [Fact]
    public async Task TheSaverAndEachPublisher_AreNamed_InOneLookup()
    {
        var kgk = (await ReadAsync(KgkLogId, TestPrincipals.InstitutionalAdmin(Kgk)))!;

        kgk.DraftSavedByName.Should().Be("Anton Kruger");
        kgk.Versions.Should().ContainSingle().Which.PublishedByName.Should().Be("Nolwazi Mbatha");
        _users.Lookups.Should().ContainSingle().Which.Should().BeEquivalentTo([Nolwazi, Kruger]);

        var miniCex = (await ReadAsync(MiniCexId, TestPrincipals.InstitutionalAdmin(Kgk)))!;
        miniCex.Versions.Should().ContainSingle().Which.PublishedByName.Should().Be("Wombat's catalogue");
        miniCex.DraftSavedByName.Should().BeNull("there is no draft");
        _users.Lookups.Should().HaveCount(1, "a type only the seeder published asks the directory nothing");
    }

    [Fact]
    public async Task SeedOwnership_IsTheOnePredicate()
    {
        (await ReadAsync(MiniCexId, TestPrincipals.Administrator()))!.IsSeedOwned.Should().BeTrue();
        (await ReadAsync(KgkLogId, TestPrincipals.Administrator()))!.IsSeedOwned.Should().BeFalse();
    }

    [Fact]
    public async Task ASystemManagedType_SaysWhichReleaseWritesIt()
    {
        var msf = (await ReadAsync(MsfId, TestPrincipals.Administrator()))!;

        msf.SystemManaged.Should().BeTrue();
        msf.WrittenBy.Should().Be("the MSF release");
        msf.CanWrite.Should().BeFalse();
        (await ReadAsync(MiniCexId, TestPrincipals.Administrator()))!.WrittenBy.Should().BeNull();
    }

    // ---- the preview's EPAs (S8) -----------------------------------------------------------------

    /// <summary>
    /// A College type previews its discipline's active national EPAs, by code, labelled as the filing page labels them;
    /// never the Demo College's EPA-001, whoever opens it, and never an institution's own.
    /// </summary>
    [Fact]
    public async Task ASpecialityType_PreviewsItsDisciplinesActiveNationalEpas()
    {
        var editor = (await ReadAsync(MiniCexId, TestPrincipals.Administrator()))!;

        editor.PreviewEpaOptions!.Select(option => option.Label).Should().Equal(
            "PAED-001 — Providing paediatric emergency care to children",
            "PAED-002 — Managing common paediatric presentations");
        editor.PreviewEpaOptions!.Select(option => option.Value).Should().Equal("2", "1");
    }

    /// <summary>An institution's type previews the EPAs on the curricula it adopted: national, and its own, in force.</summary>
    [Fact]
    public async Task AnInstitutionType_PreviewsTheEpasOfItsAdoptedCurricula_NationalAndItsOwn()
    {
        var editor = (await ReadAsync(KgkLogId, TestPrincipals.InstitutionalAdmin(Kgk)))!;

        editor.PreviewEpaOptions!.Select(option => option.Label).Should().Equal(
            "KGK-001 — KGK's own",
            "PAED-001 — Providing paediatric emergency care to children");
    }

    /// <summary>
    /// The fix pass (code review M-1): an institution's admin opening another institution's active published type (which
    /// MayRead admits anyone to) previews the EPAs by her own scope, never the other institution's own EPAs: the curricula
    /// her own institution adopted, national and hers. With none adopted, none.
    /// </summary>
    [Fact]
    public async Task AnotherInstitutionsAdmin_ReadingItsType_PreviewsByHerOwnScope_NeverItsOwnEpas()
    {
        var outsider = TestPrincipals.InstitutionalAdmin(OtherInstitution);

        (await ReadAsync(KgkLogId, outsider))!.PreviewEpaOptions.Should().BeEmpty("Baragwanath has adopted nothing");

        AdoptionSeed.Adopt(_db, 2, OtherInstitution, CpsaCurriculum, PaediatricsSub);
        await _db.SaveChangesAsync();

        var editor = (await ReadAsync(KgkLogId, outsider))!;
        editor.CanWrite.Should().BeFalse("guard: she reads, she does not write");
        editor.PreviewEpaOptions!.Select(option => option.Label).Should().Equal(
            "BARA-001 — Another's own",
            "PAED-001 — Providing paediatric emergency care to children");
        editor.PreviewEpaOptions!.Should().NotContain(option => option.Label.StartsWith("KGK-001", StringComparison.Ordinal));
    }

    /// <summary>
    /// The fix pass (code review S-2): a reader the type's scope does not admit is shown no person and no part of the
    /// draft: no saver, no publisher by name, no draft display fields; the days stay, and the seeder is still "Wombat's
    /// catalogue". The writer reads them all.
    /// </summary>
    [Fact]
    public async Task AReaderOutsideTheScope_IsShownNoNameAndNoPartOfTheDraft()
    {
        var type = await _db.ActivityTypes.SingleAsync(entity => entity.Id == KgkLogId);
        type.StagingDisplayFieldsJson = """["title"]""";
        await _db.SaveChangesAsync();

        var reader = (await ReadAsync(KgkLogId, TestPrincipals.InstitutionalAdmin(OtherInstitution)))!;
        reader.HasDraft.Should().BeTrue("guard: there is a draft to withhold");
        reader.DraftSavedByName.Should().BeNull();
        reader.StagingUpdatedByUserId.Should().BeNull();
        reader.CanDiscardDraft.Should().BeFalse();
        reader.Versions.Should().ContainSingle().Which.PublishedByName.Should().BeNull();
        reader.DraftDisplayFieldsJson.Should().Be(type.DisplayFieldsJson);
        _users.Lookups.Should().BeEmpty("no person is looked up for her");

        var writer = (await ReadAsync(KgkLogId, TestPrincipals.InstitutionalAdmin(Kgk)))!;
        writer.DraftSavedByName.Should().Be("Anton Kruger");
        writer.Versions.Single().PublishedByName.Should().Be("Nolwazi Mbatha");
        writer.DraftDisplayFieldsJson.Should().Be("""["title"]""");
        writer.CanDiscardDraft.Should().BeTrue();

        (await ReadAsync(MiniCexId, TestPrincipals.InstitutionalAdmin(OtherInstitution)))!
            .Versions.Single().PublishedByName.Should().Be("Wombat's catalogue", "the seeder is no person");
    }

    [Fact]
    public async Task AGlobalType_PreviewsNoEpa()
        => (await ReadAsync(GlobalId, TestPrincipals.Administrator()))!.PreviewEpaOptions.Should().BeEmpty();

    // ---- the read in words (Q2) -----------------------------------------------------------------

    /// <summary>R2-B04's table, word for word, from Mini-CEX (Paediatrics)'s seed.</summary>
    [Fact]
    public async Task MiniCex_ReadsInWords_AsR2B04()
    {
        var read = (await ReadAsync(MiniCexId, TestPrincipals.InstitutionalAdmin(Kgk)))!.WorkflowRead!;

        read.States.Select(state => (state.Label, state.IsInitial, state.IsTerminal)).Should().Equal(
            ("Draft", true, false),
            ("Requested", false, false),
            ("Completed", false, true),
            ("Declined", false, false),
            ("Cancelled", false, false));

        read.Moves.Select(move => (move.Label, string.Join(" or ", move.FromLabels), move.ToLabel, move.Who, move.Checks, move.RequiresNote))
            .Should().Equal(
                ("Submit", "Draft", "Requested", "The registrar, or whoever filed it", "The mover's own fields", false),
                ("Complete", "Requested", "Completed", "The person Assessor names", "The whole form", false),
                ("Decline", "Requested", "Declined", "The person Assessor names", "Formats only", true),
                ("Cancel", "Draft or Requested", "Cancelled", "The registrar, or whoever filed it", "Formats only", false));
        read.Moves.Should().OnlyContain(move => move.WhoAsWritten == null);
    }

    /// <summary>R2-B05's sentence, from the same seed.</summary>
    [Fact]
    public async Task MiniCex_CreditReadsInWords_AsR2B05()
    {
        var read = (await ReadAsync(MiniCexId, TestPrincipals.InstitutionalAdmin(Kgk)))!.CreditRead!;

        read.CreditsNothing.Should().BeFalse();
        read.Sentences.Should().Equal(
            "Counts 1 towards the curriculum item for the EPA in EPA, at the level Supervision required for this encounter records.");
    }

    [Fact]
    public async Task TheTeachingLog_ReadsInWords_AndCreditsNothing()
    {
        var editor = (await ReadAsync(KgkLogId, TestPrincipals.InstitutionalAdmin(Kgk)))!;

        editor.WorkflowRead!.Moves.Select(move => (move.Label, move.Who)).Should().Equal(
            ("Submit", "The registrar"),
            ("Accept", "A Speciality admin of the activity's speciality"));
        editor.CreditRead!.CreditsNothing.Should().BeTrue();
        editor.CreditRead.Sentences.Should().BeEmpty();
    }

    /// <summary>R3-D05: a new type's Workflow tab reads its starting draft.</summary>
    [Fact]
    public async Task ANewType_ReadsItsStartingDraft()
    {
        var editor = (await ReadAsync(null, TestPrincipals.Administrator()))!;

        editor.WorkflowRead!.Moves.Should().ContainSingle().Which.Should().BeEquivalentTo(
            new { Label = "Submit", ToLabel = "Submitted", Who = "The registrar", Checks = "The whole form", RequiresNote = false });
        editor.CreditRead!.CreditsNothing.Should().BeTrue();
    }

    /// <summary>
    /// A writer reads the saved draft, as the page edits it; a reader reads the version registrars file, not a draft
    /// they cannot see.
    /// </summary>
    [Fact]
    public async Task AWriterReadsTheDraft_AReaderThePublishedVersion()
    {
        var type = await _db.ActivityTypes.SingleAsync(entity => entity.Id == MiniCexId);
        type.StagingSchemaJson = type.SchemaJson;
        type.StagingCreditRulesJson = type.CreditRulesJson;
        type.StagingWorkflowJson = type.WorkflowJson!.Replace("\"subject|creator\"", "\"subject\"", StringComparison.Ordinal);
        type.StagingUpdatedByUserId = Kruger;
        type.StagingUpdatedOn = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var writer = (await ReadAsync(MiniCexId, TestPrincipals.Administrator()))!;
        var reader = (await ReadAsync(MiniCexId, TestPrincipals.InstitutionalAdmin(Kgk)))!;

        writer.WorkflowRead!.Moves[0].Who.Should().Be("The registrar");
        reader.WorkflowRead!.Moves[0].Who.Should().Be("The registrar, or whoever filed it");

        // The JSON the page shows follows the read: the reader is never shown the writers' draft.
        writer.DraftWorkflowJson.Should().NotContain("subject|creator");
        reader.DraftWorkflowJson.Should().Contain("subject|creator");
    }

    // ---- the JSON laid out (Q13, S11) -----------------------------------------------------------

    [Fact]
    public async Task TheBuildersJson_IsLaidOut_AndParsesToTheStoredDraft()
    {
        var editor = (await ReadAsync(KgkLogId, TestPrincipals.InstitutionalAdmin(Kgk)))!;
        var stored = await _db.ActivityTypes.AsNoTracking().SingleAsync(entity => entity.Id == KgkLogId);

        editor.DraftWorkflowJson.Should().StartWith("{\n  \"version\": 1,\n  \"initial_state\": ");
        editor.DraftCreditRulesJson.Should().Be("{\n  \"counts_for\": []\n}");
        editor.DraftSchemaJson.Should().Contain("\n    {\n      \"key\": ");

        // Canonical to canonical, never raw text (CLAUDE.md): the laid-out text is what is stored, laid out.
        WorkflowParser.Serialize(WorkflowParser.Parse(editor.DraftWorkflowJson)).Should().Be(stored.StagingWorkflowJson);
        FormSchemaParser.Serialize(FormSchemaParser.Parse(editor.DraftSchemaJson)).Should().Be(stored.StagingSchemaJson);
        CreditRulesParser.Serialize(CreditRulesParser.Parse(editor.DraftCreditRulesJson)).Should().Be(stored.StagingCreditRulesJson);
    }

    /// <summary>The filing page reads the compact text and nothing of the builder's (ForBuilder false).</summary>
    [Fact]
    public async Task TheFilingPagesRead_IsCompact_AndFillsNoneOfTheBuildersMembers()
    {
        var editor = (await ReadAsync(MiniCexId, TestPrincipals.Trainee("trainee-user", Kgk), forBuilder: false))!;

        editor.DraftWorkflowJson.Should().NotContain("\n");
        editor.Owner.Should().BeNull();
        editor.WorkflowRead.Should().BeNull();
        editor.CreditRead.Should().BeNull();
        editor.PreviewEpaOptions.Should().BeNull();
        editor.Versions.Should().OnlyContain(version => version.PublishedByName == null);
        _users.Lookups.Should().BeEmpty();
    }

    /// <summary>A command's returned editor is the builder's too: laid out, named, read in words.</summary>
    [Fact]
    public async Task ASavesReturnedEditor_IsTheBuildersRead()
    {
        var principal = TestPrincipals.InstitutionalAdmin(Kgk);
        var editor = await new SaveActivityTypeDraftCommandHandler(_db, _users).Handle(
            new SaveActivityTypeDraftCommand(
                KgkLogId, "kgk_teaching_log", "KGK Teaching Session Log", null, ActivityScope.Institution, Kgk, true, null,
                Seed("teaching_session", "schema.json"), Seed("teaching_session", "workflow.json"), Seed("teaching_session", "credit.json"),
                "[]", Nolwazi, principal),
            CancellationToken.None);

        editor.DraftSavedByName.Should().Be("Nolwazi Mbatha");
        editor.DraftWorkflowJson.Should().Contain("\n  ");
        editor.WorkflowRead.Should().NotBeNull();
        editor.Owner!.Kind.Should().Be(ActivityTypeOwnerKind.Institution);
    }

    private Task<ActivityTypeEditorDto?> ReadAsync(int? activityTypeId, System.Security.Claims.ClaimsPrincipal principal, bool forBuilder = true)
        => new GetActivityTypeEditorQueryHandler(_db, _users).Handle(
            new GetActivityTypeEditorQuery(activityTypeId, principal, forBuilder), CancellationToken.None);

    private static string Seed(string key, string file) => File.ReadAllText(Path.Combine(ShippedSeeds.Folder, key, file));

    private static ActivityType SeededType(int id, string key, string name, ActivityScope scope, int scopeId)
    {
        var type = new ActivityType
        {
            Id = id,
            Key = key,
            Name = name,
            Scope = scope,
            ScopeId = scopeId,
            OwnerUserId = ActivityTypeSeedOwnership.SeederUserId,
            CreatedOn = DateTime.UtcNow
        };
        type.SaveDraft(Seed(key, "schema.json"), Seed(key, "workflow.json"), Seed(key, "credit.json"), "[]", ActivityTypeSeedOwnership.SeederUserId);
        type.PublishDraft(ActivityTypeSeedOwnership.SeederUserId);
        return type;
    }

    private const string StartSchemaJson = """{ "version": 1, "sections": [ { "key": "details", "title": "Details", "fields": [ { "key": "title", "type": "text", "label": "Title", "required": true } ] } ] }""";

    private const string StartWorkflowJson = """{ "version": 1, "initial_state": "draft", "states": [ { "key": "draft", "label": "Draft" }, { "key": "submitted", "label": "Submitted", "terminal": true } ], "transitions": [ { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject", "validation": "all" } ] }""";

    private const string StartCreditRulesJson = """{ "counts_for": [] }""";
}
