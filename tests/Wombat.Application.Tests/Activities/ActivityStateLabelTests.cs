using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityById;
using Wombat.Application.Features.Activities.Queries.ListActivitiesByActorInbox;
using Wombat.Application.Features.Activities.Queries.ListActivitiesBySubject;
using Wombat.Application.Features.Dashboards.Assessor;
using Wombat.Application.Features.Dashboards.Trainee;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T220: every query that shows an activity's state or its recorded moves carries them in the words of the activity's
/// PINNED workflow, as T189's refusals and notices already name them. Driven over the shipped <c>clinical_audit_cpsa</c>
/// seed, whose <c>submitted</c> state is "Awaiting supervisor" and whose finishing move is two words (<c>sign_off</c>).
/// </summary>
/// <remarks>
/// A real filing: the trainee creates the audit and submits it through <c>ActivityService</c>, and the assessor signs it
/// off, so each history row is the one the write path records.
/// </remarks>
public sealed class ActivityStateLabelTests
{
    private const string TraineeId = "trainee-1";
    private const string AssessorId = "assessor-1";
    private const int InstitutionId = 10;
    private const int TypeId = 1;
    private const int EpaId = 5000;
    private const int CurriculumId = 3000;

    private static readonly DateOnly ProgrammeStart = new(2026, 1, 12);

    [Fact]
    public async Task ASubmittedClinicalAudit_ReadsAwaitingSupervisor_OnItsPage_WithItsHistoryInWords()
    {
        var options = await SeededAsync();
        var submitted = await SubmittedAuditAsync(options);

        await using var db = new ApplicationDbContext(options);
        var detail = await new GetActivityByIdQueryHandler(Service(db), new FakeUserDirectory())
            .Handle(new GetActivityByIdQuery(submitted, Principal(TraineeId)), CancellationToken.None);

        detail!.Activity.CurrentState.Should().Be("submitted", "the key stays for logic");
        detail.Activity.CurrentStateLabel.Should().Be("Awaiting supervisor");
        detail.Activity.Transitions.Select(row => (row.TransitionLabel, row.FromStateLabel, row.ToStateLabel))
            .Should().Equal(("Create", "Draft", "Draft"), ("Submit", "Draft", "Awaiting supervisor"));
        detail.Activity.Transitions.Select(row => row.TransitionKey).Should().Equal("create", "submit");
    }

    [Fact]
    public async Task ASignedOffAudit_NamesItsTwoWordMove_AsItsButtonDid()
    {
        var options = await SeededAsync();
        var submitted = await SubmittedAuditAsync(options);
        await TransitionAsync(options, submitted, "sign_off", AssessorId, """{ "supervisor_comments": "A complete cycle." }""");

        await using var db = new ApplicationDbContext(options);
        var detail = await new GetActivityByIdQueryHandler(Service(db), new FakeUserDirectory())
            .Handle(new GetActivityByIdQuery(submitted, Principal(TraineeId)), CancellationToken.None);

        detail!.Activity.CurrentStateLabel.Should().Be("Signed off");
        var signOff = detail.Activity.Transitions.Last();
        signOff.TransitionLabel.Should().Be("Sign Off").And.Be(new ActivityActionDto("sign_off", false).Label);
        (signOff.FromStateLabel, signOff.ToStateLabel).Should().Be(("Awaiting supervisor", "Signed off"));
    }

    [Fact]
    public async Task TheTraineesList_ReadsAwaitingSupervisor()
    {
        var options = await SeededAsync();
        var submitted = await SubmittedAuditAsync(options);

        await using var db = new ApplicationDbContext(options);
        var rows = await new ListActivitiesBySubjectQueryHandler(db)
            .Handle(new ListActivitiesBySubjectQuery(TraineeId, Principal(TraineeId)), CancellationToken.None);

        var row = rows.Should().ContainSingle(item => item.Id == submitted).Subject;
        row.CurrentState.Should().Be("submitted");
        row.CurrentStateLabel.Should().Be("Awaiting supervisor");
    }

    [Fact]
    public async Task TheAssessorsInbox_ReadsAwaitingSupervisor()
    {
        var options = await SeededAsync();
        var submitted = await SubmittedAuditAsync(options);

        await using var db = new ApplicationDbContext(options);
        var rows = await new ListActivitiesByActorInboxQueryHandler(db, new WorkflowEvaluator(), new FakeUserDirectory())
            .Handle(new ListActivitiesByActorInboxQuery(Principal(AssessorId)), CancellationToken.None);

        rows.Should().ContainSingle(item => item.Id == submitted)
            .Which.CurrentStateLabel.Should().Be("Awaiting supervisor");
    }

