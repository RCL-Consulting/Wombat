using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Identity;

/// <summary>
/// Seeds dev-only users so the GUI review (and other local browser verification)
/// can sign in as a Trainee, either of two CommitteeMembers, an Assessor, a Coordinator, an InstitutionalAdmin or a
/// CollegeAdmin without walking the full
/// invitation flow each time. Only invoked from Program.cs when
/// <c>IHostEnvironment.IsDevelopment()</c> is true. Production deployments
/// must never run this — the seed credentials are hardcoded by design.
/// </summary>
/// <remarks>
/// <para>
/// When the national Paediatric EPA catalogue is present, the dev trainee is admitted to it and every
/// dev user is also scoped to Paediatrics, so the CPSA instruments are offered and there is an
/// assessor who can complete them. Before T130 both were hand steps repeated after every rebuild of
/// the dev database, and nothing wrote them down.
/// </para>
/// <para>
/// The trainee's programme starts on 1 January of the year they are seeded, which is a semester boundary.
/// A start of "today" made the dev trainee exempt from every target under the College's D14 rule, so no
/// target could be seen on dev at all.
/// </para>
/// </remarks>
public sealed class DevUserSeeder
{
    private const string TraineeEmail = "trainee@wombat.local";
    private const string TraineePassword = "ChangeThisTrainee123!";
    private const string CommitteeMemberEmail = "committee@wombat.local";
    private const string CommitteeMemberPassword = "ChangeThisCommittee123!";

    // A second committee member, so a dev panel can hold the chair and one other. Since T165 a panel needs at least two
    // members, and a decision two present, so one CommitteeMember could chair no panel that decides anything.
    private const string SecondCommitteeMemberEmail = "committee2@wombat.local";
    private const string SecondCommitteeMemberPassword = "ChangeThisCommittee2123!";
    private const string AssessorEmail = "assessor@wombat.local";
    private const string AssessorPassword = "ChangeThisAssessor123!";

    // A Coordinator schedules committee reviews and runs MSF campaigns. Since T182 no other dev account but the
    // administrator can schedule a review, and verification must not depend on the administrator's credential (T204).
    private const string CoordinatorEmail = "coordinator@wombat.local";
    private const string CoordinatorPassword = "ChangeThisCoordinator123!";

    // A panel's membership is edited by an InstitutionalAdmin (T182). Without one, panel changes cannot be exercised on
    // dev with a seeded account.
    private const string InstitutionalAdminEmail = "instadmin@wombat.local";
    private const string InstitutionalAdminPassword = "ChangeThisInstAdmin123!";

    // A CollegeAdmin authors the national catalogue: sub-specialities, national EPAs and curricula (T093). Without one,
    // the College's side of the curriculum and sub-speciality pages cannot be exercised on dev with a seeded account.
    private const string CollegeAdminEmail = "collegeadmin@wombat.local";
    private const string CollegeAdminPassword = "ChangeThisCollegeAdmin123!";

    private readonly UserManager<WombatIdentityUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DevUserSeeder> _logger;

