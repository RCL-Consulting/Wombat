using FluentAssertions;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Programme.Trainees;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Web.Components.Shared.Programme;
using Wombat.Web.Components.Shared.Progress;
using static Wombat.Web.Tests.Programme.ProgrammeWordsFixtures;
using P = Wombat.Web.Tests.Progress.ProgressFixtures;

namespace Wombat.Web.Tests.Programme;

/// <summary>
/// T358 (flow 06, lane A2; R2-Registrar r1–r8; C9; review 8): the registrar page's words, and the ended record's words
/// written for staff: the registrar's name or "the programme" wherever My progress says "you" (round-3-check 2), and no
/// pronoun for a person anywhere (round-3-check 1).
/// </summary>
public sealed class RegistrarWordsTests
{
    private static ProgrammeTraineeDto Registrar(string name = "Nomsa Mahlangu", int? year = 1, ProgrammeEndDto? ended = null)
        => new(104, "mahlangu", name, year, "Kgosi Kgari Teaching Hospital", "Paediatrics", "Semester 2, 2026", ended, Kgk);

    [Fact]
    public void TheSubtitle_TabAndPageWords()
    {
        RegistrarWords.Subtitle(Registrar()).Should().Be(
            "Training year 1 · Semester 2, 2026 · Kgosi Kgari Teaching Hospital, Paediatrics");
        RegistrarWords.Subtitle(Registrar("Pieter du Plessis", 2, new ProgrammeEndDto(false, new DateOnly(2026, 10, 2), D)))
            .Should().Be("Training year 2 · Programme ended 2026-10-02 · Kgosi Kgari Teaching Hospital, Paediatrics");
        RegistrarWords.Subtitle(Registrar("Lerato Molefe", 4, new ProgrammeEndDto(true, new DateOnly(2026, 10, 4), D)))
            .Should().Be("Training year 4 · Programme completed 2026-10-04 · Kgosi Kgari Teaching Hospital, Paediatrics");

        RegistrarWords.Tab("Nomsa Mahlangu").Should().Be("Nomsa Mahlangu · Wombat");
        RegistrarWords.ErrorHeading.Should().Be("Programme trainee");
        RegistrarWords.Loading.Should().Be("Loading this registrar's progress.");
        RegistrarWords.LoadFailed.Should().Be(
            "Could not load this registrar's progress. Nothing has changed. Try again, or come back in a few minutes.");
    }

    [Fact]
    public void TheSections_InC9sOrder_EachWithItsOwnError()
    {
        new[]
        {
            RegistrarWords.PeriodHeading, RegistrarWords.EpasHeading, RegistrarWords.StandingHeading,
            RegistrarWords.TrajectoriesHeading, RegistrarWords.WaitingHeading, RegistrarWords.ReviewsHeading
        }.Should().Equal(
            "This period", "EPAs", "Entrustment against Annexure A", "Rating trajectories", "Waiting for assessors",
            "Committee reviews");

        new[]
        {
            RegistrarWords.PeriodFailed, RegistrarWords.EpasFailed, RegistrarWords.StandingFailed,
            RegistrarWords.TrajectoriesFailed, RegistrarWords.WaitingFailed, RegistrarWords.ReviewsFailed
        }.Should().Equal(
            "Could not load this period. Nothing has changed. Try again, or come back in a few minutes.",
            "Could not load the EPAs. Nothing has changed. Try again, or come back in a few minutes.",
            "Could not load the standing. Nothing has changed. Try again, or come back in a few minutes.",
            "Could not load the rating trajectories. Nothing has changed. Try again, or come back in a few minutes.",
            "Could not load what waits for an assessor. Nothing has changed. Try again, or come back in a few minutes.",
            "Could not load the committee reviews. Nothing has changed. Try again, or come back in a few minutes.");
    }

    [Fact]
    public void ThePeriodLine_TheEmpties_AndTheReviewLink()
    {
        var summary = P.Summary(D, new DateOnly(2026, 1, 15), stage: 1, []);
        RegistrarWords.PeriodLine(summary).Should().Be(
            "Semester 2, 2026 ends on 2026-11-30. Training year 1 sets the minimum level each encounter is judged against.");

        var year = TrajectoryWindow.AcademicYearOf(D);
        RegistrarWords.NoRatings(year).Should().Be("No ratings yet in the 2026 academic year.");
        RegistrarWords.NoRatings(year, ended: true).Should().Be("No ratings in the 2026 academic year.");
        RegistrarWords.NothingWaiting("Nomsa Mahlangu").Should().Be("Nothing of Nomsa Mahlangu's waits for an assessor.");
        RegistrarWords.NoReview("Nomsa Mahlangu").Should().Be("No review is scheduled for Nomsa Mahlangu.");

        var review = new CommitteeReviewListItemDto(
            11, "molefe", 1, "Panel 1", new DateOnly(2026, 7, 1), new DateOnly(2026, 11, 30), new DateOnly(2026, 10, 4),
            CommitteeReviewState.Scheduled, null, null, ReviewType: CommitteeReviewType.PreGraduation)
        {
            AcademicYear = 2026,
            Semester = 2
        };
        RegistrarWords.ReviewLink(review).Should().Be("Pre-graduation review, 2026-10-04");
        RegistrarWords.ReviewMeta(review).Should().Be("Semester 2, 2026 · Scheduled");
    }

