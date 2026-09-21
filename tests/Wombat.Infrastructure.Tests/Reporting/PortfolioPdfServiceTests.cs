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
using System.Security.Claims;

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
        var request = new PortfolioExportRequest("trainee-1", null, null, SubjectPrincipal("trainee-1"));

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
        var request = new PortfolioExportRequest("trainee-1", null, null, SubjectPrincipal("trainee-1"));

        var withStar = await service.GenerateAsync(request, CancellationToken.None);

        // Remove the active STAR; the portfolio bytes must change (proving the STAR section is rendered).
        db.Set<EntrustmentDecision>().RemoveRange(db.Set<EntrustmentDecision>());
        await db.SaveChangesAsync();

        var withoutStar = await service.GenerateAsync(request, CancellationToken.None);

        withoutStar.ContentHash.Should().NotBe(withStar.ContentHash);
    }

    /// <summary>
    /// A trainee with a released multi-source feedback campaign can still export their portfolio.
    /// </summary>
    /// <remarks>
    /// The export threw <c>NullReferenceException</c> for exactly this trainee. <c>MsfAggregationService</c>
    /// opens by grouping responses on <c>response.Invitation.RespondentCategory</c>, and the portfolio
    /// query included <c>Responses</c> and <c>Invitations</c> but never <c>Responses.Invitation</c>, so
    /// the navigation was null. It stayed invisible because the two tests above deliberately seed no
    /// campaign and inject a <c>ThrowingMsfAggregationService</c> — the MSF path was never entered at
    /// all. Found browser-verifying [T121], whose release now writes the first released campaign most
    /// portfolios will ever hold.
    /// </remarks>
    [Fact]
    public async Task Generate_SucceedsForATraineeWithAReleasedMsfCampaign()
    {
        await using var db = SeededDb();
        SeedReleasedCampaign(db);

        var service = new PortfolioPdfService(db, new MsfAggregationService());
        var request = new PortfolioExportRequest("trainee-1", null, null, SubjectPrincipal("trainee-1"));

        var export = await service.GenerateAsync(request, CancellationToken.None);

        export.PdfBytes.Should().NotBeEmpty();
    }

    private static void SeedReleasedCampaign(ApplicationDbContext db)
    {
        var template = new MsfTemplate
        {
            Name = "Annual MSF",
            Questions = [new MsfQuestion { Id = 1, Order = 1, Prompt = "Professional performance", Type = MsfQuestionType.Scale, Required = true }]
        };

        var campaign = new MsfCampaign
        {
            SubjectUserId = "trainee-1",
            CreatedByUserId = "coord-1",
            CreatedOn = DateTime.UtcNow,
            OpensOn = new DateOnly(2029, 1, 1),
            ClosesOn = new DateOnly(2029, 6, 30),
            MinimumResponses = 2,
            MinimumCategoryResponses = 2,
            MinimumRespondentCategories = 2,
            State = MsfCampaignState.Released,
            ReleasedOn = new DateTime(2029, 7, 1, 0, 0, 0, DateTimeKind.Utc),
            ReviewedByUserId = "coord-1",
            Template = template,
            CoveredEpas = [new MsfCampaignEpa { EpaId = 1 }]
        };

        foreach (var category in new[] { MsfRespondentCategory.Consultant, MsfRespondentCategory.Nurse })
        {
            var invitation = new MsfInvitation
            {
                RespondentCategory = category,
                RespondentEmailHash = "hash",
                TokenHash = Guid.NewGuid().ToString("N"),
                IssuedOn = new DateTime(2029, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                ExpiresOn = new DateOnly(2029, 6, 30),
                RespondedOn = new DateTime(2029, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                AnonymizedOn = new DateTime(2029, 6, 30, 0, 0, 0, DateTimeKind.Utc)
            };

            var response = new MsfResponse
            {
                SubmittedOn = new DateTime(2029, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                Campaign = campaign,
                Invitation = invitation,
                Answers = [new MsfResponseAnswer { QuestionId = 1, ScaleValue = 4 }]
            };

            invitation.Responses.Add(response);
            campaign.Invitations.Add(invitation);
            campaign.Responses.Add(response);
        }

        db.Set<MsfCampaign>().Add(campaign);
        db.SaveChanges();
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

    /// <summary>
    /// T101: the PDF's contents now obey the caller's read scope. These tests are about rendering, so
    /// they export as the subject, which resolves to exactly that trainee's own activities.
    /// </summary>
    private static ClaimsPrincipal SubjectPrincipal(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));

}
