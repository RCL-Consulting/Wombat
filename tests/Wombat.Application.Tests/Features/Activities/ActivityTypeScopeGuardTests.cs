using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities;
using Wombat.Application.Features.Activities.Commands.DiscardActivityTypeDraft;
using Wombat.Application.Features.Activities.Commands.SaveActivityTypeDraft;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Application.Features.Activities.Queries.ListActivityTypesAdmin;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Activities;

/// <summary>
/// Who may write an activity type, and whether the builder's pages say the same as its commands (T056.c, T091, T300).
/// </summary>
/// <remarks>
/// <para>
/// The table (<see cref="EveryCell" />) is the rule: an Administrator writes every type; a Global type is theirs alone; an
/// Institution type is its InstitutionalAdmin's; a Speciality or SubSpeciality type is the owning College's CollegeAdmin's.
/// In every cell the guard's verdict, <c>ActivityTypeAdminScope.MayWriteAsync</c>, the editor's <c>CanWrite</c> and the
/// list's <c>CanWrite</c> agree. Before T300 the pages offered Save and Publish to every caller, and every scope to a new
/// type, and the commands refused all but the guard's.
/// </para>
/// <para>
/// Every type in the fixture is published and active, so every caller can read each one and the editor answers in every
/// cell: it is the write flag that is judged, not the read.
/// </para>
/// </remarks>
public sealed class ActivityTypeScopeGuardTests : IAsyncLifetime
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;
    private const int Cpsa = 10;
    private const int OtherCollege = 11;
    private const int CpsaSpeciality = 20;
    private const int OtherSpeciality = 21;
    private const int CpsaSubSpeciality = 30;
    private const int OtherSubSpeciality = 31;

    private const string Administrator = "an Administrator";
    private const string InstitutionalAdminOfA = "an InstitutionalAdmin of A";
    private const string CollegeAdminOfCpsa = "a CollegeAdmin of CPSA";

    private const string Global = "Global";
    private const string InstitutionAType = "Institution A";
    private const string InstitutionBType = "Institution B";
    private const string CpsaSpecialityType = "Speciality of CPSA";
    private const string OtherSpecialityType = "Speciality of another College";
    private const string CpsaSubSpecialityType = "SubSpeciality of CPSA";
    private const string OtherSubSpecialityType = "SubSpeciality of another College";

    private static readonly IReadOnlyDictionary<string, (ActivityScope Scope, int? ScopeId)> Targets =
        new Dictionary<string, (ActivityScope, int?)>
        {
            [Global] = (ActivityScope.Global, null),
            [InstitutionAType] = (ActivityScope.Institution, InstitutionA),
            [InstitutionBType] = (ActivityScope.Institution, InstitutionB),
            [CpsaSpecialityType] = (ActivityScope.Speciality, CpsaSpeciality),
            [OtherSpecialityType] = (ActivityScope.Speciality, OtherSpeciality),
            [CpsaSubSpecialityType] = (ActivityScope.SubSpeciality, CpsaSubSpeciality),
            [OtherSubSpecialityType] = (ActivityScope.SubSpeciality, OtherSubSpeciality)
        };

    private ApplicationDbContext _db = null!;
    private readonly Dictionary<string, int> _typeIds = [];

    public async Task InitializeAsync()
    {
        _db = NewDb();

        _db.Institutions.AddRange(
            new Institution { Id = InstitutionA, Name = "Kalafong", ShortCode = "KGK", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = InstitutionB, Name = "Baragwanath", ShortCode = "CHBAH", IsActive = true, CreatedOn = DateTime.UtcNow });
        _db.Colleges.AddRange(
            new College { Id = Cpsa, Name = "College of Paediatricians", ShortCode = "CPSA" },
            new College { Id = OtherCollege, Name = "College of Physicians", ShortCode = "CP" });
        _db.Specialities.AddRange(
            new Speciality { Id = CpsaSpeciality, CollegeId = Cpsa, Name = "Paediatrics" },
            new Speciality { Id = OtherSpeciality, CollegeId = OtherCollege, Name = "Internal Medicine" });
        _db.SubSpecialities.AddRange(
            new SubSpeciality { Id = CpsaSubSpeciality, SpecialityId = CpsaSpeciality, Name = "Neonatology" },
            new SubSpeciality { Id = OtherSubSpeciality, SpecialityId = OtherSpeciality, Name = "Cardiology" });

        foreach (var (name, (scope, scopeId)) in Targets)
        {
            _db.ActivityTypes.Add(PublishedType(KeyOf(name), name, scope, scopeId));
        }

        await _db.SaveChangesAsync();

        foreach (var name in Targets.Keys)
        {
            _typeIds[name] = (await _db.ActivityTypes.SingleAsync(type => type.Key == KeyOf(name))).Id;
        }
    }

    public Task DisposeAsync()
    {
        _db.Dispose();
        return Task.CompletedTask;
    }

    /// <summary>Every caller the task names, on every scope, with the guard's verdict.</summary>
    public static TheoryData<string, string, bool> EveryCell() => new()
    {
        { Administrator, Global, true },
        { Administrator, InstitutionAType, true },
        { Administrator, InstitutionBType, true },
        { Administrator, CpsaSpecialityType, true },
        { Administrator, OtherSpecialityType, true },
        { Administrator, CpsaSubSpecialityType, true },
        { Administrator, OtherSubSpecialityType, true },

        { InstitutionalAdminOfA, Global, false },
        { InstitutionalAdminOfA, InstitutionAType, true },
        { InstitutionalAdminOfA, InstitutionBType, false },
        { InstitutionalAdminOfA, CpsaSpecialityType, false },
        { InstitutionalAdminOfA, OtherSpecialityType, false },
        { InstitutionalAdminOfA, CpsaSubSpecialityType, false },
        { InstitutionalAdminOfA, OtherSubSpecialityType, false },

        { CollegeAdminOfCpsa, Global, false },
        { CollegeAdminOfCpsa, InstitutionAType, false },
        { CollegeAdminOfCpsa, InstitutionBType, false },
        { CollegeAdminOfCpsa, CpsaSpecialityType, true },
        { CollegeAdminOfCpsa, OtherSpecialityType, false },
        { CollegeAdminOfCpsa, CpsaSubSpecialityType, true },
        { CollegeAdminOfCpsa, OtherSubSpecialityType, false }
    };

    [Theory]
    [MemberData(nameof(EveryCell))]
    public async Task TheGuard_TheEditor_AndTheList_GiveOneVerdict(string caller, string target, bool mayWrite)
    {
        var principal = PrincipalOf(caller);
        var (scope, scopeId) = Targets[target];
        var typeId = _typeIds[target];

        (await ActivityTypeAdminScope.MayWriteAsync(_db, principal, scope, scopeId, CancellationToken.None))
            .Should().Be(mayWrite, $"the rule for {caller} on a {target} type");

        var editor = await new GetActivityTypeEditorQueryHandler(_db).Handle(
            new GetActivityTypeEditorQuery(typeId, principal), CancellationToken.None);
        editor.CanWrite.Should().Be(mayWrite, $"the editor tells {caller} what the guard will do on a {target} type");

        var list = await new ListActivityTypesAdminQueryHandler(_db).Handle(
            new ListActivityTypesAdminQuery(principal), CancellationToken.None);
        var row = list.Items.SingleOrDefault(item => item.Id == typeId);
        if (row is not null)
        {
            row.CanWrite.Should().Be(mayWrite, $"the list tells {caller} what the guard will do on a {target} type");
        }
        else
        {
            mayWrite.Should().BeFalse("a type the caller may write is listed to them");
        }

        // The guard itself, through the save that keeps the type where it is: it asks of both the stored scope and the
        // requested one. A refusal is the guard's, and it leaves nothing for the audit pipeline's save to commit.
        var save = () => new SaveActivityTypeDraftCommandHandler(_db).Handle(
            SaveInPlace(typeId, KeyOf(target), target, scope, scopeId, principal), CancellationToken.None);
        if (mayWrite)
        {
            (await save()).CanWrite.Should().BeTrue("the save's own answer is the editor the page redraws from");
        }
        else
        {
            await save.Should().ThrowAsync<UnauthorizedAccessException>();
            _db.ChangeTracker.Entries().Where(entry => entry.State != EntityState.Unchanged).Should().BeEmpty();
        }
    }

    /// <summary>
    /// The scopes a caller may create a type in are exactly the (scope, target) pairs the guard admits them to, and the
    /// editor offers them in that order: the Scope picker's options, and a new type's default the first of them.
    /// </summary>
    [Theory]
    [InlineData(Administrator)]
    [InlineData(InstitutionalAdminOfA)]
    [InlineData(CollegeAdminOfCpsa)]
    public async Task TheWritableScopes_AreExactlyThePairsTheGuardAdmits(string caller)
    {
        var principal = PrincipalOf(caller);

        var offered = await ActivityTypeAdminScope.WritableScopesAsync(_db, principal, CancellationToken.None);

        var offeredPairs = offered
            .SelectMany(choice => choice.Scope == ActivityScope.Global
                ? [(choice.Scope, (int?)null)]
                : choice.Targets.Select(target => (choice.Scope, (int?)target.Id)))
            .ToList();

        var admitted = new List<(ActivityScope, int?)>();
        foreach (var pair in EveryPair())
        {
            if (await ActivityTypeAdminScope.MayWriteAsync(_db, principal, pair.Scope, pair.ScopeId, CancellationToken.None))
            {
                admitted.Add(pair);
            }
        }

        offeredPairs.Should().BeEquivalentTo(admitted, $"{caller} is offered exactly the scopes the guard admits them to");
    }

    [Fact]
    public async Task AnAdministrator_IsOfferedAllFourScopes_AndEveryTarget()
    {
        var offered = await ActivityTypeAdminScope.WritableScopesAsync(_db, TestPrincipals.Administrator(), CancellationToken.None);

        offered.Select(choice => choice.Scope).Should().Equal(
            ActivityScope.Global, ActivityScope.Institution, ActivityScope.Speciality, ActivityScope.SubSpeciality);
        offered[0].Targets.Should().BeEmpty();
        offered[1].Targets.Select(target => target.Name).Should().Equal("Baragwanath", "Kalafong");
        offered[2].Targets.Select(target => target.Name).Should().Equal("Internal Medicine", "Paediatrics");
        offered[3].Targets.Select(target => target.Name).Should().Equal("Internal Medicine / Cardiology", "Paediatrics / Neonatology");
    }

    [Fact]
    public async Task AnInstitutionalAdmin_IsOfferedHerOwnInstitution_Only()
    {
        var offered = await ActivityTypeAdminScope.WritableScopesAsync(_db, TestPrincipals.InstitutionalAdmin(InstitutionA), CancellationToken.None);

        offered.Should().ContainSingle();
        offered[0].Scope.Should().Be(ActivityScope.Institution);
        offered[0].Targets.Should().Equal(new ActivityTypeScopeTargetDto(InstitutionA, "Kalafong"));
    }

    [Fact]
    public async Task ACollegeAdmin_IsOfferedHisCollegesSpecialitiesAndSubSpecialities_Only()
    {
        var offered = await ActivityTypeAdminScope.WritableScopesAsync(_db, TestPrincipals.CollegeAdmin(Cpsa), CancellationToken.None);

        offered.Select(choice => choice.Scope).Should().Equal(ActivityScope.Speciality, ActivityScope.SubSpeciality);
        offered[0].Targets.Should().Equal(new ActivityTypeScopeTargetDto(CpsaSpeciality, "Paediatrics"));
        offered[1].Targets.Should().Equal(new ActivityTypeScopeTargetDto(CpsaSubSpeciality, "Paediatrics / Neonatology"));
    }

    /// <summary>
    /// A new type starts in the first scope the caller may write, never Global unless they are an Administrator (Step 1.26:
    /// a new type saved without touching Scope was refused, "Only global administrators may edit a globally-scoped activity
    /// type").
    /// </summary>
    [Theory]
    [InlineData(Administrator, ActivityScope.Global, null)]
    [InlineData(InstitutionalAdminOfA, ActivityScope.Institution, InstitutionA)]
    [InlineData(CollegeAdminOfCpsa, ActivityScope.Speciality, CpsaSpeciality)]
    public async Task ANewType_DefaultsToTheFirstWritableScope(string caller, ActivityScope scope, int? scopeId)
    {
        var editor = await NewTypeAsync(PrincipalOf(caller));

        editor.Scope.Should().Be(scope);
        editor.ScopeId.Should().Be(scopeId);
        editor.CanWrite.Should().BeTrue();
        editor.WritableScopes.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AnInstitutionalAdminsNewType_SavesAsItIsOffered_WithoutScopeBeingTouched()
    {
        var principal = TestPrincipals.InstitutionalAdmin(InstitutionA);
        var editor = await NewTypeAsync(principal);

        var saved = await new SaveActivityTypeDraftCommandHandler(_db).Handle(
            new SaveActivityTypeDraftCommand(
                null,
                "kgk_teaching_log",
                "KGK Teaching Session Log",
                editor.Description,
                editor.Scope,
                editor.ScopeId,
                editor.IsActive,
                editor.WbaToolKey,
                editor.DraftSchemaJson,
                editor.DraftWorkflowJson,
                editor.DraftCreditRulesJson,
                editor.DraftDisplayFieldsJson,
                "inst-admin-user",
                principal),
            CancellationToken.None);

        saved.Id.Should().BePositive();
        saved.Scope.Should().Be(ActivityScope.Institution);
        saved.ScopeId.Should().Be(InstitutionA);
        saved.CanWrite.Should().BeTrue();
        (await _db.ActivityTypes.SingleAsync(type => type.Key == "kgk_teaching_log")).ScopeId.Should().Be(InstitutionA);
    }

    /// <summary>A caller with no scope to write in is offered no new type, and the list offers them no New activity type.</summary>
    [Fact]
    public async Task ACallerWithNoWritableScope_CannotStartANewType_AndTheListSaysSo()
    {
        const int CollegeWithNoDisciplines = 12;
        _db.Colleges.Add(new College { Id = CollegeWithNoDisciplines, Name = "College of Nobody", ShortCode = "CN" });
        await _db.SaveChangesAsync();
        var principal = TestPrincipals.CollegeAdmin(CollegeWithNoDisciplines);

        var editor = await NewTypeAsync(principal);
        editor.CanWrite.Should().BeFalse();
        editor.WritableScopes.Should().BeEmpty();

        var list = await new ListActivityTypesAdminQueryHandler(_db).Handle(new ListActivityTypesAdminQuery(principal), CancellationToken.None);
        list.CanCreate.Should().BeFalse();
        list.Items.Should().NotBeEmpty("the published types are still his to read").And.OnlyContain(item => !item.CanWrite);
    }

    [Theory]
    [InlineData(Administrator)]
    [InlineData(InstitutionalAdminOfA)]
    [InlineData(CollegeAdminOfCpsa)]
    public async Task ACallerWithAWritableScope_IsOfferedANewType(string caller)
    {
        var list = await new ListActivityTypesAdminQueryHandler(_db).Handle(new ListActivityTypesAdminQuery(PrincipalOf(caller)), CancellationToken.None);

        list.CanCreate.Should().BeTrue();
    }

    /// <summary>
    /// The list offers only what the editor opens (T211): a College's unpublished type is not readable to an institution,
    /// so it is not listed to one, where its View would lead to "could not be found". Its College still lists it, to edit.
    /// </summary>
    [Fact]
    public async Task ACollegesUnpublishedType_IsListedToTheCollege_AndNotToAnInstitution()
    {
        var draftOnly = PublishedType("cpsa_draft_only", "CPSA draft only", ActivityScope.Speciality, CpsaSpeciality);
        draftOnly.Version = 0;
        _db.ActivityTypes.Add(draftOnly);
        await _db.SaveChangesAsync();

        var institutionList = await new ListActivityTypesAdminQueryHandler(_db).Handle(
            new ListActivityTypesAdminQuery(TestPrincipals.InstitutionalAdmin(InstitutionA)), CancellationToken.None);
        institutionList.Items.Should().NotContain(item => item.Id == draftOnly.Id);
        var openAsInstitution = () => new GetActivityTypeEditorQueryHandler(_db).Handle(
            new GetActivityTypeEditorQuery(draftOnly.Id, TestPrincipals.InstitutionalAdmin(InstitutionA)), CancellationToken.None);
        await openAsInstitution.Should().ThrowAsync<InvalidOperationException>("guard: the editor does not open it to her");

        var collegeList = await new ListActivityTypesAdminQueryHandler(_db).Handle(
            new ListActivityTypesAdminQuery(TestPrincipals.CollegeAdmin(Cpsa)), CancellationToken.None);
        collegeList.Items.Should().ContainSingle(item => item.Id == draftOnly.Id).Which.CanWrite.Should().BeTrue();
    }

    /// <summary>The editor names the stored scope's target, so a reader sees whose type it is.</summary>
    [Theory]
    [InlineData(Global, null)]
    [InlineData(InstitutionAType, "Kalafong")]
    [InlineData(CpsaSpecialityType, "Paediatrics")]
    [InlineData(CpsaSubSpecialityType, "Paediatrics / Neonatology")]
    public async Task TheEditor_NamesTheScopesTarget(string target, string? name)
    {
        var editor = await new GetActivityTypeEditorQueryHandler(_db).Handle(
            new GetActivityTypeEditorQuery(_typeIds[target], TestPrincipals.InstitutionalAdmin(InstitutionA)), CancellationToken.None);

        editor.ScopeTargetName.Should().Be(name);
    }

    /// <summary>
    /// The list names every row's target, as the editor names it, to every caller; disciplines are national, so another
    /// College's is named too. The page named targets from the caller's own institutions and disciplines until the T300
    /// review, so every other College's row read "Speciality · #1" (T291 item 5), to the CollegeAdmin T300 admits as much
    /// as to an InstitutionalAdmin.
    /// </summary>
    [Theory]
    [InlineData(Administrator)]
    [InlineData(InstitutionalAdminOfA)]
    [InlineData(CollegeAdminOfCpsa)]
    public async Task TheList_NamesEveryRowsTarget_AsTheEditorDoes(string caller)
    {
        var principal = PrincipalOf(caller);
        var names = new Dictionary<string, string?>
        {
            [Global] = null,
            [InstitutionAType] = "Kalafong",
            [InstitutionBType] = "Baragwanath",
            [CpsaSpecialityType] = "Paediatrics",
            [OtherSpecialityType] = "Internal Medicine",
            [CpsaSubSpecialityType] = "Paediatrics / Neonatology",
            [OtherSubSpecialityType] = "Internal Medicine / Cardiology"
        };

        var list = await new ListActivityTypesAdminQueryHandler(_db).Handle(new ListActivityTypesAdminQuery(principal), CancellationToken.None);

        list.Items.Select(item => item.Id).Should().Contain([_typeIds[OtherSpecialityType], _typeIds[OtherSubSpecialityType]],
            $"another College's published types are listed to {caller}");
        foreach (var (target, name) in names)
        {
            var row = list.Items.SingleOrDefault(item => item.Id == _typeIds[target]);
            if (row is null)
            {
                continue;
            }

            row.ScopeTargetName.Should().Be(name, $"the list names a {target} type's target to {caller}");
            var editor = await new GetActivityTypeEditorQueryHandler(_db).Handle(
                new GetActivityTypeEditorQuery(row.Id, principal), CancellationToken.None);
            editor.ScopeTargetName.Should().Be(row.ScopeTargetName, "the list and the editor name a target alike");
        }
    }

    /// <summary>A target that no longer exists has no name, and no College to admit a CollegeAdmin to it.</summary>
    [Fact]
    public async Task ARowWhoseTargetIsGone_IsNamedByNothing_AndWrittenOnlyByAnAdministrator()
    {
        var orphan = PublishedType("type-orphan", "Orphan", ActivityScope.Speciality, 999);
        _db.ActivityTypes.Add(orphan);
        await _db.SaveChangesAsync();

        foreach (var caller in new[] { Administrator, CollegeAdminOfCpsa })
        {
            var list = await new ListActivityTypesAdminQueryHandler(_db).Handle(
                new ListActivityTypesAdminQuery(PrincipalOf(caller)), CancellationToken.None);
            var row = list.Items.Single(item => item.Id == orphan.Id);
            row.ScopeTargetName.Should().BeNull();
            row.CanWrite.Should().Be(caller == Administrator);
        }
    }

    /// <summary>
    /// Only no id is a new type. /admin/activity-types/0 matches the builder's {ActivityTypeId:int} route, and it opened a
    /// new type's form headed "Edit " with no name, or a reader's notice, until the T300 review.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task AnIdThatNamesNoType_IsNotFound_NotANewType(int activityTypeId)
    {
        var open = () => new GetActivityTypeEditorQueryHandler(_db).Handle(
            new GetActivityTypeEditorQuery(activityTypeId, TestPrincipals.Administrator()), CancellationToken.None);

        await open.Should().ThrowAsync<InvalidOperationException>().WithMessage("The activity type could not be found.");
    }

    /// <summary>
    /// The filing page (<c>/activities/new</c>) asks for the type's definition only (<c>ForBuilder</c> false), so what the
    /// builder would let the caller change is not judged, and an Administrator's selection no longer reads every
    /// institution, speciality and sub-speciality (the T300 review). Who may open the type is judged all the same.
    /// </summary>
    [Fact]
    public async Task TheFilingPagesRead_JudgesNothingForTheBuilder_ButStillWhoMayOpenTheType()
    {
        var editor = await new GetActivityTypeEditorQueryHandler(_db).Handle(
            new GetActivityTypeEditorQuery(_typeIds[CpsaSpecialityType], TestPrincipals.Administrator(), ForBuilder: false),
            CancellationToken.None);

        editor.Key.Should().Be(KeyOf(CpsaSpecialityType));
        editor.PublishedSchemaJson.Should().Be(SchemaJson);
        editor.CanWrite.Should().BeFalse("not judged");
        editor.WritableScopes.Should().BeEmpty("not read");
        editor.ScopeTargetName.Should().BeNull("not read");

        var draftOnly = PublishedType("cpsa_filing_draft", "CPSA filing draft", ActivityScope.Speciality, CpsaSpeciality);
        draftOnly.Version = 0;
        _db.ActivityTypes.Add(draftOnly);
        await _db.SaveChangesAsync();

        var openAsInstitution = () => new GetActivityTypeEditorQueryHandler(_db).Handle(
            new GetActivityTypeEditorQuery(draftOnly.Id, TestPrincipals.InstitutionalAdmin(InstitutionA), ForBuilder: false),
            CancellationToken.None);
        await openAsInstitution.Should().ThrowAsync<InvalidOperationException>("a College's unpublished type is not hers to open");
    }

    [Fact]
    public async Task ListActivityTypesAdmin_InstitutionalAdmin_DoesNotSeeAnotherInstitutionsTypes()
    {
        var result = await new ListActivityTypesAdminQueryHandler(_db).Handle(
            new ListActivityTypesAdminQuery(TestPrincipals.InstitutionalAdmin(InstitutionA)),
            CancellationToken.None);

        result.Items.Select(item => item.Id).Should().Contain(_typeIds[InstitutionAType])
            .And.Contain(_typeIds[Global])
            .And.NotContain(_typeIds[InstitutionBType]);
    }

    [Fact]
    public async Task GetActivityTypeEditor_InstitutionalAdmin_OtherInstitutionsUnpublishedType_Throws()
    {
        var unpublished = PublishedType("type-b-draft", "TypeB draft", ActivityScope.Institution, InstitutionB);
        unpublished.Version = 0;
        _db.ActivityTypes.Add(unpublished);
        await _db.SaveChangesAsync();

        var act = () => new GetActivityTypeEditorQueryHandler(_db).Handle(
            new GetActivityTypeEditorQuery(unpublished.Id, TestPrincipals.InstitutionalAdmin(InstitutionA)),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SaveActivityTypeDraft_InstitutionalAdmin_RejectsMovingHerTypeToAnotherInstitution()
    {
        var principal = TestPrincipals.InstitutionalAdmin(InstitutionA);
        var act = () => new SaveActivityTypeDraftCommandHandler(_db).Handle(
            SaveInPlace(_typeIds[InstitutionAType], KeyOf(InstitutionAType), "Moved", ActivityScope.Institution, InstitutionB, principal),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task SaveActivityTypeDraft_InstitutionalAdmin_RejectsMovingHerTypeUpToGlobal()
    {
        var principal = TestPrincipals.InstitutionalAdmin(InstitutionA);
        var act = () => new SaveActivityTypeDraftCommandHandler(_db).Handle(
            SaveInPlace(_typeIds[InstitutionAType], KeyOf(InstitutionAType), "Moved", ActivityScope.Global, null, principal),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Only global administrators may edit a globally-scoped activity type.");
    }

    [Fact]
    public async Task DiscardActivityTypeDraft_InstitutionalAdmin_RejectsOtherInstitution()
    {
        var act = () => new DiscardActivityTypeDraftCommandHandler(_db).Handle(
            new DiscardActivityTypeDraftCommand(_typeIds[InstitutionBType], TestPrincipals.InstitutionalAdmin(InstitutionA)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You do not have permission to modify activity types in that institution.");
    }

    [Fact]
    public async Task DiscardActivityTypeDraft_InstitutionalAdmin_RejectsACollegeInstrument_InTheGuardsWords()
    {
        var act = () => new DiscardActivityTypeDraftCommandHandler(_db).Handle(
            new DiscardActivityTypeDraftCommand(_typeIds[CpsaSpecialityType], TestPrincipals.InstitutionalAdmin(InstitutionA)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You do not have permission to modify activity types in that speciality.");
    }

    private static IEnumerable<(ActivityScope Scope, int? ScopeId)> EveryPair()
    {
        yield return (ActivityScope.Global, null);
        yield return (ActivityScope.Institution, InstitutionA);
        yield return (ActivityScope.Institution, InstitutionB);
        yield return (ActivityScope.Speciality, CpsaSpeciality);
        yield return (ActivityScope.Speciality, OtherSpeciality);
        yield return (ActivityScope.SubSpeciality, CpsaSubSpeciality);
        yield return (ActivityScope.SubSpeciality, OtherSubSpeciality);
    }

    private static ClaimsPrincipal PrincipalOf(string caller) => caller switch
    {
        Administrator => TestPrincipals.Administrator(),
        InstitutionalAdminOfA => TestPrincipals.InstitutionalAdmin(InstitutionA),
        // Every signed-in user carries an institution claim (T113), a CollegeAdmin's included: it makes nobody an
        // InstitutionalAdmin, so it must not admit him to his institution's types.
        CollegeAdminOfCpsa => TestPrincipals.InRoles([WombatRoles.CollegeAdmin], "college-admin-user", InstitutionA)
            .WithCollege(Cpsa),
        _ => throw new ArgumentOutOfRangeException(nameof(caller), caller, null)
    };

    private async Task<ActivityTypeEditorDto> NewTypeAsync(ClaimsPrincipal principal)
        => await new GetActivityTypeEditorQueryHandler(_db).Handle(new GetActivityTypeEditorQuery(null, principal), CancellationToken.None);

    private static SaveActivityTypeDraftCommand SaveInPlace(
        int typeId, string key, string name, ActivityScope scope, int? scopeId, ClaimsPrincipal principal)
        => new(
            typeId,
            key,
            name,
            null,
            scope,
            scopeId,
            true,
            null,
            SchemaJson,
            WorkflowJson,
            CreditRulesJson,
            "[]",
            "actor",
            principal);

    private static string KeyOf(string target) => "type-" + target.ToLowerInvariant().Replace(' ', '-');

    private static ActivityType PublishedType(string key, string name, ActivityScope scope, int? scopeId) => new()
    {
        Key = key,
        Name = name,
        Scope = scope,
        ScopeId = scopeId,
        IsActive = true,
        Version = 1,
        SchemaJson = SchemaJson,
        WorkflowJson = WorkflowJson,
        CreditRulesJson = CreditRulesJson,
        OwnerUserId = "seeder",
        CreatedOn = DateTime.UtcNow
    };

    private static ApplicationDbContext NewDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private const string SchemaJson = """
        {
          "version": 1,
          "sections": [
            { "key": "details", "title": "Details", "fields": [ { "key": "title", "type": "text", "label": "Title", "required": true } ] }
          ]
        }
        """;

    private const string WorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "submitted", "label": "Submitted", "terminal": true }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject", "validation": "all" }
          ]
        }
        """;

    private const string CreditRulesJson = """{ "counts_for": [] }""";
}

internal static class CollegeClaimExtensions
{
    /// <summary>The principal with a College claim added, as sign-in issues one to a CollegeAdmin.</summary>
    public static ClaimsPrincipal WithCollege(this ClaimsPrincipal principal, int collegeId)
    {
        var identity = new ClaimsIdentity(principal.Claims, "test");
        identity.AddClaim(new Claim(Wombat.Application.Common.Security.WombatClaimTypes.CollegeId, collegeId.ToString()));
        return new ClaimsPrincipal(identity);
    }
}
