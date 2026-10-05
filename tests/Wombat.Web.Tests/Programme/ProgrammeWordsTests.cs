using FluentAssertions;
using Wombat.Application.Features.Programme.Trainees;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Shared.Programme;
using static Wombat.Web.Tests.Programme.ProgrammeWordsFixtures;

namespace Wombat.Web.Tests.Programme;

/// <summary>
/// T358 (flow 06, lane A2): every phrase R2-Trainees and R2-Home give the programme's registrars, singular and plural,
/// dates ISO, no pronoun for a person; and the three pages' addresses (D3).
/// </summary>
public sealed class ProgrammeWordsTests
{
    [Fact]
    public void TheSubtitle_SaysWhatWasRead_AsWhichRole()
    {
        ProgrammeWords.Subtitle(Kgk, "Semester 2, 2026").Should().Be(
            "Current registrars at Kgosi Kgari Teaching Hospital, read as Committee member · Semester 2, 2026");
        ProgrammeWords.Subtitle(Paediatrics, "Semester 2, 2026").Should().Be(
            "Current registrars in Paediatrics, read as Sub-speciality admin · Semester 2, 2026");
        ProgrammeWords.Subtitle(Paediatrics with { ActingRole = WombatRoles.SpecialityAdmin }, "Semester 2, 2026")
            .Should().Contain("read as Speciality admin").And.NotContain("SpecialityAdmin", "the role's label, never its key (Q10)");
    }

    [Fact]
    public void ARowsFigures_AreMyProgresss()
    {
        var dlamini = Row("Anele Dlamini", trainingYear: 3, semesterMet: 1);

        ProgrammeWords.SemesterFigure(dlamini).Should().Be(("1 of 10", "EPAs met this semester"));
        ProgrammeWords.YearFigure(dlamini).Should().Be(("0 of 5", "EPAs met in 2026"));
        ProgrammeWords.TrainingYear(dlamini).Should().Be("Training year 3");
        ProgrammeWords.TrainingYearCell(dlamini).Should().Be("3");
        ProgrammeWords.YearColumn(Read()).Should().Be("In 2026");

        var none = Row("Zola Abrahams", semesterApplying: 0, yearApplying: 0, trainingYear: null);
        ProgrammeWords.SemesterFigure(none).Should().Be(("none apply yet", "Semester targets"));
        ProgrammeWords.YearFigure(none).Should().Be(("none apply yet", "Yearly targets"));
        ProgrammeWords.TrainingYear(none).Should().Be("Programme not started");
        ProgrammeWords.TrainingYearCell(none).Should().Be("Not started");
    }

    [Fact]
    public void AnExemptRegistrar_SaysWhyInWords()
    {
        ProgrammeWords.Exempt.Should().Be("Exempt this period");
        ProgrammeWords.ExemptReason(Row("Sipho Ndlovu", exemption: ProgrammeExemption.StartedPartWay))
            .Should().Be("Started part-way through the period");
        ProgrammeWords.ExemptReason(Row("Yusuf Adams", exemption: ProgrammeExemption.NotStarted, programmeStart: new DateOnly(2027, 1, 15)))
            .Should().Be("Starts on 2027-01-15");
        ProgrammeWords.ExemptReason(Row("Nomsa Mahlangu")).Should().BeEmpty();
    }

    [Fact]
    public void FurthestShort_NamesTheCodes_AndOneLineWhenTheyShareAFigure()
    {
        var mahlangu = Row("Nomsa Mahlangu", furthest: [Short("PAED-001", 0, 3), Short("PAED-003", 0, 3), Short("PAED-005", 0, 3)]);
        ProgrammeWords.FurthestShortCodes(mahlangu).Should().Be("PAED-001, PAED-003, PAED-005");
        ProgrammeWords.FurthestShortLine(mahlangu).Should().Be("0 of 3 each this semester");

        var mixed = Row("Lerato Molefe", furthest: [Short("PAED-002", 0, 3), Short("PAED-004", 1, 3), Short("PAED-008", 0, 1, QuotaPeriod.AcademicYear)]);
        ProgrammeWords.FurthestShortLine(mixed).Should().Be(
            "PAED-002: 0 of 3 this semester · PAED-004: 1 of 3 this semester · PAED-008: 0 of 1 in 2026");

        var one = Row("Anele Dlamini", furthest: [Short("PAED-008", 0, 1, QuotaPeriod.AcademicYear)]);
        ProgrammeWords.FurthestShortLine(one).Should().Be("0 of 1 in 2026");

        var done = Row("Anele Dlamini");
        ProgrammeWords.FurthestShortCodes(done).Should().Be("Every target met");
        ProgrammeWords.FurthestShortLine(done).Should().BeEmpty();
    }

