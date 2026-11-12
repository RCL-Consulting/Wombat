using Wombat.Domain.Email;

namespace Wombat.Domain.Tests.Email;

/// <summary>
/// T400 (flow 18, Q18, K3; build-contracts § 0): a mail outcome holds the moment, the kind and the redacted refusal, kept
/// to the column's 2,000 characters, and nothing else.
/// </summary>
public sealed class EmailOutcomeTests
{
    private static readonly DateTime At = new(2026, 11, 10, 19, 39, 41, DateTimeKind.Utc);

    [Fact]
    public void Sent_HoldsItsMoment_AndNoRefusal()
    {
        var outcome = EmailOutcome.Sent(At);

        Assert.Equal(EmailOutcomeKind.Sent, outcome.Kind);
        Assert.Equal(At, outcome.OccurredAt);
        Assert.Null(outcome.Refusal);
    }

    [Fact]
    public void Dropped_HoldsItsMoment_AndTheRefusalTrimmed()
    {
        var outcome = EmailOutcome.Dropped(At, "  550 5.1.1 Mailbox unavailable \n");

        Assert.Equal(EmailOutcomeKind.Dropped, outcome.Kind);
        Assert.Equal(At, outcome.OccurredAt);
        Assert.Equal("550 5.1.1 Mailbox unavailable", outcome.Refusal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   \t\n")]
    public void Dropped_WithNoWords_HoldsNoRefusal(string? refusal)
        => Assert.Null(EmailOutcome.Dropped(At, refusal).Refusal);

    [Fact]
    public void ARefusalOf2001Characters_IsKeptTo2000()
    {
        var refusal = new string('a', 1999) + "bc";

        var kept = EmailOutcome.Dropped(At, refusal).Refusal;

        Assert.Equal(2000, EmailOutcome.RefusalMaxLength);
        Assert.Equal(new string('a', 1999) + "b", kept);
    }

    [Fact]
    public void ARefusalOf2000Characters_IsKeptWhole()
    {
        var refusal = new string('x', 2000);

        Assert.Equal(refusal, EmailOutcome.Dropped(At, refusal).Refusal);
    }

    [Fact]
    public void TheOutcome_HoldsNoAddressSubjectBodyReferenceOrTag()
    {
        // Q18: the row says that a mail went or did not, never to whom or what.
        var names = typeof(EmailOutcome).GetProperties().Select(property => property.Name).OrderBy(name => name, StringComparer.Ordinal);

        Assert.Equal(["Id", "Kind", "OccurredAt", "Refusal"], names);
    }
}
