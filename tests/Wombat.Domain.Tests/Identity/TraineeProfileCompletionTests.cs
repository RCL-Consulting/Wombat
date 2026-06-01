using Wombat.Domain.Identity;

namespace Wombat.Domain.Tests.Identity;

public sealed class TraineeProfileCompletionTests
{
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

        profile.Complete(new DateOnly(2029, 12, 15));

        Assert.Equal(new DateOnly(2029, 12, 15), profile.CompletedOn);
        Assert.False(profile.IsActive);
    }

    [Fact]
    public void Complete_Throws_WhenAlreadyInactive()
    {
        var profile = ActiveProfile();
        profile.IsActive = false;

        var exception = Assert.Throws<InvalidOperationException>(() => profile.Complete(new DateOnly(2029, 12, 15)));
        Assert.Contains("active", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Complete_Throws_WhenCompletionBeforeStart()
    {
        var profile = ActiveProfile();

        var exception = Assert.Throws<InvalidOperationException>(() => profile.Complete(new DateOnly(2022, 1, 1)));
        Assert.Contains("before the programme start", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
