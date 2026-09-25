using Wombat.Domain.Identity;

namespace Wombat.Domain.Tests.Identity;

/// <summary>
/// T258: erasing a trainee hands each of their profiles to the pseudonym and ends it on the erasure day, as a withdrawal
/// would. A profile that had already ended keeps the end it recorded. It never refuses: an erasure is the data subject's
/// right, not an administrator's withdrawal, so it is not held to the programme's dates.
/// </summary>
public sealed class TraineeProfileErasureTests
{
    private const string Pseudonym = "deleted_user_1a2b3c4d";

    private static readonly DateOnly ErasedOn = new(2026, 9, 25);

    private static TraineeProfile ActiveProfile() => new()
    {
        Id = 1,
        UserId = "trainee-1",
        CurriculumId = 1,
        ProgrammeStartDate = new DateOnly(2025, 1, 15),
        ExpectedCompletionDate = new DateOnly(2029, 1, 14),
        IsActive = true
    };

    [Fact]
    public void Erase_AnActiveProfile_MovesIt_AndEndsItOnTheErasureDay()
    {
        var profile = ActiveProfile();

        profile.Erase(Pseudonym, ErasedOn);

        Assert.Equal(Pseudonym, profile.UserId);
        Assert.False(profile.IsActive);
        Assert.Equal(ErasedOn, profile.DeactivatedOn);
        Assert.Null(profile.CompletedOn);
        Assert.Equal(ErasedOn, profile.EndedOn);
    }

    [Fact]
    public void Erase_ACompletedProfile_MovesIt_AndKeepsItsCompletion()
    {
        var profile = ActiveProfile();
        profile.Complete(new DateOnly(2026, 6, 30), ErasedOn);

        profile.Erase(Pseudonym, ErasedOn);

        Assert.Equal(Pseudonym, profile.UserId);
        Assert.False(profile.IsActive);
        Assert.Equal(new DateOnly(2026, 6, 30), profile.CompletedOn);
        Assert.Null(profile.DeactivatedOn);
    }

    [Fact]
    public void Erase_AProfileAlreadyWithdrawn_MovesIt_AndKeepsItsLastDay()
    {
        var profile = ActiveProfile();
        profile.Deactivate(new DateOnly(2026, 3, 31), ErasedOn);

        profile.Erase(Pseudonym, ErasedOn);

        Assert.Equal(Pseudonym, profile.UserId);
        Assert.Equal(new DateOnly(2026, 3, 31), profile.DeactivatedOn);
    }

    [Fact]
    public void Erase_AProfileWhoseProgrammeHadNotBegun_EndsItOnTheErasureDay_WithoutRefusing()
    {
        // Deactivate refuses a day before the start. Erasure is not an administrator's withdrawal and is never refused.
        var profile = ActiveProfile();
        profile.ProgrammeStartDate = new DateOnly(2027, 1, 15);

        profile.Erase(Pseudonym, ErasedOn);

        Assert.False(profile.IsActive);
        Assert.Equal(ErasedOn, profile.DeactivatedOn);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Erase_WithNoPseudonym_IsRefused_AndChangesNothing(string pseudonym)
    {
        var profile = ActiveProfile();

        Assert.ThrowsAny<ArgumentException>(() => profile.Erase(pseudonym, ErasedOn));

        Assert.Equal("trainee-1", profile.UserId);
        Assert.True(profile.IsActive);
        Assert.Null(profile.DeactivatedOn);
    }
}
