using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Features.Reporting;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
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
[Collection(QuestPdfRenderingCollection.Name)]
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
        // The per-EPA section prints the day it reads the targets on (T169), which for an open-ended export is today.
        // Pinned, so two exports that straddle midnight in South Africa are still the same data on the same day.
        var service = new PortfolioPdfService(
            db, new ThrowingMsfAggregationService(), new FixedClock(new DateTimeOffset(2026, 9, 23, 8, 0, 0, TimeSpan.Zero)));
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

    /// <summary>
    /// The MSF section says which declared EPAs the campaign recorded from its evidence rows, as the committee snapshot,
    /// the coverage grid and the campaign report have it, not from the per-EPA stamp. A campaign released before the
    /// stamp existed has its rows and a null stamp, and printed every EPA "(not recorded)" beside the activities section
    /// listing its records. (T186)
    /// </summary>
    [Fact]
    public async Task TheMsfSection_ReadsRecordedFromTheEvidenceRows_NotFromThePerEpaStamp()
    {
        await using var db = SeededDb();
        db.Set<Epa>().Add(new Epa { Id = 2, SubSpecialityId = 1, Code = "PAED-002", Title = "Chronic care", IsActive = true });
        var campaign = SeedReleasedCampaign(db, coveredEpaIds: [1, 2]);

        // PAED-001: an evidence row and a null stamp, the campaign released before the stamp existed. PAED-002: stamped,
        // with no row behind it.
        campaign.CoveredEpas.Single(covered => covered.EpaId == 2).RecordedOn = campaign.ReleasedOn;
        db.Set<ActivityType>().Add(new ActivityType
        {
            Id = 30,
            Key = MsfCampaignCoverage.MsfEvidenceTypeKey,
            Name = "Multi-Source Feedback (Paediatrics)",
            Version = 1,
            WorkflowJson = File.ReadAllText(
                Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", "msf_cpsa", "workflow.json")),
            OwnerUserId = "seed-system",
            CreatedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
        db.Set<Activity>().Add(new Activity
        {
            ActivityTypeId = 30,
            SchemaVersion = 1,
            SubjectUserId = "trainee-1",
            CreatedByUserId = "coord-1",
            CurrentState = "recorded",
            DataJson = $$"""{ "epa_id": 1, "{{MsfCampaignCoverage.CampaignIdField}}": {{campaign.Id}}, "respondent_count": 2 }""",
            EpaId = 1,
            CreatedOn = campaign.ReleasedOn!.Value,
            UpdatedOn = campaign.ReleasedOn!.Value,
            ObservedOn = campaign.ClosesOn,
            ObservedOnSource = ObservationDateSource.Declared
        });
        await db.SaveChangesAsync();

        var data = await new PortfolioPdfService(db, new MsfAggregationService()).LoadPortfolioDataAsync(
            new PortfolioExportRequest("trainee-1", null, null, SubjectPrincipal("trainee-1")),
            CancellationToken.None);

        data.MsfReports.Should().ContainSingle().Which.CoveredEpas
            .Select(covered => (covered.Code, covered.Recorded))
            .Should().Equal(("PAED-001", true), ("PAED-002", false));
    }

    /// <summary>
    /// The portfolio is headed by the profile <c>TraineeScopeResolver</c> prefers: the active one, else the most recent
    /// (highest id). (T101, T113)
    /// </summary>
    /// <remarks>
    /// That is the profile <c>ExportPortfolio</c> authorised the export against and the one every activity's scope
    /// stamp was read from. Unordered, the load picked an arbitrary profile, so a trainee with two could get a PDF
    /// headed by the institution that did NOT grant the export. The rendered bytes do not show which profile was
    /// used, so these read the loaded data.
    /// </remarks>
    [Fact]
    public async Task ThePortfolio_IsHeadedByTheMostRecentProfile_WhenTheTraineeHasOnlyPastOnes()
    {
        await using var db = SeededDb();
        SeedProgramme(db);
        AddProfile(db, id: 10, HostInstitutionId, isActive: false);
        AddProfile(db, id: 20, OtherInstitutionId, isActive: false);

        var data = await LoadAsync(db);

        data.InstitutionName.Should().Be(OtherInstitutionName);
    }

    [Fact]
    public async Task ThePortfolio_IsHeadedByTheActiveProfile_OverAMoreRecentPastOne()
    {
        await using var db = SeededDb();
        SeedProgramme(db);
        AddProfile(db, id: 10, HostInstitutionId, isActive: true);
        AddProfile(db, id: 20, OtherInstitutionId, isActive: false);

        var data = await LoadAsync(db);

        data.InstitutionName.Should().Be(HostInstitutionName);
    }

    private const int HostInstitutionId = 1;
    private const int OtherInstitutionId = 2;
    private const string HostInstitutionName = "Host Academic Hospital";
    private const string OtherInstitutionName = "Other Academic Hospital";

    private static Task<PortfolioData> LoadAsync(ApplicationDbContext db)
        => new PortfolioPdfService(db, new ThrowingMsfAggregationService()).LoadPortfolioDataAsync(
            new PortfolioExportRequest("trainee-1", null, null, SubjectPrincipal("trainee-1")),
            CancellationToken.None);

    private static void SeedProgramme(ApplicationDbContext db)
    {
        db.Set<Institution>().Add(new Institution { Id = HostInstitutionId, Name = HostInstitutionName, ShortCode = "HOST" });
        db.Set<Institution>().Add(new Institution { Id = OtherInstitutionId, Name = OtherInstitutionName, ShortCode = "OTHR" });
        db.Set<Speciality>().Add(new Speciality { Id = 1, CollegeId = 1, Name = "Paediatrics" });
        db.Set<SubSpeciality>().Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "General Paediatrics" });
        db.Set<Curriculum>().Add(new Curriculum { Id = 1, SubSpecialityId = 1, Name = "CPSA Paediatrics", Version = "11.1" });
        db.SaveChanges();
    }

    private static void AddProfile(ApplicationDbContext db, int id, int institutionId, bool isActive)
    {
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = id,
            UserId = "trainee-1",
            InstitutionId = institutionId,
            CurriculumId = 1,
            ProgrammeStartDate = new DateOnly(2025, 1, 1),
            ExpectedCompletionDate = new DateOnly(2029, 1, 1),
            IsActive = isActive
        });
        db.SaveChanges();
    }

    private static MsfCampaign SeedReleasedCampaign(ApplicationDbContext db, int[]? coveredEpaIds = null)
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
            CoveredEpas = (coveredEpaIds ?? [1]).Select(epaId => new MsfCampaignEpa { EpaId = epaId }).ToList()
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
        return campaign;
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
        public MsfCampaignAggregateReportDto BuildReport(MsfCampaign campaign, IEnumerable<MsfRecordedEpa> recordedEpas)
            => throw new NotSupportedException();
    }

    /// <summary>
    /// T101: the PDF's contents now obey the caller's read scope. These tests are about rendering, so
    /// they export as the subject, which resolves to exactly that trainee's own activities.
    /// </summary>
    private static ClaimsPrincipal SubjectPrincipal(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
