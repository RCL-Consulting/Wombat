using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Reporting;
using Wombat.Domain.Identity;
using Wombat.Domain.Reporting;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Reporting;

public sealed class ExportPortfolioCommandHandlerTests
{
    private const int InstitutionId = 1;
    private const int OtherInstitutionId = 2;
    private const int SpecialityId = 10;
    private const int OtherSpecialityId = 11;
    private const int SubSpecialityId = 100;
    private const int CurriculumId = 3000;

    [Fact]
    public async Task Handle_PersistsExportRecordWithCorrectHash()
    {
        await using var db = CreateDb();

        var expectedHash = "abc123def456";
        var expectedFileName = "portfolio-abc123def456.pdf";
        var pdfBytes = new byte[] { 1, 2, 3 };

        var pdfService = new Mock<IPortfolioPdfService>();
        pdfService
            .Setup(s => s.GenerateAsync(It.IsAny<PortfolioExportRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PortfolioExportResult(pdfBytes, expectedFileName, expectedHash));

        var principal = CreatePrincipal("user-1", WombatRoles.Administrator);
        var handler = new ExportPortfolioCommandHandler(db, pdfService.Object);

        var result = await handler.Handle(
            new ExportPortfolioCommand("trainee-1", null, null, principal),
            CancellationToken.None);

        result.ContentHash.Should().Be(expectedHash);
        result.FileName.Should().Be(expectedFileName);
        result.PdfBytes.Should().BeSameAs(pdfBytes);

        var record = await db.Set<PortfolioExport>().FirstOrDefaultAsync();
        record.Should().NotBeNull();
        record!.TraineeUserId.Should().Be("trainee-1");
        record.ExportedByUserId.Should().Be("user-1");
        record.ContentHash.Should().Be(expectedHash);
        record.FileName.Should().Be(expectedFileName);
    }

    [Fact]
    public async Task Handle_TraineeCanExportOwnPortfolio()
    {
        await using var db = CreateDb();

        var pdfService = StubPdfService();
        var principal = CreatePrincipal("trainee-1", WombatRoles.Trainee);
        var handler = new ExportPortfolioCommandHandler(db, pdfService.Object);

        var act = () => handler.Handle(
            new ExportPortfolioCommand("trainee-1", null, null, principal),
            CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_CoordinatorInTraineesInstitutionCanExport()
    {
        await using var db = CreateDb();
        await SeedTraineeProfileAsync(db);

        var pdfService = StubPdfService();
        var principal = CreatePrincipal("coord-1", WombatRoles.Coordinator, institutionId: InstitutionId);
        var handler = new ExportPortfolioCommandHandler(db, pdfService.Object);

        var act = () => handler.Handle(
            new ExportPortfolioCommand("trainee-1", null, null, principal),
            CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// The T101 defect: the oversight roles were let through on the role alone, so this coordinator
    /// could export a whole portfolio — every assessment's clinical data — out of another institution
    /// by editing the route id.
    /// </summary>
    [Fact]
    public async Task Handle_CoordinatorInAnotherInstitutionCannotExport()
    {
        await using var db = CreateDb();
        await SeedTraineeProfileAsync(db);

        var pdfService = StubPdfService();
        var principal = CreatePrincipal("coord-2", WombatRoles.Coordinator, institutionId: OtherInstitutionId);
        var handler = new ExportPortfolioCommandHandler(db, pdfService.Object);

        var act = () => handler.Handle(
            new ExportPortfolioCommand("trainee-1", null, null, principal),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();

        pdfService.Verify(
            s => s.GenerateAsync(It.IsAny<PortfolioExportRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
        (await db.Set<PortfolioExport>().CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Handle_InstitutionalAdminWithoutMatchingInstitutionClaimCannotExport()
    {
        await using var db = CreateDb();
        await SeedTraineeProfileAsync(db);

        var pdfService = StubPdfService();
        var principal = CreatePrincipal("inst-admin-1", WombatRoles.InstitutionalAdmin, institutionId: null);
        var handler = new ExportPortfolioCommandHandler(db, pdfService.Object);

        var act = () => handler.Handle(
            new ExportPortfolioCommand("trainee-1", null, null, principal),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_SpecialityAdminInTraineesSpecialityCanExport()
    {
        await using var db = CreateDb();
        await SeedTraineeProfileAsync(db);

        var pdfService = StubPdfService();
        // Both claims. A Speciality is College-owned, so the speciality id is national: T101 requires
        // the institution as well, or one hospital's admin could export the country.
        var principal = CreatePrincipal(
            "spec-admin-1",
            WombatRoles.SpecialityAdmin,
            institutionId: InstitutionId,
            specialityId: SpecialityId);
        var handler = new ExportPortfolioCommandHandler(db, pdfService.Object);

        var act = () => handler.Handle(
            new ExportPortfolioCommand("trainee-1", null, null, principal),
            CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_SpecialityAdminInAnotherSpecialityCannotExport()
    {
        await using var db = CreateDb();
        await SeedTraineeProfileAsync(db);

        var pdfService = StubPdfService();
        var principal = CreatePrincipal(
            "spec-admin-2",
            WombatRoles.SpecialityAdmin,
            specialityId: OtherSpecialityId);
        var handler = new ExportPortfolioCommandHandler(db, pdfService.Object);

        var act = () => handler.Handle(
            new ExportPortfolioCommand("trainee-1", null, null, principal),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    /// <summary>
    /// T101. The speciality id is national (a <c>Speciality</c> carries a <c>CollegeId</c>), so a
    /// SpecialityAdmin of the right speciality at the WRONG hospital must be refused — otherwise the
    /// export is a national one.
    /// </summary>
    [Fact]
    public async Task Handle_SpecialityAdminAtAnotherInstitutionCannotExport()
    {
        await using var db = CreateDb();
        await SeedTraineeProfileAsync(db);

        var pdfService = StubPdfService();
        var principal = CreatePrincipal(
            "spec-admin-elsewhere",
            WombatRoles.SpecialityAdmin,
            institutionId: OtherInstitutionId,
            specialityId: SpecialityId);
        var handler = new ExportPortfolioCommandHandler(db, pdfService.Object);

        var act = () => handler.Handle(
            new ExportPortfolioCommand("trainee-1", null, null, principal),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_SubSpecialityAdminInTraineesSubSpecialityCanExport()
    {
        await using var db = CreateDb();
        await SeedTraineeProfileAsync(db);

        var pdfService = StubPdfService();
        var principal = CreatePrincipal(
            "sub-admin-1",
            WombatRoles.SubSpecialityAdmin,
            institutionId: InstitutionId,
            subSpecialityId: SubSpecialityId);
        var handler = new ExportPortfolioCommandHandler(db, pdfService.Object);

        var act = () => handler.Handle(
            new ExportPortfolioCommand("trainee-1", null, null, principal),
            CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// No profile means no organisational home, so no scoped role can be held over the trainee —
    /// only a global Administrator or the trainee themselves may export. The same path answers a
    /// user id that matches nobody.
    /// </summary>
    [Fact]
    public async Task Handle_TraineeWithoutProfileCannotBeExportedByScopedOverseer()
    {
        await using var db = CreateDb();

        var pdfService = StubPdfService();
        var principal = CreatePrincipal("coord-1", WombatRoles.Coordinator, institutionId: InstitutionId);
        var handler = new ExportPortfolioCommandHandler(db, pdfService.Object);

        var act = () => handler.Handle(
            new ExportPortfolioCommand("trainee-1", null, null, principal),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_UnauthorizedUserCannotExportOtherPortfolio()
    {
        await using var db = CreateDb();
        await SeedTraineeProfileAsync(db);

        var pdfService = new Mock<IPortfolioPdfService>();
        var principal = CreatePrincipal("other-user", WombatRoles.Trainee, institutionId: InstitutionId);
        var handler = new ExportPortfolioCommandHandler(db, pdfService.Object);

        var act = () => handler.Handle(
            new ExportPortfolioCommand("trainee-1", null, null, principal),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_PassesDateFiltersToService()
    {
        await using var db = CreateDb();

        PortfolioExportRequest? captured = null;
        var pdfService = new Mock<IPortfolioPdfService>();
        pdfService
            .Setup(s => s.GenerateAsync(It.IsAny<PortfolioExportRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PortfolioExportRequest, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(new PortfolioExportResult([], "test.pdf", "hash"));

        var from = new DateOnly(2025, 1, 1);
        var to = new DateOnly(2025, 12, 31);
        var principal = CreatePrincipal("admin-1", WombatRoles.Administrator);
        var handler = new ExportPortfolioCommandHandler(db, pdfService.Object);

        await handler.Handle(
            new ExportPortfolioCommand("trainee-1", from, to, principal),
            CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.TraineeUserId.Should().Be("trainee-1");
        captured.FromDate.Should().Be(from);
        captured.ToDate.Should().Be(to);
    }

    // ─── Fixtures ────────────────────────────────────────────────────────────

    private static ApplicationDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static Mock<IPortfolioPdfService> StubPdfService()
    {
        var pdfService = new Mock<IPortfolioPdfService>();
        pdfService
            .Setup(s => s.GenerateAsync(It.IsAny<PortfolioExportRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PortfolioExportResult([], "test.pdf", "hash"));
        return pdfService;
    }

    /// <summary>
    /// trainee-1's organisational home: institution 1, speciality 10, sub-speciality 100 — the three
    /// ids the export gate resolves through the profile's curriculum.
    /// </summary>
    private static async Task SeedTraineeProfileAsync(ApplicationDbContext dbContext)
    {
        dbContext.Set<Wombat.Domain.Institutions.Speciality>().Add(new Wombat.Domain.Institutions.Speciality
        {
            Id = SpecialityId,
            Name = "Paediatrics"
        });

        dbContext.Set<Wombat.Domain.Institutions.SubSpeciality>().Add(new Wombat.Domain.Institutions.SubSpeciality
        {
            Id = SubSpecialityId,
            SpecialityId = SpecialityId,
            Name = "General Paediatrics"
        });

        dbContext.Set<Wombat.Domain.Curricula.Curriculum>().Add(new Wombat.Domain.Curricula.Curriculum
        {
            Id = CurriculumId,
            SubSpecialityId = SubSpecialityId,
            Name = "Paediatric EPA Curriculum",
            Version = "11.1"
        });

        dbContext.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = "trainee-1",
            InstitutionId = InstitutionId,
            CurriculumId = CurriculumId,
            ProgrammeStartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-1),
            ExpectedCompletionDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(2),
            IsActive = true
        });

        await dbContext.SaveChangesAsync();
    }

    private static ClaimsPrincipal CreatePrincipal(
        string userId,
        string role,
        int? institutionId = null,
        int? specialityId = null,
        int? subSpecialityId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, role)
        };

        if (institutionId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, institutionId.Value.ToString()));
        }

        if (specialityId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.SpecialityId, specialityId.Value.ToString()));
        }

        if (subSpecialityId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.SubSpecialityId, subSpecialityId.Value.ToString()));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }
}
