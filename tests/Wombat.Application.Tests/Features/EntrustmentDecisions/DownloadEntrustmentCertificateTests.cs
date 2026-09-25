using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

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

        result!.FileName.Should().NotBeNullOrWhiteSpace();
        pdf.Calls.Should().Be(1);
    }

    [Fact]
    public async Task DifferentTraineeCannotDownload()
    {
        await using var dbContext = CreateDbContext();
        var decisionId = await SeedIssuedDecisionAsync(dbContext);

        var pdf = new FakePdfService();
        var handler = new DownloadEntrustmentCertificateCommandHandler(dbContext, pdf);

        var result = await handler.Handle(
            new DownloadEntrustmentCertificateCommand(decisionId, CreatePrincipal("other-trainee", [WombatRoles.Trainee])),
            CancellationToken.None);

        result.Should().BeNull();
        pdf.Calls.Should().Be(0);
    }

    [Fact]
    public async Task InstitutionalAdminOfTheTraineesInstitutionCanDownload()
    {
        await using var dbContext = CreateDbContext();
        var decisionId = await SeedIssuedDecisionAsync(dbContext);

        var pdf = new FakePdfService();
        var handler = new DownloadEntrustmentCertificateCommandHandler(dbContext, pdf);

        var result = await handler.Handle(
            new DownloadEntrustmentCertificateCommand(decisionId, CreatePrincipal("admin-1", [WombatRoles.InstitutionalAdmin], institutionId: 1)),
            CancellationToken.None);

        result!.FileName.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task InstitutionalAdminOfAnotherInstitutionGetsNothing()
    {
        // Until T183 the role alone was enough, wherever the trainee trained.
        await using var dbContext = CreateDbContext();
        var decisionId = await SeedIssuedDecisionAsync(dbContext);

        var pdf = new FakePdfService();
        var handler = new DownloadEntrustmentCertificateCommandHandler(dbContext, pdf);

        var result = await handler.Handle(
            new DownloadEntrustmentCertificateCommand(decisionId, CreatePrincipal("admin-2", [WombatRoles.InstitutionalAdmin], institutionId: 2)),
            CancellationToken.None);

        result.Should().BeNull();
        pdf.Calls.Should().Be(0);
    }

    [Fact]
    public async Task TheChairWhoIssuedItCanDownload()
    {
        await using var dbContext = CreateDbContext();
        var decisionId = await SeedIssuedDecisionAsync(dbContext);

        var pdf = new FakePdfService();
        var handler = new DownloadEntrustmentCertificateCommandHandler(dbContext, pdf);

        var result = await handler.Handle(
            new DownloadEntrustmentCertificateCommand(decisionId, CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

        result!.FileName.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task UnrelatedAssessorCannotDownload()
    {
        await using var dbContext = CreateDbContext();
        var decisionId = await SeedIssuedDecisionAsync(dbContext);

        var pdf = new FakePdfService();
        var handler = new DownloadEntrustmentCertificateCommandHandler(dbContext, pdf);

        var result = await handler.Handle(
            new DownloadEntrustmentCertificateCommand(decisionId, CreatePrincipal("assessor-1", [WombatRoles.Assessor], institutionId: 1)),
            CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task MissingDecisionIsNull_AsAnOutOfScopeOneIs()
    {
        await using var dbContext = CreateDbContext();
        var pdf = new FakePdfService();
        var handler = new DownloadEntrustmentCertificateCommandHandler(dbContext, pdf);

        var result = await handler.Handle(
            new DownloadEntrustmentCertificateCommand(999, CreatePrincipal("admin-1", [WombatRoles.InstitutionalAdmin], institutionId: 1)),
            CancellationToken.None);

        result.Should().BeNull();
        pdf.Calls.Should().Be(0);
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
        var speciality = new Speciality { Id = 5, CollegeId = 1, Name = "General Medicine", IsActive = true };
        var subSpec = new SubSpeciality { Id = 9, SpecialityId = 5, Name = "Acute Care", IsActive = true };

        var scale = new EntrustmentScale { Id = 1, Name = "Standard 5-point" };
        var level = new EntrustmentLevel { Id = 3, ScaleId = 1, Order = 3, Label = "Indirect supervision" };
        var epa = new Epa { Id = 7, SubSpecialityId = 9, Code = "EPA-07", Title = "Emergency triage", IsActive = true };

        var panel = new DecisionPanel
        {
            Id = 20,
            Name = "ARCP panel",
            Scope = DecisionPanelScope.Speciality,
            InstitutionId = 1,
            SpecialityId = 5,
            CreatedOn = DateTime.UtcNow,
            Members =
            [
                new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair },
                new DecisionPanelMember { UserId = "member-1", Role = DecisionPanelMemberRole.Member }
            ]
        };

        var review = new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 1,
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

        // The trainee trains at the test hospital (T183), which is the panel's institution: a panel acts only on its own
        // institution's trainees (T182). A STAR is granted only on an EPA of the trainee's curriculum (T167).
        dbContext.Curricula.Add(new Curriculum { Id = 40, SubSpecialityId = 9, Name = "Acute Care", Version = "1" });
        dbContext.CurriculumItems.Add(new CurriculumItem { Id = 41, CurriculumId = 40, EpaId = 7, RequiredCount = 1, MinimumLevelOrder = 3 });
        dbContext.Set<TraineeProfile>().Add(new TraineeProfile
        {
            UserId = "trainee-1", InstitutionId = 1, CurriculumId = 40,
            ProgrammeStartDate = new DateOnly(2025, 1, 1), ExpectedCompletionDate = new DateOnly(2029, 1, 1), IsActive = true
        });
        dbContext.EntrustmentScales.Add(scale);
        dbContext.EntrustmentLevels.Add(level);
        dbContext.Epas.Add(epa);
        dbContext.DecisionPanels.Add(panel);
        dbContext.CommitteeReviews.Add(review);
        await dbContext.SaveChangesAsync();

        var startHandler = new StartCommitteeReviewCommandHandler(dbContext, FakeUserDirectory.PanelMembersOf(dbContext));
        await startHandler.Handle(new StartCommitteeReviewCommand(review.Id, CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])), CancellationToken.None);

        // The window held no activity, so the snapshot line the STAR rests on is written here (D38, T131).
        var line = new CommitteeEvidence
        {
            ReviewId = review.Id,
            SourceType = CommitteeEvidenceSourceType.Activity,
            ActivityId = 101,
            EpaId = 7,
            SourceLabel = "Mini-CEX #101",
            Summary = "State: completed."
        };
        dbContext.Set<CommitteeEvidence>().Add(line);
        await dbContext.SaveChangesAsync();

        // Staged at the sitting and issued by ratifying: since T165 the only way a STAR is issued.
        await new StagePendingEntrustmentDecisionCommandHandler(dbContext, FakeUserDirectory.PanelMembersOf(dbContext)).Handle(
            new StagePendingEntrustmentDecisionCommand(review.Id, null, 7, 3, new DateOnly(2026, 4, 1), new DateOnly(2027, 4, 1),
                "Sufficient evidence.", [line.Id], CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);
        var recordHandler = new RecordCommitteeDecisionCommandHandler(
            dbContext, FakeUserDirectory.CommitteeMembersAt(1, "chair-1", "member-1"));
        await recordHandler.Handle(
            new RecordCommitteeDecisionCommand(review.Id, CommitteeDecisionCategory.SatisfactoryProgress, "Satisfactory.", null, ["chair-1", "member-1"],
                CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);
        var ratifyHandler = new RatifyCommitteeDecisionCommandHandler(dbContext, FakeUserDirectory.PanelMembersOf(dbContext));
        await ratifyHandler.Handle(new RatifyCommitteeDecisionCommand(review.Id, CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])), CancellationToken.None);

        return await dbContext.Set<EntrustmentDecision>()
            .Where(decision => decision.IssuedByCommitteeReviewId == review.Id)
            .Select(decision => decision.Id)
            .SingleAsync();
    }

    private static ClaimsPrincipal CreatePrincipal(string userId, IReadOnlyCollection<string> roles, int? institutionId = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        claims.Add(new Claim(WombatClaimTypes.SpecialityId, "5"));
        if (institutionId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, institutionId.Value.ToString()));
        }

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
