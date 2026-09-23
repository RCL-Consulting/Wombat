using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Activities;

/// <summary>
/// T108. The EPA picker must not offer what cannot be credited.
/// </summary>
/// <remarks>
/// <para>
/// The symptom: a paediatric registrar was offered thirty EPAs, two catalogues interleaved, PAED-001
/// through PAED-015 twice with different titles and no visual distinction. Fifteen of them credited
/// nothing. The assessment completed, the data was stored, the assessor was thanked, and
/// <c>CurriculumItemProgresses</c> gained no row.
/// </para>
/// <para>
/// The fix mirrors <c>CreditApplier.ResolveCurriculumItemsAsync</c> exactly, and these tests pin both
/// halves of that: what it hides, and — just as load-bearing — what it must NOT hide. An empty picker
/// on a required field is a worse failure than a permissive one, so every case where the subject
/// cannot be resolved falls back to the old claims-based behaviour rather than hiding everything.
/// </para>
/// </remarks>
public sealed class EpaOptionCreditScopeTests
{
    // The subject's own curriculum: the national version their institution adopted.
    private const int SubjectCurriculumId = 2;
    private const int SubjectInstitutionId = 20;

    // A second national version, adopted by nobody here. This is the one whose EPAs leaked into the
    // picker and silently credited nothing.
    private const int OtherCurriculumId = 3;
    private const int OtherInstitutionId = 30;

    private const int AdoptedEpaId = 1;
    private const int LocalExtraEpaId = 2;
    private const int OtherCurriculumEpaId = 3;
    private const int OtherInstitutionLocalEpaId = 4;

    // T122. The adopted core item's allow-list, in the stored (canonical, sorted) form.
    private const string AdoptedItemTools = """["cbd","dops","msf"]""";

    [Fact]
    public async Task TheCreditedEpaField_OffersOnlyWhatWouldActuallyCredit()
    {
        await using var db = CreateDb();
        SeedCatalogue(db);
        SeedTrainee(db, "registrar-1");
        await db.SaveChangesAsync();

        var options = await Service(db).GetEpaOptionsAsync(
            Principal(subSpecialityIds: [1]),
            new EpaOptionScope("registrar-1", NarrowToCreditable: true));

        // The national core item in their curriculum, and their own institution's local extra — both
        // of which CreditApplier would match. Not the other curriculum's EPA, which is precisely the
        // one the registrar could pick and never be credited for. Not another institution's local
        // extra either: the ci.OwningInstitutionId clause excludes it.
        options.Select(option => option.Value).Should().BeEquivalentTo(["1", "2"]);
    }

    [Fact]
    public async Task AnInstitutionLocalExtra_IsOffered_BecauseItCredits()
    {
        // T091 phase 3: an institution may add local curriculum items on top of the national core it
        // adopted. Those are outside the College's published catalogue but inside the trainee's
        // curriculum, and CreditApplier credits them. Any picker rule phrased as "the national
        // catalogue" rather than "join CurriculumItems" would drop them.
        await using var db = CreateDb();
        SeedCatalogue(db);
        SeedTrainee(db, "registrar-1");
        await db.SaveChangesAsync();

        var options = await Service(db).GetEpaOptionsAsync(
            Principal(subSpecialityIds: [1]),
            new EpaOptionScope("registrar-1", NarrowToCreditable: true));

        options.Select(option => option.Value).Should().Contain(LocalExtraEpaId.ToString());
    }

    [Fact]
    public async Task ASubjectWithNoTraineeProfile_StillGetsAUsablePicker()
    {
        // A PendingTrainee has no profile until an admin admits them, and /activities/new is open to
        // them. CreditApplier credits them nothing either way — but a required EPA field with zero
        // options and no explanation is an unsubmittable form, which is worse than the bug.
        await using var db = CreateDb();
        SeedCatalogue(db);
        await db.SaveChangesAsync();

        var options = await Service(db).GetEpaOptionsAsync(
            Principal(subSpecialityIds: [1]),
            new EpaOptionScope("nobody-yet", NarrowToCreditable: true));

        options.Should().NotBeEmpty("an empty picker is a worse failure than a permissive one");
        options.Select(option => option.Value).Should().BeEquivalentTo(["1", "2", "3", "4"]);
    }

