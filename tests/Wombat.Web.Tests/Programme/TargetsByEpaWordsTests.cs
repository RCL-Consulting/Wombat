using FluentAssertions;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Curricula;
using Wombat.Web.Components.Shared.Programme;
using static Wombat.Web.Tests.Programme.ProgrammeWordsFixtures;

namespace Wombat.Web.Tests.Programme;

/// <summary>
/// T358 (flow 06, lane A2; Q5, review 13): Home's Targets by EPA, in the words R2-Home gives it: the programme's figure
/// over its caption, the one exemption wording, the cadence line with a local extra's owner, the hidden tail of each link.
/// </summary>
public sealed class TargetsByEpaWordsTests
{
    private static readonly EpaTargetCoverage Paed001 =
        new(1, "PAED-001", "Providing paediatric emergency care to children", QuotaPeriod.Semester, 3, 2, 5, 0);

    private static readonly EpaTargetCoverage Paed008 =
        new(8, "PAED-008", "Evaluating and managing neurodevelopmental and behavioural presentations in children", QuotaPeriod.AcademicYear, 1, 0, 5, 0);

    private static readonly EpaTargetCoverage Kgk001 =
        new(21, "KGK-001", "Running a paediatric outreach clinic at a district hospital", QuotaPeriod.AcademicYear, 1, 0, 2, 0)
        {
            OwningInstitutionName = "Kgosi Kgari Teaching Hospital"
        };

    private static readonly EpaTargetCoverage MetByAll =
        new(7, "PAED-007", "Maintaining and promoting the health and well-being of children", QuotaPeriod.Semester, 1, 8, 8, 0);

    private static readonly EpaTargetCoverage AllExempt =
        new(3, "PAED-003", "Providing intensive care to children", QuotaPeriod.Semester, 3, 0, 0, 4);

    [Fact]
    public void TheFigure_IsRegistrarsMet_OverItsWindow()
    {
        var coverage = Coverage(0, Paed001, Paed008);

        TargetsByEpaWords.Figure(Paed001, coverage).Should().Be(("2 of 5", "registrars met this semester"));
        TargetsByEpaWords.Figure(Paed008, coverage).Should().Be(("0 of 5", "registrars met in 2026"));
        TargetsByEpaWords.Figure(AllExempt, coverage).Should().Be(("All exempt", "this period"));
    }

    [Fact]
    public void TheRule_HasTheOneExemptionWording()
    {
        TargetsByEpaWords.Rule(Coverage()).Should().Be("Fewest registrars met first. Semester 2, 2026 ends on 2026-11-30.");
        TargetsByEpaWords.Rule(Coverage(exempt: 1)).Should().Be(
            "1 registrar exempt this period, not counted. Fewest registrars met first. Semester 2, 2026 ends on 2026-11-30.");
        TargetsByEpaWords.Rule(Coverage(exempt: 3)).Should().StartWith("3 registrars exempt this period, not counted. ");
    }

    [Fact]
    public void TheCadence_NamesALocalExtrasOwner_AndAnEpaEveryoneHasMet()
    {
        TargetsByEpaWords.Cadence(Paed001).Should().Be("3 per semester");
        TargetsByEpaWords.Cadence(Paed008).Should().Be("1 per academic year");
        TargetsByEpaWords.Cadence(Kgk001).Should().Be("1 per academic year · Kgosi Kgari Teaching Hospital's own");
        TargetsByEpaWords.Cadence(MetByAll).Should().Be("1 per semester · every registrar has met it");

        TargetsByEpaWords.IsMetByAll(MetByAll).Should().BeTrue();
        TargetsByEpaWords.IsMetByAll(Paed001).Should().BeFalse();
        TargetsByEpaWords.IsMetByAll(AllExempt).Should().BeFalse("nobody holds it this period, so nobody has met it");
    }

    [Fact]
    public void TheLinksHiddenTail_SaysTheFigure_AndWhereItGoes()
    {
        var coverage = Coverage(0, Paed001, Paed008);

        TargetsByEpaWords.LinkTail(Paed001, coverage).Should().Be(": 2 of 5 registrars met this semester. Show the registrars short on it.");
        TargetsByEpaWords.LinkTail(Paed008, coverage).Should().Be(": 0 of 5 registrars met in 2026. Show the registrars short on it.");
        TargetsByEpaWords.Name(Paed001).Should().Be("PAED-001 — Providing paediatric emergency care to children");
    }

    [Fact]
    public void TheFixedWords_AndNoPronoun()
    {
        TargetsByEpaWords.Title.Should().Be("Targets by EPA");
        TargetsByEpaWords.Empty.Should().Be("No targets this period: there is no current registrar.");

        var coverage = Coverage(1, Paed001);
        Constants(typeof(TargetsByEpaWords))
            .Concat([TargetsByEpaWords.Rule(coverage), TargetsByEpaWords.LinkTail(Paed001, coverage), TargetsByEpaWords.Cadence(Kgk001)])
            .Should().NotContain(phrase => NamesAPersonByPronoun(phrase) || phrase.Contains('%'));
    }
}
