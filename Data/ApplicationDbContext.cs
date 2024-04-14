using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Wombat.Configurations.Entities;
using Wombat.Models;

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

        public DbSet<AssessmentCategory> AssessmentCategories { get; set; }
        public DbSet<Assessment> Assessments { get; set; }

        public DbSet<OptionCriterion> OptionCriteria{ get; set; }

        public DbSet<TextCriterion> TextCriteria { get; set; }

        public DbSet<OptionSet> OptionSets{ get; set; }
        public DbSet<Option> Options { get; set; }
    }
}
