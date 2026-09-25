using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Curricula;

/// <summary>
/// T211 — the curriculum pages offer each admin only what they can act on. The list and the one-curriculum read share one
/// rule for which curricula a caller opens, every curriculum handed to the pages is cut to the items the caller reads, and
/// each item says whether the caller may change it, by the rule the commands enforce.
/// </summary>
/// <remarks>
/// <para>
/// Before T211 the read was the College's alone (<c>CanAccessCollege</c>), so the curricula list showed an
/// InstitutionalAdmin their adopted curricula and every link on it led to not-found; and every caller read every
/// institution's local items, with Edit and Remove on each, which the Update and Remove commands then refused.
/// </para>
/// <para>
/// The world: curriculum 3000 is adopted by institution A, and was by B until B moved to version 3001. It holds a national
/// item and two items of each institution's own: one naming its local EPA, one naming a national EPA the College has not
/// put on the curriculum (T195 allows both). Institution C adopted 3001 once and has left it, keeping nothing of its own
/// there. Curriculum 3002 is another College's.
/// </para>
/// </remarks>
public sealed class CurriculumAdminScopeTests
{
    private const int CollegeId = 1;
    private const int OtherCollegeId = 2;
    private const int InstitutionA = 40;
    private const int InstitutionB = 41;
    private const int InstitutionC = 42;

    private const int Adopted = 3000;          // A adopts it; B did, and moved on
    private const int NextVersion = 3001;      // B adopts it
    private const int OtherCollegesCurriculum = 3002;

    private const int NationalItem = 7000;     // PAED-001
    private const int ALocalItem = 7001;       // LOC-A01, A's
    private const int BLocalItem = 7002;       // LOC-B01, B's
    private const int ANationalEpaItem = 7003; // PAED-002, A's own item naming a national EPA
    private const int BNationalEpaItem = 7004; // PAED-003, B's own item naming a national EPA

    private const int Paed001 = 5000;
    private const int Paed002 = 5001;
    private const int LocA01 = 5010;
    private const int LocB01 = 5011;
    private const int LocA02 = 5012;
    private const int Paed003 = 5013;

    public enum Caller
    {
        Administrator,
        CollegeAdmin,
        OtherCollegeAdmin,
        InstitutionalAdminA,
        InstitutionalAdminB,
        InstitutionalAdminC,
        CoordinatorA,
        CollegeAdminAtA,
        // CollegeAdmin of the other College, and InstitutionalAdmin of A: one user may hold both roles (T211 review).
        OtherCollegeAdminAndInstitutionalAdminA
    }

    // ---- Who opens which curriculum ----

    [Theory]
    [InlineData(Caller.Administrator, Adopted, true)]
    [InlineData(Caller.Administrator, OtherCollegesCurriculum, true)]
    [InlineData(Caller.CollegeAdmin, Adopted, true)]
    [InlineData(Caller.CollegeAdmin, NextVersion, true)]
    [InlineData(Caller.CollegeAdmin, OtherCollegesCurriculum, false)]
    [InlineData(Caller.OtherCollegeAdmin, Adopted, false)]
    [InlineData(Caller.OtherCollegeAdmin, OtherCollegesCurriculum, true)]
    // An InstitutionalAdmin opens what their institution has actively adopted, and what holds its own items.
    [InlineData(Caller.InstitutionalAdminA, Adopted, true)]
    [InlineData(Caller.InstitutionalAdminA, NextVersion, false)]
    [InlineData(Caller.InstitutionalAdminA, OtherCollegesCurriculum, false)]
    // B moved off 3000, but its trainees admitted to 3000 stay there, measured against B's own items on it.
    [InlineData(Caller.InstitutionalAdminB, Adopted, true)]
    [InlineData(Caller.InstitutionalAdminB, NextVersion, true)]
    [InlineData(Caller.InstitutionalAdminC, Adopted, false)]
    // An adoption since superseded opens nothing by itself.
    [InlineData(Caller.InstitutionalAdminC, NextVersion, false)]
    // Each role through its own arm: the College's own curricula, and what the institution adopted.
    [InlineData(Caller.OtherCollegeAdminAndInstitutionalAdminA, Adopted, true)]
    [InlineData(Caller.OtherCollegeAdminAndInstitutionalAdminA, NextVersion, false)]
    [InlineData(Caller.OtherCollegeAdminAndInstitutionalAdminA, OtherCollegesCurriculum, true)]
    // An institution claim is on every sign-in (T113); it opens nothing without the InstitutionalAdmin role.
    [InlineData(Caller.CoordinatorA, Adopted, false)]
    public async Task TheRead_OpensACurriculum_OnlyForThoseItAdmits(Caller caller, int curriculumId, bool opens)
    {
        await using var dbContext = await SeededAsync();

        var curriculum = await ReadAsync(dbContext, curriculumId, caller);

        if (opens)
        {
            curriculum.Should().NotBeNull();
            curriculum!.Id.Should().Be(curriculumId);
        }
        else
        {
            curriculum.Should().BeNull("a curriculum the caller may not open reads as not found (T056)");
        }
    }

