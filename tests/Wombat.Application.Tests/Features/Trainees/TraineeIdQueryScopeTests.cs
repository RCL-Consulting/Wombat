using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.Trainees;

/// <summary>
/// Five queries that answered on a caller-supplied trainee id alone now answer only the trainee, an Administrator and
/// the trainee's in-scope overseers, and give everyone else what they would give for a trainee with nothing on record.
/// (T113)
/// </summary>
/// <remarks>
/// One matrix, every query against every caller, so a query that drifts from the others fails here by name. Each query
/// has something to return about "trainee-1" - curriculum progress, a ratified review, an active decision, the history
/// of that decision's EPA, a released feedback campaign - so "nothing" can only mean "refused".
/// </remarks>
public sealed class TraineeIdQueryScopeTests
{
    private const string TraineeUserId = "trainee-1";
    private const int HostInstitution = 1;
    private const int OtherInstitution = 2;
    private const int Paediatrics = 1;
    private const int EpaId = 1;

    public static TheoryData<string, string, bool> Matrix()
    {
        var data = new TheoryData<string, string, bool>();
        foreach (var query in Queries.Keys)
        {
            foreach (var (caller, sees) in Callers)
            {
                data.Add(query, caller, sees);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Matrix))]
    public async Task EachQuery_AnswersOnlyTheTraineeAnAdministratorAndAnInScopeOverseer(string query, string caller, bool sees)
    {
        await using var db = CreateDb();
        await SeedAsync(db);

        var visible = await Queries[query](db, Principal(caller));

        visible.Should().Be(sees, $"{query} asked by {caller}");
    }

    private static readonly (string Caller, bool Sees)[] Callers =
    [
        ("the trainee themselves", true),
        ("a global Administrator", true),
        ("a Coordinator at the trainee's institution", true),
        ("a SpecialityAdmin of the trainee's speciality at their institution", true),
        ("a Coordinator at another institution", false),
        ("a SpecialityAdmin of the trainee's speciality at another institution", false),
        ("a classmate at the same institution", false)
    ];

    /// <summary>Each query, reduced to "did it hand back anything about trainee-1?".</summary>
    private static readonly Dictionary<string, Func<ApplicationDbContext, ClaimsPrincipal, Task<bool>>> Queries = new()
    {
        ["GetCurriculumProgressForTrainee"] = async (db, principal) =>
            await new GetCurriculumProgressForTraineeQueryHandler(db).Handle(
                new GetCurriculumProgressForTraineeQuery(TraineeUserId, principal, new DateOnly(2026, 9, 23)),
                CancellationToken.None) is not null,

        ["ListReviewsForTrainee"] = async (db, principal) =>
            (await new ListReviewsForTraineeQueryHandler(db).Handle(
                new ListReviewsForTraineeQuery(TraineeUserId, principal), CancellationToken.None)).Count > 0,

        ["GetActiveDecisionsForTrainee"] = async (db, principal) =>
            (await new GetActiveDecisionsForTraineeQueryHandler(db).Handle(
                new GetActiveDecisionsForTraineeQuery(TraineeUserId, principal), CancellationToken.None)).Count > 0,

        ["GetDecisionHistoryForEpa"] = async (db, principal) =>
            (await new GetDecisionHistoryForEpaQueryHandler(db).Handle(
                new GetDecisionHistoryForEpaQuery(TraineeUserId, EpaId, principal), CancellationToken.None)).Count > 0,

        ["ListMsfCampaignsForTrainee"] = async (db, principal) =>
            (await new ListMsfCampaignsForTraineeQueryHandler(db).Handle(
                new ListMsfCampaignsForTraineeQuery(TraineeUserId, principal), CancellationToken.None)).Count > 0,

        // T166: the standing against Annexure A, which carries the active decision seeded below.
        ["GetEntrustmentStandingForTrainee"] = async (db, principal) =>
            await new GetEntrustmentStandingForTraineeQueryHandler(db).Handle(
                new GetEntrustmentStandingForTraineeQuery(TraineeUserId, principal, new DateOnly(2026, 9, 23)),
                CancellationToken.None) is not null
    };

