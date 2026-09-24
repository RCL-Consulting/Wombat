using System.Security.Claims;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Queries;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Common.Security;

/// <summary>
/// Who stands over a trainee is one rule, <see cref="TraineeScopeResolver" />, and its query forms must say what it
/// says, caller by caller. (T185)
/// </summary>
/// <remarks>
/// <para>
/// The overseer rule had three shapes: <c>ActivityService.IsScopedOverseerOf</c>, <see cref="ActivityReadScope.WhereReadableBy" />
/// and <see cref="TraineeScopeResolver.IsOverseenBy" />, each maintained by hand to agree with the others. The first
/// now calls the third. The second is its SQL form over an activity's stamps, and a list cannot call a predicate per
/// row, so it stays written apart, and is held to the rule here. So is
/// <see cref="TraineeScopeResolver.AdministeredProfiles" />, the SQL form of <see cref="TraineeScopeResolver.IsAdministeredBy" />
/// that the entrustment admin list narrows by.
/// </para>
/// <para>
/// The callers are every role at the right institution and the wrong one, of the right speciality and the wrong one,
/// holding two roles at once, holding no institution, and holding nothing. The stamps and profiles cover every level a
/// scope can lose. The guards at the end of each test keep it from passing by admitting nobody.
/// </para>
/// </remarks>
public sealed class OverseerRuleParityTests
{
    private const int Host = 1;
    private const int Other = 2;
    private const int Paediatrics = 1;
    private const int Surgery = 2;
    private const int GeneralPaediatrics = 11;
    private const int GeneralSurgery = 21;

    /// <summary>A sub-speciality whose speciality row is missing.</summary>
    private const int OrphanedSubSpeciality = 12;
    private const int MissingSpeciality = 404;

    /// <summary>A sub-speciality id a curriculum names and no row holds.</summary>
    private const int MissingSubSpeciality = 505;

    public static TheoryData<string> Callers()
    {
        var data = new TheoryData<string>();
        foreach (var caller in CallerNames)
        {
            data.Add(caller);
        }

        return data;
    }

    // ─── The activity read boundary ──────────────────────────────────────────

    [Theory]
    [MemberData(nameof(Callers))]
    public void TheActivityListFilter_AdmitsExactlyTheActivitiesWhoseStampedScopeTheCallerOversees(string caller)
    {
        // Nobody here is the subject, and nobody is a global Administrator: those two rungs of WhereReadableBy are not
        // oversight, and IsOverseenBy has neither.
        var principal = Caller(caller);
        var activities = Stamps();

        var listed = activities.AsQueryable().WhereReadableBy(principal).Select(activity => activity.Id).ToList();
        var overseen = activities
            .Where(activity => activity.InstitutionId is int institutionId &&
                               TraineeScopeResolver.IsOverseenBy(
                                   new TraineeScope(institutionId, activity.SpecialityId, activity.SubSpecialityId),
                                   principal))
            .Select(activity => activity.Id)
            .ToList();

        listed.Should().BeEquivalentTo(overseen, caller);
    }

    [Fact]
    public void Guard_TheActivityMatrixAdmitsSomeAndRefusesSome()
    {
        var admitted = CallerNames
            .Sum(caller => Stamps().AsQueryable().WhereReadableBy(Caller(caller)).Count());

        admitted.Should().BeInRange(1, (CallerNames.Length * Stamps().Count) - 1);
    }

    // ─── The admin list ──────────────────────────────────────────────────────

    [Theory]
    [MemberData(nameof(Callers))]
    public async Task AdministeredProfiles_HoldsExactlyTheTraineesIsAdministeredByAdmits(string caller)
    {
        await using var db = CreateDb();
        SeedProfiles(db);
        await db.SaveChangesAsync();
        var principal = Caller(caller);

        var queried = await TraineeScopeResolver.AdministeredProfiles(db, principal)
            .Select(profile => profile.UserId)
            .ToListAsync();

        var scopes = await TraineeScopeResolver.ResolveAllAsync(db, null, CancellationToken.None);
        var judged = scopes
            .Where(entry => TraineeScopeResolver.IsAdministeredBy(entry.Value, principal))
            .Select(entry => entry.Key)
            .ToList();

        queried.Should().BeEquivalentTo(judged, caller);
    }

