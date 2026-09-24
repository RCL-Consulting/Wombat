using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// Learner feedback, EPA 15's "structured feedback from learners", run as an MSF template of its own kind (T164, D35):
/// only learners answer it, it may cover only an EPA whose College list names it, and its release records
/// <c>learner_feedback_cpsa</c> evidence, with the number of teaching contexts that answered, instead of MSF.
/// </summary>
/// <remarks>
/// Both evidence types are loaded from the REAL seed folders, as <see cref="MsfEvidenceFanOutTests" /> loads
/// <c>msf_cpsa</c>: what must hold is that the schemas the product ships accept what the release writes.
/// </remarks>
public sealed class LearnerFeedbackCampaignTests
{
    private const int CurriculumId = 3000;
    private const int InstitutionId = 10;
    private const int SpecialityId = 1;

    /// <summary>On v11.1 only EPA 15's list names learner feedback; EPA 1's names MSF and not learner feedback.</summary>
    private const int Paed001 = 5001;
    private const int Paed015 = 5015;

    private const int MsfTemplateId = 70;
    private const int LearnerFeedbackTemplateId = 71;

    private const string TraineeUserId = "trainee-1";
    private const string CoordinatorUserId = "coord-1";

    // ─── Release ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ReleasingALearnerFeedbackCampaign_WritesLearnerFeedbackRecordsUnderPaed015_AndCreditsNothing()
    {
        await using var db = CreateDb();
        Seed(db);
        var campaign = AddUnderReviewCampaign(db, LearnerFeedbackTemplateId, [Paed015],
            (MsfRespondentCategory.Learner, "Ward round"),
            (MsfRespondentCategory.Learner, "ward  round"),
            (MsfRespondentCategory.Learner, "Student tutorial"));

        await ReleaseAsync(db, campaign.Id, entrustmentLevel: null, narrative: "Clear and well paced.");

        var activity = await db.Activities.Include(entity => entity.ActivityType).Include(entity => entity.Transitions).SingleAsync();
        activity.ActivityType.Key.Should().Be("learner_feedback_cpsa");
        activity.EpaId.Should().Be(Paed015);
        activity.CurrentState.Should().Be("recorded");
        activity.ObservedOn.Should().Be(DateOnly.FromDateTime(campaign.ClosedOn!.Value));

        using (var data = JsonDocument.Parse(activity.DataJson))
        {
            data.RootElement.GetProperty("epa_id").GetInt32().Should().Be(Paed015);
            data.RootElement.GetProperty("respondent_count").GetInt32().Should().Be(3);
            data.RootElement.GetProperty("teaching_context_count").GetInt32().Should().Be(2,
                "\"Ward round\" and \"ward  round\" are one context; the tutorial is the second");
            data.RootElement.TryGetProperty("overall_level", out _).Should().BeFalse("learner feedback states no level");
            data.RootElement.GetProperty("summary").GetString().Should().Be("Clear and well paced.");
        }

        // D8 for MSF, and the same here: evidence, not one of Annexure A's encounters.
        db.CurriculumItemProgresses.Should().BeEmpty();
        activity.Transitions.Single(transition => transition.TransitionKey == "record").CreditedItemCount.Should().BeNull();

        var released = await db.MsfCampaigns.Include(entity => entity.CoveredEpas).SingleAsync();
        released.State.Should().Be(MsfCampaignState.Released);
        released.ReviewerEntrustmentLevel.Should().BeNull();
        released.CoveredEpas.Should().OnlyContain(covered => covered.RecordedOn != null);
    }

    [Fact]
    public async Task ReleasingAnMsfCampaign_StillWritesMsfRecords_WithNoTeachingContextCount()
    {
        await using var db = CreateDb();
        Seed(db);
        var campaign = AddUnderReviewCampaign(db, MsfTemplateId, [Paed001, Paed015],
            (MsfRespondentCategory.Consultant, null),
            (MsfRespondentCategory.Consultant, null),
            (MsfRespondentCategory.Nurse, null),
            (MsfRespondentCategory.Nurse, null));

        await ReleaseAsync(db, campaign.Id, entrustmentLevel: 4, narrative: null);

        var activities = await db.Activities.Include(entity => entity.ActivityType).ToListAsync();
        activities.Should().HaveCount(2);
        activities.Should().OnlyContain(activity => activity.ActivityType.Key == "msf_cpsa");
        activities.Select(activity => activity.EpaId).Should().BeEquivalentTo(new int?[] { Paed001, Paed015 },
            "an MSF release covers every declared EPA on the curriculum, whatever its list says (T121)");
        activities.Should().OnlyContain(activity => !activity.DataJson.Contains("teaching_context_count"));
    }