    // ---- Which items they read, and which they may change ----

    [Fact]
    public async Task AnInstitutionalAdmin_ReadsTheNationalItemsReadOnly_AndTheirOwnEditable_AndNoOtherInstitutions()
    {
        await using var dbContext = await SeededAsync();

        var curriculum = (await ReadAsync(dbContext, Adopted, Caller.InstitutionalAdminA))!;

        curriculum.CanEditCurriculum.Should().BeFalse("the curriculum's details and national items are the College's");
        Editability(curriculum).Should().BeEquivalentTo(new Dictionary<int, bool>
        {
            [NationalItem] = false,
            [ALocalItem] = true,
            [ANationalEpaItem] = true
        }, "B's items are B's business, and a national item is the College's to change");
        curriculum.Items.Single(item => item.Id == ALocalItem).IsLocal.Should().BeTrue();
        curriculum.Items.Single(item => item.Id == NationalItem).IsLocal.Should().BeFalse();
    }

    [Fact]
    public async Task AnInstitutionThatMovedOffAVersion_ReadsItsOwnItemsThereEditable_AndNoOtherInstitutions()
    {
        await using var dbContext = await SeededAsync();

        var curriculum = (await ReadAsync(dbContext, Adopted, Caller.InstitutionalAdminB))!;

        curriculum.CanEditCurriculum.Should().BeFalse();
        Editability(curriculum).Should().BeEquivalentTo(new Dictionary<int, bool>
        {
            [NationalItem] = false,
            [BLocalItem] = true,
            [BNationalEpaItem] = true
        }, "B keeps its own items on the version its earlier trainees stay on; A's are A's");
    }

    [Theory]
    [InlineData(Caller.CollegeAdmin)]
    // A CollegeAdmin's own institution claim does not make any institution's items theirs.
    [InlineData(Caller.CollegeAdminAtA)]
    public async Task ACollegeAdmin_ReadsTheNationalItems_Editable_AndNoInstitutionsOwn(Caller caller)
    {
        await using var dbContext = await SeededAsync();

        var curriculum = (await ReadAsync(dbContext, Adopted, caller))!;

        curriculum.CanEditCurriculum.Should().BeTrue();
        Editability(curriculum).Should().BeEquivalentTo(new Dictionary<int, bool> { [NationalItem] = true },
            "an institution's own item is its own: the College neither changes it nor reads it, as it reads no local EPA");
    }

    [Fact]
    public async Task AnAdministrator_ReadsAndMayChangeEveryItem()
    {
        await using var dbContext = await SeededAsync();

        var curriculum = (await ReadAsync(dbContext, Adopted, Caller.Administrator))!;

        curriculum.CanEditCurriculum.Should().BeTrue();
        Editability(curriculum).Should().BeEquivalentTo(new Dictionary<int, bool>
        {
            [NationalItem] = true,
            [ALocalItem] = true,
            [BLocalItem] = true,
            [ANationalEpaItem] = true,
            [BNationalEpaItem] = true
        });
    }

    // ---- Whose own item (T222) ----

