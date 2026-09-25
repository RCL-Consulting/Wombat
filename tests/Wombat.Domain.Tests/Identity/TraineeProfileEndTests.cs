using Wombat.Domain.Identity;

namespace Wombat.Domain.Tests.Identity;

/// <summary>
/// T209: the day a programme actually ended, which D49 reads. Completion records it (T080), and so, since T209, does
/// deactivation. The expected completion date is a plan, not an end.
/// </summary>
public sealed class TraineeProfileEndTests
{
    /// <summary>Today on the South African calendar, as the handler passes it.</summary>
    private static readonly DateOnly Today = new(2026, 12, 31);

    private static TraineeProfile ActiveProfile() => new()
    {
        Id = 1,
        UserId = "trainee-1",
        CurriculumId = 1,
        ProgrammeStartDate = new DateOnly(2023, 1, 15),
        ExpectedCompletionDate = new DateOnly(2026, 1, 14),
        IsActive = true
    };

    [Fact]
    public void Deactivate_RecordsTheLastDay_AndDeactivates()
    {
        var profile = ActiveProfile();

        profile.Deactivate(new DateOnly(2026, 5, 31), Today);

        Assert.Equal(new DateOnly(2026, 5, 31), profile.DeactivatedOn);
        Assert.Null(profile.CompletedOn);
        Assert.False(profile.IsActive);
        Assert.Equal(new DateOnly(2026, 5, 31), profile.EndedOn);
    }

    [Fact]
    public void Deactivate_OnTheProgrammeStart_IsAllowed()
    {
        // A trainee who left on their first day, or never started: the earliest end there is.
        var profile = ActiveProfile();

        profile.Deactivate(profile.ProgrammeStartDate, Today);

        Assert.Equal(profile.ProgrammeStartDate, profile.EndedOn);
    }

    [Fact]
    public void Deactivate_RefusesAnInactiveProfile_AndChangesNothing()
    {
        var profile = ActiveProfile();
        profile.Complete(new DateOnly(2026, 6, 15), Today);

        var exception = Assert.Throws<InvalidOperationException>(() => profile.Deactivate(new DateOnly(2026, 7, 1), Today));

        Assert.Contains("active", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(profile.DeactivatedOn);
        Assert.Equal(new DateOnly(2026, 6, 15), profile.EndedOn);
    }

    [Fact]
    public void Deactivate_RefusesADayBeforeTheProgrammeStart_AndChangesNothing()
    {
        var profile = ActiveProfile();

        var exception = Assert.Throws<InvalidOperationException>(() => profile.Deactivate(new DateOnly(2022, 12, 31), Today));

        Assert.Contains("before the programme start", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.True(profile.IsActive);
        Assert.Null(profile.DeactivatedOn);
    }

    [Fact]
    public void Deactivate_OnToday_IsAllowed()
    {
        var profile = ActiveProfile();

        profile.Deactivate(Today, Today);

        Assert.Equal(Today, profile.EndedOn);
    }

    [Fact]
    public void Deactivate_RefusesADayAfterToday_AndChangesNothing()
    {
        // T209 review: an administrator who types a resignation's notice date deactivates the profile now, but the end
        // would be months away, and D49 would hold the trainee to every period up to it. Refused before anything changes.
        var profile = ActiveProfile();

        var exception = Assert.Throws<InvalidOperationException>(() => profile.Deactivate(Today.AddDays(1), Today));

        Assert.Equal("The deactivation date cannot be after today (2026-12-31).", exception.Message);
        Assert.True(profile.IsActive);
        Assert.Null(profile.DeactivatedOn);
        Assert.Null(profile.EndedOn);
    }

    [Fact]
    public void TheEndIsTheCompletionDay_ForAGraduate()
    {
        var profile = ActiveProfile();

        profile.Complete(new DateOnly(2026, 11, 20), Today);

        Assert.Equal(new DateOnly(2026, 11, 20), profile.EndedOn);
        Assert.Null(profile.DeactivatedOn);
    }

    [Fact]
    public void ARunningProgrammeHasNoEnd_EvenPastItsExpectedCompletion()
    {
        // The expected completion date (14 January 2026 here) is a plan. A trainee still active after it is still in the
        // programme, and still held to its targets.
        var profile = ActiveProfile();

        Assert.Null(profile.EndedOn);
    }
}
