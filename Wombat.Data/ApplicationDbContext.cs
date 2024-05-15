using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Wombat.Common.Models;
using Wombat.Data.Configurations.Entities;

namespace Wombat.Data
{
    public class ApplicationDbContext : IdentityDbContext<WombatUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new RoleSeedConfiguration());
            builder.ApplyConfiguration(new UserSeedConfiguration());
            builder.ApplyConfiguration(new UserRoleSeedConfiguration());
            builder.ApplyConfiguration(new OptionConfiguration());
            builder.ApplyConfiguration(new OptionSetConfiguration());
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in base.ChangeTracker.Entries<BaseEntity>().Where(q => q.State==EntityState.Added ||
                                                                                 q.State==EntityState.Modified))
            {
                entry.Entity.DateModified = DateTime.Now;
                if (entry.State == EntityState.Added)
                    entry.Entity.DateCreated = DateTime.Now;
            }
            return base.SaveChangesAsync(cancellationToken);
        }

        public DbSet<AssessmentTemplate> AssessmentTemplates { get; set; }
        public DbSet<AssessmentContext> AssessmentContexts { get; set; }

        public DbSet<OptionCriterion> OptionCriteria{ get; set; }

        public DbSet<OptionSet> OptionSets{ get; set; }
        public DbSet<Option> Options { get; set; }

        public DbSet<LoggedAssessment> LoggedAssessments { get; set; }
        public DbSet<OptionCriterionResponse> OptionCriterionResponses { get; set; }
    }
}
