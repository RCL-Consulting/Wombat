using System.Security.Claims;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;
using Microsoft.EntityFrameworkCore;

namespace Wombat.Application.Tests.Features.Programme;

/// <summary>
/// The paediatric cast at Step 3.52, cut down to what flow 06's reads need (T358, lane A2): five registrars at Kgosi Kgari
/// Teaching Hospital in Paediatrics, a surgical registrar beside them, another hospital's registrar, and the staff who read
/// them. Today is D, 2026-10-04 (semester 2 of 2026, which ends on 2026-11-30).
/// </summary>
/// <remarks>
/// <para>
/// Five semester EPAs at three a semester and one yearly EPA at one a year. The credit is chosen so the figures the boards
/// give at 3.52 come out (round-2 review 16): Nomsa Mahlangu and Sipho Ndlovu hold PAED-002 at 1 of 3; Anele Dlamini and
/// Lerato Molefe have met PAED-001; Anele Dlamini and Nomsa Mahlangu hold PAED-004 at 1 of 3. So Anele Dlamini's furthest
/// short are PAED-002, 003 and 005, and Nomsa Mahlangu's PAED-001, 003 and 005.
/// </para>
/// <para>
/// Training years come from the programme start (<see cref="TraineeProfile.GetStage" />): 1 for a start on 2026-01-15, 2 a
/// year earlier, and so on.
/// </para>
/// </remarks>
internal static class ProgrammeCast
{
    public static readonly DateOnly D = new(2026, 10, 4);

    /// <summary>10:00 on D in South Africa.</summary>
    public static readonly DateTime DAt10 = new(2026, 10, 4, 8, 0, 0, DateTimeKind.Utc);

    public const int Kgk = 10;
    public const int OtherHospital = 11;
    public const int PaediatricsSpeciality = 100;
    public const int SurgerySpeciality = 101;
    public const int PaediatricsSubSpeciality = 1000;
    public const int SurgerySubSpeciality = 1010;
    public const int PaediatricsCurriculum = 1;
    public const int SurgeryCurriculum = 2;

    public const int Paed001 = 1;
    public const int Paed002 = 2;
    public const int Paed003 = 3;
    public const int Paed004 = 4;
    public const int Paed005 = 5;
    public const int Paed008 = 8;
    public const int Kgk001 = 21;
    public const int Surg001 = 31;

    public const string DuPlessis = "du-plessis";
    public const string Dlamini = "dlamini";
    public const string Molefe = "molefe";
    public const string Mahlangu = "mahlangu";
    public const string Ndlovu = "ndlovu";
    public const string Surgeon = "surgeon";
    public const string Elsewhere = "elsewhere";

    public const int DuPlessisProfile = 101;
    public const int DlaminiProfile = 102;
    public const int MolefeProfile = 103;
    public const int MahlanguProfile = 104;
    public const int NdlovuProfile = 105;
    public const int SurgeonProfile = 106;
    public const int ElsewhereProfile = 107;

    public const int DraftType = 1;
    public const int RequestedType = 2;
    public const int LoggedType = 3;
    public const int MsfType = 4;