    [Fact]
    public void ShortOnsCell_IsTheEpasFigure_AndWhatIsLeft_ByItsEnd()
    {
        ProgrammeWords.ShortOnCell(Row("Nomsa Mahlangu", shortOn: Short("PAED-002", 1, 3)))
            .Should().Be(("1 of 3 this semester", "2 more by 2026-11-30"));
        ProgrammeWords.ShortOnCell(Row("Pieter du Plessis", shortOn: Short("PAED-002", 0, 3)))
            .Should().Be(("0 of 3 this semester", "3 more by 2026-11-30"));
        ProgrammeWords.ShortOnCell(Row("Anele Dlamini", shortOn: Short("PAED-008", 0, 1, QuotaPeriod.AcademicYear)))
            .Should().Be(("0 of 1 in 2026", "1 more by 2026-11-30"));
    }

    [Fact]
    public void LastFiled_IsIso_OrNothingFiledYet()
    {
        ProgrammeWords.LastFiled(Row("Anele Dlamini", lastFiled: new DateOnly(2026, 9, 12))).Should().Be("2026-09-12");
        ProgrammeWords.LastFiled(Row("Anele Dlamini")).Should().Be("Nothing filed yet");
        ProgrammeWords.FiledRowMeta(Row("Pieter du Plessis", trainingYear: 2, lastFiled: new DateOnly(2026, 9, 12)))
            .Should().Be("Last filed 2026-09-12 · Training year 2");
        ProgrammeWords.FiledRowMeta(Row("Pieter du Plessis", trainingYear: 2)).Should().Be("Nothing filed yet · Training year 2");
    }

    [Fact]
    public void TheHeading_CountsTheAnswer_InOneFiltersOwnWords_SingularAndPlural()
    {
        ProgrammeWords.ListHeading(Read(), new ProgrammeTraineesFilter(), 5).Should().Be("5 current registrars");
        ProgrammeWords.ListHeading(Read(current: 1), new ProgrammeTraineesFilter(), 1).Should().Be("1 current registrar");

        var shortOn = new ProgrammeTraineesFilter(ShortOnEpaId: 2);
        ProgrammeWords.ListHeading(Read(shortOn: Paed002), shortOn, 5).Should().Be("5 of 5 current registrars are short on PAED-002");
        ProgrammeWords.ListHeading(Read(shortOn: Paed002), shortOn, 1).Should().Be("1 of 5 current registrars is short on PAED-002");

        var filed = new ProgrammeTraineesFilter(NothingFiled: true);
        ProgrammeWords.ListHeading(Read(), filed, 1).Should().Be("1 of 5 current registrars has filed nothing in 30 days");
        ProgrammeWords.ListHeading(Read(current: 2), filed, 2).Should().Be("2 of 2 current registrars have filed nothing in 30 days");

        var year = new ProgrammeTraineesFilter(TrainingYear: 4);
        ProgrammeWords.ListHeading(Read(), year, 2).Should().Be("2 of 5 current registrars are in training year 4");
        ProgrammeWords.ListHeading(Read(), year, 1).Should().Be("1 of 5 current registrars is in training year 4");

        var several = new ProgrammeTraineesFilter(1, 4, true);
        ProgrammeWords.ListHeading(Read(shortOn: Paed001), several, 2).Should().Be("2 of 5 current registrars match these filters");
        ProgrammeWords.ListHeading(Read(shortOn: Paed001), several, 1).Should().Be("1 of 5 current registrars matches these filters");

        ProgrammeWords.ListHeading(Read(shortOn: Paed001), several, 0).Should().Be("0 of 5 current registrars");
        ProgrammeWords.ListHeading(Read(), filed, 0).Should().Be("0 of 5 current registrars");
    }

