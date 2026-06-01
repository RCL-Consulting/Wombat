using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Features.Reporting;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Reporting;

namespace Wombat.Infrastructure.Tests.Reporting;

/// <summary>
/// T078 / F-5-3: the portfolio PDF must be byte-for-byte reproducible from the same data so the
/// content hash (used by the file name and /portfolio/verify) is stable. Previously the cover page
/// rendered <c>Generated: {DateTime.UtcNow}</c> and QuestPDF stamped DateTime.Now metadata, so two
/// exports a minute apart produced different bytes.
/// </summary>
public sealed class PortfolioPdfServiceTests
{
    static PortfolioPdfServiceTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    [Fact]
    public async Task Generate_IsByteForByteDeterministic()
    {
        await using var db = SeededDb();
        var service = new PortfolioPdfService(db, new ThrowingMsfAggregationService());
        var request = new PortfolioExportRequest("trainee-1", null, null);

        var first = await service.GenerateAsync(request, CancellationToken.None);
        var second = await service.GenerateAsync(request, CancellationToken.None);

        first.PdfBytes.Should().NotBeEmpty();
        second.ContentHash.Should().Be(first.ContentHash);
        second.PdfBytes.Should().Equal(first.PdfBytes);
        second.FileName.Should().Be(first.FileName);
    }

    [Fact]
    public async Task Generate_StarsAffectOutput()
    {
        await using var db = SeededDb();
        var service = new PortfolioPdfService(db, new ThrowingMsfAggregationService());
        var request = new PortfolioExportRequest("trainee-1", null, null);

        var withStar = await service.GenerateAsync(request, CancellationToken.None);

        // Remove the active STAR; the portfolio bytes must change (proving the STAR section is rendered).
        db.Set<EntrustmentDecision>().RemoveRange(db.Set<EntrustmentDecision>());
        await db.SaveChangesAsync();

        var withoutStar = await service.GenerateAsync(request, CancellationToken.None);

        withoutStar.ContentHash.Should().NotBe(withStar.ContentHash);
    }

    private static ApplicationDbContext SeededDb()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        db.Set<WombatIdentityUser>().Add(new WombatIdentityUser { Id = "trainee-1", FirstName = "Lerato", LastName = "Molefe" });
        db.Set<EntrustmentScale>().Add(new EntrustmentScale { Id = 2, Name = "Paed General Entrustment Scale" });
        db.Set<EntrustmentLevel>().Add(new EntrustmentLevel { Id = 9, ScaleId = 2, Order = 4, Label = "Unsupervised" });
        db.Set<Epa>().Add(new Epa { Id = 1, SubSpecialityId = 1, Code = "PAED-001", Title = "Acute admission", IsActive = true });
        db.Set<EntrustmentDecision>().Add(EntrustmentDecision.Issue(
            "trainee-1", epaId: 1, authorisedLevelId: 9, issuedOn: new DateOnly(2029, 11, 18),
            expiresOn: null, committeeReviewId: 1, chairUserId: "chair-1", rationale: "Target met.",
            evidenceLinks: Array.Empty<EntrustmentEvidenceLink>()));
        db.SaveChanges();
        return db;
    }

    private sealed class ThrowingMsfAggregationService : IMsfAggregationService
    {
        // Never reached: the seeded data has no released MSF campaigns.
        public MsfCampaignAggregateReportDto BuildReport(MsfCampaign campaign) => throw new NotSupportedException();
    }
}
