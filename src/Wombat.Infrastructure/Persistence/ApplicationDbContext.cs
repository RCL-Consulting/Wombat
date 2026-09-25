using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Activities;
using Wombat.Domain.Audit;
using Wombat.Domain.DataRights;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.Invitations;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Domain.Reporting;
using Wombat.Domain.Scheduling;
using Wombat.Infrastructure.Identity;

namespace Wombat.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<WombatIdentityUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<WombatIdentityUserSpecialityScope> UserSpecialityScopes => Set<WombatIdentityUserSpecialityScope>();
    public DbSet<WombatIdentityUserSubSpecialityScope> UserSubSpecialityScopes => Set<WombatIdentityUserSubSpecialityScope>();
    public DbSet<College> Colleges => Set<College>();
    public DbSet<Institution> Institutions => Set<Institution>();
    public DbSet<InstitutionCurriculumAdoption> InstitutionCurriculumAdoptions => Set<InstitutionCurriculumAdoption>();
    public DbSet<Speciality> Specialities => Set<Speciality>();
    public DbSet<SubSpeciality> SubSpecialities => Set<SubSpeciality>();
    public DbSet<Epa> Epas => Set<Epa>();
    public DbSet<EntrustmentScale> EntrustmentScales => Set<EntrustmentScale>();
    public DbSet<EntrustmentLevel> EntrustmentLevels => Set<EntrustmentLevel>();
    public DbSet<WbaTool> WbaTools => Set<WbaTool>();
    public DbSet<Curriculum> Curricula => Set<Curriculum>();
    public DbSet<CurriculumItem> CurriculumItems => Set<CurriculumItem>();
    public DbSet<CurriculumItemProgress> CurriculumItemProgresses => Set<CurriculumItemProgress>();
    public DbSet<DecisionBody> DecisionBodies => Set<DecisionBody>();
    public DbSet<DecisionPanel> DecisionPanels => Set<DecisionPanel>();
    public DbSet<DecisionPanelMember> DecisionPanelMembers => Set<DecisionPanelMember>();
    public DbSet<CommitteeReview> CommitteeReviews => Set<CommitteeReview>();
    public DbSet<CommitteeDecision> CommitteeDecisions => Set<CommitteeDecision>();
    public DbSet<CommitteeAppeal> CommitteeAppeals => Set<CommitteeAppeal>();
    public DbSet<CommitteeEvidence> CommitteeEvidenceItems => Set<CommitteeEvidence>();
    public DbSet<CommitteeDecisionAttendee> CommitteeDecisionAttendees => Set<CommitteeDecisionAttendee>();
    public DbSet<CommitteeAgendaLine> CommitteeAgendaLines => Set<CommitteeAgendaLine>();
    public DbSet<EntrustmentDecision> EntrustmentDecisions => Set<EntrustmentDecision>();
    public DbSet<EntrustmentEvidenceLink> EntrustmentEvidenceLinks => Set<EntrustmentEvidenceLink>();
    public DbSet<PendingEntrustmentDecision> PendingEntrustmentDecisions => Set<PendingEntrustmentDecision>();
    public DbSet<ActivityType> ActivityTypes => Set<ActivityType>();
    public DbSet<ActivityTypeVersion> ActivityTypeVersions => Set<ActivityTypeVersion>();
    public DbSet<ProcedureCatalogueEntry> ProcedureCatalogueEntries => Set<ProcedureCatalogueEntry>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<ActivityTransition> ActivityTransitions => Set<ActivityTransition>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<MsfTemplate> MsfTemplates => Set<MsfTemplate>();
    public DbSet<MsfQuestion> MsfQuestions => Set<MsfQuestion>();
    public DbSet<MsfCampaign> MsfCampaigns => Set<MsfCampaign>();
    public DbSet<MsfCampaignEpa> MsfCampaignEpas => Set<MsfCampaignEpa>();
    public DbSet<MsfInvitation> MsfInvitations => Set<MsfInvitation>();
    public DbSet<MsfResponse> MsfResponses => Set<MsfResponse>();
    public DbSet<MsfResponseAnswer> MsfResponseAnswers => Set<MsfResponseAnswer>();
    public DbSet<TraineeProfile> TraineeProfiles => Set<TraineeProfile>();
    public DbSet<AssessorProfile> AssessorProfiles => Set<AssessorProfile>();
    public DbSet<InstitutionBrand> InstitutionBrands => Set<InstitutionBrand>();
    public DbSet<PortfolioExport> PortfolioExports => Set<PortfolioExport>();
    public DbSet<ScheduledJobDefinition> ScheduledJobDefinitions => Set<ScheduledJobDefinition>();
    public DbSet<ScheduledJobRun> ScheduledJobRuns => Set<ScheduledJobRun>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<AuditEntryArchive> AuditEntryArchives => Set<AuditEntryArchive>();
    public DbSet<DataRightsRequest> DataRightsRequests => Set<DataRightsRequest>();
    public DbSet<DataRightsRectification> DataRightsRectifications => Set<DataRightsRectification>();
    public DbSet<DataRightsErasureRecord> DataRightsErasureRecords => Set<DataRightsErasureRecord>();
    public DbSet<SsoGroupRoleMapping> SsoGroupRoleMappings => Set<SsoGroupRoleMapping>();
    public DbSet<UserRoleAssignment> UserRoleAssignments => Set<UserRoleAssignment>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        WriteAnonymisedInvitationsDeliveryOutcome();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        WriteAnonymisedInvitationsDeliveryOutcome();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Makes every save of an anonymised MSF invitation write what became of its link's mail, whatever the invitation was
    /// read holding: none, since anonymising clears it (<see cref="MsfInvitation.Anonymize" />). (T251 review)
    /// </summary>
    /// <remarks>
    /// <para>
    /// The mail worker's report (<c>MsfLinkDeliveryRecorder</c>) is one <c>UPDATE</c> on a scope of its own, and it can
    /// land between a close's read of the invitations and that close's save. EF writes only the columns it saw change, and
    /// a column read as null and cleared to null has not changed, so the report's <see cref="MsfInvitation.SentOn" /> or
    /// <see cref="MsfInvitation.DeliveryFailedOn" />, and its <see cref="MsfInvitation.DeliveryLinkSelector" />, would
    /// survive on the erased row. That time is not "nothing about the respondent": the worker logs, at the same instant,
    /// the address it sent to and the campaign, and the row's responses name the row (<c>MsfResponse.InvitationId</c>).
    /// So a timestamp one mail apart from the next would tie an erased address to its answers, the linkage T207 took out.
    /// </para>
    /// <para>
    /// Written here, not in each caller, because three writers anonymise (the close command, the withdraw command and the
    /// auto-close job) through <see cref="MsfCampaign.Close" /> and <see cref="MsfCampaign.Withdraw" />, and a fourth
    /// would not know to. With the three columns in its <c>UPDATE</c>, the anonymising save meets the report on the row's
    /// lock: a report that committed first is overwritten, and one that waited finds the address erased and, by its own
    /// condition, writes nothing.
    /// </para>
    /// </remarks>
    private void WriteAnonymisedInvitationsDeliveryOutcome()
    {
        foreach (var entry in ChangeTracker.Entries<MsfInvitation>())
        {
            if (entry.State != EntityState.Modified || !entry.Entity.HoldsNoAddress)
            {
                continue;
            }

            entry.Property(invitation => invitation.SentOn).IsModified = true;
            entry.Property(invitation => invitation.DeliveryFailedOn).IsModified = true;
            entry.Property(invitation => invitation.DeliveryLinkSelector).IsModified = true;
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        builder.Entity<WombatIdentityUser>(entity =>
        {
            entity.Property(user => user.FirstName).HasMaxLength(100);
            entity.Property(user => user.LastName).HasMaxLength(100);

            entity.HasMany(user => user.SpecialityScopes)
                .WithOne(scope => scope.User)
                .HasForeignKey(scope => scope.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(user => user.SubSpecialityScopes)
                .WithOne(scope => scope.User)
                .HasForeignKey(scope => scope.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WombatIdentityUserSpecialityScope>(entity =>
        {
            entity.ToTable("UserSpecialityScopes");
            entity.HasIndex(scope => new { scope.UserId, scope.SpecialityId }).IsUnique();
        });

        builder.Entity<WombatIdentityUserSubSpecialityScope>(entity =>
        {
            entity.ToTable("UserSubSpecialityScopes");
            entity.HasIndex(scope => new { scope.UserId, scope.SubSpecialityId }).IsUnique();
        });
    }
}