    [Fact]
    public async Task TheTraineesDashboard_ReadsAwaitingSupervisor()
    {
        var options = await SeededAsync();
        var submitted = await SubmittedAuditAsync(options);

        await using var db = new ApplicationDbContext(options);
        var summary = await new GetTraineeDashboardSummaryQueryHandler(db, new WorkflowEvaluator(), FakeUserDirectory.Empty).Handle(
            new GetTraineeDashboardSummaryQuery(Principal(TraineeId, WombatRoles.Trainee), new DateOnly(2026, 3, 20)),
            CancellationToken.None);

        var recent = summary.RecentActivities.Should().ContainSingle(item => item.ActivityId == submitted).Subject;
        recent.CurrentState.Should().Be("submitted", "the badge's colour class");
        recent.CurrentStateLabel.Should().Be("Awaiting supervisor");
    }

    [Fact]
    public async Task TheTraineesDashboardInbox_NamesADraftByItsLabel()
    {
        var options = await SeededAsync();
        var draft = await CreateAuditAsync(options);

        await using var db = new ApplicationDbContext(options);
        var summary = await new GetTraineeDashboardSummaryQueryHandler(db, new WorkflowEvaluator(), FakeUserDirectory.Empty).Handle(
            new GetTraineeDashboardSummaryQuery(Principal(TraineeId, WombatRoles.Trainee), new DateOnly(2026, 3, 20)),
            CancellationToken.None);

        summary.Inbox.Should().ContainSingle(item => item.ActivityId == draft)
            .Which.CurrentStateLabel.Should().Be("Draft");
    }

    [Fact]
    public async Task TheAssessorsDashboard_NamesADecisionByItsLabel()
    {
        var options = await SeededAsync();
        var submitted = await SubmittedAuditAsync(options);
        await TransitionAsync(options, submitted, "sign_off", AssessorId, """{ "supervisor_comments": "A complete cycle." }""");

        await using var db = new ApplicationDbContext(options);
        var summary = await new GetAssessorDashboardSummaryQueryHandler(db, new WorkflowEvaluator(), new FakeUserDirectory(), Options.Create(new DashboardThresholds()))
            .Handle(new GetAssessorDashboardSummaryQuery(Principal(AssessorId)), CancellationToken.None);

        var decision = summary.RecentDecisions.Should().ContainSingle(item => item.ActivityId == submitted).Subject;
        decision.FinalState.Should().Be("signed_off", "the badge's colour class");
        decision.FinalStateLabel.Should().Be("Signed off");
    }

    /// <summary>
    /// The label is the PINNED version's. A later version that renames the state reaches only activities filed against it.
    /// </summary>
    [Fact]
    public async Task AnActivityPinnedToAnEarlierVersion_IsNamedByThatVersion_NotByTheTypesCurrentOne()
    {
        var options = await SeededAsync();
        var submitted = await SubmittedAuditAsync(options);
        await PublishRenamedVersionAsync(options, "With the reviewer");

        await using var db = new ApplicationDbContext(options);
        var mine = await new ListActivitiesBySubjectQueryHandler(db)
            .Handle(new ListActivitiesBySubjectQuery(TraineeId, Principal(TraineeId)), CancellationToken.None);
        var inbox = await new ListActivitiesByActorInboxQueryHandler(db, new WorkflowEvaluator(), new FakeUserDirectory())
            .Handle(new ListActivitiesByActorInboxQuery(Principal(AssessorId)), CancellationToken.None);
        var detail = await new GetActivityByIdQueryHandler(Service(db), new FakeUserDirectory())
            .Handle(new GetActivityByIdQuery(submitted, Principal(TraineeId)), CancellationToken.None);

        mine.Single(item => item.Id == submitted).CurrentStateLabel.Should().Be("Awaiting supervisor");
        inbox.Single(item => item.Id == submitted).CurrentStateLabel.Should().Be("Awaiting supervisor");
        detail!.Activity.CurrentStateLabel.Should().Be("Awaiting supervisor");
    }

    /// <summary>
    /// A stored state the pinned version does not declare is shown by its key: the page says what is stored rather than
    /// nothing.
    /// </summary>
    [Fact]
    public async Task AStateThePinnedVersionDoesNotDeclare_IsShownByItsKey()
    {
        var options = await SeededAsync();
        var draft = await CreateAuditAsync(options);
        await using (var write = new ApplicationDbContext(options))
        {
            var stored = await write.Activities.SingleAsync(activity => activity.Id == draft);
            stored.CurrentState = "archived";
            await write.SaveChangesAsync();
        }

        await using var db = new ApplicationDbContext(options);
        var rows = await new ListActivitiesBySubjectQueryHandler(db)
            .Handle(new ListActivitiesBySubjectQuery(TraineeId, Principal(TraineeId)), CancellationToken.None);

        rows.Single(item => item.Id == draft).CurrentStateLabel.Should().Be("archived");
    }