    public DevUserSeeder(
        UserManager<WombatIdentityUser> userManager,
        ApplicationDbContext dbContext,
        ILogger<DevUserSeeder> logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // The demo curriculum and institution are found by the seed keys DataSeeder gives them (T229), never by a name or
        // short code an administrator can edit. Before T229 the institution was a SingleAsync on ShortCode "DEMO", so
        // changing that short code stopped every later dev startup here.
        var demoCurriculum = await _dbContext.Curricula
            .Include(curriculum => curriculum.SubSpeciality)
            .ThenInclude(subSpeciality => subSpeciality.Speciality)
            .SingleOrDefaultAsync(curriculum => curriculum.SeedKey == DataSeeder.CurriculumSeedKey, cancellationToken);

        if (demoCurriculum is null)
        {
            _logger.LogWarning(
                "Dev user seed skipped: no curriculum carries the demo seed key '{SeedKey}'. " +
                "DataSeeder must run before DevUserSeeder, and announces a demo row it cannot find.",
                DataSeeder.CurriculumSeedKey);
            return;
        }

        // The curriculum is national now (T091); the dev trainee trains at the seeded Demo Institution.
        var institutionId = await _dbContext.Institutions
            .Where(institution => institution.SeedKey == DataSeeder.InstitutionSeedKey)
            .Select(institution => (int?)institution.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (institutionId is null)
        {
            _logger.LogWarning(
                "Dev user seed skipped: no institution carries the demo seed key '{SeedKey}'. " +
                "DataSeeder announces a demo row it cannot find.",
                DataSeeder.InstitutionSeedKey);
            return;
        }

        // By the catalogue's curriculum key (T221), whatever the curriculum is now called. The key carries the catalogue
        // version, which this seeder does not read, so any version's key counts and the newest active one wins.
        var paediatricCurriculum = await _dbContext.Curricula
            .Include(curriculum => curriculum.SubSpeciality)
            .Where(curriculum => curriculum.SeedKey != null
                && curriculum.SeedKey.StartsWith(PaediatricCatalogueSeeder.CurriculumSeedKeyPrefix)
                && curriculum.IsActive)
            .OrderByDescending(curriculum => curriculum.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken);

        var scopes = new List<(int SpecialityId, int SubSpecialityId)>
        {
            (demoCurriculum.SubSpeciality.SpecialityId, demoCurriculum.SubSpecialityId)
        };

        if (paediatricCurriculum is not null)
        {
            scopes.Add((paediatricCurriculum.SubSpeciality.SpecialityId, paediatricCurriculum.SubSpecialityId));
        }

        var traineeCurriculumId = paediatricCurriculum?.Id ?? demoCurriculum.Id;

        await EnsureTraineeAsync(traineeCurriculumId, institutionId.Value, scopes, cancellationToken);
        await EnsureStaffUserAsync(CommitteeMemberEmail, CommitteeMemberPassword, "Committee", WombatRoles.CommitteeMember, institutionId.Value, scopes, cancellationToken);
        await EnsureStaffUserAsync(SecondCommitteeMemberEmail, SecondCommitteeMemberPassword, "Committee Two", WombatRoles.CommitteeMember, institutionId.Value, scopes, cancellationToken);
        await EnsureStaffUserAsync(AssessorEmail, AssessorPassword, "Assessor", WombatRoles.Assessor, institutionId.Value, scopes, cancellationToken);
        await EnsureStaffUserAsync(CoordinatorEmail, CoordinatorPassword, "Coordinator", WombatRoles.Coordinator, institutionId.Value, scopes, cancellationToken);
        await EnsureStaffUserAsync(InstitutionalAdminEmail, InstitutionalAdminPassword, "Institutional Admin", WombatRoles.InstitutionalAdmin, institutionId.Value, scopes, cancellationToken);
        await EnsureCollegeAdminAsync(cancellationToken);
    }

    private async Task EnsureCollegeAdminAsync(CancellationToken cancellationToken)
    {
        // By the catalogue's seed key (T221), not its short code, which an administrator can edit.
        var collegeId = await _dbContext.Colleges
            .Where(college => college.SeedKey == PaediatricCatalogueSeeder.CollegeSeedKey)
            .Select(college => (int?)college.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (collegeId is null || await _userManager.FindByEmailAsync(CollegeAdminEmail) is not null)
        {
            return;
        }

        // Scoped to its College, not to an institution, as an invited CollegeAdmin is.
        var user = new WombatIdentityUser
        {
            UserName = CollegeAdminEmail,
            Email = CollegeAdminEmail,
            EmailConfirmed = true,
            FirstName = "Demo",
            LastName = "College Admin",
            CollegeId = collegeId
        };

        await CreateUserAsync(user, CollegeAdminPassword, WombatRoles.CollegeAdmin);
        _logger.LogInformation("Seeded dev {Role} user {Email}.", WombatRoles.CollegeAdmin, CollegeAdminEmail);
    }

    private async Task EnsureTraineeAsync(
        int curriculumId,
        int institutionId,
        IReadOnlyList<(int SpecialityId, int SubSpecialityId)> scopes,
        CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(TraineeEmail);
        if (existingUser is null)
        {
            existingUser = new WombatIdentityUser
            {
                UserName = TraineeEmail,
                Email = TraineeEmail,
                EmailConfirmed = true,
                FirstName = "Demo",
                LastName = "Trainee",
                InstitutionId = institutionId
            };

            await CreateUserAsync(existingUser, TraineePassword, WombatRoles.Trainee);
            _logger.LogInformation("Seeded dev trainee user {Email}.", TraineeEmail);
        }

        foreach (var (specialityId, subSpecialityId) in scopes)
        {
            await EnsureScopesAsync(existingUser.Id, specialityId, subSpecialityId, cancellationToken);
        }

        var hasProfile = await _dbContext.TraineeProfiles
            .AnyAsync(profile => profile.UserId == existingUser.Id, cancellationToken);

        if (!hasProfile)
        {
            // A semester boundary, so the College's D14 exemption for a mid-period start does not apply
            // and the dev trainee has targets to look at (T130).
            var programmeStart = new DateOnly(DateTime.UtcNow.Year, 1, 1);
            _dbContext.TraineeProfiles.Add(new TraineeProfile
            {
                UserId = existingUser.Id,
                InstitutionId = institutionId,
                CurriculumId = curriculumId,
                ProgrammeStartDate = programmeStart,
                ExpectedCompletionDate = programmeStart.AddYears(4),
                IsActive = true
                // AdoptionId left null: this dev seed bypasses the AdmitTrainee adoption gate (T091); the
                // CreditApplier scopes by CurriculumId + InstitutionId, so credit still resolves correctly.
            });
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded TraineeProfile for {Email}.", TraineeEmail);
        }
    }

    private async Task EnsureStaffUserAsync(
        string email,
        string password,
        string lastName,
        string role,
        int institutionId,
        IReadOnlyList<(int SpecialityId, int SubSpecialityId)> scopes,
        CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser is null)
        {
            existingUser = new WombatIdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = "Demo",
                LastName = lastName,
                InstitutionId = institutionId
            };

            await CreateUserAsync(existingUser, password, role);
            _logger.LogInformation("Seeded dev {Role} user {Email}.", role, email);
        }

        foreach (var (specialityId, subSpecialityId) in scopes)
        {
            await EnsureScopesAsync(existingUser.Id, specialityId, subSpecialityId, cancellationToken);
        }
    }