    /// <summary>
    /// The release re-applies the create command's predicate (<c>MsfCampaignRules.CoverableEpaIdsAsync</c>): a declared
    /// EPA whose list does not name learner feedback is dropped, as an EPA that left the curriculum is, and the rest are
    /// recorded.
    /// </summary>
    [Fact]
    public async Task ALearnerFeedbackRelease_DropsADeclaredEpaWhoseListDoesNotNameLearnerFeedback()
    {
        await using var db = CreateDb();
        Seed(db);
        var campaign = AddUnderReviewCampaign(db, LearnerFeedbackTemplateId, [Paed001, Paed015],
            (MsfRespondentCategory.Learner, "Ward round"),
            (MsfRespondentCategory.Learner, "Ward round"),
            (MsfRespondentCategory.Learner, "Ward round"));

        await ReleaseAsync(db, campaign.Id, entrustmentLevel: null, narrative: null);

        (await db.Activities.Select(activity => activity.EpaId).ToListAsync()).Should().Equal(Paed015);
        var released = await db.MsfCampaigns.Include(entity => entity.CoveredEpas).SingleAsync();
        released.CoveredEpas.Single(covered => covered.EpaId == Paed001).RecordedOn.Should().BeNull();
        released.CoveredEpas.Single(covered => covered.EpaId == Paed015).RecordedOn.Should().NotBeNull();
    }

