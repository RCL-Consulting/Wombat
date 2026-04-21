using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Domain.Tests.EntrustmentDecisions;

public sealed class EntrustmentDecisionTests
{
    private static EntrustmentEvidenceLink SampleEvidence() =>
        EntrustmentEvidenceLink.Create(
            EntrustmentEvidenceSourceType.Activity,
            activityId: 42,
            msfCampaignId: null,
            committeeReviewId: null,
            sourceLabel: "Mini-CEX #42",
            summary: "Direct observation.",
            sourceRecordedOn: DateTime.UtcNow);

    [Fact]
    public void Issue_HappyPath_ReturnsActiveDecisionWithEvidence()
    {
        var decision = EntrustmentDecision.Issue(
            traineeUserId: "trainee-1",
            epaId: 7,
            authorisedLevelId: 3,
            issuedOn: new DateOnly(2026, 4, 1),
            expiresOn: new DateOnly(2027, 4, 1),
            committeeReviewId: 11,
            chairUserId: "chair-1",
            rationale: "Sufficient evidence across four sources.",
            evidenceLinks: new[] { SampleEvidence() });

        Assert.Equal(EntrustmentDecisionStatus.Active, decision.Status);
        Assert.Equal("trainee-1", decision.TraineeUserId);
        Assert.Single(decision.EvidenceLinks);
        Assert.Equal(new DateOnly(2027, 4, 1), decision.ExpiresOn);
    }

    [Theory]
    [InlineData("", 7, 3, 11, "chair-1", "rationale", "trainee")]
    [InlineData("trainee-1", 0, 3, 11, "chair-1", "rationale", "EPA")]
    [InlineData("trainee-1", 7, 0, 11, "chair-1", "rationale", "level")]
    [InlineData("trainee-1", 7, 3, 0, "chair-1", "rationale", "review")]
    [InlineData("trainee-1", 7, 3, 11, "", "rationale", "chair")]
    [InlineData("trainee-1", 7, 3, 11, "chair-1", "", "rationale")]
    public void Issue_RejectsInvalidInputs(string traineeUserId, int epaId, int levelId, int reviewId, string chairUserId, string rationale, string expectedFragment)
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            EntrustmentDecision.Issue(
                traineeUserId,
                epaId,
                levelId,
                new DateOnly(2026, 4, 1),
                null,
                reviewId,
                chairUserId,
                rationale,
                Array.Empty<EntrustmentEvidenceLink>()));

        Assert.Contains(expectedFragment, exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Issue_RejectsExpiryOnOrBeforeIssue()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            EntrustmentDecision.Issue(
                "trainee-1",
                7,
                3,
                new DateOnly(2026, 4, 1),
                new DateOnly(2026, 4, 1),
                11,
                "chair-1",
                "Rationale.",
                Array.Empty<EntrustmentEvidenceLink>()));

        Assert.Contains("expiry", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Revoke_OnlyAllowedFromActive()
    {
        var decision = BuildActive();
        decision.Revoke("Scope change.", "admin-1", DateTime.UtcNow);
        Assert.Equal(EntrustmentDecisionStatus.Revoked, decision.Status);
        Assert.Equal("Scope change.", decision.RevocationReason);
        Assert.Equal("admin-1", decision.RevokedByUserId);

        Assert.Throws<InvalidOperationException>(() => decision.Revoke("Another reason.", "admin-1", DateTime.UtcNow));
    }

    [Fact]
    public void Revoke_RequiresReason()
    {
        var decision = BuildActive();
        Assert.Throws<InvalidOperationException>(() => decision.Revoke("", "admin-1", DateTime.UtcNow));
    }

    [Fact]
    public void MarkExpired_RequiresPastExpiryAndActiveStatus()
    {
        var expiresOn = new DateOnly(2026, 4, 10);
        var decision = EntrustmentDecision.Issue(
            "trainee-1", 7, 3,
            new DateOnly(2026, 4, 1), expiresOn,
            11, "chair-1", "Rationale.", Array.Empty<EntrustmentEvidenceLink>());

        Assert.Throws<InvalidOperationException>(() =>
            decision.MarkExpired(expiresOn.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)));

        decision.MarkExpired(expiresOn.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        Assert.Equal(EntrustmentDecisionStatus.Expired, decision.Status);

        Assert.Throws<InvalidOperationException>(() =>
            decision.MarkExpired(expiresOn.AddDays(2).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)));
    }

    [Fact]
    public void MarkExpired_RequiresExpiryDate()
    {
        var decision = EntrustmentDecision.Issue(
            "trainee-1", 7, 3,
            new DateOnly(2026, 4, 1), expiresOn: null,
            11, "chair-1", "Rationale.", Array.Empty<EntrustmentEvidenceLink>());

        Assert.Throws<InvalidOperationException>(() => decision.MarkExpired(DateTime.UtcNow.AddYears(10)));
    }

    [Fact]
    public void SupersedeBy_OnlyAllowedFromActive()
    {
        var decision = BuildActive();
        decision.SupersedeBy(99);
        Assert.Equal(EntrustmentDecisionStatus.Superseded, decision.Status);
        Assert.Equal(99, decision.SupersededByDecisionId);

        Assert.Throws<InvalidOperationException>(() => decision.SupersedeBy(100));
    }

    [Fact]
    public void Amend_AlwaysThrows()
    {
        var decision = BuildActive();
        var exception = Assert.Throws<InvalidOperationException>(() => decision.Amend());
        Assert.Contains("immutable", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EvidenceLink_RequiresExactlyOneSourceId()
    {
        Assert.Throws<InvalidOperationException>(() => EntrustmentEvidenceLink.Create(
            EntrustmentEvidenceSourceType.Activity, null, null, null, "Label", "Summary", null));
        Assert.Throws<InvalidOperationException>(() => EntrustmentEvidenceLink.Create(
            EntrustmentEvidenceSourceType.Activity, 1, 2, null, "Label", "Summary", null));
    }

    [Fact]
    public void EvidenceLink_EnforcesSourceTypeMatchesPopulatedId()
    {
        Assert.Throws<InvalidOperationException>(() => EntrustmentEvidenceLink.Create(
            EntrustmentEvidenceSourceType.MsfCampaign, activityId: 1, msfCampaignId: null, committeeReviewId: null,
            sourceLabel: "Label", summary: "Summary", sourceRecordedOn: null));
    }

    private static EntrustmentDecision BuildActive(DateOnly? expiresOn = null) =>
        EntrustmentDecision.Issue(
            "trainee-1", 7, 3,
            new DateOnly(2026, 4, 1),
            expiresOn ?? new DateOnly(2027, 4, 1),
            11, "chair-1", "Rationale.", Array.Empty<EntrustmentEvidenceLink>());
}