    [Fact]
    public async Task Guard_TheProfileMatrixAdmitsEveryLevelSomewhere()
    {
        await using var db = CreateDb();
        SeedProfiles(db);
        await db.SaveChangesAsync();

        using var scope = new AssertionScope();
        (await AdministeredBy("an InstitutionalAdmin at the host")).Should().BeEquivalentTo(
            ["paeds", "surgery", "no-curriculum", "orphaned-sub-speciality", "missing-sub-speciality"],
            "the moved trainee's preferred profile is elsewhere, and so is the other trainee");
        (await AdministeredBy("a SpecialityAdmin of paediatrics at the host")).Should().BeEquivalentTo(["paeds"]);
        (await AdministeredBy("a SubSpecialityAdmin of general surgery at the host")).Should().BeEquivalentTo(["surgery"]);
        (await AdministeredBy("a SpecialityAdmin of the missing speciality at the host")).Should().BeEquivalentTo(["orphaned-sub-speciality"]);
        (await AdministeredBy("a SubSpecialityAdmin of the missing sub-speciality at the host")).Should().BeEquivalentTo(["missing-sub-speciality"]);
        (await AdministeredBy("an InstitutionalAdmin at the other institution")).Should().BeEquivalentTo(["elsewhere", "moved"]);

        async Task<List<string>> AdministeredBy(string caller)
            => await TraineeScopeResolver.AdministeredProfiles(db, Caller(caller)).Select(profile => profile.UserId).ToListAsync();
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private static readonly string[] CallerNames =
    [
        "an InstitutionalAdmin at the host",
        "a Coordinator at the host",
        "a CommitteeMember at the host",
        "a SpecialityAdmin of paediatrics at the host",
        "a SubSpecialityAdmin of general paediatrics at the host",
        "a SpecialityAdmin of surgery at the host",
        "a SubSpecialityAdmin of general surgery at the host",
        "a SpecialityAdmin of the missing speciality at the host",
        "a SubSpecialityAdmin of the missing sub-speciality at the host",
        "an InstitutionalAdmin at the other institution",
        "a Coordinator at the other institution",
        "a CommitteeMember at the other institution",
        "a SpecialityAdmin of paediatrics at the other institution",
        "a SubSpecialityAdmin of general paediatrics at the other institution",
        "a SpecialityAdmin of surgery at the host who also sits on its committee",
        "a SubSpecialityAdmin of general surgery at the host who also coordinates there",
        "a SpecialityAdmin of paediatrics with no institution",
        "an InstitutionalAdmin with no institution",
        "a Coordinator with no institution",
        "a Trainee at the host",
        "an Assessor at the host",
        "a principal with no claims at all"
    ];

    private static ClaimsPrincipal Caller(string caller) => caller switch
    {
        "an InstitutionalAdmin at the host" => TestPrincipals.InstitutionalAdmin(Host),
        "a Coordinator at the host" => TestPrincipals.Coordinator(Host),
        "a CommitteeMember at the host" => TestPrincipals.InRole(WombatRoles.CommitteeMember, "member-1", Host),
        "a SpecialityAdmin of paediatrics at the host" =>
            TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "spec-1", Host, specialityId: Paediatrics),
        "a SubSpecialityAdmin of general paediatrics at the host" =>
            TestPrincipals.InRole(WombatRoles.SubSpecialityAdmin, "sub-1", Host, subSpecialityId: GeneralPaediatrics),
        "a SpecialityAdmin of surgery at the host" =>
            TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "spec-2", Host, specialityId: Surgery),
        "a SubSpecialityAdmin of general surgery at the host" =>
            TestPrincipals.InRole(WombatRoles.SubSpecialityAdmin, "sub-2", Host, subSpecialityId: GeneralSurgery),
        "a SpecialityAdmin of the missing speciality at the host" =>
            TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "spec-3", Host, specialityId: MissingSpeciality),
        "a SubSpecialityAdmin of the missing sub-speciality at the host" =>
            TestPrincipals.InRole(WombatRoles.SubSpecialityAdmin, "sub-3", Host, subSpecialityId: MissingSubSpeciality),
        "an InstitutionalAdmin at the other institution" => TestPrincipals.InstitutionalAdmin(Other),
        "a Coordinator at the other institution" => TestPrincipals.Coordinator(Other),
        "a CommitteeMember at the other institution" => TestPrincipals.InRole(WombatRoles.CommitteeMember, "member-2", Other),
        "a SpecialityAdmin of paediatrics at the other institution" =>
            TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "spec-4", Other, specialityId: Paediatrics),
        "a SubSpecialityAdmin of general paediatrics at the other institution" =>
            TestPrincipals.InRole(WombatRoles.SubSpecialityAdmin, "sub-4", Other, subSpecialityId: GeneralPaediatrics),
        "a SpecialityAdmin of surgery at the host who also sits on its committee" =>
            TestPrincipals.InRoles([WombatRoles.SpecialityAdmin, WombatRoles.CommitteeMember], "spec-5", Host, specialityId: Surgery),
        "a SubSpecialityAdmin of general surgery at the host who also coordinates there" =>
            TestPrincipals.InRoles([WombatRoles.SubSpecialityAdmin, WombatRoles.Coordinator], "sub-5", Host, subSpecialityId: GeneralSurgery),
        "a SpecialityAdmin of paediatrics with no institution" =>
            TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "spec-6", institutionId: null, specialityId: Paediatrics),
        "an InstitutionalAdmin with no institution" =>
            TestPrincipals.InRole(WombatRoles.InstitutionalAdmin, "inst-3", institutionId: null),
        "a Coordinator with no institution" => TestPrincipals.InRole(WombatRoles.Coordinator, "coord-3", institutionId: null),
        "a Trainee at the host" => TestPrincipals.Trainee("trainee-2", Host),
        "an Assessor at the host" => TestPrincipals.InRole(WombatRoles.Assessor, "assessor-1", Host),
        "a principal with no claims at all" => TestPrincipals.Anonymous(),
        _ => throw new ArgumentOutOfRangeException(nameof(caller), caller, null)
    };

    /// <summary>One activity per shape of stamp, each about a subject none of the callers is.</summary>
    private static List<Activity> Stamps()
    {
        var stamps = new (int? Institution, int? Speciality, int? SubSpeciality)[]
        {
            (Host, Paediatrics, GeneralPaediatrics),
            (Host, Surgery, GeneralSurgery),
            (Other, Paediatrics, GeneralPaediatrics),
            (Host, Paediatrics, null),
            (Host, null, GeneralPaediatrics),
            (Host, null, null),
            (null, Paediatrics, GeneralPaediatrics),
            (null, null, null)
        };

        return stamps
            .Select((stamp, index) => new Activity
            {
                Id = index + 1,
                SubjectUserId = $"subject-{index + 1}",
                CreatedByUserId = $"subject-{index + 1}",
                InstitutionId = stamp.Institution,
                SpecialityId = stamp.Speciality,
                SubSpecialityId = stamp.SubSpeciality
            })
            .ToList();
    }

    /// <summary>
    /// Trainees whose preferred profiles reach every level of scope, lose each level in turn, sit at the other
    /// institution, or moved there: an older past profile at the host, a newer one elsewhere.
    /// </summary>
    private static void SeedProfiles(ApplicationDbContext db)
    {
        db.Specialities.Add(new Speciality { Id = Paediatrics, CollegeId = 1, Name = "Paediatrics" });
        db.Specialities.Add(new Speciality { Id = Surgery, CollegeId = 1, Name = "Surgery" });
        db.SubSpecialities.Add(new SubSpeciality { Id = GeneralPaediatrics, SpecialityId = Paediatrics, Name = "General Paediatrics" });
        db.SubSpecialities.Add(new SubSpeciality { Id = GeneralSurgery, SpecialityId = Surgery, Name = "General Surgery" });
        db.SubSpecialities.Add(new SubSpeciality { Id = OrphanedSubSpeciality, SpecialityId = MissingSpeciality, Name = "Orphaned" });
        db.Curricula.Add(new Curriculum { Id = 100, SubSpecialityId = GeneralPaediatrics, Name = "Paediatrics", Version = "1" });
        db.Curricula.Add(new Curriculum { Id = 200, SubSpecialityId = GeneralSurgery, Name = "Surgery", Version = "1" });
        db.Curricula.Add(new Curriculum { Id = 101, SubSpecialityId = OrphanedSubSpeciality, Name = "Orphaned", Version = "1" });
        db.Curricula.Add(new Curriculum { Id = 102, SubSpecialityId = MissingSubSpeciality, Name = "Dangling", Version = "1" });

        AddProfile(db, 1, "paeds", Host, curriculumId: 100, isActive: true);
        AddProfile(db, 2, "surgery", Host, curriculumId: 200, isActive: true);
        AddProfile(db, 3, "no-curriculum", Host, curriculumId: 999, isActive: true);
        AddProfile(db, 4, "orphaned-sub-speciality", Host, curriculumId: 101, isActive: true);
        AddProfile(db, 5, "missing-sub-speciality", Host, curriculumId: 102, isActive: true);
        AddProfile(db, 6, "elsewhere", Other, curriculumId: 100, isActive: true);
        AddProfile(db, 7, "moved", Host, curriculumId: 100, isActive: false);
        AddProfile(db, 8, "moved", Other, curriculumId: 100, isActive: false);
    }

    private static void AddProfile(ApplicationDbContext db, int id, string userId, int institutionId, int curriculumId, bool isActive)
        => db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = id,
            UserId = userId,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            ProgrammeStartDate = new DateOnly(2024, 1, 1),
            ExpectedCompletionDate = new DateOnly(2028, 1, 1),
            IsActive = isActive
        });

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
