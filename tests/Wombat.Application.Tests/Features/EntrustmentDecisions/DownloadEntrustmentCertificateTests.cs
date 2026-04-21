using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.EntrustmentDecisions;

public sealed class DownloadEntrustmentCertificateTests
{
    [Fact]
    public async Task TraineeCanDownloadOwnCertificate()
    {
        await using var dbContext = CreateDbContext();
        var decisionId = await SeedIssuedDecisionAsync(dbContext);

        var pdf = new FakePdfService();
        var handler = new DownloadEntrustmentCertificateCommandHandler(dbContext, pdf);

        var result = await handler.Handle(
            new DownloadEntrustmentCertificateCommand(decisionId, CreatePrincipal("trainee-1", [WombatRoles.Trainee])),
            CancellationToken.None);

        result.FileName.Should().NotBeNullOrWhiteSpace();
        pdf.Calls.Should().Be(1);
    }

    [Fact]
    public async Task DifferentTraineeCannotDownload()
    {
        await using var dbContext = CreateDbContext();
        var decisionId = await SeedIssuedDecisionAsync(dbContext);

        var pdf = new FakePdfService();
        var handler = new DownloadEntrustmentCertificateCommandHandler(dbContext, pdf);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(
            new DownloadEntrustmentCertificateCommand(decisionId, CreatePrincipal("other-trainee", [WombatRoles.Trainee])),
            CancellationToken.None));

        pdf.Calls.Should().Be(0);
    }

    [Fact]
    public async Task InstitutionalAdminCanDownloadAny()
    {
        await using var dbContext = CreateDbContext();
        var decisionId = await SeedIssuedDecisionAsync(dbContext);

        var pdf = new FakePdfService();
        var handler = new DownloadEntrustmentCertificateCommandHandler(dbContext, pdf);

        var result = await handler.Handle(
            new DownloadEntrustmentCertificateCommand(decisionId, CreatePrincipal("admin-1", [WombatRoles.InstitutionalAdmin])),
            CancellationToken.None);

        result.FileName.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task IssuingPanelChairCanDownload()
    {
        await using var dbContext = CreateDbContext();
        var decisionId = await SeedIssuedDecisionAsync(dbContext);

        var pdf = new FakePdfService();
        var handler = new DownloadEntrustmentCertificateCommandHandler(dbContext, pdf);

        var result = await handler.Handle(
            new DownloadEntrustmentCertificateCommand(decisionId, CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

        result.FileName.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task UnrelatedAssessorCannotDownload()
    {
        await using var dbContext = CreateDbContext();
        var decisionId = await SeedIssuedDecisionAsync(dbContext);

        var pdf = new FakePdfService();
        var handler = new DownloadEntrustmentCertificateCommandHandler(dbContext, pdf);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(
            new DownloadEntrustmentCertificateCommand(decisionId, CreatePrincipal("assessor-1", [WombatRoles.Assessor])),
            CancellationToken.None));
    }

    [Fact]
    public async Task MissingDecisionThrows()
    {
        await using var dbContext = CreateDbContext();
        var pdf = new FakePdfService();
        var handler = new DownloadEntrustmentCertificateCommandHandler(dbContext, pdf);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(
            new DownloadEntrustmentCertificateCommand(999, CreatePrincipal("admin-1", [WombatRoles.InstitutionalAdmin])),
            CancellationToken.None));
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task<int> SeedIssuedDecisionAsync(ApplicationDbContext dbContext)
    {
        var institution = new Institution { Id = 1, Name = "Test Hospital", IsActive = true, CreatedOn = DateTime.UtcNow };
        var speciality = new Speciality { Id = 5, InstitutionId = 1, Name = "General Medicine", IsActive = true };
        var subSpec = new SubSpeciality { Id = 9, SpecialityId = 5, Name = "Acute Care", IsActive = true };

        var scale = new EntrustmentScale { Id = 1, Name = "Standard 5-point" };
        var level = new EntrustmentLevel { Id = 3, ScaleId = 1, Order = 3, Label = "Indirect supervision" };
        var epa = new Epa { Id = 7, SubSpecialityId = 9, Code = "EPA-07", Title = "Emergency triage", IsActive = true };

        var panel = new DecisionPanel
        {
            Id = 20,
            Name = "ARCP panel",
            Scope = DecisionPanelScope.Speciality,
            SpecialityId = 5,
            CreatedOn = DateTime.UtcNow,
            Members =
            [
                new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair }
            ]
        };

        var review = new CommitteeReview
        {
            Id = 30,
            Panel = panel,
            TraineeUserId = "trainee-1",
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 3, 31),
            ScheduledOn = new DateOnly(2026, 4, 1)
        };

        dbContext.Institutions.Add(institution);
        dbContext.Specialities.Add(speciality);
        dbContext.SubSpecialities.Add(subSpec);
        dbContext.EntrustmentScales.Add(scale);
        dbContext.EntrustmentLevels.Add(level);
        dbContext.Epas.Add(epa);
        dbContext.DecisionPanels.Add(panel);
        dbContext.CommitteeReviews.Add(review);
        await dbContext.SaveChangesAsync();

        var startHandler = new StartCommitteeReviewCommandHandler(dbContext);
        await startHandler.Handle(new StartCommitteeReviewCommand(review.Id, CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])), CancellationToken.None);
        var recordHandler = new RecordCommitteeDecisionCommandHandler(dbContext);
        await recordHandler.Handle(
            new RecordCommitteeDecisionCommand(review.Id, CommitteeDecisionCategory.SatisfactoryProgress, "Satisfactory.", null,
                CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);
        var ratifyHandler = new RatifyCommitteeDecisionCommandHandler(dbContext);
        await ratifyHandler.Handle(new RatifyCommitteeDecisionCommand(review.Id, CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])), CancellationToken.None);

        var issueHandler = new IssueEntrustmentDecisionCommandHandler(dbContext);
        var issued = await issueHandler.Handle(
            new IssueEntrustmentDecisionCommand("trainee-1", 7, 3, new DateOnly(2026, 4, 1), new DateOnly(2027, 4, 1),
                review.Id, "Sufficient evidence.",
                Array.Empty<EntrustmentEvidenceLinkInput>(),
                CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

        return issued.Id;
    }

    private static ClaimsPrincipal CreatePrincipal(string userId, IReadOnlyCollection<string> roles)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        claims.Add(new Claim(WombatClaimTypes.SpecialityId, "5"));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private sealed class FakePdfService : IEntrustmentCertificatePdfService
    {
        public int Calls { get; private set; }

        public Task<EntrustmentCertificateResult> GenerateAsync(EntrustmentCertificateRequest request, CancellationToken cancellationToken)
        {
            Calls++;
            var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
            return Task.FromResult(new EntrustmentCertificateResult(pdfBytes, $"certificate-{request.DecisionId}.pdf", "fakehash"));
        }
    }
}
