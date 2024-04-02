using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Wombat.Configurations.Entities;

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
        }

        public DbSet<Category> AssessmentCategories { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
    }
}
