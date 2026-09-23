using Wombat.Domain.Audit;

namespace Wombat.Domain.Tests.Identity;

/// <summary>
/// The audit row's error text is bounded where it is created (T122). The audit pipeline writes this row from a failed
/// command's catch; a message longer than the column made THAT save fail too, so the caller saw a database error in
/// place of the refusal the command actually raised, and nothing was audited.
/// </summary>
public sealed class AuditEntryErrorMessageTests
{
    [Fact]
    public void AMessageLongerThanTheColumnIsTruncated_WithAnEllipsis()
    {
        var entry = Create(new string('x', AuditEntry.MaxErrorMessageLength + 500));

        Assert.Equal(AuditEntry.MaxErrorMessageLength, entry.ErrorMessage!.Length);
        Assert.EndsWith("…", entry.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void AMessageThatFitsIsKeptAsItIs()
    {
        var exact = new string('y', AuditEntry.MaxErrorMessageLength);

        Assert.Equal(exact, Create(exact).ErrorMessage);
        Assert.Equal("refused", Create("refused").ErrorMessage);
        Assert.Null(Create(null).ErrorMessage);
    }

    private static AuditEntry Create(string? errorMessage)
        => AuditEntry.Create(
            DateTime.UtcNow,
            AuditCategory.Command,
            "TransitionActivityCommand",
            success: false,
            actorUserId: "user-1",
            errorMessage: errorMessage);
}