    [Fact]
    public void TheRuleLine_SaysTheOrder_AndWhatTheFiguresAreReadAgainst()
    {
        ProgrammeWords.ListRule(Read(), new ProgrammeTraineesFilter())
            .Should().Be("Fewest met first, then by surname. Semester 2, 2026 ends on 2026-11-30.");
        ProgrammeWords.ListRule(Read(exempt: 1), new ProgrammeTraineesFilter())
            .Should().Be("Fewest met first, then by surname. 1 registrar exempt this period, not counted, listed last.");
        ProgrammeWords.ListRule(Read(exempt: 2), new ProgrammeTraineesFilter())
            .Should().Be("Fewest met first, then by surname. 2 registrars exempt this period, not counted, listed last.");
        ProgrammeWords.ListRule(Read(shortOn: Paed002), new ProgrammeTraineesFilter(ShortOnEpaId: 2)).Should().Be(
            "PAED-002 — Managing common paediatric presentations, 3 per semester. Furthest from its target first, then fewest " +
            "EPAs met, then by surname.");
        ProgrammeWords.ListRule(Read(), new ProgrammeTraineesFilter(NothingFiled: true)).Should().Be(
            "Nothing filed (a draft is not filed) in the last 30 days; a recorded MSF counts. A registrar admitted less than 30 " +
            "days ago is not listed. Longest without first.");
        ProgrammeWords.ListRule(Read(), new ProgrammeTraineesFilter(TrainingYear: 4, NothingFiled: true)).Should().StartWith(
            "Training year 4, nothing filed in 30 days. Nothing filed (a draft is not filed)");
    }

    [Fact]
    public void TheCaption_NoMatch_AndWhatWasAsked()
    {
        ProgrammeWords.Caption(new ProgrammeTraineesFilter(), null).Should().Be("Current registrars, fewest EPAs met first");
        ProgrammeWords.Caption(new ProgrammeTraineesFilter(), null, exemptCount: 1)
            .Should().Be("Current registrars, fewest EPAs met first, one exempt this period");
        ProgrammeWords.Caption(new ProgrammeTraineesFilter(ShortOnEpaId: 2), Paed002)
            .Should().Be("Current registrars short on PAED-002, furthest from its target first");
        ProgrammeWords.Caption(new ProgrammeTraineesFilter(NothingFiled: true), null)
            .Should().Be("Current registrars with nothing filed in 30 days");
        ProgrammeWords.Caption(new ProgrammeTraineesFilter(TrainingYear: 4), null)
            .Should().Be("Current registrars in training year 4, fewest EPAs met first");
        ProgrammeWords.Caption(new ProgrammeTraineesFilter(1, 4, true), Paed001).Should().Be("Current registrars matching these filters");

        ProgrammeWords.NoMatch.Should().Be("No registrar matches these filters.");
        ProgrammeWords.Asked(new ProgrammeTraineesFilter(1, 4, true), Paed001)
            .Should().Be("Short on PAED-001, training year 4, nothing filed in 30 days.");
        ProgrammeWords.Asked(new ProgrammeTraineesFilter(NothingFiled: true), null).Should().Be("Nothing filed in 30 days.");
        ProgrammeWords.EpaOption(Paed002).Should().Be("PAED-002 — Managing common paediatric presentations");
    }

    [Fact]
    public void ThePagesFixedWords()
    {
        ProgrammeWords.EmptyTitle.Should().Be("No current registrars");
        ProgrammeWords.EmptyBody.Should().Be("They appear here once they are admitted to the programme.");
        ProgrammeWords.Loading.Should().Be("Loading Programme trainees.");
        ProgrammeWords.LoadFailed.Should().Be(
            "Could not load Programme trainees. Nothing has changed. Try again, or come back in a few minutes.");
        ProgrammeWords.LoadFailed.Should().Be($"{ProgrammeWords.LoadFailedLead} {ProgrammeWords.LoadFailedRest}");
        (ProgrammeWords.ShortOnLabel, ProgrammeWords.AnyEpa, ProgrammeWords.YearLabel, ProgrammeWords.AnyYear,
                ProgrammeWords.FiledLabel, ProgrammeWords.Show, ProgrammeWords.ClearFilters)
            .Should().Be(("Short on", "Any EPA", "Training year", "Any", "Nothing filed in 30 days", "Show", "Clear filters"));
    }