    private static ClaimsPrincipal Principal(string caller) => caller switch
    {
        "the trainee themselves" => TestPrincipals.Trainee(TraineeUserId, HostInstitution),
        "a global Administrator" => TestPrincipals.Administrator(),
        "a Coordinator at the trainee's institution" => TestPrincipals.Coordinator(HostInstitution),
        "a SpecialityAdmin of the trainee's speciality at their institution" =>
            TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "spec-1", HostInstitution, specialityId: Paediatrics),
        "a Coordinator at another institution" => TestPrincipals.Coordinator(OtherInstitution),
        // Speciality ids are national: holding the right one at the wrong hospital is not oversight.
        "a SpecialityAdmin of the trainee's speciality at another institution" =>
            TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "spec-2", OtherInstitution, specialityId: Paediatrics),
        // Every user carries an institution claim; a matching one must not widen a read on its own.
        "a classmate at the same institution" => TestPrincipals.Trainee("trainee-2", HostInstitution),
        _ => throw new ArgumentOutOfRangeException(nameof(caller), caller, null)
    };

    private static async Task SeedAsync(ApplicationDbContext db)
    {
        db.Institutions.Add(new Institution { Id = HostInstitution, Name = "Host" });
        db.Institutions.Add(new Institution { Id = OtherInstitution, Name = "Elsewhere" });
        db.Specialities.Add(new Speciality { Id = Paediatrics, CollegeId = 1, Name = "Paediatrics" });
        db.SubSpecialities.Add(new SubSpeciality { Id = 1, SpecialityId = Paediatrics, Name = "General Paediatrics" });
        db.Epas.Add(new Epa { Id = EpaId, SubSpecialityId = 1, Code = "PAED-001", Title = "Clerk an acute admission", IsActive = true });
        db.Set<EntrustmentLevel>().Add(new EntrustmentLevel { Id = 3, ScaleId = 1, Order = 3, Label = "3a" });
        db.Curricula.Add(new Curriculum
        {
            Id = 1, SubSpecialityId = 1, Name = "FCPaed", Version = "11.1",
            EffectiveFrom = new DateOnly(2025, 1, 1), IsActive = true
        });
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 1, CurriculumId = 1, EpaId = EpaId, RequiredCount = 3, QuotaPeriod = QuotaPeriod.Semester,
            MinimumLevelOrder = 3, WindowMonths = 36
        });
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1, UserId = TraineeUserId, InstitutionId = HostInstitution, CurriculumId = 1,
            ProgrammeStartDate = new DateOnly(2024, 1, 1), ExpectedCompletionDate = new DateOnly(2028, 1, 1),
            IsActive = true
        });

        var panel = new DecisionPanel
        {
            Id = 20, Name = "Paediatrics CCC", Scope = DecisionPanelScope.Institution,
            InstitutionId = HostInstitution, CreatedOn = DateTime.UtcNow
        };
        db.DecisionPanels.Add(panel);

        var review = new CommitteeReview
        {
            Id = 30, PanelId = panel.Id, TraineeUserId = TraineeUserId,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1), ReviewPeriodTo = new DateOnly(2026, 6, 30),
            ScheduledOn = new DateOnly(2026, 7, 1)
        };
        db.CommitteeReviews.Add(review);

        // The state a trainee may see. Set through the change tracker: the domain gets there only by conducting the
        // review, which is not what this test is about.
        db.Entry(review).Property(entity => entity.State).CurrentValue = CommitteeReviewState.Ratified;

        db.EntrustmentDecisions.Add(EntrustmentDecision.Issue(
            TraineeUserId, EpaId, authorisedLevelId: 3, new DateOnly(2026, 7, 1), expiresOn: null,
            committeeReviewId: review.Id, "chair-1", "Consistent across the period.", StarEvidence.One()));

        db.MsfCampaigns.Add(new MsfCampaign
        {
            SubjectUserId = TraineeUserId,
            CreatedByUserId = "coordinator-user",
            CreatedOn = DateTime.UtcNow,
            OpensOn = new DateOnly(2026, 5, 1),
            ClosesOn = new DateOnly(2026, 5, 31),
            State = MsfCampaignState.Released,
            ReleasedOn = DateTime.UtcNow,
            Template = new MsfTemplate { Name = "Annual MSF" }
        });

        await db.SaveChangesAsync();
    }

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