    private async Task EnsureScopesAsync(string userId, int specialityId, int subSpecialityId, CancellationToken cancellationToken)
    {
        var hasSpeciality = await _dbContext.UserSpecialityScopes
            .AnyAsync(scope => scope.UserId == userId && scope.SpecialityId == specialityId, cancellationToken);

        if (!hasSpeciality)
        {
            _dbContext.UserSpecialityScopes.Add(new WombatIdentityUserSpecialityScope
            {
                UserId = userId,
                SpecialityId = specialityId
            });
        }

        var hasSubSpeciality = await _dbContext.UserSubSpecialityScopes
            .AnyAsync(scope => scope.UserId == userId && scope.SubSpecialityId == subSpecialityId, cancellationToken);

        if (!hasSubSpeciality)
        {
            _dbContext.UserSubSpecialityScopes.Add(new WombatIdentityUserSubSpecialityScope
            {
                UserId = userId,
                SubSpecialityId = subSpecialityId
            });
        }

        if (!hasSpeciality || !hasSubSpeciality)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task CreateUserAsync(WombatIdentityUser user, string password, string role)
    {
        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Failed to create dev user '{user.Email}': {errors}");
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join("; ", roleResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Failed to assign role '{role}' to dev user '{user.Email}': {errors}");
        }
    }
}