    [Fact]
    public void TheEndedRecord_IsWrittenForStaff()
    {
        RegistrarWords.EndedNotice("Pieter du Plessis", new ProgrammeEndDto(false, new DateOnly(2026, 10, 2), D), "Semester 2, 2026")
            .Should().Be(
                "Pieter du Plessis's programme ended on 2026-10-02, part-way through Semester 2, 2026. No target applies " +
                "after that; what follows is the record as it stood then, read-only.");
        RegistrarWords.EndedNotice("Lerato Molefe", new ProgrammeEndDto(true, new DateOnly(2026, 10, 4), D), "Semester 2, 2026")
            .Should().StartWith("Lerato Molefe completed the programme on 2026-10-04");
        RegistrarWords.EndedNotice("Pieter du Plessis", new ProgrammeEndDto(false, null, D), "Semester 2, 2026")
            .Should().Be(
                "Pieter du Plessis's programme has ended. Wombat did not record the day it ended, so the periods are shown up " +
                "to 2026-10-04; what follows is the record, read-only.");

        RegistrarWords.EndedWhen.Should().Be("when the programme ended");
        RegistrarWords.EndedWhenOr(new ProgrammeEndDto(false, null, D), "today").Should().Be("today");
        RegistrarWords.EndedProgrammeHeading.Should().Be("The programme");
        RegistrarWords.EndedNoTarget.Should().Be("No target (the programme ended part-way through)");
        RegistrarWords.EndedNoItems("Pieter du Plessis").Should().Be(
            "No EPA on Pieter du Plessis's curriculum is in use any more, so there are no targets to show.");
    }

    [Fact]
    public void AnEndedPeriodLine_SaysTheProgramme_WhereMyProgressSaysYour()
    {
        var endedPartWay = P.Waived(P.Semester2Of2026, QuotaWindowStatus.ExemptProgrammeEnded, count: 1, target: 3);
        var startedPartWay = P.Waived(P.Semester1Of2026, QuotaWindowStatus.ExemptPartialPeriod, count: 0, target: 3);
        var after = P.Waived(P.Semester2Of2026, QuotaWindowStatus.AfterProgrammeEnd, count: 0, target: 3);
        var before = P.Waived(P.Semester1Of2026, QuotaWindowStatus.NotStarted, count: 0, target: 3);
        var counted = P.Counting(P.Semester1Of2026, 1, 3, minimumReached: 1);

        RegistrarWords.EndedPeriodLine(endedPartWay, D).Should().Be("no target (the programme ended part-way through) · 1 recorded");
        RegistrarWords.EndedPeriodLine(startedPartWay, D).Should().Be("no target (started part-way through) · 0 recorded");
        RegistrarWords.EndedPeriodLine(after, D).Should().Be("no target (after the programme ended) · 0 recorded");
        RegistrarWords.EndedPeriodLine(before, D).Should().Be("no target (before the programme started)");
        RegistrarWords.EndedPeriodLine(counted, D).Should().Be(ProgressWords.EndedPeriodLine(counted, D))
            .And.Be("1 of 3, 2 short; 1 at the minimum level when observed");

        // Flow 05's own words still say "your": the staff words are a different set, not a change to them.
        ProgressWords.EndedPeriodLine(endedPartWay, D).Should().Contain("your programme");
    }

    [Fact]
    public void NoWordNamesAPersonByPronoun()
    {
        var ended = new ProgrammeEndDto(false, new DateOnly(2026, 10, 2), D);
        Constants(typeof(RegistrarWords))
            .Concat(
            [
                RegistrarWords.Subtitle(Registrar()),
                RegistrarWords.EndedNotice("Pieter du Plessis", ended, "Semester 2, 2026"),
                RegistrarWords.EndedNotice("Pieter du Plessis", ended with { EndedOn = null }, "Semester 2, 2026"),
                RegistrarWords.EndedNoItems("Pieter du Plessis"),
                RegistrarWords.NothingWaiting("Nomsa Mahlangu"),
                RegistrarWords.NoReview("Nomsa Mahlangu"),
                RegistrarWords.EndedPeriodLine(P.Waived(P.Semester2Of2026, QuotaWindowStatus.ExemptProgrammeEnded, 1, 3), D)
            ])
            .Should().NotContain(phrase => NamesAPersonByPronoun(phrase));
    }
}