    [Fact]
    public async Task AnAdministrator_ReadsWhichInstitutionOwnsEachLocalItem_OnTheReadAndTheList()
    {
        // An Administrator reads every institution's items, so "The institution's own item" on each told them nothing
        // about whose it was. The read and the list name the owner; a national item has none.
        await using var dbContext = await SeededAsync();

        var read = (await ReadAsync(dbContext, Adopted, Caller.Administrator))!;
        var list = (await new GetCurriculaListQueryHandler(dbContext).Handle(
            new GetCurriculaListQuery(Principal(Caller.Administrator)), CancellationToken.None)).Single(row => row.Id == Adopted);

        foreach (var curriculum in new[] { read, list })
        {
            Owners(curriculum).Should().BeEquivalentTo(new Dictionary<int, string?>
            {
                [NationalItem] = null,
                [ALocalItem] = "Institution A",
                [ANationalEpaItem] = "Institution A",
                [BLocalItem] = "Institution B",
                [BNationalEpaItem] = "Institution B"
            });
        }
    }

    [Theory]
    [InlineData(Caller.InstitutionalAdminA)]
    // A CollegeAdmin who is also an InstitutionalAdmin reads only their institution's items, as an InstitutionalAdmin does.
    [InlineData(Caller.OtherCollegeAdminAndInstitutionalAdminA)]
    public async Task AnInstitutionalAdmin_IsNotToldTheirOwnInstitutionsName_EveryLocalItemTheyReadIsTheirs(Caller caller)
    {
        await using var dbContext = await SeededAsync();

        var read = (await ReadAsync(dbContext, Adopted, caller))!;

        read.Items.Where(item => item.IsLocal).Should().NotBeEmpty("guard: A's own items are read");
        read.Items.Should().OnlyContain(item => item.OwningInstitutionName == null,
            "the editor says \"Your institution's own item\": the name would tell them nothing");
    }

    [Theory]
    [InlineData(Caller.Administrator, true)]
    [InlineData(Caller.CollegeAdmin, true)]
    [InlineData(Caller.InstitutionalAdminA, false)]
    [InlineData(Caller.OtherCollegeAdminAndInstitutionalAdminA, false)]
    public void NamesItemOwners_ForACallerWhoseReadsCanSpanInstitutions(Caller caller, bool names)
        => CurriculumAdminScope.NamesItemOwners(Principal(caller)).Should().Be(names);

    [Fact]
    public async Task EachItemCommand_ReturnsTheOwnersNamed_AsTheReadDoes()
    {
        // The item editor redraws from what each command returns (CurriculumMappings.ToDtoAsync), so the names must not
        // vanish the moment an Administrator saves.
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
        }

        var administrator = Principal(Caller.Administrator);
        var expected = new Dictionary<int, string?>
        {
            [NationalItem] = null,
            [ALocalItem] = "Institution A",
            [ANationalEpaItem] = "Institution A",
            [BLocalItem] = "Institution B",
            [BNationalEpaItem] = "Institution B"
        };

        await using (var dbContext = CreateDbContext(databaseName))
        {
            var updated = await new UpdateCurriculumItemCommandHandler(dbContext).Handle(
                new UpdateCurriculumItemCommand(Adopted, ALocalItem, LocA01, 2, QuotaPeriod.AcademicYear, 3, 12, null, null, null, null, null, false, administrator),
                CancellationToken.None);
            Owners(updated).Should().BeEquivalentTo(expected);
        }

        await using (var dbContext = CreateDbContext(databaseName))
        {
            var removed = await new RemoveCurriculumItemCommandHandler(dbContext).Handle(
                new RemoveCurriculumItemCommand(Adopted, BLocalItem, administrator), CancellationToken.None);
            Owners(removed).Should().BeEquivalentTo(expected.Where(entry => entry.Key != BLocalItem).ToDictionary());
        }

