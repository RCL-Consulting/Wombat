using Microsoft.EntityFrameworkCore;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.TestSupport;

/// <summary>
/// Gives a user the profile half of a current trainee in a bare migrated schema: a programme to be on (a college, a
/// speciality, a sub-speciality and a curriculum), an institution, and an active profile there. (T284)
/// </summary>
/// <remarks>
/// Opening a draft campaign, and inviting to one, ask for a current trainee (<c>MsfCampaignRules.EnsureCampaignTakesNewWorkAsync</c>,
/// T284), as creating one does (T238). The account half is the directory's: a fake one's <c>WithTrainees</c>, or a
/// Trainee role in the real store. A user who already holds an active profile is left as they are.
/// </remarks>
internal static class CurrentTraineeSeed
{
    public static async Task AdmitAsync(ApplicationDbContext db, string userId)
    {
        if (await db.TraineeProfiles.AnyAsync(profile => profile.UserId == userId && profile.IsActive))
        {
            return;
        }

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var curriculum = new Curriculum
        {
            Name = "T284 programme",
            Version = "1",
            EffectiveFrom = new DateOnly(2025, 1, 1),
            SubSpeciality = new SubSpeciality
            {
                Name = "General Paediatrics",
                Speciality = new Speciality
                {
                    Name = "Paediatrics",
                    College = new College { Name = $"T284 College {suffix}", ShortCode = $"T284C-{suffix}" }
                }
            }
        };
        var institution = new Institution { Name = $"T284 Hospital {suffix}", ShortCode = $"T284H-{suffix}" };

        db.Curricula.Add(curriculum);
        db.Institutions.Add(institution);
        await db.SaveChangesAsync();

        db.TraineeProfiles.Add(new TraineeProfile
        {
            UserId = userId,
            InstitutionId = institution.Id,
            CurriculumId = curriculum.Id,
            ProgrammeStartDate = new DateOnly(2025, 1, 1),
            ExpectedCompletionDate = new DateOnly(2029, 1, 1),
            IsActive = true
        });
        await db.SaveChangesAsync();
    }
}