    [Fact]
    public async Task AProfileWhoseCurriculumHasNoItems_FallsBackRatherThanEmptyingThePicker()
    {
        await using var db = CreateDb();
        SeedCatalogue(db);
        // Curriculum 99 exists for this trainee but carries no items at all.
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 9,
            UserId = "registrar-empty",
            InstitutionId = SubjectInstitutionId,
            CurriculumId = 99,
            ProgrammeStartDate = new DateOnly(2026, 1, 1),
            ExpectedCompletionDate = new DateOnly(2030, 1, 1),
            IsActive = true
        });
        await db.SaveChangesAsync();

        var options = await Service(db).GetEpaOptionsAsync(
            Principal(subSpecialityIds: [1]),
            new EpaOptionScope("registrar-empty", NarrowToCreditable: true));

        options.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AFieldTheCreditRulesNeverRead_IsNotNarrowed()
    {
        // A reflective note tags an EPA and credits nothing by design (empty counts_for). An MSF form
        // credits a fixed curriculum_item_id and never reads its epa field. Narrowing either would
        // hide EPAs whose selection changes nothing — and on the reflective note, whose epa field is
        // required, it would make the form unsubmittable.
        await using var db = CreateDb();
        SeedCatalogue(db);
        SeedTrainee(db, "registrar-1");
        await db.SaveChangesAsync();

        var options = await Service(db).GetEpaOptionsAsync(
            Principal(subSpecialityIds: [1]),
            new EpaOptionScope("registrar-1", NarrowToCreditable: false));

        options.Select(option => option.Value).Should().BeEquivalentTo(["1", "2", "3", "4"]);
    }

    [Fact]
    public async Task NoScopeAtAll_KeepsTheBuilderPreviewBehaviour()
    {
        // The activity-type builder's live preview has no subject: it designs a form for a future
        // cohort. It must keep seeing the whole catalogue it is scoped to.
        await using var db = CreateDb();
        SeedCatalogue(db);
        SeedTrainee(db, "registrar-1");
        await db.SaveChangesAsync();

        var options = await Service(db).GetEpaOptionsAsync(Principal(subSpecialityIds: [1]));

        options.Select(option => option.Value).Should().BeEquivalentTo(["1", "2", "3", "4"]);
    }

    [Fact]
    public async Task AStoredValueOutsideTheCurriculum_IsStillOfferedBack()
    {
        // An epa field renders as a <select>. Dropping a stored value from the options does not
        // narrow a choice — it renders the field as "Select…" and erases recorded evidence from the
        // page. Every encounter filed against an unadopted EPA before this fix is this case.
        await using var db = CreateDb();
        SeedCatalogue(db);
        SeedTrainee(db, "registrar-1");
        await db.SaveChangesAsync();

        var options = await Service(db).GetEpaOptionsAsync(
            Principal(subSpecialityIds: [1]),
            new EpaOptionScope("registrar-1", NarrowToCreditable: true, CurrentValue: OtherCurriculumEpaId.ToString()));

        options.Select(option => option.Value).Should().BeEquivalentTo(["1", "2", "3"]);
        options.Select(option => option.Label).Should().BeInAscendingOrder(StringComparer.Ordinal);
    }

    [Fact]
    public async Task TheAssessorsOwnClaims_DoNotNarrowTheSubjectsCreditableEpas()
    {
        // On the detail page the principal is the assessor and the subject is the registrar. The
        // curriculum-item join already implies the right discipline, so intersecting it with the
        // VIEWER's sub-speciality claims could only over-hide — here it would collapse to nothing.
        await using var db = CreateDb();
        SeedCatalogue(db);
        SeedTrainee(db, "registrar-1");
        await db.SaveChangesAsync();

        var assessorFromAnotherSubSpeciality = Principal(subSpecialityIds: [77]);

        var options = await Service(db).GetEpaOptionsAsync(
            assessorFromAnotherSubSpeciality,
            new EpaOptionScope("registrar-1", NarrowToCreditable: true));

        options.Select(option => option.Value).Should().BeEquivalentTo(["1", "2"]);
    }

    [Fact]
    public async Task AGraduatedTraineesProfile_StillNarrowsToWhatCredits()
    {
        // CreditApplier deliberately does not filter on IsActive: TraineeProfile.Complete clears it on
        // graduation, and alumni must still credit under RebuildCurriculumProgress. The picker follows.
        await using var db = CreateDb();
        SeedCatalogue(db);
        var profile = SeedTrainee(db, "alumnus-1");
        profile.IsActive = false;
        await db.SaveChangesAsync();

        var options = await Service(db).GetEpaOptionsAsync(
            Principal(subSpecialityIds: [1]),
            new EpaOptionScope("alumnus-1", NarrowToCreditable: true));

        options.Select(option => option.Value).Should().BeEquivalentTo(["1", "2"]);
    }

    /// <summary>
    /// The anti-empty-picker fallback has to be decided against the SAME predicate the caller then
    /// applies, or it does not guard anything.
    /// </summary>
    /// <remarks>
    /// <c>DeactivateEpaCommandHandler</c> does not check for referencing curriculum items, so a
    /// curriculum can hold items whose EPAs have all been deactivated — exactly what retiring a
    /// superseded catalogue does. Resolving "is the narrowed set empty?" before the <c>IsActive</c>
    /// filter reported a non-empty set, and the caller then filtered it to nothing: a required
    /// <c>&lt;select&gt;</c> with no options, which cannot be submitted.
    /// </remarks>
    [Fact]
    public async Task GetEpaOptions_WhenEveryCreditableEpaIsDeactivated_FallsBackInsteadOfEmptyingThePicker()
    {
        await using var db = CreateDb();
        SeedCatalogue(db);
        SeedTrainee(db, "trainee-1");
        await db.SaveChangesAsync();

        // Retire the subject's whole catalogue, leaving only the unadopted version's EPA active.
        // The curriculum items survive: deactivating an EPA does not remove them, which is exactly
        // what retiring a superseded catalogue leaves behind.
        foreach (var epa in await db.Epas.ToListAsync())
        {
            epa.IsActive = epa.Id == OtherCurriculumEpaId;
        }

        await db.SaveChangesAsync();

        var options = await Service(db).GetEpaOptionsAsync(
            Principal([1]),
            new EpaOptionScope("trainee-1", NarrowToCreditable: true));

        options.Should().NotBeEmpty("an empty required picker is unsubmittable — worse than a permissive one");
        options.Should().Contain(option => option.Value == OtherCurriculumEpaId.ToString());
    }

    // ---------------------------------------------------------------------------------------------------------------
    // T122. The EPA→tool allow-list intersects the T108 creditable set. The write path refuses an EPA whose item does
    // not permit the activity type's instrument (D20), so offering it would hand the trainee a choice that bounces at
    // submit. These facts pin both halves again: what the list hides, and every case where it must hide nothing,
    // because between them T108, T123 d3 and T122 must never empty a required picker.
    // ---------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// The adopted core item leaves Mini-CEX off its list, so a Mini-CEX may not credit it and the picker drops it.
    /// The local extra has no list at all and is unrestricted, so it stays.
    /// </summary>
    /// <remarks>
    /// The key arrives through <see cref="WbaTool.NormalizeKey" />, so casing and padding on the type's key must not
    /// decide the answer: the seeder, the admin editor and the migration all write the normalised form, but a type
    /// row the admin saved before normalisation existed would otherwise silently pass the gate it should meet.
    /// </remarks>
    [Theory]
    [InlineData("mini_cex")]
    [InlineData(" MINI_CEX ")]
    [InlineData("Mini_Cex")]
    public async Task AToolTheAdoptedItemForbids_LeavesOnlyTheUnrestrictedLocalExtra(string toolKey)
    {
        await using var db = CreateDb();
        SeedCatalogue(db);
        SeedTrainee(db, "registrar-1");
        await db.SaveChangesAsync();

        var offered = await CreditableFor(db, "registrar-1", toolKey);

        offered.Should().BeEquivalentTo([LocalExtraEpaId.ToString()],
            "the core item's list names CBD, DOPS and MSF; the local extra has no list, so it is unrestricted");
    }

    [Theory]
    [InlineData("cbd")]
    [InlineData("dops")]
    [InlineData("msf")]
    [InlineData(" CBD ")]
    public async Task AToolTheAdoptedItemPermits_KeepsTheWholeCreditableSet(string toolKey)
    {
        // Permitted on the core item, unrestricted on the local extra: nothing is hidden. An implementation that
        // treated "no list" as "nothing permitted" would drop the local extra here, which credits (T091 phase 3).
        await using var db = CreateDb();
        SeedCatalogue(db);
        SeedTrainee(db, "registrar-1");
        await db.SaveChangesAsync();

        var offered = await CreditableFor(db, "registrar-1", toolKey);

        offered.Should().BeEquivalentTo([AdoptedEpaId.ToString(), LocalExtraEpaId.ToString()]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ATypeWithNoToolKey_IsNotNarrowedByAnyList(string? toolKey)
    {
        // D21. The generic seeds other than mini_cex/dops/cbd, and every institution-authored type, declare no
        // instrument. A list cannot refuse a tool that has not said what it is, so the T108 set stands unchanged —
        // including the core item whose list would refuse a Mini-CEX.
        await using var db = CreateDb();
        SeedCatalogue(db);
        SeedTrainee(db, "registrar-1");
        await db.SaveChangesAsync();

        var offered = await CreditableFor(db, "registrar-1", toolKey);

        offered.Should().BeEquivalentTo([AdoptedEpaId.ToString(), LocalExtraEpaId.ToString()]);
    }

    [Theory]
    [InlineData("mini_cex")]
    [InlineData("cbd")]
    [InlineData("msf")]
    [InlineData("direct_observation")]
    public async Task ACurriculumWithNoListsAtAll_IsUnchangedByAnyToolKey(string toolKey)
    {
        // Every institution-local curriculum, and every national one the College has not annotated, holds null
        // lists. T122 must be invisible to a trainee on one of those, whatever instrument the type declares.
        await using var db = CreateDb();
        SeedCatalogue(db);
        Restrict(db, itemId: 1, permittedToolsJson: null);
        SeedTrainee(db, "registrar-1");
        await db.SaveChangesAsync();

        var offered = await CreditableFor(db, "registrar-1", toolKey);

        offered.Should().BeEquivalentTo([AdoptedEpaId.ToString(), LocalExtraEpaId.ToString()]);
    }

    /// <summary>
    /// A tool that may credit none of the subject's EPAs falls back to the T108 creditable set: not the empty set,
    /// and not the claims set.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Empty is an unsubmittable required <c>&lt;select&gt;</c> that explains nothing. The claims set re-offers the
    /// other curriculum's EPA and another institution's local extra, which credit nothing: T108's defect, reopened.
    /// The creditable set is the only fallback whose every choice the write path then refuses with a message that
    /// names the instruments the curriculum accepts, which is the one answer the trainee can act on.
    /// </para>
    /// <para>
    /// Items 3 and 4 DO permit Mini-CEX, and that is the trap: item 3 is another institution's local extra on the
    /// same curriculum and item 4 is on a curriculum the subject never adopted. If the permission test ran on any
    /// row the owner predicate had not already excluded, one of them would "rescue" the tool and leak its EPA in.
    /// </para>
    /// </remarks>
    [Fact]
    public async Task AToolNoItemPermits_FallsBackToTheCreditableSet_NotToEmptyAndNotToClaims()
    {
        await using var db = CreateDb();
        SeedCatalogue(db);
        Restrict(db, itemId: 2, permittedToolsJson: """["cbd"]""");
        Restrict(db, itemId: 3, permittedToolsJson: """["mini_cex"]""");
        Restrict(db, itemId: 4, permittedToolsJson: """["mini_cex"]""");
        SeedTrainee(db, "registrar-1");
        await db.SaveChangesAsync();

        var offered = await CreditableFor(db, "registrar-1", "mini_cex");

        offered.Should().NotBeEmpty("an empty required picker is unsubmittable and explains nothing");
        offered.Should().BeEquivalentTo([AdoptedEpaId.ToString(), LocalExtraEpaId.ToString()],
            "the fallback is T108's creditable set; the claims set would re-offer EPAs that credit nothing");
    }

    [Theory]
    [InlineData("registrar-1", false)]
    [InlineData("nobody-yet", true)]
    public async Task WhereTheClaimsFilterApplies_TheToolKeyIsIgnored(string subjectUserId, bool narrowToCreditable)
    {
        // The tool key reaches the narrowing arm only. A field the credit rules never read (NarrowToCreditable
        // false), and a subject with no profile (T108's PendingTrainee fallback), both get the claims filter, and
        // narrowing that by instrument would hide choices whose selection changes nothing — or, for the pending
        // trainee, whittle a deliberately permissive picker down on a rule that cannot credit them anyway.
        await using var db = CreateDb();
        SeedCatalogue(db);
        Restrict(db, itemId: 2, permittedToolsJson: """["cbd"]""");
        SeedTrainee(db, "registrar-1");
        await db.SaveChangesAsync();

        var options = await Service(db).GetEpaOptionsAsync(
            Principal(subSpecialityIds: [1]),
            new EpaOptionScope(subjectUserId, narrowToCreditable, WbaToolKey: "mini_cex"));

        options.Select(option => option.Value).Should().BeEquivalentTo(["1", "2", "3", "4"]);
    }

    [Fact]
    public async Task AStoredValueTheToolMayNotCredit_IsStillOfferedBack()
    {
        // D20: an allow-list can be edited after an encounter was filed, and a Mini-CEX filed against PAED-001
        // before the College took Mini-CEX off it is recorded evidence. Narrowing it out of the options would render
        // the field as "Select…" and erase that evidence from the page — the T108 stored-value rule, which must run
        // after the tool narrowing, not before it.
        await using var db = CreateDb();
        SeedCatalogue(db);
        SeedTrainee(db, "registrar-1");
        await db.SaveChangesAsync();

        var offered = await Service(db).GetEpaOptionsAsync(
            Principal(subSpecialityIds: [1]),
            new EpaOptionScope("registrar-1", NarrowToCreditable: true, CurrentValue: AdoptedEpaId.ToString(), WbaToolKey: "mini_cex"));

        offered.Select(option => option.Value).Should().BeEquivalentTo([AdoptedEpaId.ToString(), LocalExtraEpaId.ToString()],
            "the stored EPA stays on the page even though a Mini-CEX may no longer credit it");
        offered.Select(option => option.Label).Should().BeInAscendingOrder(StringComparer.Ordinal);
    }

    /// <summary>
    /// The tool fallback must be decided against the ACTIVE creditable set, or it guards nothing.
    /// </summary>
    /// <remarks>
    /// The T108 deactivation fact, one layer on. If the permitted set were computed over every item and only then
    /// filtered on <c>Epa.IsActive</c>, a tool whose only permitting item's EPA was retired would read as "permitted
    /// somewhere", skip the fallback, and then be filtered to nothing.
    /// </remarks>
    [Fact]
    public async Task WhenTheOnlyPermittingItemsEpaIsDeactivated_TheToolFallsBackToTheActiveSet_NotToEmpty()
    {
        await using var db = CreateDb();
        SeedCatalogue(db);
        Restrict(db, itemId: 2, permittedToolsJson: """["mini_cex"]""");
        SeedTrainee(db, "registrar-1");
        await db.SaveChangesAsync();

        // Before: DOPS is permitted on the core item only.
        (await CreditableFor(db, "registrar-1", "dops")).Should().BeEquivalentTo([AdoptedEpaId.ToString()]);

        (await db.Epas.SingleAsync(epa => epa.Id == AdoptedEpaId)).IsActive = false;
        await db.SaveChangesAsync();

        var offered = await CreditableFor(db, "registrar-1", "dops");

        offered.Should().NotBeEmpty("an empty required picker is unsubmittable");
        offered.Should().BeEquivalentTo([LocalExtraEpaId.ToString()],
            "the fallback is the T108 set as the caller renders it: active EPAs on the subject's curriculum");
    }

    /// <summary>
    /// A list that does not parse, and a stored empty list, restrict nothing: the item stays in.
    /// </summary>
    /// <remarks>
    /// <see cref="CurriculumItem.NormalizePermittedToolsJson" /> never writes <c>[]</c> — it writes null — but a
    /// migration, a hand-edited row or a future writer can. Reading an empty or broken list as "nothing permitted"
    /// would make the EPA unfileable by every keyed tool on the strength of a data-quality problem. Item 2 is given a
    /// real list that refuses Mini-CEX so the fact can tell the two readings apart: kept, the answer is the core item
    /// alone; refused, nothing is permitted and the fallback returns both.
    /// </remarks>
    [Theory]
    [InlineData("{not json")]
    [InlineData("""{"cbd":true}""")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("[1, 2]")]
    [InlineData("""["   "]""")]
    public async Task AMalformedOrEmptyStoredList_IsNotRestricted(string storedJson)
    {
        await using var db = CreateDb();
        SeedCatalogue(db);
        Restrict(db, itemId: 1, permittedToolsJson: storedJson);
        Restrict(db, itemId: 2, permittedToolsJson: """["cbd"]""");
        SeedTrainee(db, "registrar-1");
        await db.SaveChangesAsync();

        var offered = await CreditableFor(db, "registrar-1", "mini_cex");

        offered.Should().BeEquivalentTo([AdoptedEpaId.ToString()],
            "the core item's unreadable list restricts nothing, while the local extra's real list refuses Mini-CEX");
    }

    [Fact]
    public async Task TheMsfCoverageList_IgnoresToolListsEntirely()
    {
        // T121: a released MSF campaign writes one msf_cpsa activity per covered EPA, and the campaign's EPA list
        // comes from GetSubjectCurriculumEpaOptionsAsync. Neither item here names MSF. If that list honoured the
        // allow-lists a release would silently drop EPAs from the trainee's record; the write path is where a tool
        // is refused, with a message, and credit never re-checks (D20).
        await using var db = CreateDb();
        SeedCatalogue(db);
        Restrict(db, itemId: 1, permittedToolsJson: """["cbd"]""");
        Restrict(db, itemId: 2, permittedToolsJson: """["mini_cex"]""");
        SeedTrainee(db, "registrar-1");
        await db.SaveChangesAsync();

        var options = await Service(db).GetSubjectCurriculumEpaOptionsAsync("registrar-1");

        options.Select(option => option.Value).Should().BeEquivalentTo([AdoptedEpaId.ToString(), LocalExtraEpaId.ToString()]);
    }

    /// <summary>
    /// Two profiles tied on <c>IsActive</c> and <c>ProgrammeStartDate</c> resolve to the higher id, whichever order
    /// the rows come back in.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The picker, the credit engine and the write-path gate share <c>CreditTargetResolver.PickProfileAsync</c> so
    /// they cannot choose different curricula for one user. Without a total order that promise is only as good as the
    /// row order the store happens to return, which Postgres does not guarantee. The theory seeds both insertion
    /// orders: a pick that fell through to row order would give a different curriculum in one of them.
    /// </para>
    /// <para>
    /// Both-active is reachable only below Postgres, whose filtered unique index on <c>UserId</c> allows one active
    /// profile per user. Both-inactive is reachable there — a trainee who graduated twice with the same start date —
    /// and it is the tie credit replays on under RebuildCurriculumProgress.
    /// </para>
    /// </remarks>
    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task TwoProfilesTiedOnActiveAndStartDate_ResolveToTheHigherProfileId(bool bothActive, bool higherIdInsertedFirst)
    {
        await using var db = CreateDb();
        SeedCatalogue(db);

        var sharedStart = new DateOnly(2026, 1, 1);
        var lower = new TraineeProfile
        {
            Id = 5,
            UserId = "twice-enrolled",
            InstitutionId = SubjectInstitutionId,
            CurriculumId = SubjectCurriculumId,
            ProgrammeStartDate = sharedStart,
            ExpectedCompletionDate = new DateOnly(2030, 1, 1),
            IsActive = bothActive
        };
        var higher = new TraineeProfile
        {
            Id = 7,
            UserId = "twice-enrolled",
            InstitutionId = OtherInstitutionId,
            CurriculumId = OtherCurriculumId,
            ProgrammeStartDate = sharedStart,
            ExpectedCompletionDate = new DateOnly(2030, 1, 1),
            IsActive = bothActive
        };

        foreach (var profile in higherIdInsertedFirst ? new[] { higher, lower } : [lower, higher])
        {
            db.Set<TraineeProfile>().Add(profile);
            await db.SaveChangesAsync();
        }

        var picked = await CreditableFor(db, "twice-enrolled", toolKey: null);
        var msfList = await Service(db).GetSubjectCurriculumEpaOptionsAsync("twice-enrolled");

        // Profile 7 is on the other curriculum, whose only item is EPA 3. Profile 5 would have given EPAs 1 and 2.
        picked.Should().BeEquivalentTo([OtherCurriculumEpaId.ToString()], "the tie resolves to the higher profile id");
        msfList.Select(option => option.Value).Should().BeEquivalentTo([OtherCurriculumEpaId.ToString()],
            "both readings pick the profile through the same resolver");
    }

    private static async Task<IReadOnlyList<string>> CreditableFor(ApplicationDbContext db, string subjectUserId, string? toolKey)
    {
        var options = await Service(db).GetEpaOptionsAsync(
            Principal(subSpecialityIds: [1]),
            new EpaOptionScope(subjectUserId, NarrowToCreditable: true, WbaToolKey: toolKey));

        return options.Select(option => option.Value).ToList();
    }

    /// <summary>Overwrites one seeded item's allow-list before the first save.</summary>
    private static void Restrict(ApplicationDbContext db, int itemId, string? permittedToolsJson)
        => db.CurriculumItems.Local.Single(item => item.Id == itemId).PermittedToolsJson = permittedToolsJson;

    private static ActivityReferenceDataService Service(ApplicationDbContext db)
        => new(db, new NoUsersAdministrationService());

    private static void SeedCatalogue(ApplicationDbContext db)
    {
        db.Epas.Add(new Epa { Id = AdoptedEpaId, SubSpecialityId = 1, Code = "PAED-001", Title = "Acute admission", IsActive = true });
        db.Epas.Add(new Epa { Id = LocalExtraEpaId, SubSpecialityId = 1, Code = "PAED-L01", Title = "Local extra", OwningInstitutionId = SubjectInstitutionId, IsActive = true });
        db.Epas.Add(new Epa { Id = OtherCurriculumEpaId, SubSpecialityId = 1, Code = "PAED-001-V11", Title = "Providing paediatric emergency care", IsActive = true });
        db.Epas.Add(new Epa { Id = OtherInstitutionLocalEpaId, SubSpecialityId = 1, Code = "PAED-L99", Title = "Another institution's extra", OwningInstitutionId = OtherInstitutionId, IsActive = true });

        // The subject's curriculum: one national core item plus their institution's local extra. The core item
        // carries a T122 tool allow-list that leaves Mini-CEX off, as PAED-005 does in Annexure A; the local extra
        // carries none, so it is unrestricted (D21). No T108 fact passes a tool key, so none of them sees the list.
        db.CurriculumItems.Add(new CurriculumItem { Id = 1, CurriculumId = SubjectCurriculumId, EpaId = AdoptedEpaId, RequiredCount = 5, MinimumLevelOrder = 3, WindowMonths = 12, PermittedToolsJson = AdoptedItemTools });
        db.CurriculumItems.Add(new CurriculumItem { Id = 2, CurriculumId = SubjectCurriculumId, EpaId = LocalExtraEpaId, OwningInstitutionId = SubjectInstitutionId, RequiredCount = 2, MinimumLevelOrder = 3, WindowMonths = 12 });

        // Another institution's local extra, on the SAME curriculum — credit must not leak onto it.
        db.CurriculumItems.Add(new CurriculumItem { Id = 3, CurriculumId = SubjectCurriculumId, EpaId = OtherInstitutionLocalEpaId, OwningInstitutionId = OtherInstitutionId, RequiredCount = 2, MinimumLevelOrder = 3, WindowMonths = 12 });

        // The unadopted curriculum version. Visible and selectable today; creditable for nobody here.
        db.CurriculumItems.Add(new CurriculumItem { Id = 4, CurriculumId = OtherCurriculumId, EpaId = OtherCurriculumEpaId, RequiredCount = 5, MinimumLevelOrder = 3, WindowMonths = 12 });
    }

    private static TraineeProfile SeedTrainee(ApplicationDbContext db, string userId)
    {
        var profile = new TraineeProfile
        {
            Id = 1,
            UserId = userId,
            InstitutionId = SubjectInstitutionId,
            CurriculumId = SubjectCurriculumId,
            ProgrammeStartDate = new DateOnly(2026, 1, 1),
            ExpectedCompletionDate = new DateOnly(2030, 1, 1),
            IsActive = true
        };

        db.Set<TraineeProfile>().Add(profile);
        return profile;
    }

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ClaimsPrincipal Principal(IReadOnlyCollection<int> subSpecialityIds)
    {
        var claims = subSpecialityIds
            .Select(id => new Claim(WombatClaimTypes.SubSpecialityId, id.ToString()))
            .ToList();

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private sealed class NoUsersAdministrationService : IUserAdministrationService
    {
        public Task<IReadOnlyList<UserIdentityDetails>> ListUsersInRoleAsync(string role, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UserIdentityDetails>>([]);
        public Task<UserIdentityDetails?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
            => Task.FromResult<UserIdentityDetails?>(null);
        public Task<IReadOnlyList<UserIdentityDetails>> ListAllUsersAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UserIdentityDetails>>([]);
        public Task UpdateNamesAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task UpdateScopeAsync(string userId, int institutionId, IReadOnlyCollection<int> specialityIds, IReadOnlyCollection<int> subSpecialityIds, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task PromotePendingTraineeAsync(string userId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task AddRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task RemoveRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task SetLockoutAsync(string userId, bool locked, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
