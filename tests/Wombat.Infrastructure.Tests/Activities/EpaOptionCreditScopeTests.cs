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

    private static ActivityReferenceDataService Service(ApplicationDbContext db)
        => new(db, new NoUsersAdministrationService());

    private static void SeedCatalogue(ApplicationDbContext db)
    {
        db.Epas.Add(new Epa { Id = AdoptedEpaId, SubSpecialityId = 1, Code = "PAED-001", Title = "Acute admission", IsActive = true });
        db.Epas.Add(new Epa { Id = LocalExtraEpaId, SubSpecialityId = 1, Code = "PAED-L01", Title = "Local extra", OwningInstitutionId = SubjectInstitutionId, IsActive = true });
        db.Epas.Add(new Epa { Id = OtherCurriculumEpaId, SubSpecialityId = 1, Code = "PAED-001-V11", Title = "Providing paediatric emergency care", IsActive = true });
        db.Epas.Add(new Epa { Id = OtherInstitutionLocalEpaId, SubSpecialityId = 1, Code = "PAED-L99", Title = "Another institution's extra", OwningInstitutionId = OtherInstitutionId, IsActive = true });

        // The subject's curriculum: one national core item plus their institution's local extra.
        db.CurriculumItems.Add(new CurriculumItem { Id = 1, CurriculumId = SubjectCurriculumId, EpaId = AdoptedEpaId, RequiredCount = 5, MinimumLevelOrder = 3, WindowMonths = 12 });
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