    [Fact]
    public void HomesCards_CountInWords()
    {
        ProgrammeWords.Badge(5).Should().Be("5 registrars");
        ProgrammeWords.Badge(1).Should().Be("1 registrar");
        ProgrammeWords.More(3).Should().Be("3 more in Programme trainees.");
        ProgrammeWords.More(1).Should().Be("1 more in Programme trainees.");
        ProgrammeWords.RosterEmpty.Should().Be("No current registrars. They appear here once they are admitted to the programme.");
        ProgrammeWords.OpenTrainees.Should().Be("Open Programme trainees");
        ProgrammeWords.FiledRule.Should().Be(
            "Current registrars with nothing filed (a draft is not filed) in the last 30 days. A registrar admitted less than 30 " +
            "days ago is not listed.");
        ProgrammeWords.FiledEmpty.Should().Be("Every current registrar has filed something in the last 30 days.");
        ProgrammeWords.OpenFiled.Should().Be("Open in Programme trainees");
    }

    [Fact]
    public void NoWordNamesAPersonByPronoun_NorSaysInactive_NorPrintsAFigureWithASlash()
    {
        var phrases = Constants(typeof(ProgrammeWords)).Concat(
        [
            ProgrammeWords.Subtitle(Kgk, "Semester 2, 2026"),
            ProgrammeWords.ListRule(Read(exempt: 1), new ProgrammeTraineesFilter()),
            ProgrammeWords.ListRule(Read(shortOn: Paed002), new ProgrammeTraineesFilter(ShortOnEpaId: 2)),
            ProgrammeWords.ListRule(Read(), new ProgrammeTraineesFilter(NothingFiled: true)),
            ProgrammeWords.ListHeading(Read(), new ProgrammeTraineesFilter(NothingFiled: true), 1),
            ProgrammeWords.ExemptReason(ProgrammeExemption.StartedPartWay, D),
            ProgrammeWords.FiledRowMeta(Row("Pieter du Plessis"))
        ]).ToList();

        phrases.Should().NotContain(phrase => NamesAPersonByPronoun(phrase));
        phrases.Should().NotContain(phrase => phrase.Contains("inactive", StringComparison.OrdinalIgnoreCase));
        phrases.Should().NotContain(phrase => phrase.Contains(" / ", StringComparison.Ordinal) || phrase.Contains('%'));
        phrases.Should().NotContain(phrase => phrase.Contains("Trainees in programme", StringComparison.Ordinal) ||
                                              phrase.Contains("Pending reviews", StringComparison.Ordinal));
    }

    [Fact]
    public void TheAddresses_AndTheFiltersQueryNames()
    {
        ProgrammeLinks.Trainees.Should().Be("/programme/trainees");
        ProgrammeLinks.Trainee(7).Should().Be("/programme/trainees/7");
        ProgrammeLinks.Waiting.Should().Be("/programme/waiting");
        ProgrammeLinks.ShortOn(2).Should().Be("/programme/trainees?short=2");
        ProgrammeLinks.NothingFiled.Should().Be("/programme/trainees?filed=true");
        (ProgrammeLinks.ShortKey, ProgrammeLinks.YearKey, ProgrammeLinks.FiledKey, ProgrammeLinks.ShowKey,
                ProgrammeLinks.ShowOverdueValue, ProgrammeLinks.WithKey)
            .Should().Be(("short", "year", "filed", "show", "overdue", "with"));

        ProgrammeLinks.TraineesFiltered(null, null, false).Should().Be("/programme/trainees");
        ProgrammeLinks.TraineesFiltered(2, 4, true).Should().Be("/programme/trainees?short=2&year=4&filed=true");
        ProgrammeLinks.WaitingFiltered(false, null).Should().Be("/programme/waiting");
        ProgrammeLinks.WaitingFiltered(true, "a b").Should().Be("/programme/waiting?show=overdue&with=a%20b");
    }
}
