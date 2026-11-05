using FluentAssertions;
using Wombat.Application.Features.Curricula;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T394 (flow 16, Q2, EV2, S16, T8, build-contracts § 0): the reach in words, as the boards write it (R3-D05, D06, E05,
/// E07, E11), with the singulars every count needs.
/// </summary>
public sealed class CatalogueReachWordsTests
{
    private const string Kgk = "Kgosi Kgari Teaching Hospital";

    [Fact]
    public void AnAdoptedVersion_ReadsAsTheBoardsWriteIt()
        => CatalogueReachWords.ForCurriculum(new CurriculumReachDto([Kgk], 3, [], 0))
            .Should().Be("Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it.");

    [Fact]
    public void AVersionNobodyTrainsAgainst_ReadsTheNoneLine()
        => CatalogueReachWords.ForCurriculum(CurriculumReachDto.None).Should().Be("No institution has adopted this version yet.");

    [Fact]
    public void ASupersededVersion_SaysWhoStillUsesIt_AfterWhoAdoptedIt()
        => CatalogueReachWords.ForCurriculum(new CurriculumReachDto(["Other Hospital"], 1, [Kgk], 2))
            .Should().Be("Adopted by 1 institution, Other Hospital; 1 registrar in training on it. " +
                         "Still used by 1 institution that has superseded it, Kgosi Kgari Teaching Hospital; 2 registrars in training on it.");

    [Fact]
    public void SeveralInstitutions_AreJoinedWithAnd()
        => CatalogueReachWords.ForCurriculum(new CurriculumReachDto(["A", "B", "C"], 0, ["D", "E"], 5))
            .Should().Be("Adopted by 3 institutions, A, B and C; no registrar in training on it. " +
                         "Still used by 2 institutions that have superseded it, D and E; 5 registrars in training on it.");

    [Fact]
    public void ANationalEpa_ReadsAsTheBoardsWriteIt()
        => CatalogueReachWords.ForNationalEpa("PAED-012", new EpaReachDto(1, [Kgk], 3))
            .Should().Be("PAED-012 is on 1 curriculum adopted by 1 institution, Kgosi Kgari Teaching Hospital; " +
                         "3 registrars in training are measured against it.");

    [Fact]
    public void ANationalEpaOnNoAdoptedCurriculum_ReadsTheNoneLine()
        => CatalogueReachWords.ForNationalEpa("PAED-016", EpaReachDto.None)
            .Should().Be("PAED-016 is on no curriculum an institution has adopted, so no registrar is measured against it yet.");

    [Fact]
    public void ANationalEpaOnSeveralCurricula_CountsThemAndOneRegistrar()
        => CatalogueReachWords.ForNationalEpa("PAED-001", new EpaReachDto(2, [Kgk, "Other Hospital"], 1))
            .Should().Be("PAED-001 is on 2 curricula adopted by 2 institutions, Kgosi Kgari Teaching Hospital and Other Hospital; " +
                         "1 registrar in training is measured against it.");

    [Fact]
    public void AnOwnEpa_NamesItsOwner()
    {
        CatalogueReachWords.ForOwnEpa("KGK-001", Kgk, new EpaReachDto(1, [Kgk], 2))
            .Should().Be("KGK-001 is Kgosi Kgari Teaching Hospital's own item on 1 curriculum; 2 registrars in training are measured against it.");
        CatalogueReachWords.ForOwnEpa("KGK-002", Kgk, EpaReachDto.None)
            .Should().Be("KGK-002 is on no curriculum Kgosi Kgari Teaching Hospital trains against, so no registrar is measured against it yet.");
    }
}
