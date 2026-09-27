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

/// <summary>
/// Erases a data subject: every reference to them is moved to a pseudonym no account holds, and their account keeps no
/// name, address, password, role or login (T026).
/// </summary>
/// <remarks>
/// <para>
/// What was open about the person is ended, and what was settled is kept under the pseudonym (T258). Ended: every trainee
/// profile, deactivated on the erasure day; every committee review still open (<see cref="CommitteeReview.OpenStates" />),
/// withdrawn with its reason, and the decisions staged at it removed, since only ratifying issues one; and every multi-source
/// feedback campaign about them not yet released, withdrawn, which anonymises its invitations. Kept: ratified reviews and
/// their appeals, issued STARs, released feedback reports, activities and progress. Until T258 only the first profile moved,
/// it stayed active, and the open reviews and campaigns ran on under the pseudonym, so a panel could still ratify a review
/// and a release could still write activities about someone who had left.
/// </para>
/// <para>
/// All of it or none of it: the work runs in one transaction, and a failure leaves nothing tracked, since the audit
/// pipeline writes its failure row through this same context and would otherwise commit what was still tracked, the
/// request's approval with it. So a refused erasure leaves the request as it was, to be approved again. The request's
/// approval and completion are written inside this transaction too, with everything else: the approving handler
/// marks both before it calls this, so no erasure stands under a request left merely approved.
/// </para>
/// <para>
/// A row about the person that changed between being read here and the save (a review ratified, a campaign closed by
/// the auto-close job, the account updated) fails the save on its concurrency token. That is refused as
/// <see cref="PersonChanged" />, carrying EF's exception inside, rather than as EF's own message about row counts. The
/// account's conflict arrives differently: Identity's user store catches EF's exception and returns a failed result.
/// Every Identity write's result is checked (T156): a conflict is refused as <see cref="PersonChanged" /> too, and any
/// other refusal fails the erasure naming the step, so no erased account keeps a role or an institutional sign-in.
/// </para>
/// </remarks>
public sealed class ErasureExecutor : IErasureExecutor
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<WombatIdentityUser> _userManager;
    private readonly TimeProvider _timeProvider;

    public ErasureExecutor(
        ApplicationDbContext dbContext,
        UserManager<WombatIdentityUser> userManager,
        TimeProvider? timeProvider = null)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<DataRightsErasureRecord> ExecuteAsync(
        DataRightsRequest request,
        string pseudonymSalt,
        CancellationToken cancellationToken)
    {
        // Raw UPDATEs run beside the tracked changes, and UserManager saves this context several times on the way, so
        // without a transaction a failure part-way would leave the person half erased.
        await using var transaction = _dbContext.Database.CurrentTransaction is null
            ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
            : null;

        try
        {
            var record = await EraseAsync(request, pseudonymSalt, cancellationToken);
            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            return record;
        }
        catch (DbUpdateConcurrencyException exception)
        {
            // As below, and said in words: EF's message names row counts. Carried inside, so the audit pipeline still sees
            // a refused save (T201).
            _dbContext.ChangeTracker.Clear();
            throw new InvalidOperationException(PersonChanged, exception);
        }
        catch
        {
            // The transaction rolls back when it is disposed. What is still tracked goes too: the audit pipeline saves this
            // context from its catch (the audit trap), and would commit it, and the request's approval, without the rest.
            _dbContext.ChangeTracker.Clear();
            throw;
        }
    }

    /// <summary>
    /// The refusal when something about the person changed while they were being erased: nothing is erased, and the
    /// request stays as it was, to be approved again (T258 review).
    /// </summary>
    public const string PersonChanged =
        "Something about this person changed while they were being erased: a review, a feedback campaign or their account " +
        "was changed elsewhere at the same moment. Nothing has been erased, and the request has not been approved. " +
        "Approve it again.";

    private async Task<DataRightsErasureRecord> EraseAsync(
        DataRightsRequest request,
        string pseudonymSalt,
        CancellationToken cancellationToken)
    {
        var userId = request.RequesterUserId;
        var pseudonym = GeneratePseudonym(userId, pseudonymSalt);
        var retentionReasons = new List<string>();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        // The erasure day on the South African calendar: the last day of every profile it ends (T258, D49).
        var erasedOn = ProgrammeCalendar.DateOf(utcNow);

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
        // CommitteeReview.StartedByUserId and RatifiedByUserId are private set — use raw SQL.
        //
        // Before the tracked load below, deliberately: a review carries an xmin concurrency token (T131), and a raw
        // UPDATE of a row already loaded would move its xmin under the tracker, so the save at the end would find the
        // review "changed" and refuse the whole erasure. That happens whenever the person erased is both a review's
        // trainee and the one who started or ratified it. Loaded after, the tracker holds the xmin these leave.
        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE \"CommitteeReviews\" SET \"StartedByUserId\" = {pseudonym} WHERE \"StartedByUserId\" = {userId}",
            cancellationToken);
        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE \"CommitteeReviews\" SET \"RatifiedByUserId\" = {pseudonym} WHERE \"RatifiedByUserId\" = {userId}",
            cancellationToken);

        var reviewsAsTrainee = await _dbContext.Set<CommitteeReview>()
            .Where(r => r.TraineeUserId == userId)
            .ToListAsync(cancellationToken);
        foreach (var review in reviewsAsTrainee)
        {
            review.TraineeUserId = pseudonym;

            // T258: a review still open is withdrawn, so no panel starts, records or ratifies a review of someone who has
            // left. Only open reviews are asked, and those Withdraw accepts, so it refuses nothing here. A ratified review,
            // and one under appeal, is settled and kept as it is.
            if (CommitteeReview.OpenStates.Contains(review.State))
                review.Withdraw(CommitteeReview.WithdrawnTraineeErased, utcNow);
        }

        if (reviewsAsTrainee.Count > 0)
            retentionReasons.Add("committee_decision");

        // --- Pending entrustment decisions staged at the trainee's reviews (T258) ---
        // Only ratifying issues a staged decision, and a withdrawn review is never ratified, so what was staged at the
        // reviews just withdrawn goes. A review ratified already staged nothing that is left: ratifying cleared it.
        var traineeReviewIds = reviewsAsTrainee.Select(review => review.Id).ToArray();
        var stagedForTrainee = await _dbContext.Set<PendingEntrustmentDecision>()
            .Where(pending => traineeReviewIds.Contains(pending.ReviewId))
            .ToListAsync(cancellationToken);
        _dbContext.Set<PendingEntrustmentDecision>().RemoveRange(stagedForTrainee);

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

        // --- Committee attendance (T165): who sat when a decision was taken ---
        var attendances = await _dbContext.Set<CommitteeDecisionAttendee>()
            .Where(attendee => attendee.UserId == userId)
            .ToListAsync(cancellationToken);
        foreach (var attendee in attendances)
            attendee.UserId = pseudonym;

        // --- Committee evidence snapshot lines naming the user as the assessor (T165) ---
        var assessedLines = await _dbContext.Set<CommitteeEvidence>()
            .Where(line => line.AssessorUserId == userId)
            .ToListAsync(cancellationToken);
        foreach (var line in assessedLines)
            line.AssessorUserId = pseudonym;

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
        // The invitations are loaded because withdrawing a campaign anonymises them (MsfCampaign.Withdraw): an unloaded
        // collection is empty, and nothing would be anonymised.
        var campaigns = await _dbContext.Set<MsfCampaign>()
            .Include(c => c.Invitations)
            .Where(c => c.SubjectUserId == userId || c.CreatedByUserId == userId || c.ReviewedByUserId == userId)
            .ToListAsync(cancellationToken);
        foreach (var campaign in campaigns)
        {
            // T258: a campaign about the person that is not yet released is withdrawn, which ends it for good: no invitee
            // is added, it is not opened, closed or released, and no release writes activities about the pseudonym. That
            // includes one under review, whose report could only be released to someone who has left. A released report
            // is settled and kept. The states asked are exactly the ones Withdraw accepts (every one but Released and
            // Withdrawn, so Closed too, which nothing produces today), so it refuses nothing here.
            if (campaign.SubjectUserId == userId && campaign.State is not
                    (MsfCampaignState.Released or MsfCampaignState.Withdrawn))
            {
                campaign.Withdraw(utcNow);
            }

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

        // --- Trainee profiles: every one, each ended on the erasure day (T258) ---
        // A trainee holds one active profile and any number that have ended. Until T258 only the first moved, and it stayed
        // active. A profile that had already ended keeps its recorded end (TraineeProfile.Erase).
        var traineeProfiles = await _dbContext.Set<TraineeProfile>()
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);
        foreach (var traineeProfile in traineeProfiles)
            traineeProfile.Erase(pseudonym, erasedOn);

        // --- Assessor profiles: every one (T258) ---
        // The table holds one per user, but nothing is left under the id whatever it holds. An assessor profile has no
        // active state to end: it is a directory entry, and the assessor list drops one whose account is gone.
        var assessorProfiles = await _dbContext.Set<AssessorProfile>()
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);
        foreach (var assessorProfile in assessorProfiles)
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

        // Everything so far is saved before the account is touched, inside the transaction (T156 review). Identity's user
        // store saves the whole context on each write and reports a concurrency conflict as a failed result, not as EF's
        // exception, so a review or campaign changed elsewhere would otherwise be refused inside the account's update and
        // read as the account's. Saved here, its conflict is EF's exception, refused below as PersonChanged, and only a
        // conflict on the account itself reaches the Identity calls.
        await _dbContext.SaveChangesAsync(cancellationToken);

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
            identityUser.LockoutEnd = UserDeactivation.IndefiniteLockoutEnd;
            identityUser.TwoFactorEnabled = false;
            identityUser.FirstName = string.Empty;
            identityUser.LastName = string.Empty;
            identityUser.OptOutOfOptionalProcessing = true;
            identityUser.OptOutOfDigestEmails = true;
            identityUser.InstitutionId = null;

            // The acting role names a role the person held, and every role is removed below (T335).
            identityUser.ActingRole = null;

            EnsureSucceeded(await _userManager.UpdateAsync(identityUser), "clear the account");

            // Remove all roles
            var roles = await _userManager.GetRolesAsync(identityUser);
            if (roles.Count > 0)
                EnsureSucceeded(await _userManager.RemoveFromRolesAsync(identityUser, roles), "remove the account's roles");

            // Remove every external login (T149). A linked provider subject is an identifier of the person, and a live
            // one is a way back in: SSO sign-in finds the account by it.
            foreach (var login in await _userManager.GetLoginsAsync(identityUser))
            {
                EnsureSucceeded(
                    await _userManager.RemoveLoginAsync(identityUser, login.LoginProvider, login.ProviderKey),
                    "remove the account's institutional sign-in");
            }

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
    /// Fails the erasure on a refused Identity write, which the transaction then rolls back with everything else (T156).
    /// The user store reports a refusal as a result rather than an exception, and until T156 the erasure went on past it
    /// and committed an erased account that kept its roles or its institutional sign-in.
    /// </summary>
    /// <remarks>
    /// A concurrency conflict means the account changed elsewhere while it was being erased, and is refused as
    /// <see cref="PersonChanged" />, as a conflict on any other row about the person is. Anything else names the step and
    /// Identity's codes, never its descriptions, which can quote the person's address.
    /// </remarks>
    private static void EnsureSucceeded(IdentityResult result, string step)
    {
        if (result.Succeeded)
        {
            return;
        }

        var codes = result.Errors.Select(error => error.Code).ToArray();
        if (codes.Contains(ConcurrencyFailureCode))
        {
            throw new InvalidOperationException(PersonChanged);
        }

        throw new InvalidOperationException(
            $"The erasure could not {step} ({string.Join(", ", codes)}); nothing was erased.");
    }

    /// <summary>The code Identity's error describer gives a concurrency conflict.</summary>
    private const string ConcurrencyFailureCode = nameof(IdentityErrorDescriber.ConcurrencyFailure);

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