    /// <summary>
    /// A level on a learner-feedback release is refused before anything changes. The audit pipeline saves the request's
    /// context from its catch, so the proof is a save after the refusal and a fresh read.
    /// </summary>
    [Fact]
    public async Task ALevelOnALearnerFeedbackRelease_IsRefused_AndNothingIsReleasedOrRecorded()
    {
        await using var db = CreateDb();
        Seed(db);
        var campaign = AddUnderReviewCampaign(db, LearnerFeedbackTemplateId, [Paed015],
            (MsfRespondentCategory.Learner, "Ward round"),
            (MsfRespondentCategory.Learner, "Ward round"),
            (MsfRespondentCategory.Learner, "Student tutorial"));

        var release = () => ReleaseAsync(db, campaign.Id, entrustmentLevel: 4, narrative: "A narrative.");

        (await release.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Contain("records no supervision level");

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var after = await db.MsfCampaigns.SingleAsync();
        after.State.Should().Be(MsfCampaignState.UnderReview);
        after.ReleasedOn.Should().BeNull();
        after.CoordinatorNarrative.Should().BeNull();
        (await db.Activities.CountAsync()).Should().Be(0);
    }

    // ─── The report ──────────────────────────────────────────────────────────

    [Fact]
    public async Task TheReport_ListsTheDistinctTeachingContextsWhoseLearnersAnswered()
    {
        await using var db = CreateDb();
        Seed(db);
        var campaign = AddUnderReviewCampaign(db, LearnerFeedbackTemplateId, [Paed015],
            (MsfRespondentCategory.Learner, "Ward round"),
            (MsfRespondentCategory.Learner, " ward  round "),
            (MsfRespondentCategory.Learner, "Student tutorial"));

        // Invited from a third context, and never answered: not a context the feedback came from.
        campaign.Invitations.Add(Invitation(MsfRespondentCategory.Learner, "Outpatient clinic"));
        await db.SaveChangesAsync();

        var graph = await MsfCampaignRules.GetCampaignGraphAsync(db, campaign.Id, CancellationToken.None);
        var report = new MsfAggregationService().BuildReport(graph, [], nameTeachingContexts: true);

        report.Kind.Should().Be(MsfTemplateKind.LearnerFeedback);
        report.TeachingContextsResponded.Should().Equal("Student tutorial", "Ward round");
        report.TeachingContextCount.Should().Be(2);
        report.MinimumRespondentCategories.Should().Be(1);
        report.ReadyForRelease.Should().BeTrue("one respondent group, learners, is all learner feedback has");

        // Named only when asked: every other report of it (the trainee's, the PDF's) counts them and names none.
        var unnamed = new MsfAggregationService().BuildReport(graph, []);
        unnamed.TeachingContextsResponded.Should().BeNull();
        unnamed.TeachingContextCount.Should().Be(2);
    }

    /// <summary>
    /// T164 review: the trainee's copy of the report counts the teaching contexts and names none. A context is a finer
    /// breakdown than the respondent group the suppression threshold protects, so a name beside a handful of answers can
    /// say which learner wrote which. The coordinator, who typed them, is told them.
    /// </summary>
    [Fact]
    public async Task TheTraineesReport_CountsTheTeachingContexts_AndNamesNone_WhileTheCoordinatorsNamesThem()
    {
        await using var db = CreateDb();
        Seed(db);
        var campaign = AddUnderReviewCampaign(db, LearnerFeedbackTemplateId, [Paed015],
            (MsfRespondentCategory.Learner, "Ward round"),
            (MsfRespondentCategory.Learner, "Ward round"),
            (MsfRespondentCategory.Learner, "Neonatal night teaching"));
        await ReleaseAsync(db, campaign.Id, entrustmentLevel: null, narrative: null);

        var coordinators = await ReportAsync(db, campaign.Id, Coordinator());
        var trainees = await ReportAsync(db, campaign.Id, Trainee());

        coordinators!.TeachingContextCount.Should().Be(2);
        coordinators.TeachingContextsResponded.Should().Equal("Neonatal night teaching", "Ward round");
        trainees!.TeachingContextCount.Should().Be(2);
        trainees.TeachingContextsResponded.Should().BeNull("the trainee is told how many contexts answered, never which");
    }

    /// <summary>
    /// T164 review, after T186: which declared EPAs a released campaign recorded is read from its evidence rows, and a
    /// learner-feedback campaign's are <c>learner_feedback_cpsa</c> rows. Read as MSF's, PAED-015 would be "not recorded"
    /// beside the record the release had just written. Released through the handler, so the writer and the reader are
    /// the product's own.
    /// </summary>
    [Fact]
    public async Task AReleasedLearnerFeedbackCampaign_ReportsPaed015Recorded_ByItsLearnerFeedbackRecord()
    {
        await using var db = CreateDb();
        Seed(db);
        var campaign = AddUnderReviewCampaign(db, LearnerFeedbackTemplateId, [Paed015],
            (MsfRespondentCategory.Learner, "Ward round"),
            (MsfRespondentCategory.Learner, "Student tutorial"),
            (MsfRespondentCategory.Learner, "Student tutorial"));
        await ReleaseAsync(db, campaign.Id, entrustmentLevel: null, narrative: null);

        var report = await ReportAsync(db, campaign.Id, Coordinator());

        report!.CoveredEpas.Should().ContainSingle().Which.Should().Be(
            new MsfCoveredEpaDto(Paed015, "PAED-015", "Teach and supervise", Recorded: true));
    }

    /// <summary>
    /// The other direction: an <c>msf_cpsa</c> row naming a learner-feedback campaign, which no release writes, does not
    /// make it recorded. Each campaign is read from its own kind's type, not from whichever type names it.
    /// </summary>
    [Fact]
    public async Task ALearnerFeedbackCampaign_IsNotRecordedByAnMsfRecordNamingIt()
    {
        await using var db = CreateDb();
        Seed(db);
        var campaign = AddUnderReviewCampaign(db, LearnerFeedbackTemplateId, [Paed015],
            (MsfRespondentCategory.Learner, "Ward round"));
        campaign.State = MsfCampaignState.Released;
        campaign.ReleasedOn = DateTime.UtcNow.AddDays(-1);
        db.Activities.Add(new Activity
        {
            ActivityTypeId = 100,
            SchemaVersion = 1,
            SubjectUserId = TraineeUserId,
            CreatedByUserId = CoordinatorUserId,
            CurrentState = "recorded",
            DataJson = "{ \"epa_id\": " + Paed015 + ", \"campaign_id\": " + campaign.Id + ", \"respondent_count\": 1 }",
            EpaId = Paed015,
            CreatedOn = DateTime.UtcNow.AddDays(-1),
            UpdatedOn = DateTime.UtcNow.AddDays(-1),
            ObservedOn = DateOnly.FromDateTime(campaign.ClosedOn!.Value),
            ObservedOnSource = ObservationDateSource.Declared
        });
        await db.SaveChangesAsync();

        var report = await ReportAsync(db, campaign.Id, Coordinator());

        report!.CoveredEpas.Should().ContainSingle().Which.Recorded.Should().BeFalse(
            "an msf_cpsa row is MSF's evidence, and campaign {0} is learner feedback", campaign.Id);
    }

    [Fact]
    public async Task TheReportOfAnMsf_CountsNoTeachingContexts()
    {
        await using var db = CreateDb();
        Seed(db);
        var campaign = AddUnderReviewCampaign(db, MsfTemplateId, [Paed001],
            (MsfRespondentCategory.Consultant, null),
            (MsfRespondentCategory.Nurse, null));

        var report = new MsfAggregationService().BuildReport(
            await MsfCampaignRules.GetCampaignGraphAsync(db, campaign.Id, CancellationToken.None), [], nameTeachingContexts: true);

        report.Kind.Should().Be(MsfTemplateKind.Msf);
        report.TeachingContextsResponded.Should().BeNull();
        report.TeachingContextCount.Should().BeNull();
    }

    // ─── Create ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task ALearnerFeedbackCampaign_MayCoverPaed015_AndIsRefusedAnEpaWhoseListDoesNotNameIt()
    {
        await using var db = CreateDb();
        Seed(db);

        var created = await CreateAsync(db, LearnerFeedbackTemplateId, [Paed015], minimumRespondentCategories: 1);
        (await db.MsfCampaigns.Include(entity => entity.CoveredEpas).SingleAsync(entity => entity.Id == created.Id))
            .CoveredEpas.Select(covered => covered.EpaId).Should().Equal(Paed015);

        var refused = () => CreateAsync(db, LearnerFeedbackTemplateId, [Paed001, Paed015], minimumRespondentCategories: 1);
        (await refused.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Contain("does not name learner feedback").And.Contain(Paed001.ToString());
    }

    [Fact]
    public async Task AnMsfCampaign_StillCoversAnyEpaOnTheCurriculum_WhateverItsList()
    {
        await using var db = CreateDb();
        Seed(db);

        var created = await CreateAsync(db, MsfTemplateId, [Paed001, Paed015], minimumRespondentCategories: 2);

        (await db.MsfCampaigns.Include(entity => entity.CoveredEpas).SingleAsync(entity => entity.Id == created.Id))
            .CoveredEpas.Select(covered => covered.EpaId).Should().BeEquivalentTo([Paed001, Paed015]);
    }

    [Fact]
    public async Task ALearnerFeedbackCampaign_CannotRequireTwoRespondentGroups_ForItHasOne()
    {
        await using var db = CreateDb();
        Seed(db);

        var refused = () => CreateAsync(db, LearnerFeedbackTemplateId, [Paed015], minimumRespondentCategories: 2);

        (await refused.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Contain("one respondent group");
        (await db.MsfCampaigns.CountAsync()).Should().Be(0);
    }

    // ─── Invitations ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ALearnerFeedbackCampaign_InvitesLearners_WithTheirTeachingContextAsStored()
    {
        await using var db = CreateDb();
        Seed(db);
        var created = await CreateAsync(db, LearnerFeedbackTemplateId, [Paed015], minimumRespondentCategories: 1);

        var id = await InviteAsync(db, created.Id, MsfRespondentCategory.Learner, "  Student   tutorial ");

        (await db.MsfInvitations.SingleAsync(invitation => invitation.Id == id)).TeachingContext.Should().Be("Student tutorial");
    }

    [Theory]
    [InlineData(MsfRespondentCategory.PeerDoctor)]
    [InlineData(MsfRespondentCategory.Consultant)]
    [InlineData(MsfRespondentCategory.Nurse)]
    [InlineData(MsfRespondentCategory.Ahp)]
    [InlineData(MsfRespondentCategory.Patient)]
    [InlineData(MsfRespondentCategory.Other)]
    public async Task ALearnerFeedbackCampaign_RefusesEveryoneButALearner(MsfRespondentCategory category)
    {
        await using var db = CreateDb();
        Seed(db);
        var created = await CreateAsync(db, LearnerFeedbackTemplateId, [Paed015], minimumRespondentCategories: 1);

        var invite = () => InviteAsync(db, created.Id, category, teachingContext: null);

        (await invite.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Contain("learners only");
        (await db.MsfInvitations.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task AnMsfCampaign_RefusesALearner()
    {
        await using var db = CreateDb();
        Seed(db);
        var created = await CreateAsync(db, MsfTemplateId, [Paed001], minimumRespondentCategories: 2);

        var invite = () => InviteAsync(db, created.Id, MsfRespondentCategory.Learner, "Ward round");

        (await invite.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Contain("learner-feedback campaign");
        (await db.MsfInvitations.CountAsync()).Should().Be(0);
    }

    /// <summary>
    /// The campaign form reads whom a campaign may invite through a query scoped as the report is (T113): the campaign's
    /// runner gets the kind and the categories; a coordinator elsewhere, and an id that names nothing, get null alike.
    /// </summary>
    [Fact]
    public async Task TheFormsSetup_AnswersTheCampaignsRunner_AndNobodyElse()
    {
        await using var db = CreateDb();
        Seed(db);
        var created = await CreateAsync(db, LearnerFeedbackTemplateId, [Paed015], minimumRespondentCategories: 1);
        var handler = new GetMsfCampaignSetupQueryHandler(db);

        var setup = await handler.Handle(new GetMsfCampaignSetupQuery(created.Id, Coordinator()), CancellationToken.None);
        setup.Should().NotBeNull();
        setup!.Kind.Should().Be(MsfTemplateKind.LearnerFeedback);
        setup.AcceptedCategories.Should().Equal(MsfRespondentCategory.Learner);

        (await handler.Handle(new GetMsfCampaignSetupQuery(created.Id, Coordinator(institutionId: 99)), CancellationToken.None))
            .Should().BeNull("a coordinator at another institution does not run this campaign");
        (await handler.Handle(new GetMsfCampaignSetupQuery(created.Id + 100, Coordinator()), CancellationToken.None))
            .Should().BeNull();
    }

    [Fact]
    public void ALearnerIsNeverInvitedWithoutATeachingContext_AndNobodyElseWithOne()
    {
        var validator = new AddMsfInvitationCommandValidator();

        validator.TestValidate(Invite(MsfRespondentCategory.Learner, null))
            .ShouldHaveValidationErrorFor(command => command.TeachingContext);
        validator.TestValidate(Invite(MsfRespondentCategory.Learner, "   "))
            .ShouldHaveValidationErrorFor(command => command.TeachingContext);
        validator.TestValidate(Invite(MsfRespondentCategory.Learner, new string('x', 201)))
            .ShouldHaveValidationErrorFor(command => command.TeachingContext);
        validator.TestValidate(Invite(MsfRespondentCategory.Learner, "Ward round"))
            .ShouldNotHaveAnyValidationErrors();

        validator.TestValidate(Invite(MsfRespondentCategory.Nurse, "Ward round"))
            .ShouldHaveValidationErrorFor(command => command.TeachingContext);
        validator.TestValidate(Invite(MsfRespondentCategory.Nurse, null))
            .ShouldNotHaveAnyValidationErrors();

        static AddMsfInvitationCommand Invite(MsfRespondentCategory category, string? context)
            => new(1, "someone@example.test", category, Coordinator(), context);
    }

    [Fact]
    public void ALearnerFeedbackTemplate_CannotAllowPatients()
    {
        var validator = new CreateMsfTemplateCommandValidator();
        var questions = new[] { new CreateMsfTemplateQuestionItem("Teaching overall", MsfQuestionType.Scale, null, true) };

        validator.TestValidate(new CreateMsfTemplateCommand("Learner feedback", null, true, questions, MsfTemplateKind.LearnerFeedback))
            .ShouldHaveValidationErrorFor(command => command.AllowPatientResponses);
        validator.TestValidate(new CreateMsfTemplateCommand("Learner feedback", null, false, questions, MsfTemplateKind.LearnerFeedback))
            .ShouldNotHaveAnyValidationErrors();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static async Task ReleaseAsync(ApplicationDbContext db, int campaignId, int? entrustmentLevel, string? narrative)
    {
        var handler = new ReleaseMsfCampaignCommandHandler(
            db,
            new MsfAggregationService(),
            new ActivityService(db, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator()),
            new ActivityReferenceDataService(db),
            NullLogger<ReleaseMsfCampaignCommandHandler>.Instance);

        await handler.Handle(
            new ReleaseMsfCampaignCommand(campaignId, CoordinatorUserId, narrative, entrustmentLevel, Coordinator()),
            CancellationToken.None);
    }

    private static Task<MsfCampaignAggregateReportDto?> ReportAsync(ApplicationDbContext db, int campaignId, ClaimsPrincipal caller)
        => new GetCampaignAggregateReportQueryHandler(db, new MsfAggregationService()).Handle(
            new GetCampaignAggregateReportQuery(campaignId, caller), CancellationToken.None);

    private static Task<MsfCampaignSummaryDto> CreateAsync(
        ApplicationDbContext db, int templateId, int[] epaIds, int minimumRespondentCategories)
        => new CreateMsfCampaignCommandHandler(db, new ActivityReferenceDataService(db)).Handle(
            new CreateMsfCampaignCommand(
                TraineeUserId,
                templateId,
                new DateOnly(2026, 8, 1),
                new DateOnly(2026, 8, 31),
                MinimumResponses: 3,
                MinimumCategoryResponses: 3,
                minimumRespondentCategories,
                epaIds,
                CoordinatorUserId,
                Coordinator()),
            CancellationToken.None);

    private static Task<int> InviteAsync(ApplicationDbContext db, int campaignId, MsfRespondentCategory category, string? teachingContext)
        => new AddMsfInvitationCommandHandler(db, new InvitationTokenService()).Handle(
            new AddMsfInvitationCommand(campaignId, $"{Guid.NewGuid():N}@example.test", category, Coordinator(), teachingContext),
            CancellationToken.None);

    /// <summary>
    /// A curriculum holding PAED-001 (whose list names MSF, not learner feedback) and PAED-015 (whose list names both), an
    /// admitted trainee, both real evidence types, and one template of each kind.
    /// </summary>
    private static void Seed(ApplicationDbContext db)
    {
        db.Institutions.Add(new Institution { Id = InstitutionId, Name = "Host" });
        db.Epas.Add(new Epa { Id = Paed001, SubSpecialityId = 1, Code = "PAED-001", Title = "Resuscitate", IsActive = true });
        db.Epas.Add(new Epa { Id = Paed015, SubSpecialityId = 1, Code = "PAED-015", Title = "Teach and supervise", IsActive = true });

        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 4001, CurriculumId = CurriculumId, EpaId = Paed001, RequiredCount = 6, MinimumLevelOrder = 3,
            PermittedToolsJson = """["cbd","mini_cex","msf"]"""
        });
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 4015, CurriculumId = CurriculumId, EpaId = Paed015, RequiredCount = 6, MinimumLevelOrder = 3,
            PermittedToolsJson = """["cbd","direct_observation","learner_feedback","msf","portfolio_review"]"""
        });

        db.Set<Speciality>().Add(new Speciality { Id = SpecialityId, CollegeId = 1, Name = "Paediatrics" });
        db.Set<SubSpeciality>().Add(new SubSpeciality { Id = 1, SpecialityId = SpecialityId, Name = "General Paediatrics" });
        db.Set<Curriculum>().Add(new Curriculum { Id = CurriculumId, SubSpecialityId = 1, Name = "CPSA Paediatrics", Version = "11.1" });

        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = TraineeUserId,
            InstitutionId = InstitutionId,
            CurriculumId = CurriculumId,
            ProgrammeStartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-1),
            ExpectedCompletionDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(2),
            IsActive = true
        });

        db.ActivityTypes.Add(SeededType(100, "msf_cpsa", "Multi-Source Feedback (Paediatrics)"));
        db.ActivityTypes.Add(SeededType(101, "learner_feedback_cpsa", "Learner Feedback (Paediatrics)"));

        db.MsfTemplates.Add(Template(MsfTemplateId, "Annual MSF", MsfTemplateKind.Msf));
        db.MsfTemplates.Add(Template(LearnerFeedbackTemplateId, "Learner feedback (interim questionnaire)", MsfTemplateKind.LearnerFeedback));

        db.SaveChanges();
    }

    private static MsfTemplate Template(int id, string name, MsfTemplateKind kind)
        => new()
        {
            Id = id,
            Name = name,
            Kind = kind,
            Questions =
            [
                new MsfQuestion { Id = id * 10 + 1, Order = 1, Prompt = "Overall", Type = MsfQuestionType.Scale, Required = true },
                new MsfQuestion { Id = id * 10 + 2, Order = 2, Prompt = "Comments", Type = MsfQuestionType.LongText, Required = false }
            ]
        };

    /// <summary>
    /// A campaign closed five days ago and under review, with one returned questionnaire per respondent given. Its
    /// thresholds are the least that let every respondent group given report, so the release gate is not what a test is
    /// about.
    /// </summary>
    private static MsfCampaign AddUnderReviewCampaign(
        ApplicationDbContext db,
        int templateId,
        int[] coveredEpaIds,
        params (MsfRespondentCategory Category, string? TeachingContext)[] respondents)
    {
        var template = db.MsfTemplates.Local.Single(entity => entity.Id == templateId);
        var campaign = new MsfCampaign
        {
            SubjectUserId = TraineeUserId,
            TemplateId = templateId,
            CreatedByUserId = CoordinatorUserId,
            CreatedOn = DateTime.UtcNow.AddDays(-30),
            OpensOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30),
            ClosesOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-5),
            MinimumResponses = 1,
            MinimumCategoryResponses = 1,
            MinimumRespondentCategories = template.Kind == MsfTemplateKind.LearnerFeedback ? 1 : 2,
            State = MsfCampaignState.UnderReview,
            ClosedOn = DateTime.UtcNow.AddDays(-5),
            CoveredEpas = coveredEpaIds.Select(epaId => new MsfCampaignEpa { EpaId = epaId }).ToList()
        };

