using Wombat.Domain.Identity;

namespace Wombat.Domain.Tests.Identity;

public sealed class TraineeProfileCompletionTests
{
    /// <summary>Today on the South African calendar, as the handler passes it.</summary>
    private static readonly DateOnly Today = new(2030, 1, 10);

    private static TraineeProfile ActiveProfile() => new()
    {
        Id = 1,
        UserId = "trainee-1",
        CurriculumId = 1,
        ProgrammeStartDate = new DateOnly(2023, 1, 15),
        ExpectedCompletionDate = new DateOnly(2029, 12, 31),
        IsActive = true
    };

    [Fact]
    public void Complete_RecordsDate_AndDeactivates()
    {
        var profile = ActiveProfile();

        profile.Complete(new DateOnly(2029, 12, 15), Today);

        Assert.Equal(new DateOnly(2029, 12, 15), profile.CompletedOn);
        Assert.False(profile.IsActive);
    }

    [Fact]
    public void Complete_Throws_WhenAlreadyInactive()
    {
        var profile = ActiveProfile();
        profile.IsActive = false;

        var exception = Assert.Throws<InvalidOperationException>(() => profile.Complete(new DateOnly(2029, 12, 15), Today));
        Assert.Contains("active", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(profile.CompletedOn);
    }

    [Fact]
    public void Complete_Throws_WhenCompletionBeforeStart()
    {
        var profile = ActiveProfile();

        var exception = Assert.Throws<InvalidOperationException>(() => profile.Complete(new DateOnly(2022, 1, 1), Today));
        Assert.Contains("before the programme start", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.True(profile.IsActive);
        Assert.Null(profile.CompletedOn);
    }

    [Fact]
    public void Complete_OnToday_IsAllowed()
    {
        var profile = ActiveProfile();

        profile.Complete(Today, Today);

        Assert.Equal(Today, profile.CompletedOn);
    }

    [Fact]
    public void Complete_Throws_WhenCompletionIsAfterToday_AndChangesNothing()
    {
        // T209 review: the profile ends now. A later day is an end that has not happened, and D49 would read the periods up
        // to it as still in the programme.
        var profile = ActiveProfile();

        var exception = Assert.Throws<InvalidOperationException>(() => profile.Complete(Today.AddDays(1), Today));

        Assert.Equal("The completion date cannot be after today (2030-01-10).", exception.Message);
        Assert.True(profile.IsActive);
        Assert.Null(profile.CompletedOn);
    }
}
