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
                    MinimumLevelOrder = 4,
                    WindowMonths = 12,
                    Weight = 1.5
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
        clone.Items.Single().MinimumLevelOrder.Should().Be(4);
        clone.Items.Single().WindowMonths.Should().Be(12);
        clone.Items.Single().Weight.Should().Be(1.5);
    }
}
