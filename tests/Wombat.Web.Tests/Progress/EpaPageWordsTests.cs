using FluentAssertions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.Curricula;
using Wombat.Web.Components.Shared.Progress;
using static Wombat.Web.Tests.Progress.ProgressFixtures;

namespace Wombat.Web.Tests.Progress;

/// <summary>T355: one EPA's page, every phrase Spec § 1 and the R3-C-Epa board give it, dates ISO (decision D1).</summary>
public sealed class EpaPageWordsTests
{
    private const string Paed012 = "Communicating with and counselling patients, caregivers and healthcare teams";

    [Fact]
    public void TheH1_TheTab_AndThePausedMark()
    {
        EpaPageWords.Title("PAED-001", "Providing paediatric emergency care to children")
            .Should().Be("PAED-001 — Providing paediatric emergency care to children");
        EpaPageWords.Tab("PAED-001", "Providing paediatric emergency care to children", inForce: true)
            .Should().Be("PAED-001 — Providing paediatric emergency care to children · Wombat");
        EpaPageWords.Tab("PAED-012", Paed012, inForce: false)
            .Should().Be($"PAED-012 — {Paed012} (no longer in use) · Wombat", "the tab is the h1's words, mark included (C5)");
    }

    [Fact]
    public void TheSubtitle_NamesTheTarget_TheCadence_AndTheExitLevel()
    {
        var semester = Item("PAED-001", QuotaPeriod.Semester, 3, Counting(Semester2Of2026, 3, 3)) with
        {
            DecisionCadence = QuotaPeriod.Semester
        };
        var opportunistic = Item("PAED-008", QuotaPeriod.AcademicYear, 1, Counting(Year2026, 0, 1)) with
        {
            DecisionCadence = QuotaPeriod.AcademicYear, DecisionIsOpportunistic = true, ExitLevelLabel = "4"
        };
        var local = Item("KGK-001", QuotaPeriod.AcademicYear, 1, Counting(Year2026, 0, 1)) with
        {
            OwningInstitutionName = "Kgosi Kgari Teaching Hospital", ExitLevelLabel = "3a"
        };

        EpaPageWords.Subtitle(semester).Should().Be("3 a semester · Decided each semester · Exit level 5");
        EpaPageWords.Subtitle(opportunistic).Should().Be("1 a year · Decided as opportunity allows · Exit level 4");
        EpaPageWords.Subtitle(local).Should().Be("1 a year · Not in the College's exit rule");
    }

    [Fact]
    public void TheLevelLine_NamesItsTrainingYear_OrIsTheItemsOwnMinimum()
    {
        var item = Item("PAED-001", QuotaPeriod.Semester, 3, Counting(Semester2Of2026, 3, 3));

        EpaPageWords.LevelLine(item, 4).Should().Be(
            "Training year 4: level 5, the minimum each encounter is judged against and your STAR's target.");
        EpaPageWords.LevelLine(item with { MinimumByTrainingYear = false, EffectiveMinimumLevelLabel = "3a" }, 3)
            .Should().Be("Minimum 3a", "KGK-001 has no per-year map (C4)");
        EpaPageWords.LevelLine(item, null).Should().Be("Minimum 5", "before the programme starts there is no training year");
    }

    [Fact]
    public void TheEntrustmentLines()
    {
        var decision = new StandingDecisionDto(1, 6, "5", null, new DateOnly(2026, 10, 3), null);
        EpaPageWords.Issued(decision).Should().Be("Issued 2026-10-03", "a STAR with no expiry says nothing of one (C4)");
        EpaPageWords.Issued(decision with { LevelOrder = 5, LevelLabel = "4", ExpiresOn = new DateOnly(2026, 10, 23) })
            .Should().Be("Issued 2026-10-03, expires 2026-10-23");

        EpaPageWords.ExitLine(Standing(EntrustmentStandingStatus.AtOrAbove)).Should().Be("Exit level 5 · reached");
        EpaPageWords.ExitLine(Standing(EntrustmentStandingStatus.Below)).Should().Be("Exit level 5 · not yet");
        EpaPageWords.ExitLine(Standing(EntrustmentStandingStatus.NoDecision)).Should().Be("Exit level 5 · not yet");

        EpaPageWords.PausedEntrustment("PAED-012").Should().Be("While PAED-012 is paused it is not in your standing against Annexure A.");
        EpaPageWords.NoStar.Should().Be("No STAR yet.");
    }

    [Fact]
    public void TheEmpties_AndThePagesLoadingAndErrorWords()
    {
        EpaPageWords.NoActivity.Should().Be("No activity on this EPA yet.");
        EpaPageWords.Loading.Should().Be("Loading this EPA");
        EpaPageWords.LoadingStatus.Should().Be("Loading this EPA.");
        EpaPageWords.ErrorHeading.Should().Be("EPA");
        EpaPageWords.LoadError.Should().Be("Could not load this EPA. Nothing has changed. Try again, or come back in a few minutes.");
    }

    [Fact]
    public void ProgressLinks_AddressTheEpaById_AndNameEachWayIn()
    {
        ProgressLinks.Epa(2).Should().Be("/portfolio/progress/2");
        ProgressLinks.OpenAt("PAED-001").Should().Be("Open My progress at PAED-001");
        ProgressLinks.CreditTo("1 item", "PAED-001").Should().Be("1 item to PAED-001, in My progress");
    }

    private static EpaStandingDto Standing(EntrustmentStandingStatus exit)
        => new(1, 2, "PAED-001", "Providing paediatric emergency care to children", "CPSA Paediatric Entrustment Scale v11.1",
            IsLocal: false, YearTargetOrder: 6, YearTargetLabel: "5", YearTargetIsExitLevel: false, ExitLevelOrder: 6,
            ExitLevelLabel: "5", Decision: null, YearStatus: exit, ExitStatus: exit, LatestRating: null);
}
