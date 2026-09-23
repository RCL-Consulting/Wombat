using FluentAssertions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;

namespace Wombat.Application.Tests.Features.Curricula;

public sealed class CurriculumCloneTests
{
    [Fact]
    public void CloneAsNewVersion_CopiesMetadataAndItemsIntoANewAggregate()
    {
        var curriculum = new Curriculum
        {
            Id = 10,
            SubSpecialityId = 3,
            Name = "IM Core Curriculum",
            Version = "2026.1",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            Items =
            [
                new CurriculumItem
                {
                    Id = 21,
                    EpaId = 100,
                    Epa = new Epa { Id = 100, Code = "EPA-001", Title = "Admit a patient" },
                    RequiredCount = 3,
                    QuotaPeriod = QuotaPeriod.Semester,
                    MinimumLevelOrder = 4,
                    WindowMonths = 12,
                    Weight = 1.5,
                    ScaleId = 7
                }
            ]
        };

        var clone = curriculum.CloneAsNewVersion("2026.2", new DateOnly(2026, 7, 1), null);

        clone.Id.Should().Be(0);
        clone.SubSpecialityId.Should().Be(curriculum.SubSpecialityId);
        clone.Name.Should().Be(curriculum.Name);
        clone.Version.Should().Be("2026.2");
        clone.EffectiveFrom.Should().Be(new DateOnly(2026, 7, 1));
        clone.Items.Should().HaveCount(1);
        clone.Items.Single().Should().NotBeSameAs(curriculum.Items.Single());
        clone.Items.Single().EpaId.Should().Be(100);
        clone.Items.Single().RequiredCount.Should().Be(3);
        // T130: RequiredCount is a target per this window. The source is Semester, not the zero value, because
        // a clone that dropped the field would land on AcademicYear and turn "3 per semester" into "3 per year"
        // for every trainee on the new version — and an AcademicYear source could not tell the two apart.
        clone.Items.Single().QuotaPeriod.Should().Be(QuotaPeriod.Semester);
        clone.Items.Single().MinimumLevelOrder.Should().Be(4);
        clone.Items.Single().WindowMonths.Should().Be(12);
        clone.Items.Single().Weight.Should().Be(1.5);
        // T109: the cloned minima are the same numbers on the same ladder. Dropping the pin here would
        // silently unpin every item of every new curriculum version and regenerate the defect one version
        // at a time — the kind of omission that is invisible until a trainee is mis-credited.
        clone.Items.Single().ScaleId.Should().Be(7);
    }

    [Fact]
    public void CloneAsNewVersion_WhenTheSourceItemIsUnpinned_LeavesTheCloneUnpinned()
    {
        var curriculum = new Curriculum
        {
            Id = 10,
            SubSpecialityId = 3,
            Name = "IM Core Curriculum",
            Version = "2026.1",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            Items =
            [
                new CurriculumItem
                {
                    Id = 21,
                    EpaId = 100,
                    RequiredCount = 3,
                    MinimumLevelOrder = 4,
                    WindowMonths = 12,
                    ScaleId = null
                }
            ]
        };

        var clone = curriculum.CloneAsNewVersion("2026.2", new DateOnly(2026, 7, 1), null);

        clone.Items.Single().ScaleId.Should().BeNull("cloning must not invent a ladder the source never had");
    }