    /// <summary>A database holding the cast, and the directory that names them.</summary>
    public static (ApplicationDbContext Db, FakeUserDirectory Users) Build()
    {
        var db = NewDb();
        var users = new FakeUserDirectory();

        db.Institutions.AddRange(
            new Institution { Id = Kgk, Name = "Kgosi Kgari Teaching Hospital" },
            new Institution { Id = OtherHospital, Name = "Other Hospital" });
        db.Specialities.AddRange(
            new Speciality { Id = PaediatricsSpeciality, CollegeId = 1, Name = "Paediatrics" },
            new Speciality { Id = SurgerySpeciality, CollegeId = 1, Name = "Surgery" });
        db.SubSpecialities.AddRange(
            new SubSpeciality { Id = PaediatricsSubSpeciality, SpecialityId = PaediatricsSpeciality, Name = "Paediatrics" },
            new SubSpeciality { Id = SurgerySubSpeciality, SpecialityId = SurgerySpeciality, Name = "General Surgery" });
        db.Curricula.AddRange(
            new Curriculum
            {
                Id = PaediatricsCurriculum, SubSpecialityId = PaediatricsSubSpeciality, Name = "Paediatrics", Version = "11.1",
                EffectiveFrom = new DateOnly(2025, 1, 1), IsActive = true
            },
            new Curriculum
            {
                Id = SurgeryCurriculum, SubSpecialityId = SurgerySubSpeciality, Name = "Surgery", Version = "1",
                EffectiveFrom = new DateOnly(2025, 1, 1), IsActive = true
            });

        AddEpa(db, Paed001, "PAED-001", "Providing paediatric emergency care to children");
        AddEpa(db, Paed002, "PAED-002", "Managing common paediatric presentations");
        AddEpa(db, Paed003, "PAED-003", "Providing intensive care to children");
        AddEpa(db, Paed004, "PAED-004", "Managing common neonatal conditions");
        AddEpa(db, Paed005, "PAED-005", "Providing neonatal care in intensive and high-care settings");
        AddEpa(db, Paed008, "PAED-008", "Evaluating and managing neurodevelopmental and behavioural presentations in children");
        AddEpa(db, Surg001, "SURG-001", "Operating", subSpecialityId: SurgerySubSpeciality);

        foreach (var epaId in new[] { Paed001, Paed002, Paed003, Paed004, Paed005 })
        {
            AddItem(db, epaId, PaediatricsCurriculum, epaId, QuotaPeriod.Semester, 3);
        }

        AddItem(db, Paed008, PaediatricsCurriculum, Paed008, QuotaPeriod.AcademicYear, 1);
        AddItem(db, Surg001, SurgeryCurriculum, Surg001, QuotaPeriod.Semester, 2);

        AddRegistrar(db, users, DuPlessisProfile, DuPlessis, "Pieter", "du Plessis", trainingYear: 2);
        AddRegistrar(db, users, DlaminiProfile, Dlamini, "Anele", "Dlamini", trainingYear: 3);
        AddRegistrar(db, users, MolefeProfile, Molefe, "Lerato", "Molefe", trainingYear: 4);
        AddRegistrar(db, users, MahlanguProfile, Mahlangu, "Nomsa", "Mahlangu", trainingYear: 1);
        AddRegistrar(db, users, NdlovuProfile, Ndlovu, "Sipho", "Ndlovu", trainingYear: 1);
        AddRegistrar(db, users, SurgeonProfile, Surgeon, "Thabo", "Surgeon", trainingYear: 2, curriculumId: SurgeryCurriculum);
        AddRegistrar(db, users, ElsewhereProfile, Elsewhere, "Erin", "Elsewhere", trainingYear: 2, institutionId: OtherHospital);

        AddCredit(db, Paed001, Dlamini, 3);
        AddCredit(db, Paed004, Dlamini, 1);
        AddCredit(db, Paed001, Molefe, 3);
        AddCredit(db, Paed002, Mahlangu, 1);
        AddCredit(db, Paed004, Mahlangu, 1);
        AddCredit(db, Paed002, Ndlovu, 1);

        AddActivityTypes(db);

        db.SaveChanges();
        return (db, users);
    }

    public static ApplicationDbContext NewDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    /// <summary>A programme start that puts the registrar in this training year on D.</summary>
    public static DateOnly StartFor(int trainingYear) => new(2026 - (trainingYear - 1), 1, 15);

    public static TraineeProfile AddRegistrar(
        ApplicationDbContext db,
        FakeUserDirectory users,
        int profileId,
        string userId,
        string firstName,
        string lastName,
        int trainingYear,
        int institutionId = Kgk,
        int curriculumId = PaediatricsCurriculum,
        DateOnly? admittedOn = null,
        DateOnly? programmeStart = null)
    {
        var start = programmeStart ?? StartFor(trainingYear);
        var profile = new TraineeProfile
        {
            Id = profileId,
            UserId = userId,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            ProgrammeStartDate = start,
            AdmittedOn = admittedOn ?? start,
            ExpectedCompletionDate = start.AddYears(4),
            IsActive = true
        };
        db.TraineeProfiles.Add(profile);
        users.With(new UserIdentityDetails(
            userId, $"{userId}@kgk.wombat.local", firstName, lastName, institutionId, [], [], [WombatRoles.Trainee]));
        return profile;
    }

    public static void AddEpa(ApplicationDbContext db, int id, string code, string title, int subSpecialityId = PaediatricsSubSpeciality, int? owningInstitutionId = null)
        => db.Epas.Add(new Epa { Id = id, SubSpecialityId = subSpecialityId, OwningInstitutionId = owningInstitutionId, Code = code, Title = title });

