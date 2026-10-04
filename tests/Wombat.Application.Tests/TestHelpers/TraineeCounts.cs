using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.TestHelpers;

/// <summary>
/// The curriculum the count a decision made is read against (T355, C5; E5; <c>EpaCountLines</c>): the cast's on
/// 2026-10-03, Semester 2, 2026. PAED-001 and PAED-002 are semester items (3 each), PAED-008 a yearly one (1), PAED-012 is
/// paused (D48), KGK-001 is the host institution's own and OTH-001 another institution's. A trainee's stored progress rows
/// are added by <see cref="Credit" />, as the credit engine writes them.
/// </summary>
internal static class TraineeCounts
{
    public const int HostInstitution = 1;
    public const int OtherInstitution = 2;
    public const int CurriculumId = 10;

    public const int Paed001 = 1;
    public const int Paed002 = 2;
    public const int Paed008 = 8;
    public const int Paed012 = 12;
    public const int Kgk001 = 30;
    public const int Oth001 = 31;

    /// <summary>An EPA on no curriculum of this fixture.</summary>
    public const int OffCurriculum = 40;

    /// <summary>The day the cast reads on: Semester 2, 2026.</summary>
    public static readonly DateOnly Today = new(2026, 10, 3);

    /// <summary>08:00 SAST on <see cref="Today" />, for a handler that reads its own clock.</summary>
    public static readonly DateTime TodayUtc = new(2026, 10, 3, 6, 0, 0, DateTimeKind.Utc);

    /// <summary>The catalogue and the curriculum, with no trainee. Adds and saves.</summary>
    public static void SeedCurriculum(ApplicationDbContext db)
    {
        db.Institutions.Add(new Institution { Id = HostInstitution, Name = "Kgosi Kgari Teaching Hospital" });
        db.Institutions.Add(new Institution { Id = OtherInstitution, Name = "Elsewhere" });
        db.Specialities.Add(new Speciality { Id = 1, CollegeId = 1, Name = "Paediatrics" });
        db.SubSpecialities.Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "General Paediatrics" });

        var paused = Epa(Paed012, "PAED-012");
        paused.Deactivate(new DateTime(2026, 9, 30, 8, 0, 0, DateTimeKind.Utc));
        db.Epas.AddRange(
            Epa(Paed001, "PAED-001"), Epa(Paed002, "PAED-002"), Epa(Paed008, "PAED-008"), paused,
            Epa(Kgk001, "KGK-001"), Epa(Oth001, "OTH-001"), Epa(OffCurriculum, "PAED-099"));

        db.Curricula.Add(new Curriculum
        {
            Id = CurriculumId, SubSpecialityId = 1, Name = "Paediatric EPA Curriculum", Version = "11.1",
            EffectiveFrom = new DateOnly(2025, 1, 1), IsActive = true
        });
        db.CurriculumItems.AddRange(
            Item(Paed001, QuotaPeriod.Semester, 3),
            Item(Paed002, QuotaPeriod.Semester, 3),
            Item(Paed008, QuotaPeriod.AcademicYear, 1),
            Item(Paed012, QuotaPeriod.Semester, 3),
            Item(Kgk001, QuotaPeriod.Semester, 2, owningInstitutionId: HostInstitution),
            Item(Oth001, QuotaPeriod.Semester, 2, owningInstitutionId: OtherInstitution));

        db.SaveChanges();
    }

    /// <summary>A trainee on the curriculum at the host institution. Adds and saves.</summary>
    public static void SeedTrainee(ApplicationDbContext db, string userId, DateOnly programmeStart, int profileId)
    {
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = profileId, UserId = userId, InstitutionId = HostInstitution, CurriculumId = CurriculumId,
            ProgrammeStartDate = programmeStart, ExpectedCompletionDate = programmeStart.AddYears(4), IsActive = true
        });
        db.SaveChanges();
    }

    /// <summary>A stored progress row for the item of <paramref name="epaId" />, as credit writes it. Adds and saves.</summary>
    public static void Credit(ApplicationDbContext db, string userId, int epaId, int year, int semester, int count)
    {
        db.CurriculumItemProgresses.Add(new CurriculumItemProgress
        {
            CurriculumItemId = 100 + epaId, TraineeUserId = userId, AcademicYear = year, Semester = semester,
            CountsSoFar = count, MinimumLevelReachedCount = count, LastUpdated = TodayUtc
        });
        db.SaveChanges();
    }

    private static Epa Epa(int id, string code)
        => new() { Id = id, SubSpecialityId = 1, Code = code, Title = $"{code} title", IsActive = true };

    private static CurriculumItem Item(int epaId, QuotaPeriod period, int target, int? owningInstitutionId = null)
        => new()
        {
            Id = 100 + epaId, CurriculumId = CurriculumId, EpaId = epaId, RequiredCount = target, QuotaPeriod = period,
            MinimumLevelOrder = 1, WindowMonths = 12, OwningInstitutionId = owningInstitutionId
        };
}
