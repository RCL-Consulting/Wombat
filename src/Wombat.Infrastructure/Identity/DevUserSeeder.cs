using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Identity;

/// <summary>
/// Seeds dev-only users so the GUI review (and other local browser verification)
/// can sign in as a Trainee or CommitteeMember without walking the full
/// invitation flow each time. Only invoked from Program.cs when
/// <c>IHostEnvironment.IsDevelopment()</c> is true. Production deployments
/// must never run this — the seed credentials are hardcoded by design.
/// </summary>
public sealed class DevUserSeeder
{
    private const string TraineeEmail = "trainee@wombat.local";
    private const string TraineePassword = "ChangeThisTrainee123!";
    private const string CommitteeMemberEmail = "committee@wombat.local";
    private const string CommitteeMemberPassword = "ChangeThisCommittee123!";

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
        var demoCurriculum = await _dbContext.Curricula
            .Include(curriculum => curriculum.SubSpeciality)
            .ThenInclude(subSpeciality => subSpeciality.Speciality)
            .SingleOrDefaultAsync(
                curriculum => curriculum.Name == "IM Core Curriculum" && curriculum.Version == "2026.1",
                cancellationToken);

        if (demoCurriculum is null)
        {
            _logger.LogWarning(
                "Dev user seed skipped: the Demo curriculum is not present. " +
                "DataSeeder must run before DevUserSeeder.");
            return;
        }

        var subSpecialityId = demoCurriculum.SubSpecialityId;
        var specialityId = demoCurriculum.SubSpeciality.SpecialityId;
        // The curriculum is national now (T091); the dev trainee trains at the seeded DEMO institution.
        var institutionId = await _dbContext.Institutions
            .Where(institution => institution.ShortCode == "DEMO")
            .Select(institution => institution.Id)
            .SingleAsync(cancellationToken);

        await EnsureTraineeAsync(demoCurriculum.Id, institutionId, specialityId, subSpecialityId, cancellationToken);
        await EnsureCommitteeMemberAsync(institutionId, specialityId, subSpecialityId, cancellationToken);
    }

    private async Task EnsureTraineeAsync(int curriculumId, int institutionId, int specialityId, int subSpecialityId, CancellationToken cancellationToken)
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

        await EnsureScopesAsync(existingUser.Id, specialityId, subSpecialityId, cancellationToken);

        var hasProfile = await _dbContext.TraineeProfiles
            .AnyAsync(profile => profile.UserId == existingUser.Id, cancellationToken);

        if (!hasProfile)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            _dbContext.TraineeProfiles.Add(new TraineeProfile
            {
                UserId = existingUser.Id,
                CurriculumId = curriculumId,
                ProgrammeStartDate = today,
                ExpectedCompletionDate = today.AddYears(4),
                IsActive = true
            });
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded TraineeProfile for {Email}.", TraineeEmail);
        }
    }

    private async Task EnsureCommitteeMemberAsync(int institutionId, int specialityId, int subSpecialityId, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(CommitteeMemberEmail);
        if (existingUser is null)
        {
            existingUser = new WombatIdentityUser
            {
                UserName = CommitteeMemberEmail,
                Email = CommitteeMemberEmail,
                EmailConfirmed = true,
                FirstName = "Demo",
                LastName = "Committee",
                InstitutionId = institutionId
            };

            await CreateUserAsync(existingUser, CommitteeMemberPassword, WombatRoles.CommitteeMember);
            _logger.LogInformation("Seeded dev committee member user {Email}.", CommitteeMemberEmail);
        }

        await EnsureScopesAsync(existingUser.Id, specialityId, subSpecialityId, cancellationToken);
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
