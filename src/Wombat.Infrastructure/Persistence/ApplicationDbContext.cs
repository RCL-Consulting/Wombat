using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Forms;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.Invitations;
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
    public DbSet<Institution> Institutions => Set<Institution>();
    public DbSet<Speciality> Specialities => Set<Speciality>();
    public DbSet<SubSpeciality> SubSpecialities => Set<SubSpeciality>();
    public DbSet<Epa> Epas => Set<Epa>();
    public DbSet<EntrustmentScale> EntrustmentScales => Set<EntrustmentScale>();
    public DbSet<EntrustmentLevel> EntrustmentLevels => Set<EntrustmentLevel>();
    public DbSet<Curriculum> Curricula => Set<Curriculum>();
    public DbSet<CurriculumItem> CurriculumItems => Set<CurriculumItem>();
    public DbSet<CurriculumItemProgress> CurriculumItemProgresses => Set<CurriculumItemProgress>();
    public DbSet<AssessmentForm> AssessmentForms => Set<AssessmentForm>();
    public DbSet<FormCriterion> FormCriteria => Set<FormCriterion>();
    public DbSet<FormEpaLink> FormEpaLinks => Set<FormEpaLink>();
    public DbSet<ActivityType> ActivityTypes => Set<ActivityType>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<ActivityTransition> ActivityTransitions => Set<ActivityTransition>();
    public DbSet<ActivityPermissionRule> ActivityPermissionRules => Set<ActivityPermissionRule>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<TraineeProfile> TraineeProfiles => Set<TraineeProfile>();
    public DbSet<AssessorProfile> AssessorProfiles => Set<AssessorProfile>();

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
