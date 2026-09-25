using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Features.Reporting;
using Wombat.Domain.Activities;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Reporting;
using Wombat.Tests.Shared;
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
    /// T164: a released learner-feedback campaign prints in the feedback section as learner feedback, with how many
    /// teaching contexts its learners answered from; the load reads each response's invitation, which carries the context.
    /// The count and never the names (T164 review): the portfolio is the trainee's, and a context's name beside a handful
    /// of answers can say which learner wrote which. The summary page counts it as learner feedback, not as MSF.
    /// </summary>
    [Fact]
    public async Task Generate_SucceedsForATraineeWithAReleasedLearnerFeedbackCampaign_AndCountsItsTeachingContexts()
    {
        await using var db = SeededDb();
        SeedReleasedCampaign(db, MsfTemplateKind.LearnerFeedback);

        var service = new PortfolioPdfService(db, new MsfAggregationService());
        var request = new PortfolioExportRequest("trainee-1", null, null, SubjectPrincipal("trainee-1"));

        var data = await service.LoadPortfolioDataAsync(request, CancellationToken.None);
        var report = data.MsfReports.Should().ContainSingle().Subject;
        report.Kind.Should().Be(MsfTemplateKind.LearnerFeedback);
        report.TeachingContextCount.Should().Be(2);
        report.TeachingContextsResponded.Should().BeNull("the portfolio names no teaching context");

        var text = string.Join("\f", PdfTextLayer.Pages((await service.GenerateAsync(request, CancellationToken.None)).PdfBytes));
        text.Should().Contain("Teaching contexts that responded:")
            .And.Contain("Learner feedback reports:")
            .And.NotContain("MSF reports:")
            .And.NotContain("Student tutorial")
            .And.NotContain("Ward round");
    }

    /// <summary>
    /// T225: the feedback section names each respondent group by its label, as the report page does, never by the key it
    /// stores. A peer doctor's line read "PeerDoctor: 1 responses", and an allied health professional's "Ahp: …".
    /// </summary>
    [Fact]
    public async Task TheFeedbackSection_NamesEachRespondentGroupByItsLabel()
    {
        await using var db = SeededDb();
        SeedReleasedCampaign(db, categories: [MsfRespondentCategory.PeerDoctor, MsfRespondentCategory.Ahp]);

        var service = new PortfolioPdfService(db, new MsfAggregationService());
        var request = new PortfolioExportRequest("trainee-1", null, null, SubjectPrincipal("trainee-1"));

        var text = string.Join("\f", PdfTextLayer.Pages((await service.GenerateAsync(request, CancellationToken.None)).PdfBytes));
        text.Should().Contain("Peer doctor:")
            .And.Contain("Allied health professional:")
            .And.NotContain("PeerDoctor")
            .And.NotContain("Ahp:");
    }

    /// <summary>
    /// T164 review, after T186: the PDF reads a learner-feedback campaign's recorded EPAs from the rows of the type its
    /// release writes. A <c>learner_feedback_cpsa</c> row naming it records PAED-001; an <c>msf_cpsa</c> row naming it,
    /// which no learner-feedback release writes, records PAED-002 for nothing.
    /// </summary>
    [Fact]
    public async Task TheFeedbackSection_ReadsALearnerFeedbackCampaignsRecordedEpas_FromItsOwnTypesRows()
    {
        await using var db = SeededDb();
        db.Set<Epa>().Add(new Epa { Id = 2, SubSpecialityId = 1, Code = "PAED-002", Title = "Chronic care", IsActive = true });
        var campaign = SeedReleasedCampaign(db, MsfTemplateKind.LearnerFeedback, coveredEpaIds: [1, 2]);
        AddEvidenceRow(db, 31, "learner_feedback_cpsa", campaign, epaId: 1);
        AddEvidenceRow(db, 30, MsfEvidenceKinds.MsfActivityTypeKey, campaign, epaId: 2);
        await db.SaveChangesAsync();

        var data = await new PortfolioPdfService(db, new MsfAggregationService()).LoadPortfolioDataAsync(
            new PortfolioExportRequest("trainee-1", null, null, SubjectPrincipal("trainee-1")),
            CancellationToken.None);

        data.MsfReports.Should().ContainSingle().Which.CoveredEpas
            .Select(covered => (covered.Code, covered.Recorded))
            .Should().Equal(("PAED-001", true), ("PAED-002", false));
    }

    /// <summary>
    /// The data-subject access report says which questionnaire each campaign about the subject was, since multi-source
    /// feedback and learner feedback run on one aggregate (T164 review).
    /// </summary>
    [Fact]
    public async Task TheAccessReport_NamesEachCampaignsKind()
    {
        await using var db = SeededDb();
        SeedReleasedCampaign(db, MsfTemplateKind.LearnerFeedback);

        var export = await new Wombat.Infrastructure.DataRights.AccessReportBuilder(db, new PortfolioPdfService(db, new MsfAggregationService()))
            .BuildAsync("trainee-1", CancellationToken.None);

        using var zip = new System.IO.Compression.ZipArchive(new MemoryStream(export.ZipBytes), System.IO.Compression.ZipArchiveMode.Read);
        await using var json = zip.GetEntry("data-export.json")!.Open();
        using var document = await System.Text.Json.JsonDocument.ParseAsync(json);
        document.RootElement.GetProperty("msfCampaigns").EnumerateArray()
            .Select(campaign => campaign.GetProperty("kind").GetString())
            .Should().Equal("LearnerFeedback");
    }

    /// <summary>
    /// The access report's curriculum-progress entries say whether each last encounter date was stated, since the date
    /// alone cannot tell a stated one from the day a form was created (T219).
    /// </summary>
    [Fact]
    public async Task TheAccessReport_SaysWhetherEachLastEncounterDateWasStated()
    {
        await using var db = SeededDb();
        db.Set<CurriculumItemProgress>().AddRange(
            new CurriculumItemProgress
            {
                CurriculumItemId = 1, TraineeUserId = "trainee-1", AcademicYear = 2026, Semester = 1, CountsSoFar = 1,
                LastObservedOn = new DateOnly(2026, 3, 10), LastObservedOnDeclared = true
            },
            new CurriculumItemProgress
            {
                CurriculumItemId = 1, TraineeUserId = "trainee-1", AcademicYear = 2026, Semester = 2, CountsSoFar = 1,
                LastObservedOn = new DateOnly(2026, 8, 20), LastObservedOnDeclared = false
            });
        await db.SaveChangesAsync();

        var export = await new Wombat.Infrastructure.DataRights.AccessReportBuilder(db, new PortfolioPdfService(db, new MsfAggregationService()))
            .BuildAsync("trainee-1", CancellationToken.None);

        using var zip = new System.IO.Compression.ZipArchive(new MemoryStream(export.ZipBytes), System.IO.Compression.ZipArchiveMode.Read);
        await using var json = zip.GetEntry("data-export.json")!.Open();
        using var document = await System.Text.Json.JsonDocument.ParseAsync(json);
        document.RootElement.GetProperty("curriculumProgress").EnumerateArray()
            .Select(row => (row.GetProperty("lastObservedOn").GetString(), row.GetProperty("lastObservedOnDeclared").GetBoolean()))
            .OrderBy(row => row.Item1)
            .Should().Equal(("2026-03-10", true), ("2026-08-20", false));
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
            Key = MsfEvidenceKinds.MsfActivityTypeKey,
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

    /// <summary>
    /// A ratified committee decision in the portfolio says who took it: the members recorded as present, by name, the
    /// chair marked. (T165)
    /// </summary>
    [Fact]
    public async Task ARatifiedReview_SaysWhoWasPresent_ByName()
    {
        await using var db = SeededDb();
        SeedRatifiedReview(db);

        var data = await LoadAsync(db);

        var review = data.CommitteeReviews.Should().ContainSingle().Subject;
        CommitteeSectionComponent.PresentLine(review, data.CommitteeAttendeeNames)
            .Should().Be("Thandi Zulu (chair), Priya Naidoo, Anna Botha (external)");
    }

    [Fact]
    public async Task WhoWasPresent_IsPrinted()
    {
        // The rendered bytes change with the attendance, so the line is on the page, not only in the loaded data.
        await using var db = SeededDb();
        SeedRatifiedReview(db);
        var service = new PortfolioPdfService(db, new ThrowingMsfAggregationService());
        var request = new PortfolioExportRequest("trainee-1", null, null, SubjectPrincipal("trainee-1"));

        var withThree = await service.GenerateAsync(request, CancellationToken.None);

        db.Set<CommitteeDecisionAttendee>().RemoveRange(db.Set<CommitteeDecisionAttendee>().Where(attendee => attendee.UserId == "external-1"));
        await db.SaveChangesAsync();
        var withTwo = await service.GenerateAsync(request, CancellationToken.None);

        withTwo.ContentHash.Should().NotBe(withThree.ContentHash);
    }

    [Fact]
    public void APresentLine_ForAMemberWithNoNameOnRecord_ShowsTheId_AndForNobody_IsAbsent()
    {
        var review = new CommitteeReview { AcademicYear = 2026, Semester = 1 };
        CommitteeSectionComponent.PresentLine(review, new Dictionary<string, string>()).Should().BeNull("there is no decision");

        var decision = CommitteeDecision.Create(
            CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, "chair-9", DateTime.UtcNow, []);
        review.Decisions.Add(decision);
        CommitteeSectionComponent.PresentLine(review, new Dictionary<string, string>()).Should().BeNull("nobody was recorded");

        decision.Attendees.Add(new CommitteeDecisionAttendee { UserId = "chair-9", Role = DecisionPanelMemberRole.Chair });
        CommitteeSectionComponent.PresentLine(review, new Dictionary<string, string>()).Should().Be("chair-9 (chair)");
    }

    /// <summary>
    /// The portfolio prints the current decision, and so the sitting that took it: a decision an appeal remitted was taken
    /// by whoever sat for the appeal, not by the review's first sitting. Before T165 attendance was the review's, so the
    /// PDF credited the replacement to the first sitting. (T165)
    /// </summary>
    [Fact]
    public async Task ARemittedReview_PrintsTheReplacementsOwnSitting()
    {
        await using var db = SeededDb();
        var review = SeedRatifiedReview(db);
        var appealSitting = new DateTime(2029, 12, 9, 9, 0, 0, DateTimeKind.Utc);
        review.LodgeAppeal("The conditions are disproportionate.", "trainee-1", appealSitting);
        review.ResolveAppeal(
            CommitteeAppealOutcome.Remitted, "external-1", appealSitting, CommitteeDecisionCategory.SatisfactoryProgress,
            "Conditions lifted on appeal.", null,
            review.Panel.Members.Where(member => member.UserId != "member-1").ToArray());
        await db.SaveChangesAsync();

        var data = await LoadAsync(db);

        CommitteeSectionComponent.PresentLine(data.CommitteeReviews.Single(), data.CommitteeAttendeeNames)
            .Should().Be("Thandi Zulu (chair), Anna Botha (external)");
    }

    /// <summary>
    /// An entrustment-only review's decision records no progression category, and the portfolio still exports it, saying
    /// what the review decided instead of a blank. (T131 slice 5)
    /// </summary>
    [Fact]
    public async Task AnEntrustmentOnlyReview_IsExported_AndItsDecisionReadsAsEntrustmentOnly()
    {
        await using var db = SeededDb();
        SeedRatifiedReview(db, CommitteeReviewType.EntrustmentOnly);
        var service = new PortfolioPdfService(db, new ThrowingMsfAggregationService());
        var request = new PortfolioExportRequest("trainee-1", null, null, SubjectPrincipal("trainee-1"));

        var data = await LoadAsync(db);
        var exported = await service.GenerateAsync(request, CancellationToken.None);

        var decision = data.CommitteeReviews.Should().ContainSingle().Subject.GetCurrentDecision()!;
        decision.Category.Should().BeNull();
        CommitteeSectionComponent.DecisionLine(decision).Should().Be("Entrustment decisions only");
        exported.PdfBytes.Should().NotBeEmpty();
    }

    private static CommitteeReview SeedRatifiedReview(
        ApplicationDbContext db, CommitteeReviewType type = CommitteeReviewType.AnnualProgression)
    {
        db.Set<WombatIdentityUser>().AddRange(
            new WombatIdentityUser { Id = "chair-1", FirstName = "Thandi", LastName = "Zulu" },
            new WombatIdentityUser { Id = "member-1", FirstName = "Priya", LastName = "Naidoo" },
            new WombatIdentityUser { Id = "external-1", FirstName = "Anna", LastName = "Botha" });

        var members = new[]
        {
            new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair },
            new DecisionPanelMember { UserId = "member-1", Role = DecisionPanelMemberRole.Member },
            new DecisionPanelMember { UserId = "external-1", Role = DecisionPanelMemberRole.External }
        };
        var panel = new DecisionPanel
        {
            Name = "Paediatrics CCC",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = HostInstitutionId,
            CreatedOn = new DateTime(2029, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Members = members
        };

        var sitting = new DateTime(2029, 11, 18, 9, 0, 0, DateTimeKind.Utc);
        var review = new CommitteeReview
        {
            AcademicYear = 2029,
            Semester = 2,
            Panel = panel,
            TraineeUserId = "trainee-1",
            ReviewPeriodFrom = new DateOnly(2029, 1, 1),
            ReviewPeriodTo = new DateOnly(2029, 10, 31),
            ScheduledOn = new DateOnly(2029, 11, 18),
            ReviewType = type
        };
        review.Start([], "chair-1", sitting);
        review.RecordDecision(
            type == CommitteeReviewType.EntrustmentOnly ? null : CommitteeDecisionCategory.SatisfactoryProgress,
            "On track.", null, "chair-1", sitting, members);
        review.Ratify("chair-1", sitting);

        db.Set<DecisionPanel>().Add(panel);
        db.Set<CommitteeReview>().Add(review);
        db.SaveChanges();
        return review;
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

    private static MsfCampaign SeedReleasedCampaign(
        ApplicationDbContext db,
        MsfTemplateKind kind = MsfTemplateKind.Msf,
        int[]? coveredEpaIds = null,
        MsfRespondentCategory[]? categories = null)
    {
        var learnerFeedback = kind == MsfTemplateKind.LearnerFeedback;
        var template = new MsfTemplate
        {
            Name = learnerFeedback ? "Learner feedback" : "Annual MSF",
            Kind = kind,
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
            MinimumRespondentCategories = learnerFeedback ? 1 : 2,
            State = MsfCampaignState.Released,
            ReleasedOn = new DateTime(2029, 7, 1, 0, 0, 0, DateTimeKind.Utc),
            ReviewedByUserId = "coord-1",
            Template = template,
            CoveredEpas = (coveredEpaIds ?? [1]).Select(epaId => new MsfCampaignEpa { EpaId = epaId }).ToList()
        };

        var respondents = learnerFeedback
            ? new[] { (MsfRespondentCategory.Learner, (string?)"Ward round"), (MsfRespondentCategory.Learner, "Student tutorial") }
            : (categories ?? [MsfRespondentCategory.Consultant, MsfRespondentCategory.Nurse])
                .Select(category => (category, (string?)null))
                .ToArray();

        foreach (var (category, teachingContext) in respondents)
        {
            var invitation = new MsfInvitation
            {
                RespondentCategory = category,
                TeachingContext = teachingContext,
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

    /// <summary>
    /// One evidence row naming <paramref name="campaign" />, as a release writes it, of the type keyed
    /// <paramref name="key" /> read from its shipped seed workflow (both write <c>recorded</c>, terminal).
    /// </summary>
    private static void AddEvidenceRow(ApplicationDbContext db, int typeId, string key, MsfCampaign campaign, int epaId)
    {
        if (db.Set<ActivityType>().Local.All(type => type.Id != typeId))
        {
            db.Set<ActivityType>().Add(new ActivityType
            {
                Id = typeId,
                Key = key,
                Name = key,
                Version = 1,
                WorkflowJson = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", key, "workflow.json")),
                OwnerUserId = "seed-system",
                CreatedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }

        db.Set<Activity>().Add(new Activity
        {
            ActivityTypeId = typeId,
            SchemaVersion = 1,
            SubjectUserId = "trainee-1",
            CreatedByUserId = "coord-1",
            CurrentState = "recorded",
            DataJson = "{ \"epa_id\": " + epaId + ", \"" + MsfCampaignCoverage.CampaignIdField + "\": " + campaign.Id + ", \"respondent_count\": 2 }",
            EpaId = epaId,
            CreatedOn = campaign.ReleasedOn!.Value,
            UpdatedOn = campaign.ReleasedOn!.Value,
            ObservedOn = campaign.ClosesOn,
            ObservedOnSource = ObservationDateSource.Declared
        });
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
            evidenceLinks: StarEvidence.One()));
        db.SaveChanges();
        return db;
    }

    private sealed class ThrowingMsfAggregationService : IMsfAggregationService
    {
        // Never reached: the seeded data has no released MSF campaigns.
        public MsfCampaignAggregateReportDto BuildReport(MsfCampaign campaign, IEnumerable<MsfRecordedEpa> recordedEpas, bool nameTeachingContexts = false)
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
