using Wombat.Domain.Audit;

namespace Wombat.Domain.Tests.Identity;

/// <summary>
/// The actor's User-Agent and display name are bounded where the row is created, as its error text is (T208). The
/// header's length is the client's to choose; one value longer than its column made the row's save fail, so a command
/// that had committed answered with a database error and left no audit row. <c>AuditOverlongActorPostgresTests</c> shows
/// the row stored on a real server.
/// </summary>
public sealed class AuditEntryActorTruncationTests
{
    [Fact]
    public void A2000CharacterUserAgent_IsTruncatedToTheColumn_WithAnEllipsis()
    {
        var userAgent = new string('u', 2000);

        var entry = Create(userAgent: userAgent);

        Assert.Equal(AuditEntry.MaxActorUserAgentLength, entry.ActorUserAgent!.Length);
        Assert.Equal(userAgent[..(AuditEntry.MaxActorUserAgentLength - 1)] + "…", entry.ActorUserAgent);
    }

    [Fact]
    public void ADisplayNameLongerThanTheColumn_IsTruncatedToIt_WithAnEllipsis()
    {
        var display = new string('d', AuditEntry.MaxActorDisplayLength + 1);

        var entry = Create(display: display);

        Assert.Equal(AuditEntry.MaxActorDisplayLength, entry.ActorDisplay!.Length);
        Assert.EndsWith("…", entry.ActorDisplay, StringComparison.Ordinal);
    }

    [Fact]
    public void ValuesThatFit_AreKeptAsTheyAre()
    {
        var userAgent = new string('u', AuditEntry.MaxActorUserAgentLength);
        var display = new string('d', AuditEntry.MaxActorDisplayLength);

        var exact = Create(display: display, userAgent: userAgent);
        Assert.Equal(userAgent, exact.ActorUserAgent);
        Assert.Equal(display, exact.ActorDisplay);

        var ordinary = Create(display: "Thandi Nkosi", userAgent: "Mozilla/5.0");
        Assert.Equal("Mozilla/5.0", ordinary.ActorUserAgent);
        Assert.Equal("Thandi Nkosi", ordinary.ActorDisplay);

        var absent = Create();
        Assert.Null(absent.ActorUserAgent);
        Assert.Null(absent.ActorDisplay);
    }

    /// <summary>
    /// A cut at the column's width less one lands between the two halves of the emoji. Npgsql cannot encode the lone
    /// high surrogate that would leave, and refuses the save (seen on a real server in <c>AuditOverlongActorPostgresTests</c>).
    /// The error text's truncation (T122) shares the cut, so it is held to the same.
    /// </summary>
    [Fact]
    public void ACutThatWouldSplitASurrogatePair_BacksOffTheWholeCharacter()
    {
        var display = new string('d', AuditEntry.MaxActorDisplayLength - 2) + "\U0001F600" + new string('e', 50);
        var errorMessage = new string('x', AuditEntry.MaxErrorMessageLength - 2) + "\U0001F600" + "tail";

        var entry = Create(display: display, errorMessage: errorMessage);

        Assert.Equal(new string('d', AuditEntry.MaxActorDisplayLength - 2) + "…", entry.ActorDisplay);
        Assert.Equal(new string('x', AuditEntry.MaxErrorMessageLength - 2) + "…", entry.ErrorMessage);
    }

    /// <summary>
    /// No column is this narrow today; the cut is held to it anyway, so a future one cannot turn the surrogate check into
    /// an out-of-range read that refuses the row (T208 review). A width below one is a programming error, refused
    /// whatever the value.
    /// </summary>
    [Fact]
    public void AWidthOfOne_KeepsOnlyTheEllipsis_AndAWidthBelowOneIsRefused()
    {
        Assert.Equal("…", AuditEntry.Truncate("ab", 1));
        Assert.Equal("…", AuditEntry.Truncate("\U0001F600", 1));
        Assert.Equal("…", AuditEntry.Truncate("\U0001F600x", 2));
        Assert.Equal("a", AuditEntry.Truncate("a", 1));

        Assert.Throws<ArgumentOutOfRangeException>(() => AuditEntry.Truncate(null, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => AuditEntry.Truncate("a", 0));
    }

    private static AuditEntry Create(string? display = null, string? userAgent = null, string? errorMessage = null)
        => AuditEntry.Create(
            DateTime.UtcNow,
            AuditCategory.Command,
            "CreateInstitutionCommand",
            success: errorMessage is null,
            actorUserId: "user-1",
            actorDisplay: display,
            actorUserAgent: userAgent,
            errorMessage: errorMessage);
}
