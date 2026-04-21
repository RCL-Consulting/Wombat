using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.DataRights;
using Wombat.Domain.Activities;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;
using Wombat.Domain.Invitations;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Domain.Reporting;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.DataRights;

public sealed class ErasureExecutor : IErasureExecutor
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<WombatIdentityUser> _userManager;

    public ErasureExecutor(ApplicationDbContext dbContext, UserManager<WombatIdentityUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<DataRightsErasureRecord> ExecuteAsync(
        DataRightsRequest request,
        string pseudonymSalt,
        CancellationToken cancellationToken)
    {
        var userId = request.RequesterUserId;
        var pseudonym = GeneratePseudonym(userId, pseudonymSalt);
        var retentionReasons = new List<string>();
        var utcNow = DateTime.UtcNow;

        // --- Activities: pseudonymise subject and author ---
        var activitiesAsSubject = await _dbContext.Set<Activity>()
            .Where(a => a.SubjectUserId == userId)
            .ToListAsync(cancellationToken);
        foreach (var activity in activitiesAsSubject)
            activity.SubjectUserId = pseudonym;

        var activitiesAsCreator = await _dbContext.Set<Activity>()
            .Where(a => a.CreatedByUserId == userId)
            .ToListAsync(cancellationToken);
        foreach (var activity in activitiesAsCreator)
            activity.CreatedByUserId = pseudonym;

        if (activitiesAsSubject.Count > 0 || activitiesAsCreator.Count > 0)
            retentionReasons.Add("ratified_assessment_record");

        // --- Activity transitions ---
        var transitions = await _dbContext.Set<ActivityTransition>()
            .Where(t => t.ActorUserId == userId)
            .ToListAsync(cancellationToken);
        foreach (var transition in transitions)
            transition.ActorUserId = pseudonym;

        // --- Activity types (owner/staging) ---
        var activityTypes = await _dbContext.Set<ActivityType>()
            .Where(at => at.OwnerUserId == userId || at.StagingUpdatedByUserId == userId)
            .ToListAsync(cancellationToken);
        foreach (var activityType in activityTypes)
        {
            if (activityType.OwnerUserId == userId)
                activityType.OwnerUserId = pseudonym;
            if (activityType.StagingUpdatedByUserId == userId)
                activityType.StagingUpdatedByUserId = pseudonym;
        }

        // --- Activity type versions ---
        var activityTypeVersions = await _dbContext.Set<ActivityTypeVersion>()
            .Where(v => v.PublishedByUserId == userId)
            .ToListAsync(cancellationToken);
        foreach (var version in activityTypeVersions)
            version.PublishedByUserId = pseudonym;

        // --- Committee reviews: pseudonymise trainee and actor references ---
        var reviewsAsTrainee = await _dbContext.Set<CommitteeReview>()
            .Where(r => r.TraineeUserId == userId)
            .ToListAsync(cancellationToken);
        foreach (var review in reviewsAsTrainee)
            review.TraineeUserId = pseudonym;

        if (reviewsAsTrainee.Count > 0)
            retentionReasons.Add("committee_decision");

        // CommitteeReview.StartedByUserId and RatifiedByUserId are private set — use raw SQL
        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE \"CommitteeReviews\" SET \"StartedByUserId\" = {pseudonym} WHERE \"StartedByUserId\" = {userId}",
            cancellationToken);
        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE \"CommitteeReviews\" SET \"RatifiedByUserId\" = {pseudonym} WHERE \"RatifiedByUserId\" = {userId}",
            cancellationToken);

        // --- Committee decisions ---
        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE \"CommitteeDecisions\" SET \"DecidedByChairUserId\" = {pseudonym} WHERE \"DecidedByChairUserId\" = {userId}",
            cancellationToken);

        // --- Committee appeals ---
        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE \"CommitteeAppeals\" SET \"LodgedByUserId\" = {pseudonym} WHERE \"LodgedByUserId\" = {userId}",
            cancellationToken);
        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE \"CommitteeAppeals\" SET \"ResolvedByUserId\" = {pseudonym} WHERE \"ResolvedByUserId\" = {userId}",
            cancellationToken);

        // --- Decision panel members ---
        var panelMembers = await _dbContext.Set<DecisionPanelMember>()
            .Where(m => m.UserId == userId)
            .ToListAsync(cancellationToken);
        foreach (var member in panelMembers)
            member.UserId = pseudonym;

        // --- Entrustment decisions (STAR): pseudonymise trainee, chair, revoker references ---
        // All three columns are private set on the aggregate; use raw SQL.
        var entrustmentCount = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE \"EntrustmentDecisions\" SET \"TraineeUserId\" = {pseudonym} WHERE \"TraineeUserId\" = {userId}",
            cancellationToken);
        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE \"EntrustmentDecisions\" SET \"IssuedByChairUserId\" = {pseudonym} WHERE \"IssuedByChairUserId\" = {userId}",
            cancellationToken);
        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE \"EntrustmentDecisions\" SET \"RevokedByUserId\" = {pseudonym} WHERE \"RevokedByUserId\" = {userId}",
            cancellationToken);

        if (entrustmentCount > 0)
            retentionReasons.Add("entrustment_decision");

        // --- Pending entrustment decisions: chair-staging transient state ---
        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE \"PendingEntrustmentDecisions\" SET \"StagedByUserId\" = {pseudonym} WHERE \"StagedByUserId\" = {userId}",
            cancellationToken);

        // --- MSF campaigns: pseudonymise subject, creator, reviewer ---
        var campaigns = await _dbContext.Set<MsfCampaign>()
            .Where(c => c.SubjectUserId == userId || c.CreatedByUserId == userId || c.ReviewedByUserId == userId)
            .ToListAsync(cancellationToken);
        foreach (var campaign in campaigns)
        {
            if (campaign.SubjectUserId == userId) campaign.SubjectUserId = pseudonym;
            if (campaign.CreatedByUserId == userId) campaign.CreatedByUserId = pseudonym;
            if (campaign.ReviewedByUserId == userId) campaign.ReviewedByUserId = pseudonym;
        }
        // MSF responses are already anonymous — no action needed.

        // --- Curriculum item progress ---
        var progress = await _dbContext.Set<CurriculumItemProgress>()
            .Where(p => p.TraineeUserId == userId)
            .ToListAsync(cancellationToken);
        foreach (var item in progress)
            item.TraineeUserId = pseudonym;

        // --- Portfolio exports ---
        var exports = await _dbContext.Set<PortfolioExport>()
            .Where(e => e.TraineeUserId == userId || e.ExportedByUserId == userId)
            .ToListAsync(cancellationToken);
        foreach (var export in exports)
        {
            if (export.TraineeUserId == userId) export.TraineeUserId = pseudonym;
            if (export.ExportedByUserId == userId) export.ExportedByUserId = pseudonym;
        }

        // --- Trainee profile ---
        var traineeProfile = await _dbContext.Set<TraineeProfile>()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        if (traineeProfile is not null)
            traineeProfile.UserId = pseudonym;

        // --- Assessor profile ---
        var assessorProfile = await _dbContext.Set<AssessorProfile>()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        if (assessorProfile is not null)
            assessorProfile.UserId = pseudonym;

        // --- Invitations (issued by erased user) ---
        var invitations = await _dbContext.Set<Invitation>()
            .Where(i => i.IssuedByUserId == userId)
            .ToListAsync(cancellationToken);
        foreach (var invitation in invitations)
            invitation.IssuedByUserId = pseudonym;

        // --- UserRoleAssignment: remove SSO/manual role tracking records ---
        var roleAssignments = await _dbContext.Set<UserRoleAssignment>()
            .Where(a => a.UserId == userId)
            .ToListAsync(cancellationToken);
        _dbContext.Set<UserRoleAssignment>().RemoveRange(roleAssignments);

        // --- Audit entries: RETAINED unchanged (legitimate interest / legal obligation) ---
        retentionReasons.Add("audit_log");

        // --- Identity user: clear PII, disable login ---
        var identityUser = await _userManager.FindByIdAsync(userId);
        if (identityUser is not null)
        {
            identityUser.UserName = pseudonym;
            identityUser.NormalizedUserName = pseudonym.ToUpperInvariant();
            identityUser.Email = null;
            identityUser.NormalizedEmail = null;
            identityUser.PhoneNumber = null;
            identityUser.PasswordHash = null;
            identityUser.SecurityStamp = Guid.NewGuid().ToString();
            identityUser.ConcurrencyStamp = Guid.NewGuid().ToString();
            identityUser.LockoutEnd = DateTimeOffset.MaxValue;
            identityUser.TwoFactorEnabled = false;
            identityUser.FirstName = string.Empty;
            identityUser.LastName = string.Empty;
            identityUser.OptOutOfOptionalProcessing = true;
            identityUser.OptOutOfDigestEmails = true;
            identityUser.InstitutionId = null;

            await _userManager.UpdateAsync(identityUser);

            // Remove all roles
            var roles = await _userManager.GetRolesAsync(identityUser);
            if (roles.Count > 0)
                await _userManager.RemoveFromRolesAsync(identityUser, roles);

            // Remove institution scope associations
            var specialityScopes = await _dbContext.Set<WombatIdentityUserSpecialityScope>()
                .Where(s => s.UserId == userId)
                .ToListAsync(cancellationToken);
            _dbContext.Set<WombatIdentityUserSpecialityScope>().RemoveRange(specialityScopes);

            var subSpecialityScopes = await _dbContext.Set<WombatIdentityUserSubSpecialityScope>()
                .Where(s => s.UserId == userId)
                .ToListAsync(cancellationToken);
            _dbContext.Set<WombatIdentityUserSubSpecialityScope>().RemoveRange(subSpecialityScopes);
        }

        // --- DataRightsRequest: the request itself references the requester ---
        // We keep the request record for accountability; the requester display name
        // was denormalized at submission time and stays for auditability.

        // --- Create erasure record ---
        var retentionJson = JsonSerializer.Serialize(retentionReasons.Distinct().Order().ToArray());
        var erasureRecord = DataRightsErasureRecord.Create(
            request.Id,
            userId,
            pseudonym,
            utcNow,
            retentionJson);

        _dbContext.Set<DataRightsErasureRecord>().Add(erasureRecord);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return erasureRecord;
    }

    /// <summary>
    /// Generates a stable, deterministic pseudonym: deleted_user_ + first 8 hex chars
    /// of SHA-256(salt + userId). Unlinkable without the salt.
    /// </summary>
    internal static string GeneratePseudonym(string userId, string salt)
    {
        var input = Encoding.UTF8.GetBytes(salt + userId);
        var hash = SHA256.HashData(input);
        var hexPrefix = Convert.ToHexString(hash).Substring(0, 8).ToLowerInvariant();
        return $"deleted_user_{hexPrefix}";
    }
}