        foreach (var (category, context) in respondents)
        {
            var invitation = Invitation(category, context);
            invitation.RespondedOn = DateTime.UtcNow.AddDays(-6);
            invitation.Responses.Add(new MsfResponse
            {
                SubmittedOn = DateTime.UtcNow.AddDays(-6),
                Campaign = campaign,
                Answers =
                [
                    new MsfResponseAnswer { QuestionId = template.Questions.First().Id, ScaleValue = 4 },
                    new MsfResponseAnswer { QuestionId = template.Questions.Last().Id, LongText = "Explained it twice, kindly." }
                ]
            });

            campaign.Invitations.Add(invitation);
            foreach (var response in invitation.Responses)
            {
                campaign.Responses.Add(response);
            }
        }

        db.MsfCampaigns.Add(campaign);
        db.SaveChanges();
        return campaign;
    }

    private static MsfInvitation Invitation(MsfRespondentCategory category, string? teachingContext)
        => new()
        {
            RespondentCategory = category,
            TeachingContext = teachingContext,
            RespondentEmailHash = "hash",
            TokenHash = Guid.NewGuid().ToString("N"),
            IssuedOn = DateTime.UtcNow.AddDays(-20),
            ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-2),
            AnonymizedOn = DateTime.UtcNow.AddDays(-5)
        };

    /// <summary>A shipped seed, read off disk and published at v1, system-managed as its catalogue entry declares.</summary>
    private static ActivityType SeededType(int id, string key, string name)
    {
        var schemaJson = ReadSeedFile(key, "schema.json");
        var workflowJson = ReadSeedFile(key, "workflow.json");
        var creditRulesJson = ReadSeedFile(key, "credit.json");

        var activityType = new ActivityType
        {
            Id = id,
            Key = key,
            Name = name,
            Scope = ActivityScope.Speciality,
            ScopeId = SpecialityId,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditRulesJson,
            DisplayFieldsJson = "[]",
            OwnerUserId = "seed-system",
            CreatedOn = DateTime.UtcNow,
            SystemManaged = true,
            WbaToolKey = key == "learner_feedback_cpsa" ? "learner_feedback" : "msf"
        };

        activityType.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = id,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditRulesJson,
            DisplayFieldsJson = "[]",
            PublishedByUserId = "seed-system",
            PublishedOn = DateTime.UtcNow
        });

        return activityType;
    }

    private static string ReadSeedFile(string key, string fileName)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", key, fileName));

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    /// <summary>The trainee the campaigns are about.</summary>
    private static ClaimsPrincipal Trainee()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, TraineeUserId),
                new Claim(ClaimTypes.Role, WombatRoles.Trainee),
                new Claim(WombatClaimTypes.InstitutionId, InstitutionId.ToString())
            ],
            "test",
            ClaimTypes.Name,
            ClaimTypes.Role));

    /// <summary>The coordinator at the trainee's institution, with a role claim type the BCL's <c>IsInRole</c> reads.</summary>
    private static ClaimsPrincipal Coordinator(int institutionId = InstitutionId)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, CoordinatorUserId),
                new Claim(ClaimTypes.Role, WombatRoles.Coordinator),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString())
            ],
            "test",
            ClaimTypes.Name,
            ClaimTypes.Role));
}
