using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;

namespace Wombat.Domain.Tests.Identity;

/// <summary>T312: the list a programme ended with, as the profile stores it.</summary>
public sealed class EndedCurriculumItemTests
{
    private static readonly EndedCurriculumItem National =
        new(101, 1, false, 3, QuotaPeriod.Semester, 6, """{"1": 3, "4": 6}""", 42, QuotaPeriod.AcademicYear, false);

    private static readonly EndedCurriculumItem Local =
        new(109, 9, true, 1, QuotaPeriod.AcademicYear, 3, null, null, null, true);

    [Fact]
    public void TheStoredList_ReadsBackAsWritten_InItemOrder()
    {
        var json = EndedCurriculumItem.Serialize([Local, National]);

        Assert.Equal([National, Local], EndedCurriculumItem.Parse(json)!);
    }

    [Fact]
    public void TheListAsPostgresRendersIt_KeysReorderedAndSpaced_ReadsTheSame()
    {
        // jsonb re-renders what it stores (CLAUDE.md): what the T312 migration writes in SQL comes back in this shape, the
        // enums as their integers (Semester 1, AcademicYear 0).
        const string rendered = """
            [{"epaId": 1, "isLocal": false, "scaleId": 42, "quotaPeriod": 1, "requiredCount": 3, "decisionCadence": 0,
              "curriculumItemId": 101, "minimumLevelOrder": 6, "decisionIsOpportunistic": false,
              "minimumLevelByStageJson": "{\"1\": 3, \"4\": 6}"}]
            """;

        Assert.Equal([National], EndedCurriculumItem.Parse(rendered)!);
    }

    [Fact]
    public void NothingStored_IsNoList_AndAnEmptyListIsAList()
    {
        Assert.Null(EndedCurriculumItem.Parse(null));
        Assert.Empty(EndedCurriculumItem.Parse("[]")!);
    }

    [Fact]
    public void TheListIsRecordedOnlyOnceTheProgrammeHasEnded_AndOnlyOnce()
    {
        var profile = new TraineeProfile { ProgrammeStartDate = new DateOnly(2023, 1, 15), IsActive = true };

        Assert.Throws<InvalidOperationException>(() => profile.RecordEndedCurriculum([National]));
        Assert.Null(profile.EndedCurriculumJson);

        profile.Complete(new DateOnly(2026, 9, 26), new DateOnly(2026, 9, 26));
        profile.RecordEndedCurriculum([National]);
        Assert.Equal([National], profile.ReadEndedCurriculum()!);

        Assert.Throws<InvalidOperationException>(() => profile.RecordEndedCurriculum([]));
        Assert.Equal([National], profile.ReadEndedCurriculum()!);
    }

    [Fact]
    public void ASouthAfricanDay_EndsAt22hUtc()
    {
        var end = ProgrammeCalendar.EndOf(new DateOnly(2026, 9, 26));

        Assert.Equal(new DateTime(2026, 9, 26, 22, 0, 0, DateTimeKind.Utc), end);
        Assert.Equal(DateTimeKind.Utc, end.Kind);
        Assert.Equal(new DateOnly(2026, 9, 26), ProgrammeCalendar.DateOf(end.AddTicks(-1)));
        Assert.Equal(new DateOnly(2026, 9, 27), ProgrammeCalendar.DateOf(end));
    }
}
