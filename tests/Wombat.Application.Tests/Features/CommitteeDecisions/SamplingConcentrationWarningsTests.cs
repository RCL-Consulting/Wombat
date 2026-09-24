using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.Activities;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.CommitteeDecisions;

public sealed class SamplingConcentrationWarningsTests
{
    /// <summary>The institution running the review, and the stamp on its evidence rows.</summary>
    private const int HostInstitution = 1;

    /// <summary>Where an External panel member — or a trainee's pre-transfer evidence — comes from.</summary>
    private const int OtherInstitution = 2;

    [Fact]
    public async Task NoRatedEvidence_ReturnsEmptyReport()
    {
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var report = await handler.Handle(new GetSamplingConcentrationWarningsQuery(reviewId, AdministratorPrincipal()), CancellationToken.None);

        report.AnyWarning.Should().BeFalse();
        report.PerEpa.Should().BeEmpty();
        report.TotalRatedActivities.Should().Be(0);
    }

    [Fact]
    public async Task OneAssessorOverHalf_EmitsWarning()
    {
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");
        var cbd = await SeedActivityTypeAsync(dbContext, "cbd");

        // 3 ratings on EPA 7: assessor-A twice, assessor-B once -> one assessor at 2/3 > 50%
        AddActivity(dbContext, miniCex, subject: "trainee-1", assessor: "assessor-a", epaId: 7, createdOn: new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, miniCex, subject: "trainee-1", assessor: "assessor-a", epaId: 7, createdOn: new DateTime(2026, 2, 5, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, cbd, subject: "trainee-1", assessor: "assessor-b", epaId: 7, createdOn: new DateTime(2026, 2, 10, 10, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var report = await handler.Handle(new GetSamplingConcentrationWarningsQuery(reviewId, AdministratorPrincipal()), CancellationToken.None);

        report.AnyWarning.Should().BeTrue();
        var warning = report.PerEpa.Should().ContainSingle(entry => entry.EpaId == 7).Subject;
        warning.OneAssessorOverHalf.Should().BeTrue();
        warning.DominantAssessorUserId.Should().Be("assessor-a");
        warning.DominantAssessorCount.Should().Be(2);
        warning.RatingCount.Should().Be(3);
        warning.DistinctAssessorCount.Should().Be(2);
        warning.DistinctSourceCount.Should().Be(2);
    }

    [Fact]
    public async Task SingleSource_EmitsWarning()
    {
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");
        var dops = await SeedActivityTypeAsync(dbContext, "dops");

        // Both activity types map to DirectObservation -> only one source covered
        AddActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, dops, "trainee-1", "assessor-b", 7, new DateTime(2026, 2, 5, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, miniCex, "trainee-1", "assessor-c", 7, new DateTime(2026, 2, 10, 10, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var report = await handler.Handle(new GetSamplingConcentrationWarningsQuery(reviewId, AdministratorPrincipal()), CancellationToken.None);

        var warning = report.PerEpa.Should().ContainSingle(entry => entry.EpaId == 7).Subject;
        warning.SingleSource.Should().BeTrue();
        warning.DistinctSourceCount.Should().Be(1);
        warning.FewerThanThreeAssessors.Should().BeFalse();
        warning.OneAssessorOverHalf.Should().BeFalse();
    }

    [Fact]
    public async Task FewerThanThreeAssessors_EmitsWarning()
    {
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");
        var cbd = await SeedActivityTypeAsync(dbContext, "cbd");

        // Two assessors across two sources -> triggers only fewer-than-three
        AddActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, cbd, "trainee-1", "assessor-b", 7, new DateTime(2026, 2, 5, 10, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var report = await handler.Handle(new GetSamplingConcentrationWarningsQuery(reviewId, AdministratorPrincipal()), CancellationToken.None);

        var warning = report.PerEpa.Should().ContainSingle(entry => entry.EpaId == 7).Subject;
        warning.FewerThanThreeAssessors.Should().BeTrue();
        warning.OneAssessorOverHalf.Should().BeFalse();
        warning.SingleSource.Should().BeFalse();
        warning.DistinctAssessorCount.Should().Be(2);
    }

    [Fact]
    public async Task WellSampledEvidence_NoWarnings()
    {
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");
        var cbd = await SeedActivityTypeAsync(dbContext, "cbd");
        var acat = await SeedActivityTypeAsync(dbContext, "acat");

        // 4 ratings, 4 distinct assessors, 2 sources -> no assessor > 50%, multi-source, >=3 assessors
        AddActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, cbd, "trainee-1", "assessor-b", 7, new DateTime(2026, 2, 5, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, acat, "trainee-1", "assessor-c", 7, new DateTime(2026, 2, 10, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, miniCex, "trainee-1", "assessor-d", 7, new DateTime(2026, 2, 15, 10, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var report = await handler.Handle(new GetSamplingConcentrationWarningsQuery(reviewId, AdministratorPrincipal()), CancellationToken.None);

        report.AnyWarning.Should().BeFalse();
        report.PerEpa.Should().BeEmpty();
        report.TotalRatedActivities.Should().Be(4);
        report.DistinctAssessorCount.Should().Be(4);
    }

    [Fact]
    public async Task ExcludesActivitiesOutsideReviewPeriod()
    {
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        // Activity outside window should be ignored
        AddActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, new DateTime(2025, 12, 1, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, miniCex, "trainee-1", "assessor-b", 7, new DateTime(2026, 4, 15, 10, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var report = await handler.Handle(new GetSamplingConcentrationWarningsQuery(reviewId, AdministratorPrincipal()), CancellationToken.None);

        report.TotalRatedActivities.Should().Be(0);
        report.AnyWarning.Should().BeFalse();
    }

    [Fact]
    public async Task IgnoresUnratedActivityTypes()
    {
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);
        var reflectiveNote = await SeedUnratedActivityTypeAsync(dbContext, "reflective_note");

        AddActivity(dbContext, reflectiveNote, "trainee-1", "assessor-a", 7, new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var report = await handler.Handle(new GetSamplingConcentrationWarningsQuery(reviewId, AdministratorPrincipal()), CancellationToken.None);

        report.TotalRatedActivities.Should().Be(0);
        report.AnyWarning.Should().BeFalse();
    }

    [Fact]
    public async Task RefusesACallerWithNoClaimOnTheReview()
    {
        // The report is computed from the trainee's activity rows, so naming a review id used to be
        // enough to pull another institution's rating counts and assessor ids out of it. It is now a
        // refusal rather than an empty report: a caller with no business in the review should not be
        // able to tell an empty sample from one they were never shown. (T101)
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddActivity(dbContext, miniCex, subject: "trainee-1", assessor: "assessor-a", epaId: 7, createdOn: new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var act = () => handler.Handle(
            new GetSamplingConcentrationWarningsQuery(reviewId, Principal("stranger-1")), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task RefusesACoordinatorFromAnotherInstitution()
    {
        // A Coordinator used to be waived past the panel check by role alone, which made every
        // institution's committee evidence readable by any institution's administrative staff.
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var act = () => handler.Handle(
            new GetSamplingConcentrationWarningsQuery(
                reviewId, Principal("coord-2", WombatRoles.Coordinator, OtherInstitution)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ExternalPanelMember_IsToldTheSampleIsIncompleteRatherThanClean()
    {
        // An External panel member sits on the panel precisely because they come from elsewhere, so
        // the read filter withholds every row stamped to the host institution. Without the withheld
        // count the panel's independent voice would be handed a silent report and read it as a clean
        // sample, on a progression decision.
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc), HostInstitution);
        AddActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, new DateTime(2026, 2, 5, 10, 0, 0, DateTimeKind.Utc), HostInstitution);
        AddActivity(dbContext, miniCex, "trainee-1", "assessor-b", 7, new DateTime(2026, 2, 10, 10, 0, 0, DateTimeKind.Utc), HostInstitution);
        await dbContext.SaveChangesAsync();

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var report = await handler.Handle(
            new GetSamplingConcentrationWarningsQuery(
                reviewId, Principal("external-1", WombatRoles.CommitteeMember, OtherInstitution)),
            CancellationToken.None);

        report.TotalRatedActivities.Should().Be(0);
        report.AnyWarning.Should().BeFalse();
        report.EvidenceComplete.Should().BeFalse();
        report.WithheldRatedActivities.Should().Be(3);
    }

    [Fact]
    public async Task CommitteeMemberInTheHostInstitution_SeesTheWholeSample()
    {
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");
        var cbd = await SeedActivityTypeAsync(dbContext, "cbd");

        AddActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc), HostInstitution);
        AddActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, new DateTime(2026, 2, 5, 10, 0, 0, DateTimeKind.Utc), HostInstitution);
        AddActivity(dbContext, cbd, "trainee-1", "assessor-b", 7, new DateTime(2026, 2, 10, 10, 0, 0, DateTimeKind.Utc), HostInstitution);
        await dbContext.SaveChangesAsync();

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var report = await handler.Handle(
            new GetSamplingConcentrationWarningsQuery(
                reviewId, Principal("member-1", WombatRoles.CommitteeMember, HostInstitution)),
            CancellationToken.None);

        report.EvidenceComplete.Should().BeTrue();
        report.WithheldRatedActivities.Should().Be(0);
        report.TotalRatedActivities.Should().Be(3);
        report.PerEpa.Should().ContainSingle(entry => entry.EpaId == 7)
            .Which.OneAssessorOverHalf.Should().BeTrue();
    }

    [Fact]
    public async Task PartiallyWithheldEvidence_FlagsTheWarningItInventedAsIncomplete()
    {
        // The transferred trainee: rows recorded before the transfer keep the old institution's
        // stamp and drop out of the host's view. The whole sample here is three assessors across two
        // sources — no warning — but the two rows this caller may read are one assessor, which the
        // arithmetic reports as a concentration. The count is honest, the warning is an artefact of
        // the filter, and only EvidenceComplete tells the panel which of the two it is looking at.
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");
        var cbd = await SeedActivityTypeAsync(dbContext, "cbd");

        AddActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc), HostInstitution);
        AddActivity(dbContext, cbd, "trainee-1", "assessor-a", 7, new DateTime(2026, 2, 2, 10, 0, 0, DateTimeKind.Utc), HostInstitution);
        AddActivity(dbContext, miniCex, "trainee-1", "assessor-b", 7, new DateTime(2026, 2, 5, 10, 0, 0, DateTimeKind.Utc), OtherInstitution);
        AddActivity(dbContext, cbd, "trainee-1", "assessor-c", 7, new DateTime(2026, 2, 10, 10, 0, 0, DateTimeKind.Utc), OtherInstitution);
        await dbContext.SaveChangesAsync();

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var report = await handler.Handle(
            new GetSamplingConcentrationWarningsQuery(
                reviewId, Principal("member-1", WombatRoles.CommitteeMember, HostInstitution)),
            CancellationToken.None);

        report.TotalRatedActivities.Should().Be(2);
        report.WithheldRatedActivities.Should().Be(2);
        report.EvidenceComplete.Should().BeFalse();
        report.AnyWarning.Should().BeTrue();
        report.PerEpa.Should().ContainSingle(entry => entry.EpaId == 7)
            .Which.FewerThanThreeAssessors.Should().BeTrue();
    }

    /// <summary>
    /// These tests are about the concentration arithmetic, so they read as a global Administrator —
    /// the one caller neither gate narrows. Who may ask for the report at all is exercised by
    /// RefusesACallerWithNoClaimOnTheReview and RefusesACoordinatorFromAnotherInstitution; what the
    /// arithmetic is allowed to run on, by the panel-member tests above.
    /// </summary>
    private static ClaimsPrincipal AdministratorPrincipal()
        => Principal("admin-1", WombatRoles.Administrator);

    private static ClaimsPrincipal Principal(string userId, string? role = null, int? institutionId = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        if (role is not null)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        if (institutionId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, institutionId.Value.ToString()));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task<int> SeedReviewAsync(ApplicationDbContext dbContext)
    {
        var epa = new Epa { Id = 7, SubSpecialityId = 1, Code = "EPA-07", Title = "Emergency triage", IsActive = true };
        dbContext.Epas.Add(epa);

        // The panel is real here because the report now climbs the review ladder before it counts
        // anything, and that ladder asks who sits on the panel.
        dbContext.DecisionPanels.Add(new DecisionPanel
        {
            Id = 1,
            Name = "Paediatrics ARCP",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = HostInstitution,
            CreatedOn = DateTime.UtcNow,
            Members =
            [
                new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair },
                new DecisionPanelMember { UserId = "member-1", Role = DecisionPanelMemberRole.Member },
                new DecisionPanelMember { UserId = "external-1", Role = DecisionPanelMemberRole.External }
            ]
        });

        var review = new CommitteeReview
        {
            Id = 42,
            PanelId = 1,
            TraineeUserId = "trainee-1",
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 3, 31),
            ScheduledOn = new DateOnly(2026, 4, 1)
        };
        dbContext.CommitteeReviews.Add(review);
        await dbContext.SaveChangesAsync();
        return review.Id;
    }

    /// <summary>
    /// T134. The rated-type gate matched EXACT keys over mini_cex/dops/cbd/acat, so the whole seeded
    /// v11.1 tool set fell outside it and a CPSA trainee's report told the panel there was no rated
    /// evidence to sample. Note the fixture is UNTOUCHED: these types carry no SchemaJson, which is
    /// what keeps this test evidence rather than a restatement.
    /// </summary>
    [Fact]
    public async Task CpsaToolKeys_AreCountedAsRatedEvidence()
    {
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);
        var miniCexCpsa = await SeedActivityTypeAsync(dbContext, "mini_cex_cpsa");
        var cbdCpsa = await SeedActivityTypeAsync(dbContext, "cbd_cpsa");

        AddActivity(dbContext, miniCexCpsa, subject: "trainee-1", assessor: "assessor-a", epaId: 7, createdOn: new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, miniCexCpsa, subject: "trainee-1", assessor: "assessor-a", epaId: 7, createdOn: new DateTime(2026, 2, 5, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, cbdCpsa, subject: "trainee-1", assessor: "assessor-b", epaId: 7, createdOn: new DateTime(2026, 2, 10, 10, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var report = await handler.Handle(new GetSamplingConcentrationWarningsQuery(reviewId, AdministratorPrincipal()), CancellationToken.None);

        report.TotalRatedActivities.Should().Be(3, "every CPSA tool is rated evidence");
        report.EvidenceComplete.Should().BeTrue();
        var warning = report.PerEpa.Should().ContainSingle(entry => entry.EpaId == 7).Subject;
        warning.DistinctSourceCount.Should().Be(2, "mini_cex_cpsa is direct observation, cbd_cpsa is conversation");
        warning.DistinctAssessorCount.Should().Be(2);
    }

    /// <summary>
    /// Two ratings from the SAME institution-built rated tool are ONE source, not none. A design that
    /// drops an unknown family from the numerator would under-report TotalRatedActivities while
    /// EvidenceComplete still read true — the same lie T134 removes, moved somewhere harder to see.
    /// </summary>
    [Fact]
    public async Task TwoRatingsFromOneUnfamiliarRatedToolAreOneSource()
    {
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);
        var custom = await SeedActivityTypeAsync(dbContext, "ward_round_review");

        AddActivity(dbContext, custom, subject: "trainee-1", assessor: "assessor-a", epaId: 7, createdOn: new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, custom, subject: "trainee-1", assessor: "assessor-b", epaId: 7, createdOn: new DateTime(2026, 2, 5, 10, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var report = await handler.Handle(new GetSamplingConcentrationWarningsQuery(reviewId, AdministratorPrincipal()), CancellationToken.None);

        report.TotalRatedActivities.Should().Be(2, "a declared rating counts even under an unfamiliar key");
        report.EvidenceComplete.Should().BeTrue();
        var warning = report.PerEpa.Should().ContainSingle(entry => entry.EpaId == 7).Subject;
        warning.DistinctSourceCount.Should().Be(1);
        warning.SingleSource.Should().BeTrue();
    }

    [Fact]
    public async Task TwoDifferentUnfamiliarRatedToolsAreTwoSources()
    {
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);
        var first = await SeedActivityTypeAsync(dbContext, "ward_round_review");
        var second = await SeedActivityTypeAsync(dbContext, "handover_review");

        AddActivity(dbContext, first, subject: "trainee-1", assessor: "assessor-a", epaId: 7, createdOn: new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, second, subject: "trainee-1", assessor: "assessor-b", epaId: 7, createdOn: new DateTime(2026, 2, 5, 10, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var report = await handler.Handle(new GetSamplingConcentrationWarningsQuery(reviewId, AdministratorPrincipal()), CancellationToken.None);

        report.PerEpa.Should().ContainSingle(entry => entry.EpaId == 7)
            .Subject.DistinctSourceCount.Should().Be(2);
    }

    /// <summary>
    /// Both CPSA observation tools are the same category, so they are one source — which is what makes
    /// the SingleSource warning meaningful for a v11.1 trainee rather than accidentally absent.
    /// </summary>
    [Fact]
    public async Task TwoCpsaToolsOfTheSameCategoryAreOneSource()
    {
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);
        var miniCexCpsa = await SeedActivityTypeAsync(dbContext, "mini_cex_cpsa");
        var dopsCpsa = await SeedActivityTypeAsync(dbContext, "dops_cpsa");

        AddActivity(dbContext, miniCexCpsa, subject: "trainee-1", assessor: "assessor-a", epaId: 7, createdOn: new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, dopsCpsa, subject: "trainee-1", assessor: "assessor-b", epaId: 7, createdOn: new DateTime(2026, 2, 5, 10, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var report = await handler.Handle(new GetSamplingConcentrationWarningsQuery(reviewId, AdministratorPrincipal()), CancellationToken.None);

        var warning = report.PerEpa.Should().ContainSingle(entry => entry.EpaId == 7).Subject;
        warning.DistinctSourceCount.Should().Be(1, "both are direct observation");
        warning.SingleSource.Should().BeTrue();
    }

    /// <summary>
    /// T144: a builder-made type under a key no family matches, declaring DOPS as its instrument, is the
    /// same source as the seeded Mini-CEX, because both are direct observation. Read by its key, it was a
    /// second source and hid the SingleSource warning.
    /// </summary>
    [Fact]
    public async Task ABuilderTypeIsCountedAsTheCategoryOfTheInstrumentItDeclares()
    {
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);
        var miniCexCpsa = await SeedActivityTypeAsync(dbContext, "mini_cex_cpsa", wbaToolKey: "mini_cex");
        var builderMade = await SeedActivityTypeAsync(dbContext, "ward_round_review", wbaToolKey: "dops");

        AddActivity(dbContext, miniCexCpsa, subject: "trainee-1", assessor: "assessor-a", epaId: 7, createdOn: new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, builderMade, subject: "trainee-1", assessor: "assessor-b", epaId: 7, createdOn: new DateTime(2026, 2, 5, 10, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var report = await handler.Handle(new GetSamplingConcentrationWarningsQuery(reviewId, AdministratorPrincipal()), CancellationToken.None);

        var warning = report.PerEpa.Should().ContainSingle(entry => entry.EpaId == 7).Subject;
        warning.DistinctSourceCount.Should().Be(1, "Mini-CEX and DOPS are both direct observation");
        warning.SingleSource.Should().BeTrue();
    }

    /// <summary>
    /// A rated type that declares no instrument is counted because its schema declares a rating, and its
    /// category is still read from its key, prefix included: an unkeyed <c>mini_cex_paed</c> is the same
    /// source as a DOPS that declares its instrument. (T144)
    /// </summary>
    [Fact]
    public async Task AnUnkeyedTypeUnderAFamilyPrefixIsCountedAsThatFamilysCategory()
    {
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);
        var paed = await SeedActivityTypeAsync(dbContext, "mini_cex_paed");
        var dopsCpsa = await SeedActivityTypeAsync(dbContext, "dops_cpsa", wbaToolKey: "dops");

        AddActivity(dbContext, paed, subject: "trainee-1", assessor: "assessor-a", epaId: 7, createdOn: new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, dopsCpsa, subject: "trainee-1", assessor: "assessor-b", epaId: 7, createdOn: new DateTime(2026, 2, 5, 10, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var report = await handler.Handle(new GetSamplingConcentrationWarningsQuery(reviewId, AdministratorPrincipal()), CancellationToken.None);

        report.TotalRatedActivities.Should().Be(2, "both schemas declare a rating");
        var warning = report.PerEpa.Should().ContainSingle(entry => entry.EpaId == 7).Subject;
        warning.DistinctSourceCount.Should().Be(1,
            "the unkeyed mini_cex_paed reads as Direct observation by its key, like the DOPS by its instrument");
    }

    /// <summary>
    /// A type that rates nothing — a reflection, a logbook entry. Several seeds are like this, and they
    /// must stay out of a committee's evidence arithmetic entirely.
    /// </summary>
    private static async Task<ActivityType> SeedUnratedActivityTypeAsync(ApplicationDbContext dbContext, string key)
    {
        var activityType = new ActivityType
        {
            Key = key,
            Name = key,
            Version = 1,
            IsActive = true,
            OwnerUserId = "admin-1",
            CreatedOn = DateTime.UtcNow,
            SchemaJson = UnratedSchemaJson
        };
        dbContext.ActivityTypes.Add(activityType);
        await dbContext.SaveChangesAsync();
        return activityType;
    }

    private const string UnratedSchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "reflection",
              "title": "Reflection",
              "fields": [
                { "key": "what_i_learned", "type": "longtext", "label": "What I learned" }
              ]
            }
          ]
        }
        """;

    private const string RatedSchemaJson = """
        {
          "version": 1,
          "rated_level_field": "overall_level",
          "sections": [
            {
              "key": "assessment",
              "title": "Assessment",
              "fields": [
                { "key": "overall_level", "type": "scale", "label": "Overall", "options": ["1", "2"], "scale_key": "O-R Scale" }
              ]
            }
          ]
        }
        """;
    private static async Task<ActivityType> SeedActivityTypeAsync(
        ApplicationDbContext dbContext,
        string key,
        string? wbaToolKey = null)
    {
        var activityType = new ActivityType
        {
            Key = key,
            Name = key,
            Version = 1,
            IsActive = true,
            OwnerUserId = "admin-1",
            CreatedOn = DateTime.UtcNow,
            SchemaJson = RatedSchemaJson,
            WbaToolKey = wbaToolKey
        };
        dbContext.ActivityTypes.Add(activityType);
        await dbContext.SaveChangesAsync();
        return activityType;
    }

    private static void AddActivity(
        ApplicationDbContext dbContext,
        ActivityType activityType,
        string subject,
        string assessor,
        int epaId,
        DateTime createdOn,
        int? institutionId = null)
    {
        var dataJson = $"{{\"epa_id\": {epaId}, \"assessor_user_id\": \"{assessor}\"}}";
        dbContext.Activities.Add(new Activity
        {
            ActivityTypeId = activityType.Id,
            ActivityType = activityType,
            SchemaVersion = activityType.Version,
            InstitutionId = institutionId,
            SubjectUserId = subject,
            CreatedByUserId = assessor,
            CurrentState = "completed",
            DataJson = dataJson,
            CreatedOn = createdOn,
            // T119: production stamps this in ActivityService; a fixture that builds the
            // entity directly must set it, or it defaults to 0001-01-01.
            ObservedOn = DateOnly.FromDateTime(createdOn),
            UpdatedOn = createdOn
        });
    }
}