    // ---- helpers -------------------------------------------------------------------------------------------------

    private static readonly string AuditData = $$"""
        {
          "epa_id": {{EpaId}},
          "assessor_user_id": "{{AssessorId}}",
          "observed_on": "2026-03-10",
          "audit_title": "Hand hygiene before resuscitation",
          "standard": "The WHO five moments, as adopted in the unit's infection control policy.",
          "sample": "Forty consecutive resuscitation-bay episodes in February 2026.",
          "findings": "31 of 40 compliant, against a standard of 95%.",
          "change_made": "Alcohol rub at the bay entrance, and a prompt on the resuscitation checklist.",
          "reaudit_on": "2026-06-10",
          "report_link": "https://intranet.example.org/audits/hand-hygiene-2026.pdf"
        }
        """;

    private static async Task<int> CreateAuditAsync(DbContextOptions<ApplicationDbContext> options)
    {
        await using var db = new ApplicationDbContext(options);
        var created = await Service(db).CreateDraftAsync(
            new CreateActivityInput(TypeId, TraineeId, TraineeId, AuditData, Principal(TraineeId)));
        return created.Id;
    }

    private static async Task<int> SubmittedAuditAsync(DbContextOptions<ApplicationDbContext> options)
    {
        var id = await CreateAuditAsync(options);
        var submitted = await TransitionAsync(options, id, "submit", TraineeId);
        submitted.CurrentState.Should().Be("submitted");
        return id;
    }

    private static async Task<ActivityDto> TransitionAsync(
        DbContextOptions<ApplicationDbContext> options,
        int activityId,
        string transitionKey,
        string actorUserId,
        string? patch = null)
    {
        await using var db = new ApplicationDbContext(options);
        return await Service(db).TransitionAsync(new TransitionActivityInput(
            activityId, transitionKey, actorUserId, Principal(actorUserId), patch, null));
    }

    /// <summary>Publishes version 2 of the type, whose <c>submitted</c> state is called <paramref name="label" />.</summary>
    private static async Task PublishRenamedVersionAsync(DbContextOptions<ApplicationDbContext> options, string label)
    {
        await using var db = new ApplicationDbContext(options);
        var type = await db.ActivityTypes.Include(entity => entity.Versions).SingleAsync(entity => entity.Id == TypeId);
        var renamed = type.WorkflowJson!.Replace("Awaiting supervisor", label, StringComparison.Ordinal);
        renamed.Should().NotBe(type.WorkflowJson, "the seed names the state 'Awaiting supervisor'");

        type.WorkflowJson = renamed;
        type.Version = 2;
        type.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = TypeId,
            Version = 2,
            SchemaJson = type.SchemaJson!,
            WorkflowJson = renamed,
            CreditRulesJson = type.CreditRulesJson!,
            DisplayFieldsJson = "[]",
            PublishedByUserId = "system",
            PublishedOn = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task<DbContextOptions<ApplicationDbContext>> SeededAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);

        NomineeSeed.AddUser(db, TraineeId, InstitutionId, WombatRoles.Trainee);
        NomineeSeed.AddUser(db, AssessorId, InstitutionId, WombatRoles.Assessor);

        db.Epas.Add(new Epa { Id = EpaId, Code = "PAED-001", Title = "Quality improvement" });
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = TraineeId,
            InstitutionId = InstitutionId,
            CurriculumId = CurriculumId,
            ProgrammeStartDate = ProgrammeStart,
            ExpectedCompletionDate = ProgrammeStart.AddYears(4),
            IsActive = true
        });

        string Read(string file) => File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", "clinical_audit_cpsa", file));
        var schemaJson = Read("schema.json");
        var workflowJson = Read("workflow.json");
        var creditJson = Read("credit.json");

        var type = new ActivityType
        {
            Id = TypeId,
            Key = "clinical_audit_cpsa",
            Name = "Clinical Audit (Paediatrics)",
            Scope = ActivityScope.Global,
            Version = 1,
            IsActive = true,
            WbaToolKey = "clinical_audit",
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditJson,
            DisplayFieldsJson = "[]",
            OwnerUserId = "system",
            CreatedOn = DateTime.UtcNow
        };
        type.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = TypeId,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditJson,
            DisplayFieldsJson = "[]",
            PublishedByUserId = "system",
            PublishedOn = DateTime.UtcNow
        });
        db.ActivityTypes.Add(type);

        await db.SaveChangesAsync();
        return options;
    }

    private static ActivityService Service(ApplicationDbContext db)
        => new(db, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator());

    private static ClaimsPrincipal Principal(string userId, params string[] roles)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId),
                .. roles.Select(role => new Claim(ClaimTypes.Role, role))
            ],
            "test"));
}
