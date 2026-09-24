using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Credit;

namespace Wombat.Domain.Tests.Activities;

/// <summary>
/// D15's fourteen days (T160): day 14 is on time, day 15 is late. And the one predicate that decides which types the
/// credit-protecting rules (the programme-start bound, the lateness warning and record) apply to.
/// </summary>
public sealed class EncounterDatePolicyTests
{
    [Fact]
    public void TheCollegesFigureIsFourteenDays() => Assert.Equal(14, EncounterDatePolicy.LateFilingDays);

    [Theory]
    [InlineData(0, false)]
    [InlineData(14, false)]
    [InlineData(15, true)]
    [InlineData(200, true)]
    public void AFilingIsLateOnlyPastTheFourteenthDay(int daysAfterEncounter, bool late)
        => Assert.Equal(late, EncounterDatePolicy.IsLateFiling(daysAfterEncounter));

    [Fact]
    public void DaysAreCountedInWholeCalendarDays_AcrossAMonthEnd()
        => Assert.Equal(15, EncounterDatePolicy.DaysAfterEncounter(new DateOnly(2026, 8, 25), new DateOnly(2026, 9, 9)));

    [Fact]
    public void TheSameDayIsZero()
        => Assert.Equal(0, EncounterDatePolicy.DaysAfterEncounter(new DateOnly(2026, 9, 24), new DateOnly(2026, 9, 24)));

    // ---- which types the credit-protecting rules apply to ----------------------------------------------------------

    [Fact]
    public void RulesWithACountsForDirective_CanCredit()
        => Assert.True(EncounterDatePolicy.CanCredit(
            """{ "counts_for": [ { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1 } ] }"""));

    [Fact]
    public void RulesWithAnEmptyCountsFor_CannotCredit()
        => Assert.False(EncounterDatePolicy.CanCredit("""{ "counts_for": [] }"""));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BlankRules_CannotCredit(string? creditRulesJson)
        => Assert.False(EncounterDatePolicy.CanCredit(creditRulesJson));

    [Fact]
    public void MalformedRules_AreACorruptRow_NotATypeThatCreditsNothing()
        => Assert.Throws<CreditRulesParseException>(() => EncounterDatePolicy.CanCredit("""{ "counts_for": "all" }"""));
}
