using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Wombat.Data
{
    public class ApplicationDbContext : IdentityDbContext<WombatUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AssessmentCategory> AssessmentCategories { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
    }
}