    public static void AddItem(ApplicationDbContext db, int id, int curriculumId, int epaId, QuotaPeriod period, int target, int? owningInstitutionId = null)
        => db.CurriculumItems.Add(new CurriculumItem
        {
            Id = id, CurriculumId = curriculumId, EpaId = epaId, OwningInstitutionId = owningInstitutionId,
            RequiredCount = target, QuotaPeriod = period, MinimumLevelOrder = 3, WindowMonths = 12
        });

    /// <summary>Credit in semester 2 of 2026, the window D falls in.</summary>
    public static void AddCredit(ApplicationDbContext db, int itemId, string traineeUserId, int counts, int year = 2026, int semester = 2)
        => db.CurriculumItemProgresses.Add(new CurriculumItemProgress
        {
            CurriculumItemId = itemId,
            TraineeUserId = traineeUserId,
            AcademicYear = year,
            Semester = semester,
            CountsSoFar = counts,
            MinimumLevelReachedCount = counts,
            LastObservedOn = new AcademicPeriod(year, semester).Start,
            LastUpdated = DAt10
        });

    // ─── Activities ─────────────────────────────────────────────────────────

    /// <summary>A draft-born type: the author submits it; the assessor completes it, or returns it to draft.</summary>
    public const string DraftWorkflow = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "submitted", "label": "Awaiting discussion" },
            { "key": "discussed", "label": "Discussed", "terminal": true },
            { "key": "cancelled", "label": "Cancelled" }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject|creator", "validation": "owned" },
            { "key": "record_discussion", "from": "submitted", "to": "discussed", "actor": "field:assessor_user_id", "validation": "all" },
            { "key": "return", "from": "submitted", "to": "draft", "actor": "field:assessor_user_id", "requires_note": true, "validation": "draft" },
            { "key": "cancel", "from": ["draft", "submitted"], "to": "cancelled", "actor": "subject|creator", "validation": "draft" }
          ]
        }
        """;

    /// <summary>A type born requested: the create is the submission; the assessor accepts it.</summary>
    public const string RequestedWorkflow = """
        {
          "version": 1,
          "initial_state": "requested",
          "states": [
            { "key": "requested", "label": "Requested" },
            { "key": "accepted", "label": "Accepted" },
            { "key": "completed", "label": "Completed", "terminal": true },
            { "key": "cancelled", "label": "Cancelled" }
          ],
          "transitions": [
            { "key": "accept", "from": "requested", "to": "accepted", "actor": "field:assessor_user_id", "validation": "draft" },
            { "key": "cancel", "from": ["requested", "accepted"], "to": "cancelled", "actor": "subject|field:assessor_user_id", "validation": "draft" },
            { "key": "complete", "from": "accepted", "to": "completed", "actor": "field:assessor_user_id", "validation": "all" }
          ]
        }
        """;

    /// <summary>A type born terminal, as a journal club is: the create is the whole record.</summary>
    public const string LoggedWorkflow = """
        { "version": 1, "initial_state": "logged", "states": [ { "key": "logged", "label": "Logged", "terminal": true } ], "transitions": [] }
        """;

    /// <summary>The MSF record: staff release it from draft by <c>record</c>.</summary>
    public const string MsfWorkflow = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [ { "key": "draft", "label": "Draft" }, { "key": "recorded", "label": "Recorded", "terminal": true } ],
          "transitions": [ { "key": "record", "from": "draft", "to": "recorded", "actor": "role:Coordinator|role:Administrator", "validation": "all" } ]
        }
        """;

    private static void AddActivityTypes(ApplicationDbContext db)
    {
        AddType(db, DraftType, "reflective_exercise_cpsa", "Reflective Exercise (Paediatrics)", DraftWorkflow);
        AddType(db, RequestedType, "mini_cex", "Mini-CEX", RequestedWorkflow);
        AddType(db, LoggedType, "journal_club", "Journal Club", LoggedWorkflow);
        AddType(db, MsfType, "msf_cpsa", "Multi-Source Feedback (Paediatrics)", MsfWorkflow);
    }

    /// <summary>A published global type with this workflow, beside the cast's four (T358, build review R2).</summary>
    public static void AddType(ApplicationDbContext db, int id, string key, string name, string workflowJson)
    {
        var type = new ActivityType
        {
            Id = id, Key = key, Name = name, Scope = ActivityScope.Global, Version = 1, IsActive = true,
            SchemaJson = "{}", WorkflowJson = workflowJson, CreditRulesJson = "{}", DisplayFieldsJson = "[]",
            OwnerUserId = "system", CreatedOn = DAt10.AddYears(-1)
        };
        type.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = id, Version = 1, SchemaJson = "{}", WorkflowJson = workflowJson, CreditRulesJson = "{}",
            DisplayFieldsJson = "[]", PublishedByUserId = "system", PublishedOn = DAt10.AddYears(-1)
        });
        db.ActivityTypes.Add(type);
    }

    private static int _nextActivityId = 1000;

    /// <summary>
    /// An activity about <paramref name="subjectUserId" />, created <paramref name="createdOn" /> by its subject (or
    /// <paramref name="createdBy" />), in <paramref name="state" />, with the moves given after its create row.
    /// </summary>
    public static Activity AddActivity(
        ApplicationDbContext db,
        int typeId,
        string subjectUserId,
        DateTime createdOn,
        string state,
        string? createdBy = null,
        string dataJson = """{"assessor_user_id":"zulu"}""",
        params (string From, string To, string Key, DateTime On)[] moves)
    {
        var initial = typeId switch
        {
            RequestedType => "requested",
            LoggedType => "logged",
            _ => "draft"
        };
        var activity = new Activity
        {
            Id = Interlocked.Increment(ref _nextActivityId),
            ActivityTypeId = typeId,
            SchemaVersion = 1,
            SubjectUserId = subjectUserId,
            CreatedByUserId = createdBy ?? subjectUserId,
            CurrentState = state,
            DataJson = dataJson,
            CreatedOn = createdOn,
            UpdatedOn = moves.Length == 0 ? createdOn : moves[^1].On,
            ObservedOn = DateOnly.FromDateTime(createdOn),
            InstitutionId = Kgk,
            SpecialityId = PaediatricsSpeciality,
            SubSpecialityId = PaediatricsSubSpeciality
        };
        activity.Transitions.Add(new ActivityTransition
        {
            FromState = initial, ToState = initial, TransitionKey = "create", ActorUserId = activity.CreatedByUserId,
            OccurredOn = createdOn
        });
        foreach (var move in moves)
        {
            activity.Transitions.Add(new ActivityTransition
            {
                FromState = move.From, ToState = move.To, TransitionKey = move.Key, ActorUserId = activity.CreatedByUserId,
                OccurredOn = move.On
            });
        }

        db.Activities.Add(activity);
        return activity;
    }

    // ─── Staff ──────────────────────────────────────────────────────────────

    public static ClaimsPrincipal CommitteeMember(string userId = "zulu", int institutionId = Kgk)
        => TestPrincipals.InRole(WombatRoles.CommitteeMember, userId, institutionId);

    public static ClaimsPrincipal Coordinator(string userId = "smit", int institutionId = Kgk)
        => TestPrincipals.InRole(WombatRoles.Coordinator, userId, institutionId);

    public static ClaimsPrincipal SpecialityAdmin(int specialityId = PaediatricsSpeciality, string userId = "mokoena")
        => TestPrincipals.InRole(WombatRoles.SpecialityAdmin, userId, Kgk, specialityId: specialityId);

    public static ClaimsPrincipal SubSpecialityAdmin(int subSpecialityId = PaediatricsSubSpeciality, string userId = "sithole")
        => TestPrincipals.InRole(WombatRoles.SubSpecialityAdmin, userId, Kgk, subSpecialityId: subSpecialityId);

    // ─── Reviews ────────────────────────────────────────────────────────────

    public static CommitteeReview AddReview(
        ApplicationDbContext db, int id, int panelId, string traineeUserId, DateOnly scheduledOn, int semester = 2)
    {
        var review = new CommitteeReview
        {
            Id = id,
            AcademicYear = 2026,
            Semester = semester,
            PanelId = panelId,
            TraineeUserId = traineeUserId,
            ReviewPeriodFrom = new AcademicPeriod(2026, semester).Start,
            ReviewPeriodTo = new AcademicPeriod(2026, semester).NominalEnd,
            ScheduledOn = scheduledOn
        };
        db.CommitteeReviews.Add(review);
        return review;
    }

    public static DecisionPanel AddPanel(IApplicationDbContext db, int id, int institutionId, params string[] memberIds)
    {
        var panel = new DecisionPanel
        {
            Id = id, Name = $"Panel {id}", Scope = DecisionPanelScope.Institution, InstitutionId = institutionId, CreatedOn = DAt10
        };
        foreach (var memberId in memberIds)
        {
            panel.Members.Add(new DecisionPanelMember { UserId = memberId });
        }

        db.Set<DecisionPanel>().Add(panel);
        return panel;
    }
}
