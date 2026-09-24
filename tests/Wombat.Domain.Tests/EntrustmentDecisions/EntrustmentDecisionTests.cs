using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Domain.Tests.EntrustmentDecisions;

public sealed class EntrustmentDecisionTests
{
    private static readonly DateTime Recorded = new(2026, 3, 2, 8, 30, 0, DateTimeKind.Utc);

    private static CommitteeEvidence ActivityLine(int id = 501) => new()
    {
        Id = id,
        ReviewId = 11,
        SourceType = CommitteeEvidenceSourceType.Activity,
        ActivityId = 42,
        SourceLabel = "Mini-CEX #42",
        Summary = "Direct observation.",
        SourceRecordedOn = Recorded
    };

    private static EntrustmentEvidenceLink SampleEvidence() => EntrustmentEvidenceLink.FromSnapshot(ActivityLine());

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
                [SampleEvidence()]));

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
                [SampleEvidence()]));

        Assert.Contains("expiry", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Issue_RefusesADecisionThatRestsOnNoEvidence()
    {
        // D38 (T131): the committee's decision draws on the assessment evidence, never on none.
        var exception = Assert.Throws<InvalidOperationException>(() => EntrustmentDecision.Issue(
            "trainee-1", 7, 3, new DateOnly(2026, 4, 1), null, 11, "chair-1", "Rationale.",
            Array.Empty<EntrustmentEvidenceLink>()));

        Assert.Equal(EntrustmentDecision.EvidenceRequired, exception.Message);
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
            11, "chair-1", "Rationale.", [SampleEvidence()]);

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
            11, "chair-1", "Rationale.", [SampleEvidence()]);

        Assert.Throws<InvalidOperationException>(() => decision.MarkExpired(DateTime.UtcNow.AddYears(10)));
    }

    [Fact]
    public void SupersedeBy_OnlyAllowedFromActive_AndThroughTheSuccessorBeforeItHasAnId()
    {
        var decision = BuildActive();
        var successor = BuildActive();

        decision.SupersedeBy(successor);

        Assert.Equal(EntrustmentDecisionStatus.Superseded, decision.Status);
        Assert.Same(successor, decision.SupersededByDecision);
        Assert.Null(decision.SupersededByDecisionId); // the save fills it from the navigation
        Assert.Throws<InvalidOperationException>(() => decision.SupersedeBy(BuildActive()));
    }

    [Fact]
    public void SupersedeBy_RefusesItself_AndAStarOnAnotherEpa()
    {
        var decision = BuildActive();
        var otherEpa = EntrustmentDecision.Issue(
            "trainee-1", 8, 3, new DateOnly(2026, 4, 1), null, 11, "chair-1", "Rationale.", [SampleEvidence()]);

        Assert.Throws<InvalidOperationException>(() => decision.SupersedeBy(decision));
        Assert.Throws<InvalidOperationException>(() => decision.SupersedeBy(otherEpa));
        Assert.Equal(EntrustmentDecisionStatus.Active, decision.Status);
    }

    [Fact]
    public void Amend_AlwaysThrows()
    {
        var decision = BuildActive();
        var exception = Assert.Throws<InvalidOperationException>(() => decision.Amend());
        Assert.Contains("immutable", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AnEvidenceLink_IsWhatItsSnapshotLineSays_AndNamesTheLine()
    {
        var line = ActivityLine(id: 777);

        var link = EntrustmentEvidenceLink.FromSnapshot(line);

        Assert.Equal(EntrustmentEvidenceSourceType.Activity, link.SourceType);
        Assert.Equal(42, link.ActivityId);
        Assert.Null(link.MsfCampaignId);
        Assert.Null(link.CommitteeReviewId);
        Assert.Equal(777, link.CommitteeEvidenceId);
        Assert.Equal("Mini-CEX #42", link.SourceLabel);
        Assert.Equal("Direct observation.", link.Summary);
        Assert.Equal(Recorded, link.SourceRecordedOn);
    }

    [Fact]
    public void AnMsfCampaignLine_BecomesACampaignLink()
    {
        var link = EntrustmentEvidenceLink.FromSnapshot(new CommitteeEvidence
        {
            Id = 12,
            SourceType = CommitteeEvidenceSourceType.MsfCampaign,
            MsfCampaignId = 5,
            SourceLabel = "Annual MSF #5",
            Summary = "State: Released; responses 8."
        });

        Assert.Equal(EntrustmentEvidenceSourceType.MsfCampaign, link.SourceType);
        Assert.Equal(5, link.MsfCampaignId);
        Assert.Null(link.ActivityId);
        Assert.Equal(12, link.CommitteeEvidenceId);
    }

    [Fact]
    public void ASupervisorReportLine_CannotGroundADecision()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => EntrustmentEvidenceLink.FromSnapshot(new CommitteeEvidence
        {
            Id = 13,
            SourceType = CommitteeEvidenceSourceType.SupervisorReport,
            SupervisorReportId = 3,
            SourceLabel = "Supervisor report #3"
        }));

        Assert.Contains("supervisor report", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ALineNotYetStored_OrNamingNoSource_IsRefused()
    {
        Assert.Throws<InvalidOperationException>(() => EntrustmentEvidenceLink.FromSnapshot(ActivityLine(id: 0)));

        var noActivity = ActivityLine();
        noActivity.ActivityId = null;
        Assert.Throws<InvalidOperationException>(() => EntrustmentEvidenceLink.FromSnapshot(noActivity));

        var noLabel = ActivityLine();
        noLabel.SourceLabel = " ";
        Assert.Throws<InvalidOperationException>(() => EntrustmentEvidenceLink.FromSnapshot(noLabel));
    }

    private static EntrustmentDecision BuildActive(DateOnly? expiresOn = null) =>
        EntrustmentDecision.Issue(
            "trainee-1", 7, 3,
            new DateOnly(2026, 4, 1),
            expiresOn ?? new DateOnly(2027, 4, 1),
            11, "chair-1", "Rationale.", [SampleEvidence()]);
}
