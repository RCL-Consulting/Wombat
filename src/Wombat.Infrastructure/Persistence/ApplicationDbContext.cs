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
using Wombat.Domain.Forms;
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
    public DbSet<Speciality> Specialities => Set<Speciality>();
    public DbSet<SubSpeciality> SubSpecialities => Set<SubSpeciality>();
    public DbSet<Epa> Epas => Set<Epa>();
    public DbSet<EntrustmentScale> EntrustmentScales => Set<EntrustmentScale>();
    public DbSet<EntrustmentLevel> EntrustmentLevels => Set<EntrustmentLevel>();
    public DbSet<Curriculum> Curricula => Set<Curriculum>();
    public DbSet<CurriculumItem> CurriculumItems => Set<CurriculumItem>();
    public DbSet<CurriculumItemProgress> CurriculumItemProgresses => Set<CurriculumItemProgress>();
    public DbSet<DecisionPanel> DecisionPanels => Set<DecisionPanel>();
    public DbSet<DecisionPanelMember> DecisionPanelMembers => Set<DecisionPanelMember>();
    public DbSet<CommitteeReview> CommitteeReviews => Set<CommitteeReview>();
    public DbSet<CommitteeDecision> CommitteeDecisions => Set<CommitteeDecision>();
    public DbSet<CommitteeAppeal> CommitteeAppeals => Set<CommitteeAppeal>();
    public DbSet<CommitteeEvidence> CommitteeEvidenceItems => Set<CommitteeEvidence>();
    public DbSet<EntrustmentDecision> EntrustmentDecisions => Set<EntrustmentDecision>();
    public DbSet<EntrustmentEvidenceLink> EntrustmentEvidenceLinks => Set<EntrustmentEvidenceLink>();
    public DbSet<PendingEntrustmentDecision> PendingEntrustmentDecisions => Set<PendingEntrustmentDecision>();
    public DbSet<AssessmentForm> AssessmentForms => Set<AssessmentForm>();
    public DbSet<FormCriterion> FormCriteria => Set<FormCriterion>();
    public DbSet<FormEpaLink> FormEpaLinks => Set<FormEpaLink>();
    public DbSet<ActivityType> ActivityTypes => Set<ActivityType>();
    public DbSet<ActivityTypeVersion> ActivityTypeVersions => Set<ActivityTypeVersion>();
    public DbSet<ProcedureCatalogueEntry> ProcedureCatalogueEntries => Set<ProcedureCatalogueEntry>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<ActivityTransition> ActivityTransitions => Set<ActivityTransition>();
    public DbSet<ActivityPermissionRule> ActivityPermissionRules => Set<ActivityPermissionRule>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<MsfTemplate> MsfTemplates => Set<MsfTemplate>();
    public DbSet<MsfQuestion> MsfQuestions => Set<MsfQuestion>();
    public DbSet<MsfCampaign> MsfCampaigns => Set<MsfCampaign>();
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