    /// <summary>
    /// T122: the tool list is the fourth cell of the same Annexure A row as the target, the minima and the ladder. A clone
    /// that dropped it would silently let every instrument credit every EPA again, one curriculum version at a time, and
    /// nothing would look wrong until a Mini-CEX credited PAED-005.
    /// </summary>
    /// <remarks>
    /// Compared through <see cref="CurriculumItem.ParsePermittedTools" />, not as a string: the column is <c>jsonb</c>, and
    /// the house rule is canonical-to-canonical everywhere, InMemory tests included, so a test written here cannot be
    /// copied into a Postgres fixture and start failing on Postgres's re-rendering.
    /// </remarks>
    [Fact]
    public void CloneAsNewVersion_CarriesEachItemsToolList()
    {
        var curriculum = new Curriculum
        {
            Id = 10,
            SubSpecialityId = 3,
            Name = "Paediatric EPA Curriculum",
            Version = "11.1",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            Items =
            [
                new CurriculumItem
                {
                    Id = 21,
                    EpaId = 100,
                    RequiredCount = 3,
                    MinimumLevelOrder = 4,
                    WindowMonths = 12,
                    PermittedToolsJson = "[\"cbd\",\"dops\",\"msf\"]"
                },
                new CurriculumItem
                {
                    Id = 22,
                    EpaId = 101,
                    RequiredCount = 2,
                    MinimumLevelOrder = 3,
                    WindowMonths = 12,
                    PermittedToolsJson = "[\"cbd\",\"mini_cex\",\"msf\"]"
                }
            ]
        };

        var clone = curriculum.CloneAsNewVersion("11.2", new DateOnly(2027, 1, 1), null);

        clone.Items.Should().HaveCount(2);
        var clonedFirst = clone.Items.Single(item => item.EpaId == 100);
        var clonedSecond = clone.Items.Single(item => item.EpaId == 101);

        CurriculumItem.ParsePermittedTools(clonedFirst.PermittedToolsJson)
            .Should().Equal(new[] { "cbd", "dops", "msf" }, "each item keeps its own list, not its neighbour's");
        CurriculumItem.ParsePermittedTools(clonedSecond.PermittedToolsJson)
            .Should().Equal(new[] { "cbd", "mini_cex", "msf" });
        clonedFirst.PermittedToolsJson.Should().NotBeNull("a carried list must not decay into 'any instrument'");
    }

    /// <summary>
    /// Null is the permanent, meaningful "any instrument" (D21). The clone must not invent a restriction the source never
    /// had, and must not write the <c>[]</c> spelling the writer never stores.
    /// </summary>
    [Fact]
    public void CloneAsNewVersion_WhenTheSourceItemHasNoToolList_LeavesTheCloneUnrestricted()
    {
        var curriculum = new Curriculum
        {
            Id = 10,
            SubSpecialityId = 3,
            Name = "IM Core Curriculum",
            Version = "2026.1",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            Items =
            [
                new CurriculumItem
                {
                    Id = 21,
                    EpaId = 100,
                    RequiredCount = 3,
                    MinimumLevelOrder = 4,
                    WindowMonths = 12,
                    PermittedToolsJson = null
                }
            ]
        };

        var clone = curriculum.CloneAsNewVersion("2026.2", new DateOnly(2026, 7, 1), null);

        clone.Items.Single().PermittedToolsJson.Should().BeNull("cloning must not invent a restriction the source never had");
    }

    /// <summary>
    /// T091 still holds with the new column: institution-local items belong to the institution, not to the College's
    /// published version, so they are not cloned, and their tool lists do not ride along or bleed into a national item.
    /// </summary>
    [Fact]
    public void CloneAsNewVersion_StillSkipsInstitutionLocalItems_AndTheirToolLists()
    {
        var curriculum = new Curriculum
        {
            Id = 10,
            SubSpecialityId = 3,
            Name = "Paediatric EPA Curriculum",
            Version = "11.1",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            Items =
            [
                new CurriculumItem
                {
                    Id = 21,
                    EpaId = 100,
                    RequiredCount = 3,
                    MinimumLevelOrder = 4,
                    WindowMonths = 12,
                    PermittedToolsJson = "[\"cbd\",\"dops\",\"msf\"]"
                },
                new CurriculumItem
                {
                    Id = 22,
                    EpaId = 900,
                    OwningInstitutionId = 5,
                    RequiredCount = 1,
                    MinimumLevelOrder = 3,
                    WindowMonths = 12,
                    PermittedToolsJson = "[\"mini_cex\"]"
                },
                new CurriculumItem
                {
                    Id = 23,
                    EpaId = 901,
                    OwningInstitutionId = 6,
                    RequiredCount = 1,
                    MinimumLevelOrder = 3,
                    WindowMonths = 12,
                    PermittedToolsJson = null
                }
            ]
        };

        var clone = curriculum.CloneAsNewVersion("11.2", new DateOnly(2027, 1, 1), null);

        clone.Items.Should().ContainSingle("only the national core is part of the College's published version");
        var national = clone.Items.Single();
        national.EpaId.Should().Be(100);
        national.OwningInstitutionId.Should().BeNull();
        CurriculumItem.ParsePermittedTools(national.PermittedToolsJson)
            .Should().Equal(new[] { "cbd", "dops", "msf" }, "a local item's list must not bleed into the national one");
    }
}
