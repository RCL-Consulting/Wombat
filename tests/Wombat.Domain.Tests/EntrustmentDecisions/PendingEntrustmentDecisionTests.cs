using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Domain.Tests.EntrustmentDecisions;

/// <summary>
/// A staged entrustment decision names the lines of its review's evidence snapshot it rests on, at least one, each once,
/// and keeps only their ids (D38, T131).
/// </summary>
public sealed class PendingEntrustmentDecisionTests
{
    private static readonly DateTime Now = new(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Stage_KeepsTheNamedIds()
    {
        var pending = Stage([501, 503]);

        Assert.Equal(new[] { 501, 503 }, pending.EvidenceItemIds);
    }

    [Fact]
    public void Stage_RefusesADecisionThatNamesNoEvidence()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Stage([]));

        Assert.Equal(PendingEntrustmentDecision.EvidenceRequired, exception.Message);
    }

    [Fact]
    public void Stage_RefusesAnIdNamedTwice_AndAnIdThatNamesNoStoredLine()
    {
        Assert.Throws<InvalidOperationException>(() => Stage([501, 501]));
        Assert.Throws<InvalidOperationException>(() => Stage([0]));
        Assert.Throws<InvalidOperationException>(() => Stage([-4]));
    }

    [Fact]
    public void Update_RefusesNoEvidence_AndChangesNothing()
    {
        var pending = Stage([501]);

        Assert.Throws<InvalidOperationException>(() => pending.Update(4, new DateOnly(2026, 7, 3), null, "Edited.", []));

        Assert.Equal(3, pending.AuthorisedLevelId);
        Assert.Equal("Ready for indirect supervision.", pending.Rationale);
        Assert.Equal(new[] { 501 }, pending.EvidenceItemIds);
    }

    [Fact]
    public void Update_ReplacesTheNamedIds()
    {
        var pending = Stage([501]);

        pending.Update(4, new DateOnly(2026, 7, 3), null, "Edited.", [502, 503]);

        Assert.Equal(new[] { 502, 503 }, pending.EvidenceItemIds);
    }

    private static PendingEntrustmentDecision Stage(int[] evidenceItemIds)
        => PendingEntrustmentDecision.Stage(
            30, 7, 3, new DateOnly(2026, 7, 2), null, "Ready for indirect supervision.", evidenceItemIds, "chair-1", Now);
}