        await using (var dbContext = CreateDbContext(databaseName))
        {
            var byInstitution = await new AddCurriculumItemCommandHandler(dbContext).Handle(
                new AddCurriculumItemCommand(Adopted, LocA02, 1, QuotaPeriod.AcademicYear, 3, 12, null, null, null, null, null, false, Principal(Caller.InstitutionalAdminA)),
                CancellationToken.None);
            byInstitution.Items.Should().OnlyContain(item => item.OwningInstitutionName == null, "institution A is told none");
        }
    }

    // ---- The list ----

    [Theory]
    [InlineData(Caller.Administrator, new[] { Adopted, NextVersion, OtherCollegesCurriculum })]
    [InlineData(Caller.CollegeAdmin, new[] { Adopted, NextVersion })]
    [InlineData(Caller.InstitutionalAdminA, new[] { Adopted })]
    [InlineData(Caller.InstitutionalAdminB, new[] { Adopted, NextVersion })]
    [InlineData(Caller.InstitutionalAdminC, new int[0])]
    [InlineData(Caller.CoordinatorA, new int[0])]
    [InlineData(Caller.OtherCollegeAdminAndInstitutionalAdminA, new[] { Adopted, OtherCollegesCurriculum })]
    public async Task TheList_ShowsOnlyCurriculaTheCallerCanOpen_EachAsTheReadShowsIt(Caller caller, int[] expected)
    {
        await using var dbContext = await SeededAsync();

        var list = await new GetCurriculaListQueryHandler(dbContext).Handle(
            new GetCurriculaListQuery(Principal(caller)), CancellationToken.None);

        list.Select(curriculum => curriculum.Id).Should().BeEquivalentTo(expected);

        // Each row's links lead to a page that opens: the read admits every curriculum the list shows, and shows it the
        // same way, down to which items are offered for editing. The items page also loads its Add form's EPA picker, and
        // shows a load error if that is refused, so a row opens only if the picker answers as well.
        foreach (var row in list)
        {
            var read = await ReadAsync(dbContext, row.Id, caller);
            read.Should().NotBeNull($"curriculum {row.Id} is on the list, so its Items link must open");
            row.CanEditCurriculum.Should().Be(read!.CanEditCurriculum);
            Editability(row).Should().BeEquivalentTo(Editability(read));

            var addPicker = () => new ListCurriculumItemEpaOptionsQueryHandler(dbContext).Handle(
                new ListCurriculumItemEpaOptionsQuery(row.Id, null, Principal(caller)), CancellationToken.None);
            await addPicker.Should().NotThrowAsync($"curriculum {row.Id}'s items page loads its Add form's EPA picker");
        }
    }

    [Fact]
    public async Task ACollegeAdminElsewhere_WhoIsInstitutionalAdminHere_AddsTheirInstitutionsOwnItem()
    {
        // Their CollegeAdmin role writes the other College's national items, not this curriculum's. On a curriculum their
        // institution adopted they are its InstitutionalAdmin, and the Add form, its picker and the command all make an
        // item of the institution's own. Decided by role alone, the CollegeAdmin role made the new item national, and the
        // picker and the command refused it on a page the list had offered.
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
        }

        var principal = Principal(Caller.OtherCollegeAdminAndInstitutionalAdminA);
        await using (var dbContext = CreateDbContext(databaseName))
        {
            var read = (await ReadAsync(dbContext, Adopted, Caller.OtherCollegeAdminAndInstitutionalAdminA))!;
            read.CanEditCurriculum.Should().BeFalse("the item editor's Add heading reads this: an institution's own item");
            Editability(read).Should().BeEquivalentTo(new Dictionary<int, bool>
            {
                [NationalItem] = false,
                [ALocalItem] = true,
                [ANationalEpaItem] = true
            });

            var offered = await new ListCurriculumItemEpaOptionsQueryHandler(dbContext).Handle(
                new ListCurriculumItemEpaOptionsQuery(Adopted, null, principal), CancellationToken.None);
            // An item of A's own names a national EPA or one of A's, never B's (T195); and not one the curriculum already
            // holds for A's trainees (T222, T223): PAED-001 is the national item's, PAED-002 and LOC-A01 A's own items'.
            // PAED-003 is B's own item's, which no trainee of A's reads.
            offered.Select(epa => epa.Code).Should().Equal(["LOC-A02", "PAED-003"],
                "of the national EPAs and A's own, only LOC-A02 and PAED-003 are not yet on the curriculum for A");

            var added = await new AddCurriculumItemCommandHandler(dbContext).Handle(
                new AddCurriculumItemCommand(Adopted, LocA02, 1, QuotaPeriod.AcademicYear, 3, 12, null, null, null, null, null, false, principal),
                CancellationToken.None);
            added.Items.Single(item => item.EpaId == LocA02).OwningInstitutionId.Should().Be(InstitutionA);
            added.CanEditCurriculum.Should().BeFalse();
        }

        await using (var dbContext = CreateDbContext(databaseName))
        {
            (await dbContext.Set<CurriculumItem>().SingleAsync(item => item.EpaId == LocA02)).OwningInstitutionId
                .Should().Be(InstitutionA, "guard: stored as A's own");

            // On their own College's curriculum the CollegeAdmin role decides: the Add form makes a national item.
            (await ReadAsync(dbContext, OtherCollegesCurriculum, Caller.OtherCollegeAdminAndInstitutionalAdminA))!
                .CanEditCurriculum.Should().BeTrue();
        }
    }

    [Fact]
    public async Task TheList_OffersAnInstitutionalAdminNoCurriculumToEdit()
    {
        await using var dbContext = await SeededAsync();

        var list = await new GetCurriculaListQueryHandler(dbContext).Handle(
            new GetCurriculaListQuery(Principal(Caller.InstitutionalAdminA)), CancellationToken.None);

        list.Should().OnlyContain(curriculum => !curriculum.CanEditCurriculum,
            "the list offers Edit only where the update and clone commands would accept it");
        list.Single().Items.Select(item => item.Id).Should().BeEquivalentTo([NationalItem, ALocalItem, ANationalEpaItem],
            "the list's item count is the items the caller reads");
    }

    // ---- What the commands hand back ----

    [Fact]
    public async Task EachItemCommand_ReturnsTheCurriculumCutToTheCaller()
    {
        // The item editor redraws from what each command returns, so a command that returned every item would put
        // institution B's item, and an Edit on the national one, in front of institution A the moment it saved.
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
        }

        var principal = Principal(Caller.InstitutionalAdminA);
        await using (var dbContext = CreateDbContext(databaseName))
        {
            var added = await new AddCurriculumItemCommandHandler(dbContext).Handle(
                new AddCurriculumItemCommand(Adopted, LocA02, 1, QuotaPeriod.AcademicYear, 3, 12, null, null, null, null, null, false, principal),
                CancellationToken.None);

            var newItem = added.Items.Single(item => item.EpaId == LocA02);
            newItem.OwningInstitutionId.Should().Be(InstitutionA);
            newItem.CanEdit.Should().BeTrue();
            AssertCutToInstitutionA(added);
        }

        await using (var dbContext = CreateDbContext(databaseName))
        {
            var updated = await new UpdateCurriculumItemCommandHandler(dbContext).Handle(
                new UpdateCurriculumItemCommand(Adopted, ALocalItem, LocA01, 2, QuotaPeriod.AcademicYear, 3, 12, null, null, null, null, null, false, principal),
                CancellationToken.None);

            AssertCutToInstitutionA(updated);
        }

        await using (var dbContext = CreateDbContext(databaseName))
        {
            var removed = await new RemoveCurriculumItemCommandHandler(dbContext).Handle(
                new RemoveCurriculumItemCommand(Adopted, ALocalItem, principal), CancellationToken.None);

            removed.Items.Should().NotContain(item => item.Id == ALocalItem);
            AssertCutToInstitutionA(removed);
        }

        await using (var dbContext = CreateDbContext(databaseName))
        {
            var byCollege = await new UpdateCurriculumItemCommandHandler(dbContext).Handle(
                new UpdateCurriculumItemCommand(Adopted, NationalItem, Paed001, 2, QuotaPeriod.AcademicYear, 3, 12, null, null, null, null, null, false, Principal(Caller.CollegeAdmin)),
                CancellationToken.None);

            byCollege.CanEditCurriculum.Should().BeTrue();
            Editability(byCollege).Should().BeEquivalentTo(new Dictionary<int, bool> { [NationalItem] = true });
        }
    }

    // ---- An EPA already on the curriculum, as an item the caller does not read ----

    [Fact]
    public async Task AddingAnEpaAnInstitutionHasOnTheCurriculumAsItsOwn_SaysSo_WithoutNamingTheInstitution()
    {
        // The College cannot see institution A's item naming PAED-002, so "already contains" alone would point at a list
        // that does not show it. The refusal says why, and names neither A nor the item. A national item beside A's would
        // measure A's trainees against PAED-002 twice (T223).
        await using var dbContext = await SeededAsync();

        var act = () => new AddCurriculumItemCommandHandler(dbContext).Handle(
            new AddCurriculumItemCommand(Adopted, Paed002, 1, QuotaPeriod.AcademicYear, 3, 12, null, null, null, null, null, false, Principal(Caller.CollegeAdmin)),
            CancellationToken.None);

        var refusal = (await act.Should().ThrowAsync<InvalidOperationException>()).Which;
        refusal.Message.Should().Be(
            "This curriculum already contains the selected EPA, as an institution's own item. A national item cannot share an EPA with an institution's own item: that institution's trainees would be measured against the EPA twice.");
        refusal.Message.Should().NotContain("Institution A").And.NotContain(ANationalEpaItem.ToString(CultureInfo.InvariantCulture));
        dbContext.ChangeTracker.Entries<CurriculumItem>().Should().NotContain(entry => entry.State == EntityState.Added);
    }

    [Theory]
    // A's own item holds PAED-002 for A.
    [InlineData(Caller.InstitutionalAdminA, "This curriculum already contains the selected EPA.")]
    // An Administrator adds a national item, which A's own item holds PAED-002 from (T223).
    [InlineData(Caller.Administrator, "This curriculum already contains the selected EPA, as an institution's own item. A national item cannot share an EPA with an institution's own item: that institution's trainees would be measured against the EPA twice.")]
    public async Task AddingAnEpaOnAnItemTheCallerReads_IsRefused_SayingWhichKindOfItemHoldsIt(Caller caller, string message)
    {
        await using var dbContext = await SeededAsync();

        var act = () => new AddCurriculumItemCommandHandler(dbContext).Handle(
            new AddCurriculumItemCommand(Adopted, Paed002, 1, QuotaPeriod.AcademicYear, 3, 12, null, null, null, null, null, false, Principal(caller)),
            CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(message);
    }

    [Theory]
    // The national item holds PAED-001 for A's trainees too (T223).
    [InlineData(Paed001, "This curriculum already contains the selected EPA, as a national item. An institution's own item adds an EPA to the national curriculum and cannot repeat one on it.")]
    // A's other own item holds LOC-A01.
    [InlineData(LocA01, "This curriculum already contains the selected EPA.")]
    public async Task MovingAnItemOntoAnEpaAlreadyOnTheCurriculumForItsInstitution_IsRefused(int epaId, string refusal)
    {
        await using var dbContext = await SeededAsync();

        var act = () => new UpdateCurriculumItemCommandHandler(dbContext).Handle(
            new UpdateCurriculumItemCommand(Adopted, ANationalEpaItem, epaId, 1, QuotaPeriod.AcademicYear, 3, 12, null, null, null, null, null, false, Principal(Caller.InstitutionalAdminA)),
            CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(refusal);
        dbContext.ChangeTracker.Entries<CurriculumItem>().Should().NotContain(entry => entry.State == EntityState.Modified);
    }

    [Fact]
    public async Task MovingAnItemOntoAnEpaAnotherInstitutionHasAsItsOwn_IsStored()
    {
        // B's own item, left from B's earlier adoption, holds PAED-003 for B's trainees alone. Before T223 one index held one
        // item per EPA whoever owned it, so A was refused ("as an institution's own item") an EPA it could not see held.
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
            var updated = await new UpdateCurriculumItemCommandHandler(dbContext).Handle(
                new UpdateCurriculumItemCommand(Adopted, ANationalEpaItem, Paed003, 1, QuotaPeriod.AcademicYear, 3, 12, null, null, null, null, null, false, Principal(Caller.InstitutionalAdminA)),
                CancellationToken.None);
            updated.Items.Single(item => item.Id == ANationalEpaItem).EpaId.Should().Be(Paed003);
            updated.Items.Should().NotContain(item => item.Id == BNationalEpaItem, "B's item is B's business, as before");
        }

        await using var readContext = CreateDbContext(databaseName);
        (await readContext.Set<CurriculumItem>().Where(item => item.EpaId == Paed003).Select(item => item.OwningInstitutionId).ToListAsync())
            .Should().BeEquivalentTo(new int?[] { InstitutionA, InstitutionB });
    }

    // ---- helpers ----

    private static void AssertCutToInstitutionA(CurriculumDto curriculum)
    {
        curriculum.CanEditCurriculum.Should().BeFalse();
        curriculum.Items.Should().NotContain(item => item.OwningInstitutionId == InstitutionB, "B's own items are B's");
        curriculum.Items.Single(item => item.Id == NationalItem).CanEdit.Should().BeFalse();
        curriculum.Items.Where(item => item.IsLocal).Should().OnlyContain(item => item.CanEdit);
    }

    private static Dictionary<int, bool> Editability(CurriculumDto curriculum)
        => curriculum.Items.ToDictionary(item => item.Id, item => item.CanEdit);

    private static Dictionary<int, string?> Owners(CurriculumDto curriculum)
        => curriculum.Items.ToDictionary(item => item.Id, item => item.OwningInstitutionName);

    private static Task<CurriculumDto?> ReadAsync(ApplicationDbContext dbContext, int curriculumId, Caller caller)
        => new GetCurriculumByIdQueryHandler(dbContext).Handle(
            new GetCurriculumByIdQuery(curriculumId, Principal(caller)), CancellationToken.None);

    private static ClaimsPrincipal Principal(Caller caller) => caller switch
    {
        Caller.Administrator => TestPrincipals.Administrator(),
        Caller.CollegeAdmin => TestPrincipals.CollegeAdmin(CollegeId),
        Caller.OtherCollegeAdmin => TestPrincipals.CollegeAdmin(OtherCollegeId),
        Caller.InstitutionalAdminA => TestPrincipals.InstitutionalAdmin(InstitutionA),
        Caller.InstitutionalAdminB => TestPrincipals.InstitutionalAdmin(InstitutionB),
        Caller.InstitutionalAdminC => TestPrincipals.InstitutionalAdmin(InstitutionC),
        Caller.CoordinatorA => TestPrincipals.Coordinator(InstitutionA),
        Caller.OtherCollegeAdminAndInstitutionalAdminA => new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "college-and-institutional-admin"),
                new Claim(ClaimTypes.Role, WombatRoles.CollegeAdmin),
                new Claim(ClaimTypes.Role, WombatRoles.InstitutionalAdmin),
                new Claim(WombatClaimTypes.CollegeId, OtherCollegeId.ToString(CultureInfo.InvariantCulture)),
                new Claim(WombatClaimTypes.InstitutionId, InstitutionA.ToString(CultureInfo.InvariantCulture))
            ],
            "test")),
        Caller.CollegeAdminAtA => new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "college-admin-at-a"),
                new Claim(ClaimTypes.Role, WombatRoles.CollegeAdmin),
                new Claim(WombatClaimTypes.CollegeId, CollegeId.ToString(CultureInfo.InvariantCulture)),
                new Claim(WombatClaimTypes.InstitutionId, InstitutionA.ToString(CultureInfo.InvariantCulture))
            ],
            "test")),
        _ => throw new ArgumentOutOfRangeException(nameof(caller))
    };

    private static ApplicationDbContext CreateDbContext(string databaseName)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options);

    private static async Task<ApplicationDbContext> SeededAsync()
    {
        var dbContext = CreateDbContext(Guid.NewGuid().ToString());
        await SeedAsync(dbContext);
        return dbContext;
    }

    private static async Task SeedAsync(ApplicationDbContext dbContext)
    {
        var college = new College { Id = CollegeId, Name = "CPSA", ShortCode = "CPSA" };
        var otherCollege = new College { Id = OtherCollegeId, Name = "CoS", ShortCode = "COS" };
        var speciality = new Speciality { Id = 1, Name = "Paediatrics", CollegeId = CollegeId, College = college };
        var otherSpeciality = new Speciality { Id = 2, Name = "Surgery", CollegeId = OtherCollegeId, College = otherCollege };
        var subSpeciality = new SubSpeciality { Id = 1, Name = "General Paediatrics", SpecialityId = 1, Speciality = speciality };
        var otherSubSpeciality = new SubSpeciality { Id = 2, Name = "General Surgery", SpecialityId = 2, Speciality = otherSpeciality };

        dbContext.Colleges.AddRange(college, otherCollege);
        dbContext.Specialities.AddRange(speciality, otherSpeciality);
        dbContext.SubSpecialities.AddRange(subSpeciality, otherSubSpeciality);
        dbContext.Institutions.AddRange(
            new Institution { Id = InstitutionA, Name = "Institution A", ShortCode = "IA", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = InstitutionB, Name = "Institution B", ShortCode = "IB", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = InstitutionC, Name = "Institution C", ShortCode = "IC", IsActive = true, CreatedOn = DateTime.UtcNow });

        dbContext.Epas.AddRange(
            new Epa { Id = Paed001, Code = "PAED-001", Title = "National one", SubSpecialityId = 1 },
            new Epa { Id = Paed002, Code = "PAED-002", Title = "National two", SubSpecialityId = 1 },
            new Epa { Id = Paed003, Code = "PAED-003", Title = "National three", SubSpecialityId = 1 },
            new Epa { Id = LocA01, Code = "LOC-A01", Title = "A's first", SubSpecialityId = 1, OwningInstitutionId = InstitutionA },
            new Epa { Id = LocA02, Code = "LOC-A02", Title = "A's second", SubSpecialityId = 1, OwningInstitutionId = InstitutionA },
            new Epa { Id = LocB01, Code = "LOC-B01", Title = "B's first", SubSpecialityId = 1, OwningInstitutionId = InstitutionB });

        var adopted = Curriculum(Adopted, subSpeciality, "11.1");
        adopted.Items.Add(Item(NationalItem, Paed001, owner: null));
        adopted.Items.Add(Item(ALocalItem, LocA01, InstitutionA));
        adopted.Items.Add(Item(BLocalItem, LocB01, InstitutionB));
        adopted.Items.Add(Item(ANationalEpaItem, Paed002, InstitutionA));
        adopted.Items.Add(Item(BNationalEpaItem, Paed003, InstitutionB));

        var nextVersion = Curriculum(NextVersion, subSpeciality, "12.0");
        nextVersion.Items.Add(Item(7100, Paed001, owner: null));

        var otherColleges = Curriculum(OtherCollegesCurriculum, otherSubSpeciality, "1.0");

        dbContext.Curricula.AddRange(adopted, nextVersion, otherColleges);

        dbContext.InstitutionCurriculumAdoptions.AddRange(
            Adoption(1, InstitutionA, Adopted, isActive: true),
            Adoption(2, InstitutionB, Adopted, isActive: false),
            Adoption(3, InstitutionB, NextVersion, isActive: true),
            Adoption(4, InstitutionC, NextVersion, isActive: false));

        await dbContext.SaveChangesAsync();
    }

    private static Curriculum Curriculum(int id, SubSpeciality subSpeciality, string version)
        => new()
        {
            Id = id,
            SubSpecialityId = subSpeciality.Id,
            SubSpeciality = subSpeciality,
            Name = "EPA Curriculum",
            Version = version,
            EffectiveFrom = new DateOnly(2026, 1, 1),
            IsActive = true
        };

    private static CurriculumItem Item(int id, int epaId, int? owner)
        => new()
        {
            Id = id,
            EpaId = epaId,
            OwningInstitutionId = owner,
            RequiredCount = 3,
            QuotaPeriod = QuotaPeriod.Semester,
            MinimumLevelOrder = 3,
            WindowMonths = 12
        };

    private static InstitutionCurriculumAdoption Adoption(int id, int institutionId, int curriculumId, bool isActive)
        => new()
        {
            Id = id,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            SubSpecialityId = 1,
            AdoptedOn = new DateOnly(2026, 1, 1),
            IsActive = isActive
        };
}
